using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Polybrush;

[Serializable]
internal class PolyMesh
{
	[SerializeField]
	internal string name = "";

	[SerializeField]
	internal Vector3[] vertices;

	[SerializeField]
	internal Vector3[] normals;

	[SerializeField]
	internal Color[] colors;

	[SerializeField]
	internal Vector4[] tangents;

	[SerializeField]
	internal List<Vector4> uv0;

	[SerializeField]
	internal List<Vector4> uv1;

	[SerializeField]
	internal List<Vector4> uv2;

	[SerializeField]
	internal List<Vector4> uv3;

	[SerializeField]
	private int[] m_Triangles;

	[SerializeField]
	private SubMesh[] m_SubMeshes;

	[SerializeField]
	private Mesh m_Mesh;

	private ComputeBuffer m_VertexBuffer;

	private ComputeBuffer m_TriangleBuffer;

	private static Vector3[] s_PerTriangleNormalsBuffer = new Vector3[4096];

	private static int[] s_PerTriangleAvgBuffer = new int[4096];

	internal Mesh mesh => m_Mesh;

	internal int vertexCount
	{
		get
		{
			if (vertices == null)
			{
				return 0;
			}
			return vertices.Length;
		}
	}

	internal SubMesh[] subMeshes => m_SubMeshes;

	internal int subMeshCount
	{
		get
		{
			if (m_SubMeshes == null)
			{
				return 0;
			}
			return m_SubMeshes.Length;
		}
	}

	internal ComputeBuffer vertexBuffer
	{
		get
		{
			if (m_VertexBuffer == null)
			{
				m_VertexBuffer = new ComputeBuffer(vertices.Length, 12);
				m_VertexBuffer.SetData(vertices);
			}
			return m_VertexBuffer;
		}
	}

	internal ComputeBuffer triangleBuffer
	{
		get
		{
			if (m_TriangleBuffer == null)
			{
				int[] triangles = GetTriangles();
				m_TriangleBuffer = new ComputeBuffer(triangles.Length, 4);
				m_TriangleBuffer.SetData(triangles);
			}
			return m_TriangleBuffer;
		}
	}

	internal PolyMesh()
	{
		uv0 = new List<Vector4>();
		uv1 = new List<Vector4>();
		uv2 = new List<Vector4>();
		uv3 = new List<Vector4>();
	}

	internal void InitializeWithUnityMesh(Mesh mesh)
	{
		m_Mesh = mesh;
		name = mesh.name;
		ApplyAttributesFromUnityMesh(mesh);
	}

	internal List<Vector4> GetUVs(int channel)
	{
		return channel switch
		{
			0 => uv0, 
			1 => uv1, 
			2 => uv2, 
			3 => uv3, 
			_ => null, 
		};
	}

	internal void SetUVs(int channel, List<Vector4> uvs)
	{
		switch (channel)
		{
		case 0:
			uv0 = uvs;
			break;
		case 1:
			uv1 = uvs;
			break;
		case 2:
			uv2 = uvs;
			break;
		case 3:
			uv3 = uvs;
			break;
		}
	}

	internal void Clear()
	{
		vertices = null;
		normals = null;
		colors = null;
		tangents = null;
		uv0 = null;
		uv1 = null;
		uv2 = null;
		uv3 = null;
		m_SubMeshes = null;
		ClearBuffers();
	}

	internal void ClearBuffers()
	{
		if (m_VertexBuffer != null)
		{
			m_VertexBuffer.Dispose();
		}
		if (m_TriangleBuffer != null)
		{
			m_TriangleBuffer.Dispose();
		}
		m_VertexBuffer = null;
		m_TriangleBuffer = null;
	}

	internal int[] GetTriangles()
	{
		if (m_Triangles == null)
		{
			RefreshTriangles();
		}
		return m_Triangles;
	}

	internal void SetSubMeshes(Mesh mesh)
	{
		m_SubMeshes = new SubMesh[mesh.subMeshCount];
		for (int i = 0; i < m_SubMeshes.Length; i++)
		{
			m_SubMeshes[i] = new SubMesh(mesh, i);
		}
	}

	private void RefreshTriangles()
	{
		m_Triangles = ((m_SubMeshes == null) ? null : m_SubMeshes.SelectMany((SubMesh x) => x.indexes).ToArray());
	}

	internal void RecalculateNormals()
	{
		if (s_PerTriangleNormalsBuffer.Length < vertexCount)
		{
			Array.Resize(ref s_PerTriangleNormalsBuffer, vertexCount);
			Array.Resize(ref s_PerTriangleAvgBuffer, vertexCount);
		}
		for (int i = 0; i < vertexCount; i++)
		{
			s_PerTriangleNormalsBuffer[i].x = 0f;
			s_PerTriangleNormalsBuffer[i].y = 0f;
			s_PerTriangleNormalsBuffer[i].z = 0f;
			s_PerTriangleAvgBuffer[i] = 0;
		}
		int[] triangles = GetTriangles();
		for (int j = 0; j < triangles.Length; j += 3)
		{
			int num = triangles[j];
			int num2 = triangles[j + 1];
			int num3 = triangles[j + 2];
			Vector3 vector = Math.Normal(vertices[num], vertices[num2], vertices[num3]);
			s_PerTriangleNormalsBuffer[num].x += vector.x;
			s_PerTriangleNormalsBuffer[num2].x += vector.x;
			s_PerTriangleNormalsBuffer[num3].x += vector.x;
			s_PerTriangleNormalsBuffer[num].y += vector.y;
			s_PerTriangleNormalsBuffer[num2].y += vector.y;
			s_PerTriangleNormalsBuffer[num3].y += vector.y;
			s_PerTriangleNormalsBuffer[num].z += vector.z;
			s_PerTriangleNormalsBuffer[num2].z += vector.z;
			s_PerTriangleNormalsBuffer[num3].z += vector.z;
			s_PerTriangleAvgBuffer[num]++;
			s_PerTriangleAvgBuffer[num2]++;
			s_PerTriangleAvgBuffer[num3]++;
		}
		for (int k = 0; k < vertexCount; k++)
		{
			normals[k].x = s_PerTriangleNormalsBuffer[k].x * (float)s_PerTriangleAvgBuffer[k];
			normals[k].y = s_PerTriangleNormalsBuffer[k].y * (float)s_PerTriangleAvgBuffer[k];
			normals[k].z = s_PerTriangleNormalsBuffer[k].z * (float)s_PerTriangleAvgBuffer[k];
			Math.Divide(normals[k], normals[k].magnitude, ref normals[k]);
		}
	}

	internal void ApplyAttributesToUnityMesh(Mesh mesh, MeshChannel attrib = MeshChannel.All)
	{
		if (attrib == MeshChannel.All)
		{
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.colors = colors;
			mesh.tangents = tangents;
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);
			mesh.subMeshCount = subMeshCount;
			for (int i = 0; i < subMeshCount; i++)
			{
				mesh.SetIndices(m_SubMeshes[i].indexes, m_SubMeshes[i].topology, i);
			}
			RefreshTriangles();
			return;
		}
		if ((attrib & MeshChannel.Position) > MeshChannel.Null)
		{
			mesh.vertices = vertices;
		}
		if ((attrib & MeshChannel.Normal) > MeshChannel.Null)
		{
			mesh.normals = normals;
		}
		if ((attrib & MeshChannel.Color) > MeshChannel.Null)
		{
			mesh.colors = colors;
		}
		if ((attrib & MeshChannel.Tangent) > MeshChannel.Null)
		{
			mesh.tangents = tangents;
		}
		if ((attrib & MeshChannel.UV0) > MeshChannel.Null)
		{
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
		}
		if ((attrib & MeshChannel.UV2) > MeshChannel.Null)
		{
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
		}
		if ((attrib & MeshChannel.UV3) > MeshChannel.Null)
		{
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
		}
		if ((attrib & MeshChannel.UV4) > MeshChannel.Null)
		{
			mesh.SetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);
		}
	}

	internal void ApplyAttributesFromUnityMesh(Mesh mesh, MeshChannel attrib = MeshChannel.All)
	{
		if (attrib == MeshChannel.All)
		{
			vertices = mesh.vertices;
			normals = mesh.normals;
			colors = mesh.colors;
			tangents = mesh.tangents;
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);
			if (subMeshCount == 0 || mesh.subMeshCount == subMeshCount)
			{
				SetSubMeshes(mesh);
				RefreshTriangles();
			}
			return;
		}
		if ((attrib & MeshChannel.Position) > MeshChannel.Null)
		{
			vertices = mesh.vertices;
		}
		if ((attrib & MeshChannel.Normal) > MeshChannel.Null)
		{
			normals = mesh.normals;
		}
		if ((attrib & MeshChannel.Color) > MeshChannel.Null)
		{
			colors = mesh.colors;
		}
		if ((attrib & MeshChannel.Tangent) > MeshChannel.Null)
		{
			tangents = mesh.tangents;
		}
		if ((attrib & MeshChannel.UV0) > MeshChannel.Null)
		{
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV0), uv0);
		}
		if ((attrib & MeshChannel.UV2) > MeshChannel.Null)
		{
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV2), uv1);
		}
		if ((attrib & MeshChannel.UV3) > MeshChannel.Null)
		{
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV3), uv2);
		}
		if ((attrib & MeshChannel.UV4) > MeshChannel.Null)
		{
			mesh.GetUVs(MeshChannelUtility.UVChannelToIndex(MeshChannel.UV4), uv3);
		}
	}

	internal Mesh ToUnityMesh()
	{
		if (m_Mesh == null)
		{
			m_Mesh = new Mesh();
			m_Mesh.name = name;
			UpdateMeshFromData();
		}
		return m_Mesh;
	}

	internal void UpdateMeshFromData()
	{
		if (m_Mesh == null)
		{
			m_Mesh = new Mesh();
			m_Mesh.name = name;
		}
		ApplyAttributesToUnityMesh(m_Mesh);
	}

	internal bool IsValid()
	{
		if (vertexCount == 0)
		{
			return false;
		}
		if (m_Triangles.Length == 0)
		{
			return false;
		}
		return true;
	}
}
