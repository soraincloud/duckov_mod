using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class MultiColumnListView : BaseListView
{
	public new class UxmlFactory : UxmlFactory<MultiColumnListView, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseListView.UxmlTraits
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
			MultiColumnListView multiColumnListView = (MultiColumnListView)ve;
			multiColumnListView.sortingEnabled = m_SortingEnabled.GetValueFromBag(bag, cc);
			multiColumnListView.sortColumnDescriptions = m_SortColumnDescriptions.GetValueFromBag(bag, cc);
			multiColumnListView.columns = m_Columns.GetValueFromBag(bag, cc);
		}
	}

	private Columns m_Columns;

	private bool m_SortingEnabled;

	private SortColumnDescriptions m_SortColumnDescriptions = new SortColumnDescriptions();

	private List<SortColumnDescription> m_SortedColumns = new List<SortColumnDescription>();

	public new MultiColumnListViewController viewController => base.viewController as MultiColumnListViewController;

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

	public MultiColumnListView()
		: this(new Columns())
	{
	}

	public MultiColumnListView(Columns columns)
	{
		base.scrollView.viewDataKey = "unity-multi-column-scroll-view";
		this.columns = columns ?? new Columns();
	}

	protected override CollectionViewController CreateViewController()
	{
		return new MultiColumnListViewController(columns, sortColumnDescriptions, m_SortedColumns);
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
		CreateVirtualizationController<ReusableMultiColumnListViewItem>();
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
