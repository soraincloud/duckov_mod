namespace UnityEngine.Rendering.Universal;

internal class UpscalePass : ScriptableRenderPass
{
	private static readonly ProfilingSampler m_ProfilingScope = new ProfilingSampler("Upscale Pass");

	private RTHandle m_Source;

	private RTHandle m_UpscaleHandle;

	private static Material m_BlitMaterial;

	public UpscalePass(RenderPassEvent evt, Material blitMaterial)
	{
		base.renderPassEvent = evt;
		m_BlitMaterial = blitMaterial;
	}

	public void Setup(RTHandle colorTargetHandle, int width, int height, FilterMode mode, ref RenderingData renderingData, out RTHandle upscaleHandle)
	{
		m_Source = colorTargetHandle;
		RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
		descriptor.width = width;
		descriptor.height = height;
		descriptor.depthBufferBits = 0;
		RenderingUtils.ReAllocateIfNeeded(ref m_UpscaleHandle, in descriptor, mode, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_UpscaleTexture");
		upscaleHandle = m_UpscaleHandle;
	}

	public void Dispose()
	{
		m_UpscaleHandle?.Release();
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingScope))
		{
			CoreUtils.SetRenderTarget(commandBuffer, m_UpscaleHandle, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.None, Color.clear);
			Blit(commandBuffer, m_Source, m_UpscaleHandle, m_BlitMaterial);
		}
	}
}
