using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Math/Vector 4")]
[UnitTitle("Distance")]
public sealed class Vector4Distance : Distance<Vector4>
{
	public override float Operation(Vector4 a, Vector4 b)
	{
		return Vector4.Distance(a, b);
	}
}
