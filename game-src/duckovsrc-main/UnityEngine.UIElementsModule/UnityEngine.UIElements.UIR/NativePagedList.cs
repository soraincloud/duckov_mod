#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.UIElements.UIR;

internal class NativePagedList<T> : IDisposable where T : struct
{
	private readonly int k_PoolCapacity;

	private List<NativeArray<T>> m_Pages = new List<NativeArray<T>>(8);

	private NativeArray<T> m_CurrentPage;

	private int m_CurrentPageCount;

	private List<NativeSlice<T>> m_Enumerator = new List<NativeSlice<T>>(8);

	protected bool disposed { get; private set; }

	public NativePagedList(int poolCapacity)
	{
		Debug.Assert(poolCapacity > 0);
		k_PoolCapacity = Mathf.NextPowerOfTwo(poolCapacity);
	}

	public void Add(ref T data)
	{
		if (m_CurrentPageCount < m_CurrentPage.Length)
		{
			m_CurrentPage[m_CurrentPageCount++] = data;
			return;
		}
		int length = ((m_Pages.Count > 0) ? (m_CurrentPage.Length << 1) : k_PoolCapacity);
		m_CurrentPage = new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_Pages.Add(m_CurrentPage);
		m_CurrentPage[0] = data;
		m_CurrentPageCount = 1;
	}

	public void Add(T data)
	{
		Add(ref data);
	}

	public List<NativeSlice<T>> GetPages()
	{
		m_Enumerator.Clear();
		if (m_Pages.Count > 0)
		{
			int num = m_Pages.Count - 1;
			for (int i = 0; i < num; i++)
			{
				m_Enumerator.Add(m_Pages[i]);
			}
			if (m_CurrentPageCount > 0)
			{
				m_Enumerator.Add(m_CurrentPage.Slice(0, m_CurrentPageCount));
			}
		}
		return m_Enumerator;
	}

	public void Reset()
	{
		if (m_Pages.Count > 1)
		{
			m_CurrentPage = m_Pages[0];
			for (int i = 1; i < m_Pages.Count; i++)
			{
				m_Pages[i].Dispose();
			}
			m_Pages.Clear();
			m_Pages.Add(m_CurrentPage);
		}
		m_CurrentPageCount = 0;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			for (int i = 0; i < m_Pages.Count; i++)
			{
				m_Pages[i].Dispose();
			}
			m_Pages.Clear();
			m_CurrentPageCount = 0;
		}
		disposed = true;
	}
}
