using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Math/Vector 3")]
[UnitTitle("Dot Product")]
public sealed class Vector3DotProduct : DotProduct<Vector3>
{
	public override float Operation(Vector3 a, Vector3 b)
	{
		return Vector3.Dot(a, b);
	}
}
