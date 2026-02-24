namespace UnityEngine.UIElements;

internal class NavigationEventDispatchingStrategy : IEventDispatchingStrategy
{
	public bool CanDispatchEvent(EventBase evt)
	{
		return evt is INavigationEvent;
	}

	public void DispatchEvent(EventBase evt, IPanel panel)
	{
		if (panel != null)
		{
			if (evt.target == null)
			{
				Focusable obj = panel.focusController.GetLeafFocusedElement() ?? panel.visualTree;
				IEventHandler eventHandler = obj;
				evt.target = obj;
			}
			EventDispatchUtilities.PropagateEvent(evt);
		}
		evt.propagateToIMGUI = false;
		evt.stopDispatch = true;
	}
}
