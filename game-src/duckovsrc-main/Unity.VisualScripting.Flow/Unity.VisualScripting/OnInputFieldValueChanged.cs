using System;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[TypeIcon(typeof(InputField))]
[UnitOrder(2)]
public sealed class OnInputFieldValueChanged : GameObjectEventUnit<string>
{
	public override Type MessageListenerType => typeof(UnityOnInputFieldValueChangedMessageListener);

	protected override string hookName => "OnInputFieldValueChanged";

	[DoNotSerialize]
	[PortLabelHidden]
	public ValueOutput value { get; private set; }

	protected override void Definition()
	{
		base.Definition();
		value = ValueOutput<string>("value");
	}

	protected override void AssignArguments(Flow flow, string value)
	{
		flow.SetValue(this.value, value);
	}
}
