using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Bindings;
using UnityEngine.Internal;

namespace UnityEngine;

public class PhysicsShapeGroup2D
{
	[NativeHeader(Header = "Modules/Physics2D/Public/PhysicsScripting2D.h")]
	internal struct GroupState
	{
		[NativeName("shapesList")]
		public List<PhysicsShape2D> m_Shapes;

		[NativeName("verticesList")]
		public List<Vector2> m_Vertices;

		[NativeName("localToWorld")]
		public Matrix4x4 m_LocalToWorld;

		public void ClearGeometry()
		{
			m_Shapes.Clear();
			m_Vertices.Clear();
		}
	}

	internal GroupState m_GroupState;

	private const float MinVertexSeparation = 0.0025f;

	internal List<Vector2> groupVertices => m_GroupState.m_Vertices;

	internal List<PhysicsShape2D> groupShapes => m_GroupState.m_Shapes;

	public int shapeCount => m_GroupState.m_Shapes.Count;

	public int vertexCount => m_GroupState.m_Vertices.Count;

	public Matrix4x4 localToWorldMatrix
	{
		get
		{
			return m_GroupState.m_LocalToWorld;
		}
		set
		{
			m_GroupState.m_LocalToWorld = value;
		}
	}

	public PhysicsShapeGroup2D([DefaultValue("1")] int shapeCapacity = 1, [DefaultValue("8")] int vertexCapacity = 8)
	{
		m_GroupState = new GroupState
		{
			m_Shapes = new List<PhysicsShape2D>(shapeCapacity),
			m_Vertices = new List<Vector2>(vertexCapacity),
			m_LocalToWorld = Matrix4x4.identity
		};
	}

	public void Clear()
	{
		m_GroupState.ClearGeometry();
		m_GroupState.m_LocalToWorld = Matrix4x4.identity;
	}

	public void Add(PhysicsShapeGroup2D physicsShapeGroup)
	{
		if (physicsShapeGroup == null)
		{
			throw new ArgumentNullException("Cannot merge a NULL PhysicsShapeGroup2D.");
		}
		if (physicsShapeGroup == this)
		{
			throw new ArgumentException("Cannot merge a PhysicsShapeGroup2D with itself.");
		}
		if (physicsShapeGroup.shapeCount == 0)
		{
			return;
		}
		int count = groupShapes.Count;
		int count2 = m_GroupState.m_Vertices.Count;
		groupShapes.AddRange(physicsShapeGroup.groupShapes);
		groupVertices.AddRange(physicsShapeGroup.groupVertices);
		if (count > 0)
		{
			for (int i = count; i < m_GroupState.m_Shapes.Count; i++)
			{
				PhysicsShape2D value = m_GroupState.m_Shapes[i];
				value.vertexStartIndex += count2;
				m_GroupState.m_Shapes[i] = value;
			}
		}
	}

	public void GetShapeData(List<PhysicsShape2D> shapes, List<Vector2> vertices)
	{
		shapes.AddRange(groupShapes);
		vertices.AddRange(groupVertices);
	}

	public void GetShapeData(NativeArray<PhysicsShape2D> shapes, NativeArray<Vector2> vertices)
	{
		if (!shapes.IsCreated || shapes.Length != shapeCount)
		{
			throw new ArgumentException($"Cannot get shape data as the native shapes array length must be identical to the current custom shape count of {shapeCount}.", "shapes");
		}
		if (!vertices.IsCreated || vertices.Length != vertexCount)
		{
			throw new ArgumentException($"Cannot get shape data as the native vertices array length must be identical to the current custom vertex count of {shapeCount}.", "vertices");
		}
		for (int i = 0; i < shapeCount; i++)
		{
			shapes[i] = m_GroupState.m_Shapes[i];
		}
		for (int j = 0; j < vertexCount; j++)
		{
			vertices[j] = m_GroupState.m_Vertices[j];
		}
	}

	public void GetShapeVertices(int shapeIndex, List<Vector2> vertices)
	{
		PhysicsShape2D shape = GetShape(shapeIndex);
		int num = shape.vertexCount;
		vertices.Clear();
		if (vertices.Capacity < num)
		{
			vertices.Capacity = num;
		}
		List<Vector2> list = groupVertices;
		int vertexStartIndex = shape.vertexStartIndex;
		for (int i = 0; i < num; i++)
		{
			vertices.Add(list[vertexStartIndex++]);
		}
	}

	public Vector2 GetShapeVertex(int shapeIndex, int vertexIndex)
	{
		int num = GetShape(shapeIndex).vertexStartIndex + vertexIndex;
		if (num < 0 || num >= groupVertices.Count)
		{
			throw new ArgumentOutOfRangeException($"Cannot get shape-vertex at index {num}. There are {shapeCount} shape-vertices.");
		}
		return groupVertices[num];
	}

	public void SetShapeVertex(int shapeIndex, int vertexIndex, Vector2 vertex)
	{
		int num = GetShape(shapeIndex).vertexStartIndex + vertexIndex;
		if (num < 0 || num >= groupVertices.Count)
		{
			throw new ArgumentOutOfRangeException($"Cannot set shape-vertex at index {num}. There are {shapeCount} shape-vertices.");
		}
		groupVertices[num] = vertex;
	}

	public void SetShapeRadius(int shapeIndex, float radius)
	{
		PhysicsShape2D shape = GetShape(shapeIndex);
		switch (shape.shapeType)
		{
		case PhysicsShapeType2D.Circle:
			if (radius <= 0f)
			{
				throw new ArgumentException($"Circle radius {radius} must be greater than zero.");
			}
			break;
		case PhysicsShapeType2D.Capsule:
			if (radius <= 1E-05f)
			{
				throw new ArgumentException($"Capsule radius: {radius} is too small.");
			}
			break;
		case PhysicsShapeType2D.Polygon:
		case PhysicsShapeType2D.Edges:
			radius = Mathf.Max(0f, radius);
			break;
		}
		shape.radius = radius;
		groupShapes[shapeIndex] = shape;
	}

	public void SetShapeAdjacentVertices(int shapeIndex, bool useAdjacentStart, bool useAdjacentEnd, Vector2 adjacentStart, Vector2 adjacentEnd)
	{
		if (shapeIndex < 0 || shapeIndex >= shapeCount)
		{
			throw new ArgumentOutOfRangeException($"Cannot set shape adjacent vertices at index {shapeIndex}. There are {shapeCount} shapes(s).");
		}
		PhysicsShape2D value = groupShapes[shapeIndex];
		if (value.shapeType != PhysicsShapeType2D.Edges)
		{
			throw new InvalidOperationException($"Cannot set shape adjacent vertices at index {shapeIndex}. The shape must be of type {PhysicsShapeType2D.Edges} but it is of typee {value.shapeType}.");
		}
		value.useAdjacentStart = useAdjacentStart;
		value.useAdjacentEnd = useAdjacentEnd;
		value.adjacentStart = adjacentStart;
		value.adjacentEnd = adjacentEnd;
		groupShapes[shapeIndex] = value;
	}

	public void DeleteShape(int shapeIndex)
	{
		if (shapeIndex < 0 || shapeIndex >= shapeCount)
		{
			throw new ArgumentOutOfRangeException($"Cannot delete shape at index {shapeIndex}. There are {shapeCount} shapes(s).");
		}
		PhysicsShape2D physicsShape2D = groupShapes[shapeIndex];
		int num = physicsShape2D.vertexCount;
		groupShapes.RemoveAt(shapeIndex);
		groupVertices.RemoveRange(physicsShape2D.vertexStartIndex, num);
		while (shapeIndex < groupShapes.Count)
		{
			PhysicsShape2D value = m_GroupState.m_Shapes[shapeIndex];
			value.vertexStartIndex -= num;
			m_GroupState.m_Shapes[shapeIndex++] = value;
		}
	}

	public PhysicsShape2D GetShape(int shapeIndex)
	{
		if (shapeIndex < 0 || shapeIndex >= shapeCount)
		{
			throw new ArgumentOutOfRangeException($"Cannot get shape at index {shapeIndex}. There are {shapeCount} shapes(s).");
		}
		return groupShapes[shapeIndex];
	}

	public int AddCircle(Vector2 center, float radius)
	{
		if (radius <= 0f)
		{
			throw new ArgumentException($"radius {radius} must be greater than zero.");
		}
		int count = groupVertices.Count;
		groupVertices.Add(center);
		groupShapes.Add(new PhysicsShape2D
		{
			shapeType = PhysicsShapeType2D.Circle,
			radius = radius,
			vertexStartIndex = count,
			vertexCount = 1
		});
		return groupShapes.Count - 1;
	}

	public int AddCapsule(Vector2 vertex0, Vector2 vertex1, float radius)
	{
		if (radius <= 1E-05f)
		{
			throw new ArgumentException($"radius: {radius} is too small.");
		}
		int count = groupVertices.Count;
		groupVertices.Add(vertex0);
		groupVertices.Add(vertex1);
		groupShapes.Add(new PhysicsShape2D
		{
			shapeType = PhysicsShapeType2D.Capsule,
			radius = radius,
			vertexStartIndex = count,
			vertexCount = 2
		});
		return groupShapes.Count - 1;
	}

	public int AddBox(Vector2 center, Vector2 size, [DefaultValue("0f")] float angle = 0f, [DefaultValue("0f")] float edgeRadius = 0f)
	{
		if (size.x <= 0.0025f || size.y <= 0.0025f)
		{
			throw new ArgumentException($"size: {size} is too small. Vertex need to be separated by at least {0.0025f}");
		}
		edgeRadius = Mathf.Max(0f, edgeRadius);
		angle *= MathF.PI / 180f;
		float cos = Mathf.Cos(angle);
		float sin = Mathf.Sin(angle);
		Vector2 vector = size * 0.5f;
		Vector2 item = center + Rotate(cos, sin, -vector);
		Vector2 item2 = center + Rotate(cos, sin, new Vector2(vector.x, 0f - vector.y));
		Vector2 item3 = center + Rotate(cos, sin, vector);
		Vector2 item4 = center + Rotate(cos, sin, new Vector2(0f - vector.x, vector.y));
		int count = groupVertices.Count;
		groupVertices.Add(item);
		groupVertices.Add(item2);
		groupVertices.Add(item3);
		groupVertices.Add(item4);
		groupShapes.Add(new PhysicsShape2D
		{
			shapeType = PhysicsShapeType2D.Polygon,
			radius = edgeRadius,
			vertexStartIndex = count,
			vertexCount = 4
		});
		return groupShapes.Count - 1;
		static Vector2 Rotate(float num, float num2, Vector2 value)
		{
			return new Vector2(num * value.x - num2 * value.y, num2 * value.x + num * value.y);
		}
	}

	public int AddPolygon(List<Vector2> vertices)
	{
		int count = vertices.Count;
		if (count < 3 || count > 8)
		{
			throw new ArgumentException($"Vertex Count {count} must be >= 3 and <= {8}.");
		}
		float num = 6.25E-06f;
		for (int i = 1; i < count; i++)
		{
			Vector2 vector = vertices[i - 1];
			Vector2 vector2 = vertices[i];
			if ((vector2 - vector).sqrMagnitude <= num)
			{
				throw new ArgumentException($"vertices: {vector} and {vector2} are too close. Vertices need to be separated by at least {num}");
			}
		}
		int count2 = groupVertices.Count;
		groupVertices.AddRange(vertices);
		groupShapes.Add(new PhysicsShape2D
		{
			shapeType = PhysicsShapeType2D.Polygon,
			radius = 0f,
			vertexStartIndex = count2,
			vertexCount = count
		});
		return groupShapes.Count - 1;
	}

	public int AddEdges(List<Vector2> vertices, [DefaultValue("0f")] float edgeRadius = 0f)
	{
		return AddEdges(vertices, useAdjacentStart: false, useAdjacentEnd: false, Vector2.zero, Vector2.zero, edgeRadius);
	}

	public int AddEdges(List<Vector2> vertices, bool useAdjacentStart, bool useAdjacentEnd, Vector2 adjacentStart, Vector2 adjacentEnd, [DefaultValue("0f")] float edgeRadius = 0f)
	{
		int count = vertices.Count;
		if (count < 2)
		{
			throw new ArgumentOutOfRangeException($"Vertex Count {count} must be >= 2.");
		}
		edgeRadius = Mathf.Max(0f, edgeRadius);
		int count2 = groupVertices.Count;
		groupVertices.AddRange(vertices);
		groupShapes.Add(new PhysicsShape2D
		{
			shapeType = PhysicsShapeType2D.Edges,
			radius = edgeRadius,
			vertexStartIndex = count2,
			vertexCount = count,
			useAdjacentStart = useAdjacentStart,
			useAdjacentEnd = useAdjacentEnd,
			adjacentStart = adjacentStart,
			adjacentEnd = adjacentEnd
		});
		return groupShapes.Count - 1;
	}
}
