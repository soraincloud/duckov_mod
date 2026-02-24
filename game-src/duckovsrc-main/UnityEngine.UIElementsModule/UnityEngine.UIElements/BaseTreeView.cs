using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

public abstract class BaseTreeView : BaseVerticalCollectionView
{
	public new class UxmlTraits : BaseVerticalCollectionView.UxmlTraits
	{
		private readonly UxmlBoolAttributeDescription m_AutoExpand = new UxmlBoolAttributeDescription
		{
			name = "auto-expand",
			defaultValue = false
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			BaseTreeView baseTreeView = (BaseTreeView)ve;
			baseTreeView.autoExpand = m_AutoExpand.GetValueFromBag(bag, cc);
		}
	}

	public new static readonly string ussClassName = "unity-tree-view";

	public new static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string itemToggleUssClassName = ussClassName + "__item-toggle";

	public static readonly string itemIndentsContainerUssClassName = ussClassName + "__item-indents";

	public static readonly string itemIndentUssClassName = ussClassName + "__item-indent";

	public static readonly string itemContentContainerUssClassName = ussClassName + "__item-content";

	private bool m_AutoExpand;

	[SerializeField]
	private List<int> m_ExpandedItemIds;

	public new IList itemsSource
	{
		get
		{
			return viewController.itemsSource;
		}
		internal set
		{
			GetOrCreateViewController().itemsSource = value;
		}
	}

	public new BaseTreeViewController viewController => base.viewController as BaseTreeViewController;

	public bool autoExpand
	{
		get
		{
			return m_AutoExpand;
		}
		set
		{
			m_AutoExpand = value;
			viewController?.RegenerateWrappers();
			RefreshItems();
		}
	}

	internal List<int> expandedItemIds
	{
		get
		{
			return m_ExpandedItemIds;
		}
		set
		{
			m_ExpandedItemIds = value;
		}
	}

	public void SetRootItems<T>(IList<TreeViewItemData<T>> rootItems)
	{
		SetRootItemsInternal(rootItems);
	}

	internal abstract void SetRootItemsInternal<T>(IList<TreeViewItemData<T>> rootItems);

	public IEnumerable<int> GetRootIds()
	{
		return viewController.GetRootItemIds();
	}

	public int GetTreeCount()
	{
		return viewController.GetTreeItemsCount();
	}

	private protected override void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableTreeViewItem>();
	}

	public override void SetViewController(CollectionViewController controller)
	{
		if (viewController != null)
		{
			viewController.itemIndexChanged -= OnItemIndexChanged;
		}
		base.SetViewController(controller);
		if (viewController != null)
		{
			viewController.itemIndexChanged += OnItemIndexChanged;
		}
	}

	private void OnItemIndexChanged(int srcIndex, int dstIndex)
	{
		RefreshItems();
	}

	internal override ICollectionDragAndDropController CreateDragAndDropController()
	{
		return new TreeViewReorderableDragAndDropController(this);
	}

	public BaseTreeView()
		: this(-1)
	{
	}

	public BaseTreeView(int itemHeight)
		: base(null, itemHeight)
	{
		m_ExpandedItemIds = new List<int>();
		AddToClassList(ussClassName);
		RegisterCallback<PointerUpEvent>(OnTreeViewPointerUp, TrickleDown.TrickleDown);
	}

	public int GetIdForIndex(int index)
	{
		return viewController.GetIdForIndex(index);
	}

	public int GetParentIdForIndex(int index)
	{
		return viewController.GetParentId(GetIdForIndex(index));
	}

	public IEnumerable<int> GetChildrenIdsForIndex(int index)
	{
		return viewController.GetChildrenIdsByIndex(index);
	}

	public IEnumerable<TreeViewItemData<T>> GetSelectedItems<T>()
	{
		return GetSelectedItemsInternal<T>();
	}

	private protected abstract IEnumerable<TreeViewItemData<T>> GetSelectedItemsInternal<T>();

	public T GetItemDataForIndex<T>(int index)
	{
		return GetItemDataForIndexInternal<T>(index);
	}

	private protected abstract T GetItemDataForIndexInternal<T>(int index);

	public T GetItemDataForId<T>(int id)
	{
		return GetItemDataForIdInternal<T>(id);
	}

	private protected abstract T GetItemDataForIdInternal<T>(int id);

	public void AddItem<T>(TreeViewItemData<T> item, int parentId = -1, int childIndex = -1, bool rebuildTree = true)
	{
		AddItemInternal(item, parentId, childIndex, rebuildTree);
	}

	private protected abstract void AddItemInternal<T>(TreeViewItemData<T> item, int parentId, int childIndex, bool rebuildTree);

	public bool TryRemoveItem(int id)
	{
		if (viewController.TryRemoveItem(id))
		{
			RefreshItems();
			return true;
		}
		return false;
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		if (viewController != null)
		{
			viewController.RebuildTree();
			RefreshItems();
		}
	}

	private protected override bool HandleItemNavigation(bool moveIn, bool altPressed)
	{
		int num = 1;
		bool flag = false;
		foreach (int selectedId in base.selectedIds)
		{
			int indexForId = viewController.GetIndexForId(selectedId);
			if (!viewController.HasChildrenByIndex(indexForId))
			{
				break;
			}
			if (moveIn && !IsExpandedByIndex(indexForId))
			{
				ExpandItemByIndex(indexForId, altPressed);
				flag = true;
			}
			else if (!moveIn && IsExpandedByIndex(indexForId))
			{
				CollapseItemByIndex(indexForId, altPressed);
				flag = true;
			}
		}
		if (flag)
		{
			return true;
		}
		if (!moveIn)
		{
			int idForIndex = viewController.GetIdForIndex(base.selectedIndex);
			int parentId = viewController.GetParentId(idForIndex);
			if (parentId != -1)
			{
				SetSelectionById(parentId);
				ScrollToItemById(parentId);
				return true;
			}
			num = -1;
		}
		int num2 = base.selectedIndex;
		bool flag2;
		do
		{
			num2 += num;
			flag2 = viewController.HasChildrenByIndex(num2);
		}
		while (!flag2 && num2 >= 0 && num2 < itemsSource.Count);
		if (flag2)
		{
			SetSelection(num2);
			ScrollToItem(num2);
			return true;
		}
		return false;
	}

	public void SetSelectionById(int id)
	{
		SetSelectionById(new int[1] { id });
	}

	public void SetSelectionById(IEnumerable<int> ids)
	{
		SetSelectionInternalById(ids, sendNotification: true);
	}

	public void SetSelectionByIdWithoutNotify(IEnumerable<int> ids)
	{
		SetSelectionInternalById(ids, sendNotification: false);
	}

	internal void SetSelectionInternalById(IEnumerable<int> ids, bool sendNotification)
	{
		if (ids != null)
		{
			List<int> indices = ids.Select((int id) => GetItemIndex(id, expand: true)).ToList();
			SetSelectionInternal(indices, sendNotification);
		}
	}

	public void AddToSelectionById(int id)
	{
		int itemIndex = GetItemIndex(id, expand: true);
		Rebuild();
		AddToSelection(itemIndex);
	}

	public void RemoveFromSelectionById(int id)
	{
		int itemIndex = GetItemIndex(id);
		RemoveFromSelection(itemIndex);
	}

	private int GetItemIndex(int id, bool expand = false)
	{
		if (expand)
		{
			for (int parentId = viewController.GetParentId(id); parentId != -1; parentId = viewController.GetParentId(parentId))
			{
				if (!m_ExpandedItemIds.Contains(parentId))
				{
					viewController.ExpandItem(parentId, expandAllChildren: false);
				}
			}
		}
		return viewController.GetIndexForId(id);
	}

	internal void CopyExpandedStates(int sourceId, int targetId)
	{
		if (IsExpanded(sourceId))
		{
			ExpandItem(targetId);
			if (!viewController.HasChildren(sourceId))
			{
				return;
			}
			if (viewController.GetChildrenIds(sourceId).Count() != viewController.GetChildrenIds(targetId).Count())
			{
				Debug.LogWarning("Source and target hierarchies are not the same");
				return;
			}
			for (int i = 0; i < viewController.GetChildrenIds(sourceId).Count(); i++)
			{
				int sourceId2 = viewController.GetChildrenIds(sourceId).ElementAt(i);
				int targetId2 = viewController.GetChildrenIds(targetId).ElementAt(i);
				CopyExpandedStates(sourceId2, targetId2);
			}
		}
		else
		{
			CollapseItem(targetId);
		}
	}

	public bool IsExpanded(int id)
	{
		return viewController.IsExpanded(id);
	}

	public void CollapseItem(int id, bool collapseAllChildren = false)
	{
		viewController.CollapseItem(id, collapseAllChildren);
		RefreshItems();
	}

	public void ExpandItem(int id, bool expandAllChildren = false)
	{
		viewController.ExpandItem(id, expandAllChildren);
	}

	public void ExpandRootItems()
	{
		foreach (int rootItemId in viewController.GetRootItemIds())
		{
			viewController.ExpandItem(rootItemId, expandAllChildren: false, refresh: false);
		}
		RefreshItems();
	}

	public void ExpandAll()
	{
		viewController.ExpandAll();
	}

	public void CollapseAll()
	{
		viewController.CollapseAll();
	}

	private void OnTreeViewPointerUp(PointerUpEvent evt)
	{
		base.scrollView.contentContainer.Focus();
	}

	private bool IsExpandedByIndex(int index)
	{
		return viewController.IsExpandedByIndex(index);
	}

	private void CollapseItemByIndex(int index, bool collapseAll)
	{
		if (viewController.HasChildrenByIndex(index))
		{
			viewController.CollapseItemByIndex(index, collapseAll);
			RefreshItems();
			SaveViewData();
		}
	}

	private void ExpandItemByIndex(int index, bool expandAll)
	{
		if (viewController.HasChildrenByIndex(index))
		{
			viewController.ExpandItemByIndex(index, expandAll);
			RefreshItems();
			SaveViewData();
		}
	}
}
