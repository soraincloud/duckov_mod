using System.Collections.Generic;

namespace UnityEngine.UIElements.StyleSheets;

internal static class StyleSheetCache
{
	private struct SheetHandleKey
	{
		public readonly int sheetInstanceID;

		public readonly int index;

		public SheetHandleKey(StyleSheet sheet, int index)
		{
			sheetInstanceID = sheet.GetInstanceID();
			this.index = index;
		}
	}

	private class SheetHandleKeyComparer : IEqualityComparer<SheetHandleKey>
	{
		public bool Equals(SheetHandleKey x, SheetHandleKey y)
		{
			return x.sheetInstanceID == y.sheetInstanceID && x.index == y.index;
		}

		public int GetHashCode(SheetHandleKey key)
		{
			return key.sheetInstanceID.GetHashCode() ^ key.index.GetHashCode();
		}
	}

	private static SheetHandleKeyComparer s_Comparer = new SheetHandleKeyComparer();

	private static Dictionary<SheetHandleKey, StylePropertyId[]> s_RulePropertyIdsCache = new Dictionary<SheetHandleKey, StylePropertyId[]>(s_Comparer);

	internal static void ClearCaches()
	{
		s_RulePropertyIdsCache.Clear();
	}

	internal static StylePropertyId[] GetPropertyIds(StyleSheet sheet, int ruleIndex)
	{
		SheetHandleKey key = new SheetHandleKey(sheet, ruleIndex);
		if (!s_RulePropertyIdsCache.TryGetValue(key, out var value))
		{
			StyleRule styleRule = sheet.rules[ruleIndex];
			value = new StylePropertyId[styleRule.properties.Length];
			for (int i = 0; i < value.Length; i++)
			{
				value[i] = GetPropertyId(styleRule, i);
			}
			s_RulePropertyIdsCache.Add(key, value);
		}
		return value;
	}

	internal static StylePropertyId[] GetPropertyIds(StyleRule rule)
	{
		StylePropertyId[] array = new StylePropertyId[rule.properties.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetPropertyId(rule, i);
		}
		return array;
	}

	private static StylePropertyId GetPropertyId(StyleRule rule, int index)
	{
		StyleProperty styleProperty = rule.properties[index];
		string name = styleProperty.name;
		if (!StylePropertyUtil.s_NameToId.TryGetValue(name, out var value))
		{
			return styleProperty.isCustomProperty ? StylePropertyId.Custom : StylePropertyId.Unknown;
		}
		return value;
	}
}
