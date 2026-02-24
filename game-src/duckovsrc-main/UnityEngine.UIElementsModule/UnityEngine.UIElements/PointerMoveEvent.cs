namespace UnityEngine.UIElements;

[EventCategory(EventCategory.PointerMove)]
public sealed class PointerMoveEvent : PointerEventBase<PointerMoveEvent>
{
	internal bool isHandledByDraggable { get; set; }

	static PointerMoveEvent()
	{
		EventBase<PointerMoveEvent>.SetCreateFunction(() => new PointerMoveEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable;
		((IPointerEventInternal)this).triggeredByOS = true;
		((IPointerEventInternal)this).recomputeTopElementUnderPointer = true;
		isHandledByDraggable = false;
	}

	public PointerMoveEvent()
	{
		LocalInit();
	}

	protected internal override void PostDispatch(IPanel panel)
	{
		if (panel.ShouldSendCompatibilityMouseEvents(this))
		{
			if (base.imguiEvent != null && base.imguiEvent.rawType == EventType.MouseDown)
			{
				using MouseDownEvent mouseDownEvent = MouseDownEvent.GetPooled(this);
				mouseDownEvent.target = base.target;
				mouseDownEvent.target.SendEvent(mouseDownEvent);
			}
			else if (base.imguiEvent != null && base.imguiEvent.rawType == EventType.MouseUp)
			{
				using MouseUpEvent mouseUpEvent = MouseUpEvent.GetPooled(this);
				mouseUpEvent.target = base.target;
				mouseUpEvent.target.SendEvent(mouseUpEvent);
			}
			else
			{
				using MouseMoveEvent mouseMoveEvent = MouseMoveEvent.GetPooled(this);
				mouseMoveEvent.target = base.target;
				mouseMoveEvent.target.SendEvent(mouseMoveEvent);
			}
		}
		base.PostDispatch(panel);
	}
}
