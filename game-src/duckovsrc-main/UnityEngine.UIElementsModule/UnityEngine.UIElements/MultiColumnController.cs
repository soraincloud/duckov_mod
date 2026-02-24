using System;
using System.Collections.Generic;
using UnityEngine.UIElements.Internal;

namespace UnityEngine.UIElements;

public class MultiColumnController : IDisposable
{
	private static readonly PropertyName k_BoundColumnVePropertyName = "__unity-multi-column-bound-column";

	internal static readonly PropertyName bindableElementPropertyName = "__unity-multi-column-bindable-element";

	internal static readonly string baseUssClassName = "unity-multi-column-view";

	private static readonly string k_HeaderContainerViewDataKey = "unity-multi-column-header-container";

	public static readonly string headerContainerUssClassName = baseUssClassName + "__header-container";

	public static readonly string rowContainerUssClassName = baseUssClassName + "__row-container";

	public static readonly string cellUssClassName = baseUssClassName + "__cell";

	public static readonly string cellLabelUssClassName = cellUssClassName + "__label";

	private static readonly string k_HeaderViewDataKey = "Header";

	private BaseVerticalCollectionView m_View;

	private VisualElement m_HeaderContainer;

	private MultiColumnCollectionHeader m_MultiColumnHeader;

	internal MultiColumnCollectionHeader header => m_MultiColumnHeader;

	public event Action columnSortingChanged;

	public event Action<ContextualMenuPopulateEvent, Column> headerContextMenuPopulateEvent;

	public MultiColumnController(Columns columns, SortColumnDescriptions sortDescriptions, List<SortColumnDescription> sortedColumns)
	{
		m_MultiColumnHeader = new MultiColumnCollectionHeader(columns, sortDescriptions, sortedColumns)
		{
			viewDataKey = k_HeaderViewDataKey
		};
		m_MultiColumnHeader.columnSortingChanged += OnColumnSortingChanged;
		m_MultiColumnHeader.contextMenuPopulateEvent += OnContextMenuPopulateEvent;
		m_MultiColumnHeader.columnResized += OnColumnResized;
		m_MultiColumnHeader.viewDataRestored += OnViewDataRestored;
		m_MultiColumnHeader.columns.columnAdded += OnColumnAdded;
		m_MultiColumnHeader.columns.columnRemoved += OnColumnRemoved;
		m_MultiColumnHeader.columns.columnReordered += OnColumnReordered;
		m_MultiColumnHeader.columns.columnChanged += OnColumnsChanged;
		m_MultiColumnHeader.columns.changed += OnColumnChanged;
	}

	private static void BindCellItem<T>(VisualElement ve, int rowIndex, Column column, T item)
	{
		if (column.bindCell != null)
		{
			column.bindCell(ve, rowIndex);
		}
		else
		{
			DefaultBindCellItem(ve, column, item);
		}
	}

	private static void UnbindCellItem(VisualElement ve, int rowIndex, Column column)
	{
		column.unbindCell?.Invoke(ve, rowIndex);
	}

	private static VisualElement DefaultMakeCellItem()
	{
		Label label = new Label();
		label.AddToClassList(cellLabelUssClassName);
		return label;
	}

	private static void DefaultBindCellItem<T>(VisualElement ve, Column column, T item)
	{
		if (ve is Label label)
		{
			label.text = item.ToString();
		}
	}

	public VisualElement MakeItem()
	{
		VisualElement visualElement = new VisualElement
		{
			name = rowContainerUssClassName
		};
		visualElement.AddToClassList(rowContainerUssClassName);
		foreach (Column visible in m_MultiColumnHeader.columns.visibleList)
		{
			VisualElement visualElement2 = new VisualElement();
			visualElement2.AddToClassList(cellUssClassName);
			VisualElement visualElement3 = visible.makeCell?.Invoke() ?? DefaultMakeCellItem();
			visualElement2.SetProperty(bindableElementPropertyName, visualElement3);
			visualElement2.Add(visualElement3);
			visualElement.Add(visualElement2);
		}
		return visualElement;
	}

	public void BindItem<T>(VisualElement element, int index, T item)
	{
		int num = 0;
		foreach (Column visible in m_MultiColumnHeader.columns.visibleList)
		{
			if (m_MultiColumnHeader.columnDataMap.TryGetValue(visible, out var value))
			{
				VisualElement visualElement = element[num++];
				VisualElement ve = visualElement.GetProperty(bindableElementPropertyName) as VisualElement;
				BindCellItem(ve, index, visible, item);
				visualElement.style.width = value.control.resolvedStyle.width;
				visualElement.SetProperty(k_BoundColumnVePropertyName, visible);
			}
		}
	}

	public void UnbindItem(VisualElement element, int index)
	{
		foreach (VisualElement item in element.Children())
		{
			if (item.GetProperty(k_BoundColumnVePropertyName) is Column column)
			{
				VisualElement ve = item.GetProperty(bindableElementPropertyName) as VisualElement;
				UnbindCellItem(ve, index, column);
			}
		}
	}

	public void DestroyItem(VisualElement element)
	{
		foreach (VisualElement item in element.Children())
		{
			if (item.GetProperty(k_BoundColumnVePropertyName) is Column column)
			{
				VisualElement obj = item.GetProperty(bindableElementPropertyName) as VisualElement;
				column.destroyCell?.Invoke(obj);
				item.SetProperty(k_BoundColumnVePropertyName, null);
			}
		}
	}

	public void PrepareView(BaseVerticalCollectionView collectionView)
	{
		if (m_View != null)
		{
			Debug.LogWarning("Trying to initialize multi column view more than once. This shouldn't happen.");
			return;
		}
		m_View = collectionView;
		m_HeaderContainer = new VisualElement
		{
			name = headerContainerUssClassName
		};
		m_HeaderContainer.AddToClassList(headerContainerUssClassName);
		m_HeaderContainer.viewDataKey = k_HeaderContainerViewDataKey;
		collectionView.scrollView.hierarchy.Insert(0, m_HeaderContainer);
		m_HeaderContainer.Add(m_MultiColumnHeader);
		m_View.scrollView.horizontalScroller.valueChanged += OnHorizontalScrollerValueChanged;
		m_View.scrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
		m_MultiColumnHeader.columnContainer.RegisterCallback<GeometryChangedEvent>(OnColumnContainerGeometryChanged);
	}

	public void Dispose()
	{
		if (m_View != null)
		{
			m_View.scrollView.horizontalScroller.valueChanged -= OnHorizontalScrollerValueChanged;
			m_View.scrollView.contentViewport.UnregisterCallback<GeometryChangedEvent>(OnViewportGeometryChanged);
			m_View = null;
		}
		m_MultiColumnHeader.columnContainer.UnregisterCallback<GeometryChangedEvent>(OnColumnContainerGeometryChanged);
		m_MultiColumnHeader.columnSortingChanged -= OnColumnSortingChanged;
		m_MultiColumnHeader.contextMenuPopulateEvent -= OnContextMenuPopulateEvent;
		m_MultiColumnHeader.columnResized -= OnColumnResized;
		m_MultiColumnHeader.viewDataRestored -= OnViewDataRestored;
		m_MultiColumnHeader.columns.columnAdded -= OnColumnAdded;
		m_MultiColumnHeader.columns.columnRemoved -= OnColumnRemoved;
		m_MultiColumnHeader.columns.columnReordered -= OnColumnReordered;
		m_MultiColumnHeader.columns.columnChanged -= OnColumnsChanged;
		m_MultiColumnHeader.columns.changed -= OnColumnChanged;
		m_MultiColumnHeader.RemoveFromHierarchy();
		m_MultiColumnHeader.Dispose();
		m_MultiColumnHeader = null;
		m_HeaderContainer.RemoveFromHierarchy();
		m_HeaderContainer = null;
	}

	private void OnHorizontalScrollerValueChanged(float v)
	{
		m_MultiColumnHeader.ScrollHorizontally(v);
	}

	private void OnViewportGeometryChanged(GeometryChangedEvent evt)
	{
		float num = m_MultiColumnHeader.resolvedStyle.paddingLeft + m_MultiColumnHeader.resolvedStyle.paddingRight;
		m_MultiColumnHeader.style.maxWidth = evt.newRect.width - num;
		m_MultiColumnHeader.style.maxWidth = evt.newRect.width - num;
		UpdateContentContainer(m_View);
	}

	private void OnColumnContainerGeometryChanged(GeometryChangedEvent evt)
	{
		UpdateContentContainer(m_View);
	}

	private void UpdateContentContainer(BaseVerticalCollectionView collectionView)
	{
		float width = m_MultiColumnHeader.columnContainer.layout.width;
		float num = Mathf.Max(width, collectionView.scrollView.contentViewport.resolvedStyle.width);
		collectionView.scrollView.contentContainer.style.width = num;
	}

	private void OnColumnSortingChanged()
	{
		this.columnSortingChanged?.Invoke();
	}

	private void OnContextMenuPopulateEvent(ContextualMenuPopulateEvent evt, Column column)
	{
		this.headerContextMenuPopulateEvent?.Invoke(evt, column);
	}

	private void OnColumnResized(int index, float width)
	{
		foreach (ReusableCollectionItem activeItem in m_View.activeItems)
		{
			activeItem.bindableElement.ElementAt(index).style.width = width;
		}
	}

	private void OnColumnAdded(Column column, int index)
	{
		m_View.Rebuild();
	}

	private void OnColumnRemoved(Column column)
	{
		m_View.Rebuild();
	}

	private void OnColumnReordered(Column column, int from, int to)
	{
		if (!m_MultiColumnHeader.isApplyingViewState)
		{
			m_View.Rebuild();
		}
	}

	private void OnColumnsChanged(Column column, ColumnDataType type)
	{
		if (!m_MultiColumnHeader.isApplyingViewState && type == ColumnDataType.Visibility)
		{
			m_View.ScheduleRebuild();
		}
	}

	private void OnColumnChanged(ColumnsDataType type)
	{
		if (!m_MultiColumnHeader.isApplyingViewState && type == ColumnsDataType.PrimaryColumn)
		{
			m_View.ScheduleRebuild();
		}
	}

	private void OnViewDataRestored()
	{
		m_View.Rebuild();
	}
}
