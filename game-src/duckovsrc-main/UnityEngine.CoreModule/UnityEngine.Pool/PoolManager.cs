using System;
using System.Collections.Generic;

namespace UnityEngine.Pool;

internal static class PoolManager
{
	private static readonly List<WeakReference<IPool>> s_WeakPoolReferences = new List<WeakReference<IPool>>();

	public static void Reset()
	{
		for (int num = s_WeakPoolReferences.Count - 1; num >= 0; num--)
		{
			if (s_WeakPoolReferences[num].TryGetTarget(out var target))
			{
				target.Clear();
			}
			else
			{
				s_WeakPoolReferences.RemoveAt(num);
			}
		}
	}

	public static void Register(IPool pool)
	{
		s_WeakPoolReferences.Add(new WeakReference<IPool>(pool));
	}
}
