namespace UnityEngine.UIElements;

public sealed class PointerStationaryEvent : PointerEventBase<PointerStationaryEvent>
{
	static PointerStationaryEvent()
	{
		EventBase<PointerStationaryEvent>.SetCreateFunction(() => new PointerStationaryEvent());
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
	}

	public PointerStationaryEvent()
	{
		LocalInit();
	}
}
