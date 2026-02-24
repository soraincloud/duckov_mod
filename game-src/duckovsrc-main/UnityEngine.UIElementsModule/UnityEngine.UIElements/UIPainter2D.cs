using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.UIElements;

[NativeHeader("ModuleOverrides/com.unity.ui/Core/Native/Renderer/UIPainter2D.bindings.h")]
internal static class UIPainter2D
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern IntPtr Create(bool computeBBox = false);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void Destroy(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void Reset(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern float GetLineWidth(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetLineWidth(IntPtr handle, float value);

	public static Color GetStrokeColor(IntPtr handle)
	{
		GetStrokeColor_Injected(handle, out var ret);
		return ret;
	}

	public static void SetStrokeColor(IntPtr handle, Color value)
	{
		SetStrokeColor_Injected(handle, ref value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetStrokeGradientCopy")]
	public static extern Gradient GetStrokeGradient(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetStrokeGradient(IntPtr handle, Gradient gradient);

	public static Color GetFillColor(IntPtr handle)
	{
		GetFillColor_Injected(handle, out var ret);
		return ret;
	}

	public static void SetFillColor(IntPtr handle, Color value)
	{
		SetFillColor_Injected(handle, ref value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern LineJoin GetLineJoin(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetLineJoin(IntPtr handle, LineJoin value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern LineCap GetLineCap(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetLineCap(IntPtr handle, LineCap value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern float GetMiterLimit(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void SetMiterLimit(IntPtr handle, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void BeginPath(IntPtr handle);

	public static void MoveTo(IntPtr handle, Vector2 pos)
	{
		MoveTo_Injected(handle, ref pos);
	}

	public static void LineTo(IntPtr handle, Vector2 pos)
	{
		LineTo_Injected(handle, ref pos);
	}

	public static void ArcTo(IntPtr handle, Vector2 p1, Vector2 p2, float radius)
	{
		ArcTo_Injected(handle, ref p1, ref p2, radius);
	}

	public static void Arc(IntPtr handle, Vector2 center, float radius, float startAngleRads, float endAngleRads, ArcDirection direction)
	{
		Arc_Injected(handle, ref center, radius, startAngleRads, endAngleRads, direction);
	}

	public static void BezierCurveTo(IntPtr handle, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		BezierCurveTo_Injected(handle, ref p1, ref p2, ref p3);
	}

	public static void QuadraticCurveTo(IntPtr handle, Vector2 p1, Vector2 p2)
	{
		QuadraticCurveTo_Injected(handle, ref p1, ref p2);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void ClosePath(IntPtr handle);

	public static Rect GetBBox(IntPtr handle)
	{
		GetBBox_Injected(handle, out var ret);
		return ret;
	}

	public static MeshWriteDataInterface Stroke(IntPtr handle)
	{
		Stroke_Injected(handle, out var ret);
		return ret;
	}

	public static MeshWriteDataInterface Fill(IntPtr handle, FillRule fillRule)
	{
		Fill_Injected(handle, fillRule, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetStrokeColor_Injected(IntPtr handle, out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetStrokeColor_Injected(IntPtr handle, ref Color value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetFillColor_Injected(IntPtr handle, out Color ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetFillColor_Injected(IntPtr handle, ref Color value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void MoveTo_Injected(IntPtr handle, ref Vector2 pos);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void LineTo_Injected(IntPtr handle, ref Vector2 pos);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ArcTo_Injected(IntPtr handle, ref Vector2 p1, ref Vector2 p2, float radius);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Arc_Injected(IntPtr handle, ref Vector2 center, float radius, float startAngleRads, float endAngleRads, ArcDirection direction);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void BezierCurveTo_Injected(IntPtr handle, ref Vector2 p1, ref Vector2 p2, ref Vector2 p3);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void QuadraticCurveTo_Injected(IntPtr handle, ref Vector2 p1, ref Vector2 p2);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetBBox_Injected(IntPtr handle, out Rect ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Stroke_Injected(IntPtr handle, out MeshWriteDataInterface ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Fill_Injected(IntPtr handle, FillRule fillRule, out MeshWriteDataInterface ret);
}
