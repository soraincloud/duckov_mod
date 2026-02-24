using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

public abstract class BaseTreeViewController : CollectionViewController
{
	private Dictionary<int, TreeItem> m_TreeItems = new Dictionary<int, TreeItem>();

	private List<int> m_RootIndices = new List<int>();

	private List<TreeViewItemWrapper> m_ItemWrappers = new List<TreeViewItemWrapper>();

	private HashSet<int> m_TreeItemIdsWithItemWrappers = new HashSet<int>();

	private List<TreeViewItemWrapper> m_WrapperInsertionList = new List<TreeViewItemWrapper>();

	private static readonly ProfilerMarker K_ExpandItemByIndex = new ProfilerMarker(ProfilerCategory.Scripts, "BaseTreeViewController.ExpandItemByIndex");

	private static readonly ProfilerMarker k_CreateWrappers = new ProfilerMarker("BaseTreeViewController.CreateWrappers");

	protected BaseTreeView baseTreeView => base.view as BaseTreeView;

	public override IList itemsSource
	{
		get
		{
			return base.itemsSource;
		}
		set
		{
			throw new InvalidOperationException("Can't set itemsSource directly. Override this controller to manage tree data.");
		}
	}

	public void RebuildTree()
	{
		m_TreeItems.Clear();
		m_RootIndices.Clear();
		foreach (int allItemId in GetAllItemIds())
		{
			int parentId = GetParentId(allItemId);
			if (parentId == -1)
			{
				m_RootIndices.Add(allItemId);
			}
			m_TreeItems.Add(allItemId, new TreeItem(allItemId, parentId, GetChildrenIds(allItemId)));
		}
		RegenerateWrappers();
	}

	public IEnumerable<int> GetRootItemIds()
	{
		return m_RootIndices;
	}

	public abstract IEnumerable<int> GetAllItemIds(IEnumerable<int> rootIds = null);

	public abstract int GetParentId(int id);

	public abstract IEnumerable<int> GetChildrenIds(int id);

	public abstract void Move(int id, int newParentId, int childIndex = -1, bool rebuildTree = true);

	public abstract bool TryRemoveItem(int id, bool rebuildTree = true);

	internal override void InvokeMakeItem(ReusableCollectionItem reusableItem)
	{
		if (reusableItem is ReusableTreeViewItem reusableTreeViewItem)
		{
			reusableTreeViewItem.Init(MakeItem());
			PostInitRegistration(reusableTreeViewItem);
		}
	}

	internal override void InvokeBindItem(ReusableCollectionItem reusableItem, int index)
	{
		if (reusableItem is ReusableTreeViewItem reusableTreeViewItem)
		{
			reusableTreeViewItem.Indent(GetIndentationDepthByIndex(index));
			reusableTreeViewItem.SetExpandedWithoutNotify(IsExpandedByIndex(index));
			reusableTreeViewItem.SetToggleVisibility(HasChildrenByIndex(index));
		}
		base.InvokeBindItem(reusableItem, index);
	}

	internal override void InvokeDestroyItem(ReusableCollectionItem reusableItem)
	{
		if (reusableItem is ReusableTreeViewItem reusableTreeViewItem)
		{
			reusableTreeViewItem.onPointerUp -= OnItemPointerUp;
			reusableTreeViewItem.onToggleValueChanged -= OnToggleValueChanged;
		}
		base.InvokeDestroyItem(reusableItem);
	}

	internal void PostInitRegistration(ReusableTreeViewItem treeItem)
	{
		treeItem.onPointerUp += OnItemPointerUp;
		treeItem.onToggleValueChanged += OnToggleValueChanged;
		if (baseTreeView.autoExpand)
		{
			baseTreeView.expandedItemIds.Remove(treeItem.id);
			baseTreeView.schedule.Execute((Action)delegate
			{
				ExpandItem(treeItem.id, expandAllChildren: true);
			});
		}
	}

	private void OnItemPointerUp(PointerUpEvent evt)
	{
		if ((evt.modifiers & EventModifiers.Alt) == 0)
		{
			return;
		}
		VisualElement e = evt.currentTarget as VisualElement;
		Toggle toggle = e.Q<Toggle>(BaseTreeView.itemToggleUssClassName);
		int index = ((ReusableTreeViewItem)toggle.userData).index;
		int idForIndex = GetIdForIndex(index);
		bool flag = IsExpandedByIndex(index);
		if (!HasChildrenByIndex(index))
		{
			return;
		}
		HashSet<int> hashSet = new HashSet<int>(baseTreeView.expandedItemIds);
		if (flag)
		{
			hashSet.Remove(idForIndex);
		}
		else
		{
			hashSet.Add(idForIndex);
		}
		IEnumerable<int> childrenIdsByIndex = GetChildrenIdsByIndex(index);
		foreach (int allItemId in GetAllItemIds(childrenIdsByIndex))
		{
			if (HasChildren(allItemId))
			{
				if (flag)
				{
					hashSet.Remove(allItemId);
				}
				else
				{
					hashSet.Add(allItemId);
				}
			}
		}
		baseTreeView.expandedItemIds = hashSet.ToList();
		RegenerateWrappers();
		baseTreeView.RefreshItems();
		evt.StopPropagation();
	}

	private void OnToggleValueChanged(ChangeEvent<bool> evt)
	{
		Toggle toggle = evt.target as Toggle;
		int index = ((ReusableTreeViewItem)toggle.userData).index;
		if (IsExpandedByIndex(index))
		{
			CollapseItemByIndex(index, collapseAllChildren: false);
		}
		else
		{
			ExpandItemByIndex(index, expandAllChildren: false);
		}
		baseTreeView.scrollView.contentContainer.Focus();
	}

	public virtual int GetTreeItemsCount()
	{
		return m_TreeItems.Count;
	}

	public override int GetIndexForId(int id)
	{
		if (m_TreeItemIdsWithItemWrappers.Contains(id))
		{
			for (int i = 0; i < m_ItemWrappers.Count; i++)
			{
				if (m_ItemWrappers[i].id == id)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public override int GetIdForIndex(int index)
	{
		return IsIndexValid(index) ? m_ItemWrappers[index].id : (-1);
	}

	public virtual bool HasChildren(int id)
	{
		if (m_TreeItems.TryGetValue(id, out var value))
		{
			return value.hasChildren;
		}
		return false;
	}

	internal bool Exists(int id)
	{
		return m_TreeItems.ContainsKey(id);
	}

	public bool HasChildrenByIndex(int index)
	{
		return IsIndexValid(index) && m_ItemWrappers[index].hasChildren;
	}

	public IEnumerable<int> GetChildrenIdsByIndex(int index)
	{
		return IsIndexValid(index) ? m_ItemWrappers[index].childrenIds : null;
	}

	public int GetChildIndexForId(int id)
	{
		if (!m_TreeItems.TryGetValue(id, out var value))
		{
			return -1;
		}
		int num = 0;
		IEnumerable<int> enumerable;
		if (!m_TreeItems.TryGetValue(value.parentId, out var value2))
		{
			IEnumerable<int> rootIndices = m_RootIndices;
			enumerable = rootIndices;
		}
		else
		{
			enumerable = value2.childrenIds;
		}
		IEnumerable<int> enumerable2 = enumerable;
		foreach (int item in enumerable2)
		{
			if (item == id)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	internal int GetIndentationDepth(int id)
	{
		int num = 0;
		int parentId = GetParentId(id);
		while (parentId != -1)
		{
			parentId = GetParentId(parentId);
			num++;
		}
		return num;
	}

	internal int GetIndentationDepthByIndex(int index)
	{
		int idForIndex = GetIdForIndex(index);
		return GetIndentationDepth(idForIndex);
	}

	internal virtual bool CanChangeExpandedState(int id)
	{
		return true;
	}

	public bool IsExpanded(int id)
	{
		return baseTreeView.expandedItemIds.Contains(id);
	}

	public bool IsExpandedByIndex(int index)
	{
		if (!IsIndexValid(index))
		{
			return false;
		}
		return IsExpanded(m_ItemWrappers[index].id);
	}

	public void ExpandItemByIndex(int index, bool expandAllChildren, bool refresh = true)
	{
		using (K_ExpandItemByIndex.Auto())
		{
			if (!HasChildrenByIndex(index))
			{
				return;
			}
			int idForIndex = GetIdForIndex(index);
			if (!CanChangeExpandedState(idForIndex))
			{
				return;
			}
			if (!baseTreeView.expandedItemIds.Contains(idForIndex) || expandAllChildren)
			{
				IEnumerable<int> childrenIdsByIndex = GetChildrenIdsByIndex(index);
				List<int> list = new List<int>();
				foreach (int item in childrenIdsByIndex)
				{
					if (!m_TreeItemIdsWithItemWrappers.Contains(item))
					{
						list.Add(item);
					}
				}
				CreateWrappers(list, GetIndentationDepth(idForIndex) + 1, ref m_WrapperInsertionList);
				m_ItemWrappers.InsertRange(index + 1, m_WrapperInsertionList);
				if (!baseTreeView.expandedItemIds.Contains(m_ItemWrappers[index].id))
				{
					baseTreeView.expandedItemIds.Add(m_ItemWrappers[index].id);
				}
				m_WrapperInsertionList.Clear();
			}
			if (expandAllChildren)
			{
				IEnumerable<int> childrenIds = GetChildrenIds(idForIndex);
				foreach (int allItemId in GetAllItemIds(childrenIds))
				{
					if (!baseTreeView.expandedItemIds.Contains(allItemId))
					{
						ExpandItemByIndex(GetIndexForId(allItemId), expandAllChildren: true, refresh: false);
					}
				}
			}
			if (refresh)
			{
				baseTreeView.RefreshItems();
			}
		}
	}

	public void ExpandItem(int id, bool expandAllChildren, bool refresh = true)
	{
		if (!HasChildren(id) || !CanChangeExpandedState(id))
		{
			return;
		}
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].id == id && (expandAllChildren || !IsExpandedByIndex(i)))
			{
				ExpandItemByIndex(i, expandAllChildren, refresh);
				return;
			}
		}
		if (!baseTreeView.expandedItemIds.Contains(id))
		{
			baseTreeView.expandedItemIds.Add(id);
		}
	}

	public void CollapseItemByIndex(int index, bool collapseAllChildren)
	{
		if (!HasChildrenByIndex(index))
		{
			return;
		}
		int idForIndex = GetIdForIndex(index);
		if (!CanChangeExpandedState(idForIndex))
		{
			return;
		}
		if (collapseAllChildren)
		{
			IEnumerable<int> childrenIds = GetChildrenIds(idForIndex);
			foreach (int allItemId in GetAllItemIds(childrenIds))
			{
				baseTreeView.expandedItemIds.Remove(allItemId);
			}
		}
		baseTreeView.expandedItemIds.Remove(idForIndex);
		int num = 0;
		int i = index + 1;
		for (int indentationDepthByIndex = GetIndentationDepthByIndex(index); i < m_ItemWrappers.Count && GetIndentationDepthByIndex(i) > indentationDepthByIndex; i++)
		{
			num++;
		}
		int num2 = index + 1 + num;
		for (int j = index + 1; j < num2; j++)
		{
			m_TreeItemIdsWithItemWrappers.Remove(m_ItemWrappers[j].id);
		}
		m_ItemWrappers.RemoveRange(index + 1, num);
		baseTreeView.RefreshItems();
	}

	public void CollapseItem(int id, bool collapseAllChildren)
	{
		if (!CanChangeExpandedState(id))
		{
			return;
		}
		for (int i = 0; i < m_ItemWrappers.Count; i++)
		{
			if (m_ItemWrappers[i].id == id)
			{
				if (IsExpandedByIndex(i))
				{
					CollapseItemByIndex(i, collapseAllChildren);
					return;
				}
				break;
			}
		}
		if (baseTreeView.expandedItemIds.Contains(id))
		{
			baseTreeView.expandedItemIds.Remove(id);
		}
	}

	public void ExpandAll()
	{
		foreach (int allItemId in GetAllItemIds())
		{
			if (CanChangeExpandedState(allItemId) && !baseTreeView.expandedItemIds.Contains(allItemId))
			{
				baseTreeView.expandedItemIds.Add(allItemId);
			}
		}
		RegenerateWrappers();
		baseTreeView.RefreshItems();
	}

	public void CollapseAll()
	{
		if (baseTreeView.expandedItemIds.Count == 0)
		{
			return;
		}
		List<int> value;
		using (CollectionPool<List<int>, int>.Get(out value))
		{
			foreach (int expandedItemId in baseTreeView.expandedItemIds)
			{
				if (!CanChangeExpandedState(expandedItemId))
				{
					value.Add(expandedItemId);
				}
			}
			baseTreeView.expandedItemIds.Clear();
			baseTreeView.expandedItemIds.AddRange(value);
		}
		RegenerateWrappers();
		baseTreeView.RefreshItems();
	}

	internal void RegenerateWrappers()
	{
		m_ItemWrappers.Clear();
		m_TreeItemIdsWithItemWrappers.Clear();
		IEnumerable<int> rootItemIds = GetRootItemIds();
		if (rootItemIds != null)
		{
			CreateWrappers(rootItemIds, 0, ref m_ItemWrappers);
			SetItemsSourceWithoutNotify(m_ItemWrappers);
		}
	}

	private void CreateWrappers(IEnumerable<int> treeViewItemIds, int depth, ref List<TreeViewItemWrapper> wrappers)
	{
		using (k_CreateWrappers.Auto())
		{
			if (treeViewItemIds == null || wrappers == null || m_TreeItemIdsWithItemWrappers == null)
			{
				return;
			}
			foreach (int treeViewItemId in treeViewItemIds)
			{
				if (m_TreeItems.TryGetValue(treeViewItemId, out var value))
				{
					TreeViewItemWrapper item = new TreeViewItemWrapper(value, depth);
					wrappers.Add(item);
					m_TreeItemIdsWithItemWrappers.Add(treeViewItemId);
					if (baseTreeView?.expandedItemIds != null && baseTreeView.expandedItemIds.Contains(item.id) && item.hasChildren)
					{
						CreateWrappers(GetChildrenIds(item.id), depth + 1, ref wrappers);
					}
				}
			}
		}
	}

	private bool IsIndexValid(int index)
	{
		return index >= 0 && index < m_ItemWrappers.Count;
	}

	internal void RaiseItemParentChanged(int id, int newParentId)
	{
		RaiseItemIndexChanged(id, newParentId);
	}
}
