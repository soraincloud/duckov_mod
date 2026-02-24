using UnityEngine;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[AddComponentMenu("")]
public sealed class UnityOnScrollbarValueChangedMessageListener : MessageListener
{
	private void Start()
	{
		GetComponent<Scrollbar>()?.onValueChanged?.AddListener(delegate(float value)
		{
			EventBus.Trigger("OnScrollbarValueChanged", base.gameObject, value);
		});
	}
}
