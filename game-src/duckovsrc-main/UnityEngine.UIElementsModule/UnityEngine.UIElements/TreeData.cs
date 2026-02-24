using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal readonly struct TreeData<T>
{
	private readonly IList<int> m_RootItemIds;

	private readonly Dictionary<int, TreeViewItemData<T>> m_Tree;

	private readonly Dictionary<int, int> m_ParentIds;

	private readonly Dictionary<int, List<int>> m_ChildrenIds;

	public IEnumerable<int> rootItemIds => m_RootItemIds;

	public TreeData(IList<TreeViewItemData<T>> rootItems)
	{
		m_RootItemIds = new List<int>();
		m_Tree = new Dictionary<int, TreeViewItemData<T>>();
		m_ParentIds = new Dictionary<int, int>();
		m_ChildrenIds = new Dictionary<int, List<int>>();
		RefreshTree(rootItems);
	}

	public TreeViewItemData<T> GetDataForId(int id)
	{
		if (m_Tree.TryGetValue(id, out var value))
		{
			return value;
		}
		return default(TreeViewItemData<T>);
	}

	public int GetParentId(int id)
	{
		if (m_ParentIds.TryGetValue(id, out var value))
		{
			return value;
		}
		return -1;
	}

	public void AddItem(TreeViewItemData<T> item, int parentId, int childIndex)
	{
		List<TreeViewItemData<T>> list = CollectionPool<List<TreeViewItemData<T>>, TreeViewItemData<T>>.Get();
		list.Add(item);
		BuildTree(list, isRoot: false);
		AddItemToParent(item, parentId, childIndex);
		CollectionPool<List<TreeViewItemData<T>>, TreeViewItemData<T>>.Release(list);
	}

	public bool TryRemove(int id)
	{
		if (m_ParentIds.TryGetValue(id, out var value))
		{
			RemoveFromParent(id, value);
		}
		else
		{
			m_RootItemIds.Remove(id);
		}
		return TryRemoveChildrenIds(id);
	}

	public void Move(int id, int newParentId, int childIndex)
	{
		if (!m_Tree.TryGetValue(id, out var value))
		{
			return;
		}
		if (m_ParentIds.TryGetValue(id, out var value2))
		{
			if (value2 == newParentId)
			{
				int childIndex2 = m_Tree[value2].GetChildIndex(id);
				if (childIndex2 < childIndex)
				{
					childIndex--;
				}
			}
			RemoveFromParent(value.id, value2);
		}
		else
		{
			int num = m_RootItemIds.IndexOf(id);
			if (newParentId == -1 && num < childIndex)
			{
				childIndex--;
			}
			m_RootItemIds.Remove(id);
		}
		AddItemToParent(value, newParentId, childIndex);
	}

	public bool HasAncestor(int childId, int ancestorId)
	{
		if (childId == -1 || ancestorId == -1)
		{
			return false;
		}
		int id = childId;
		int parentId;
		while ((parentId = GetParentId(id)) != -1)
		{
			if (ancestorId == parentId)
			{
				return true;
			}
			id = parentId;
		}
		return false;
	}

	private void AddItemToParent(TreeViewItemData<T> item, int parentId, int childIndex)
	{
		if (parentId == -1)
		{
			m_ParentIds.Remove(item.id);
			if (childIndex < 0 || childIndex >= m_RootItemIds.Count)
			{
				m_RootItemIds.Add(item.id);
			}
			else
			{
				m_RootItemIds.Insert(childIndex, item.id);
			}
		}
		else
		{
			TreeViewItemData<T> treeViewItemData = m_Tree[parentId];
			treeViewItemData.InsertChild(item, childIndex);
			m_Tree[parentId] = treeViewItemData;
			m_ParentIds[item.id] = parentId;
			UpdateParentTree(treeViewItemData);
		}
	}

	private void RemoveFromParent(int id, int parentId)
	{
		TreeViewItemData<T> treeViewItemData = m_Tree[parentId];
		treeViewItemData.RemoveChild(id);
		m_Tree[parentId] = treeViewItemData;
		if (m_ChildrenIds.TryGetValue(parentId, out var value))
		{
			value.Remove(id);
		}
		UpdateParentTree(treeViewItemData);
	}

	private void UpdateParentTree(TreeViewItemData<T> current)
	{
		int value;
		while (m_ParentIds.TryGetValue(current.id, out value))
		{
			TreeViewItemData<T> treeViewItemData = m_Tree[value];
			treeViewItemData.ReplaceChild(current);
			m_Tree[value] = treeViewItemData;
			current = treeViewItemData;
		}
	}

	private bool TryRemoveChildrenIds(int id)
	{
		if (m_Tree.TryGetValue(id, out var value) && value.children != null)
		{
			foreach (TreeViewItemData<T> child in value.children)
			{
				TryRemoveChildrenIds(child.id);
			}
		}
		if (m_ChildrenIds.TryGetValue(id, out var value2))
		{
			CollectionPool<List<int>, int>.Release(value2);
		}
		bool flag = false;
		flag |= m_RootItemIds.Remove(id);
		flag |= m_ChildrenIds.Remove(id);
		flag |= m_ParentIds.Remove(id);
		flag |= m_Tree.Remove(id);
		return flag | m_RootItemIds.Remove(id);
	}

	private void RefreshTree(IList<TreeViewItemData<T>> rootItems)
	{
		m_Tree.Clear();
		m_ParentIds.Clear();
		m_ChildrenIds.Clear();
		m_RootItemIds.Clear();
		BuildTree(rootItems, isRoot: true);
	}

	private void BuildTree(IEnumerable<TreeViewItemData<T>> items, bool isRoot)
	{
		if (items == null)
		{
			return;
		}
		foreach (TreeViewItemData<T> item in items)
		{
			m_Tree.Add(item.id, item);
			if (isRoot)
			{
				m_RootItemIds.Add(item.id);
			}
			if (item.children == null)
			{
				continue;
			}
			if (!m_ChildrenIds.TryGetValue(item.id, out var value))
			{
				m_ChildrenIds.Add(item.id, value = CollectionPool<List<int>, int>.Get());
			}
			foreach (TreeViewItemData<T> child in item.children)
			{
				m_ParentIds.Add(child.id, item.id);
				value.Add(child.id);
			}
			BuildTree(item.children, isRoot: false);
		}
	}
}
