namespace UnityEngine.UIElements;

public class BlurEvent : FocusEventBase<BlurEvent>
{
	static BlurEvent()
	{
		EventBase<BlurEvent>.SetCreateFunction(() => new BlurEvent());
	}

	protected internal override void PreDispatch(IPanel panel)
	{
		base.PreDispatch(panel);
		if (base.relatedTarget == null)
		{
			base.focusController.ProcessPendingFocusChange(null);
		}
	}
}
