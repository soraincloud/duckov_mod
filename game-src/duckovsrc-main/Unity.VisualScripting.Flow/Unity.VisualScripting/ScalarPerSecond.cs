using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Math/Scalar")]
[UnitTitle("Per Second")]
public sealed class ScalarPerSecond : PerSecond<float>
{
	public override float Operation(float input)
	{
		return input * Time.deltaTime;
	}
}
