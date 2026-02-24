using Unity.Collections;
using Unity.Profiling;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements.UIR;

internal static class MeshBuilder
{
	internal struct AllocMeshData
	{
		internal delegate MeshWriteData Allocator(uint vertexCount, uint indexCount, ref AllocMeshData allocatorData);

		internal Allocator alloc;

		internal Texture texture;

		internal TextureId svgTexture;

		internal Material material;

		internal MeshGenerationContext.MeshFlags flags;

		internal BMPAlloc colorAlloc;

		internal MeshWriteData Allocate(uint vertexCount, uint indexCount)
		{
			return alloc(vertexCount, indexCount, ref this);
		}
	}

	private static ProfilerMarker s_VectorGraphics9Slice = new ProfilerMarker("UIR.MakeVector9Slice");

	private static ProfilerMarker s_VectorGraphicsSplitTriangle = new ProfilerMarker("UIR.SplitTriangle");

	private static ProfilerMarker s_VectorGraphicsScaleTriangle = new ProfilerMarker("UIR.ScaleTriangle");

	private static ProfilerMarker s_VectorGraphicsStretch = new ProfilerMarker("UIR.MakeVectorStretch");

	internal static readonly int s_MaxTextMeshVertices = 49152;

	private static Vertex ConvertTextVertexToUIRVertex(MeshInfo info, int index, Vector2 offset, VertexFlags flags = VertexFlags.IsText, bool isDynamicColor = false)
	{
		float num = 0f;
		if (info.uvs2[index].y < 0f)
		{
			num = 1f;
		}
		return new Vertex
		{
			position = new Vector3(info.vertices[index].x + offset.x, info.vertices[index].y + offset.y, 0f),
			uv = new Vector2(info.uvs0[index].x, info.uvs0[index].y),
			tint = info.colors32[index],
			flags = new Color32((byte)flags, (byte)(num * 255f), 0, (byte)(isDynamicColor ? 1 : 0))
		};
	}

	private static Vertex ConvertTextVertexToUIRVertex(TextVertex textVertex, Vector2 offset)
	{
		return new Vertex
		{
			position = new Vector3(textVertex.position.x + offset.x, textVertex.position.y + offset.y, 0f),
			uv = textVertex.uv0,
			tint = textVertex.color,
			flags = new Color32(1, 0, 0, 0)
		};
	}

	private static int LimitTextVertices(int vertexCount, bool logTruncation = true)
	{
		if (vertexCount <= s_MaxTextMeshVertices)
		{
			return vertexCount;
		}
		if (logTruncation)
		{
			Debug.LogWarning($"Generated text will be truncated because it exceeds {s_MaxTextMeshVertices} vertices.");
		}
		return s_MaxTextMeshVertices;
	}

	internal static void MakeText(MeshInfo meshInfo, Vector2 offset, AllocMeshData meshAlloc, VertexFlags flags = VertexFlags.IsText, bool isDynamicColor = false)
	{
		int num = LimitTextVertices(meshInfo.vertexCount);
		int num2 = num / 4;
		MeshWriteData meshWriteData = meshAlloc.Allocate((uint)(num2 * 4), (uint)(num2 * 6));
		int num3 = 0;
		int num4 = 0;
		while (num3 < num2)
		{
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4, offset, flags, isDynamicColor));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4 + 1, offset, flags, isDynamicColor));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4 + 2, offset, flags, isDynamicColor));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(meshInfo, num4 + 3, offset, flags, isDynamicColor));
			meshWriteData.SetNextIndex((ushort)num4);
			meshWriteData.SetNextIndex((ushort)(num4 + 1));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 3));
			meshWriteData.SetNextIndex((ushort)num4);
			num3++;
			num4 += 4;
		}
	}

	internal static void MakeText(NativeArray<TextVertex> uiVertices, Vector2 offset, AllocMeshData meshAlloc)
	{
		int num = LimitTextVertices(uiVertices.Length);
		int num2 = num / 4;
		MeshWriteData meshWriteData = meshAlloc.Allocate((uint)(num2 * 4), (uint)(num2 * 6));
		int num3 = 0;
		int num4 = 0;
		while (num3 < num2)
		{
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4], offset));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4 + 1], offset));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4 + 2], offset));
			meshWriteData.SetNextVertex(ConvertTextVertexToUIRVertex(uiVertices[num4 + 3], offset));
			meshWriteData.SetNextIndex((ushort)num4);
			meshWriteData.SetNextIndex((ushort)(num4 + 1));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 2));
			meshWriteData.SetNextIndex((ushort)(num4 + 3));
			meshWriteData.SetNextIndex((ushort)num4);
			num3++;
			num4 += 4;
		}
	}
}
