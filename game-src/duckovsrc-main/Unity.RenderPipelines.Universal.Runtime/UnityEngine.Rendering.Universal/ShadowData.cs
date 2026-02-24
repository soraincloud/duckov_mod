using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

public struct ShadowData
{
	public bool supportsMainLightShadows;

	internal bool mainLightShadowsEnabled;

	[Obsolete("Obsolete, this feature was replaced by new 'ScreenSpaceShadows' renderer feature")]
	public bool requiresScreenSpaceShadowResolve;

	public int mainLightShadowmapWidth;

	public int mainLightShadowmapHeight;

	public int mainLightShadowCascadesCount;

	public Vector3 mainLightShadowCascadesSplit;

	public float mainLightShadowCascadeBorder;

	public bool supportsAdditionalLightShadows;

	internal bool additionalLightShadowsEnabled;

	public int additionalLightsShadowmapWidth;

	public int additionalLightsShadowmapHeight;

	public bool supportsSoftShadows;

	public int shadowmapDepthBufferBits;

	public List<Vector4> bias;

	public List<int> resolution;

	internal bool isKeywordAdditionalLightShadowsEnabled;

	internal bool isKeywordSoftShadowsEnabled;
}
