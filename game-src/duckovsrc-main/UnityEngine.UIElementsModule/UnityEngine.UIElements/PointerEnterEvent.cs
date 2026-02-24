namespace UnityEngine.UIElements;

[EventCategory(EventCategory.EnterLeave)]
public sealed class PointerEnterEvent : PointerEventBase<PointerEnterEvent>
{
	static PointerEnterEvent()
	{
		EventBase<PointerEnterEvent>.SetCreateFunction(() => new PointerEnterEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.TricklesDown | EventPropagation.IgnoreCompositeRoots;
	}

	public PointerEnterEvent()
	{
		LocalInit();
	}
}
