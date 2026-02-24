using System;

namespace UnityEngine.UIElements;

public class UxmlTypeAttributeDescription<TBase> : TypedUxmlAttributeDescription<Type>
{
	public override string defaultValueAsString => (base.defaultValue == null) ? "null" : base.defaultValue.FullName;

	public UxmlTypeAttributeDescription()
	{
		base.type = "string";
		base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
		base.defaultValue = null;
	}

	public override Type GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		return GetValueFromBag(bag, cc, (string s, Type type1) => ConvertValueToType(s, type1), base.defaultValue);
	}

	public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref Type value)
	{
		return TryGetValueFromBag(bag, cc, (string s, Type type1) => ConvertValueToType(s, type1), base.defaultValue, ref value);
	}

	private Type ConvertValueToType(string v, Type defaultValue)
	{
		if (string.IsNullOrEmpty(v))
		{
			return defaultValue;
		}
		try
		{
			Type type = Type.GetType(v, throwOnError: true);
			if (typeof(TBase).IsAssignableFrom(type))
			{
				return type;
			}
			Debug.LogError("Type: Invalid type \"" + v + "\". Type must derive from " + typeof(TBase).FullName + ".");
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return defaultValue;
	}
}
