namespace UnityEngine.Rendering;

public sealed class ProbeReferenceVolumeProfile : ScriptableObject
{
	internal enum Version
	{
		Initial
	}

	[SerializeField]
	private Version version = CoreUtils.GetLastEnumValue<Version>();

	[SerializeField]
	internal bool freezePlacement;

	[Range(2f, 5f)]
	public int simplificationLevels = 3;

	[Min(0.1f)]
	public float minDistanceBetweenProbes = 1f;

	public LayerMask renderersLayerMask = -1;

	[Min(0f)]
	public float minRendererVolumeSize = 0.1f;

	public int cellSizeInBricks => (int)Mathf.Pow(3f, simplificationLevels);

	public int maxSubdivision => simplificationLevels + 1;

	public float minBrickSize => Mathf.Max(0.01f, minDistanceBetweenProbes * 3f);

	public float cellSizeInMeters => (float)cellSizeInBricks * minBrickSize;

	private void OnEnable()
	{
		_ = version;
		CoreUtils.GetLastEnumValue<Version>();
	}

	public bool IsEquivalent(ProbeReferenceVolumeProfile otherProfile)
	{
		if (minDistanceBetweenProbes == otherProfile.minDistanceBetweenProbes && cellSizeInMeters == otherProfile.cellSizeInMeters && simplificationLevels == otherProfile.simplificationLevels)
		{
			return (int)renderersLayerMask == (int)otherProfile.renderersLayerMask;
		}
		return false;
	}
}
