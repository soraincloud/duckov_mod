using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal struct PostProcessParams
{
	public Material blitMaterial;

	public GraphicsFormat requestHDRFormat;

	public static PostProcessParams Create()
	{
		PostProcessParams result = default(PostProcessParams);
		result.blitMaterial = null;
		result.requestHDRFormat = GraphicsFormat.None;
		return result;
	}
}
