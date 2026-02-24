namespace UnityEngine.UIElements;

[EventCategory(EventCategory.EnterLeave)]
public class MouseEnterEvent : MouseEventBase<MouseEnterEvent>
{
	static MouseEnterEvent()
	{
		EventBase<MouseEnterEvent>.SetCreateFunction(() => new MouseEnterEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.TricklesDown | EventPropagation.Cancellable | EventPropagation.IgnoreCompositeRoots;
	}

	public MouseEnterEvent()
	{
		LocalInit();
	}
}
