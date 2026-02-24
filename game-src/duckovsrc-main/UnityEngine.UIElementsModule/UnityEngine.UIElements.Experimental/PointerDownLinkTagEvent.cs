namespace UnityEngine.UIElements.Experimental;

public sealed class PointerDownLinkTagEvent : PointerEventBase<PointerDownLinkTagEvent>
{
	public string linkID { get; private set; }

	public string linkText { get; private set; }

	static PointerDownLinkTagEvent()
	{
		EventBase<PointerDownLinkTagEvent>.SetCreateFunction(() => new PointerDownLinkTagEvent());
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

	public static PointerDownLinkTagEvent GetPooled(IPointerEvent evt, string linkID, string linkText)
	{
		PointerDownLinkTagEvent pointerDownLinkTagEvent = PointerEventBase<PointerDownLinkTagEvent>.GetPooled(evt);
		pointerDownLinkTagEvent.linkID = linkID;
		pointerDownLinkTagEvent.linkText = linkText;
		return pointerDownLinkTagEvent;
	}

	public PointerDownLinkTagEvent()
	{
		LocalInit();
	}
}
