using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace UnityEngine.Rendering;

public static class RemoveRangeExtensions
{
	[CollectionAccess(CollectionAccessType.ModifyExistingContent)]
	[MustUseReturnValue]
	public static bool TryRemoveElementsInRange<TValue>([DisallowNull] this IList<TValue> list, int index, int count, [NotNullWhen(false)] out Exception error)
	{
		try
		{
			if (list is List<TValue> list2)
			{
				list2.RemoveRange(index, count);
			}
			else
			{
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException("count");
				}
				if (list.Count - index < count)
				{
					throw new ArgumentException("index and count do not denote a valid range of elements in the list");
				}
				for (int num = count; num > 0; num--)
				{
					list.RemoveAt(index);
				}
			}
		}
		catch (Exception ex)
		{
			error = ex;
			return false;
		}
		error = null;
		return true;
	}
}
