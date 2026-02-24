namespace UnityEngine.UIElements;

public class UxmlHash128AttributeDescription : TypedUxmlAttributeDescription<Hash128>
{
	public override string defaultValueAsString => base.defaultValue.ToString();

	public UxmlHash128AttributeDescription()
	{
		base.type = "string";
		base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
		base.defaultValue = default(Hash128);
	}

	public override Hash128 GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		return GetValueFromBag(bag, cc, (string s, Hash128 i) => i = Hash128.Parse(s), base.defaultValue);
	}

	public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref Hash128 value)
	{
		return TryGetValueFromBag(bag, cc, (string s, Hash128 i) => i = Hash128.Parse(s), base.defaultValue, ref value);
	}
}
