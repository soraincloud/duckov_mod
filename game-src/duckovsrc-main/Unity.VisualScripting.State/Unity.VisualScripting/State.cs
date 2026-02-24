using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting;

public abstract class State : GraphElement<StateGraph>, IState, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable, IGraphElementWithData
{
	public class Data : IGraphElementData
	{
		public bool isActive;

		public bool hasEntered;
	}

	public class DebugData : IStateDebugData, IGraphElementDebugData
	{
		public int lastEnterFrame { get; set; }

		public float lastExitTime { get; set; }

		public Exception runtimeException { get; set; }
	}

	public const float DefaultWidth = 170f;

	[Serialize]
	public bool isStart { get; set; }

	[DoNotSerialize]
	public virtual bool canBeSource => true;

	[DoNotSerialize]
	public virtual bool canBeDestination => true;

	public IEnumerable<IStateTransition> outgoingTransitions => base.graph?.transitions.WithSource(this) ?? Enumerable.Empty<IStateTransition>();

	public IEnumerable<IStateTransition> incomingTransitions => base.graph?.transitions.WithDestination(this) ?? Enumerable.Empty<IStateTransition>();

	protected List<IStateTransition> outgoingTransitionsNoAlloc => base.graph?.transitions.WithSourceNoAlloc(this) ?? Empty<IStateTransition>.list;

	public IEnumerable<IStateTransition> transitions => LinqUtility.Concat<IStateTransition>(new IEnumerable[2] { outgoingTransitions, incomingTransitions });

	[Serialize]
	public Vector2 position { get; set; }

	[Serialize]
	public float width { get; set; } = 170f;

	public IGraphElementData CreateData()
	{
		return new Data();
	}

	public IGraphElementDebugData CreateDebugData()
	{
		return new DebugData();
	}

	public override void BeforeRemove()
	{
		base.BeforeRemove();
		Disconnect();
	}

	public override void Instantiate(GraphReference instance)
	{
		base.Instantiate(instance);
		Data elementData = instance.GetElementData<Data>(this);
		if (this is IGraphEventListener listener && elementData.isActive)
		{
			listener.StartListening(instance);
		}
		else if (isStart && !elementData.hasEntered && base.graph.IsListening(instance))
		{
			using (Flow flow = Flow.New(instance))
			{
				OnEnter(flow, StateEnterReason.Start);
			}
		}
	}

	public override void Uninstantiate(GraphReference instance)
	{
		if (this is IGraphEventListener listener)
		{
			listener.StopListening(instance);
		}
		base.Uninstantiate(instance);
	}

	protected void CopyFrom(State source)
	{
		CopyFrom((GraphElement<StateGraph>)source);
		isStart = source.isStart;
		width = source.width;
	}

	public void Disconnect()
	{
		IStateTransition[] array = transitions.ToArray();
		foreach (IStateTransition item in array)
		{
			base.graph.transitions.Remove(item);
		}
	}

	public virtual void OnEnter(Flow flow, StateEnterReason reason)
	{
		Data elementData = flow.stack.GetElementData<Data>(this);
		if (elementData.isActive)
		{
			return;
		}
		elementData.isActive = true;
		elementData.hasEntered = true;
		foreach (IStateTransition item in outgoingTransitionsNoAlloc)
		{
			(item as IGraphEventListener)?.StartListening(flow.stack);
		}
		if (flow.enableDebug)
		{
			flow.stack.GetElementDebugData<DebugData>(this).lastEnterFrame = EditorTimeBinding.frame;
		}
		OnEnterImplementation(flow);
		foreach (IStateTransition item2 in outgoingTransitionsNoAlloc)
		{
			try
			{
				item2.OnEnter(flow);
			}
			catch (Exception ex)
			{
				item2.HandleException(flow.stack, ex);
				throw;
			}
		}
	}

	public virtual void OnExit(Flow flow, StateExitReason reason)
	{
		Data elementData = flow.stack.GetElementData<Data>(this);
		if (!elementData.isActive)
		{
			return;
		}
		OnExitImplementation(flow);
		elementData.isActive = false;
		if (flow.enableDebug)
		{
			flow.stack.GetElementDebugData<DebugData>(this).lastExitTime = EditorTimeBinding.time;
		}
		foreach (IStateTransition item in outgoingTransitionsNoAlloc)
		{
			try
			{
				item.OnExit(flow);
			}
			catch (Exception ex)
			{
				item.HandleException(flow.stack, ex);
				throw;
			}
		}
	}

	protected virtual void OnEnterImplementation(Flow flow)
	{
	}

	protected virtual void UpdateImplementation(Flow flow)
	{
	}

	protected virtual void FixedUpdateImplementation(Flow flow)
	{
	}

	protected virtual void LateUpdateImplementation(Flow flow)
	{
	}

	protected virtual void OnExitImplementation(Flow flow)
	{
	}

	public virtual void OnBranchTo(Flow flow, IState destination)
	{
	}

	public override AnalyticsIdentifier GetAnalyticsIdentifier()
	{
		AnalyticsIdentifier obj = new AnalyticsIdentifier
		{
			Identifier = GetType().FullName,
			Namespace = GetType().Namespace
		};
		obj.Hashcode = obj.Identifier.GetHashCode();
		return obj;
	}

	StateGraph IState.get_graph()
	{
		return base.graph;
	}
}
