using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Dictionaries")]
[UnitSurtitle("Dictionary")]
[UnitShortTitle("Clear")]
[UnitOrder(4)]
[TypeIcon(typeof(RemoveDictionaryItem))]
public sealed class ClearDictionary : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabel("Dictionary")]
	[PortLabelHidden]
	public ValueInput dictionaryInput { get; private set; }

	[DoNotSerialize]
	[PortLabel("Dictionary")]
	[PortLabelHidden]
	public ValueOutput dictionaryOutput { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Clear);
		dictionaryInput = ValueInput<IDictionary>("dictionaryInput");
		dictionaryOutput = ValueOutput<IDictionary>("dictionaryOutput");
		exit = ControlOutput("exit");
		Requirement(dictionaryInput, enter);
		Assignment(enter, dictionaryOutput);
		Succession(enter, exit);
	}

	private ControlOutput Clear(Flow flow)
	{
		IDictionary value = flow.GetValue<IDictionary>(dictionaryInput);
		flow.SetValue(dictionaryOutput, value);
		value.Clear();
		return exit;
	}
}
