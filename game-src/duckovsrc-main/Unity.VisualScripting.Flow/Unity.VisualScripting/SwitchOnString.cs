using System;

namespace Unity.VisualScripting;

[UnitCategory("Control")]
[UnitTitle("Switch On String")]
[UnitShortTitle("Switch")]
[UnitSubtitle("On String")]
[UnitOrder(4)]
public class SwitchOnString : SwitchUnit<string>
{
	[Serialize]
	[Inspectable]
	[UnitHeaderInspectable("Ignore Case")]
	[InspectorToggleLeft]
	public bool ignoreCase { get; set; }

	protected override bool Matches(string a, string b)
	{
		if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
		{
			return true;
		}
		return string.Equals(a, b, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}
}
