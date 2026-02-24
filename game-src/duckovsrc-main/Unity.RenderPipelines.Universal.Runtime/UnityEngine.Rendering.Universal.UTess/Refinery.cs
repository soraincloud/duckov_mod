using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.UTess;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct Refinery
{
	private static readonly float kMinAreaFactor = 0.0482f;

	private static readonly float kMaxAreaFactor = 0.482f;

	private static readonly int kMaxSteinerCount = 4084;

	private static bool RequiresRefining(UTriangle tri, float maxArea)
	{
		return tri.area > maxArea;
	}

	private static void FetchEncroachedSegments(NativeArray<float2> pgPoints, int pgPointCount, NativeArray<int2> pgEdges, int pgEdgeCount, ref NativeArray<UEncroachingSegment> encroach, ref int encroachCount, UCircle c)
	{
		for (int i = 0; i < pgEdgeCount; i++)
		{
			int2 @int = pgEdges[i];
			float2 @float = pgPoints[@int.x];
			float2 float2 = pgPoints[@int.y];
			if (math.any(c.center - @float) && math.any(c.center - float2))
			{
				float2 x = @float - float2;
				float2 float3 = (@float + float2) * 0.5f;
				float num = math.length(x) * 0.5f;
				if (!(math.length(float3 - c.center) > num))
				{
					UEncroachingSegment value = new UEncroachingSegment
					{
						a = @float,
						b = float2,
						index = i
					};
					encroach[encroachCount++] = value;
				}
			}
		}
	}

	private static void InsertVertex(ref NativeArray<float2> pgPoints, ref int pgPointCount, float2 newVertex, ref int nid)
	{
		nid = pgPointCount;
		pgPoints[nid] = newVertex;
		pgPointCount++;
	}

	private static int FindSegment(NativeArray<float2> pgPoints, int pgPointCount, NativeArray<int2> pgEdges, int pgEdgeCount, UEncroachingSegment es)
	{
		for (int i = es.index; i < pgEdgeCount; i++)
		{
			int2 @int = pgEdges[i];
			float2 @float = pgPoints[@int.x];
			float2 float2 = pgPoints[@int.y];
			if (!math.any(@float - es.a) && !math.any(float2 - es.b))
			{
				return i;
			}
		}
		return -1;
	}

	private static void SplitSegments(ref NativeArray<float2> pgPoints, ref int pgPointCount, ref NativeArray<int2> pgEdges, ref int pgEdgeCount, UEncroachingSegment es)
	{
		int num = FindSegment(pgPoints, pgPointCount, pgEdges, pgEdgeCount, es);
		if (num == -1)
		{
			return;
		}
		int2 @int = pgEdges[num];
		float2 @float = pgPoints[@int.x];
		float2 float2 = pgPoints[@int.y];
		float2 float3 = (@float + float2) * 0.5f;
		int num2 = 0;
		if (math.abs(@int.x - @int.y) == 1)
		{
			num2 = ((@int.x > @int.y) ? @int.x : @int.y);
			InsertVertex(ref pgPoints, ref pgPointCount, float3, ref num2);
			int2 int2 = pgEdges[num];
			pgEdges[num] = new int2(int2.x, num2);
			for (int num3 = pgEdgeCount; num3 > num + 1; num3--)
			{
				pgEdges[num3] = pgEdges[num3 - 1];
			}
			pgEdges[num + 1] = new int2(num2, int2.y);
			pgEdgeCount++;
		}
		else
		{
			num2 = pgPointCount;
			pgPoints[pgPointCount++] = float3;
			pgEdges[num] = new int2(math.max(@int.x, @int.y), num2);
			pgEdges[pgEdgeCount++] = new int2(math.min(@int.x, @int.y), num2);
		}
	}

	internal static bool Condition(Allocator allocator, float factorArea, float targetArea, ref NativeArray<float2> pgPoints, ref int pgPointCount, ref NativeArray<int2> pgEdges, ref int pgEdgeCount, ref NativeArray<float2> vertices, ref int vertexCount, ref NativeArray<int> indices, ref int indexCount, ref float maxArea)
	{
		maxArea = 0f;
		float minArea = 0f;
		float avgArea = 0f;
		bool flag = false;
		bool flag2 = true;
		int triangleCount = 0;
		int num = -1;
		int num2 = pgPointCount;
		NativeArray<UEncroachingSegment> encroach = new NativeArray<UEncroachingSegment>(ModuleHandle.kMaxEdgeCount, allocator);
		NativeArray<UTriangle> triangles = new NativeArray<UTriangle>(ModuleHandle.kMaxTriangleCount, allocator);
		ModuleHandle.BuildTriangles(vertices, vertexCount, indices, indexCount, ref triangles, ref triangleCount, ref maxArea, ref avgArea, ref minArea);
		factorArea = ((factorArea != 0f) ? math.clamp(factorArea, kMinAreaFactor, kMaxAreaFactor) : factorArea);
		float x = maxArea * factorArea;
		x = math.max(x, targetArea);
		while (!flag && flag2)
		{
			for (int i = 0; i < triangleCount; i++)
			{
				if (RequiresRefining(triangles[i], x))
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				UTriangle uTriangle = triangles[num];
				int encroachCount = 0;
				FetchEncroachedSegments(pgPoints, pgPointCount, pgEdges, pgEdgeCount, ref encroach, ref encroachCount, uTriangle.c);
				if (encroachCount != 0)
				{
					for (int j = 0; j < encroachCount; j++)
					{
						SplitSegments(ref pgPoints, ref pgPointCount, ref pgEdges, ref pgEdgeCount, encroach[j]);
					}
				}
				else
				{
					float2 center = uTriangle.c.center;
					pgPoints[pgPointCount++] = center;
				}
				indexCount = 0;
				vertexCount = 0;
				flag2 = Tessellator.Tessellate(allocator, pgPoints, pgPointCount, pgEdges, pgEdgeCount, ref vertices, ref vertexCount, ref indices, ref indexCount);
				encroachCount = 0;
				triangleCount = 0;
				num = -1;
				if (flag2)
				{
					ModuleHandle.BuildTriangles(vertices, vertexCount, indices, indexCount, ref triangles, ref triangleCount, ref maxArea, ref avgArea, ref minArea);
				}
				if (pgPointCount - num2 > kMaxSteinerCount)
				{
					break;
				}
			}
			else
			{
				flag = true;
			}
		}
		triangles.Dispose();
		encroach.Dispose();
		return flag;
	}
}
