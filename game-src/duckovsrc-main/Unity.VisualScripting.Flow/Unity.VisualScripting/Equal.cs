using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Logic")]
[UnitOrder(5)]
public sealed class Equal : BinaryComparisonUnit
{
	protected override string outputKey => "equal";

	[DoNotSerialize]
	[PortLabel("A = B")]
	[PortKey("equal")]
	public override ValueOutput comparison => base.comparison;

	public Equal()
	{
		base.numeric = false;
	}

	protected override bool NumericComparison(float a, float b)
	{
		return Mathf.Approximately(a, b);
	}

	protected override bool GenericComparison(object a, object b)
	{
		return OperatorUtility.Equal(a, b);
	}
}
