using UnityEngine;

namespace Unity.VisualScripting;

[SpecialUnit]
[RenamedFrom("Bolt.Self")]
[RenamedFrom("Unity.VisualScripting.Self")]
public sealed class This : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	[PortLabel("This")]
	public ValueOutput self { get; private set; }

	protected override void Definition()
	{
		self = ValueOutput("self", Result).PredictableIf(IsPredictable);
	}

	private GameObject Result(Flow flow)
	{
		return flow.stack.self;
	}

	private bool IsPredictable(Flow flow)
	{
		return flow.stack.self != null;
	}
}
