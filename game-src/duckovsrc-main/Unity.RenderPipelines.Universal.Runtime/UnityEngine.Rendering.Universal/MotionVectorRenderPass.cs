using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal sealed class MotionVectorRenderPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle motionVectorColor;

		internal TextureHandle motionVectorDepth;

		internal TextureHandle cameraDepth;

		internal RenderingData renderingData;

		internal Material cameraMaterial;

		internal Material objectMaterial;

		internal FilteringSettings filteringSettings;
	}

	private const string kPreviousViewProjectionNoJitter = "_PrevViewProjMatrix";

	private const string kViewProjectionNoJitter = "_NonJitteredViewProjMatrix";

	private const string kPreviousViewProjectionNoJitterStereo = "_PrevViewProjMatrixStereo";

	private const string kViewProjectionNoJitterStereo = "_NonJitteredViewProjMatrixStereo";

	internal const GraphicsFormat k_TargetFormat = GraphicsFormat.R16G16_SFloat;

	private static readonly string[] s_ShaderTags = new string[1] { "MotionVectors" };

	private RTHandle m_Color;

	private RTHandle m_Depth;

	private readonly Material m_CameraMaterial;

	private readonly Material m_ObjectMaterial;

	private readonly FilteringSettings m_FilteringSettings;

	private PassData m_PassData;

	internal MotionVectorRenderPass(RenderPassEvent evt, Material cameraMaterial, Material objectMaterial, LayerMask opaqueLayerMask)
	{
		base.renderPassEvent = evt;
		m_CameraMaterial = cameraMaterial;
		m_ObjectMaterial = objectMaterial;
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, opaqueLayerMask);
		m_PassData = new PassData();
		base.profilingSampler = ProfilingSampler.Get(URPProfileId.MotionVectors);
		ConfigureInput(ScriptableRenderPassInput.Depth);
	}

	internal void Setup(RTHandle color, RTHandle depth)
	{
		m_Color = color;
		m_Depth = depth;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		cmd.SetGlobalTexture(m_Color.name, m_Color.nameID);
		cmd.SetGlobalTexture(m_Depth.name, m_Depth.nameID);
		ConfigureTarget(m_Color, m_Depth);
		ConfigureClear(ClearFlag.Color | ClearFlag.Depth, Color.black);
		ConfigureDepthStoreAction(RenderBufferStoreAction.DontCare);
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData passData, ref RenderingData renderingData)
	{
		Material cameraMaterial = passData.cameraMaterial;
		Material objectMaterial = passData.objectMaterial;
		if (cameraMaterial == null || objectMaterial == null)
		{
			return;
		}
		ref CameraData cameraData = ref renderingData.cameraData;
		Camera camera = cameraData.camera;
		MotionVectorsPersistentData motionVectorsPersistentData = null;
		if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
		{
			motionVectorsPersistentData = component.motionVectorsPersistentData;
		}
		if (motionVectorsPersistentData == null || camera.cameraType == CameraType.Preview)
		{
			return;
		}
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.MotionVectors)))
		{
			int xRMultiPassId = motionVectorsPersistentData.GetXRMultiPassId(ref cameraData);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
			{
				commandBuffer.SetGlobalMatrixArray("_PrevViewProjMatrixStereo", motionVectorsPersistentData.previousViewProjectionStereo);
				commandBuffer.SetGlobalMatrixArray("_NonJitteredViewProjMatrixStereo", motionVectorsPersistentData.viewProjectionStereo);
			}
			else
			{
				commandBuffer.SetGlobalMatrix("_PrevViewProjMatrix", motionVectorsPersistentData.previousViewProjectionStereo[xRMultiPassId]);
				commandBuffer.SetGlobalMatrix("_NonJitteredViewProjMatrix", motionVectorsPersistentData.viewProjectionStereo[xRMultiPassId]);
			}
			camera.depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
			DrawCameraMotionVectors(context, commandBuffer, ref renderingData, camera, cameraMaterial);
			DrawObjectMotionVectors(context, ref renderingData, camera, objectMaterial, commandBuffer, ref passData.filteringSettings);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.cameraMaterial = m_CameraMaterial;
		m_PassData.objectMaterial = m_ObjectMaterial;
		m_PassData.filteringSettings = m_FilteringSettings;
		ExecutePass(context, m_PassData, ref renderingData);
	}

	private static DrawingSettings GetDrawingSettings(ref RenderingData renderingData, Material objectMaterial)
	{
		Camera camera = renderingData.cameraData.camera;
		SortingSettings sortingSettings = new SortingSettings(camera);
		sortingSettings.criteria = SortingCriteria.CommonOpaque;
		SortingSettings sortingSettings2 = sortingSettings;
		DrawingSettings drawingSettings = new DrawingSettings(ShaderTagId.none, sortingSettings2);
		drawingSettings.perObjectData = PerObjectData.MotionVectors;
		drawingSettings.enableDynamicBatching = renderingData.supportsDynamicBatching;
		drawingSettings.enableInstancing = true;
		DrawingSettings result = drawingSettings;
		for (int i = 0; i < s_ShaderTags.Length; i++)
		{
			result.SetShaderPassName(i, new ShaderTagId(s_ShaderTags[i]));
		}
		result.fallbackMaterial = objectMaterial;
		return result;
	}

	private static void DrawCameraMotionVectors(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData, Camera camera, Material cameraMaterial)
	{
		bool supportsFoveatedRendering = renderingData.cameraData.xr.supportsFoveatedRendering;
		bool flag = supportsFoveatedRendering && XRSystem.foveatedRenderingCaps.HasFlag(FoveatedRenderingCaps.NonUniformRaster);
		if (supportsFoveatedRendering)
		{
			if (flag)
			{
				cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
			}
			else
			{
				cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
			}
		}
		cmd.DrawProcedural(Matrix4x4.identity, cameraMaterial, 0, MeshTopology.Triangles, 3, 1);
		if (supportsFoveatedRendering && !flag)
		{
			cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
		}
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}

	private static void DrawObjectMotionVectors(ScriptableRenderContext context, ref RenderingData renderingData, Camera camera, Material objectMaterial, CommandBuffer cmd, ref FilteringSettings filteringSettings)
	{
		bool supportsFoveatedRendering = renderingData.cameraData.xr.supportsFoveatedRendering;
		if (supportsFoveatedRendering)
		{
			cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
		}
		DrawingSettings drawingSettings = GetDrawingSettings(ref renderingData, objectMaterial);
		RenderStateBlock stateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref stateBlock);
		if (supportsFoveatedRendering)
		{
			cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
		}
	}

	internal void Render(RenderGraph renderGraph, ref TextureHandle cameraDepthTexture, in TextureHandle motionVectorColor, in TextureHandle motionVectorDepth, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Motion Vector Pass", out passData, base.profilingSampler);
		renderGraphBuilder.AllowPassCulling(value: false);
		passData.motionVectorColor = renderGraphBuilder.UseColorBuffer(in motionVectorColor, 0);
		passData.motionVectorDepth = renderGraphBuilder.UseDepthBuffer(in motionVectorDepth, DepthAccess.Write);
		passData.cameraDepth = renderGraphBuilder.ReadTexture(in cameraDepthTexture);
		passData.renderingData = renderingData;
		passData.cameraMaterial = m_CameraMaterial;
		passData.objectMaterial = m_ObjectMaterial;
		passData.filteringSettings = m_FilteringSettings;
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.renderContext, data, ref data.renderingData);
			data.renderingData.commandBuffer.SetGlobalTexture("_MotionVectorTexture", data.motionVectorColor);
		});
	}
}
