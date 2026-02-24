namespace UnityEngine.UIElements;

internal class UxmlAssetAttributeDescription<T> : TypedUxmlAttributeDescription<T> where T : Object
{
	public override string defaultValueAsString => base.defaultValue?.ToString() ?? "null";

	public UxmlAssetAttributeDescription()
	{
		base.type = "string";
		base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
		base.defaultValue = null;
	}

	public override T GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		string value = null;
		if (TryGetValueFromBag(bag, cc, (string s, string t) => s, null, ref value))
		{
			VisualTreeAsset visualTreeAsset = cc.visualTreeAsset;
			return ((object)visualTreeAsset != null) ? visualTreeAsset.GetAsset<T>(value) : null;
		}
		return null;
	}
}
