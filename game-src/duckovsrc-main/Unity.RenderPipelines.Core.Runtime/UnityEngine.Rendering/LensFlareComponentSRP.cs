using System;

namespace UnityEngine.Rendering;

[ExecuteAlways]
[AddComponentMenu("Rendering/Lens Flare (SRP)")]
public sealed class LensFlareComponentSRP : MonoBehaviour
{
	[SerializeField]
	private LensFlareDataSRP m_LensFlareData;

	[Min(0f)]
	public float intensity = 1f;

	[Min(1E-05f)]
	public float maxAttenuationDistance = 100f;

	[Min(1E-05f)]
	public float maxAttenuationScale = 100f;

	public AnimationCurve distanceAttenuationCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	public AnimationCurve scaleByDistanceCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

	public bool attenuationByLightShape = true;

	public AnimationCurve radialScreenAttenuationCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	public bool useOcclusion;

	[Min(0f)]
	public float occlusionRadius = 0.1f;

	public bool useBackgroundCloudOcclusion;

	[Range(1f, 64f)]
	public uint sampleCount = 32u;

	public float occlusionOffset = 0.05f;

	[Min(0f)]
	public float scale = 1f;

	public bool allowOffScreen;

	public bool volumetricCloudOcclusion;

	private static float sCelestialAngularRadius = 0.057595868f;

	public TextureCurve occlusionRemapCurve = new TextureCurve(AnimationCurve.Linear(0f, 0f, 1f, 1f), 1f, loop: false, new Vector2(0f, 1f));

	public LensFlareDataSRP lensFlareData
	{
		get
		{
			return m_LensFlareData;
		}
		set
		{
			m_LensFlareData = value;
			OnValidate();
		}
	}

	public float celestialProjectedOcclusionRadius(Camera mainCam)
	{
		float num = (float)Math.Tan(sCelestialAngularRadius) * mainCam.farClipPlane;
		return occlusionRadius * num;
	}

	private void OnEnable()
	{
		if ((bool)lensFlareData)
		{
			LensFlareCommonSRP.Instance.AddData(this);
		}
		else
		{
			LensFlareCommonSRP.Instance.RemoveData(this);
		}
	}

	private void OnDisable()
	{
		LensFlareCommonSRP.Instance.RemoveData(this);
	}

	private void OnValidate()
	{
		if (base.isActiveAndEnabled && lensFlareData != null)
		{
			LensFlareCommonSRP.Instance.AddData(this);
		}
		else
		{
			LensFlareCommonSRP.Instance.RemoveData(this);
		}
	}
}
