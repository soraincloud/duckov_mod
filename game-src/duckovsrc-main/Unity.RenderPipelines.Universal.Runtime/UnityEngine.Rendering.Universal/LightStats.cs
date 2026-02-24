namespace UnityEngine.Rendering.Universal;

internal struct LightStats
{
	public int totalLights;

	public int totalNormalMapUsage;

	public int totalVolumetricUsage;

	public uint blendStylesUsed;

	public uint blendStylesWithLights;

	public bool useNormalMap => totalNormalMapUsage > 0;
}
