using System;

namespace UnityEngine.UIElements;

internal class ClampedDragger<T> : Clickable where T : IComparable<T>
{
	[Flags]
	public enum DragDirection
	{
		None = 0,
		LowToHigh = 1,
		HighToLow = 2,
		Free = 4
	}

	public DragDirection dragDirection { get; set; }

	private BaseSlider<T> slider { get; set; }

	public Vector2 startMousePosition { get; private set; }

	public Vector2 delta => base.lastMousePosition - startMousePosition;

	public event Action dragging;

	public event Action draggingEnded;

	public ClampedDragger(BaseSlider<T> slider, Action clickHandler, Action dragHandler)
		: base(clickHandler, 250L, 30L)
	{
		dragDirection = DragDirection.None;
		this.slider = slider;
		dragging += dragHandler;
	}

	protected override void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId)
	{
		startMousePosition = localPosition;
		dragDirection = DragDirection.None;
		base.ProcessDownEvent(evt, localPosition, pointerId);
		this.dragging?.Invoke();
	}

	protected override void ProcessUpEvent(EventBase evt, Vector2 localPosition, int pointerId)
	{
		base.ProcessUpEvent(evt, localPosition, pointerId);
		this.draggingEnded?.Invoke();
	}

	protected override void ProcessMoveEvent(EventBase evt, Vector2 localPosition)
	{
		base.ProcessMoveEvent(evt, localPosition);
		if (dragDirection == DragDirection.None)
		{
			dragDirection = DragDirection.Free;
		}
		if (dragDirection != DragDirection.Free)
		{
			return;
		}
		if (evt.eventTypeId == EventBase<PointerMoveEvent>.TypeId())
		{
			PointerMoveEvent pointerMoveEvent = (PointerMoveEvent)evt;
			if (pointerMoveEvent.pointerId != PointerId.mousePointerId)
			{
				pointerMoveEvent.isHandledByDraggable = true;
			}
		}
		this.dragging?.Invoke();
	}
}
