using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class DrawObjectsPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle m_Albedo;

		internal TextureHandle m_Depth;

		internal RenderingData m_RenderingData;

		internal bool m_IsOpaque;

		internal RenderStateBlock m_RenderStateBlock;

		internal FilteringSettings m_FilteringSettings;

		internal List<ShaderTagId> m_ShaderTagIdList;

		internal ProfilingSampler m_ProfilingSampler;

		internal bool m_ShouldTransparentsReceiveShadows;

		internal bool m_IsActiveTargetBackBuffer;

		internal DrawObjectsPass pass;
	}

	private FilteringSettings m_FilteringSettings;

	private RenderStateBlock m_RenderStateBlock;

	private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	private string m_ProfilerTag;

	private ProfilingSampler m_ProfilingSampler;

	private bool m_IsOpaque;

	public bool m_IsActiveTargetBackBuffer;

	public bool m_ShouldTransparentsReceiveShadows;

	private PassData m_PassData;

	private bool m_UseDepthPriming;

	private static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");

	public DrawObjectsPass(string profilerTag, ShaderTagId[] shaderTagIds, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
	{
		base.profilingSampler = new ProfilingSampler("DrawObjectsPass");
		m_PassData = new PassData();
		m_ProfilerTag = profilerTag;
		m_ProfilingSampler = new ProfilingSampler(profilerTag);
		foreach (ShaderTagId item in shaderTagIds)
		{
			m_ShaderTagIdList.Add(item);
		}
		base.renderPassEvent = evt;
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_IsOpaque = opaque;
		m_ShouldTransparentsReceiveShadows = false;
		m_IsActiveTargetBackBuffer = false;
		if (stencilState.enabled)
		{
			m_RenderStateBlock.stencilReference = stencilReference;
			m_RenderStateBlock.mask = RenderStateMask.Stencil;
			m_RenderStateBlock.stencilState = stencilState;
		}
	}

	public DrawObjectsPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
		: this(profilerTag, new ShaderTagId[3]
		{
			new ShaderTagId("SRPDefaultUnlit"),
			new ShaderTagId("UniversalForward"),
			new ShaderTagId("UniversalForwardOnly")
		}, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
	{
	}

	internal DrawObjectsPass(URPProfileId profileId, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
		: this(profileId.GetType().Name, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
	{
		m_ProfilingSampler = ProfilingSampler.Get(profileId);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.m_IsOpaque = m_IsOpaque;
		m_PassData.m_RenderingData = renderingData;
		m_PassData.m_RenderStateBlock = m_RenderStateBlock;
		m_PassData.m_FilteringSettings = m_FilteringSettings;
		m_PassData.m_ShaderTagIdList = m_ShaderTagIdList;
		m_PassData.m_ProfilingSampler = m_ProfilingSampler;
		m_PassData.m_IsActiveTargetBackBuffer = m_IsActiveTargetBackBuffer;
		m_PassData.pass = this;
		CameraSetup(renderingData.commandBuffer, m_PassData, ref renderingData);
		ExecutePass(context, m_PassData, ref renderingData, renderingData.cameraData.IsCameraProjectionMatrixFlipped());
	}

	private static void CameraSetup(CommandBuffer cmd, PassData data, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.renderer.useDepthPriming && data.m_IsOpaque && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
		{
			data.m_RenderStateBlock.depthState = new DepthState(writeEnabled: false, CompareFunction.Equal);
			data.m_RenderStateBlock.mask |= RenderStateMask.Depth;
		}
		else if (data.m_RenderStateBlock.depthState.compareFunction == CompareFunction.Equal)
		{
			data.m_RenderStateBlock.depthState = new DepthState(writeEnabled: true, CompareFunction.LessEqual);
			data.m_RenderStateBlock.mask |= RenderStateMask.Depth;
		}
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData data, ref RenderingData renderingData, bool yFlip)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, data.m_ProfilingSampler))
		{
			Vector4 value = new Vector4(0f, 0f, 0f, data.m_IsOpaque ? 1f : 0f);
			commandBuffer.SetGlobalVector(s_DrawObjectPassDataPropID, value);
			if (data.m_RenderingData.cameraData.xr.enabled && data.m_IsActiveTargetBackBuffer)
			{
				commandBuffer.SetViewport(data.m_RenderingData.cameraData.xr.GetViewport());
			}
			float num = (yFlip ? (-1f) : 1f);
			Vector4 value2 = ((num < 0f) ? new Vector4(num, 1f, -1f, 1f) : new Vector4(num, 0f, 1f, 1f));
			commandBuffer.SetGlobalVector(ShaderPropertyId.scaleBiasRt, value2);
			float value3 = ((renderingData.cameraData.cameraTargetDescriptor.msaaSamples > 1 && data.m_IsOpaque) ? 1f : 0f);
			commandBuffer.SetGlobalFloat(ShaderPropertyId.alphaToMaskAvailable, value3);
			data.pass.OnExecute(commandBuffer);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			SortingCriteria sortingCriteria = (data.m_IsOpaque ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent);
			if (renderingData.cameraData.renderer.useDepthPriming && data.m_IsOpaque && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
			{
				sortingCriteria = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
			}
			FilteringSettings filteringSettings = data.m_FilteringSettings;
			DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(data.m_ShaderTagIdList, ref renderingData, sortingCriteria);
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
			if (activeDebugHandler != null)
			{
				activeDebugHandler.DrawWithDebugRenderState(context, commandBuffer, ref renderingData, ref drawingSettings, ref filteringSettings, ref data.m_RenderStateBlock, delegate(ScriptableRenderContext ctx, ref RenderingData reference, ref DrawingSettings ds, ref FilteringSettings fs, ref RenderStateBlock rsb)
				{
					ctx.DrawRenderers(reference.cullResults, ref ds, ref fs, ref rsb);
				});
			}
			else
			{
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref data.m_RenderStateBlock);
			}
			CoreUtils.SetKeyword(commandBuffer, "_WRITE_RENDERING_LAYERS", state: false);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
	}

	internal void Render(RenderGraph renderGraph, TextureHandle colorTarget, TextureHandle depthTarget, TextureHandle mainShadowsTexture, TextureHandle additionalShadowsTexture, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Draw Objects Pass", out passData, m_ProfilingSampler);
		passData.m_Albedo = renderGraphBuilder.UseColorBuffer(in colorTarget, 0);
		passData.m_Depth = renderGraphBuilder.UseDepthBuffer(in depthTarget, DepthAccess.Write);
		if (mainShadowsTexture.IsValid())
		{
			renderGraphBuilder.ReadTexture(in mainShadowsTexture);
		}
		if (additionalShadowsTexture.IsValid())
		{
			renderGraphBuilder.ReadTexture(in additionalShadowsTexture);
		}
		passData.m_RenderingData = renderingData;
		renderGraphBuilder.AllowPassCulling(value: false);
		passData.m_IsOpaque = m_IsOpaque;
		passData.m_RenderStateBlock = m_RenderStateBlock;
		passData.m_FilteringSettings = m_FilteringSettings;
		passData.m_ShaderTagIdList = m_ShaderTagIdList;
		passData.m_ProfilingSampler = m_ProfilingSampler;
		passData.m_ShouldTransparentsReceiveShadows = m_ShouldTransparentsReceiveShadows;
		passData.m_IsActiveTargetBackBuffer = m_IsActiveTargetBackBuffer;
		passData.pass = this;
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ref RenderingData renderingData2 = ref data.m_RenderingData;
			if (renderingData2.cameraData.xr.enabled)
			{
				bool renderIntoTexture = data.m_Albedo != renderingData2.cameraData.xr.renderTarget;
				renderingData2.cameraData.PushBuiltinShaderConstantsXR(renderingData2.commandBuffer, renderIntoTexture);
				XRSystemUniversal.MarkShaderProperties(renderingData2.commandBuffer, renderingData2.cameraData.xrUniversal, renderIntoTexture);
			}
			if (!data.m_IsOpaque && !data.m_ShouldTransparentsReceiveShadows)
			{
				TransparentSettingsPass.ExecutePass(context.cmd, data.m_ShouldTransparentsReceiveShadows);
			}
			bool yFlip = renderingData2.cameraData.IsRenderTargetProjectionMatrixFlipped(data.m_Albedo, data.m_Depth);
			CameraSetup(context.cmd, data, ref renderingData2);
			ExecutePass(context.renderContext, data, ref renderingData2, yFlip);
		});
	}

	protected virtual void OnExecute(CommandBuffer cmd)
	{
	}
}
