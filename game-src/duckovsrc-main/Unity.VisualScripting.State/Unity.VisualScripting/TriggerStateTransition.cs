namespace Unity.VisualScripting;

[UnitSurtitle("State")]
[UnitCategory("Nesting")]
[UnitShortTitle("Trigger Transition")]
[TypeIcon(typeof(IStateTransition))]
public sealed class TriggerStateTransition : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput trigger { get; private set; }

	protected override void Definition()
	{
		trigger = ControlInput("trigger", Trigger);
	}

	private ControlOutput Trigger(Flow flow)
	{
		INesterStateTransition parent = flow.stack.GetParent<INesterStateTransition>();
		flow.stack.ExitParentElement();
		parent.Branch(flow);
		return null;
	}
}
