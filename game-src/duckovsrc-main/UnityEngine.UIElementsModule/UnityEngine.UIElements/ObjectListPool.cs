using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class ObjectListPool<T>
{
	private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>(() => new List<T>(), 20);

	public static List<T> Get()
	{
		return pool.Get();
	}

	public static void Release(List<T> elements)
	{
		elements.Clear();
		pool.Release(elements);
	}
}
