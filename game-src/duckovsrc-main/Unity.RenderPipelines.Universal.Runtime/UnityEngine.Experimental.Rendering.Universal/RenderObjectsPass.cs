using System.Collections.Generic;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experimental.Rendering.Universal;

public class RenderObjectsPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RenderObjectsPass pass;

		internal RenderingData renderingData;
	}

	private RenderQueueType renderQueueType;

	private FilteringSettings m_FilteringSettings;

	private RenderObjects.CustomCameraSettings m_CameraSettings;

	private string m_ProfilerTag;

	private ProfilingSampler m_ProfilingSampler;

	private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	private RenderStateBlock m_RenderStateBlock;

	public Material overrideMaterial { get; set; }

	public int overrideMaterialPassIndex { get; set; }

	public Shader overrideShader { get; set; }

	public int overrideShaderPassIndex { get; set; }

	public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
	{
		m_RenderStateBlock.mask |= RenderStateMask.Depth;
		m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
	}

	public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp, StencilOp zFailOp)
	{
		StencilState defaultValue = StencilState.defaultValue;
		defaultValue.enabled = true;
		defaultValue.SetCompareFunction(compareFunction);
		defaultValue.SetPassOperation(passOp);
		defaultValue.SetFailOperation(failOp);
		defaultValue.SetZFailOperation(zFailOp);
		m_RenderStateBlock.mask |= RenderStateMask.Stencil;
		m_RenderStateBlock.stencilReference = reference;
		m_RenderStateBlock.stencilState = defaultValue;
	}

	public RenderObjectsPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, RenderObjects.CustomCameraSettings cameraSettings)
	{
		base.profilingSampler = new ProfilingSampler("RenderObjectsPass");
		m_ProfilerTag = profilerTag;
		m_ProfilingSampler = new ProfilingSampler(profilerTag);
		base.renderPassEvent = renderPassEvent;
		this.renderQueueType = renderQueueType;
		overrideMaterial = null;
		overrideMaterialPassIndex = 0;
		overrideShader = null;
		overrideShaderPassIndex = 0;
		m_FilteringSettings = new FilteringSettings((renderQueueType == RenderQueueType.Transparent) ? RenderQueueRange.transparent : RenderQueueRange.opaque, layerMask);
		if (shaderTags != null && shaderTags.Length != 0)
		{
			foreach (string name in shaderTags)
			{
				m_ShaderTagIdList.Add(new ShaderTagId(name));
			}
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
		}
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_CameraSettings = cameraSettings;
	}

	internal RenderObjectsPass(URPProfileId profileId, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask, RenderObjects.CustomCameraSettings cameraSettings)
		: this(profileId.GetType().Name, renderPassEvent, shaderTags, renderQueueType, layerMask, cameraSettings)
	{
		m_ProfilingSampler = ProfilingSampler.Get(profileId);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		SortingCriteria sortingCriteria = ((renderQueueType == RenderQueueType.Transparent) ? SortingCriteria.CommonTransparent : renderingData.cameraData.defaultOpaqueSortFlags);
		DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
		drawingSettings.overrideMaterial = overrideMaterial;
		drawingSettings.overrideMaterialPassIndex = overrideMaterialPassIndex;
		drawingSettings.overrideShader = overrideShader;
		drawingSettings.overrideShaderPassIndex = overrideShaderPassIndex;
		ref CameraData cameraData = ref renderingData.cameraData;
		Camera camera = cameraData.camera;
		Rect pixelRect = renderingData.cameraData.pixelRect;
		float aspect = pixelRect.width / pixelRect.height;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			if (m_CameraSettings.overrideCamera)
			{
				if (cameraData.xr.enabled)
				{
					Debug.LogWarning("RenderObjects pass is configured to override camera matrices. While rendering in stereo camera matrices cannot be overridden.");
				}
				else
				{
					Matrix4x4 proj = Matrix4x4.Perspective(m_CameraSettings.cameraFieldOfView, aspect, camera.nearClipPlane, camera.farClipPlane);
					proj = GL.GetGPUProjectionMatrix(proj, cameraData.IsCameraProjectionMatrixFlipped());
					Matrix4x4 viewMatrix = cameraData.GetViewMatrix();
					Vector4 column = viewMatrix.GetColumn(3);
					viewMatrix.SetColumn(3, column + m_CameraSettings.offset);
					RenderingUtils.SetViewAndProjectionMatrices(commandBuffer, viewMatrix, proj, setInverseMatrices: false);
				}
			}
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
			if (activeDebugHandler != null)
			{
				activeDebugHandler.DrawWithDebugRenderState(context, commandBuffer, ref renderingData, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock, delegate(ScriptableRenderContext ctx, ref RenderingData data, ref DrawingSettings ds, ref FilteringSettings fs, ref RenderStateBlock rsb)
				{
					ctx.DrawRenderers(data.cullResults, ref ds, ref fs, ref rsb);
				});
			}
			else
			{
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
			}
			if (m_CameraSettings.overrideCamera && m_CameraSettings.restoreCamera && !cameraData.xr.enabled)
			{
				RenderingUtils.SetViewAndProjectionMatrices(commandBuffer, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), setInverseMatrices: false);
			}
		}
	}

	internal override void RecordRenderGraph(RenderGraph renderGraph, ref RenderingData renderingData)
	{
		UniversalRenderer universalRenderer = (UniversalRenderer)renderingData.cameraData.renderer;
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Render Objects Pass", out passData, m_ProfilingSampler);
		TextureHandle activeRenderGraphColor = UniversalRenderer.m_ActiveRenderGraphColor;
		renderGraphBuilder.UseColorBuffer(in activeRenderGraphColor, 0);
		renderGraphBuilder.UseDepthBuffer(in UniversalRenderer.m_ActiveRenderGraphDepth, DepthAccess.Write);
		renderGraphBuilder.ReadTexture(in universalRenderer.frameResources.mainShadowsTexture);
		renderGraphBuilder.AllowPassCulling(value: false);
		passData.pass = this;
		passData.renderingData = renderingData;
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext rgContext)
		{
			data.pass.Execute(rgContext.renderContext, ref data.renderingData);
		});
	}
}
