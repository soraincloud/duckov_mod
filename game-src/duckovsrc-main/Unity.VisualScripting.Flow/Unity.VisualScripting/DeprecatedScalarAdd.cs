using System;

namespace Unity.VisualScripting;

[UnitCategory("Math/Scalar")]
[UnitTitle("Add")]
[Obsolete("Use the new \"Add (Math/Scalar)\" node instead.")]
[RenamedFrom("Bolt.ScalarAdd")]
[RenamedFrom("Unity.VisualScripting.ScalarAdd")]
public sealed class DeprecatedScalarAdd : Add<float>
{
	protected override float defaultB => 1f;

	public override float Operation(float a, float b)
	{
		return a + b;
	}
}
