using System;

namespace UnityEngine.Experimental.Rendering;

internal readonly struct XRView
{
	internal readonly Matrix4x4 projMatrix;

	internal readonly Matrix4x4 viewMatrix;

	internal readonly Rect viewport;

	internal readonly Mesh occlusionMesh;

	internal readonly int textureArraySlice;

	internal readonly Vector2 eyeCenterUV;

	internal XRView(Matrix4x4 projMatrix, Matrix4x4 viewMatrix, Rect viewport, Mesh occlusionMesh, int textureArraySlice)
	{
		this.projMatrix = projMatrix;
		this.viewMatrix = viewMatrix;
		this.viewport = viewport;
		this.occlusionMesh = occlusionMesh;
		this.textureArraySlice = textureArraySlice;
		eyeCenterUV = ComputeEyeCenterUV(projMatrix);
	}

	private static Vector2 ComputeEyeCenterUV(Matrix4x4 proj)
	{
		FrustumPlanes decomposeProjection = proj.decomposeProjection;
		float num = Math.Abs(decomposeProjection.left);
		float num2 = Math.Abs(decomposeProjection.right);
		float num3 = Math.Abs(decomposeProjection.top);
		float num4 = Math.Abs(decomposeProjection.bottom);
		return new Vector2(num / (num2 + num), num3 / (num3 + num4));
	}
}
