using System;
using UnityEngine;

namespace Unity.VisualScripting;

[UnitCategory("Logic")]
[UnitShortTitle("Equal")]
[UnitSubtitle("(Approximately)")]
[UnitOrder(7)]
[Obsolete("Use the Equal node with Numeric enabled instead.")]
public sealed class ApproximatelyEqual : Unit
{
	[DoNotSerialize]
	public ValueInput a { get; private set; }

	[DoNotSerialize]
	public ValueInput b { get; private set; }

	[DoNotSerialize]
	[PortLabel("A ≈ B")]
	public ValueOutput equal { get; private set; }

	protected override void Definition()
	{
		a = ValueInput<float>("a");
		b = ValueInput("b", 0f);
		equal = ValueOutput("equal", Comparison).Predictable();
		Requirement(a, equal);
		Requirement(b, equal);
	}

	public bool Comparison(Flow flow)
	{
		return Mathf.Approximately(flow.GetValue<float>(a), flow.GetValue<float>(b));
	}
}
