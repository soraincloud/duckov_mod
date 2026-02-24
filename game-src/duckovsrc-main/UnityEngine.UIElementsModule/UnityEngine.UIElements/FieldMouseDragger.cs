using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class FieldMouseDragger<T> : BaseFieldMouseDragger
{
	private readonly IValueField<T> m_DrivenField;

	private VisualElement m_DragElement;

	private Rect m_DragHotZone;

	public bool dragging { get; set; }

	public T startValue { get; set; }

	public FieldMouseDragger(IValueField<T> drivenField)
	{
		m_DrivenField = drivenField;
		m_DragElement = null;
		m_DragHotZone = new Rect(0f, 0f, -1f, -1f);
		dragging = false;
	}

	public sealed override void SetDragZone(VisualElement dragElement, Rect hotZone)
	{
		if (m_DragElement != null)
		{
			m_DragElement.UnregisterCallback<PointerDownEvent>(UpdateValueOnPointerDown);
			m_DragElement.UnregisterCallback<PointerUpEvent>(UpdateValueOnPointerUp);
			m_DragElement.UnregisterCallback<KeyDownEvent>(UpdateValueOnKeyDown);
		}
		m_DragElement = dragElement;
		m_DragHotZone = hotZone;
		if (m_DragElement != null)
		{
			dragging = false;
			m_DragElement.RegisterCallback<PointerDownEvent>(UpdateValueOnPointerDown);
			m_DragElement.RegisterCallback<PointerUpEvent>(UpdateValueOnPointerUp);
			m_DragElement.RegisterCallback<KeyDownEvent>(UpdateValueOnKeyDown);
		}
	}

	private bool CanStartDrag(int button, Vector2 localPosition)
	{
		return button == 0 && (m_DragHotZone.width < 0f || m_DragHotZone.height < 0f || m_DragHotZone.Contains(m_DragElement.WorldToLocal(localPosition)));
	}

	private void UpdateValueOnPointerDown(PointerDownEvent evt)
	{
		if (CanStartDrag(evt.button, evt.localPosition))
		{
			if (evt.pointerType == PointerType.mouse)
			{
				m_DragElement.CaptureMouse();
				ProcessDownEvent(evt);
			}
			else if (m_DragElement.panel.contextType == ContextType.Editor)
			{
				evt.PreventDefault();
				m_DragElement.CapturePointer(evt.pointerId);
				ProcessDownEvent(evt);
			}
		}
	}

	private void ProcessDownEvent(EventBase evt)
	{
		evt.StopPropagation();
		dragging = true;
		m_DragElement.RegisterCallback<PointerMoveEvent>(UpdateValueOnPointerMove);
		startValue = m_DrivenField.value;
		m_DrivenField.StartDragging();
		(m_DragElement.panel as BaseVisualElementPanel)?.uiElementsBridge?.SetWantsMouseJumping(1);
	}

	private void UpdateValueOnPointerMove(PointerMoveEvent evt)
	{
		ProcessMoveEvent(evt.shiftKey, evt.altKey, evt.deltaPosition);
	}

	private void ProcessMoveEvent(bool shiftKey, bool altKey, Vector2 deltaPosition)
	{
		if (dragging)
		{
			DeltaSpeed speed = ((!shiftKey) ? ((!altKey) ? DeltaSpeed.Normal : DeltaSpeed.Slow) : DeltaSpeed.Fast);
			m_DrivenField.ApplyInputDeviceDelta(deltaPosition, speed, startValue);
		}
	}

	private void UpdateValueOnPointerUp(PointerUpEvent evt)
	{
		ProcessUpEvent(evt, evt.pointerId);
	}

	private void ProcessUpEvent(EventBase evt, int pointerId)
	{
		if (dragging)
		{
			dragging = false;
			m_DragElement.UnregisterCallback<PointerMoveEvent>(UpdateValueOnPointerMove);
			m_DragElement.ReleasePointer(pointerId);
			if (evt is IMouseEvent)
			{
				m_DragElement.panel.ProcessPointerCapture(PointerId.mousePointerId);
			}
			(m_DragElement.panel as BaseVisualElementPanel)?.uiElementsBridge?.SetWantsMouseJumping(0);
			m_DrivenField.StopDragging();
		}
	}

	private void UpdateValueOnKeyDown(KeyDownEvent evt)
	{
		if (dragging && evt.keyCode == KeyCode.Escape)
		{
			dragging = false;
			m_DrivenField.value = startValue;
			m_DrivenField.StopDragging();
			IPanel panel = (evt.target as VisualElement)?.panel;
			panel.ReleasePointer(PointerId.mousePointerId);
			(panel as BaseVisualElementPanel)?.uiElementsBridge?.SetWantsMouseJumping(0);
		}
	}
}
