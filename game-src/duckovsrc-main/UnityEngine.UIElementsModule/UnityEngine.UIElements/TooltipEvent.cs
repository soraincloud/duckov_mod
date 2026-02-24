namespace UnityEngine.UIElements;

[EventCategory(EventCategory.Tooltip)]
public class TooltipEvent : EventBase<TooltipEvent>
{
	public string tooltip { get; set; }

	public Rect rect { get; set; }

	static TooltipEvent()
	{
		EventBase<TooltipEvent>.SetCreateFunction(() => new TooltipEvent());
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown;
		rect = default(Rect);
		tooltip = string.Empty;
		base.ignoreCompositeRoots = true;
	}

	internal static TooltipEvent GetPooled(string tooltip, Rect rect)
	{
		TooltipEvent tooltipEvent = EventBase<TooltipEvent>.GetPooled();
		tooltipEvent.tooltip = tooltip;
		tooltipEvent.rect = rect;
		return tooltipEvent;
	}

	public TooltipEvent()
	{
		LocalInit();
	}
}
