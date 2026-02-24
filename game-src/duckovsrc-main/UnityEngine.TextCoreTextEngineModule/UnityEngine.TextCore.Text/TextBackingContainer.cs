using System;

namespace UnityEngine.TextCore.Text;

internal struct TextBackingContainer
{
	private uint[] m_Array;

	private int m_Count;

	public uint[] Text => m_Array;

	public int Capacity => m_Array.Length;

	public int Count
	{
		get
		{
			return m_Count;
		}
		set
		{
			m_Count = value;
		}
	}

	public uint this[int index]
	{
		get
		{
			return m_Array[index];
		}
		set
		{
			if (index >= m_Array.Length)
			{
				Resize(index);
			}
			m_Array[index] = value;
		}
	}

	public TextBackingContainer(int size)
	{
		m_Array = new uint[size];
		m_Count = 0;
	}

	public void Resize(int size)
	{
		size = Mathf.NextPowerOfTwo(size + 1);
		Array.Resize(ref m_Array, size);
	}
}
