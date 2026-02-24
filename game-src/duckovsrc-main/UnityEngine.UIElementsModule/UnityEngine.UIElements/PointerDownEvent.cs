namespace UnityEngine.UIElements;

public sealed class PointerDownEvent : PointerEventBase<PointerDownEvent>
{
	static PointerDownEvent()
	{
		EventBase<PointerDownEvent>.SetCreateFunction(() => new PointerDownEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable | EventPropagation.SkipDisabledElements;
		((IPointerEventInternal)this).triggeredByOS = true;
		((IPointerEventInternal)this).recomputeTopElementUnderPointer = true;
	}

	public PointerDownEvent()
	{
		LocalInit();
	}

	protected internal override void PostDispatch(IPanel panel)
	{
		if (!base.isDefaultPrevented)
		{
			if (panel.ShouldSendCompatibilityMouseEvents(this))
			{
				using MouseDownEvent mouseDownEvent = MouseDownEvent.GetPooled(this);
				mouseDownEvent.target = base.target;
				mouseDownEvent.target.SendEvent(mouseDownEvent);
			}
		}
		else
		{
			panel.PreventCompatibilityMouseEvents(base.pointerId);
		}
		base.PostDispatch(panel);
	}
}
