namespace UnityEngine.UIElements;

internal class PointerCaptureDispatchingStrategy : IEventDispatchingStrategy
{
	public bool CanDispatchEvent(EventBase evt)
	{
		return evt is IPointerEvent;
	}

	public void DispatchEvent(EventBase evt, IPanel panel)
	{
		if (!(evt is IPointerEvent pointerEvent))
		{
			return;
		}
		IEventHandler capturingElement = panel.GetCapturingElement(pointerEvent.pointerId);
		if (capturingElement == null)
		{
			return;
		}
		VisualElement visualElement = capturingElement as VisualElement;
		if (evt.eventTypeId != EventBase<PointerCaptureOutEvent>.TypeId() && visualElement != null && visualElement.panel == null)
		{
			panel.ReleasePointer(pointerEvent.pointerId);
		}
		else
		{
			if ((evt.target != null && evt.target != capturingElement) || (panel != null && visualElement != null && visualElement.panel != panel))
			{
				return;
			}
			if (evt.eventTypeId != EventBase<PointerCaptureEvent>.TypeId() && evt.eventTypeId != EventBase<PointerCaptureOutEvent>.TypeId())
			{
				panel.ProcessPointerCapture(pointerEvent.pointerId);
			}
			if (panel is BaseVisualElementPanel baseVisualElementPanel)
			{
				IPointerEventInternal obj = pointerEvent as IPointerEventInternal;
				if (obj == null || obj.recomputeTopElementUnderPointer)
				{
					baseVisualElementPanel.RecomputeTopElementUnderPointer(pointerEvent.pointerId, pointerEvent.position, evt);
				}
			}
			evt.dispatch = true;
			evt.target = capturingElement;
			evt.skipDisabledElements = false;
			(capturingElement as CallbackEventHandler)?.HandleEventAtTargetPhase(evt);
			evt.currentTarget = null;
			evt.propagationPhase = PropagationPhase.None;
			evt.dispatch = false;
			evt.stopDispatch = true;
			evt.propagateToIMGUI = false;
		}
	}
}
