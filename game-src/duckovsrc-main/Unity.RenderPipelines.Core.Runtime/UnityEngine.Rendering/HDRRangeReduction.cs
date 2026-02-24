namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Packages\\com.unity.render-pipelines.core@14.0.11\\Runtime\\PostProcessing\\HDROutputDefines.cs")]
public enum HDRRangeReduction
{
	None,
	Reinhard,
	BT2390,
	ACES1000Nits,
	ACES2000Nits,
	ACES4000Nits
}
