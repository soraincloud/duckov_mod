using System.Globalization;

namespace UnityEngine.UIElements;

public class UxmlUnsignedIntAttributeDescription : TypedUxmlAttributeDescription<uint>
{
	public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

	public UxmlUnsignedIntAttributeDescription()
	{
		base.type = "unsignedInt";
		base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
		base.defaultValue = 0u;
	}

	public override uint GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		return GetValueFromBag(bag, cc, (string s, uint i) => ConvertValueToUInt(s, i), base.defaultValue);
	}

	public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref uint value)
	{
		return TryGetValueFromBag(bag, cc, (string s, uint i) => ConvertValueToUInt(s, i), base.defaultValue, ref value);
	}

	private static uint ConvertValueToUInt(string v, uint defaultValue)
	{
		if (v == null || !uint.TryParse(v, out var result))
		{
			return defaultValue;
		}
		return result;
	}
}
