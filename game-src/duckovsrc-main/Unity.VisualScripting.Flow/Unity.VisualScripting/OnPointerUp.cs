using System;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[UnitOrder(13)]
public sealed class OnPointerUp : PointerEventUnit
{
	public override Type MessageListenerType => typeof(UnityOnPointerUpMessageListener);

	protected override string hookName => "OnPointerUp";
}
