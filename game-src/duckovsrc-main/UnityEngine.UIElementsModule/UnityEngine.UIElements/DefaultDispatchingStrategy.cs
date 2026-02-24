namespace UnityEngine.UIElements;

internal class DefaultDispatchingStrategy : IEventDispatchingStrategy
{
	public bool CanDispatchEvent(EventBase evt)
	{
		return !(evt is IMGUIEvent);
	}

	public void DispatchEvent(EventBase evt, IPanel panel)
	{
		if (evt.target is VisualElement visualElement && visualElement.panel == panel)
		{
			evt.propagateToIMGUI = visualElement.isIMGUIContainer;
			EventDispatchUtilities.PropagateEvent(evt);
		}
		else if (!evt.isPropagationStopped && panel != null && (evt.propagateToIMGUI || evt.eventTypeId == EventBase<MouseEnterWindowEvent>.TypeId() || evt.eventTypeId == EventBase<MouseLeaveWindowEvent>.TypeId()))
		{
			EventDispatchUtilities.PropagateToIMGUIContainer(panel.visualTree, evt);
		}
		evt.stopDispatch = true;
	}
}
