using System;

namespace UnityEngine.UIElements.Internal;

internal class ColumnMover : PointerManipulator
{
	private const float k_StartDragDistance = 5f;

	private float m_StartPos;

	private float m_LastPos;

	private bool m_Active;

	private bool m_Moving;

	private bool m_Cancelled;

	private MultiColumnCollectionHeader m_Header;

	private VisualElement m_PreviewElement;

	private MultiColumnHeaderColumnMoveLocationPreview m_LocationPreviewElement;

	private Column m_ColumnToMove;

	private float m_ColumnToMovePos;

	private float m_ColumnToMoveWidth;

	private Column m_DestinationColumn;

	private bool m_MoveBeforeDestination;

	public ColumnLayout columnLayout { get; set; }

	public bool active
	{
		get
		{
			return m_Active;
		}
		set
		{
			if (m_Active != value)
			{
				m_Active = value;
				this.activeChanged?.Invoke(this);
			}
		}
	}

	public bool moving
	{
		get
		{
			return m_Moving;
		}
		set
		{
			if (m_Moving != value)
			{
				m_Moving = value;
				this.movingChanged?.Invoke(this);
			}
		}
	}

	public event Action<ColumnMover> activeChanged;

	public event Action<ColumnMover> movingChanged;

	public ColumnMover()
	{
		base.activators.Add(new ManipulatorActivationFilter
		{
			button = MouseButton.LeftMouse
		});
	}

	protected override void RegisterCallbacksOnTarget()
	{
		base.target.RegisterCallback<MouseDownEvent>(OnMouseDown);
		base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
		base.target.RegisterCallback<MouseUpEvent>(OnMouseUp);
		base.target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
		base.target.RegisterCallback<PointerDownEvent>(OnPointerDown);
		base.target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
		base.target.RegisterCallback<PointerUpEvent>(OnPointerUp);
		base.target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
		base.target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
		base.target.RegisterCallback<KeyDownEvent>(OnKeyDown);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		base.target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
		base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
		base.target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
		base.target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOut);
		base.target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
		base.target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
		base.target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
		base.target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
		base.target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
		base.target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
	}

	protected void OnMouseDown(MouseDownEvent evt)
	{
		if (CanStartManipulation(evt))
		{
			ProcessDownEvent(evt, evt.localMousePosition, PointerId.mousePointerId);
		}
	}

	protected void OnMouseMove(MouseMoveEvent evt)
	{
		if (active)
		{
			ProcessMoveEvent(evt, evt.localMousePosition);
		}
	}

	protected void OnMouseUp(MouseUpEvent evt)
	{
		if (active && CanStopManipulation(evt))
		{
			ProcessUpEvent(evt, evt.localMousePosition, PointerId.mousePointerId);
		}
	}

	private void OnMouseCaptureOut(MouseCaptureOutEvent evt)
	{
		if (active)
		{
			ProcessCancelEvent(evt, PointerId.mousePointerId);
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		if (CanStartManipulation(evt))
		{
			if (evt.pointerId != PointerId.mousePointerId)
			{
				ProcessDownEvent(evt, evt.localPosition, evt.pointerId);
				base.target.panel.PreventCompatibilityMouseEvents(evt.pointerId);
			}
			else
			{
				evt.StopImmediatePropagation();
			}
		}
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		if (active)
		{
			if (evt.pointerId != PointerId.mousePointerId)
			{
				ProcessMoveEvent(evt, evt.localPosition);
				base.target.panel.PreventCompatibilityMouseEvents(evt.pointerId);
			}
			else
			{
				evt.StopPropagation();
			}
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (active && CanStopManipulation(evt))
		{
			if (evt.pointerId != PointerId.mousePointerId)
			{
				ProcessUpEvent(evt, evt.localPosition, evt.pointerId);
				base.target.panel.PreventCompatibilityMouseEvents(evt.pointerId);
			}
			else
			{
				evt.StopPropagation();
			}
		}
	}

	private void OnPointerCancel(PointerCancelEvent evt)
	{
		if (active && CanStopManipulation(evt) && IsNotMouseEvent(evt.pointerId))
		{
			ProcessCancelEvent(evt, evt.pointerId);
		}
	}

	private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
	{
		if (active && IsNotMouseEvent(evt.pointerId))
		{
			ProcessCancelEvent(evt, evt.pointerId);
		}
	}

	private static bool IsNotMouseEvent(int pointerId)
	{
		return pointerId != PointerId.mousePointerId;
	}

	protected void ProcessCancelEvent(EventBase evt, int pointerId)
	{
		active = false;
		base.target.ReleasePointer(pointerId);
		if (!(evt is IPointerEvent))
		{
			base.target.panel.ProcessPointerCapture(pointerId);
		}
		if (moving)
		{
			EndDragMove(cancelled: true);
		}
		evt.StopPropagation();
	}

	private void OnKeyDown(KeyDownEvent e)
	{
		if (e.keyCode == KeyCode.Escape && moving)
		{
			EndDragMove(cancelled: true);
		}
	}

	private void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId)
	{
		if (active)
		{
			evt.StopImmediatePropagation();
			return;
		}
		base.target.CapturePointer(pointerId);
		if (!(evt is IPointerEvent))
		{
			base.target.panel.ProcessPointerCapture(pointerId);
		}
		VisualElement visualElement = evt.currentTarget as VisualElement;
		MultiColumnCollectionHeader firstAncestorOfType = visualElement.GetFirstAncestorOfType<MultiColumnCollectionHeader>();
		if (firstAncestorOfType.columns.reorderable)
		{
			m_Header = firstAncestorOfType;
			Vector2 vector = visualElement.ChangeCoordinatesTo(m_Header, localPosition);
			columnLayout = m_Header.columnLayout;
			m_Cancelled = false;
			m_StartPos = vector.x;
			active = true;
			evt.StopPropagation();
		}
	}

	private void ProcessMoveEvent(EventBase e, Vector2 localPosition)
	{
		if (!m_Cancelled)
		{
			VisualElement src = e.currentTarget as VisualElement;
			Vector2 vector = src.ChangeCoordinatesTo(m_Header, localPosition);
			if (!moving && Mathf.Abs(m_StartPos - vector.x) > 5f)
			{
				BeginDragMove(m_StartPos);
			}
			if (moving)
			{
				DragMove(vector.x);
			}
			e.StopPropagation();
		}
	}

	private void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId)
	{
		active = false;
		base.target.ReleasePointer(pointerId);
		if (!(evt is IPointerEvent))
		{
			base.target.panel.ProcessPointerCapture(pointerId);
		}
		bool flag = moving || m_Cancelled;
		EndDragMove(cancelled: false);
		if (flag)
		{
			evt.StopImmediatePropagation();
		}
		else
		{
			evt.StopPropagation();
		}
	}

	private void BeginDragMove(float pos)
	{
		float num = 0f;
		Columns columns = columnLayout.columns;
		foreach (Column visible in columns.visibleList)
		{
			num += columnLayout.GetDesiredWidth(visible);
			if (m_ColumnToMove == null && num > pos)
			{
				m_ColumnToMove = visible;
			}
		}
		moving = true;
		m_LastPos = pos;
		m_PreviewElement = new MultiColumnHeaderColumnMovePreview();
		m_LocationPreviewElement = new MultiColumnHeaderColumnMoveLocationPreview();
		m_Header.hierarchy.Add(m_PreviewElement);
		VisualElement visualElement = m_Header.GetFirstAncestorOfType<ScrollView>()?.parent ?? m_Header;
		visualElement.hierarchy.Add(m_LocationPreviewElement);
		m_ColumnToMovePos = columnLayout.GetDesiredPosition(m_ColumnToMove);
		m_ColumnToMoveWidth = columnLayout.GetDesiredWidth(m_ColumnToMove);
		UpdateMoveLocation();
	}

	internal void DragMove(float pos)
	{
		m_LastPos = pos;
		UpdateMoveLocation();
	}

	private void UpdatePreviewPosition()
	{
		m_PreviewElement.style.left = m_ColumnToMovePos + m_LastPos - m_StartPos;
		m_PreviewElement.style.width = m_ColumnToMoveWidth;
		if (m_DestinationColumn != null)
		{
			m_LocationPreviewElement.style.left = columnLayout.GetDesiredPosition(m_DestinationColumn) + ((!m_MoveBeforeDestination) ? columnLayout.GetDesiredWidth(m_DestinationColumn) : 0f);
		}
	}

	private void UpdateMoveLocation()
	{
		float num = 0f;
		m_DestinationColumn = null;
		m_MoveBeforeDestination = false;
		foreach (Column visible in columnLayout.columns.visibleList)
		{
			m_DestinationColumn = visible;
			float desiredWidth = columnLayout.GetDesiredWidth(m_DestinationColumn);
			float num2 = num + desiredWidth / 2f;
			num += desiredWidth;
			if (num > m_LastPos)
			{
				m_MoveBeforeDestination = m_LastPos < num2;
				break;
			}
		}
		UpdatePreviewPosition();
	}

	private void EndDragMove(bool cancelled)
	{
		if (!moving || m_Cancelled)
		{
			return;
		}
		m_Cancelled = cancelled;
		if (!cancelled)
		{
			int num = m_DestinationColumn.displayIndex;
			if (!m_MoveBeforeDestination)
			{
				num++;
			}
			if (m_ColumnToMove.displayIndex < num)
			{
				num--;
			}
			if (m_ColumnToMove.displayIndex != num)
			{
				columnLayout.columns.ReorderDisplay(m_ColumnToMove.displayIndex, num);
			}
		}
		m_PreviewElement?.RemoveFromHierarchy();
		m_PreviewElement = null;
		m_LocationPreviewElement?.RemoveFromHierarchy();
		m_LocationPreviewElement = null;
		m_ColumnToMove = null;
		moving = false;
	}
}
