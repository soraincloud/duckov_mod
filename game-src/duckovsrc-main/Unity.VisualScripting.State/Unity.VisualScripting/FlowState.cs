using System;
using System.ComponentModel;
using UnityEngine;

namespace Unity.VisualScripting;

[SerializationVersion("A", new Type[] { })]
[TypeIcon(typeof(FlowGraph))]
[DisplayName("Script State")]
public sealed class FlowState : NesterState<FlowGraph, ScriptGraphAsset>, IGraphEventListener
{
	public FlowState()
	{
	}

	public FlowState(ScriptGraphAsset macro)
		: base(macro)
	{
	}

	protected override void OnEnterImplementation(Flow flow)
	{
		if (flow.stack.TryEnterParentElement(this))
		{
			base.nest.graph.StartListening(flow.stack);
			flow.stack.TriggerEventHandler((EventHook hook) => hook == "OnEnterState", default(EmptyEventArgs), (IGraphParentElement parent) => parent is SubgraphUnit, force: false);
			flow.stack.ExitParentElement();
		}
	}

	protected override void OnExitImplementation(Flow flow)
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
		return pointer.GetElementData<Data>(this).isActive;
	}

	public override FlowGraph DefaultGraph()
	{
		return GraphWithEnterUpdateExit();
	}

	public static FlowState WithEnterUpdateExit()
	{
		FlowState flowState = new FlowState();
		flowState.nest.source = GraphSource.Embed;
		flowState.nest.embed = GraphWithEnterUpdateExit();
		return flowState;
	}

	public static FlowGraph GraphWithEnterUpdateExit()
	{
		return new FlowGraph
		{
			units = 
			{
				(IUnit)new OnEnterState
				{
					position = new Vector2(-205f, -215f)
				},
				(IUnit)new Update
				{
					position = new Vector2(-161f, -38f)
				},
				(IUnit)new OnExitState
				{
					position = new Vector2(-205f, 145f)
				}
			}
		};
	}
}
