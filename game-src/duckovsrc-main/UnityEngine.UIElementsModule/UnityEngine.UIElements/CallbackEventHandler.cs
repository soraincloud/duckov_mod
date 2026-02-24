using System;

namespace UnityEngine.UIElements;

public abstract class CallbackEventHandler : IEventHandler
{
	internal bool isIMGUIContainer = false;

	private EventCallbackRegistry m_CallbackRegistry;

	internal const string ExecuteDefaultActionName = "ExecuteDefaultAction";

	internal const string ExecuteDefaultActionAtTargetName = "ExecuteDefaultActionAtTarget";

	public void RegisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		if (m_CallbackRegistry == null)
		{
			m_CallbackRegistry = new EventCallbackRegistry();
		}
		m_CallbackRegistry.RegisterCallback(callback, useTrickleDown);
		AddEventCategories<TEventType>();
	}

	private void AddEventCategories<TEventType>() where TEventType : EventBase<TEventType>, new()
	{
		if (this is VisualElement visualElement)
		{
			visualElement.eventCallbackCategories |= 1 << (int)EventBase<TEventType>.EventCategory;
		}
	}

	public void RegisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TUserArgsType userArgs, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		if (m_CallbackRegistry == null)
		{
			m_CallbackRegistry = new EventCallbackRegistry();
		}
		m_CallbackRegistry.RegisterCallback(callback, userArgs, useTrickleDown);
		AddEventCategories<TEventType>();
	}

	internal void RegisterCallback<TEventType>(EventCallback<TEventType> callback, InvokePolicy invokePolicy, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		if (m_CallbackRegistry == null)
		{
			m_CallbackRegistry = new EventCallbackRegistry();
		}
		m_CallbackRegistry.RegisterCallback(callback, useTrickleDown, invokePolicy);
		AddEventCategories<TEventType>();
	}

	public void UnregisterCallback<TEventType>(EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		if (m_CallbackRegistry != null)
		{
			m_CallbackRegistry.UnregisterCallback(callback, useTrickleDown);
		}
	}

	public void UnregisterCallback<TEventType, TUserArgsType>(EventCallback<TEventType, TUserArgsType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
	{
		if (m_CallbackRegistry != null)
		{
			m_CallbackRegistry.UnregisterCallback(callback, useTrickleDown);
		}
	}

	internal bool TryGetUserArgs<TEventType, TCallbackArgs>(EventCallback<TEventType, TCallbackArgs> callback, TrickleDown useTrickleDown, out TCallbackArgs userData) where TEventType : EventBase<TEventType>, new()
	{
		userData = default(TCallbackArgs);
		if (m_CallbackRegistry != null)
		{
			return m_CallbackRegistry.TryGetUserArgs(callback, useTrickleDown, out userData);
		}
		return false;
	}

	public abstract void SendEvent(EventBase e);

	internal abstract void SendEvent(EventBase e, DispatchMode dispatchMode);

	internal void HandleEventAtTargetPhase(EventBase evt)
	{
		evt.currentTarget = evt.target;
		evt.propagationPhase = PropagationPhase.AtTarget;
		HandleEventAtCurrentTargetAndPhase(evt);
		evt.propagationPhase = PropagationPhase.DefaultActionAtTarget;
		HandleEventAtCurrentTargetAndPhase(evt);
	}

	internal void HandleEventAtTargetAndDefaultPhase(EventBase evt)
	{
		HandleEventAtTargetPhase(evt);
		evt.propagationPhase = PropagationPhase.DefaultAction;
		HandleEventAtCurrentTargetAndPhase(evt);
	}

	internal void HandleEventAtCurrentTargetAndPhase(EventBase evt)
	{
		HandleEvent(evt);
	}

	void IEventHandler.HandleEvent(EventBase evt)
	{
		HandleEventAtCurrentTargetAndPhase(evt);
	}

	[Obsolete("The virtual method CallbackEventHandler.HandleEvent is deprecated and will be removed in a future release. Please override ExecuteDefaultAction instead.")]
	public virtual void HandleEvent(EventBase evt)
	{
		if (evt == null)
		{
			return;
		}
		switch (evt.propagationPhase)
		{
		case PropagationPhase.TrickleDown:
		case PropagationPhase.BubbleUp:
			if (!evt.isPropagationStopped)
			{
				m_CallbackRegistry?.InvokeCallbacks(evt, evt.propagationPhase);
			}
			if (isIMGUIContainer && !evt.isPropagationStopped)
			{
				((IMGUIContainer)this).ProcessEvent(evt);
			}
			break;
		case PropagationPhase.AtTarget:
			if (!evt.isPropagationStopped)
			{
				m_CallbackRegistry?.InvokeCallbacks(evt, PropagationPhase.TrickleDown);
			}
			if (!evt.isPropagationStopped)
			{
				m_CallbackRegistry?.InvokeCallbacks(evt, PropagationPhase.BubbleUp);
			}
			if (isIMGUIContainer && !evt.isPropagationStopped)
			{
				((IMGUIContainer)this).ProcessEvent(evt);
			}
			break;
		case PropagationPhase.DefaultActionAtTarget:
			if (evt.isDefaultPrevented)
			{
				break;
			}
			using (new EventDebuggerLogExecuteDefaultAction(evt))
			{
				if (evt.skipDisabledElements && this is VisualElement { enabledInHierarchy: false })
				{
					ExecuteDefaultActionDisabledAtTarget(evt);
				}
				else
				{
					ExecuteDefaultActionAtTarget(evt);
				}
				break;
			}
		case PropagationPhase.DefaultAction:
			if (evt.isDefaultPrevented)
			{
				break;
			}
			using (new EventDebuggerLogExecuteDefaultAction(evt))
			{
				if (evt.skipDisabledElements && this is VisualElement { enabledInHierarchy: false })
				{
					ExecuteDefaultActionDisabled(evt);
				}
				else
				{
					ExecuteDefaultAction(evt);
				}
				break;
			}
		}
	}

	public bool HasTrickleDownHandlers()
	{
		return m_CallbackRegistry != null && m_CallbackRegistry.HasTrickleDownHandlers();
	}

	public bool HasBubbleUpHandlers()
	{
		return m_CallbackRegistry != null && m_CallbackRegistry.HasBubbleHandlers();
	}

	[EventInterest(EventInterestOptions.Inherit)]
	protected virtual void ExecuteDefaultActionAtTarget(EventBase evt)
	{
	}

	[EventInterest(EventInterestOptions.Inherit)]
	protected virtual void ExecuteDefaultAction(EventBase evt)
	{
	}

	[EventInterest(EventInterestOptions.Inherit)]
	internal virtual void ExecuteDefaultActionDisabledAtTarget(EventBase evt)
	{
	}

	[EventInterest(EventInterestOptions.Inherit)]
	internal virtual void ExecuteDefaultActionDisabled(EventBase evt)
	{
	}
}
