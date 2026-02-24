namespace UnityEngine.UIElements;

internal class CommandEventDispatchingStrategy : IEventDispatchingStrategy
{
	public bool CanDispatchEvent(EventBase evt)
	{
		return evt is ICommandEvent;
	}

	public void DispatchEvent(EventBase evt, IPanel panel)
	{
		if (panel != null)
		{
			Focusable leafFocusedElement = panel.focusController.GetLeafFocusedElement();
			if (leafFocusedElement != null)
			{
				if (leafFocusedElement.isIMGUIContainer)
				{
					IMGUIContainer iMGUIContainer = (IMGUIContainer)leafFocusedElement;
					if (!evt.Skip(iMGUIContainer) && iMGUIContainer.SendEventToIMGUI(evt))
					{
						evt.StopPropagation();
						evt.PreventDefault();
					}
					if (!evt.isPropagationStopped && evt.propagateToIMGUI)
					{
						evt.skipElements.Add(iMGUIContainer);
						EventDispatchUtilities.PropagateToIMGUIContainer(panel.visualTree, evt);
					}
				}
				else
				{
					evt.target = leafFocusedElement;
					EventDispatchUtilities.PropagateEvent(evt);
					if (!evt.isPropagationStopped && evt.propagateToIMGUI)
					{
						EventDispatchUtilities.PropagateToIMGUIContainer(panel.visualTree, evt);
					}
				}
			}
			else
			{
				EventDispatchUtilities.PropagateToIMGUIContainer(panel.visualTree, evt);
			}
		}
		evt.propagateToIMGUI = false;
		evt.stopDispatch = true;
	}
}
