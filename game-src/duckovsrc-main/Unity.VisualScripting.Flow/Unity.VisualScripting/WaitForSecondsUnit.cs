using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting;

[UnitTitle("Wait For Seconds")]
[UnitOrder(1)]
public class WaitForSecondsUnit : WaitUnit
{
	[DoNotSerialize]
	[PortLabel("Delay")]
	public ValueInput seconds { get; private set; }

	[DoNotSerialize]
	[PortLabel("Unscaled")]
	public ValueInput unscaledTime { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		seconds = ValueInput("seconds", 0f);
		unscaledTime = ValueInput("unscaledTime", @default: false);
		Requirement(seconds, base.enter);
		Requirement(unscaledTime, base.enter);
	}

	protected override IEnumerator Await(Flow flow)
	{
		float value = flow.GetValue<float>(seconds);
		if (flow.GetValue<bool>(unscaledTime))
		{
			yield return new WaitForSecondsRealtime(value);
		}
		else
		{
			yield return new WaitForSeconds(value);
		}
		yield return base.exit;
	}
}
