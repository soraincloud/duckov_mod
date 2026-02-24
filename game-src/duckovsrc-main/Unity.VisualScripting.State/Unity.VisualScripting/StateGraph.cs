using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting;

[SerializationVersion("A", new Type[] { })]
public sealed class StateGraph : Graph, IGraphEventListener
{
	[DoNotSerialize]
	public GraphElementCollection<IState> states { get; internal set; }

	[DoNotSerialize]
	public GraphConnectionCollection<IStateTransition, IState, IState> transitions { get; internal set; }

	[DoNotSerialize]
	public GraphElementCollection<GraphGroup> groups { get; internal set; }

	[DoNotSerialize]
	public GraphElementCollection<StickyNote> sticky { get; private set; }

	public StateGraph()
	{
		states = new GraphElementCollection<IState>(this);
		transitions = new GraphConnectionCollection<IStateTransition, IState, IState>(this);
		groups = new GraphElementCollection<GraphGroup>(this);
		sticky = new GraphElementCollection<StickyNote>(this);
		base.elements.Include(states);
		base.elements.Include(transitions);
		base.elements.Include(groups);
		base.elements.Include(sticky);
	}

	public override IGraphData CreateData()
	{
		return new StateGraphData(this);
	}

	public void StartListening(GraphStack stack)
	{
		stack.GetGraphData<StateGraphData>().isListening = true;
		HashSet<IState> activeStatesNoAlloc = GetActiveStatesNoAlloc(stack);
		foreach (IState item in activeStatesNoAlloc)
		{
			(item as IGraphEventListener)?.StartListening(stack);
		}
		activeStatesNoAlloc.Free();
	}

	public void StopListening(GraphStack stack)
	{
		HashSet<IState> activeStatesNoAlloc = GetActiveStatesNoAlloc(stack);
		foreach (IState item in activeStatesNoAlloc)
		{
			(item as IGraphEventListener)?.StopListening(stack);
		}
		activeStatesNoAlloc.Free();
		stack.GetGraphData<StateGraphData>().isListening = false;
	}

	public bool IsListening(GraphPointer pointer)
	{
		return pointer.GetGraphData<StateGraphData>().isListening;
	}

	private HashSet<IState> GetActiveStatesNoAlloc(GraphPointer pointer)
	{
		HashSet<IState> hashSet = HashSetPool<IState>.New();
		foreach (IState state in states)
		{
			if (pointer.GetElementData<State.Data>(state).isActive)
			{
				hashSet.Add(state);
			}
		}
		return hashSet;
	}

	public void Start(Flow flow)
	{
		flow.stack.GetGraphData<StateGraphData>().isListening = true;
		foreach (IState item in states.Where((IState s) => s.isStart))
		{
			try
			{
				item.OnEnter(flow, StateEnterReason.Start);
			}
			catch (Exception ex)
			{
				item.HandleException(flow.stack, ex);
				throw;
			}
		}
	}

	public void Stop(Flow flow)
	{
		HashSet<IState> activeStatesNoAlloc = GetActiveStatesNoAlloc(flow.stack);
		foreach (IState item in activeStatesNoAlloc)
		{
			try
			{
				item.OnExit(flow, StateExitReason.Stop);
			}
			catch (Exception ex)
			{
				item.HandleException(flow.stack, ex);
				throw;
			}
		}
		activeStatesNoAlloc.Free();
		flow.stack.GetGraphData<StateGraphData>().isListening = false;
	}

	public static StateGraph WithStart()
	{
		StateGraph stateGraph = new StateGraph();
		FlowState flowState = FlowState.WithEnterUpdateExit();
		flowState.isStart = true;
		flowState.nest.embed.title = "Start";
		flowState.position = new Vector2(-86f, -15f);
		stateGraph.states.Add(flowState);
		return stateGraph;
	}
}
