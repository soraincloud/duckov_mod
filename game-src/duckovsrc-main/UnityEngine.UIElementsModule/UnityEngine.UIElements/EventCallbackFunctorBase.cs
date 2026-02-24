using System;

namespace UnityEngine.UIElements;

internal abstract class EventCallbackFunctorBase
{
	public CallbackPhase phase { get; }

	public InvokePolicy invokePolicy { get; }

	protected EventCallbackFunctorBase(CallbackPhase phase, InvokePolicy invokePolicy)
	{
		this.phase = phase;
		this.invokePolicy = invokePolicy;
	}

	public abstract void Invoke(EventBase evt, PropagationPhase propagationPhase);

	public abstract bool IsEquivalentTo(long eventTypeId, Delegate callback, CallbackPhase phase);

	protected bool PhaseMatches(PropagationPhase propagationPhase)
	{
		switch (phase)
		{
		case CallbackPhase.TrickleDownAndTarget:
			if (propagationPhase != PropagationPhase.TrickleDown && propagationPhase != PropagationPhase.AtTarget)
			{
				return false;
			}
			break;
		case CallbackPhase.TargetAndBubbleUp:
			if (propagationPhase != PropagationPhase.AtTarget && propagationPhase != PropagationPhase.BubbleUp)
			{
				return false;
			}
			break;
		}
		return true;
	}
}
