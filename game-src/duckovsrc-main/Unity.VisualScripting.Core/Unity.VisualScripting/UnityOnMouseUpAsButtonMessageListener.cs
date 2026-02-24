using UnityEngine;

namespace Unity.VisualScripting;

[AddComponentMenu("")]
public sealed class UnityOnMouseUpAsButtonMessageListener : MessageListener
{
	private void OnMouseUpAsButton()
	{
		EventBus.Trigger("OnMouseUpAsButton", base.gameObject);
	}
}
