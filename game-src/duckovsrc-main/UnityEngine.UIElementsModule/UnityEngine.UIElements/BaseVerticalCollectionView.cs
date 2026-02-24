using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

public abstract class BaseVerticalCollectionView : BindableElement, ISerializationCallbackReceiver
{
	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private readonly UxmlIntAttributeDescription m_FixedItemHeight = new UxmlIntAttributeDescription
		{
			name = "fixed-item-height",
			obsoleteNames = new string[1] { "itemHeight, item-height" },
			defaultValue = s_DefaultItemHeight
		};

		private readonly UxmlEnumAttributeDescription<CollectionVirtualizationMethod> m_VirtualizationMethod = new UxmlEnumAttributeDescription<CollectionVirtualizationMethod>
		{
			name = "virtualization-method",
			defaultValue = CollectionVirtualizationMethod.FixedHeight
		};

		private readonly UxmlBoolAttributeDescription m_ShowBorder = new UxmlBoolAttributeDescription
		{
			name = "show-border",
			defaultValue = false
		};

		private readonly UxmlEnumAttributeDescription<SelectionType> m_SelectionType = new UxmlEnumAttributeDescription<SelectionType>
		{
			name = "selection-type",
			defaultValue = SelectionType.Single
		};

		private readonly UxmlEnumAttributeDescription<AlternatingRowBackground> m_ShowAlternatingRowBackgrounds = new UxmlEnumAttributeDescription<AlternatingRowBackground>
		{
			name = "show-alternating-row-backgrounds",
			defaultValue = AlternatingRowBackground.None
		};

		private readonly UxmlBoolAttributeDescription m_Reorderable = new UxmlBoolAttributeDescription
		{
			name = "reorderable",
			defaultValue = false
		};

		private readonly UxmlBoolAttributeDescription m_HorizontalScrollingEnabled = new UxmlBoolAttributeDescription
		{
			name = "horizontal-scrolling",
			defaultValue = false
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public UxmlTraits()
		{
			base.focusable.defaultValue = true;
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			int value = 0;
			BaseVerticalCollectionView baseVerticalCollectionView = (BaseVerticalCollectionView)ve;
			baseVerticalCollectionView.reorderable = m_Reorderable.GetValueFromBag(bag, cc);
			if (m_FixedItemHeight.TryGetValueFromBag(bag, cc, ref value))
			{
				baseVerticalCollectionView.fixedItemHeight = value;
			}
			baseVerticalCollectionView.virtualizationMethod = m_VirtualizationMethod.GetValueFromBag(bag, cc);
			baseVerticalCollectionView.showBorder = m_ShowBorder.GetValueFromBag(bag, cc);
			baseVerticalCollectionView.selectionType = m_SelectionType.GetValueFromBag(bag, cc);
			baseVerticalCollectionView.showAlternatingRowBackgrounds = m_ShowAlternatingRowBackgrounds.GetValueFromBag(bag, cc);
			baseVerticalCollectionView.horizontalScrollingEnabled = m_HorizontalScrollingEnabled.GetValueFromBag(bag, cc);
		}
	}

	private class Selection
	{
		private readonly HashSet<int> m_IndexLookup = new HashSet<int>();

		private readonly HashSet<int> m_IdLookup = new HashSet<int>();

		private int m_MinIndex = -1;

		private int m_MaxIndex = -1;

		public readonly List<int> indices = new List<int>();

		public readonly Dictionary<int, object> items = new Dictionary<int, object>();

		public List<int> selectedIds { get; set; }

		public int indexCount => indices.Count;

		public int idCount => selectedIds.Count;

		public int minIndex
		{
			get
			{
				if (m_MinIndex == -1)
				{
					m_MinIndex = indices.Min();
				}
				return m_MinIndex;
			}
		}

		public int maxIndex
		{
			get
			{
				if (m_MaxIndex == -1)
				{
					m_MaxIndex = indices.Max();
				}
				return m_MaxIndex;
			}
		}

		public int capacity
		{
			get
			{
				return indices.Capacity;
			}
			set
			{
				indices.Capacity = value;
				if (selectedIds.Capacity < value)
				{
					selectedIds.Capacity = value;
				}
			}
		}

		public int FirstIndex()
		{
			return (indices.Count > 0) ? indices[0] : (-1);
		}

		public object FirstObject()
		{
			object value;
			return items.TryGetValue(FirstIndex(), out value) ? value : null;
		}

		public bool ContainsIndex(int index)
		{
			return m_IndexLookup.Contains(index);
		}

		public bool ContainsId(int id)
		{
			return m_IdLookup.Contains(id);
		}

		public void AddId(int id)
		{
			selectedIds.Add(id);
			m_IdLookup.Add(id);
		}

		public void AddIndex(int index, object obj)
		{
			m_IndexLookup.Add(index);
			indices.Add(index);
			items[index] = obj;
			if (index < m_MinIndex)
			{
				m_MinIndex = index;
			}
			if (index > m_MaxIndex)
			{
				m_MaxIndex = index;
			}
		}

		public bool TryRemove(int index)
		{
			if (!m_IndexLookup.Remove(index))
			{
				return false;
			}
			int num = indices.IndexOf(index);
			if (num >= 0)
			{
				indices.RemoveAt(num);
				items.Remove(index);
				if (index == m_MinIndex)
				{
					m_MinIndex = -1;
				}
				if (index == m_MaxIndex)
				{
					m_MaxIndex = -1;
				}
			}
			return true;
		}

		public void RemoveId(int id)
		{
			selectedIds.Remove(id);
			m_IdLookup.Remove(id);
		}

		public void ClearItems()
		{
			items.Clear();
		}

		public void ClearIds()
		{
			m_IdLookup.Clear();
			selectedIds.Clear();
		}

		public void ClearIndices()
		{
			m_IndexLookup.Clear();
			indices.Clear();
			m_MinIndex = -1;
			m_MaxIndex = -1;
		}

		public void Clear()
		{
			ClearItems();
			ClearIds();
			ClearIndices();
		}
	}

	private static readonly ProfilerMarker k_RefreshMarker = new ProfilerMarker("BaseVerticalCollectionView.RefreshItems");

	private static readonly ProfilerMarker k_RebuildMarker = new ProfilerMarker("BaseVerticalCollectionView.Rebuild");

	internal const string internalBindingKey = "__unity-collection-view-internal-binding";

	private SelectionType m_SelectionType;

	private static readonly List<ReusableCollectionItem> k_EmptyItems = new List<ReusableCollectionItem>();

	private bool m_HorizontalScrollingEnabled;

	[SerializeField]
	private AlternatingRowBackground m_ShowAlternatingRowBackgrounds = AlternatingRowBackground.None;

	internal static readonly int s_DefaultItemHeight = 22;

	internal float m_FixedItemHeight = s_DefaultItemHeight;

	internal bool m_ItemHeightIsInline;

	private CollectionVirtualizationMethod m_VirtualizationMethod;

	private readonly ScrollView m_ScrollView;

	private CollectionViewController m_ViewController;

	private CollectionVirtualizationController m_VirtualizationController;

	private KeyboardNavigationManipulator m_NavigationManipulator;

	[SerializeField]
	internal SerializedVirtualizationData serializedVirtualizationData = new SerializedVirtualizationData();

	[SerializeField]
	private readonly List<int> m_SelectedIds = new List<int>();

	private readonly Selection m_Selection;

	private float m_LastHeight;

	private bool m_IsRangeSelectionDirectionUp;

	private ListViewDragger m_Dragger;

	internal const float ItemHeightUnset = -1f;

	internal static CustomStyleProperty<int> s_ItemHeightProperty = new CustomStyleProperty<int>("--unity-item-height");

	private Action<int, int> m_ItemIndexChangedCallback;

	private Action m_ItemsSourceChangedCallback;

	internal IVisualElementScheduledItem m_RebuildScheduled;

	public static readonly string ussClassName = "unity-collection-view";

	public static readonly string borderUssClassName = ussClassName + "--with-border";

	public static readonly string itemUssClassName = ussClassName + "__item";

	public static readonly string dragHoverBarUssClassName = ussClassName + "__drag-hover-bar";

	public static readonly string dragHoverMarkerUssClassName = ussClassName + "__drag-hover-marker";

	public static readonly string itemDragHoverUssClassName = itemUssClassName + "--drag-hover";

	public static readonly string itemSelectedVariantUssClassName = itemUssClassName + "--selected";

	public static readonly string itemAlternativeBackgroundUssClassName = itemUssClassName + "--alternative-background";

	public static readonly string listScrollViewUssClassName = ussClassName + "__scroll-view";

	internal static readonly string backgroundFillUssClassName = ussClassName + "__background-fill";

	private Vector3 m_TouchDownPosition;

	private long m_LastPointerDownTimeStamp;

	private int m_PointerDownCount;

	public IList itemsSource
	{
		get
		{
			return viewController?.itemsSource;
		}
		set
		{
			GetOrCreateViewController().itemsSource = value;
		}
	}

	[Obsolete("makeItem has been moved to ListView and TreeView. Use these ones instead.")]
	public Func<VisualElement> makeItem
	{
		get
		{
			throw new UnityException("makeItem has been moved to ListView and TreeView. Use these ones instead.");
		}
		set
		{
			throw new UnityException("makeItem has been moved to ListView and TreeView. Use these ones instead.");
		}
	}

	[Obsolete("bindItem has been moved to ListView and TreeView. Use these ones instead.")]
	public Action<VisualElement, int> bindItem
	{
		get
		{
			throw new UnityException("bindItem has been moved to ListView and TreeView. Use these ones instead.");
		}
		set
		{
			throw new UnityException("bindItem has been moved to ListView and TreeView. Use these ones instead.");
		}
	}

	[Obsolete("unbindItem has been moved to ListView and TreeView. Use these ones instead.")]
	public Action<VisualElement, int> unbindItem
	{
		get
		{
			throw new UnityException("unbindItem has been moved to ListView and TreeView. Use these ones instead.");
		}
		set
		{
			throw new UnityException("unbindItem has been moved to ListView and TreeView. Use these ones instead.");
		}
	}

	[Obsolete("destroyItem has been moved to ListView and TreeView. Use these ones instead.")]
	public Action<VisualElement> destroyItem
	{
		get
		{
			throw new UnityException("destroyItem has been moved to ListView and TreeView. Use these ones instead.");
		}
		set
		{
			throw new UnityException("destroyItem has been moved to ListView and TreeView. Use these ones instead.");
		}
	}

	public override VisualElement contentContainer => null;

	public SelectionType selectionType
	{
		get
		{
			return m_SelectionType;
		}
		set
		{
			m_SelectionType = value;
			if (m_SelectionType == SelectionType.None)
			{
				ClearSelection();
			}
			else if (m_SelectionType == SelectionType.Single && m_Selection.indexCount > 1)
			{
				SetSelection(m_Selection.FirstIndex());
			}
		}
	}

	public object selectedItem => m_Selection.FirstObject();

	public IEnumerable<object> selectedItems
	{
		get
		{
			foreach (int index in m_Selection.indices)
			{
				if (m_Selection.items.TryGetValue(index, out var item))
				{
					yield return item;
				}
				else
				{
					yield return null;
				}
				item = null;
			}
		}
	}

	public IEnumerable<int> selectedIds => m_Selection.selectedIds;

	public int selectedIndex
	{
		get
		{
			return (m_Selection.indexCount == 0) ? (-1) : m_Selection.FirstIndex();
		}
		set
		{
			SetSelection(value);
		}
	}

	public IEnumerable<int> selectedIndices => m_Selection.indices;

	internal IEnumerable<ReusableCollectionItem> activeItems => m_VirtualizationController?.activeItems ?? k_EmptyItems;

	internal ScrollView scrollView => m_ScrollView;

	internal ListViewDragger dragger => m_Dragger;

	internal CollectionVirtualizationController virtualizationController => GetOrCreateVirtualizationController();

	public CollectionViewController viewController => m_ViewController;

	[Obsolete("resolvedItemHeight is deprecated and will be removed from the API.", false)]
	public float resolvedItemHeight => ResolveItemHeight();

	public bool showBorder
	{
		get
		{
			return m_ScrollView.ClassListContains(borderUssClassName);
		}
		set
		{
			m_ScrollView.EnableInClassList(borderUssClassName, value);
		}
	}

	public bool reorderable
	{
		get
		{
			return m_Dragger?.dragAndDropController?.enableReordering == true;
		}
		set
		{
			ICollectionDragAndDropController dragAndDropController = m_Dragger.dragAndDropController;
			if (dragAndDropController != null && dragAndDropController.enableReordering != value)
			{
				dragAndDropController.enableReordering = value;
				Rebuild();
			}
		}
	}

	public bool horizontalScrollingEnabled
	{
		get
		{
			return m_HorizontalScrollingEnabled;
		}
		set
		{
			if (m_HorizontalScrollingEnabled != value)
			{
				m_HorizontalScrollingEnabled = value;
				m_ScrollView.horizontalScrollerVisibility = ((!value) ? ScrollerVisibility.Hidden : ScrollerVisibility.Auto);
				m_ScrollView.mode = (value ? ScrollViewMode.VerticalAndHorizontal : ScrollViewMode.Vertical);
			}
		}
	}

	public AlternatingRowBackground showAlternatingRowBackgrounds
	{
		get
		{
			return m_ShowAlternatingRowBackgrounds;
		}
		set
		{
			if (m_ShowAlternatingRowBackgrounds != value)
			{
				m_ShowAlternatingRowBackgrounds = value;
				RefreshItems();
			}
		}
	}

	public CollectionVirtualizationMethod virtualizationMethod
	{
		get
		{
			return m_VirtualizationMethod;
		}
		set
		{
			CollectionVirtualizationMethod collectionVirtualizationMethod = m_VirtualizationMethod;
			m_VirtualizationMethod = value;
			if (collectionVirtualizationMethod != value)
			{
				CreateVirtualizationController();
				Rebuild();
			}
		}
	}

	[Obsolete("itemHeight is deprecated, use fixedItemHeight instead.", false)]
	public int itemHeight
	{
		get
		{
			return (int)fixedItemHeight;
		}
		set
		{
			fixedItemHeight = value;
		}
	}

	public float fixedItemHeight
	{
		get
		{
			return m_FixedItemHeight;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("fixedItemHeight", "Value needs to be positive for virtualization.");
			}
			m_ItemHeightIsInline = true;
			if (Math.Abs(m_FixedItemHeight - value) > float.Epsilon)
			{
				m_FixedItemHeight = value;
				RefreshItems();
			}
		}
	}

	internal float lastHeight => m_LastHeight;

	[Obsolete("onItemsChosen is deprecated, use itemsChosen instead", false)]
	public event Action<IEnumerable<object>> onItemsChosen
	{
		add
		{
			itemsChosen += value;
		}
		remove
		{
			itemsChosen -= value;
		}
	}

	public event Action<IEnumerable<object>> itemsChosen;

	[Obsolete("onSelectionChange is deprecated, use selectionChanged instead", false)]
	public event Action<IEnumerable<object>> onSelectionChange
	{
		add
		{
			selectionChanged += value;
		}
		remove
		{
			selectionChanged -= value;
		}
	}

	public event Action<IEnumerable<object>> selectionChanged;

	[Obsolete("onSelectedIndicesChange is deprecated, use selectedIndicesChanged instead", false)]
	public event Action<IEnumerable<int>> onSelectedIndicesChange
	{
		add
		{
			selectedIndicesChanged += value;
		}
		remove
		{
			selectedIndicesChanged -= value;
		}
	}

	public event Action<IEnumerable<int>> selectedIndicesChanged;

	public event Action<int, int> itemIndexChanged;

	public event Action itemsSourceChanged;

	internal event Action selectionNotChanged;

	internal event Func<CanStartDragArgs, bool> canStartDrag;

	internal event Func<SetupDragAndDropArgs, StartDragArgs> setupDragAndDrop;

	internal event Func<HandleDragAndDropArgs, DragVisualMode> dragAndDropUpdate;

	internal event Func<HandleDragAndDropArgs, DragVisualMode> handleDrop;

	internal bool HasCanStartDrag()
	{
		return this.canStartDrag != null;
	}

	internal bool RaiseCanStartDrag(ReusableCollectionItem item, IEnumerable<int> ids)
	{
		return this.canStartDrag?.Invoke(new CanStartDragArgs(item?.rootElement, item?.id ?? (-1), ids)) ?? true;
	}

	internal StartDragArgs RaiseSetupDragAndDrop(ReusableCollectionItem item, IEnumerable<int> ids, StartDragArgs args)
	{
		return this.setupDragAndDrop?.Invoke(new SetupDragAndDropArgs(item?.rootElement, ids, args)) ?? args;
	}

	internal DragVisualMode RaiseHandleDragAndDrop(Vector2 pointerPosition, DragAndDropArgs dragAndDropArgs)
	{
		return this.dragAndDropUpdate?.Invoke(new HandleDragAndDropArgs(pointerPosition, dragAndDropArgs)) ?? DragVisualMode.None;
	}

	internal DragVisualMode RaiseDrop(Vector2 pointerPosition, DragAndDropArgs dragAndDropArgs)
	{
		return this.handleDrop?.Invoke(new HandleDragAndDropArgs(pointerPosition, dragAndDropArgs)) ?? DragVisualMode.None;
	}

	internal float ResolveItemHeight(float height = -1f)
	{
		return (height < 0f) ? fixedItemHeight : height;
	}

	private protected virtual void CreateVirtualizationController()
	{
		CreateVirtualizationController<ReusableCollectionItem>();
	}

	internal CollectionVirtualizationController GetOrCreateVirtualizationController()
	{
		if (m_VirtualizationController == null)
		{
			CreateVirtualizationController();
		}
		return m_VirtualizationController;
	}

	internal void CreateVirtualizationController<T>() where T : ReusableCollectionItem, new()
	{
		switch (virtualizationMethod)
		{
		case CollectionVirtualizationMethod.FixedHeight:
			m_VirtualizationController = new FixedHeightVirtualizationController<T>(this);
			break;
		case CollectionVirtualizationMethod.DynamicHeight:
			m_VirtualizationController = new DynamicHeightVirtualizationController<T>(this);
			break;
		default:
			throw new ArgumentOutOfRangeException("virtualizationMethod", virtualizationMethod, "Unsupported virtualizationMethod virtualization");
		}
	}

	internal CollectionViewController GetOrCreateViewController()
	{
		if (m_ViewController == null)
		{
			SetViewController(CreateViewController());
		}
		return m_ViewController;
	}

	protected abstract CollectionViewController CreateViewController();

	public virtual void SetViewController(CollectionViewController controller)
	{
		if (m_ViewController != null)
		{
			m_ViewController.itemIndexChanged -= m_ItemIndexChangedCallback;
			m_ViewController.itemsSourceChanged -= m_ItemsSourceChangedCallback;
			m_ViewController.Dispose();
			m_ViewController = null;
		}
		m_ViewController = controller;
		if (m_ViewController != null)
		{
			m_ViewController.SetView(this);
			m_ViewController.itemIndexChanged += m_ItemIndexChangedCallback;
			m_ViewController.itemsSourceChanged += m_ItemsSourceChangedCallback;
		}
	}

	internal virtual ListViewDragger CreateDragger()
	{
		return new ListViewDragger(this);
	}

	internal void InitializeDragAndDropController(bool enableReordering)
	{
		if (m_Dragger != null)
		{
			m_Dragger.UnregisterCallbacksFromTarget(unregisterPanelEvents: true);
			m_Dragger.dragAndDropController = null;
			m_Dragger = null;
		}
		m_Dragger = CreateDragger();
		m_Dragger.dragAndDropController = CreateDragAndDropController();
		if (m_Dragger.dragAndDropController != null)
		{
			m_Dragger.dragAndDropController.enableReordering = enableReordering;
		}
	}

	internal abstract ICollectionDragAndDropController CreateDragAndDropController();

	internal void SetDragAndDropController(ICollectionDragAndDropController dragAndDropController)
	{
		if (m_Dragger == null)
		{
			m_Dragger = CreateDragger();
		}
		m_Dragger.dragAndDropController = dragAndDropController;
	}

	public BaseVerticalCollectionView()
	{
		AddToClassList(ussClassName);
		m_Selection = new Selection
		{
			selectedIds = m_SelectedIds
		};
		selectionType = SelectionType.Single;
		m_ScrollView = new ScrollView();
		m_ScrollView.AddToClassList(listScrollViewUssClassName);
		m_ScrollView.verticalScroller.valueChanged += delegate(float v)
		{
			OnScroll(new Vector2(0f, v));
		};
		m_ScrollView.RegisterCallback<GeometryChangedEvent>(OnSizeChanged);
		RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
		m_ScrollView.contentContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		m_ScrollView.contentContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		base.hierarchy.Add(m_ScrollView);
		m_ScrollView.contentContainer.focusable = true;
		m_ScrollView.contentContainer.usageHints &= ~UsageHints.GroupTransform;
		m_ScrollView.viewDataKey = "unity-vertical-collection-scroll-view";
		m_ScrollView.verticalScroller.viewDataKey = null;
		m_ScrollView.horizontalScroller.viewDataKey = null;
		base.focusable = true;
		base.isCompositeRoot = true;
		base.delegatesFocus = true;
		m_ItemIndexChangedCallback = OnItemIndexChanged;
		m_ItemsSourceChangedCallback = OnItemsSourceChanged;
		InitializeDragAndDropController(enableReordering: false);
	}

	public BaseVerticalCollectionView(IList itemsSource, float itemHeight = -1f)
		: this()
	{
		if (Math.Abs(itemHeight - -1f) > float.Epsilon)
		{
			m_FixedItemHeight = itemHeight;
			m_ItemHeightIsInline = true;
		}
		if (itemsSource != null)
		{
			this.itemsSource = itemsSource;
		}
	}

	[Obsolete("makeItem and bindItem are now in ListView and TreeView directly, please use a constructor without these parameters.")]
	public BaseVerticalCollectionView(IList itemsSource, float itemHeight = -1f, Func<VisualElement> makeItem = null, Action<VisualElement, int> bindItem = null)
		: this()
	{
		if (Math.Abs(itemHeight - -1f) > float.Epsilon)
		{
			m_FixedItemHeight = itemHeight;
			m_ItemHeightIsInline = true;
		}
		this.itemsSource = itemsSource;
	}

	public VisualElement GetRootElementForId(int id)
	{
		return activeItems.FirstOrDefault((ReusableCollectionItem t) => t.id == id)?.rootElement;
	}

	public VisualElement GetRootElementForIndex(int index)
	{
		return GetRootElementForId(viewController.GetIdForIndex(index));
	}

	internal virtual bool HasValidDataAndBindings()
	{
		return m_ViewController != null && itemsSource != null;
	}

	private void OnItemIndexChanged(int srcIndex, int dstIndex)
	{
		this.itemIndexChanged?.Invoke(srcIndex, dstIndex);
		RefreshItems();
	}

	private void OnItemsSourceChanged()
	{
		this.itemsSourceChanged?.Invoke();
	}

	public void RefreshItem(int index)
	{
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			int index2 = activeItem.index;
			if (index2 == index)
			{
				viewController.InvokeUnbindItem(activeItem, index2);
				viewController.InvokeBindItem(activeItem, index2);
				break;
			}
		}
	}

	public void RefreshItems()
	{
		using (k_RefreshMarker.Auto())
		{
			if (m_ViewController != null)
			{
				IVisualElementScheduledItem rebuildScheduled = m_RebuildScheduled;
				if (rebuildScheduled != null && rebuildScheduled.isActive)
				{
					Rebuild();
					return;
				}
				RefreshSelection();
				virtualizationController.Refresh(rebuild: false);
				PostRefresh();
			}
		}
	}

	[Obsolete("Refresh() has been deprecated. Use Rebuild() instead. (UnityUpgradable) -> Rebuild()", false)]
	public void Refresh()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		using (k_RebuildMarker.Auto())
		{
			if (m_ViewController != null)
			{
				RefreshSelection();
				virtualizationController.Refresh(rebuild: true);
				PostRefresh();
				m_RebuildScheduled?.Pause();
			}
		}
	}

	internal void ScheduleRebuild()
	{
		if (m_RebuildScheduled == null)
		{
			m_RebuildScheduled = base.schedule.Execute(Rebuild);
		}
		else if (!m_RebuildScheduled.isActive)
		{
			m_RebuildScheduled.Resume();
		}
	}

	private void RefreshSelection()
	{
		bool selectedIndicesChanged = false;
		int previousSelectionCount = m_Selection.indexCount;
		m_Selection.items.Clear();
		if (viewController?.itemsSource == null)
		{
			m_Selection.ClearIndices();
			NotifyIfChanged();
			return;
		}
		if (m_Selection.idCount > 0)
		{
			List<int> value;
			using (CollectionPool<List<int>, int>.Get(out value))
			{
				foreach (int selectedId in m_Selection.selectedIds)
				{
					int indexForId = viewController.GetIndexForId(selectedId);
					if (indexForId < 0)
					{
						selectedIndicesChanged = true;
						continue;
					}
					if (!m_Selection.ContainsIndex(indexForId))
					{
						selectedIndicesChanged = true;
					}
					value.Add(indexForId);
				}
				m_Selection.ClearIndices();
				foreach (int item in value)
				{
					m_Selection.AddIndex(item, viewController.GetItemForIndex(item));
				}
			}
		}
		NotifyIfChanged();
		void NotifyIfChanged()
		{
			if (selectedIndicesChanged || m_Selection.indexCount != previousSelectionCount)
			{
				NotifyOfSelectionChange();
			}
		}
	}

	private protected virtual void PostRefresh()
	{
		if (HasValidDataAndBindings())
		{
			m_LastHeight = m_ScrollView.layout.height;
			if (!float.IsNaN(m_ScrollView.layout.height))
			{
				Resize(m_ScrollView.layout.size);
			}
		}
	}

	public void ScrollTo(VisualElement visualElement)
	{
		m_ScrollView.ScrollTo(visualElement);
	}

	public void ScrollToItem(int index)
	{
		if (HasValidDataAndBindings())
		{
			virtualizationController.ScrollToItem(index);
		}
	}

	[Obsolete("ScrollToId() has been deprecated. Use ScrollToItemById() instead. (UnityUpgradable) -> ScrollToItemById(*)", false)]
	public void ScrollToId(int id)
	{
		ScrollToItemById(id);
	}

	public void ScrollToItemById(int id)
	{
		if (HasValidDataAndBindings())
		{
			int indexForId = viewController.GetIndexForId(id);
			virtualizationController.ScrollToItem(indexForId);
		}
	}

	private void OnScroll(Vector2 offset)
	{
		if (HasValidDataAndBindings())
		{
			virtualizationController.OnScroll(offset);
		}
	}

	private void Resize(Vector2 size)
	{
		virtualizationController.Resize(size);
		m_LastHeight = size.y;
		virtualizationController.UpdateBackground();
	}

	private void OnAttachToPanel(AttachToPanelEvent evt)
	{
		if (evt.destinationPanel != null)
		{
			m_ScrollView.contentContainer.AddManipulator(m_NavigationManipulator = new KeyboardNavigationManipulator(Apply));
			m_ScrollView.contentContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
			m_ScrollView.contentContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
			m_ScrollView.contentContainer.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
			m_ScrollView.contentContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
		}
	}

	private void OnDetachFromPanel(DetachFromPanelEvent evt)
	{
		if (evt.originPanel != null)
		{
			m_ScrollView.contentContainer.RemoveManipulator(m_NavigationManipulator);
			m_ScrollView.contentContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
			m_ScrollView.contentContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			m_ScrollView.contentContainer.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
			m_ScrollView.contentContainer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
		}
	}

	[Obsolete("OnKeyDown is obsolete and will be removed from ListView. Use the event system instead, i.e. SendEvent(EventBase e).", true)]
	public void OnKeyDown(KeyDownEvent evt)
	{
		m_NavigationManipulator.OnKeyDown(evt);
	}

	private bool Apply(KeyboardNavigationOperation op, bool shiftKey, bool altKey)
	{
		if (selectionType == SelectionType.None || !HasValidDataAndBindings())
		{
			return false;
		}
		switch (op)
		{
		case KeyboardNavigationOperation.SelectAll:
			SelectAll();
			return true;
		case KeyboardNavigationOperation.Cancel:
			ClearSelection();
			return true;
		case KeyboardNavigationOperation.Submit:
			this.itemsChosen?.Invoke(selectedItems);
			ScrollToItem(selectedIndex);
			return true;
		case KeyboardNavigationOperation.Previous:
			if (selectedIndex > 0)
			{
				HandleSelectionAndScroll(selectedIndex - 1);
				return true;
			}
			break;
		case KeyboardNavigationOperation.Next:
			if (selectedIndex + 1 < m_ViewController.itemsSource.Count)
			{
				HandleSelectionAndScroll(selectedIndex + 1);
				return true;
			}
			break;
		case KeyboardNavigationOperation.Begin:
			HandleSelectionAndScroll(0);
			return true;
		case KeyboardNavigationOperation.End:
			HandleSelectionAndScroll(m_ViewController.itemsSource.Count - 1);
			return true;
		case KeyboardNavigationOperation.PageDown:
			if (m_Selection.indexCount > 0)
			{
				int num2 = (m_IsRangeSelectionDirectionUp ? m_Selection.minIndex : m_Selection.maxIndex);
				HandleSelectionAndScroll(Mathf.Min(viewController.itemsSource.Count - 1, num2 + (virtualizationController.visibleItemCount - 1)));
			}
			return true;
		case KeyboardNavigationOperation.PageUp:
			if (m_Selection.indexCount > 0)
			{
				int num = (m_IsRangeSelectionDirectionUp ? m_Selection.minIndex : m_Selection.maxIndex);
				HandleSelectionAndScroll(Mathf.Max(0, num - (virtualizationController.visibleItemCount - 1)));
			}
			return true;
		case KeyboardNavigationOperation.MoveRight:
			if (m_Selection.indexCount > 0)
			{
				return HandleItemNavigation(moveIn: true, altKey);
			}
			break;
		case KeyboardNavigationOperation.MoveLeft:
			if (m_Selection.indexCount > 0)
			{
				return HandleItemNavigation(moveIn: false, altKey);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("op", op, null);
		}
		return false;
		void HandleSelectionAndScroll(int index)
		{
			if (index >= 0 && index < m_ViewController.itemsSource.Count)
			{
				if (selectionType == SelectionType.Multiple && shiftKey && m_Selection.indexCount != 0)
				{
					DoRangeSelection(index);
				}
				else
				{
					selectedIndex = index;
				}
				ScrollToItem(index);
			}
		}
	}

	private void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
	{
		bool flag = ((sourceEvent is KeyDownEvent { shiftKey: not false } || sourceEvent is INavigationEvent { shiftKey: not false }) ? true : false);
		bool shiftKey = flag;
		bool flag2 = ((sourceEvent is KeyDownEvent { altKey: not false } || sourceEvent is INavigationEvent { altKey: not false }) ? true : false);
		bool altKey = flag2;
		if (Apply(op, shiftKey, altKey))
		{
			sourceEvent.StopPropagation();
			sourceEvent.PreventDefault();
		}
	}

	private protected virtual bool HandleItemNavigation(bool moveIn, bool altKey)
	{
		return false;
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		if (evt.button == 0)
		{
			if ((evt.pressedButtons & 1) == 0)
			{
				ProcessPointerUp(evt);
			}
			else
			{
				ProcessPointerDown(evt);
			}
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (evt.pointerType != PointerType.mouse)
		{
			ProcessPointerDown(evt);
			base.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		else
		{
			ProcessPointerDown(evt);
		}
	}

	private void OnPointerCancel(PointerCancelEvent evt)
	{
		if (HasValidDataAndBindings() && evt.isPrimary)
		{
			ClearSelection();
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (evt.pointerType != PointerType.mouse)
		{
			ProcessPointerUp(evt);
			base.panel.PreventCompatibilityMouseEvents(evt.pointerId);
		}
		else
		{
			ProcessPointerUp(evt);
		}
	}

	private void ProcessPointerDown(IPointerEvent evt)
	{
		if (HasValidDataAndBindings() && evt.isPrimary && evt.button == 0)
		{
			if (evt.pointerType != PointerType.mouse)
			{
				m_TouchDownPosition = evt.position;
				long num = (evt as PointerDownEvent)?.timestamp ?? 0;
				m_PointerDownCount = ((num - m_LastPointerDownTimeStamp >= Event.GetDoubleClickTime()) ? 1 : (m_PointerDownCount + 1));
				m_LastPointerDownTimeStamp = num;
			}
			else
			{
				m_PointerDownCount = evt.clickCount;
				DoSelect(evt.localPosition, m_PointerDownCount, evt.actionKey, evt.shiftKey);
			}
		}
	}

	private void ProcessPointerUp(IPointerEvent evt)
	{
		if (!HasValidDataAndBindings() || !evt.isPrimary || evt.button != 0)
		{
			return;
		}
		if (evt.pointerType != PointerType.mouse)
		{
			if ((evt.position - m_TouchDownPosition).sqrMagnitude <= 100f)
			{
				DoSelect(evt.localPosition, m_PointerDownCount, evt.actionKey, evt.shiftKey);
			}
			long num = (evt as PointerUpEvent)?.timestamp ?? 0;
			m_PointerDownCount = ((num - m_LastPointerDownTimeStamp < Event.GetDoubleClickTime()) ? m_PointerDownCount : 0);
		}
		else
		{
			int indexFromPosition = virtualizationController.GetIndexFromPosition(evt.localPosition);
			if (selectionType == SelectionType.Multiple && !evt.shiftKey && !evt.actionKey && m_Selection.indexCount > 1 && m_Selection.ContainsIndex(indexFromPosition))
			{
				ProcessSingleClick(indexFromPosition);
			}
		}
	}

	private void DoSelect(Vector2 localPosition, int clickCount, bool actionKey, bool shiftKey)
	{
		int indexFromPosition = virtualizationController.GetIndexFromPosition(localPosition);
		int num = ((m_Selection.indexCount > 0 && m_Selection.FirstIndex() != indexFromPosition) ? 1 : ((clickCount > 2) ? 2 : clickCount));
		if (indexFromPosition > viewController.itemsSource.Count - 1 || selectionType == SelectionType.None)
		{
			return;
		}
		int idForIndex = viewController.GetIdForIndex(indexFromPosition);
		switch (num)
		{
		case 1:
			if (selectionType == SelectionType.Multiple && actionKey)
			{
				if (m_Selection.ContainsId(idForIndex))
				{
					RemoveFromSelection(indexFromPosition);
				}
				else
				{
					AddToSelection(indexFromPosition);
				}
				break;
			}
			if (selectionType == SelectionType.Multiple && shiftKey)
			{
				if (m_Selection.indexCount == 0)
				{
					SetSelection(indexFromPosition);
				}
				else
				{
					DoRangeSelection(indexFromPosition);
				}
				break;
			}
			if (selectionType == SelectionType.Multiple && m_Selection.ContainsIndex(indexFromPosition))
			{
				this.selectionNotChanged?.Invoke();
				break;
			}
			if (selectionType == SelectionType.Single && m_Selection.ContainsIndex(indexFromPosition))
			{
				this.selectionNotChanged?.Invoke();
			}
			SetSelection(indexFromPosition);
			break;
		case 2:
		{
			if (this.itemsChosen == null)
			{
				break;
			}
			bool flag = false;
			foreach (int selectedIndex in selectedIndices)
			{
				if (indexFromPosition == selectedIndex)
				{
					flag = true;
					break;
				}
			}
			ProcessSingleClick(indexFromPosition);
			if (flag)
			{
				this.itemsChosen?.Invoke(selectedItems);
			}
			break;
		}
		}
	}

	internal void DoRangeSelection(int rangeSelectionFinalIndex)
	{
		int num = (m_IsRangeSelectionDirectionUp ? m_Selection.maxIndex : m_Selection.minIndex);
		ClearSelectionWithoutValidation();
		List<int> list = new List<int>();
		m_IsRangeSelectionDirectionUp = rangeSelectionFinalIndex < num;
		if (m_IsRangeSelectionDirectionUp)
		{
			for (int i = rangeSelectionFinalIndex; i <= num; i++)
			{
				list.Add(i);
			}
		}
		else
		{
			for (int num2 = rangeSelectionFinalIndex; num2 >= num; num2--)
			{
				list.Add(num2);
			}
		}
		AddToSelection(list);
	}

	private void ProcessSingleClick(int clickedIndex)
	{
		SetSelection(clickedIndex);
	}

	internal void SelectAll()
	{
		if (!HasValidDataAndBindings() || selectionType != SelectionType.Multiple)
		{
			return;
		}
		for (int i = 0; i < m_ViewController.itemsSource.Count; i++)
		{
			int idForIndex = viewController.GetIdForIndex(i);
			object itemForIndex = viewController.GetItemForIndex(i);
			foreach (ReusableCollectionItem activeItem in activeItems)
			{
				if (activeItem.id == idForIndex)
				{
					activeItem.SetSelected(selected: true);
				}
			}
			if (!m_Selection.ContainsId(idForIndex))
			{
				m_Selection.AddId(idForIndex);
				m_Selection.AddIndex(i, itemForIndex);
			}
		}
		NotifyOfSelectionChange();
		SaveViewData();
	}

	public void AddToSelection(int index)
	{
		AddToSelection(new int[1] { index });
	}

	internal void AddToSelection(IList<int> indexes)
	{
		if (!HasValidDataAndBindings() || indexes == null || indexes.Count == 0)
		{
			return;
		}
		foreach (int index in indexes)
		{
			AddToSelectionWithoutValidation(index);
		}
		NotifyOfSelectionChange();
		SaveViewData();
	}

	private void AddToSelectionWithoutValidation(int index)
	{
		if (m_Selection.ContainsIndex(index))
		{
			return;
		}
		int idForIndex = viewController.GetIdForIndex(index);
		object itemForIndex = viewController.GetItemForIndex(index);
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			if (activeItem.id == idForIndex)
			{
				activeItem.SetSelected(selected: true);
			}
		}
		m_Selection.AddId(idForIndex);
		m_Selection.AddIndex(index, itemForIndex);
	}

	public void RemoveFromSelection(int index)
	{
		if (HasValidDataAndBindings())
		{
			RemoveFromSelectionWithoutValidation(index);
			NotifyOfSelectionChange();
			SaveViewData();
		}
	}

	private void RemoveFromSelectionWithoutValidation(int index)
	{
		if (!m_Selection.TryRemove(index))
		{
			return;
		}
		int idForIndex = viewController.GetIdForIndex(index);
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			if (activeItem.id == idForIndex)
			{
				activeItem.SetSelected(selected: false);
			}
		}
		m_Selection.RemoveId(idForIndex);
	}

	public void SetSelection(int index)
	{
		if (index < 0)
		{
			ClearSelection();
			return;
		}
		SetSelection(new int[1] { index });
	}

	public void SetSelection(IEnumerable<int> indices)
	{
		SetSelectionInternal(indices, sendNotification: true);
	}

	public void SetSelectionWithoutNotify(IEnumerable<int> indices)
	{
		SetSelectionInternal(indices, sendNotification: false);
	}

	internal void SetSelectionInternal(IEnumerable<int> indices, bool sendNotification)
	{
		if (!HasValidDataAndBindings() || indices == null || MatchesExistingSelection(indices))
		{
			return;
		}
		ClearSelectionWithoutValidation();
		if (indices is ICollection collection && m_Selection.capacity < collection.Count)
		{
			m_Selection.capacity = collection.Count;
		}
		foreach (int index in indices)
		{
			AddToSelectionWithoutValidation(index);
		}
		if (sendNotification)
		{
			NotifyOfSelectionChange();
		}
		SaveViewData();
	}

	private bool MatchesExistingSelection(IEnumerable<int> indices)
	{
		IList<int> list = indices as IList<int>;
		List<int> list2 = null;
		try
		{
			if (list == null)
			{
				list2 = CollectionPool<List<int>, int>.Get();
				list2.AddRange(indices);
				list = list2;
			}
			if (list.Count != m_Selection.indexCount)
			{
				return false;
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != m_Selection.indices[i])
				{
					return false;
				}
			}
			return true;
		}
		finally
		{
			if (list2 != null)
			{
				CollectionPool<List<int>, int>.Release(list2);
			}
		}
	}

	private void NotifyOfSelectionChange()
	{
		if (HasValidDataAndBindings())
		{
			this.selectionChanged?.Invoke(selectedItems);
			this.selectedIndicesChanged?.Invoke(m_Selection.indices);
		}
	}

	public void ClearSelection()
	{
		if (HasValidDataAndBindings() && m_Selection.idCount != 0)
		{
			ClearSelectionWithoutValidation();
			NotifyOfSelectionChange();
		}
	}

	private void ClearSelectionWithoutValidation()
	{
		foreach (ReusableCollectionItem activeItem in activeItems)
		{
			activeItem.SetSelected(selected: false);
		}
		m_Selection.Clear();
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		m_ScrollView.UpdateContentViewTransform();
	}

	[EventInterest(new Type[]
	{
		typeof(PointerUpEvent),
		typeof(FocusEvent),
		typeof(NavigationSubmitEvent),
		typeof(BlurEvent)
	})]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt.eventTypeId == EventBase<PointerUpEvent>.TypeId())
		{
			m_Dragger?.OnPointerUpEvent((PointerUpEvent)evt);
		}
		else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
		{
			m_VirtualizationController?.OnFocus(evt.leafTarget as VisualElement);
		}
		else if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			m_VirtualizationController?.OnBlur((evt as BlurEvent)?.relatedTarget as VisualElement);
		}
		else if (evt.eventTypeId == EventBase<NavigationSubmitEvent>.TypeId() && evt.target == this)
		{
			m_ScrollView.contentContainer.Focus();
		}
	}

	private void OnSizeChanged(GeometryChangedEvent evt)
	{
		if (HasValidDataAndBindings() && (!Mathf.Approximately(evt.newRect.width, evt.oldRect.width) || !Mathf.Approximately(evt.newRect.height, evt.oldRect.height)))
		{
			Resize(evt.newRect.size);
		}
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent e)
	{
		if (!m_ItemHeightIsInline && e.customStyle.TryGetValue(s_ItemHeightProperty, out var value) && Math.Abs(m_FixedItemHeight - (float)value) > float.Epsilon)
		{
			m_FixedItemHeight = value;
			RefreshItems();
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_Selection.selectedIds = m_SelectedIds;
		RefreshItems();
	}
}
