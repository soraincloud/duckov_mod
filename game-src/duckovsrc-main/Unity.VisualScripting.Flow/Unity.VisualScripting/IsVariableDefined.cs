using UnityEngine;

namespace Unity.VisualScripting;

[UnitTitle("Has Variable")]
public sealed class IsVariableDefined : UnifiedVariableUnit
{
	[DoNotSerialize]
	[PortLabel("Defined")]
	[PortLabelHidden]
	[PortKey("isDefined")]
	public ValueOutput isVariableDefined { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		isVariableDefined = ValueOutput("isDefined", IsDefined);
		Requirement(base.name, isVariableDefined);
		if (base.kind == VariableKind.Object)
		{
			Requirement(base.@object, isVariableDefined);
		}
	}

	private bool IsDefined(Flow flow)
	{
		string value = flow.GetValue<string>(base.name);
		return base.kind switch
		{
			VariableKind.Flow => flow.variables.IsDefined(value), 
			VariableKind.Graph => Variables.Graph(flow.stack).IsDefined(value), 
			VariableKind.Object => Variables.Object(flow.GetValue<GameObject>(base.@object)).IsDefined(value), 
			VariableKind.Scene => Variables.Scene(flow.stack.scene).IsDefined(value), 
			VariableKind.Application => Variables.Application.IsDefined(value), 
			VariableKind.Saved => Variables.Saved.IsDefined(value), 
			_ => throw new UnexpectedEnumValueException<VariableKind>(base.kind), 
		};
	}
}
