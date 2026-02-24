using UnityEngine;

namespace Unity.VisualScripting;

[AddComponentMenu("Visual Scripting/Listeners/Animator Message Listener")]
public sealed class AnimatorMessageListener : MonoBehaviour
{
	private void OnAnimatorMove()
	{
		EventBus.Trigger("OnAnimatorMove", base.gameObject);
	}

	private void OnAnimatorIK(int layerIndex)
	{
		EventBus.Trigger("OnAnimatorIK", base.gameObject, layerIndex);
	}
}
