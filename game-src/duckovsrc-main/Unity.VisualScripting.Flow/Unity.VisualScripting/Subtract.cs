namespace Unity.VisualScripting;

[UnitOrder(102)]
public abstract class Subtract<T> : Unit
{
	[DoNotSerialize]
	[PortLabel("A")]
	public ValueInput minuend { get; private set; }

	[DoNotSerialize]
	[PortLabel("B")]
	public ValueInput subtrahend { get; private set; }

	[DoNotSerialize]
	[PortLabel("A − B")]
	public ValueOutput difference { get; private set; }

	[DoNotSerialize]
	protected virtual T defaultMinuend => default(T);

	[DoNotSerialize]
	protected virtual T defaultSubtrahend => default(T);

	protected override void Definition()
	{
		minuend = ValueInput("minuend", defaultMinuend);
		subtrahend = ValueInput("subtrahend", defaultSubtrahend);
		difference = ValueOutput("difference", Operation).Predictable();
		Requirement(minuend, difference);
		Requirement(subtrahend, difference);
	}

	public abstract T Operation(T a, T b);

	public T Operation(Flow flow)
	{
		return Operation(flow.GetValue<T>(minuend), flow.GetValue<T>(subtrahend));
	}
}
