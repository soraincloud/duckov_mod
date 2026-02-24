using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting;

public sealed class GetVariable : UnifiedVariableUnit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput value { get; private set; }

	[DoNotSerialize]
	public ValueInput fallback { get; private set; }

	[Serialize]
	[Inspectable]
	[InspectorLabel("Fallback")]
	public bool specifyFallback { get; set; }

	protected override void Definition()
	{
		base.Definition();
		value = ValueOutput("value", Get).PredictableIf(IsDefined);
		Requirement(base.name, value);
		if (base.kind == VariableKind.Object)
		{
			Requirement(base.@object, value);
		}
		if (specifyFallback)
		{
			fallback = ValueInput<object>("fallback");
			Requirement(fallback, value);
		}
	}

	private bool IsDefined(Flow flow)
	{
		string variable = flow.GetValue<string>(base.name);
		if (string.IsNullOrEmpty(variable))
		{
			return false;
		}
		GameObject gameObject = null;
		if (base.kind == VariableKind.Object)
		{
			gameObject = flow.GetValue<GameObject>(base.@object);
			if (gameObject == null)
			{
				return false;
			}
		}
		Scene? scene = flow.stack.scene;
		if (base.kind == VariableKind.Scene && (!scene.HasValue || !scene.Value.IsValid() || !scene.Value.isLoaded || !Variables.ExistInScene(scene)))
		{
			return false;
		}
		return base.kind switch
		{
			VariableKind.Flow => flow.variables.IsDefined(variable), 
			VariableKind.Graph => Variables.Graph(flow.stack).IsDefined(variable), 
			VariableKind.Object => Variables.Object(gameObject).IsDefined(variable), 
			VariableKind.Scene => Variables.Scene(scene.Value).IsDefined(variable), 
			VariableKind.Application => Variables.Application.IsDefined(variable), 
			VariableKind.Saved => Variables.Saved.IsDefined(variable), 
			_ => throw new UnexpectedEnumValueException<VariableKind>(base.kind), 
		};
	}

	private object Get(Flow flow)
	{
		string variable = flow.GetValue<string>(base.name);
		VariableDeclarations variableDeclarations = base.kind switch
		{
			VariableKind.Flow => flow.variables, 
			VariableKind.Graph => Variables.Graph(flow.stack), 
			VariableKind.Object => Variables.Object(flow.GetValue<GameObject>(base.@object)), 
			VariableKind.Scene => Variables.Scene(flow.stack.scene), 
			VariableKind.Application => Variables.Application, 
			VariableKind.Saved => Variables.Saved, 
			_ => throw new UnexpectedEnumValueException<VariableKind>(base.kind), 
		};
		if (specifyFallback && !variableDeclarations.IsDefined(variable))
		{
			return flow.GetValue(fallback);
		}
		return variableDeclarations.Get(variable);
	}
}
