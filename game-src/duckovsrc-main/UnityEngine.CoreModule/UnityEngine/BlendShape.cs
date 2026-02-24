using System;
using UnityEngine.Scripting;

namespace UnityEngine;

[Serializable]
[UsedByNativeCode]
internal struct BlendShape
{
	[SerializeField]
	private uint m_FirstVertex;

	[SerializeField]
	private uint m_VertexCount;

	[SerializeField]
	private bool m_HasNormals;

	[SerializeField]
	private bool m_HasTangents;

	public uint firstVertex
	{
		get
		{
			return m_FirstVertex;
		}
		set
		{
			m_FirstVertex = value;
		}
	}

	public uint vertexCount
	{
		get
		{
			return m_VertexCount;
		}
		set
		{
			m_VertexCount = value;
		}
	}

	public bool hasNormals
	{
		get
		{
			return m_HasNormals;
		}
		set
		{
			m_HasNormals = value;
		}
	}

	public bool hasTangents
	{
		get
		{
			return m_HasTangents;
		}
		set
		{
			m_HasTangents = value;
		}
	}
}
