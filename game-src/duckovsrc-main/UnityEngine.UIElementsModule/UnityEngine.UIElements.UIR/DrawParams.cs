using System.Collections.Generic;

namespace UnityEngine.UIElements.UIR;

internal class DrawParams
{
	internal static readonly Rect k_UnlimitedRect = new Rect(-100000f, -100000f, 200000f, 200000f);

	internal static readonly Rect k_FullNormalizedRect = new Rect(-1f, -1f, 2f, 2f);

	internal readonly Stack<Matrix4x4> view = new Stack<Matrix4x4>(8);

	internal readonly Stack<Rect> scissor = new Stack<Rect>(8);

	internal readonly List<RenderTexture> renderTexture = new List<RenderTexture>(8);

	internal readonly List<Material> defaultMaterial = new List<Material>(8);

	public void Reset()
	{
		view.Clear();
		view.Push(Matrix4x4.identity);
		scissor.Clear();
		scissor.Push(k_UnlimitedRect);
		renderTexture.Clear();
		defaultMaterial.Clear();
	}
}
