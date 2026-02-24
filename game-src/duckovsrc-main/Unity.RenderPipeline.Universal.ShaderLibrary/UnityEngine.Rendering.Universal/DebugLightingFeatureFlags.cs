using System;

namespace UnityEngine.Rendering.Universal;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Packages\\com.unity.render-pipelines.universal@14.0.11\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
[Flags]
public enum DebugLightingFeatureFlags
{
	None = 0,
	GlobalIllumination = 1,
	MainLight = 2,
	AdditionalLights = 4,
	VertexLighting = 8,
	Emission = 0x10,
	AmbientOcclusion = 0x20
}
