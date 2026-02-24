using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct Smoothen
{
	private static readonly float kMaxAreaTolerance = 1.842f;

	private static readonly float kMaxEdgeTolerance = 2.482f;

	private static void RefineEdges(ref NativeArray<int4> refinedEdges, ref NativeArray<int4> delaEdges, ref int delaEdgeCount, ref NativeArray<int4> voronoiEdges)
	{
		int num = delaEdgeCount;
		delaEdgeCount = 0;
		for (int i = 0; i < num - 1; i++)
		{
			int4 value = delaEdges[i];
			int4 @int = delaEdges[i + 1];
			if (value.x == @int.x && value.y == @int.y)
			{
				value.w = @int.z;
				i++;
			}
			refinedEdges[delaEdgeCount++] = value;
		}
		for (int j = 0; j < delaEdgeCount; j++)
		{
			int z = refinedEdges[j].z;
			int w = refinedEdges[j].w;
			if (z != -1 && w != -1)
			{
				int4 value2 = new int4(w, z, j, 0);
				voronoiEdges[j] = value2;
			}
		}
		ModuleHandle.Copy(refinedEdges, delaEdges, delaEdgeCount);
	}

	private static void GetAffectingEdges(int pointIndex, NativeArray<int4> edges, int edgeCount, ref NativeArray<int> resultSet, ref NativeArray<int> checkSet, ref int resultCount)
	{
		resultCount = 0;
		for (int i = 0; i < edgeCount; i++)
		{
			if (pointIndex == edges[i].x || pointIndex == edges[i].y)
			{
				resultSet[resultCount++] = i;
			}
			checkSet[i] = 0;
		}
	}

	private static void CentroidByPoints(int triIndex, NativeArray<UTriangle> triangles, ref NativeArray<int> centroidTris, ref int centroidCount, ref float2 aggregate, ref float2 point)
	{
		for (int i = 0; i < centroidCount; i++)
		{
			if (triIndex == centroidTris[i])
			{
				return;
			}
		}
		centroidTris[centroidCount++] = triIndex;
		aggregate += triangles[triIndex].c.center;
		point = aggregate / centroidCount;
	}

	private static void CentroidByPolygon(int4 e, NativeArray<UTriangle> triangles, ref float2 centroid, ref float area, ref float distance)
	{
		float2 center = triangles[e.x].c.center;
		float2 center2 = triangles[e.y].c.center;
		float num = center.x * center2.y - center2.x * center.y;
		distance += math.distance(center, center2);
		area += num;
		centroid.x += (center2.x + center.x) * num;
		centroid.y += (center2.y + center.y) * num;
	}

	private static bool ConnectTriangles(ref NativeArray<int4> connectedTri, ref NativeArray<int> affectEdges, ref NativeArray<int> checkSet, NativeArray<int4> voronoiEdges, int triangleCount)
	{
		int index = affectEdges[0];
		int index2 = affectEdges[0];
		connectedTri[0] = new int4(voronoiEdges[index].x, voronoiEdges[index].y, 0, 0);
		checkSet[index2] = 1;
		for (int i = 1; i < triangleCount; i++)
		{
			index2 = affectEdges[i];
			if (checkSet[index2] == 0)
			{
				if (voronoiEdges[index2].x == connectedTri[i - 1].y)
				{
					connectedTri[i] = new int4(voronoiEdges[index2].x, voronoiEdges[index2].y, 0, 0);
					checkSet[index2] = 1;
					continue;
				}
				if (voronoiEdges[index2].y == connectedTri[i - 1].y)
				{
					connectedTri[i] = new int4(voronoiEdges[index2].y, voronoiEdges[index2].x, 0, 0);
					checkSet[index2] = 1;
					continue;
				}
			}
			bool flag = false;
			for (int j = 0; j < triangleCount; j++)
			{
				index2 = affectEdges[j];
				if (checkSet[index2] != 1)
				{
					if (voronoiEdges[index2].x == connectedTri[i - 1].y)
					{
						connectedTri[i] = new int4(voronoiEdges[index2].x, voronoiEdges[index2].y, 0, 0);
						checkSet[index2] = 1;
						flag = true;
						break;
					}
					if (voronoiEdges[index2].y == connectedTri[i - 1].y)
					{
						connectedTri[i] = new int4(voronoiEdges[index2].y, voronoiEdges[index2].x, 0, 0);
						checkSet[index2] = 1;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	internal unsafe static bool Condition(Allocator allocator, ref NativeArray<float2> pgPoints, int pgPointCount, NativeArray<int2> pgEdges, int pgEdgeCount, ref NativeArray<float2> vertices, ref int vertexCount, ref NativeArray<int> indices, ref int indexCount)
	{
		float maxArea = 0f;
		float maxArea2 = 0f;
		float minArea = 0f;
		float minArea2 = 0f;
		float avgArea = 0f;
		float minEdge = 0f;
		float maxEdge = 0f;
		float avgEdge = 0f;
		bool flag = true;
		bool flag2 = true;
		int triangleCount = 0;
		int delaEdgeCount = 0;
		int resultCount = 0;
		NativeArray<UTriangle> triangles = new NativeArray<UTriangle>(indexCount, allocator);
		NativeArray<int4> delaEdges = new NativeArray<int4>(indexCount, allocator);
		NativeArray<int4> voronoiEdges = new NativeArray<int4>(indexCount, allocator);
		NativeArray<int4> connectedTri = new NativeArray<int4>(vertexCount, allocator);
		NativeArray<int> checkSet = new NativeArray<int>(indexCount, allocator);
		NativeArray<int> resultSet = new NativeArray<int>(indexCount, allocator);
		NativeArray<int> nativeArray = new NativeArray<int>(vertexCount, allocator);
		ModuleHandle.BuildTrianglesAndEdges(vertices, vertexCount, indices, indexCount, ref triangles, ref triangleCount, ref delaEdges, ref delaEdgeCount, ref maxArea, ref avgArea, ref minArea);
		NativeArray<int4> refinedEdges = new NativeArray<int4>(delaEdgeCount, allocator);
		ModuleHandle.InsertionSort<int4, DelaEdgeCompare>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(delaEdges), 0, delaEdgeCount - 1, default(DelaEdgeCompare));
		RefineEdges(ref refinedEdges, ref delaEdges, ref delaEdgeCount, ref voronoiEdges);
		for (int i = 0; i < vertexCount; i++)
		{
			GetAffectingEdges(i, delaEdges, delaEdgeCount, ref resultSet, ref checkSet, ref resultCount);
			bool flag3 = resultCount != 0;
			for (int j = 0; j < resultCount; j++)
			{
				int index = resultSet[j];
				if (delaEdges[index].z == -1 || delaEdges[index].w == -1)
				{
					flag3 = false;
					break;
				}
			}
			if (flag3)
			{
				flag = ConnectTriangles(ref connectedTri, ref resultSet, ref checkSet, voronoiEdges, resultCount);
				if (!flag)
				{
					break;
				}
				float2 centroid = float2.zero;
				float area = 0f;
				float distance = 0f;
				for (int k = 0; k < resultCount; k++)
				{
					CentroidByPolygon(connectedTri[k], triangles, ref centroid, ref area, ref distance);
				}
				centroid /= 3f * area;
				pgPoints[i] = centroid;
			}
		}
		int num = indexCount;
		int num2 = vertexCount;
		indexCount = 0;
		vertexCount = 0;
		triangleCount = 0;
		if (flag)
		{
			flag2 = Tessellator.Tessellate(allocator, pgPoints, pgPointCount, pgEdges, pgEdgeCount, ref vertices, ref vertexCount, ref indices, ref indexCount);
			if (flag2)
			{
				ModuleHandle.BuildTriangles(vertices, vertexCount, indices, indexCount, ref triangles, ref triangleCount, ref maxArea2, ref avgArea, ref minArea2, ref maxEdge, ref avgEdge, ref minEdge);
			}
			flag2 = flag2 && maxArea2 < maxArea * kMaxAreaTolerance && maxEdge < avgEdge * kMaxEdgeTolerance;
		}
		triangles.Dispose();
		delaEdges.Dispose();
		refinedEdges.Dispose();
		checkSet.Dispose();
		voronoiEdges.Dispose();
		resultSet.Dispose();
		nativeArray.Dispose();
		connectedTri.Dispose();
		if (flag2 && num == indexCount)
		{
			return num2 == vertexCount;
		}
		return false;
	}
}
