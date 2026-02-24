using System;

namespace UnityEngine.UIElements;

[Serializable]
[HelpURL("UIE-tss")]
public class ThemeStyleSheet : StyleSheet
{
	internal override void OnEnable()
	{
		base.isDefaultStyleSheet = true;
		base.OnEnable();
	}
}
