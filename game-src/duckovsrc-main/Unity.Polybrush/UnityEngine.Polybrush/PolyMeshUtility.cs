using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.Polybrush;

internal static class PolyMeshUtility
{
	private struct CommonVertexCache
	{
		private int m_Hash;

		public int[][] indices;

		public CommonVertexCache(Mesh mesh)
		{
			m_Hash = GetHash(mesh);
			Vector3[] v = mesh.vertices;
			int[] source = Util.Fill((int x) => x, v.Length);
			indices = (from y in ((IEnumerable<int>)source).ToLookup((Func<int, RndVec3>)((int x) => v[x]))
				select y.ToArray()).ToArray();
		}

		public bool IsValidForMesh(Mesh mesh)
		{
			return m_Hash == GetHash(mesh);
		}

		private static int GetHash(Mesh mesh)
		{
			int num = 783 + mesh.vertexCount;
			int i = 0;
			for (int subMeshCount = mesh.subMeshCount; i < subMeshCount; i++)
			{
				num = num * 29 + (int)mesh.GetIndexCount(i);
			}
			return num;
		}
	}

	private static readonly Color clear = new Color(0f, 0f, 0f, 0f);

	private static readonly Vector2[] k_VertexBillboardUV0Content = new Vector2[4]
	{
		Vector3.zero,
		Vector3.right,
		Vector3.up,
		Vector3.one
	};

	private static readonly Vector2[] k_VertexBillboardUV2Content = new Vector2[4]
	{
		-Vector3.up - Vector3.right,
		-Vector3.up + Vector3.right,
		Vector3.up - Vector3.right,
		Vector3.up + Vector3.right
	};

	private static Dictionary<PolyMesh, CommonVertexCache> commonVerticesCache = new Dictionary<PolyMesh, CommonVertexCache>();

	private static Dictionary<PolyMesh, Dictionary<PolyEdge, List<int>>> adjacentTrianglesCache = new Dictionary<PolyMesh, Dictionary<PolyEdge, List<int>>>();

	private static Dictionary<PolyMesh, int[][]> commonNormalsCache = new Dictionary<PolyMesh, int[][]>();

	internal static Mesh DeepCopy(Mesh src)
	{
		if (src == null)
		{
			return null;
		}
		Mesh mesh = new Mesh();
		Copy(src, mesh);
		return mesh;
	}

	internal static void Copy(Mesh src, Mesh dst)
	{
		if (!(dst == null) && !(src == null))
		{
			dst.Clear();
			dst.vertices = src.vertices;
			List<Vector4> uvs = new List<Vector4>();
			src.GetUVs(0, uvs);
			dst.SetUVs(0, uvs);
			src.GetUVs(1, uvs);
			dst.SetUVs(1, uvs);
			src.GetUVs(2, uvs);
			dst.SetUVs(2, uvs);
			src.GetUVs(3, uvs);
			dst.SetUVs(3, uvs);
			dst.normals = src.normals;
			dst.tangents = src.tangents;
			dst.boneWeights = src.boneWeights;
			dst.colors = src.colors;
			dst.colors32 = src.colors32;
			dst.bindposes = src.bindposes;
			dst.subMeshCount = src.subMeshCount;
			dst.indexFormat = src.indexFormat;
			for (int i = 0; i < src.subMeshCount; i++)
			{
				dst.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
			}
			dst.name = Util.IncrementPrefix("z", src.name);
		}
	}

	internal static Mesh CreateOverlayMesh(PolyMesh src)
	{
		if (src == null)
		{
			return null;
		}
		Mesh mesh = new Mesh();
		mesh.name = "Overlay Mesh: " + src.name;
		mesh.vertices = src.vertices;
		mesh.normals = src.normals;
		mesh.colors = Util.Fill(new Color(0f, 0f, 0f, 0f), mesh.vertexCount);
		mesh.indexFormat = ((src.vertexCount >= 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		mesh.subMeshCount = src.subMeshCount;
		for (int i = 0; i < src.subMeshCount; i++)
		{
			SubMesh obj = src.subMeshes[i];
			MeshTopology topology = obj.topology;
			int[] indexes = obj.indexes;
			if (topology == MeshTopology.Triangles)
			{
				int[] array = indexes;
				int[] array2 = new int[array.Length * 2];
				int num = 0;
				for (int j = 0; j < array.Length; j += 3)
				{
					array2[num++] = array[j];
					array2[num++] = array[j + 1];
					array2[num++] = array[j + 1];
					array2[num++] = array[j + 2];
					array2[num++] = array[j + 2];
					array2[num++] = array[j];
				}
				mesh.SetIndices(array2, MeshTopology.Lines, i);
			}
			else
			{
				mesh.SetIndices(indexes, topology, i);
			}
		}
		return mesh;
	}

	internal static Mesh CreateVertexBillboardMesh(PolyMesh src, int[][] common)
	{
		if (src == null || common == null)
		{
			return null;
		}
		int num = System.Math.Min(16383, common.Count());
		Vector3[] array = new Vector3[num * 4];
		Vector2[] array2 = new Vector2[num * 4];
		Vector2[] array3 = new Vector2[num * 4];
		Color[] array4 = new Color[num * 4];
		int[] array5 = new int[num * 6];
		int num2 = 0;
		int num3 = 0;
		Vector3[] vertices = src.vertices;
		for (int i = 0; i < num; i++)
		{
			int num4 = common[i][0];
			array[num3] = vertices[num4];
			array[num3 + 1] = vertices[num4];
			array[num3 + 2] = vertices[num4];
			array[num3 + 3] = vertices[num4];
			array2[num3] = k_VertexBillboardUV0Content[0];
			array2[num3 + 1] = k_VertexBillboardUV0Content[1];
			array2[num3 + 2] = k_VertexBillboardUV0Content[2];
			array2[num3 + 3] = k_VertexBillboardUV0Content[3];
			array3[num3] = k_VertexBillboardUV2Content[0];
			array3[num3 + 1] = k_VertexBillboardUV2Content[1];
			array3[num3 + 2] = k_VertexBillboardUV2Content[2];
			array3[num3 + 3] = k_VertexBillboardUV2Content[3];
			array5[num2] = num3;
			array5[num2 + 1] = num3 + 1;
			array5[num2 + 2] = num3 + 2;
			array5[num2 + 3] = num3 + 1;
			array5[num2 + 4] = num3 + 3;
			array5[num2 + 5] = num3 + 2;
			array4[num3] = clear;
			array4[num3 + 1] = clear;
			array4[num3 + 2] = clear;
			array4[num3 + 3] = clear;
			num3 += 4;
			num2 += 6;
		}
		return new Mesh
		{
			vertices = array,
			uv = array2,
			uv2 = array3,
			colors = array4,
			triangles = array5
		};
	}

	internal static Dictionary<int, Vector3> GetSmoothNormalLookup(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		Vector3[] normals = mesh.normals;
		Dictionary<int, Vector3> dictionary = new Dictionary<int, Vector3>();
		if (normals == null || normals.Length != mesh.vertexCount)
		{
			return dictionary;
		}
		int[][] commonVertices = GetCommonVertices(mesh);
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		int[][] array = commonVertices;
		foreach (int[] array2 in array)
		{
			zero.x = 0f;
			zero.y = 0f;
			zero.z = 0f;
			int[] array3 = array2;
			foreach (int num in array3)
			{
				zero2 = normals[num];
				zero.x += zero2.x;
				zero.y += zero2.y;
				zero.z += zero2.z;
			}
			zero /= (float)array2.Count();
			array3 = array2;
			foreach (int key in array3)
			{
				dictionary.Add(key, zero);
			}
		}
		return dictionary;
	}

	internal static int[][] GetCommonVertices(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		if (commonVerticesCache.TryGetValue(mesh, out var value) && value.IsValidForMesh(mesh.mesh))
		{
			return value.indices;
		}
		if (commonVerticesCache.ContainsKey(mesh))
		{
			value = (commonVerticesCache[mesh] = new CommonVertexCache(mesh.mesh));
		}
		else
		{
			Dictionary<PolyMesh, CommonVertexCache> dictionary = commonVerticesCache;
			value = new CommonVertexCache(mesh.mesh);
			dictionary.Add(mesh, value);
		}
		return value.indices;
	}

	internal static List<CommonEdge> GetEdges(PolyMesh m)
	{
		Dictionary<int, int> commonLookup = GetCommonVertices(m).GetCommonLookup();
		return GetEdges(m, commonLookup);
	}

	internal static List<CommonEdge> GetEdges(PolyMesh m, Dictionary<int, int> lookup)
	{
		int[] triangles = m.GetTriangles();
		int num = triangles.Length;
		List<CommonEdge> list = new List<CommonEdge>(num);
		for (int i = 0; i < num; i += 3)
		{
			list.Add(new CommonEdge(triangles[i], triangles[i + 1], lookup[triangles[i]], lookup[triangles[i + 1]]));
			list.Add(new CommonEdge(triangles[i + 1], triangles[i + 2], lookup[triangles[i + 1]], lookup[triangles[i + 2]]));
			list.Add(new CommonEdge(triangles[i + 2], triangles[i], lookup[triangles[i + 2]], lookup[triangles[i]]));
		}
		return list;
	}

	internal static HashSet<CommonEdge> GetEdgesDistinct(PolyMesh mesh, out List<CommonEdge> duplicates)
	{
		if (mesh == null)
		{
			duplicates = null;
			return null;
		}
		Dictionary<int, int> commonLookup = GetCommonVertices(mesh).GetCommonLookup();
		return GetEdgesDistinct(mesh, commonLookup, out duplicates);
	}

	private static HashSet<CommonEdge> GetEdgesDistinct(PolyMesh m, Dictionary<int, int> lookup, out List<CommonEdge> duplicates)
	{
		int[] triangles = m.GetTriangles();
		int num = triangles.Length;
		HashSet<CommonEdge> hashSet = new HashSet<CommonEdge>();
		duplicates = new List<CommonEdge>();
		for (int i = 0; i < num; i += 3)
		{
			CommonEdge item = new CommonEdge(triangles[i], triangles[i + 1], lookup[triangles[i]], lookup[triangles[i + 1]]);
			CommonEdge item2 = new CommonEdge(triangles[i + 1], triangles[i + 2], lookup[triangles[i + 1]], lookup[triangles[i + 2]]);
			CommonEdge item3 = new CommonEdge(triangles[i + 2], triangles[i], lookup[triangles[i + 2]], lookup[triangles[i]]);
			if (!hashSet.Add(item))
			{
				duplicates.Add(item);
			}
			if (!hashSet.Add(item2))
			{
				duplicates.Add(item2);
			}
			if (!hashSet.Add(item3))
			{
				duplicates.Add(item3);
			}
		}
		return hashSet;
	}

	internal static HashSet<int> GetNonManifoldIndices(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		List<CommonEdge> duplicates;
		HashSet<CommonEdge> edgesDistinct = GetEdgesDistinct(mesh, out duplicates);
		edgesDistinct.ExceptWith(duplicates);
		return CommonEdge.ToHashSet(edgesDistinct);
	}

	internal static Dictionary<int, int[]> GetAdjacentVertices(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		int[][] commonVertices = GetCommonVertices(mesh);
		Dictionary<int, int> commonLookup = commonVertices.GetCommonLookup();
		List<CommonEdge> edges = GetEdges(mesh, commonLookup);
		List<List<int>> list = new List<List<int>>();
		for (int i = 0; i < commonVertices.Count(); i++)
		{
			list.Add(new List<int>());
		}
		for (int j = 0; j < edges.Count; j++)
		{
			list[edges[j].cx].Add(edges[j].y);
			list[edges[j].cy].Add(edges[j].x);
		}
		Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
		foreach (int item in mesh.GetTriangles().Distinct())
		{
			dictionary.Add(item, list[commonLookup[item]].ToArray());
		}
		return dictionary;
	}

	internal static Dictionary<PolyEdge, List<int>> GetAdjacentTriangles(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		int num = mesh.GetTriangles().Length;
		if (num % 3 != 0 || num / 3 == mesh.vertexCount)
		{
			return new Dictionary<PolyEdge, List<int>>();
		}
		Dictionary<PolyEdge, List<int>> value = null;
		if (adjacentTrianglesCache.TryGetValue(mesh, out value) && value.Count == mesh.vertexCount)
		{
			return value;
		}
		if (adjacentTrianglesCache.ContainsKey(mesh))
		{
			adjacentTrianglesCache.Remove(mesh);
		}
		int subMeshCount = mesh.subMeshCount;
		value = new Dictionary<PolyEdge, List<int>>();
		for (int i = 0; i < subMeshCount; i++)
		{
			int[] indexes = mesh.subMeshes[i].indexes;
			for (int j = 0; j < indexes.Length; j += 3)
			{
				int item = j / 3;
				PolyEdge key = new PolyEdge(indexes[j], indexes[j + 1]);
				PolyEdge key2 = new PolyEdge(indexes[j + 1], indexes[j + 2]);
				PolyEdge key3 = new PolyEdge(indexes[j + 2], indexes[j]);
				if (value.TryGetValue(key, out var value2))
				{
					value2.Add(item);
				}
				else
				{
					value.Add(key, new List<int> { item });
				}
				if (value.TryGetValue(key2, out value2))
				{
					value2.Add(item);
				}
				else
				{
					value.Add(key2, new List<int> { item });
				}
				if (value.TryGetValue(key3, out value2))
				{
					value2.Add(item);
					continue;
				}
				value.Add(key3, new List<int> { item });
			}
		}
		adjacentTrianglesCache.Add(mesh, value);
		return value;
	}

	internal static int[][] GetSmoothSeamLookup(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return null;
		}
		Vector3[] normals = mesh.normals;
		if (normals == null)
		{
			return null;
		}
		int[][] value = null;
		if (commonNormalsCache.TryGetValue(mesh, out value))
		{
			return value;
		}
		int[][] array = (from t in GetCommonVertices(mesh).SelectMany((int[] x) => ((IEnumerable<int>)x).GroupBy((Func<int, RndVec3>)((int i) => normals[i])))
			where t.Count() > 1
			select t.ToArray()).ToArray();
		commonNormalsCache.Add(mesh, array);
		return array;
	}

	internal static void RecalculateNormals(PolyMesh mesh)
	{
		if (mesh == null)
		{
			return;
		}
		int[][] smoothSeamLookup = GetSmoothSeamLookup(mesh);
		mesh.RecalculateNormals();
		if (smoothSeamLookup == null)
		{
			return;
		}
		Vector3[] normals = mesh.normals;
		foreach (int[] array in smoothSeamLookup)
		{
			Vector3 vector = Math.Average(normals, array);
			for (int j = 0; j < array.Length; j++)
			{
				normals[array[j]] = vector;
			}
		}
		mesh.normals = normals;
	}
}
