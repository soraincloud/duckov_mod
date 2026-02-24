using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Math/Vector 2")]
[UnitTitle("Project")]
public sealed class Vector2Project : Project<Vector2>
{
	public override Vector2 Operation(Vector2 a, Vector2 b)
	{
		return Vector2.Dot(a, b) * b.normalized;
	}
}
