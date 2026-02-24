using System;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[TypeIcon(typeof(Toggle))]
[UnitOrder(5)]
public sealed class OnToggleValueChanged : GameObjectEventUnit<bool>
{
	public override Type MessageListenerType => typeof(UnityOnToggleValueChangedMessageListener);

	protected override string hookName => "OnToggleValueChanged";

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput value { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		value = ValueOutput<bool>("value");
	}

	protected override void AssignArguments(Flow flow, bool value)
	{
		flow.SetValue(this.value, value);
	}
}
