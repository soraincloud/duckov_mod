using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal interface ILight2DCullResult
{
	List<Light2D> visibleLights { get; }

	List<ShadowCasterGroup2D> visibleShadows { get; }

	LightStats GetLightStatsByLayer(int layer);

	bool IsSceneLit();
}
