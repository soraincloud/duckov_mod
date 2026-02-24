using System;

namespace UnityEngine.UIElements;

public class ContextualMenuManipulator : MouseManipulator
{
	private Action<ContextualMenuPopulateEvent> m_MenuBuilder;

	public ContextualMenuManipulator(Action<ContextualMenuPopulateEvent> menuBuilder)
	{
		m_MenuBuilder = menuBuilder;
		base.activators.Add(new ManipulatorActivationFilter
		{
			button = MouseButton.RightMouse
		});
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse,
				modifiers = EventModifiers.Control
			});
		}
	}

	protected override void RegisterCallbacksOnTarget()
	{
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			base.target.RegisterCallback<MouseDownEvent>(OnMouseDownEventOSX);
			base.target.RegisterCallback<MouseUpEvent>(OnMouseUpEventOSX);
		}
		else
		{
			base.target.RegisterCallback<MouseUpEvent>(OnMouseUpDownEvent);
		}
		base.target.RegisterCallback<KeyUpEvent>(OnKeyUpEvent);
		base.target.RegisterCallback<ContextualMenuPopulateEvent>(OnContextualMenuEvent);
	}

	protected override void UnregisterCallbacksFromTarget()
	{
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			base.target.UnregisterCallback<MouseDownEvent>(OnMouseDownEventOSX);
			base.target.UnregisterCallback<MouseUpEvent>(OnMouseUpEventOSX);
		}
		else
		{
			base.target.UnregisterCallback<MouseUpEvent>(OnMouseUpDownEvent);
		}
		base.target.UnregisterCallback<KeyUpEvent>(OnKeyUpEvent);
		base.target.UnregisterCallback<ContextualMenuPopulateEvent>(OnContextualMenuEvent);
	}

	private void OnMouseUpDownEvent(IMouseEvent evt)
	{
		if (CanStartManipulation(evt))
		{
			DoDisplayMenu(evt as EventBase);
		}
	}

	private void OnMouseDownEventOSX(MouseDownEvent evt)
	{
		if (base.target.elementPanel?.contextualMenuManager != null)
		{
			base.target.elementPanel.contextualMenuManager.displayMenuHandledOSX = false;
		}
		if (!evt.isDefaultPrevented)
		{
			OnMouseUpDownEvent(evt);
		}
	}

	private void OnMouseUpEventOSX(MouseUpEvent evt)
	{
		if (base.target.elementPanel?.contextualMenuManager == null || !base.target.elementPanel.contextualMenuManager.displayMenuHandledOSX)
		{
			OnMouseUpDownEvent(evt);
		}
	}

	private void OnKeyUpEvent(KeyUpEvent evt)
	{
		if (evt.keyCode == KeyCode.Menu)
		{
			DoDisplayMenu(evt);
		}
	}

	private void DoDisplayMenu(EventBase evt)
	{
		if (base.target.elementPanel?.contextualMenuManager != null)
		{
			base.target.elementPanel.contextualMenuManager.DisplayMenu(evt, base.target);
			evt.StopPropagation();
			evt.PreventDefault();
		}
	}

	private void OnContextualMenuEvent(ContextualMenuPopulateEvent evt)
	{
		m_MenuBuilder?.Invoke(evt);
	}
}
