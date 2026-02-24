using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Dictionaries")]
[UnitSurtitle("Dictionary")]
[UnitShortTitle("Add Item")]
[UnitOrder(2)]
public sealed class AddDictionaryItem : Unit
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
	public ValueInput key { get; private set; }

	[DoNotSerialize]
	public ValueInput value { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Add);
		dictionaryInput = ValueInput<IDictionary>("dictionaryInput");
		key = ValueInput<object>("key");
		value = ValueInput<object>("value");
		dictionaryOutput = ValueOutput<IDictionary>("dictionaryOutput");
		exit = ControlOutput("exit");
		Requirement(dictionaryInput, enter);
		Requirement(key, enter);
		Requirement(value, enter);
		Assignment(enter, dictionaryOutput);
		Succession(enter, exit);
	}

	private ControlOutput Add(Flow flow)
	{
		IDictionary dictionary = flow.GetValue<IDictionary>(dictionaryInput);
		object obj = flow.GetValue<object>(key);
		object obj2 = flow.GetValue<object>(value);
		flow.SetValue(dictionaryOutput, dictionary);
		dictionary.Add(obj, obj2);
		return exit;
	}
}
