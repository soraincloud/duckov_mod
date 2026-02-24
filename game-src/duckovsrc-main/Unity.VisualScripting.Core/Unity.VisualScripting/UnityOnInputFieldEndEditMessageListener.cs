using UnityEngine;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[AddComponentMenu("")]
public sealed class UnityOnInputFieldEndEditMessageListener : MessageListener
{
	private void Start()
	{
		GetComponent<InputField>()?.onEndEdit?.AddListener(delegate(string value)
		{
			EventBus.Trigger("OnInputFieldEndEdit", base.gameObject, value);
		});
	}
}
