using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityEngine.UIElements;

public class UxmlEnumAttributeDescription<T> : TypedUxmlAttributeDescription<T> where T : struct, IConvertible
{
	public override string defaultValueAsString => base.defaultValue.ToString(CultureInfo.InvariantCulture.NumberFormat);

	public UxmlEnumAttributeDescription()
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("T must be an enumerated type");
		}
		base.type = "string";
		base.typeNamespace = "http://www.w3.org/2001/XMLSchema";
		base.defaultValue = new T();
		UxmlEnumeration uxmlEnumeration = new UxmlEnumeration();
		List<string> list = new List<string>();
		foreach (T value in Enum.GetValues(typeof(T)))
		{
			list.Add(value.ToString(CultureInfo.InvariantCulture));
		}
		uxmlEnumeration.values = list;
		base.restriction = uxmlEnumeration;
	}

	public override T GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		return GetValueFromBag(bag, cc, (string s, T convertible) => ConvertValueToEnum(s, convertible), base.defaultValue);
	}

	public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, ref T value)
	{
		return TryGetValueFromBag(bag, cc, (string s, T convertible) => ConvertValueToEnum(s, convertible), base.defaultValue, ref value);
	}

	private static U ConvertValueToEnum<U>(string v, U defaultValue) where U : struct
	{
		try
		{
			if (string.IsNullOrEmpty(v))
			{
				return defaultValue;
			}
			return (U)Enum.Parse(typeof(U), v, ignoreCase: true);
		}
		catch (ArgumentException)
		{
			Debug.LogError(GetEnumNameErrorMessage(v, typeof(U)));
		}
		catch (OverflowException)
		{
			Debug.LogError(GetEnumRangeErrorMessage(v, typeof(U)));
		}
		return defaultValue;
	}

	private static string GetEnumNameErrorMessage(string v, Type enumType)
	{
		return "The " + enumType.Name + " enum does not contain the value `" + v + "`. Value must be in range [" + string.Join(" | ", Enum.GetNames(enumType)) + "].";
	}

	private static string GetEnumRangeErrorMessage(string v, Type enumType)
	{
		return v + " is outside of the range of possible values for the " + enumType.Name + " enum.";
	}
}
