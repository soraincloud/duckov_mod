using System;
using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Lists")]
[UnitSurtitle("List")]
[UnitShortTitle("Remove Item")]
[UnitOrder(4)]
public sealed class RemoveListItem : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ControlInput enter { get; private set; }

	[DoNotSerialize]
	[PortLabel("List")]
	[PortLabelHidden]
	public ValueInput listInput { get; private set; }

	[DoNotSerialize]
	[PortLabel("List")]
	[PortLabelHidden]
	public ValueOutput listOutput { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput item { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Remove);
		listInput = ValueInput<IList>("listInput");
		listOutput = ValueOutput<IList>("listOutput");
		item = ValueInput<object>("item");
		exit = ControlOutput("exit");
		Requirement(listInput, enter);
		Requirement(item, enter);
		Assignment(enter, listOutput);
		Succession(enter, exit);
	}

	public ControlOutput Remove(Flow flow)
	{
		IList value = flow.GetValue<IList>(listInput);
		object value2 = flow.GetValue<object>(item);
		if (value is Array)
		{
			ArrayList arrayList = new ArrayList(value);
			arrayList.Remove(value2);
			flow.SetValue(listOutput, arrayList.ToArray(value.GetType().GetElementType()));
		}
		else
		{
			value.Remove(value2);
			flow.SetValue(listOutput, value);
		}
		return exit;
	}
}
