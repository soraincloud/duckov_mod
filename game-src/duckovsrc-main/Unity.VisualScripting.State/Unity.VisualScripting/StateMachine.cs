using UnityEngine;

namespace Unity.VisualScripting;

[AddComponentMenu("Visual Scripting/State Machine")]
[RequireComponent(typeof(Variables))]
[DisableAnnotation]
public sealed class StateMachine : EventMachine<StateGraph, StateGraphAsset>
{
	protected override void OnEnable()
	{
		if (base.hasGraph)
		{
			using Flow flow = Flow.New(base.reference);
			base.graph.Start(flow);
		}
		base.OnEnable();
	}

	protected override void OnInstantiateWhileEnabled()
	{
		if (base.hasGraph)
		{
			using Flow flow = Flow.New(base.reference);
			base.graph.Start(flow);
		}
		base.OnInstantiateWhileEnabled();
	}

	protected override void OnUninstantiateWhileEnabled()
	{
		base.OnUninstantiateWhileEnabled();
		if (base.hasGraph)
		{
			using (Flow flow = Flow.New(base.reference))
			{
				base.graph.Stop(flow);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (base.hasGraph)
		{
			using (Flow flow = Flow.New(base.reference))
			{
				base.graph.Stop(flow);
			}
		}
	}

	[ContextMenu("Show Data...")]
	protected override void ShowData()
	{
		base.ShowData();
	}

	public override StateGraph DefaultGraph()
	{
		return StateGraph.WithStart();
	}
}
