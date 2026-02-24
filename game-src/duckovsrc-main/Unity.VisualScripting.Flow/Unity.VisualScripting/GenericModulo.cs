namespace Unity.VisualScripting;

[UnitCategory("Math/Generic")]
[UnitTitle("Modulo")]
public sealed class GenericModulo : Modulo<object>
{
	public override object Operation(object a, object b)
	{
		return OperatorUtility.Modulo(a, b);
	}
}
