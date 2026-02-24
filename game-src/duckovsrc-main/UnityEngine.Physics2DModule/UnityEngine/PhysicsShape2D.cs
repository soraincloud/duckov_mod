using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
[NativeHeader(Header = "Modules/Physics2D/Public/PhysicsScripting2D.h")]
public struct PhysicsShape2D
{
	private PhysicsShapeType2D m_ShapeType;

	private float m_Radius;

	private int m_VertexStartIndex;

	private int m_VertexCount;

	private int m_UseAdjacentStart;

	private int m_UseAdjacentEnd;

	private Vector2 m_AdjacentStart;

	private Vector2 m_AdjacentEnd;

	public PhysicsShapeType2D shapeType
	{
		get
		{
			return m_ShapeType;
		}
		set
		{
			m_ShapeType = value;
		}
	}

	public float radius
	{
		get
		{
			return m_Radius;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("radius cannot be negative.");
			}
			if (float.IsNaN(value) || float.IsInfinity(value))
			{
				throw new ArgumentException("radius contains an invalid value.");
			}
			m_Radius = value;
		}
	}

	public int vertexStartIndex
	{
		get
		{
			return m_VertexStartIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("vertexStartIndex cannot be negative.");
			}
			m_VertexStartIndex = value;
		}
	}

	public int vertexCount
	{
		get
		{
			return m_VertexCount;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException("vertexCount cannot be less than one.");
			}
			m_VertexCount = value;
		}
	}

	public bool useAdjacentStart
	{
		get
		{
			return m_UseAdjacentStart != 0;
		}
		set
		{
			m_UseAdjacentStart = (value ? 1 : 0);
		}
	}

	public bool useAdjacentEnd
	{
		get
		{
			return m_UseAdjacentEnd != 0;
		}
		set
		{
			m_UseAdjacentEnd = (value ? 1 : 0);
		}
	}

	public Vector2 adjacentStart
	{
		get
		{
			return m_AdjacentStart;
		}
		set
		{
			if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.x) || float.IsInfinity(value.y))
			{
				throw new ArgumentException("adjacentStart contains an invalid value.");
			}
			m_AdjacentStart = value;
		}
	}

	public Vector2 adjacentEnd
	{
		get
		{
			return m_AdjacentEnd;
		}
		set
		{
			if (float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsInfinity(value.x) || float.IsInfinity(value.y))
			{
				throw new ArgumentException("adjacentEnd contains an invalid value.");
			}
			m_AdjacentEnd = value;
		}
	}
}
