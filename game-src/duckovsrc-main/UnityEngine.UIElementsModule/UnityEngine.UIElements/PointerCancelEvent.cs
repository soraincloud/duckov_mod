namespace UnityEngine.UIElements;

public sealed class PointerCancelEvent : PointerEventBase<PointerCancelEvent>
{
	static PointerCancelEvent()
	{
		EventBase<PointerCancelEvent>.SetCreateFunction(() => new PointerCancelEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.SkipDisabledElements;
		((IPointerEventInternal)this).triggeredByOS = true;
		((IPointerEventInternal)this).recomputeTopElementUnderPointer = true;
	}

	public PointerCancelEvent()
	{
		LocalInit();
	}

	protected internal override void PostDispatch(IPanel panel)
	{
		if (PointerType.IsDirectManipulationDevice(base.pointerType))
		{
			panel.ReleasePointer(base.pointerId);
			if (panel is BaseVisualElementPanel baseVisualElementPanel)
			{
				baseVisualElementPanel.ClearCachedElementUnderPointer(base.pointerId, this);
			}
		}
		if (panel.ShouldSendCompatibilityMouseEvents(this))
		{
			using MouseUpEvent mouseUpEvent = MouseUpEvent.GetPooled(this);
			mouseUpEvent.target = base.target;
			base.target.SendEvent(mouseUpEvent);
		}
		base.PostDispatch(panel);
		panel.ActivateCompatibilityMouseEvents(base.pointerId);
	}
}
