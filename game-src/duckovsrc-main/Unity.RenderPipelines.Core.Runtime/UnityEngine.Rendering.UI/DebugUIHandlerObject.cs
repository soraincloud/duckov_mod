using UnityEngine.UI;

namespace UnityEngine.Rendering.UI;

public class DebugUIHandlerObject : DebugUIHandlerWidget
{
	public Text nameLabel;

	public Text valueLabel;

	internal override void SetWidget(DebugUI.Widget widget)
	{
		base.SetWidget(widget);
		DebugUI.ObjectField objectField = CastWidget<DebugUI.ObjectField>();
		nameLabel.text = objectField.displayName;
		valueLabel.text = objectField.GetValue().name;
	}

	public override bool OnSelection(bool fromNext, DebugUIHandlerWidget previous)
	{
		nameLabel.color = colorSelected;
		valueLabel.color = colorSelected;
		return true;
	}

	public override void OnDeselection()
	{
		nameLabel.color = colorDefault;
		valueLabel.color = colorDefault;
	}
}
