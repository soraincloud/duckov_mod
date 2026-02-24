#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.UIElements.UIR;

internal class TempAllocator<T> : IDisposable where T : struct
{
	private struct Page
	{
		public NativeArray<T> array;

		public int used;
	}

	public struct Statistics
	{
		public PageStatistics pool;

		public PageStatistics[] excess;
	}

	public struct PageStatistics
	{
		public int size;

		public int used;
	}

	private readonly int m_ExcessMinCapacity;

	private readonly int m_ExcessMaxCapacity;

	private Page m_Pool;

	private List<Page> m_Excess;

	private int m_NextExcessSize;

	protected bool disposed { get; private set; }

	public TempAllocator(int poolCapacity, int excessMinCapacity, int excessMaxCapacity)
	{
		Debug.Assert(poolCapacity >= 1);
		Debug.Assert(excessMinCapacity >= 1);
		Debug.Assert(excessMinCapacity <= excessMaxCapacity);
		m_ExcessMinCapacity = excessMinCapacity;
		m_ExcessMaxCapacity = excessMaxCapacity;
		m_NextExcessSize = m_ExcessMinCapacity;
		m_Pool = default(Page);
		m_Pool.array = new NativeArray<T>(poolCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_Excess = new List<Page>(8);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				ReleaseExcess();
				m_Pool.array.Dispose();
				m_Pool.used = 0;
			}
			disposed = true;
		}
	}

	public NativeSlice<T> Alloc(int count)
	{
		Debug.Assert(!disposed);
		int num = m_Pool.used + count;
		if (num <= m_Pool.array.Length)
		{
			NativeSlice<T> result = m_Pool.array.Slice(m_Pool.used, count);
			m_Pool.used = num;
			return result;
		}
		if (count > m_ExcessMaxCapacity)
		{
			Page item = new Page
			{
				array = new NativeArray<T>(count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
				used = count
			};
			m_Excess.Add(item);
			return item.array.Slice(0, count);
		}
		for (int num2 = m_Excess.Count - 1; num2 >= 0; num2--)
		{
			Page value = m_Excess[num2];
			num = value.used + count;
			if (num <= value.array.Length)
			{
				NativeSlice<T> result2 = value.array.Slice(value.used, count);
				value.used = num;
				m_Excess[num2] = value;
				return result2;
			}
		}
		while (count > m_NextExcessSize)
		{
			m_NextExcessSize <<= 1;
		}
		Page item2 = new Page
		{
			array = new NativeArray<T>(m_NextExcessSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory),
			used = count
		};
		m_Excess.Add(item2);
		m_NextExcessSize = Mathf.Min(m_NextExcessSize << 1, m_ExcessMaxCapacity);
		return item2.array.Slice(0, count);
	}

	public void Reset()
	{
		ReleaseExcess();
		m_Pool.used = 0;
		m_NextExcessSize = m_ExcessMinCapacity;
	}

	private void ReleaseExcess()
	{
		foreach (Page item in m_Excess)
		{
			item.array.Dispose();
		}
		m_Excess.Clear();
	}

	public Statistics GatherStatistics()
	{
		Statistics result = new Statistics
		{
			pool = new PageStatistics
			{
				size = m_Pool.array.Length,
				used = m_Pool.used
			},
			excess = new PageStatistics[m_Excess.Count]
		};
		for (int i = 0; i < m_Excess.Count; i++)
		{
			result.excess[i] = new PageStatistics
			{
				size = m_Excess[i].array.Length,
				used = m_Excess[i].used
			};
		}
		return result;
	}
}
