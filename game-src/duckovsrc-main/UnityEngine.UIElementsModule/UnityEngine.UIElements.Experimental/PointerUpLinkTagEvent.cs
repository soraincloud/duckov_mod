namespace UnityEngine.UIElements.Experimental;

public class PointerUpLinkTagEvent : PointerEventBase<PointerUpLinkTagEvent>
{
	public string linkID { get; private set; }

	public string linkText { get; private set; }

	static PointerUpLinkTagEvent()
	{
		EventBase<PointerUpLinkTagEvent>.SetCreateFunction(() => new PointerUpLinkTagEvent());
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

	public static PointerUpLinkTagEvent GetPooled(IPointerEvent evt, string linkID, string linkText)
	{
		PointerUpLinkTagEvent pointerUpLinkTagEvent = PointerEventBase<PointerUpLinkTagEvent>.GetPooled(evt);
		pointerUpLinkTagEvent.linkID = linkID;
		pointerUpLinkTagEvent.linkText = linkText;
		return pointerUpLinkTagEvent;
	}

	public PointerUpLinkTagEvent()
	{
		LocalInit();
	}
}
