namespace UnityEngine.UIElements;

[EventCategory(EventCategory.IMGUI)]
public class IMGUIEvent : EventBase<IMGUIEvent>
{
	static IMGUIEvent()
	{
		EventBase<IMGUIEvent>.SetCreateFunction(() => new IMGUIEvent());
	}

	public static IMGUIEvent GetPooled(Event systemEvent)
	{
		IMGUIEvent iMGUIEvent = EventBase<IMGUIEvent>.GetPooled();
		iMGUIEvent.imguiEvent = systemEvent;
		return iMGUIEvent;
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable;
	}

	public IMGUIEvent()
	{
		LocalInit();
	}
}
