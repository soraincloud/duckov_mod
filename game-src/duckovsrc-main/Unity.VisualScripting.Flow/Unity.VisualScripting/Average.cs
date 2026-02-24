using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting;

[UnitOrder(304)]
public abstract class Average<T> : MultiInputUnit<T>
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput average { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		average = ValueOutput("average", Operation).Predictable();
		foreach (ValueInput multiInput in base.multiInputs)
		{
			Requirement(multiInput, average);
		}
	}

	public abstract T Operation(T a, T b);

	public abstract T Operation(IEnumerable<T> values);

	public T Operation(Flow flow)
	{
		if (inputCount == 2)
		{
			return Operation(flow.GetValue<T>(base.multiInputs[0]), flow.GetValue<T>(base.multiInputs[1]));
		}
		return Operation(base.multiInputs.Select(flow.GetValue<T>));
	}
}
