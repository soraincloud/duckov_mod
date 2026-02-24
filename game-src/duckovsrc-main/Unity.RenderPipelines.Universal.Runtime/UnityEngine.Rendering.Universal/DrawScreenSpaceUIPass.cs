using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class DrawScreenSpaceUIPass : ScriptableRenderPass
{
	private class PassData
	{
		internal CommandBuffer cmd;

		internal Camera camera;

		internal TextureHandle offscreenTexture;
	}

	private PassData m_PassData;

	private RTHandle m_ColorTarget;

	private RTHandle m_DepthTarget;

	private bool m_RenderOffscreen;

	public DrawScreenSpaceUIPass(RenderPassEvent evt, bool renderOffscreen)
	{
		base.profilingSampler = new ProfilingSampler("DrawScreenSpaceUIPass");
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
		m_RenderOffscreen = renderOffscreen;
		m_PassData = new PassData();
	}

	public static void ConfigureColorDescriptor(ref RenderTextureDescriptor descriptor, int cameraWidth, int cameraHeight)
	{
		descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
		descriptor.depthBufferBits = 0;
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
	}

	public static void ConfigureDepthDescriptor(ref RenderTextureDescriptor descriptor, int depthBufferBits, int cameraWidth, int cameraHeight)
	{
		descriptor.graphicsFormat = GraphicsFormat.None;
		descriptor.depthBufferBits = depthBufferBits;
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData passData)
	{
		context.ExecuteCommandBuffer(passData.cmd);
		passData.cmd.Clear();
		context.DrawUIOverlay(passData.camera);
	}

	public void Dispose()
	{
		m_ColorTarget?.Release();
		m_DepthTarget?.Release();
	}

	public void Setup(ref CameraData cameraData, int depthBufferBits)
	{
		if (m_RenderOffscreen)
		{
			RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
			ConfigureColorDescriptor(ref descriptor, cameraData.pixelWidth, cameraData.pixelHeight);
			RenderingUtils.ReAllocateIfNeeded(ref m_ColorTarget, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_OverlayUITexture");
			RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
			ConfigureDepthDescriptor(ref descriptor2, depthBufferBits, cameraData.pixelWidth, cameraData.pixelHeight);
			RenderingUtils.ReAllocateIfNeeded(ref m_DepthTarget, in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_OverlayUITexture_Depth");
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.cmd = renderingData.commandBuffer;
		m_PassData.camera = renderingData.cameraData.camera;
		if (m_RenderOffscreen)
		{
			CoreUtils.SetRenderTarget(renderingData.commandBuffer, m_ColorTarget, m_DepthTarget, ClearFlag.Color, Color.clear);
			renderingData.commandBuffer.SetGlobalTexture(ShaderPropertyId.overlayUITexture, m_ColorTarget);
		}
		else
		{
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
			RenderTargetIdentifier cameraTargetIdentifier = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
			if (activeDebugHandler != null && activeDebugHandler.WriteToDebugScreenTexture(ref renderingData.cameraData))
			{
				CoreUtils.SetRenderTarget(renderingData.commandBuffer, activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
			}
			else
			{
				RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTargetIdentifier);
				RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
				CoreUtils.SetRenderTarget(renderingData.commandBuffer, s_RTHandleWrapper);
			}
		}
		using (new ProfilingScope(renderingData.commandBuffer, ProfilingSampler.Get(URPProfileId.DrawScreenSpaceUI)))
		{
			ExecutePass(context, m_PassData);
		}
	}

	internal void RenderOffscreen(RenderGraph renderGraph, int depthBufferBits, out TextureHandle output, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Draw Screen Space UI Pass - Offscreen", out passData, base.profilingSampler);
		RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
		ConfigureColorDescriptor(ref descriptor, renderingData.cameraData.pixelWidth, renderingData.cameraData.pixelHeight);
		output = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_OverlayUITexture", clear: true);
		renderGraphBuilder.UseColorBuffer(in output, 0);
		RenderTextureDescriptor descriptor2 = renderingData.cameraData.cameraTargetDescriptor;
		ConfigureDepthDescriptor(ref descriptor2, depthBufferBits, renderingData.cameraData.pixelWidth, renderingData.cameraData.pixelHeight);
		renderGraphBuilder.UseDepthBuffer(UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor2, "_OverlayUITexture_Depth", clear: false), DepthAccess.ReadWrite);
		passData.cmd = renderingData.commandBuffer;
		passData.camera = renderingData.cameraData.camera;
		passData.offscreenTexture = output;
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.renderContext, data);
			data.cmd.SetGlobalTexture(ShaderPropertyId.overlayUITexture, data.offscreenTexture);
		});
	}

	internal void RenderOverlay(RenderGraph renderGraph, in TextureHandle colorBuffer, in TextureHandle depthBuffer, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Draw Screen Space UI Pass - Overlay", out passData, base.profilingSampler);
		renderGraphBuilder.UseColorBuffer(in colorBuffer, 0);
		renderGraphBuilder.UseDepthBuffer(in depthBuffer, DepthAccess.ReadWrite);
		passData.cmd = renderingData.commandBuffer;
		passData.camera = renderingData.cameraData.camera;
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.renderContext, data);
		});
	}
}
