using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Math/Vector 2")]
[UnitTitle("Multiply")]
public sealed class Vector2Multiply : Multiply<Vector2>
{
	protected override Vector2 defaultB => Vector2.zero;

	public override Vector2 Operation(Vector2 a, Vector2 b)
	{
		return new Vector2(a.x * b.x, a.y * b.y);
	}
}
