using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Lists")]
[UnitSurtitle("List")]
[UnitShortTitle("Contains Item")]
[TypeIcon(typeof(IList))]
public sealed class ListContainsItem : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput list { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput item { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput contains { get; private set; }

	protected override void Definition()
	{
		list = ValueInput<IList>("list");
		item = ValueInput<object>("item");
		contains = ValueOutput("contains", Contains);
		Requirement(list, contains);
		Requirement(item, contains);
	}

	public bool Contains(Flow flow)
	{
		IList value = flow.GetValue<IList>(list);
		object value2 = flow.GetValue<object>(item);
		return value.Contains(value2);
	}
}
