using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class IEnumerableExtensions
{
	internal static bool HasValues(this IEnumerable<string> collection)
	{
		if (collection == null)
		{
			return false;
		}
		using (IEnumerator<string> enumerator = collection.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				string current = enumerator.Current;
				return true;
			}
		}
		return false;
	}
}
