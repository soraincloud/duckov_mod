using UnityEngine;

namespace Unity.VisualScripting;

[UnitShortTitle("Set Variable")]
public sealed class SetVariable : UnifiedVariableUnit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput assign { get; set; }

	[DoNotSerialize]
	[PortLabel("New Value")]
	[PortLabelHidden]
	public ValueInput input { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput assigned { get; set; }

	[DoNotSerialize]
	[PortLabel("Value")]
	[PortLabelHidden]
	public ValueOutput output { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		assign = ControlInput("assign", Assign);
		input = ValueInput<object>("input").AllowsNull();
		output = ValueOutput<object>("output");
		assigned = ControlOutput("assigned");
		Requirement(base.name, assign);
		Requirement(input, assign);
		Assignment(assign, output);
		Succession(assign, assigned);
		if (base.kind == VariableKind.Object)
		{
			Requirement(base.@object, assign);
		}
	}

	private ControlOutput Assign(Flow flow)
	{
		string value = flow.GetValue<string>(base.name);
		object value2 = flow.GetValue(input);
		switch (base.kind)
		{
		case VariableKind.Flow:
			flow.variables.Set(value, value2);
			break;
		case VariableKind.Graph:
			Variables.Graph(flow.stack).Set(value, value2);
			break;
		case VariableKind.Object:
			Variables.Object(flow.GetValue<GameObject>(base.@object)).Set(value, value2);
			break;
		case VariableKind.Scene:
			Variables.Scene(flow.stack.scene).Set(value, value2);
			break;
		case VariableKind.Application:
			Variables.Application.Set(value, value2);
			break;
		case VariableKind.Saved:
			Variables.Saved.Set(value, value2);
			break;
		default:
			throw new UnexpectedEnumValueException<VariableKind>(base.kind);
		}
		flow.SetValue(output, value2);
		return assigned;
	}
}
