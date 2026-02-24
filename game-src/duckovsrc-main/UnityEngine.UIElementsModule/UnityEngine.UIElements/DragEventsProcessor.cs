#define UNITY_ASSERTIONS
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

internal abstract class DragEventsProcessor
{
	internal enum DragState
	{
		None,
		CanStartDrag,
		Dragging
	}

	private bool m_IsRegistered;

	private DragState m_DragState;

	private Vector3 m_Start;

	protected readonly VisualElement m_Target;

	internal bool isRegistered => m_IsRegistered;

	internal DragState dragState => m_DragState;

	protected virtual bool supportsDragEvents => true;

	private bool useDragEvents => isEditorContext && supportsDragEvents;

	protected IDragAndDrop dragAndDrop => DragAndDropUtility.GetDragAndDrop(m_Target.panel);

	internal virtual bool isEditorContext
	{
		get
		{
			Assert.IsNotNull(m_Target);
			Assert.IsNotNull(m_Target.parent);
			return m_Target.panel.contextType == ContextType.Editor;
		}
	}

	internal DragEventsProcessor(VisualElement target)
	{
		m_Target = target;
		m_Target.RegisterCallback<AttachToPanelEvent>(RegisterCallbacksFromTarget);
		m_Target.RegisterCallback<DetachFromPanelEvent>(UnregisterCallbacksFromTarget);
		RegisterCallbacksFromTarget();
	}

	private void RegisterCallbacksFromTarget(AttachToPanelEvent evt)
	{
		RegisterCallbacksFromTarget();
	}

	private void RegisterCallbacksFromTarget()
	{
		if (!m_IsRegistered)
		{
			m_IsRegistered = true;
			m_Target.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
			m_Target.RegisterCallback<PointerUpEvent>(OnPointerUpEvent, TrickleDown.TrickleDown);
			m_Target.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
			m_Target.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
			m_Target.RegisterCallback<PointerCancelEvent>(OnPointerCancelEvent);
			m_Target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCapturedOut);
		}
	}

	private void UnregisterCallbacksFromTarget(DetachFromPanelEvent evt)
	{
		UnregisterCallbacksFromTarget();
	}

	internal void UnregisterCallbacksFromTarget(bool unregisterPanelEvents = false)
	{
		m_IsRegistered = false;
		m_Target.UnregisterCallback<PointerDownEvent>(OnPointerDownEvent);
		m_Target.UnregisterCallback<PointerUpEvent>(OnPointerUpEvent, TrickleDown.TrickleDown);
		m_Target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveEvent);
		m_Target.UnregisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
		m_Target.UnregisterCallback<PointerCancelEvent>(OnPointerCancelEvent);
		m_Target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCapturedOut);
		if (unregisterPanelEvents)
		{
			m_Target.UnregisterCallback<AttachToPanelEvent>(RegisterCallbacksFromTarget);
			m_Target.UnregisterCallback<DetachFromPanelEvent>(UnregisterCallbacksFromTarget);
		}
	}

	protected abstract bool CanStartDrag(Vector3 pointerPosition);

	protected internal abstract StartDragArgs StartDrag(Vector3 pointerPosition);

	protected internal abstract void UpdateDrag(Vector3 pointerPosition);

	protected internal abstract void OnDrop(Vector3 pointerPosition);

	protected abstract void ClearDragAndDropUI(bool dragCancelled);

	private void OnPointerDownEvent(PointerDownEvent evt)
	{
		if (evt.button != 0 || evt.leafTarget is VisualElement { isIMGUIContainer: not false })
		{
			m_DragState = DragState.None;
		}
		else if (CanStartDrag(evt.position))
		{
			m_DragState = DragState.CanStartDrag;
			m_Start = evt.position;
		}
	}

	internal void OnPointerUpEvent(PointerUpEvent evt)
	{
		if (!useDragEvents && m_DragState == DragState.Dragging)
		{
			DragEventsProcessor dragEventsProcessor = GetDropTarget(evt.position) ?? this;
			dragEventsProcessor.UpdateDrag(evt.position);
			dragEventsProcessor.OnDrop(evt.position);
			dragEventsProcessor.ClearDragAndDropUI(dragCancelled: false);
			evt.StopPropagation();
		}
		m_Target.ReleasePointer(evt.pointerId);
		ClearDragAndDropUI(m_DragState == DragState.Dragging);
		dragAndDrop.DragCleanup();
		m_DragState = DragState.None;
	}

	private void OnPointerLeaveEvent(PointerLeaveEvent evt)
	{
		ClearDragAndDropUI(dragCancelled: false);
	}

	private void OnPointerCancelEvent(PointerCancelEvent evt)
	{
		if (!useDragEvents)
		{
			ClearDragAndDropUI(dragCancelled: true);
		}
		m_Target.ReleasePointer(evt.pointerId);
		ClearDragAndDropUI(m_DragState == DragState.Dragging);
		dragAndDrop.DragCleanup();
		m_DragState = DragState.None;
	}

	private void OnPointerCapturedOut(PointerCaptureOutEvent evt)
	{
		if (!useDragEvents)
		{
			ClearDragAndDropUI(dragCancelled: true);
		}
		ClearDragAndDropUI(m_DragState == DragState.Dragging);
		dragAndDrop.DragCleanup();
		m_DragState = DragState.None;
	}

	private void OnPointerMoveEvent(PointerMoveEvent evt)
	{
		if (evt.isHandledByDraggable)
		{
			return;
		}
		if (!useDragEvents && m_DragState == DragState.Dragging)
		{
			DragEventsProcessor dragEventsProcessor = GetDropTarget(evt.position) ?? this;
			dragEventsProcessor.UpdateDrag(evt.position);
		}
		else
		{
			if (m_DragState != DragState.CanStartDrag || !((m_Start - evt.position).sqrMagnitude >= 100f))
			{
				return;
			}
			StartDragArgs args = StartDrag(m_Start);
			if (args.visualMode == DragVisualMode.Rejected)
			{
				m_DragState = DragState.None;
				return;
			}
			if (!useDragEvents)
			{
				if (supportsDragEvents)
				{
					dragAndDrop.StartDrag(args, evt.position);
				}
			}
			else
			{
				if (Event.current != null && Event.current.type != EventType.MouseDown && Event.current.type != EventType.MouseDrag)
				{
					return;
				}
				dragAndDrop.StartDrag(args, evt.position);
			}
			m_DragState = DragState.Dragging;
			m_Target.CapturePointer(evt.pointerId);
			evt.isHandledByDraggable = true;
			evt.StopPropagation();
		}
	}

	private DragEventsProcessor GetDropTarget(Vector2 position)
	{
		DragEventsProcessor result = null;
		if (m_Target.worldBound.Contains(position))
		{
			result = this;
		}
		else if (supportsDragEvents)
		{
			result = (m_Target.elementPanel.Pick(position)?.GetFirstOfType<BaseVerticalCollectionView>())?.dragger;
		}
		return result;
	}
}
