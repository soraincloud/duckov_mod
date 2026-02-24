using System;

namespace Unity.VisualScripting;

[UnitCategory("Control")]
[UnitOrder(0)]
[RenamedFrom("Bolt.Branch")]
[RenamedFrom("Unity.VisualScripting.Branch")]
public sealed class If : Unit, IBranchUnit, IUnit, IGraphElementWithDebugData, IGraphElement, IGraphItem, INotifiedCollectionItem, IDisposable, IPrewarmable, IAotStubbable, IIdentifiable, IAnalyticsIdentifiable
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput condition { get; private set; }

	[DoNotSerialize]
	[PortLabel("True")]
	public ControlOutput ifTrue { get; private set; }

	[DoNotSerialize]
	[PortLabel("False")]
	public ControlOutput ifFalse { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Enter);
		condition = ValueInput<bool>("condition");
		ifTrue = ControlOutput("ifTrue");
		ifFalse = ControlOutput("ifFalse");
		Requirement(condition, enter);
		Succession(enter, ifTrue);
		Succession(enter, ifFalse);
	}

	public ControlOutput Enter(Flow flow)
	{
		if (!flow.GetValue<bool>(condition))
		{
			return ifFalse;
		}
		return ifTrue;
	}

	FlowGraph IUnit.get_graph()
	{
		return base.graph;
	}
}
