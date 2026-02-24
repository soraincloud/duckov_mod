using System;

namespace UnityEngine.UIElements;

public class KeyboardNavigationManipulator : Manipulator
{
	private readonly Action<KeyboardNavigationOperation, EventBase> m_Action;

	public KeyboardNavigationManipulator(Action<KeyboardNavigationOperation, EventBase> action)
	{
		m_Action = action;
	}

	protected override void RegisterCallbacksOnTarget()
	{
		base.target.RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
		base.target.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
		base.target.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);
		base.target.RegisterCallback<KeyDownEvent>(OnKeyDown);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		base.target.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove);
		base.target.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
		base.target.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
		base.target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
	}

	internal void OnKeyDown(KeyDownEvent evt)
	{
		KeyboardNavigationOperation keyboardNavigationOperation = GetOperation();
		if (keyboardNavigationOperation != KeyboardNavigationOperation.None)
		{
			Invoke(keyboardNavigationOperation, evt);
		}
		KeyboardNavigationOperation GetOperation()
		{
			switch (evt.keyCode)
			{
			case KeyCode.A:
				if (evt.actionKey)
				{
					return KeyboardNavigationOperation.SelectAll;
				}
				break;
			case KeyCode.Home:
				return KeyboardNavigationOperation.Begin;
			case KeyCode.End:
				return KeyboardNavigationOperation.End;
			case KeyCode.PageUp:
				return KeyboardNavigationOperation.PageUp;
			case KeyCode.PageDown:
				return KeyboardNavigationOperation.PageDown;
			case KeyCode.UpArrow:
			case KeyCode.DownArrow:
			case KeyCode.RightArrow:
			case KeyCode.LeftArrow:
				evt.StopPropagation();
				break;
			}
			return KeyboardNavigationOperation.None;
		}
	}

	private void OnNavigationCancel(NavigationCancelEvent evt)
	{
		Invoke(KeyboardNavigationOperation.Cancel, evt);
	}

	private void OnNavigationSubmit(NavigationSubmitEvent evt)
	{
		Invoke(KeyboardNavigationOperation.Submit, evt);
	}

	private void OnNavigationMove(NavigationMoveEvent evt)
	{
		switch (evt.direction)
		{
		case NavigationMoveEvent.Direction.Up:
			Invoke(KeyboardNavigationOperation.Previous, evt);
			break;
		case NavigationMoveEvent.Direction.Down:
			Invoke(KeyboardNavigationOperation.Next, evt);
			break;
		case NavigationMoveEvent.Direction.Left:
			Invoke(KeyboardNavigationOperation.MoveLeft, evt);
			break;
		case NavigationMoveEvent.Direction.Right:
			Invoke(KeyboardNavigationOperation.MoveRight, evt);
			break;
		}
	}

	private void Invoke(KeyboardNavigationOperation operation, EventBase evt)
	{
		m_Action?.Invoke(operation, evt);
	}
}
