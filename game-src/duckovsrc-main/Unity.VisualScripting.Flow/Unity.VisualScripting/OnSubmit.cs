using System;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[UnitOrder(24)]
public sealed class OnSubmit : GenericGuiEventUnit
{
	public override Type MessageListenerType => typeof(UnityOnSubmitMessageListener);

	protected override string hookName => "OnSubmit";
}
