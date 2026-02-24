namespace Unity.VisualScripting;

[TypeIcon(typeof(StateGraph))]
[UnitCategory("Nesting")]
public sealed class StateUnit : NesterUnit<StateGraph, StateGraphAsset>
{
	[DoNotSerialize]
	public ControlInput start { get; private set; }

	[DoNotSerialize]
	public ControlInput stop { get; private set; }

	[DoNotSerialize]
	public ControlOutput started { get; private set; }

	[DoNotSerialize]
	public ControlOutput stopped { get; private set; }

	public StateUnit()
	{
	}

	public StateUnit(StateGraphAsset macro)
		: base(macro)
	{
	}

	public static StateUnit WithStart()
	{
		StateUnit stateUnit = new StateUnit();
		stateUnit.nest.source = GraphSource.Embed;
		stateUnit.nest.embed = StateGraph.WithStart();
		return stateUnit;
	}

	protected override void Definition()
	{
		start = ControlInput("start", Start);
		stop = ControlInput("stop", Stop);
		started = ControlOutput("started");
		stopped = ControlOutput("stopped");
		Succession(start, started);
		Succession(stop, stopped);
	}

	private ControlOutput Start(Flow flow)
	{
		flow.stack.EnterParentElement(this);
		base.nest.graph.Start(flow);
		flow.stack.ExitParentElement();
		return started;
	}

	private ControlOutput Stop(Flow flow)
	{
		flow.stack.EnterParentElement(this);
		base.nest.graph.Stop(flow);
		flow.stack.ExitParentElement();
		return stopped;
	}

	public override StateGraph DefaultGraph()
	{
		return StateGraph.WithStart();
	}
}
