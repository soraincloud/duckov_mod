using System;

namespace UnityEngine.Rendering;

[ExecuteAlways]
[AddComponentMenu("Rendering/Probe Volume")]
public class ProbeVolume : MonoBehaviour
{
	public enum Mode
	{
		Global,
		Scene,
		Local
	}

	private enum Version
	{
		Initial,
		LocalMode,
		Count
	}

	[Tooltip("When set to Global this Probe Volume considers all renderers with Contribute Global Illumination enabled. Local only considers renderers in the scene.\nThis list updates every time the Scene is saved or the lighting is baked.")]
	public Mode mode = Mode.Scene;

	public Vector3 size = new Vector3(10f, 10f, 10f);

	[HideInInspector]
	[Min(0f)]
	public bool overrideRendererFilters;

	[HideInInspector]
	[Min(0f)]
	public float minRendererVolumeSize = 0.1f;

	public LayerMask objectLayerMask = -1;

	[HideInInspector]
	public int lowestSubdivLevelOverride;

	[HideInInspector]
	public int highestSubdivLevelOverride = -1;

	[HideInInspector]
	public bool overridesSubdivLevels;

	[SerializeField]
	internal bool mightNeedRebaking;

	[SerializeField]
	internal Matrix4x4 cachedTransform;

	[SerializeField]
	internal int cachedHashCode;

	[HideInInspector]
	[Tooltip("Whether spaces with no renderers need to be filled with bricks at lowest subdivision level.")]
	public bool fillEmptySpaces;

	[SerializeField]
	private Version version;

	[SerializeField]
	[Obsolete("Use mode instead")]
	public bool globalVolume;

	private void Awake()
	{
		if (version != Version.Count && version == Version.Initial)
		{
			mode = (globalVolume ? Mode.Scene : Mode.Local);
			version++;
		}
	}
}
