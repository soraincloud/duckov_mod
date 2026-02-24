using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public class DrawSkyboxPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle color;

		internal TextureHandle depth;

		internal RenderingData renderingData;

		internal DrawSkyboxPass pass;
	}

	public bool m_IsActiveTargetBackBuffer;

	public DrawSkyboxPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("DrawSkyboxPass");
		base.renderPassEvent = evt;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
		if (activeDebugHandler != null && activeDebugHandler.IsScreenClearNeeded)
		{
			return;
		}
		RendererList rendererList = CreateSkyboxRendererList(context, cameraData);
		if (cameraData.xr.enabled)
		{
			if (cameraData.xr.singlePassEnabled)
			{
				renderingData.commandBuffer.SetSinglePassStereo(SystemInfo.supportsMultiview ? SinglePassStereoMode.Multiview : SinglePassStereoMode.Instancing);
			}
			if (m_IsActiveTargetBackBuffer)
			{
				renderingData.commandBuffer.SetViewport(cameraData.xr.GetViewport());
			}
		}
		renderingData.commandBuffer.DrawRendererList(rendererList);
		if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
		{
			renderingData.commandBuffer.SetSinglePassStereo(SinglePassStereoMode.None);
		}
	}

	private RendererList CreateSkyboxRendererList(ScriptableRenderContext context, CameraData cameraData)
	{
		RendererList rendererList = default(RendererList);
		if (cameraData.xr.enabled)
		{
			if (cameraData.xr.singlePassEnabled)
			{
				return context.CreateSkyboxRendererList(cameraData.camera, cameraData.GetProjectionMatrix(), cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix(1), cameraData.GetViewMatrix(1));
			}
			return context.CreateSkyboxRendererList(cameraData.camera, cameraData.GetProjectionMatrix(), cameraData.GetViewMatrix());
		}
		return context.CreateSkyboxRendererList(cameraData.camera);
	}

	internal void Render(RenderGraph renderGraph, TextureHandle colorTarget, TextureHandle depthTarget, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Draw Skybox Pass", out passData, base.profilingSampler);
		passData.color = renderGraphBuilder.UseColorBuffer(in colorTarget, 0);
		passData.depth = renderGraphBuilder.UseDepthBuffer(in depthTarget, DepthAccess.Read);
		passData.renderingData = renderingData;
		passData.pass = this;
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			data.pass.Execute(context.renderContext, ref data.renderingData);
		});
	}
}
