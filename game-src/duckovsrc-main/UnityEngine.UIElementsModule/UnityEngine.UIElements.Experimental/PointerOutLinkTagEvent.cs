namespace UnityEngine.UIElements.Experimental;

[EventCategory(EventCategory.EnterLeave)]
public class PointerOutLinkTagEvent : PointerEventBase<PointerOutLinkTagEvent>
{
	static PointerOutLinkTagEvent()
	{
		EventBase<PointerOutLinkTagEvent>.SetCreateFunction(() => new PointerOutLinkTagEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown;
	}

	public static PointerOutLinkTagEvent GetPooled(IPointerEvent evt, string linkID)
	{
		return PointerEventBase<PointerOutLinkTagEvent>.GetPooled(evt);
	}

	public PointerOutLinkTagEvent()
	{
		LocalInit();
	}
}
