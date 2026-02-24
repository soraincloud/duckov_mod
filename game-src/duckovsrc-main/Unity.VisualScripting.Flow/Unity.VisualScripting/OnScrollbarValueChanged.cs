using System;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[TypeIcon(typeof(Scrollbar))]
[UnitOrder(6)]
public sealed class OnScrollbarValueChanged : GameObjectEventUnit<float>
{
	public override Type MessageListenerType => typeof(UnityOnScrollbarValueChangedMessageListener);

	protected override string hookName => "OnScrollbarValueChanged";

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
