namespace UnityEngine.Rendering.Universal;

public struct RenderingData
{
	internal CommandBuffer commandBuffer;

	public CullingResults cullResults;

	public CameraData cameraData;

	public LightData lightData;

	public ShadowData shadowData;

	public PostProcessingData postProcessingData;

	public bool supportsDynamicBatching;

	public PerObjectData perObjectData;

	public bool postProcessingEnabled;
}
