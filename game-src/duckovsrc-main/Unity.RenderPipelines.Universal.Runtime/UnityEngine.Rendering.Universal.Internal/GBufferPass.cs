using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

internal class GBufferPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle[] gbuffer;

		internal TextureHandle depth;

		internal RenderingData renderingData;

		internal DeferredLights deferredLights;

		internal FilteringSettings filteringSettings;

		internal DrawingSettings drawingSettings;
	}

	private static readonly int s_CameraNormalsTextureID = Shader.PropertyToID("_CameraNormalsTexture");

	private static ShaderTagId s_ShaderTagLit = new ShaderTagId("Lit");

	private static ShaderTagId s_ShaderTagSimpleLit = new ShaderTagId("SimpleLit");

	private static ShaderTagId s_ShaderTagUnlit = new ShaderTagId("Unlit");

	private static ShaderTagId s_ShaderTagComplexLit = new ShaderTagId("ComplexLit");

	private static ShaderTagId s_ShaderTagUniversalGBuffer = new ShaderTagId("UniversalGBuffer");

	private static ShaderTagId s_ShaderTagUniversalMaterialType = new ShaderTagId("UniversalMaterialType");

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Render GBuffer");

	private DeferredLights m_DeferredLights;

	private static ShaderTagId[] s_ShaderTagValues;

	private static RenderStateBlock[] s_RenderStateBlocks;

	private FilteringSettings m_FilteringSettings;

	private RenderStateBlock m_RenderStateBlock;

	private PassData m_PassData;

	public GBufferPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, DeferredLights deferredLights)
	{
		base.profilingSampler = new ProfilingSampler("GBufferPass");
		base.renderPassEvent = evt;
		m_PassData = new PassData();
		m_DeferredLights = deferredLights;
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_RenderStateBlock.stencilState = stencilState;
		m_RenderStateBlock.stencilReference = stencilReference;
		m_RenderStateBlock.mask = RenderStateMask.Stencil;
		if (s_ShaderTagValues == null)
		{
			s_ShaderTagValues = new ShaderTagId[5];
			s_ShaderTagValues[0] = s_ShaderTagLit;
			s_ShaderTagValues[1] = s_ShaderTagSimpleLit;
			s_ShaderTagValues[2] = s_ShaderTagUnlit;
			s_ShaderTagValues[3] = s_ShaderTagComplexLit;
			s_ShaderTagValues[4] = default(ShaderTagId);
		}
		if (s_RenderStateBlocks == null)
		{
			s_RenderStateBlocks = new RenderStateBlock[5];
			s_RenderStateBlocks[0] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 32);
			s_RenderStateBlocks[1] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 64);
			s_RenderStateBlocks[2] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 0);
			s_RenderStateBlocks[3] = DeferredLights.OverwriteStencil(m_RenderStateBlock, 96, 0);
			s_RenderStateBlocks[4] = s_RenderStateBlocks[0];
		}
	}

	public void Dispose()
	{
		m_DeferredLights.ReleaseGbufferResources();
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		RTHandle[] gbufferAttachments = m_DeferredLights.GbufferAttachments;
		if (cmd != null)
		{
			bool flag = true;
			if (m_DeferredLights.UseRenderPass && m_DeferredLights.DepthCopyTexture != null && m_DeferredLights.DepthCopyTexture.rt != null)
			{
				m_DeferredLights.GbufferAttachments[m_DeferredLights.GbufferDepthIndex] = m_DeferredLights.DepthCopyTexture;
				flag = false;
			}
			for (int i = 0; i < gbufferAttachments.Length; i++)
			{
				if (i != m_DeferredLights.GBufferLightingIndex && (i != m_DeferredLights.GBufferNormalSmoothnessIndex || !m_DeferredLights.HasNormalPrepass) && (i != m_DeferredLights.GbufferDepthIndex || flag) && (!m_DeferredLights.UseRenderPass || i == m_DeferredLights.GBufferRenderingLayers || i == m_DeferredLights.GbufferDepthIndex || m_DeferredLights.HasDepthPrepass))
				{
					m_DeferredLights.ReAllocateGBufferIfNeeded(cameraTextureDescriptor, i);
					cmd.SetGlobalTexture(m_DeferredLights.GbufferAttachments[i].name, m_DeferredLights.GbufferAttachments[i].nameID);
				}
			}
		}
		if (m_DeferredLights.UseRenderPass)
		{
			m_DeferredLights.UpdateDeferredInputAttachments();
		}
		ConfigureTarget(m_DeferredLights.GbufferAttachments, m_DeferredLights.DepthAttachment, m_DeferredLights.GbufferFormats);
		ConfigureClear(ClearFlag.None, Color.black);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		m_PassData.filteringSettings = m_FilteringSettings;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			m_PassData.deferredLights = m_DeferredLights;
			ShaderTagId shaderTagId = s_ShaderTagUniversalGBuffer;
			m_PassData.drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
			ExecutePass(context, m_PassData, ref renderingData);
			if (!m_DeferredLights.UseRenderPass)
			{
				renderingData.commandBuffer.SetGlobalTexture(s_CameraNormalsTextureID, m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferNormalSmoothnessIndex]);
			}
		}
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData data, ref RenderingData renderingData, bool useRenderGraph = false)
	{
		int num;
		if (data.deferredLights.UseRenderingLayers)
		{
			num = ((!data.deferredLights.HasRenderingLayerPrepass) ? 1 : 0);
			if (num != 0)
			{
				CoreUtils.SetKeyword(renderingData.commandBuffer, "_WRITE_RENDERING_LAYERS", state: true);
			}
		}
		else
		{
			num = 0;
		}
		context.ExecuteCommandBuffer(renderingData.commandBuffer);
		renderingData.commandBuffer.Clear();
		if (data.deferredLights.IsOverlay)
		{
			data.deferredLights.ClearStencilPartial(renderingData.commandBuffer);
			context.ExecuteCommandBuffer(renderingData.commandBuffer);
			renderingData.commandBuffer.Clear();
		}
		NativeArray<ShaderTagId> tagValues = new NativeArray<ShaderTagId>(s_ShaderTagValues, Allocator.Temp);
		NativeArray<RenderStateBlock> stateBlocks = new NativeArray<RenderStateBlock>(s_RenderStateBlocks, Allocator.Temp);
		context.DrawRenderers(renderingData.cullResults, ref data.drawingSettings, ref data.filteringSettings, s_ShaderTagUniversalMaterialType, isPassTagName: false, tagValues, stateBlocks);
		tagValues.Dispose();
		stateBlocks.Dispose();
		if (!data.deferredLights.UseRenderPass)
		{
			renderingData.commandBuffer.SetGlobalTexture(s_CameraNormalsTextureID, data.deferredLights.GbufferAttachments[data.deferredLights.GBufferNormalSmoothnessIndex]);
		}
		if (num != 0)
		{
			CoreUtils.SetKeyword(renderingData.commandBuffer, "_WRITE_RENDERING_LAYERS", state: false);
			context.ExecuteCommandBuffer(renderingData.commandBuffer);
			renderingData.commandBuffer.Clear();
		}
	}

	internal void Render(RenderGraph renderGraph, TextureHandle cameraColor, TextureHandle cameraDepth, ref RenderingData renderingData, ref UniversalRenderer.RenderGraphFrameResources frameResources)
	{
		PassData passData;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("GBuffer Pass", out passData, m_ProfilingSampler))
		{
			passData.gbuffer = (frameResources.gbuffer = m_DeferredLights.GbufferTextureHandles);
			for (int i = 0; i < m_DeferredLights.GBufferSliceCount; i++)
			{
				RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
				cameraTargetDescriptor.depthBufferBits = 0;
				cameraTargetDescriptor.stencilFormat = GraphicsFormat.None;
				if (i != m_DeferredLights.GBufferLightingIndex)
				{
					cameraTargetDescriptor.graphicsFormat = m_DeferredLights.GetGBufferFormat(i);
					frameResources.gbuffer[i] = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, DeferredLights.k_GBufferNames[i], clear: true);
				}
				else
				{
					frameResources.gbuffer[i] = cameraColor;
				}
				passData.gbuffer[i] = renderGraphBuilder.UseColorBuffer(in frameResources.gbuffer[i], i);
			}
			passData.deferredLights = m_DeferredLights;
			passData.depth = renderGraphBuilder.UseDepthBuffer(in cameraDepth, DepthAccess.Write);
			passData.renderingData = renderingData;
			renderGraphBuilder.AllowPassCulling(value: false);
			passData.filteringSettings = m_FilteringSettings;
			ShaderTagId shaderTagId = s_ShaderTagUniversalGBuffer;
			passData.drawingSettings = CreateDrawingSettings(shaderTagId, ref passData.renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
			renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				ExecutePass(context.renderContext, data, ref data.renderingData, useRenderGraph: true);
			});
		}
		PassData passData2;
		using RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<PassData>("Set GBuffer Globals", out passData2, m_ProfilingSampler);
		passData2.gbuffer = (frameResources.gbuffer = m_DeferredLights.GbufferTextureHandles);
		for (int num = 0; num < m_DeferredLights.GBufferSliceCount; num++)
		{
			passData2.gbuffer[num] = renderGraphBuilder2.UseColorBuffer(in frameResources.gbuffer[num], num);
		}
		passData2.depth = renderGraphBuilder2.UseDepthBuffer(in cameraDepth, DepthAccess.Read);
		passData2.renderingData = renderingData;
		passData2.deferredLights = m_DeferredLights;
		renderGraphBuilder2.AllowPassCulling(value: false);
		renderGraphBuilder2.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			for (int j = 0; j < data.gbuffer.Length; j++)
			{
				if (j != data.deferredLights.GBufferLightingIndex)
				{
					data.renderingData.commandBuffer.SetGlobalTexture(DeferredLights.k_GBufferNames[j], data.gbuffer[j]);
				}
			}
		});
	}
}
