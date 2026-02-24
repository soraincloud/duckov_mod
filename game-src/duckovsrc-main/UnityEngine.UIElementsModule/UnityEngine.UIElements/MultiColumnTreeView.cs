using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class MultiColumnTreeView : BaseTreeView
{
	public new class UxmlFactory : UxmlFactory<MultiColumnTreeView, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseTreeView.UxmlTraits
	{
		private readonly UxmlBoolAttributeDescription m_SortingEnabled = new UxmlBoolAttributeDescription
		{
			name = "sorting-enabled"
		};

		private readonly UxmlObjectAttributeDescription<Columns> m_Columns = new UxmlObjectAttributeDescription<Columns>();

		private readonly UxmlObjectAttributeDescription<SortColumnDescriptions> m_SortColumnDescriptions = new UxmlObjectAttributeDescription<SortColumnDescriptions>();

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			MultiColumnTreeView multiColumnTreeView = (MultiColumnTreeView)ve;
			multiColumnTreeView.sortingEnabled = m_SortingEnabled.GetValueFromBag(bag, cc);
			multiColumnTreeView.sortColumnDescriptions = m_SortColumnDescriptions.GetValueFromBag(bag, cc);
			multiColumnTreeView.columns = m_Columns.GetValueFromBag(bag, cc);
		}
	}

	private Columns m_Columns;

	private bool m_SortingEnabled;

	private SortColumnDescriptions m_SortColumnDescriptions = new SortColumnDescriptions();

	private List<SortColumnDescription> m_SortedColumns = new List<SortColumnDescription>();

	public new MultiColumnTreeViewController viewController => base.viewController as MultiColumnTreeViewController;

	public IEnumerable<SortColumnDescription> sortedColumns => m_SortedColumns;

	public Columns columns
	{
		get
		{
			return m_Columns;
		}
		private set
		{
			if (value == null)
			{
				m_Columns.Clear();
				return;
			}
			m_Columns = value;
			if (m_Columns.Count > 0)
			{
				GetOrCreateViewController();
			}
		}
	}

	public SortColumnDescriptions sortColumnDescriptions
	{
		get
		{
			return m_SortColumnDescriptions;
		}
		private set
		{
			if (value == null)
			{
				m_SortColumnDescriptions.Clear();
				return;
			}
			m_SortColumnDescriptions = value;
			if (viewController != null)
			{
				viewController.columnController.header.sortDescriptions = value;
				RaiseColumnSortingChanged();
			}
		}
	}

	public bool sortingEnabled
	{
		get
		{
			return m_SortingEnabled;
		}
		set
		{
			m_SortingEnabled = value;
			if (viewController != null)
			{
				viewController.columnController.header.sortingEnabled = value;
			}
		}
	}

	public event Action columnSortingChanged;

	public event Action<ContextualMenuPopulateEvent, Column> headerContextMenuPopulateEvent;

	public MultiColumnTreeView()
		: this(new Columns())
	{
	}

	public MultiColumnTreeView(Columns columns)
	{
		base.scrollView.viewDataKey = "unity-multi-column-scroll-view";
		this.columns = columns ?? new Columns();
	}

	internal override void SetRootItemsInternal<T>(IList<TreeViewItemData<T>> rootItems)
	{
		TreeViewHelpers<T, DefaultMultiColumnTreeViewController<T>>.SetRootItems(this, rootItems, () => new DefaultMultiColumnTreeViewController<T>(columns, m_SortColumnDescriptions, m_SortedColumns));
	}

	private protected override IEnumerable<TreeViewItemData<T>> GetSelectedItemsInternal<T>()
	{
		return TreeViewHelpers<T, DefaultMultiColumnTreeViewController<T>>.GetSelectedItems(this);
	}

	private protected override T GetItemDataForIndexInternal<T>(int index)
	{
		return TreeViewHelpers<T, DefaultMultiColumnTreeViewController<T>>.GetItemDataForIndex(this, index);
	}

	private protected override T GetItemDataForIdInternal<T>(int id)
	{
		return TreeViewHelpers<T, DefaultMultiColumnTreeViewController<T>>.GetItemDataForId(this, id);
	}

	private protected override void AddItemInternal<T>(TreeViewItemData<T> item, int parentId, int childIndex, bool rebuildTree)
	{
		TreeViewHelpers<T, DefaultMultiColumnTreeViewController<T>>.AddItem(this, item, parentId, childIndex, rebuildTree);
	}

	protected override CollectionViewController CreateViewController()
	{
		return new DefaultMultiColumnTreeViewController<object>(columns, sortColumnDescriptions, m_SortedColumns);
	}

	public override void SetViewController(CollectionViewController controller)
	{
		if (viewController != null)
		{
			viewController.columnController.columnSortingChanged -= RaiseColumnSortingChanged;
			viewController.columnController.headerContextMenuPopulateEvent -= RaiseHeaderContextMenuPopulate;
		}
		base.SetViewController(controller);
		if (viewController != null)
		{
			viewController.header.sortingEnabled = m_SortingEnabled;
			viewController.columnController.columnSortingChanged += RaiseColumnSortingChanged;
			viewController.columnController.headerContextMenuPopulateEvent += RaiseHeaderContextMenuPopulate;
		}
	}

	private protected override void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableMultiColumnTreeViewItem>();
	}

	private void RaiseColumnSortingChanged()
	{
		this.columnSortingChanged?.Invoke();
	}

	private void RaiseHeaderContextMenuPopulate(ContextualMenuPopulateEvent evt, Column column)
	{
		this.headerContextMenuPopulateEvent?.Invoke(evt, column);
	}
}
