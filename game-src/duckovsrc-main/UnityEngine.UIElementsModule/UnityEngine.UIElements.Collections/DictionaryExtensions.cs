using System.Collections.Generic;

namespace UnityEngine.UIElements.Collections;

internal static class DictionaryExtensions
{
	public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue fallbackValue = default(TValue))
	{
		TValue value;
		return dict.TryGetValue(key, out value) ? value : fallbackValue;
	}
}
