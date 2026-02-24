namespace Unity.VisualScripting;

[UnitOrder(104)]
public abstract class Divide<T> : Unit
{
	[DoNotSerialize]
	[PortLabel("A")]
	public ValueInput dividend { get; private set; }

	[DoNotSerialize]
	[PortLabel("B")]
	public ValueInput divisor { get; private set; }

	[DoNotSerialize]
	[PortLabel("A ÷ B")]
	public ValueOutput quotient { get; private set; }

	[DoNotSerialize]
	protected virtual T defaultDivisor => default(T);

	[DoNotSerialize]
	protected virtual T defaultDividend => default(T);

	protected override void Definition()
	{
		dividend = ValueInput("dividend", defaultDividend);
		divisor = ValueInput("divisor", defaultDivisor);
		quotient = ValueOutput("quotient", Operation).Predictable();
		Requirement(dividend, quotient);
		Requirement(divisor, quotient);
	}

	public abstract T Operation(T divident, T divisor);

	public T Operation(Flow flow)
	{
		return Operation(flow.GetValue<T>(dividend), flow.GetValue<T>(divisor));
	}
}
