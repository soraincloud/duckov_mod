using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Nulls")]
[TypeIcon(typeof(Null))]
public sealed class NullCheck : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput input { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabel("Not Null")]
	public ControlOutput ifNotNull { get; private set; }

	[DoNotSerialize]
	[PortLabel("Null")]
	public ControlOutput ifNull { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Enter);
		input = ValueInput<object>("input").AllowsNull();
		ifNotNull = ControlOutput("ifNotNull");
		ifNull = ControlOutput("ifNull");
		Requirement(input, enter);
		Succession(enter, ifNotNull);
		Succession(enter, ifNull);
	}

	public ControlOutput Enter(Flow flow)
	{
		object value = flow.GetValue(input);
		if ((!(value is Object)) ? (value == null) : ((Object)value == null))
		{
			return ifNull;
		}
		return ifNotNull;
	}
}
