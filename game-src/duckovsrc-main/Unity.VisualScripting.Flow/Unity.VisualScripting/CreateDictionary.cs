using System.Collections;

namespace Unity.VisualScripting;

[UnitCategory("Collections/Dictionaries")]
[UnitOrder(-1)]
[TypeIcon(typeof(IDictionary))]
[RenamedFrom("Bolt.CreateDitionary")]
public sealed class CreateDictionary : Unit
{
	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput dictionary { get; private set; }

	protected override void Definition()
	{
		dictionary = ValueOutput("dictionary", Create);
	}

	public IDictionary Create(Flow flow)
	{
		return new AotDictionary();
	}
}
