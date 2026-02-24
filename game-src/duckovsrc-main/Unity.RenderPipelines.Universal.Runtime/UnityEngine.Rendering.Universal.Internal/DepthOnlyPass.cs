using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class DepthOnlyPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle cameraDepthTexture;

		internal RenderingData renderingData;

		internal ShaderTagId shaderTagId;

		internal FilteringSettings filteringSettings;
	}

	private static readonly ShaderTagId k_ShaderTagId = new ShaderTagId("DepthOnly");

	private GraphicsFormat depthStencilFormat;

	private PassData m_PassData;

	private FilteringSettings m_FilteringSettings;

	private RTHandle destination { get; set; }

	internal ShaderTagId shaderTagId { get; set; } = k_ShaderTagId;

	public DepthOnlyPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
	{
		base.profilingSampler = new ProfilingSampler("DepthOnlyPass");
		m_PassData = new PassData();
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
		shaderTagId = k_ShaderTagId;
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, RTHandle depthAttachmentHandle)
	{
		destination = depthAttachmentHandle;
		depthStencilFormat = baseDescriptor.depthStencilFormat;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.renderer.useDepthPriming && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
		{
			ConfigureTarget(renderingData.cameraData.renderer.cameraDepthTargetHandle);
			ConfigureClear(ClearFlag.Depth, Color.black);
		}
		else
		{
			base.useNativeRenderPass = true;
			ConfigureTarget(destination);
			ConfigureClear(ClearFlag.All, Color.black);
		}
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData passData, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		ShaderTagId shaderTagId = passData.shaderTagId;
		FilteringSettings filteringSettings = passData.filteringSettings;
		using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.DepthPrepass)))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			SortingCriteria defaultOpaqueSortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(shaderTagId, ref renderingData, defaultOpaqueSortFlags);
			drawingSettings.perObjectData = PerObjectData.None;
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.shaderTagId = shaderTagId;
		m_PassData.filteringSettings = m_FilteringSettings;
		ExecutePass(context, m_PassData, ref renderingData);
	}

	internal void Render(RenderGraph renderGraph, out TextureHandle cameraDepthTexture, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("DepthOnly Prepass", out passData, base.profilingSampler);
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.graphicsFormat = GraphicsFormat.None;
		cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
		cameraTargetDescriptor.depthBufferBits = 32;
		cameraTargetDescriptor.msaaSamples = 1;
		cameraDepthTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_CameraDepthTexture", clear: true);
		passData.cameraDepthTexture = renderGraphBuilder.UseDepthBuffer(in cameraDepthTexture, DepthAccess.Write);
		passData.renderingData = renderingData;
		passData.shaderTagId = shaderTagId;
		passData.filteringSettings = m_FilteringSettings;
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.renderContext, data, ref data.renderingData);
		});
	}
}
