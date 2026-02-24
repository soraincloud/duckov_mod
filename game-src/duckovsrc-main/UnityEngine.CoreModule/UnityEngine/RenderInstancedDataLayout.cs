using System;
using System.Runtime.InteropServices;

namespace UnityEngine;

internal readonly struct RenderInstancedDataLayout
{
	public int size { get; }

	public int offsetObjectToWorld { get; }

	public int offsetPrevObjectToWorld { get; }

	public int offsetRenderingLayerMask { get; }

	public RenderInstancedDataLayout(Type t)
	{
		size = Marshal.SizeOf(t);
		offsetObjectToWorld = ((!(t == typeof(Matrix4x4))) ? Marshal.OffsetOf(t, "objectToWorld").ToInt32() : 0);
		try
		{
			offsetPrevObjectToWorld = Marshal.OffsetOf(t, "prevObjectToWorld").ToInt32();
		}
		catch (ArgumentException)
		{
			offsetPrevObjectToWorld = -1;
		}
		try
		{
			offsetRenderingLayerMask = Marshal.OffsetOf(t, "renderingLayerMask").ToInt32();
		}
		catch (ArgumentException)
		{
			offsetRenderingLayerMask = -1;
		}
	}
}
