using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace UnityEngine.UIElements.Internal;

internal class MultiColumnCollectionHeader : VisualElement, IDisposable
{
	[Serializable]
	private class ViewState
	{
		[Serializable]
		private struct ColumnState
		{
			public int index;

			public string name;

			public float actualWidth;

			public Length width;

			public bool visible;
		}

		[SerializeField]
		private bool m_HasPersistedData;

		[SerializeField]
		private List<SortColumnDescription> m_SortDescriptions = new List<SortColumnDescription>();

		[SerializeField]
		private List<ColumnState> m_OrderedColumnStates = new List<ColumnState>();

		internal void Save(MultiColumnCollectionHeader header)
		{
			m_SortDescriptions.Clear();
			m_OrderedColumnStates.Clear();
			foreach (SortColumnDescription sortDescription in header.sortDescriptions)
			{
				m_SortDescriptions.Add(sortDescription);
			}
			foreach (Column display in header.columns.displayList)
			{
				ColumnState item = new ColumnState
				{
					index = display.index,
					name = display.name,
					actualWidth = display.desiredWidth,
					width = display.width,
					visible = display.visible
				};
				m_OrderedColumnStates.Add(item);
			}
			m_HasPersistedData = true;
		}

		internal void Apply(MultiColumnCollectionHeader header)
		{
			if (!m_HasPersistedData)
			{
				return;
			}
			int num = Math.Min(m_OrderedColumnStates.Count, header.columns.Count);
			int num2 = 0;
			for (int i = 0; i < m_OrderedColumnStates.Count && num2 < num; i++)
			{
				ColumnState columnState = m_OrderedColumnStates[i];
				Column column = null;
				if (!string.IsNullOrEmpty(columnState.name))
				{
					if (header.columns.Contains(columnState.name))
					{
						column = header.columns[columnState.name];
					}
				}
				else
				{
					if (columnState.index > header.columns.Count - 1)
					{
						continue;
					}
					column = header.columns[columnState.index];
					if (!string.IsNullOrEmpty(column.name))
					{
						column = null;
					}
				}
				if (column != null)
				{
					header.columns.ReorderDisplay(column.displayIndex, num2++);
					column.visible = columnState.visible;
					column.width = columnState.width;
					column.desiredWidth = columnState.actualWidth;
				}
			}
			header.sortDescriptions.Clear();
			foreach (SortColumnDescription sortDescription in m_SortDescriptions)
			{
				header.sortDescriptions.Add(sortDescription);
			}
		}
	}

	internal class ColumnData
	{
		public MultiColumnHeaderColumn control { get; set; }

		public MultiColumnHeaderColumnResizeHandle resizeHandle { get; set; }
	}

	private struct SortedColumnState
	{
		public SortColumnDescription columnDesc;

		public SortDirection direction;

		public SortedColumnState(SortColumnDescription desc, SortDirection dir)
		{
			columnDesc = desc;
			direction = dir;
		}
	}

	private const int kMaxStableLayoutPassCount = 2;

	public static readonly string ussClassName = "unity-multi-column-header";

	public static readonly string columnContainerUssClassName = ussClassName + "__column-container";

	public static readonly string handleContainerUssClassName = ussClassName + "__resize-handle-container";

	public static readonly string reorderableUssClassName = ussClassName + "__header";

	private bool m_SortingEnabled;

	private List<SortColumnDescription> m_SortedColumns;

	private SortColumnDescriptions m_SortDescriptions;

	private List<SortedColumnState> m_OldSortedColumnStates = new List<SortedColumnState>();

	private bool m_SortingUpdatesTemporarilyDisabled;

	private ViewState m_ViewState;

	private bool m_ApplyingViewState;

	private bool m_DoLayoutScheduled;

	internal bool isApplyingViewState => m_ApplyingViewState;

	public Dictionary<Column, ColumnData> columnDataMap { get; } = new Dictionary<Column, ColumnData>();

	public ColumnLayout columnLayout { get; }

	public VisualElement columnContainer { get; }

	public VisualElement resizeHandleContainer { get; }

	public IEnumerable<SortColumnDescription> sortedColumns => m_SortedColumns;

	public SortColumnDescriptions sortDescriptions
	{
		get
		{
			return m_SortDescriptions;
		}
		protected internal set
		{
			m_SortDescriptions = value;
			m_SortDescriptions.changed += UpdateSortedColumns;
			UpdateSortedColumns();
		}
	}

	public Columns columns { get; }

	public bool sortingEnabled
	{
		get
		{
			return m_SortingEnabled;
		}
		set
		{
			if (m_SortingEnabled != value)
			{
				m_SortingEnabled = value;
				UpdateSortingStatus();
				UpdateSortedColumns();
			}
		}
	}

	public event Action<int, float> columnResized;

	public event Action columnSortingChanged;

	public event Action<ContextualMenuPopulateEvent, Column> contextMenuPopulateEvent;

	internal event Action viewDataRestored;

	public MultiColumnCollectionHeader()
		: this(new Columns(), new SortColumnDescriptions(), new List<SortColumnDescription>())
	{
	}

	public MultiColumnCollectionHeader(Columns columns, SortColumnDescriptions sortDescriptions, List<SortColumnDescription> sortedColumns)
	{
		AddToClassList(ussClassName);
		this.columns = columns;
		m_SortedColumns = sortedColumns;
		this.sortDescriptions = sortDescriptions;
		columnContainer = new VisualElement
		{
			pickingMode = PickingMode.Ignore
		};
		columnContainer.AddToClassList(columnContainerUssClassName);
		Add(columnContainer);
		resizeHandleContainer = new VisualElement
		{
			pickingMode = PickingMode.Ignore
		};
		resizeHandleContainer.AddToClassList(handleContainerUssClassName);
		resizeHandleContainer.StretchToParentSize();
		Add(resizeHandleContainer);
		columnLayout = new ColumnLayout(columns);
		columnLayout.layoutRequested += ScheduleDoLayout;
		foreach (Column visible in columns.visibleList)
		{
			OnColumnAdded(visible);
		}
		this.columns.columnAdded += OnColumnAdded;
		this.columns.columnRemoved += OnColumnRemoved;
		this.columns.columnChanged += OnColumnChanged;
		this.columns.columnReordered += OnColumnReordered;
		this.columns.columnResized += OnColumnResized;
		this.AddManipulator(new ContextualMenuManipulator(OnContextualMenuManipulator));
		RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
	}

	private void ScheduleDoLayout()
	{
		if (!m_DoLayoutScheduled)
		{
			base.schedule.Execute(DoLayout);
			m_DoLayoutScheduled = true;
		}
	}

	private void ResizeToFit()
	{
		columnLayout.ResizeToFit(base.layout.width);
	}

	private void UpdateSortedColumns()
	{
		if (m_SortingUpdatesTemporarilyDisabled)
		{
			return;
		}
		List<SortedColumnState> value;
		using (CollectionPool<List<SortedColumnState>, SortedColumnState>.Get(out value))
		{
			if (sortingEnabled)
			{
				foreach (SortColumnDescription sortDescription in sortDescriptions)
				{
					Column column = null;
					if (sortDescription.columnIndex != -1)
					{
						column = columns[sortDescription.columnIndex];
					}
					else if (!string.IsNullOrEmpty(sortDescription.columnName))
					{
						column = columns[sortDescription.columnName];
					}
					if (column != null && column.sortable)
					{
						sortDescription.column = column;
						value.Add(new SortedColumnState(sortDescription, sortDescription.direction));
					}
					else
					{
						sortDescription.column = null;
					}
				}
			}
			if (m_OldSortedColumnStates.SequenceEqual(value))
			{
				return;
			}
			m_SortedColumns.Clear();
			foreach (SortedColumnState item in value)
			{
				m_SortedColumns.Add(item.columnDesc);
			}
			m_OldSortedColumnStates.CopyFrom(value);
		}
		SaveViewState();
		RaiseColumnSortingChanged();
	}

	private void UpdateColumnControls()
	{
		bool flag = false;
		Column key = null;
		foreach (Column visible in columns.visibleList)
		{
			flag |= visible.stretchable;
			ColumnData value = null;
			if (columnDataMap.TryGetValue(visible, out value))
			{
				value.control.style.minWidth = visible.minWidth;
				value.control.style.maxWidth = visible.maxWidth;
				value.resizeHandle.style.display = ((!columns.resizable || !visible.resizable) ? DisplayStyle.None : DisplayStyle.Flex);
			}
			key = visible;
		}
		if (flag)
		{
			columnContainer.style.flexGrow = 1f;
			if (columns.stretchMode == Columns.StretchMode.GrowAndFill && columnDataMap.TryGetValue(key, out var value2))
			{
				value2.resizeHandle.style.display = DisplayStyle.None;
			}
		}
		else
		{
			columnContainer.style.flexGrow = 0f;
		}
		UpdateSortingStatus();
	}

	private void OnColumnAdded(Column column, int index)
	{
		OnColumnAdded(column);
	}

	private void OnColumnAdded(Column column)
	{
		if (!columnDataMap.ContainsKey(column))
		{
			MultiColumnHeaderColumn multiColumnHeaderColumn = new MultiColumnHeaderColumn(column);
			MultiColumnHeaderColumnResizeHandle multiColumnHeaderColumnResizeHandle = new MultiColumnHeaderColumnResizeHandle();
			multiColumnHeaderColumn.RegisterCallback<GeometryChangedEvent>(OnColumnControlGeometryChanged);
			multiColumnHeaderColumn.clickable.clickedWithEventInfo += OnColumnClicked;
			multiColumnHeaderColumn.mover.activeChanged += OnMoveManipulatorActivated;
			multiColumnHeaderColumnResizeHandle.dragArea.AddManipulator(new ColumnResizer(column));
			columnDataMap[column] = new ColumnData
			{
				control = multiColumnHeaderColumn,
				resizeHandle = multiColumnHeaderColumnResizeHandle
			};
			if (column.visible)
			{
				columnContainer.Insert(column.visibleIndex, multiColumnHeaderColumn);
				resizeHandleContainer.Insert(column.visibleIndex, multiColumnHeaderColumnResizeHandle);
			}
			else
			{
				OnColumnRemoved(column);
			}
			UpdateColumnControls();
			SaveViewState();
		}
	}

	private void OnColumnRemoved(Column column)
	{
		if (columnDataMap.TryGetValue(column, out var value))
		{
			CleanupColumnData(value);
			columnDataMap.Remove(column);
			UpdateColumnControls();
			SaveViewState();
		}
	}

	private void OnColumnChanged(Column column, ColumnDataType type)
	{
		if (type == ColumnDataType.Visibility)
		{
			if (column.visible)
			{
				OnColumnAdded(column);
			}
			else
			{
				OnColumnRemoved(column);
			}
			ApplyColumnSorting();
		}
		UpdateColumnControls();
		if (type == ColumnDataType.Visibility)
		{
			SaveViewState();
		}
	}

	private void OnColumnReordered(Column column, int from, int to)
	{
		if (!column.visible || from == to)
		{
			return;
		}
		if (columnDataMap.TryGetValue(column, out var value))
		{
			int num = column.visibleIndex;
			if (num == columns.visibleList.Count() - 1)
			{
				value.control.BringToFront();
			}
			else
			{
				if (to > from)
				{
					num++;
				}
				value.control.PlaceBehind(columnContainer[num]);
				value.resizeHandle.PlaceBehind(resizeHandleContainer[num]);
			}
		}
		UpdateColumnControls();
		SaveViewState();
	}

	private void OnColumnResized(Column column)
	{
		SaveViewState();
	}

	private void OnContextualMenuManipulator(ContextualMenuPopulateEvent evt)
	{
		Column column = null;
		bool flag = columns.visibleList.Count() > 0;
		foreach (Column visible in columns.visibleList)
		{
			if (columns.stretchMode == Columns.StretchMode.GrowAndFill && flag && visible.stretchable)
			{
				flag = false;
			}
			if (column == null && columnDataMap.TryGetValue(visible, out var value) && value.control.layout.Contains(evt.localMousePosition))
			{
				column = visible;
			}
		}
		evt.menu.AppendAction("Resize To Fit", delegate
		{
			ResizeToFit();
		}, flag ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
		evt.menu.AppendSeparator();
		foreach (Column column2 in columns)
		{
			string text = column2.title;
			if (string.IsNullOrEmpty(text))
			{
				text = column2.name;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "Unnamed Column_" + column2.index;
			}
			evt.menu.AppendAction(text, delegate
			{
				column2.visible = !column2.visible;
			}, delegate
			{
				if (!string.IsNullOrEmpty(column2.name) && columns.primaryColumnName == column2.name)
				{
					return DropdownMenuAction.Status.Disabled;
				}
				if (!column2.optional)
				{
					return DropdownMenuAction.Status.Disabled;
				}
				return (!column2.visible) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Checked;
			});
		}
		this.contextMenuPopulateEvent?.Invoke(evt, column);
	}

	private void OnMoveManipulatorActivated(ColumnMover mover)
	{
		resizeHandleContainer.style.display = (mover.active ? DisplayStyle.None : DisplayStyle.Flex);
	}

	private void OnGeometryChanged(GeometryChangedEvent e)
	{
		if (!float.IsNaN(e.newRect.width) && !float.IsNaN(e.newRect.height))
		{
			columnLayout.Dirty();
			if (e.layoutPass > 2)
			{
				ScheduleDoLayout();
			}
			else
			{
				DoLayout();
			}
		}
	}

	private void DoLayout()
	{
		columnLayout.DoLayout(base.layout.width);
		m_DoLayoutScheduled = false;
	}

	private void OnColumnControlGeometryChanged(GeometryChangedEvent evt)
	{
		if (evt.target is MultiColumnHeaderColumn multiColumnHeaderColumn)
		{
			ColumnData columnData = columnDataMap[multiColumnHeaderColumn.column];
			columnData.resizeHandle.style.left = multiColumnHeaderColumn.layout.xMax;
			if (!(Math.Abs(evt.newRect.width - evt.oldRect.width) < float.Epsilon))
			{
				RaiseColumnResized(columnContainer.IndexOf(evt.target as VisualElement));
			}
		}
	}

	private void OnColumnClicked(EventBase evt)
	{
		if (!sortingEnabled || !(evt.currentTarget is MultiColumnHeaderColumn multiColumnHeaderColumn) || !multiColumnHeaderColumn.column.sortable)
		{
			return;
		}
		EventModifiers modifiers;
		if (evt is IPointerEvent pointerEvent)
		{
			modifiers = pointerEvent.modifiers;
		}
		else
		{
			if (!(evt is IMouseEvent mouseEvent))
			{
				return;
			}
			modifiers = mouseEvent.modifiers;
		}
		m_SortingUpdatesTemporarilyDisabled = true;
		try
		{
			UpdateSortColumnDescriptionsOnClick(multiColumnHeaderColumn.column, modifiers);
		}
		finally
		{
			m_SortingUpdatesTemporarilyDisabled = false;
		}
		UpdateSortedColumns();
	}

	private void UpdateSortColumnDescriptionsOnClick(Column column, EventModifiers modifiers)
	{
		SortColumnDescription sortColumnDescription = sortDescriptions.FirstOrDefault((SortColumnDescription d) => d.column == column || (!string.IsNullOrEmpty(column.name) && d.columnName == column.name) || d.columnIndex == column.index);
		if (sortColumnDescription != null)
		{
			if (modifiers == EventModifiers.Shift)
			{
				sortDescriptions.Remove(sortColumnDescription);
				return;
			}
			sortColumnDescription.direction = ((sortColumnDescription.direction == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending);
		}
		else
		{
			sortColumnDescription = (string.IsNullOrEmpty(column.name) ? new SortColumnDescription(column.index, SortDirection.Ascending) : new SortColumnDescription(column.name, SortDirection.Ascending));
		}
		EventModifiers eventModifiers = EventModifiers.Control;
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
		{
			eventModifiers = EventModifiers.Command;
		}
		if (modifiers != eventModifiers)
		{
			sortDescriptions.Clear();
		}
		if (!sortDescriptions.Contains(sortColumnDescription))
		{
			sortDescriptions.Add(sortColumnDescription);
		}
	}

	public void ScrollHorizontally(float horizontalOffset)
	{
		base.transform.position = new Vector3(0f - horizontalOffset, base.transform.position.y, base.transform.position.z);
	}

	private void RaiseColumnResized(int columnIndex)
	{
		this.columnResized?.Invoke(columnIndex, columnContainer[columnIndex].resolvedStyle.width);
	}

	private void RaiseColumnSortingChanged()
	{
		ApplyColumnSorting();
		if (!m_ApplyingViewState)
		{
			this.columnSortingChanged?.Invoke();
		}
	}

	private void ApplyColumnSorting()
	{
		foreach (Column visible in columns.visibleList)
		{
			if (columnDataMap.TryGetValue(visible, out var value))
			{
				value.control.sortOrderLabel = "";
				value.control.RemoveFromClassList(MultiColumnHeaderColumn.sortedAscendingUssClassName);
				value.control.RemoveFromClassList(MultiColumnHeaderColumn.sortedDescendingUssClassName);
			}
		}
		List<ColumnData> list = new List<ColumnData>();
		foreach (SortColumnDescription sortedColumn in sortedColumns)
		{
			if (columnDataMap.TryGetValue(sortedColumn.column, out var value2))
			{
				list.Add(value2);
				if (sortedColumn.direction == SortDirection.Ascending)
				{
					value2.control.AddToClassList(MultiColumnHeaderColumn.sortedAscendingUssClassName);
				}
				else
				{
					value2.control.AddToClassList(MultiColumnHeaderColumn.sortedDescendingUssClassName);
				}
			}
		}
		if (list.Count > 1)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].control.sortOrderLabel = (i + 1).ToString();
			}
		}
	}

	private void UpdateSortingStatus()
	{
		bool flag = false;
		foreach (Column visible in columns.visibleList)
		{
			if (columnDataMap.TryGetValue(visible, out var _) && sortingEnabled && visible.sortable)
			{
				flag = true;
			}
		}
		foreach (Column visible2 in columns.visibleList)
		{
			if (columnDataMap.TryGetValue(visible2, out var value2))
			{
				if (flag)
				{
					value2.control.AddToClassList(MultiColumnHeaderColumn.sortableUssClassName);
				}
				else
				{
					value2.control.RemoveFromClassList(MultiColumnHeaderColumn.sortableUssClassName);
				}
			}
		}
	}

	internal override void OnViewDataReady()
	{
		try
		{
			m_ApplyingViewState = true;
			base.OnViewDataReady();
			string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
			m_ViewState = GetOrCreateViewData<ViewState>(m_ViewState, fullHierarchicalViewDataKey);
			m_ViewState.Apply(this);
			this.viewDataRestored?.Invoke();
		}
		finally
		{
			m_ApplyingViewState = false;
		}
	}

	private void SaveViewState()
	{
		if (!m_ApplyingViewState)
		{
			m_ViewState?.Save(this);
			SaveViewData();
		}
	}

	private void CleanupColumnData(ColumnData data)
	{
		data.control.UnregisterCallback<GeometryChangedEvent>(OnColumnControlGeometryChanged);
		data.control.clickable.clickedWithEventInfo -= OnColumnClicked;
		data.control.mover.activeChanged -= OnMoveManipulatorActivated;
		data.control.RemoveFromHierarchy();
		data.control.Dispose();
		data.resizeHandle.RemoveFromHierarchy();
	}

	public void Dispose()
	{
		sortDescriptions.changed -= UpdateSortedColumns;
		columnLayout.layoutRequested -= ScheduleDoLayout;
		columns.columnAdded -= OnColumnAdded;
		columns.columnRemoved -= OnColumnRemoved;
		columns.columnChanged -= OnColumnChanged;
		columns.columnReordered -= OnColumnReordered;
		columns.columnResized -= OnColumnResized;
		foreach (ColumnData value in columnDataMap.Values)
		{
			CleanupColumnData(value);
		}
		columnDataMap.Clear();
	}
}
