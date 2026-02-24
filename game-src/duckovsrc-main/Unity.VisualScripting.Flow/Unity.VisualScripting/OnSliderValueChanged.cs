using System;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[TypeIcon(typeof(Slider))]
[UnitOrder(8)]
public sealed class OnSliderValueChanged : GameObjectEventUnit<float>
{
	public override Type MessageListenerType => typeof(UnityOnSliderValueChangedMessageListener);

	protected override string hookName => "OnSliderValueChanged";

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput value { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		value = ValueOutput<float>("value");
	}

	protected override void AssignArguments(Flow flow, float value)
	{
		flow.SetValue(this.value, value);
	}
}
