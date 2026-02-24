namespace UnityEngine.UIElements.Experimental;

[EventCategory(EventCategory.EnterLeave)]
public class PointerOverLinkTagEvent : PointerEventBase<PointerOverLinkTagEvent>
{
	public string linkID { get; private set; }

	public string linkText { get; private set; }

	static PointerOverLinkTagEvent()
	{
		EventBase<PointerOverLinkTagEvent>.SetCreateFunction(() => new PointerOverLinkTagEvent());
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

	public static PointerOverLinkTagEvent GetPooled(IPointerEvent evt, string linkID, string linkText)
	{
		PointerOverLinkTagEvent pointerOverLinkTagEvent = PointerEventBase<PointerOverLinkTagEvent>.GetPooled(evt);
		pointerOverLinkTagEvent.linkID = linkID;
		pointerOverLinkTagEvent.linkText = linkText;
		return pointerOverLinkTagEvent;
	}

	public PointerOverLinkTagEvent()
	{
		LocalInit();
	}
}
