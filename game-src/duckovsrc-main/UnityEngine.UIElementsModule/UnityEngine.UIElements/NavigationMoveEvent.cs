namespace UnityEngine.UIElements;

public class NavigationMoveEvent : NavigationEventBase<NavigationMoveEvent>
{
	public enum Direction
	{
		None,
		Left,
		Up,
		Right,
		Down,
		Next,
		Previous
	}

	public Direction direction { get; private set; }

	public Vector2 move { get; private set; }

	static NavigationMoveEvent()
	{
		EventBase<NavigationMoveEvent>.SetCreateFunction(() => new NavigationMoveEvent());
	}

	internal static Direction DetermineMoveDirection(float x, float y, float deadZone = 0.6f)
	{
		if (new Vector2(x, y).sqrMagnitude < deadZone * deadZone)
		{
			return Direction.None;
		}
		if (Mathf.Abs(x) > Mathf.Abs(y))
		{
			if (x > 0f)
			{
				return Direction.Right;
			}
			return Direction.Left;
		}
		if (y > 0f)
		{
			return Direction.Up;
		}
		return Direction.Down;
	}

	public static NavigationMoveEvent GetPooled(Vector2 moveVector, EventModifiers modifiers = EventModifiers.None)
	{
		NavigationMoveEvent navigationMoveEvent = NavigationEventBase<NavigationMoveEvent>.GetPooled(NavigationDeviceType.Unknown, modifiers);
		navigationMoveEvent.direction = DetermineMoveDirection(moveVector.x, moveVector.y);
		navigationMoveEvent.move = moveVector;
		return navigationMoveEvent;
	}

	internal static NavigationMoveEvent GetPooled(Vector2 moveVector, NavigationDeviceType deviceType, EventModifiers modifiers = EventModifiers.None)
	{
		NavigationMoveEvent navigationMoveEvent = NavigationEventBase<NavigationMoveEvent>.GetPooled(deviceType, modifiers);
		navigationMoveEvent.direction = DetermineMoveDirection(moveVector.x, moveVector.y);
		navigationMoveEvent.move = moveVector;
		return navigationMoveEvent;
	}

	public static NavigationMoveEvent GetPooled(Direction direction, EventModifiers modifiers = EventModifiers.None)
	{
		NavigationMoveEvent navigationMoveEvent = NavigationEventBase<NavigationMoveEvent>.GetPooled(NavigationDeviceType.Unknown, modifiers);
		navigationMoveEvent.direction = direction;
		navigationMoveEvent.move = Vector2.zero;
		return navigationMoveEvent;
	}

	internal static NavigationMoveEvent GetPooled(Direction direction, NavigationDeviceType deviceType, EventModifiers modifiers = EventModifiers.None)
	{
		NavigationMoveEvent navigationMoveEvent = NavigationEventBase<NavigationMoveEvent>.GetPooled(deviceType, modifiers);
		navigationMoveEvent.direction = direction;
		navigationMoveEvent.move = Vector2.zero;
		return navigationMoveEvent;
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	public NavigationMoveEvent()
	{
		LocalInit();
	}

	private void LocalInit()
	{
		direction = Direction.None;
		move = Vector2.zero;
	}
}
