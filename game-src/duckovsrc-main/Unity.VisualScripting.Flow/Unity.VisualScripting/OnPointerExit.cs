using System;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[UnitOrder(15)]
public sealed class OnPointerExit : PointerEventUnit
{
	public override Type MessageListenerType => typeof(UnityOnPointerExitMessageListener);

	protected override string hookName => "OnPointerExit";
}
