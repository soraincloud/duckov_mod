using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class DepthNormalOnlyPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle cameraDepthTexture;

		internal TextureHandle cameraNormalsTexture;

		internal RenderingData renderingData;

		internal List<ShaderTagId> shaderTagIds;

		internal FilteringSettings filteringSettings;

		internal bool enableRenderingLayers;
	}

	private FilteringSettings m_FilteringSettings;

	private PassData m_PassData;

	private static readonly List<ShaderTagId> k_DepthNormals = new List<ShaderTagId>
	{
		new ShaderTagId("DepthNormals"),
		new ShaderTagId("DepthNormalsOnly")
	};

	private static readonly RTHandle[] k_ColorAttachment1 = new RTHandle[1];

	private static readonly RTHandle[] k_ColorAttachment2 = new RTHandle[2];

	internal List<ShaderTagId> shaderTagIds { get; set; }

	private RTHandle depthHandle { get; set; }

	private RTHandle normalHandle { get; set; }

	private RTHandle renderingLayersHandle { get; set; }

	internal bool enableRenderingLayers { get; set; }

	public DepthNormalOnlyPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
	{
		base.profilingSampler = new ProfilingSampler("DepthNormalOnlyPass");
		m_PassData = new PassData();
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
		shaderTagIds = k_DepthNormals;
	}

	public static GraphicsFormat GetGraphicsFormat()
	{
		if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R8G8B8A8_SNorm, FormatUsage.Render))
		{
			return GraphicsFormat.R8G8B8A8_SNorm;
		}
		if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render))
		{
			return GraphicsFormat.R16G16B16A16_SFloat;
		}
		return GraphicsFormat.R32G32B32A32_SFloat;
	}

	public void Setup(RTHandle depthHandle, RTHandle normalHandle)
	{
		this.depthHandle = depthHandle;
		this.normalHandle = normalHandle;
		enableRenderingLayers = false;
	}

	public void Setup(RTHandle depthHandle, RTHandle normalHandle, RTHandle decalLayerHandle)
	{
		Setup(depthHandle, normalHandle);
		renderingLayersHandle = decalLayerHandle;
		enableRenderingLayers = true;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RTHandle[] array;
		if (enableRenderingLayers)
		{
			k_ColorAttachment2[0] = normalHandle;
			k_ColorAttachment2[1] = renderingLayersHandle;
			array = k_ColorAttachment2;
		}
		else
		{
			k_ColorAttachment1[0] = normalHandle;
			array = k_ColorAttachment1;
		}
		if (renderingData.cameraData.renderer.useDepthPriming && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
		{
			ConfigureTarget(array, renderingData.cameraData.renderer.cameraDepthTargetHandle);
		}
		else
		{
			ConfigureTarget(array, depthHandle);
		}
		ConfigureClear(ClearFlag.All, Color.black);
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData passData, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		List<ShaderTagId> shaderTagIdList = passData.shaderTagIds;
		FilteringSettings filteringSettings = passData.filteringSettings;
		using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.DepthNormalPrepass)))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			if (passData.enableRenderingLayers)
			{
				CoreUtils.SetKeyword(commandBuffer, "_WRITE_RENDERING_LAYERS", state: true);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
			}
			SortingCriteria defaultOpaqueSortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
			DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(shaderTagIdList, ref renderingData, defaultOpaqueSortFlags);
			drawingSettings.perObjectData = PerObjectData.None;
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
			if (passData.enableRenderingLayers)
			{
				CoreUtils.SetKeyword(commandBuffer, "_WRITE_RENDERING_LAYERS", state: false);
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
			}
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.shaderTagIds = shaderTagIds;
		m_PassData.filteringSettings = m_FilteringSettings;
		m_PassData.enableRenderingLayers = enableRenderingLayers;
		ExecutePass(context, m_PassData, ref renderingData);
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		normalHandle = null;
		depthHandle = null;
		renderingLayersHandle = null;
		shaderTagIds = k_DepthNormals;
	}

	internal void Render(RenderGraph renderGraph, out TextureHandle cameraNormalsTexture, out TextureHandle cameraDepthTexture, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("DepthNormals Prepass", out passData, base.profilingSampler);
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.graphicsFormat = GraphicsFormat.None;
		cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
		cameraTargetDescriptor.depthBufferBits = 32;
		cameraTargetDescriptor.msaaSamples = 1;
		cameraDepthTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_CameraDepthTexture", clear: true);
		RenderTextureDescriptor cameraTargetDescriptor2 = renderingData.cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor2.depthBufferBits = 0;
		cameraTargetDescriptor2.msaaSamples = 1;
		cameraTargetDescriptor2.graphicsFormat = GetGraphicsFormat();
		cameraNormalsTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor2, "_CameraNormalsTexture", clear: true);
		passData.cameraNormalsTexture = renderGraphBuilder.UseColorBuffer(in cameraNormalsTexture, 0);
		passData.cameraDepthTexture = renderGraphBuilder.UseDepthBuffer(in cameraDepthTexture, DepthAccess.Write);
		passData.renderingData = renderingData;
		passData.shaderTagIds = shaderTagIds;
		passData.filteringSettings = m_FilteringSettings;
		passData.enableRenderingLayers = enableRenderingLayers;
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.renderContext, data, ref data.renderingData);
		});
	}
}
