using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class TextValueFieldTraits<TValueType, TValueUxmlAttributeType> : BaseFieldTraits<TValueType, TValueUxmlAttributeType> where TValueUxmlAttributeType : TypedUxmlAttributeDescription<TValueType>, new()
{
	private UxmlBoolAttributeDescription m_IsReadOnly = new UxmlBoolAttributeDescription
	{
		name = "readonly"
	};

	private UxmlBoolAttributeDescription m_IsDelayed = new UxmlBoolAttributeDescription
	{
		name = "is-delayed"
	};

	public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
	{
		base.Init(ve, bag, cc);
		TextInputBaseField<TValueType> textInputBaseField = (TextInputBaseField<TValueType>)ve;
		if (textInputBaseField != null)
		{
			textInputBaseField.isReadOnly = m_IsReadOnly.GetValueFromBag(bag, cc);
			textInputBaseField.isDelayed = m_IsDelayed.GetValueFromBag(bag, cc);
		}
	}
}
