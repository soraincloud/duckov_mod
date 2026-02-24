using System;

namespace UnityEngine.UIElements;

internal static class EnumFieldHelpers
{
	internal static readonly UxmlTypeAttributeDescription<Enum> type = new UxmlTypeAttributeDescription<Enum>
	{
		name = "type"
	};

	internal static readonly UxmlStringAttributeDescription value = new UxmlStringAttributeDescription
	{
		name = "value"
	};

	internal static readonly UxmlBoolAttributeDescription includeObsoleteValues = new UxmlBoolAttributeDescription
	{
		name = "include-obsolete-values",
		defaultValue = false
	};

	internal static bool ExtractValue(IUxmlAttributes bag, CreationContext cc, out Type resEnumType, out Enum resEnumValue, out bool resIncludeObsoleteValues)
	{
		resIncludeObsoleteValues = false;
		resEnumValue = null;
		resEnumType = type.GetValueFromBag(bag, cc);
		if (resEnumType == null)
		{
			return false;
		}
		string text = null;
		object result = null;
		if (value.TryGetValueFromBag(bag, cc, ref text) && !Enum.TryParse(resEnumType, text, ignoreCase: false, out result))
		{
			Debug.LogErrorFormat("EnumField: Could not parse value of '{0}', because it isn't defined in the {1} enum.", text, resEnumType.FullName);
			return false;
		}
		resEnumValue = ((text != null && result != null) ? ((Enum)result) : ((Enum)Enum.ToObject(resEnumType, 0)));
		resIncludeObsoleteValues = includeObsoleteValues.GetValueFromBag(bag, cc);
		return true;
	}
}
