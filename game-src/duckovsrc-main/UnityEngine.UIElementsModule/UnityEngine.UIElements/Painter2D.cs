using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

public class Painter2D : IDisposable
{
	private MeshGenerationContext m_Ctx;

	internal DetachedAllocator m_DetachedAllocator;

	internal SafeHandleAccess m_Handle;

	private bool m_Disposed;

	private static float s_MaxArcRadius = -1f;

	private static readonly ProfilerMarker s_StrokeMarker = new ProfilerMarker("Painter2D.Stroke");

	private static readonly ProfilerMarker s_FillMarker = new ProfilerMarker("Painter2D.Fill");

	internal bool isDetached => m_DetachedAllocator != null;

	public float lineWidth
	{
		get
		{
			return UIPainter2D.GetLineWidth(m_Handle);
		}
		set
		{
			UIPainter2D.SetLineWidth(m_Handle, value);
		}
	}

	public Color strokeColor
	{
		get
		{
			return UIPainter2D.GetStrokeColor(m_Handle);
		}
		set
		{
			UIPainter2D.SetStrokeColor(m_Handle, value);
		}
	}

	public Gradient strokeGradient
	{
		get
		{
			return UIPainter2D.GetStrokeGradient(m_Handle);
		}
		set
		{
			UIPainter2D.SetStrokeGradient(m_Handle, value);
		}
	}

	public Color fillColor
	{
		get
		{
			return UIPainter2D.GetFillColor(m_Handle);
		}
		set
		{
			UIPainter2D.SetFillColor(m_Handle, value);
		}
	}

	public LineJoin lineJoin
	{
		get
		{
			return UIPainter2D.GetLineJoin(m_Handle);
		}
		set
		{
			UIPainter2D.SetLineJoin(m_Handle, value);
		}
	}

	public LineCap lineCap
	{
		get
		{
			return UIPainter2D.GetLineCap(m_Handle);
		}
		set
		{
			UIPainter2D.SetLineCap(m_Handle, value);
		}
	}

	public float miterLimit
	{
		get
		{
			return UIPainter2D.GetMiterLimit(m_Handle);
		}
		set
		{
			UIPainter2D.SetMiterLimit(m_Handle, value);
		}
	}

	internal static bool isPainterActive { get; set; }

	private static float maxArcRadius
	{
		get
		{
			if (s_MaxArcRadius < 0f)
			{
				if (!UIRenderDevice.vertexTexturingIsAvailable)
				{
					s_MaxArcRadius = 1000f;
				}
				else
				{
					s_MaxArcRadius = 100000f;
				}
			}
			return s_MaxArcRadius;
		}
	}

	internal Painter2D(MeshGenerationContext ctx)
	{
		m_Handle = new SafeHandleAccess(UIPainter2D.Create());
		m_Ctx = ctx;
		Reset();
	}

	public Painter2D()
	{
		m_Handle = new SafeHandleAccess(UIPainter2D.Create(computeBBox: true));
		m_DetachedAllocator = new DetachedAllocator();
		isPainterActive = true;
		Reset();
	}

	internal void Reset()
	{
		UIPainter2D.Reset(m_Handle);
	}

	internal MeshWriteData Allocate(int vertexCount, int indexCount)
	{
		if (isDetached)
		{
			return m_DetachedAllocator.Alloc(vertexCount, indexCount);
		}
		return m_Ctx.Allocate(vertexCount, indexCount);
	}

	public void Clear()
	{
		if (!isDetached)
		{
			Debug.LogError("Clear() cannot be called on a Painter2D associated with a MeshGenerationContext. You should create your own instance of Painter2D instead.");
			return;
		}
		m_DetachedAllocator.Clear();
		Reset();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (m_Disposed)
		{
			return;
		}
		if (disposing)
		{
			if (!m_Handle.IsNull())
			{
				UIPainter2D.Destroy(m_Handle);
				m_Handle = new SafeHandleAccess(IntPtr.Zero);
			}
			if (m_DetachedAllocator != null)
			{
				m_DetachedAllocator.Dispose();
			}
		}
		m_Disposed = true;
	}

	private bool ValidateState()
	{
		bool flag = isDetached || isPainterActive;
		if (!flag)
		{
			Debug.LogError("Cannot issue vector graphics commands outside of generateVisualContent callback");
		}
		return flag;
	}

	public void BeginPath()
	{
		if (ValidateState())
		{
			UIPainter2D.BeginPath(m_Handle);
		}
	}

	public void ClosePath()
	{
		if (ValidateState())
		{
			UIPainter2D.ClosePath(m_Handle);
		}
	}

	public void MoveTo(Vector2 pos)
	{
		if (ValidateState())
		{
			UIPainter2D.MoveTo(m_Handle, pos);
		}
	}

	public void LineTo(Vector2 pos)
	{
		if (ValidateState())
		{
			UIPainter2D.LineTo(m_Handle, pos);
		}
	}

	public void ArcTo(Vector2 p1, Vector2 p2, float radius)
	{
		if (ValidateState())
		{
			UIPainter2D.ArcTo(m_Handle, p1, p2, radius);
		}
	}

	public void Arc(Vector2 center, float radius, Angle startAngle, Angle endAngle, ArcDirection direction = ArcDirection.Clockwise)
	{
		if (ValidateState())
		{
			UIPainter2D.Arc(m_Handle, center, radius, startAngle.ToRadians(), endAngle.ToRadians(), direction);
		}
	}

	public void BezierCurveTo(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		if (ValidateState())
		{
			UIPainter2D.BezierCurveTo(m_Handle, p1, p2, p3);
		}
	}

	public void QuadraticCurveTo(Vector2 p1, Vector2 p2)
	{
		if (ValidateState())
		{
			UIPainter2D.QuadraticCurveTo(m_Handle, p1, p2);
		}
	}

	public unsafe void Stroke()
	{
		using (s_StrokeMarker.Auto())
		{
			if (ValidateState())
			{
				MeshWriteDataInterface meshWriteDataInterface = UIPainter2D.Stroke(m_Handle);
				if (meshWriteDataInterface.vertexCount != 0)
				{
					MeshWriteData meshWriteData = Allocate(meshWriteDataInterface.vertexCount, meshWriteDataInterface.indexCount);
					NativeSlice<Vertex> allVertices = UIRenderDevice.PtrToSlice<Vertex>((void*)meshWriteDataInterface.vertices, meshWriteDataInterface.vertexCount);
					NativeSlice<ushort> allIndices = UIRenderDevice.PtrToSlice<ushort>((void*)meshWriteDataInterface.indices, meshWriteDataInterface.indexCount);
					meshWriteData.SetAllVertices(allVertices);
					meshWriteData.SetAllIndices(allIndices);
				}
			}
		}
	}

	public unsafe void Fill(FillRule fillRule = FillRule.NonZero)
	{
		using (s_FillMarker.Auto())
		{
			if (ValidateState())
			{
				MeshWriteDataInterface meshWriteDataInterface = UIPainter2D.Fill(m_Handle, fillRule);
				if (meshWriteDataInterface.vertexCount != 0)
				{
					MeshWriteData meshWriteData = Allocate(meshWriteDataInterface.vertexCount, meshWriteDataInterface.indexCount);
					NativeSlice<Vertex> allVertices = UIRenderDevice.PtrToSlice<Vertex>((void*)meshWriteDataInterface.vertices, meshWriteDataInterface.vertexCount);
					NativeSlice<ushort> allIndices = UIRenderDevice.PtrToSlice<ushort>((void*)meshWriteDataInterface.indices, meshWriteDataInterface.indexCount);
					meshWriteData.SetAllVertices(allVertices);
					meshWriteData.SetAllIndices(allIndices);
				}
			}
		}
	}

	public bool SaveToVectorImage(VectorImage vectorImage)
	{
		if (!isDetached)
		{
			Debug.LogError("SaveToVectorImage cannot be called on a Painter2D associated with a MeshGenerationContext. You should create your own instance of Painter2D instead.");
			return false;
		}
		if (vectorImage == null)
		{
			throw new NullReferenceException("The provided vectorImage is null");
		}
		List<MeshWriteData> meshes = m_DetachedAllocator.meshes;
		int num = 0;
		int num2 = 0;
		foreach (MeshWriteData item in meshes)
		{
			num += item.m_Vertices.Length;
			num2 += item.m_Indices.Length;
		}
		Rect bBox = UIPainter2D.GetBBox(m_Handle);
		VectorImageVertex[] array = new VectorImageVertex[num];
		ushort[] array2 = new ushort[num2];
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		foreach (MeshWriteData item2 in meshes)
		{
			NativeSlice<Vertex> vertices = item2.m_Vertices;
			for (int i = 0; i < vertices.Length; i++)
			{
				Vertex vertex = vertices[i];
				Vector3 position = vertex.position;
				position.x -= bBox.x;
				position.y -= bBox.y;
				array[num3++] = new VectorImageVertex
				{
					position = new Vector3(position.x, position.y, Vertex.nearZ),
					tint = vertex.tint,
					uv = vertex.uv,
					flags = vertex.flags,
					circle = vertex.circle
				};
			}
			NativeSlice<ushort> indices = item2.m_Indices;
			for (int j = 0; j < indices.Length; j++)
			{
				array2[num4++] = (ushort)(indices[j] + num5);
			}
			num5 += vertices.Length;
		}
		vectorImage.version = 0;
		vectorImage.vertices = array;
		vectorImage.indices = array2;
		vectorImage.size = bBox.size;
		return true;
	}
}
