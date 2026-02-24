using UnityEngine;

namespace Umbra;

[CreateAssetMenu(fileName = "UmbraProfile", menuName = "Umbra Profile", order = 251)]
[HelpURL("https://kronnect.com/guides-category/umbra-soft-shadows")]
public class UmbraProfile : ScriptableObject
{
	[Tooltip("Which shadow generation system should be used")]
	public ShadowSource shadowSource = ShadowSource.UmbraShadows;

	[Tooltip("Translates to the number of shadow map samples used to resolve shadows")]
	[Range(1f, 64f)]
	public int sampleCount = 16;

	[Tooltip("Size of the directional light which influences the penumbra size")]
	[Range(0f, 32f)]
	public float lightSize = 6f;

	[Tooltip("Makes shadows sharper nearer to the occluder")]
	public bool enableContactHardening = true;

	[Tooltip("Makes shadows sharper and stronger near the occluder")]
	[Range(0f, 1f)]
	public float contactStrength = 0.5f;

	[Tooltip("Usual value is 5. Modify to alter the shadow edge appearance.")]
	public float contactStrengthKnee = 0.0001f;

	[Tooltip("Makes shadows smoother when far from occluder")]
	[Range(1f, 16f)]
	public float distantSpread = 1f;

	[Tooltip("Number of shadow map samples to find occluders which improve the penumbra radius calculation")]
	[Range(1f, 64f)]
	public int occludersCount = 8;

	[Tooltip("Multiplier to the occluder search radius in texture space. A higher value creates more diffused penumbra.")]
	public float occludersSearchRadius = 8f;

	[Tooltip("Number of blur passes. Each blur pass increases shadow softness but reduces performance.")]
	[Range(0f, 3f)]
	public int blurIterations;

	[Tooltip("Specifies the blur method. Gaussian blur uses two passes per iteration and produces smoother results.")]
	public BlurType blurType = BlurType.Gaussian15;

	[Tooltip("Blur kerner radius multiplier. Improves softness but can introduce artifacts")]
	[Range(0.5f, 5f)]
	public float blurSpread = 1f;

	[Tooltip("Increases the contrast on the shadow border to create sharper shadows when blur is used")]
	[Range(0f, 1f)]
	public float blurEdgeSharpness;

	[Tooltip("Strength of the edge weight when blurring")]
	public float blurEdgeTolerance = 5f;

	[Tooltip("Reduces blur on distance from camera")]
	public float blurDepthAttenStart = 50f;

	[Tooltip("Blur reduction intensity")]
	public float blurDepthAttenLength = 50f;

	[Tooltip("Blur reduction when viewing shadow from a grazing angle")]
	[Range(0f, 1f)]
	public float blurGrazingAttenuation;

	public bool blendCascades;

	[Range(1f, 10f)]
	public int posterization = 1;

	public float cascade1BlendingStrength = 0.1f;

	public float cascade2BlendingStrength = 0.1f;

	public float cascade3BlendingStrength = 0.1f;

	[Tooltip("Manual adjustment of shadow smoothness multiplier for this cascade")]
	public float cascade1Scale = 1f;

	[Tooltip("Manual adjustment of shadow smoothness multiplier for this cascade")]
	public float cascade2Scale = 1f;

	[Tooltip("Manual adjustment of shadow smoothness multiplier for this cascade")]
	public float cascade3Scale = 1f;

	[Tooltip("Manual adjustment of shadow smoothness multiplier for this cascade")]
	public float cascade4Scale = 1f;

	[Tooltip("Method used to obtain surface normals in forward rendering path (not used in deferred). Reconstruct from depth is faster but less accurate.")]
	public NormalSource normalsSource;

	[Tooltip("Reduces the number of samples while keeping the shadow size")]
	public LoopStep loopStepOptimization;

	[Tooltip("Resolves shadowmap every two frames, reusing the result from previous frame")]
	public bool frameSkipOptimization;

	[Tooltip("Distance threshold - if camera has moved this distance from previous frame, ignore cached shadowmap")]
	public float skipFrameMaxCameraDisplacement = 0.1f;

	[Tooltip("Rotation threshold - if camera has rotated this angle from previous frame, ignore cached shadowmap")]
	public float skipFrameMaxCameraRotation = 5f;

	[Tooltip("Resolves screen space shadows in a reduced buffer of half of screen size")]
	public bool downsample;

	[Tooltip("Prevents shadow blurring on geometry edges")]
	public bool preserveEdges = true;

	[Tooltip("Bias applied to Umbra shadows. Use only if self-shadowing occurs on certain surfaces.")]
	[Range(0f, 10f)]
	public float bias;

	[Tooltip("Stylized look for shadows")]
	public Style style;

	[Tooltip("Optional mask texture to create stylized shadows")]
	public Texture2D maskTexture;

	public float maskScale = 1f;

	public bool contactShadows;

	[Range(0f, 1f)]
	public float contactShadowsIntensityMultiplier = 0.85f;

	public ContactShadowsInjectionPoint contactShadowsInjectionPoint;

	[Range(1f, 64f)]
	public int contactShadowsSampleCount = 16;

	public float contactShadowsStepping = 0.01f;

	public float contactShadowsThicknessNear = 0.5f;

	public float contactShadowsThicknessDistanceMultiplier;

	public float contactShadowsJitter = 0.3f;

	[Range(0f, 1f)]
	[Tooltip("Attenuates shadow intensity with distance to occluder")]
	public float contactShadowsDistanceFade = 0.75f;

	public float contactShadowsStartDistance;

	public float contactShadowsStartDistanceFade = 0.01f;

	[Tooltip("Adds an offset to the pixel position to avoid self-occlusion")]
	[Range(0.0001f, 0.25f)]
	public float contactShadowsNormalBias = 0.1f;

	[Tooltip("Attenuates contact shadows on the edges of screen")]
	[Range(0f, 0.5f)]
	public float contactShadowsVignetteSize = 0.15f;

	private void OnValidate()
	{
		blurDepthAttenLength = Mathf.Max(0.001f, blurDepthAttenLength);
		blurEdgeTolerance = Mathf.Max(0f, blurEdgeTolerance);
		blurDepthAttenStart = Mathf.Max(0f, blurDepthAttenStart);
		blurDepthAttenLength = Mathf.Max(0f, blurDepthAttenLength);
		blurGrazingAttenuation = Mathf.Max(0f, blurGrazingAttenuation);
		occludersSearchRadius = Mathf.Max(0.01f, occludersSearchRadius);
		contactStrengthKnee = Mathf.Max(0.0001f, contactStrengthKnee);
		cascade1BlendingStrength = Mathf.Max(0.01f, cascade1BlendingStrength);
		cascade2BlendingStrength = Mathf.Max(0.01f, cascade2BlendingStrength);
		cascade3BlendingStrength = Mathf.Max(0.01f, cascade3BlendingStrength);
		cascade1Scale = Mathf.Max(0f, cascade1Scale);
		cascade2Scale = Mathf.Max(0f, cascade2Scale);
		cascade3Scale = Mathf.Max(0f, cascade3Scale);
		cascade4Scale = Mathf.Max(0f, cascade4Scale);
		maskScale = Mathf.Max(maskScale, 0f);
		skipFrameMaxCameraRotation = Mathf.Max(skipFrameMaxCameraRotation, 0f);
		skipFrameMaxCameraDisplacement = Mathf.Max(skipFrameMaxCameraDisplacement, 0f);
		contactShadowsStepping = Mathf.Max(0.0001f, contactShadowsStepping);
		contactShadowsThicknessNear = Mathf.Max(0f, contactShadowsThicknessNear);
		contactShadowsThicknessDistanceMultiplier = Mathf.Max(0f, contactShadowsThicknessDistanceMultiplier);
		contactShadowsJitter = Mathf.Max(0f, contactShadowsJitter);
		contactShadowsStartDistance = Mathf.Max(0f, contactShadowsStartDistance);
		contactShadowsStartDistanceFade = Mathf.Max(1E-05f, contactShadowsStartDistanceFade);
	}

	public void ApplyPreset(UmbraPreset preset)
	{
		switch (preset)
		{
		case UmbraPreset.Fast:
			sampleCount = 12;
			lightSize = 12f;
			occludersCount = 6;
			occludersSearchRadius = 5f;
			enableContactHardening = true;
			contactStrength = 0.5f;
			contactStrengthKnee = 0.0001f;
			blurIterations = 0;
			distantSpread = 1f;
			break;
		case UmbraPreset.Hard:
			sampleCount = 1;
			lightSize = 0.5f;
			enableContactHardening = false;
			blurIterations = 1;
			blurType = BlurType.Box;
			blurSpread = 1f;
			blurEdgeTolerance = 5f;
			blurEdgeSharpness = 1f;
			blurDepthAttenStart = 75f;
			blurDepthAttenLength = 50f;
			blurGrazingAttenuation = 0f;
			break;
		case UmbraPreset.Soft:
			sampleCount = 16;
			lightSize = 8f;
			enableContactHardening = true;
			occludersCount = 8;
			occludersSearchRadius = 8f;
			contactStrength = 0.5f;
			contactStrengthKnee = 0.0001f;
			distantSpread = 1f;
			blurIterations = 0;
			break;
		case UmbraPreset.Smooth:
			sampleCount = 16;
			lightSize = 13f;
			enableContactHardening = true;
			occludersCount = 9;
			occludersSearchRadius = 8f;
			contactStrength = 0.5f;
			contactStrengthKnee = 0.0001f;
			distantSpread = 1.2f;
			blurIterations = 0;
			break;
		case UmbraPreset.ExtraSmooth:
			sampleCount = 20;
			lightSize = 16f;
			enableContactHardening = true;
			occludersCount = 16;
			occludersSearchRadius = 10f;
			contactStrength = 0.8f;
			contactStrengthKnee = 0.0001f;
			distantSpread = 1.4f;
			blurIterations = 0;
			break;
		case UmbraPreset.Blurred:
			sampleCount = 32;
			lightSize = 16f;
			enableContactHardening = true;
			occludersCount = 24;
			occludersSearchRadius = 16f;
			contactStrength = 0.4f;
			contactStrengthKnee = 0.0001f;
			blurIterations = 0;
			distantSpread = 2f;
			break;
		}
	}
}
