using UnityEngine;

namespace Unity.VisualScripting;

[AddComponentMenu("")]
public sealed class UnityOnCollisionStayMessageListener : MessageListener
{
	private void OnCollisionStay(Collision collision)
	{
		EventBus.Trigger("OnCollisionStay", base.gameObject, collision);
	}
}
