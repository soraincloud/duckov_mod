using System;

namespace UnityEngine;

[Serializable]
public struct BlendShapeBufferRange
{
	[SerializeField]
	private uint m_StartIndex;

	[SerializeField]
	private uint m_EndIndex;

	public uint startIndex
	{
		get
		{
			return m_StartIndex;
		}
		internal set
		{
			m_StartIndex = value;
		}
	}

	public uint endIndex
	{
		get
		{
			return m_EndIndex;
		}
		internal set
		{
			m_EndIndex = value;
		}
	}
}
