using UnityEngine;
using UnityEngine.UI;

namespace Unity.VisualScripting;

[AddComponentMenu("")]
public sealed class UnityOnInputFieldValueChangedMessageListener : MessageListener
{
	private void Start()
	{
		GetComponent<InputField>()?.onValueChanged?.AddListener(delegate(string value)
		{
			EventBus.Trigger("OnInputFieldValueChanged", base.gameObject, value);
		});
	}
}
