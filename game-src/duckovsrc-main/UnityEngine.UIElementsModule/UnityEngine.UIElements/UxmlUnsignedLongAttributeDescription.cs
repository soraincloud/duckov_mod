using System.Globalization;

namespace UnityEngine.UIElements;

public class UxmlUnsignedLongAttributeDescription : TypedUxmlAttributeDescription<ulong>
{
	public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

	public UxmlUnsignedLongAttributeDescription()
	{
		base.type = "unsignedLong";
		base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
		base.defaultValue = 0uL;
	}

	public override ulong GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		return GetValueFromBag(bag, cc, (string s, ulong l) => ConvertValueToUlong(s, l), base.defaultValue);
	}

	public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref ulong value)
	{
		return TryGetValueFromBag(bag, cc, (string s, ulong l) => ConvertValueToUlong(s, l), base.defaultValue, ref value);
	}

	private static ulong ConvertValueToUlong(string v, ulong defaultValue)
	{
		if (v == null || !ulong.TryParse(v, out var result))
		{
			return defaultValue;
		}
		return result;
	}
}
