using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

internal class DeferredPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle color;

		internal TextureHandle depth;

		internal RenderingData renderingData;

		internal DeferredLights deferredLights;
	}

	private DeferredLights m_DeferredLights;

	public DeferredPass(RenderPassEvent evt, DeferredLights deferredLights)
	{
		base.profilingSampler = new ProfilingSampler("DeferredPass");
		base.renderPassEvent = evt;
		m_DeferredLights = deferredLights;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
	{
		RTHandle rTHandle = m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferLightingIndex];
		RTHandle rTHandle2 = m_DeferredLights.DepthAttachmentHandle;
		if (m_DeferredLights.UseRenderPass)
		{
			ConfigureInputAttachments(m_DeferredLights.DeferredInputAttachments, m_DeferredLights.DeferredInputIsTransient);
		}
		ConfigureTarget(rTHandle, rTHandle2);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_DeferredLights.ExecuteDeferredPass(context, ref renderingData);
	}

	internal void Render(RenderGraph renderGraph, TextureHandle color, TextureHandle depth, TextureHandle[] gbuffer, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Deferred Lighting Pass", out passData, base.profilingSampler);
		passData.color = renderGraphBuilder.UseColorBuffer(in color, 0);
		passData.depth = renderGraphBuilder.UseDepthBuffer(in depth, DepthAccess.ReadWrite);
		passData.deferredLights = m_DeferredLights;
		passData.renderingData = renderingData;
		for (int i = 0; i < gbuffer.Length; i++)
		{
			if (i != m_DeferredLights.GBufferLightingIndex)
			{
				renderGraphBuilder.ReadTexture(in gbuffer[i]);
			}
		}
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			data.deferredLights.ExecuteDeferredPass(context.renderContext, ref data.renderingData);
		});
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		m_DeferredLights.OnCameraCleanup(cmd);
	}
}
