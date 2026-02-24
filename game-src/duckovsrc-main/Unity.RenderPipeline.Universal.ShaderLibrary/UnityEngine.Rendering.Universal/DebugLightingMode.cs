namespace UnityEngine.Rendering.Universal;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Packages\\com.unity.render-pipelines.universal@14.0.11\\ShaderLibrary\\Debug\\DebugViewEnums.cs")]
public enum DebugLightingMode
{
	None,
	ShadowCascades,
	LightingWithoutNormalMaps,
	LightingWithNormalMaps,
	Reflections,
	ReflectionsWithSmoothness
}
