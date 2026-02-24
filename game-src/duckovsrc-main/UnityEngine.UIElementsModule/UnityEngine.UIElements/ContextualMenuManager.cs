namespace UnityEngine.UIElements;

public abstract class ContextualMenuManager
{
	internal bool displayMenuHandledOSX { get; set; }

	public abstract void DisplayMenuIfEventMatches(EventBase evt, IEventHandler eventHandler);

	public void DisplayMenu(EventBase triggerEvent, IEventHandler target)
	{
		DropdownMenu menu = new DropdownMenu();
		int pointerId;
		int button;
		using (ContextualMenuPopulateEvent contextualMenuPopulateEvent = ContextualMenuPopulateEvent.GetPooled(triggerEvent, menu, target, this))
		{
			pointerId = ((triggerEvent is IPointerEvent pointerEvent) ? pointerEvent.pointerId : PointerId.mousePointerId);
			button = contextualMenuPopulateEvent.button;
			target?.SendEvent(contextualMenuPopulateEvent);
		}
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			displayMenuHandledOSX = true;
			if (button >= 0)
			{
				PointerDeviceState.ReleaseButton(pointerId, button);
			}
		}
	}

	protected internal abstract void DoDisplayMenu(DropdownMenu menu, EventBase triggerEvent);
}
