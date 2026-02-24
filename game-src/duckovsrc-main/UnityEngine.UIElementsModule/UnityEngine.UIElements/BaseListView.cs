using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

public abstract class BaseListView : BaseVerticalCollectionView
{
	public new class UxmlTraits : BaseVerticalCollectionView.UxmlTraits
	{
		private readonly UxmlBoolAttributeDescription m_ShowFoldoutHeader = new UxmlBoolAttributeDescription
		{
			name = "show-foldout-header",
			defaultValue = false
		};

		private readonly UxmlStringAttributeDescription m_HeaderTitle = new UxmlStringAttributeDescription
		{
			name = "header-title",
			defaultValue = string.Empty
		};

		private readonly UxmlBoolAttributeDescription m_ShowAddRemoveFooter = new UxmlBoolAttributeDescription
		{
			name = "show-add-remove-footer",
			defaultValue = false
		};

		private readonly UxmlEnumAttributeDescription<ListViewReorderMode> m_ReorderMode = new UxmlEnumAttributeDescription<ListViewReorderMode>
		{
			name = "reorder-mode",
			defaultValue = ListViewReorderMode.Simple
		};

		private readonly UxmlBoolAttributeDescription m_ShowBoundCollectionSize = new UxmlBoolAttributeDescription
		{
			name = "show-bound-collection-size",
			defaultValue = true
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
			BaseListView baseListView = (BaseListView)ve;
			baseListView.reorderMode = m_ReorderMode.GetValueFromBag(bag, cc);
			baseListView.showFoldoutHeader = m_ShowFoldoutHeader.GetValueFromBag(bag, cc);
			baseListView.headerTitle = m_HeaderTitle.GetValueFromBag(bag, cc);
			baseListView.showAddRemoveFooter = m_ShowAddRemoveFooter.GetValueFromBag(bag, cc);
			baseListView.showBoundCollectionSize = m_ShowBoundCollectionSize.GetValueFromBag(bag, cc);
		}

		protected UxmlTraits()
		{
			m_PickingMode.defaultValue = PickingMode.Ignore;
		}
	}

	private static readonly string k_SizeFieldLabel = "Size";

	private const int k_FoldoutTabIndex = 10;

	private const int k_ArraySizeFieldTabIndex = 20;

	private bool m_ShowBoundCollectionSize = true;

	private bool m_ShowFoldoutHeader;

	private string m_HeaderTitle;

	private Label m_ListViewLabel;

	private Foldout m_Foldout;

	private TextField m_ArraySizeField;

	private bool m_IsOverMultiEditLimit;

	private int m_MaxMultiEditCount;

	private VisualElement m_Footer;

	private Button m_AddButton;

	private Button m_RemoveButton;

	private Action<IEnumerable<int>> m_ItemAddedCallback;

	private Action<IEnumerable<int>> m_ItemRemovedCallback;

	private Action m_ItemsSourceSizeChangedCallback;

	private ListViewReorderMode m_ReorderMode;

	public new static readonly string ussClassName = "unity-list-view";

	public new static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string emptyLabelUssClassName = ussClassName + "__empty-label";

	public static readonly string overMaxMultiEditLimitClassName = ussClassName + "__over-max-multi-edit-limit-label";

	public static readonly string reorderableUssClassName = ussClassName + "__reorderable";

	public static readonly string reorderableItemUssClassName = reorderableUssClassName + "-item";

	public static readonly string reorderableItemContainerUssClassName = reorderableItemUssClassName + "__container";

	public static readonly string reorderableItemHandleUssClassName = reorderableUssClassName + "-handle";

	public static readonly string reorderableItemHandleBarUssClassName = reorderableItemHandleUssClassName + "-bar";

	public static readonly string footerUssClassName = ussClassName + "__footer";

	public static readonly string foldoutHeaderUssClassName = ussClassName + "__foldout-header";

	public static readonly string arraySizeFieldUssClassName = ussClassName + "__size-field";

	public static readonly string arraySizeFieldWithHeaderUssClassName = arraySizeFieldUssClassName + "--with-header";

	public static readonly string arraySizeFieldWithFooterUssClassName = arraySizeFieldUssClassName + "--with-footer";

	public static readonly string listViewWithHeaderUssClassName = ussClassName + "--with-header";

	public static readonly string listViewWithFooterUssClassName = ussClassName + "--with-footer";

	public static readonly string scrollViewWithFooterUssClassName = ussClassName + "__scroll-view--with-footer";

	public static readonly string footerAddButtonName = ussClassName + "__add-button";

	public static readonly string footerRemoveButtonName = ussClassName + "__remove-button";

	private string m_MaxMultiEditStr;

	private static readonly string k_EmptyListStr = "List is empty";

	public bool showBoundCollectionSize
	{
		get
		{
			return m_ShowBoundCollectionSize;
		}
		set
		{
			if (m_ShowBoundCollectionSize != value)
			{
				m_ShowBoundCollectionSize = value;
				SetupArraySizeField();
			}
		}
	}

	public bool showFoldoutHeader
	{
		get
		{
			return m_ShowFoldoutHeader;
		}
		set
		{
			if (m_ShowFoldoutHeader == value)
			{
				return;
			}
			m_ShowFoldoutHeader = value;
			EnableInClassList(listViewWithHeaderUssClassName, value);
			if (m_ShowFoldoutHeader)
			{
				if (m_Foldout != null)
				{
					return;
				}
				m_Foldout = new Foldout
				{
					name = foldoutHeaderUssClassName,
					text = m_HeaderTitle
				};
				m_Foldout.toggle.tabIndex = 10;
				m_Foldout.toggle.m_Clickable.acceptClicksIfDisabled = true;
				m_Foldout.AddToClassList(foldoutHeaderUssClassName);
				m_Foldout.tabIndex = 1;
				base.hierarchy.Add(m_Foldout);
				m_Foldout.Add(base.scrollView);
			}
			else if (m_Foldout != null)
			{
				m_Foldout?.RemoveFromHierarchy();
				m_Foldout = null;
				base.hierarchy.Add(base.scrollView);
			}
			SetupArraySizeField();
			UpdateListViewLabel();
			if (showAddRemoveFooter)
			{
				EnableFooter(enabled: true);
			}
		}
	}

	public string headerTitle
	{
		get
		{
			return m_HeaderTitle;
		}
		set
		{
			m_HeaderTitle = value;
			if (m_Foldout != null)
			{
				m_Foldout.text = m_HeaderTitle;
			}
		}
	}

	public bool showAddRemoveFooter
	{
		get
		{
			return m_Footer != null;
		}
		set
		{
			EnableFooter(value);
		}
	}

	internal Foldout headerFoldout => m_Foldout;

	internal TextField arraySizeField => m_ArraySizeField;

	internal VisualElement footer => m_Footer;

	public new BaseListViewController viewController => base.viewController as BaseListViewController;

	public ListViewReorderMode reorderMode
	{
		get
		{
			return m_ReorderMode;
		}
		set
		{
			if (value != m_ReorderMode)
			{
				m_ReorderMode = value;
				InitializeDragAndDropController(base.reorderable);
				this.reorderModeChanged?.Invoke();
				Rebuild();
			}
		}
	}

	public event Action<IEnumerable<int>> itemsAdded;

	public event Action<IEnumerable<int>> itemsRemoved;

	internal event Action itemsSourceSizeChanged;

	internal event Action reorderModeChanged;

	internal void SetupArraySizeField()
	{
		if (!showBoundCollectionSize || (!showFoldoutHeader && GetProperty("__unity-collection-view-internal-binding") == null))
		{
			m_ArraySizeField?.RemoveFromHierarchy();
			return;
		}
		if (m_ArraySizeField == null)
		{
			m_ArraySizeField = new TextField
			{
				name = arraySizeFieldUssClassName,
				tabIndex = 20
			};
			m_ArraySizeField.AddToClassList(arraySizeFieldUssClassName);
			m_ArraySizeField.RegisterValueChangedCallback(OnArraySizeFieldChanged);
			m_ArraySizeField.isDelayed = true;
			m_ArraySizeField.focusable = true;
		}
		m_ArraySizeField.EnableInClassList(arraySizeFieldWithFooterUssClassName, showAddRemoveFooter);
		m_ArraySizeField.EnableInClassList(arraySizeFieldWithHeaderUssClassName, showFoldoutHeader);
		if (showFoldoutHeader)
		{
			m_ArraySizeField.label = string.Empty;
			base.hierarchy.Add(m_ArraySizeField);
		}
		else
		{
			m_ArraySizeField.label = k_SizeFieldLabel;
			base.hierarchy.Insert(0, m_ArraySizeField);
		}
		UpdateArraySizeField();
	}

	private void EnableFooter(bool enabled)
	{
		EnableInClassList(listViewWithFooterUssClassName, enabled);
		base.scrollView.EnableInClassList(scrollViewWithFooterUssClassName, enabled);
		if (m_ArraySizeField != null)
		{
			m_ArraySizeField.EnableInClassList(arraySizeFieldWithFooterUssClassName, enabled);
		}
		if (enabled)
		{
			if (m_Footer == null)
			{
				m_Footer = new VisualElement
				{
					name = footerUssClassName
				};
				m_Footer.AddToClassList(footerUssClassName);
				m_AddButton = new Button(OnAddClicked)
				{
					name = footerAddButtonName,
					text = "+"
				};
				m_Footer.Add(m_AddButton);
				m_RemoveButton = new Button(OnRemoveClicked)
				{
					name = footerRemoveButtonName,
					text = "-"
				};
				m_Footer.Add(m_RemoveButton);
			}
			if (m_Foldout != null)
			{
				m_Foldout.contentContainer.Add(m_Footer);
			}
			else
			{
				base.hierarchy.Add(m_Footer);
			}
		}
		else
		{
			m_RemoveButton?.RemoveFromHierarchy();
			m_AddButton?.RemoveFromHierarchy();
			m_Footer?.RemoveFromHierarchy();
			m_RemoveButton = null;
			m_AddButton = null;
			m_Footer = null;
		}
	}

	private void AddItems(int itemCount)
	{
		viewController.AddItems(itemCount);
	}

	private void RemoveItems(List<int> indices)
	{
		viewController.RemoveItems(indices);
	}

	private void OnArraySizeFieldChanged(ChangeEvent<string> evt)
	{
		if (m_ArraySizeField.showMixedValue && BaseField<string>.mixedValueString == evt.newValue)
		{
			return;
		}
		if (!int.TryParse(evt.newValue, out var result) || result < 0)
		{
			m_ArraySizeField.SetValueWithoutNotify(evt.previousValue);
			return;
		}
		int itemsCount = viewController.GetItemsCount();
		if (itemsCount != 0 || result != viewController.GetItemsMinCount())
		{
			if (result > itemsCount)
			{
				viewController.AddItems(result - itemsCount);
			}
			else if (result < itemsCount)
			{
				viewController.RemoveItems(itemsCount - result);
			}
			else if (result == 0)
			{
				viewController.ClearItems();
				m_IsOverMultiEditLimit = false;
			}
			UpdateListViewLabel();
		}
	}

	internal void UpdateArraySizeField()
	{
		if (HasValidDataAndBindings() && m_ArraySizeField != null)
		{
			if (!m_ArraySizeField.showMixedValue)
			{
				m_ArraySizeField.SetValueWithoutNotify(viewController.GetItemsMinCount().ToString());
			}
			footer?.SetEnabled(!m_IsOverMultiEditLimit);
		}
	}

	internal void UpdateListViewLabel()
	{
		if (!HasValidDataAndBindings())
		{
			return;
		}
		bool flag = base.itemsSource.Count == 0;
		if (m_IsOverMultiEditLimit)
		{
			if (m_ListViewLabel == null)
			{
				m_ListViewLabel = new Label();
			}
			m_ListViewLabel.text = m_MaxMultiEditStr;
			base.scrollView.contentViewport.Add(m_ListViewLabel);
		}
		else if (flag)
		{
			if (m_ListViewLabel == null)
			{
				m_ListViewLabel = new Label();
			}
			m_ListViewLabel.text = k_EmptyListStr;
			base.scrollView.contentViewport.Add(m_ListViewLabel);
		}
		else
		{
			m_ListViewLabel?.RemoveFromHierarchy();
			m_ListViewLabel = null;
		}
		m_ListViewLabel?.EnableInClassList(emptyLabelUssClassName, flag);
		m_ListViewLabel?.EnableInClassList(overMaxMultiEditLimitClassName, m_IsOverMultiEditLimit);
	}

	private void OnAddClicked()
	{
		AddItems(1);
		if (base.binding == null)
		{
			SetSelection(base.itemsSource.Count - 1);
			ScrollToItem(-1);
		}
		else
		{
			base.schedule.Execute((Action)delegate
			{
				SetSelection(base.itemsSource.Count - 1);
				ScrollToItem(-1);
			}).ExecuteLater(100L);
		}
		if (HasValidDataAndBindings() && m_ArraySizeField != null)
		{
			m_ArraySizeField.showMixedValue = false;
		}
	}

	private void OnRemoveClicked()
	{
		if (base.selectedIndices.Any())
		{
			viewController.RemoveItems(base.selectedIndices.ToList());
			ClearSelection();
		}
		else if (base.itemsSource.Count > 0)
		{
			int index = base.itemsSource.Count - 1;
			viewController.RemoveItem(index);
		}
		if (HasValidDataAndBindings() && m_ArraySizeField != null)
		{
			m_ArraySizeField.showMixedValue = false;
		}
	}

	internal void SetOverMaxMultiEditLimit(bool isOverLimit, int maxMultiEditCount)
	{
		m_IsOverMultiEditLimit = isOverLimit;
		m_MaxMultiEditCount = maxMultiEditCount;
		m_MaxMultiEditStr = $"This field cannot display arrays with more than {m_MaxMultiEditCount} elements when multiple objects are selected.";
	}

	private protected override void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableListViewItem>();
	}

	public override void SetViewController(CollectionViewController controller)
	{
		if (m_ItemAddedCallback == null)
		{
			m_ItemAddedCallback = OnItemAdded;
		}
		if (m_ItemRemovedCallback == null)
		{
			m_ItemRemovedCallback = OnItemsRemoved;
		}
		if (m_ItemsSourceSizeChangedCallback == null)
		{
			m_ItemsSourceSizeChangedCallback = OnItemsSourceSizeChanged;
		}
		if (viewController != null)
		{
			viewController.itemsAdded -= m_ItemAddedCallback;
			viewController.itemsRemoved -= m_ItemRemovedCallback;
			viewController.itemsSourceSizeChanged -= m_ItemsSourceSizeChangedCallback;
		}
		base.SetViewController(controller);
		if (viewController != null)
		{
			viewController.itemsAdded += m_ItemAddedCallback;
			viewController.itemsRemoved += m_ItemRemovedCallback;
			viewController.itemsSourceSizeChanged += m_ItemsSourceSizeChangedCallback;
		}
	}

	private void OnItemAdded(IEnumerable<int> indices)
	{
		this.itemsAdded?.Invoke(indices);
	}

	private void OnItemsRemoved(IEnumerable<int> indices)
	{
		this.itemsRemoved?.Invoke(indices);
	}

	private void OnItemsSourceSizeChanged()
	{
		if (GetProperty("__unity-collection-view-internal-binding") == null)
		{
			RefreshItems();
		}
		this.itemsSourceSizeChanged?.Invoke();
	}

	internal override ListViewDragger CreateDragger()
	{
		if (m_ReorderMode == ListViewReorderMode.Simple)
		{
			return new ListViewDragger(this);
		}
		return new ListViewDraggerAnimated(this);
	}

	internal override ICollectionDragAndDropController CreateDragAndDropController()
	{
		return new ListViewReorderableDragAndDropController(this);
	}

	public BaseListView()
	{
		AddToClassList(ussClassName);
		base.pickingMode = PickingMode.Ignore;
	}

	public BaseListView(IList itemsSource, float itemHeight = -1f)
		: base(itemsSource, itemHeight)
	{
		AddToClassList(ussClassName);
		base.pickingMode = PickingMode.Ignore;
	}

	private protected override void PostRefresh()
	{
		UpdateArraySizeField();
		UpdateListViewLabel();
		base.PostRefresh();
	}

	private protected override bool HandleItemNavigation(bool moveIn, bool altPressed)
	{
		bool result = false;
		foreach (int selectedIndex in base.selectedIndices)
		{
			foreach (ReusableCollectionItem activeItem in base.activeItems)
			{
				if (activeItem.index == selectedIndex && GetProperty("__unity-collection-view-internal-binding") != null)
				{
					Foldout foldout = activeItem.bindableElement.Q<Foldout>();
					if (foldout != null)
					{
						foldout.value = moveIn;
						result = true;
					}
				}
			}
		}
		return result;
	}
}
