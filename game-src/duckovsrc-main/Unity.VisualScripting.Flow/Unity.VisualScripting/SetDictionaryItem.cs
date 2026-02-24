using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Dictionaries")]
[UnitSurtitle("Dictionary")]
[UnitShortTitle("Set Item")]
[UnitOrder(1)]
[TypeIcon(typeof(IDictionary))]
public sealed class SetDictionaryItem : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput dictionary { get; private set; }

	[DoNotSerialize]
	public ValueInput key { get; private set; }

	[DoNotSerialize]
	public ValueInput value { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Set);
		dictionary = ValueInput<IDictionary>("dictionary");
		key = ValueInput<object>("key");
		value = ValueInput<object>("value");
		exit = ControlOutput("exit");
		Requirement(dictionary, enter);
		Requirement(key, enter);
		Requirement(value, enter);
		Succession(enter, exit);
	}

	public ControlOutput Set(Flow flow)
	{
		IDictionary obj = flow.GetValue<IDictionary>(dictionary);
		object obj2 = flow.GetValue<object>(key);
		object obj3 = flow.GetValue<object>(value);
		obj[obj2] = obj3;
		return exit;
	}
}
