using System;

namespace UnityEngine.Rendering;

[Serializable]
internal struct VirtualOffsetSettings
{
	public bool useVirtualOffset;

	[Range(0f, 1f)]
	public float outOfGeoOffset;

	[Range(0f, 2f)]
	public float searchMultiplier;

	[Range(-0.05f, 0f)]
	public float rayOriginBias;

	[Range(4f, 24f)]
	public int maxHitsPerRay;

	public LayerMask collisionMask;

	internal void SetDefaults()
	{
		useVirtualOffset = true;
		outOfGeoOffset = 0.01f;
		searchMultiplier = 0.2f;
		UpgradeFromTo(ProbeVolumeBakingProcessSettings.SettingsVersion.Initial, ProbeVolumeBakingProcessSettings.SettingsVersion.ThreadedVirtualOffset);
	}

	internal void UpgradeFromTo(ProbeVolumeBakingProcessSettings.SettingsVersion from, ProbeVolumeBakingProcessSettings.SettingsVersion to)
	{
		if (from < ProbeVolumeBakingProcessSettings.SettingsVersion.ThreadedVirtualOffset && to >= ProbeVolumeBakingProcessSettings.SettingsVersion.ThreadedVirtualOffset)
		{
			rayOriginBias = -0.001f;
			maxHitsPerRay = 10;
			collisionMask = -5;
		}
	}
}
