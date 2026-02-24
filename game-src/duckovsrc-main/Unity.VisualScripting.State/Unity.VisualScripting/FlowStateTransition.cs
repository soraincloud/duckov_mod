using System;
using UnityEngine;

namespace Unity.VisualScripting;

[SerializationVersion("A", new Type[] { })]
public sealed class FlowStateTransition : NesterStateTransition<FlowGraph, ScriptGraphAsset>, IGraphEventListener
{
	public FlowStateTransition()
	{
	}

	public FlowStateTransition(IState source, IState destination)
		: base(source, destination)
	{
		if (!source.canBeSource)
		{
			throw new InvalidOperationException("Source state cannot emit transitions.");
		}
		if (!destination.canBeDestination)
		{
			throw new InvalidOperationException("Destination state cannot receive transitions.");
		}
	}

	public static FlowStateTransition WithDefaultTrigger(IState source, IState destination)
	{
		FlowStateTransition flowStateTransition = new FlowStateTransition(source, destination);
		flowStateTransition.nest.source = GraphSource.Embed;
		flowStateTransition.nest.embed = GraphWithDefaultTrigger();
		return flowStateTransition;
	}

	public static FlowGraph GraphWithDefaultTrigger()
	{
		return new FlowGraph
		{
			units = { (IUnit)new TriggerStateTransition
			{
				position = new Vector2(100f, -50f)
			} }
		};
	}

	public override void OnEnter(Flow flow)
	{
		if (flow.stack.TryEnterParentElement(this))
		{
			flow.stack.TriggerEventHandler((EventHook hook) => hook == "OnEnterState", default(EmptyEventArgs), (IGraphParentElement parent) => parent is SubgraphUnit, force: false);
			flow.stack.ExitParentElement();
		}
	}

	public override void OnExit(Flow flow)
	{
		if (flow.stack.TryEnterParentElement(this))
		{
			flow.stack.TriggerEventHandler((EventHook hook) => hook == "OnExitState", default(EmptyEventArgs), (IGraphParentElement parent) => parent is SubgraphUnit, force: false);
			base.nest.graph.StopListening(flow.stack);
			flow.stack.ExitParentElement();
		}
	}

	public void StartListening(GraphStack stack)
	{
		if (stack.TryEnterParentElement(this))
		{
			base.nest.graph.StartListening(stack);
			stack.ExitParentElement();
		}
	}

	public void StopListening(GraphStack stack)
	{
		if (stack.TryEnterParentElement(this))
		{
			base.nest.graph.StopListening(stack);
			stack.ExitParentElement();
		}
	}

	public bool IsListening(GraphPointer pointer)
	{
		return pointer.GetElementData<State.Data>(base.source).isActive;
	}

	public override FlowGraph DefaultGraph()
	{
		return GraphWithDefaultTrigger();
	}
}
