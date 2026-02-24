namespace UnityEngine.UIElements.Experimental;

[EventCategory(EventCategory.PointerMove)]
public class PointerMoveLinkTagEvent : PointerEventBase<PointerMoveLinkTagEvent>
{
	public string linkID { get; private set; }

	public string linkText { get; private set; }

	static PointerMoveLinkTagEvent()
	{
		EventBase<PointerMoveLinkTagEvent>.SetCreateFunction(() => new PointerMoveLinkTagEvent());
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

	public static PointerMoveLinkTagEvent GetPooled(IPointerEvent evt, string linkID, string linkText)
	{
		PointerMoveLinkTagEvent pointerMoveLinkTagEvent = PointerEventBase<PointerMoveLinkTagEvent>.GetPooled(evt);
		pointerMoveLinkTagEvent.linkID = linkID;
		pointerMoveLinkTagEvent.linkText = linkText;
		return pointerMoveLinkTagEvent;
	}

	public PointerMoveLinkTagEvent()
	{
		LocalInit();
	}
}
