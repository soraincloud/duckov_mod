using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Lists")]
[UnitSurtitle("List")]
[UnitShortTitle("Get Item")]
[UnitOrder(0)]
[TypeIcon(typeof(IList))]
public sealed class GetListItem : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueInput list { get; private set; }

	[DoNotSerialize]
	public ValueInput index { get; private set; }

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput item { get; private set; }

	protected override void Definition()
	{
		list = ValueInput<IList>("list");
		index = ValueInput("index", 0);
		item = ValueOutput("item", Get);
		Requirement(list, item);
		Requirement(index, item);
	}

	public object Get(Flow flow)
	{
		IList value = flow.GetValue<IList>(list);
		int value2 = flow.GetValue<int>(index);
		return value[value2];
	}
}
