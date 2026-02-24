using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Bindings;

namespace Unity.Collections;

[VisibleToOtherModules]
internal static class CollectionExtensions
{
	internal static void AddSorted<T>([DisallowNull] this List<T> list, T item, IComparer<T> comparer = null)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list must not be null.");
		}
		if (comparer == null)
		{
			comparer = Comparer<T>.Default;
		}
		if (list.Count == 0)
		{
			list.Add(item);
			return;
		}
		if (comparer.Compare(list[list.Count - 1], item) <= 0)
		{
			list.Add(item);
			return;
		}
		if (comparer.Compare(list[0], item) >= 0)
		{
			list.Insert(0, item);
			return;
		}
		int num = list.BinarySearch(item, comparer);
		if (num < 0)
		{
			num = ~num;
		}
		list.Insert(num, item);
	}

	internal static bool ContainsByEquals<T>([DisallowNull] this IEnumerable<T> collection, T element)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection must not be null.");
		}
		foreach (T item in collection)
		{
			if (item.Equals(element))
			{
				return true;
			}
		}
		return false;
	}
}
