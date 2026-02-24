using System;
using Unity.Profiling;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

public class MeshGenerationContext
{
	[Flags]
	internal enum MeshFlags
	{
		None = 0,
		UVisDisplacement = 1,
		SkipDynamicAtlas = 2
	}

	private Painter2D m_Painter2D;

	private static readonly ProfilerMarker s_AllocateMarker = new ProfilerMarker("UIR.MeshGenerationContext.Allocate");

	private static readonly ProfilerMarker s_DrawVectorImageMarker = new ProfilerMarker("UIR.MeshGenerationContext.DrawVectorImage");

	internal IStylePainter painter;

	public VisualElement visualElement => painter.visualElement;

	public Painter2D painter2D
	{
		get
		{
			if (m_Painter2D == null)
			{
				m_Painter2D = new Painter2D(this);
			}
			return m_Painter2D;
		}
	}

	internal bool hasPainter2D => m_Painter2D != null;

	internal MeshGenerationContext(IStylePainter painter)
	{
		this.painter = painter;
	}

	public MeshWriteData Allocate(int vertexCount, int indexCount, Texture texture = null)
	{
		using (s_AllocateMarker.Auto())
		{
			return painter.DrawMesh(vertexCount, indexCount, texture, null, MeshFlags.None);
		}
	}

	internal MeshWriteData Allocate(int vertexCount, int indexCount, Texture texture, Material material, MeshFlags flags)
	{
		using (s_AllocateMarker.Auto())
		{
			return painter.DrawMesh(vertexCount, indexCount, texture, material, flags);
		}
	}

	public void DrawVectorImage(VectorImage vectorImage, Vector2 offset, Angle rotationAngle, Vector2 scale)
	{
		using (s_DrawVectorImageMarker.Auto())
		{
			painter.DrawVectorImage(vectorImage, offset, rotationAngle, scale);
		}
	}

	public void DrawText(string text, Vector2 pos, float fontSize, Color color, FontAsset font = null)
	{
		if (font == null)
		{
			font = TextUtilities.GetFontAsset(visualElement);
		}
		painter.DrawText(text, pos, fontSize, color, font);
	}
}
