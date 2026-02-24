namespace UnityEngine.UIElements;

public class KeyDownEvent : KeyboardEventBase<KeyDownEvent>
{
	static KeyDownEvent()
	{
		EventBase<KeyDownEvent>.SetCreateFunction(() => new KeyDownEvent());
	}

	internal void GetEquivalentImguiEvent(Event outImguiEvent)
	{
		if (base.imguiEvent != null)
		{
			outImguiEvent.CopyFrom(base.imguiEvent);
			return;
		}
		outImguiEvent.type = EventType.KeyDown;
		outImguiEvent.modifiers = base.modifiers;
		outImguiEvent.character = base.character;
		outImguiEvent.keyCode = base.keyCode;
	}

	protected internal override void PostDispatch(IPanel panel)
	{
		base.PostDispatch(panel);
		if (panel.contextType == ContextType.Editor)
		{
			Event obj = base.imguiEvent;
			if (obj == null || obj.type != EventType.Used)
			{
				SendEquivalentNavigationEventIfAny(panel);
			}
		}
	}

	private void SendEquivalentNavigationEventIfAny(IPanel panel)
	{
		if (base.character == '\n' || base.character == '\u0003' || base.character == '\n' || base.character == ' ')
		{
			using (NavigationSubmitEvent navigationSubmitEvent = NavigationEventBase<NavigationSubmitEvent>.GetPooled(NavigationDeviceType.Keyboard, base.modifiers))
			{
				navigationSubmitEvent.target = base.leafTarget;
				panel.visualTree.SendEvent(navigationSubmitEvent);
				return;
			}
		}
		if (base.keyCode == KeyCode.Escape)
		{
			using (NavigationCancelEvent navigationCancelEvent = NavigationEventBase<NavigationCancelEvent>.GetPooled(NavigationDeviceType.Keyboard, base.modifiers))
			{
				navigationCancelEvent.target = base.leafTarget;
				panel.visualTree.SendEvent(navigationCancelEvent);
				return;
			}
		}
		if (this.ShouldSendNavigationMoveEvent())
		{
			using (NavigationMoveEvent navigationMoveEvent = NavigationMoveEvent.GetPooled(base.shiftKey ? NavigationMoveEvent.Direction.Previous : NavigationMoveEvent.Direction.Next, NavigationDeviceType.Keyboard, base.modifiers))
			{
				navigationMoveEvent.target = base.leafTarget;
				panel.visualTree.SendEvent(navigationMoveEvent);
				return;
			}
		}
		if (base.keyCode == KeyCode.RightArrow || base.keyCode == KeyCode.LeftArrow || base.keyCode == KeyCode.UpArrow || base.keyCode == KeyCode.DownArrow)
		{
			Vector2 moveVector = ((base.keyCode == KeyCode.RightArrow) ? Vector2.right : ((base.keyCode == KeyCode.LeftArrow) ? Vector2.left : ((base.keyCode == KeyCode.UpArrow) ? Vector2.up : Vector2.down)));
			using NavigationMoveEvent navigationMoveEvent2 = NavigationMoveEvent.GetPooled(moveVector, NavigationDeviceType.Keyboard, base.modifiers);
			navigationMoveEvent2.target = base.leafTarget;
			panel.visualTree.SendEvent(navigationMoveEvent2);
		}
	}
}
