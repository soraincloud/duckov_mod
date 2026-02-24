using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class StyleCache
{
	private static Dictionary<long, ComputedStyle> s_ComputedStyleCache = new Dictionary<long, ComputedStyle>();

	private static Dictionary<int, StyleVariableContext> s_StyleVariableContextCache = new Dictionary<int, StyleVariableContext>();

	private static Dictionary<int, ComputedTransitionProperty[]> s_ComputedTransitionsCache = new Dictionary<int, ComputedTransitionProperty[]>();

	public static bool TryGetValue(long hash, out ComputedStyle data)
	{
		return s_ComputedStyleCache.TryGetValue(hash, out data);
	}

	public static void SetValue(long hash, ref ComputedStyle data)
	{
		s_ComputedStyleCache[hash] = data;
	}

	public static bool TryGetValue(int hash, out StyleVariableContext data)
	{
		return s_StyleVariableContextCache.TryGetValue(hash, out data);
	}

	public static void SetValue(int hash, StyleVariableContext data)
	{
		s_StyleVariableContextCache[hash] = data;
	}

	public static bool TryGetValue(int hash, out ComputedTransitionProperty[] data)
	{
		return s_ComputedTransitionsCache.TryGetValue(hash, out data);
	}

	public static void SetValue(int hash, ComputedTransitionProperty[] data)
	{
		s_ComputedTransitionsCache[hash] = data;
	}

	public static void ClearStyleCache()
	{
		foreach (KeyValuePair<long, ComputedStyle> item in s_ComputedStyleCache)
		{
			item.Value.Release();
		}
		s_ComputedStyleCache.Clear();
		s_StyleVariableContextCache.Clear();
		s_ComputedTransitionsCache.Clear();
	}
}
