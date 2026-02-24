namespace Unity.VisualScripting;

[UnitCategory("Logic")]
[UnitOrder(3)]
public sealed class Negate : Unit
{
	[DoNotSerialize]
	[PortLabel("X")]
	public ValueInput input { get; private set; }

	[DoNotSerialize]
	[PortLabel("~X")]
	public ValueOutput output { get; private set; }

	protected override void Definition()
	{
		input = ValueInput<bool>("input");
		output = ValueOutput("output", Operation).Predictable();
		Requirement(input, output);
	}

	public bool Operation(Flow flow)
	{
		return !flow.GetValue<bool>(input);
	}
}
