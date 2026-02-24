using UnityEngine.Rendering;

namespace UnityEngine;

public struct RenderParams
{
	public int layer { get; set; }

	public uint renderingLayerMask { get; set; }

	public int rendererPriority { get; set; }

	public Bounds worldBounds { get; set; }

	public Camera camera { get; set; }

	public MotionVectorGenerationMode motionVectorMode { get; set; }

	public ReflectionProbeUsage reflectionProbeUsage { get; set; }

	public Material material { get; set; }

	public MaterialPropertyBlock matProps { get; set; }

	public ShadowCastingMode shadowCastingMode { get; set; }

	public bool receiveShadows { get; set; }

	public LightProbeUsage lightProbeUsage { get; set; }

	public LightProbeProxyVolume lightProbeProxyVolume { get; set; }

	public RenderParams(Material mat)
	{
		layer = 0;
		renderingLayerMask = GraphicsSettings.defaultRenderingLayerMask;
		rendererPriority = 0;
		worldBounds = new Bounds(Vector3.zero, Vector3.zero);
		camera = null;
		motionVectorMode = MotionVectorGenerationMode.Camera;
		reflectionProbeUsage = ReflectionProbeUsage.Off;
		material = mat;
		matProps = null;
		shadowCastingMode = ShadowCastingMode.Off;
		receiveShadows = false;
		lightProbeUsage = LightProbeUsage.Off;
		lightProbeProxyVolume = null;
	}
}
