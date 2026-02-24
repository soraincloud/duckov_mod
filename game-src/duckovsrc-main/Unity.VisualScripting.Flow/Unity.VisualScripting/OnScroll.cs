using System;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[UnitOrder(20)]
public sealed class OnScroll : PointerEventUnit
{
	public override Type MessageListenerType => typeof(UnityOnScrollMessageListener);

	protected override string hookName => "OnScroll";
}
