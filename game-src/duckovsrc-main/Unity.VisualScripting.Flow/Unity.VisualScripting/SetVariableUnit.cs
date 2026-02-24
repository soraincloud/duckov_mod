namespace Unity.VisualScripting;

[UnitShortTitle("Set Variable")]
public abstract class SetVariableUnit : VariableUnit
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

	protected SetVariableUnit()
	{
	}

	protected SetVariableUnit(string defaultName)
		: base(defaultName)
	{
	}

	protected override void Definition()
	{
		base.Definition();
		assign = ControlInput("assign", Assign);
		input = ValueInput<object>("input");
		output = ValueOutput<object>("output");
		assigned = ControlOutput("assigned");
		Requirement(input, assign);
		Requirement(base.name, assign);
		Assignment(assign, output);
		Succession(assign, assigned);
	}

	protected virtual ControlOutput Assign(Flow flow)
	{
		object value = flow.GetValue<object>(input);
		string value2 = flow.GetValue<string>(base.name);
		GetDeclarations(flow).Set(value2, value);
		flow.SetValue(output, value);
		return assigned;
	}
}
