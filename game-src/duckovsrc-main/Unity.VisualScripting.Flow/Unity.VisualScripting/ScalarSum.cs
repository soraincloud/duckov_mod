using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting;

[UnitCategory("Math/Scalar")]
[UnitTitle("Add")]
public sealed class ScalarSum : Sum<float>, IDefaultValue<float>
{
	[DoNotSerialize]
	public float defaultValue => 1f;

	public override float Operation(float a, float b)
	{
		return a + b;
	}

	public override float Operation(IEnumerable<float> values)
	{
		return values.Sum();
	}
}
