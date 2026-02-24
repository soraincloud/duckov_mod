namespace UnityEngine.UIElements;

internal class PointerEventDispatchingStrategy : IEventDispatchingStrategy
{
	public bool CanDispatchEvent(EventBase evt)
	{
		return evt is IPointerEvent;
	}

	public virtual void DispatchEvent(EventBase evt, IPanel panel)
	{
		SetBestTargetForEvent(evt, panel);
		SendEventToTarget(evt, panel);
		evt.stopDispatch = true;
	}

	private static void SendEventToTarget(EventBase evt, IPanel panel)
	{
		if (evt.target is VisualElement visualElement && visualElement.panel == panel)
		{
			EventDispatchUtilities.PropagateEvent(evt);
		}
	}

	private static void SetBestTargetForEvent(EventBase evt, IPanel panel)
	{
		UpdateElementUnderPointer(evt, panel, out var elementUnderPointer);
		if (evt.target == null && elementUnderPointer != null)
		{
			evt.propagateToIMGUI = false;
			evt.target = elementUnderPointer;
		}
		else if (evt.target == null && elementUnderPointer == null)
		{
			if (panel != null && panel.contextType == ContextType.Editor && evt.eventTypeId == EventBase<PointerUpEvent>.TypeId())
			{
				evt.target = (panel as Panel)?.rootIMGUIContainer;
			}
			else
			{
				evt.target = panel?.visualTree;
			}
		}
		else if (evt.target != null)
		{
			evt.propagateToIMGUI = false;
		}
	}

	private static void UpdateElementUnderPointer(EventBase evt, IPanel panel, out VisualElement elementUnderPointer)
	{
		IPointerEvent pointerEvent = evt as IPointerEvent;
		BaseVisualElementPanel baseVisualElementPanel = panel as BaseVisualElementPanel;
		bool flag = (evt as IPointerEventInternal)?.recomputeTopElementUnderPointer ?? true;
		elementUnderPointer = ((!flag) ? baseVisualElementPanel?.GetTopElementUnderPointer(pointerEvent.pointerId) : baseVisualElementPanel?.RecomputeTopElementUnderPointer(pointerEvent.pointerId, pointerEvent.position, evt));
	}
}
