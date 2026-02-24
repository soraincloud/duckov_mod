namespace Unity.VisualScripting;

[UnitTitle("Break Loop")]
[UnitCategory("Control")]
[UnitOrder(13)]
public class Break : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Operation);
	}

	public ControlOutput Operation(Flow flow)
	{
		flow.BreakLoop();
		return null;
	}
}
