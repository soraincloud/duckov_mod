namespace Unity.VisualScripting;

[UnitCategory("Logic")]
[UnitOrder(11)]
public sealed class Greater : BinaryComparisonUnit
{
	[PortLabel("A > B")]
	public override ValueOutput comparison => base.comparison;

	protected override bool NumericComparison(float a, float b)
	{
		return a > b;
	}

	protected override bool GenericComparison(object a, object b)
	{
		return OperatorUtility.GreaterThan(a, b);
	}
}
