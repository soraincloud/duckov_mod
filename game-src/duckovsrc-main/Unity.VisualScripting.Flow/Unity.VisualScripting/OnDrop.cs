using System;

namespace Unity.VisualScripting;

[UnitCategory("Events/GUI")]
[TypeIcon(typeof(OnDrag))]
[UnitOrder(19)]
public sealed class OnDrop : PointerEventUnit
{
	public override Type MessageListenerType => typeof(UnityOnDropMessageListener);

	protected override string hookName => "OnDrop";
}
