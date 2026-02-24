using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal class DBufferRenderPass : ScriptableRenderPass
{
	private static string[] s_DBufferNames = new string[4] { "_DBufferTexture0", "_DBufferTexture1", "_DBufferTexture2", "_DBufferTexture3" };

	private static string s_DBufferDepthName = "DBufferDepth";

	private DecalDrawDBufferSystem m_DrawSystem;

	private DBufferSettings m_Settings;

	private Material m_DBufferClear;

	private FilteringSettings m_FilteringSettings;

	private List<ShaderTagId> m_ShaderTagIdList;

	private ProfilingSampler m_ProfilingSampler;

	private ProfilingSampler m_DBufferClearSampler;

	private bool m_DecalLayers;

	private RTHandle m_DBufferDepth;

	internal RTHandle[] dBufferColorHandles { get; private set; }

	internal RTHandle depthHandle { get; private set; }

	internal RTHandle dBufferDepth => m_DBufferDepth;

	public DBufferRenderPass(Material dBufferClear, DBufferSettings settings, DecalDrawDBufferSystem drawSystem, bool decalLayers)
	{
		base.renderPassEvent = (RenderPassEvent)201;
		ScriptableRenderPassInput passInput = ScriptableRenderPassInput.Normal;
		ConfigureInput(passInput);
		m_DrawSystem = drawSystem;
		m_Settings = settings;
		m_DBufferClear = dBufferClear;
		m_ProfilingSampler = new ProfilingSampler("DBuffer Render");
		m_DBufferClearSampler = new ProfilingSampler("Clear");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_DecalLayers = decalLayers;
		m_ShaderTagIdList = new List<ShaderTagId>();
		m_ShaderTagIdList.Add(new ShaderTagId("DBufferMesh"));
		int num = (int)(settings.surfaceData + 1);
		dBufferColorHandles = new RTHandle[num];
	}

	public void Dispose()
	{
		m_DBufferDepth?.Release();
		RTHandle[] array = dBufferColorHandles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Release();
		}
	}

	public void Setup(in CameraData cameraData)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		descriptor.graphicsFormat = GraphicsFormat.None;
		descriptor.depthStencilFormat = cameraData.cameraTargetDescriptor.depthStencilFormat;
		descriptor.msaaSamples = 1;
		RenderingUtils.ReAllocateIfNeeded(ref m_DBufferDepth, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, s_DBufferDepthName);
		Setup(in cameraData, m_DBufferDepth);
	}

	public void Setup(in CameraData cameraData, RTHandle depthTextureHandle)
	{
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		descriptor.graphicsFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
		descriptor.depthBufferBits = 0;
		descriptor.msaaSamples = 1;
		RenderingUtils.ReAllocateIfNeeded(ref dBufferColorHandles[0], in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, s_DBufferNames[0]);
		if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormal || m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
		{
			RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
			descriptor2.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
			descriptor2.depthBufferBits = 0;
			descriptor2.msaaSamples = 1;
			RenderingUtils.ReAllocateIfNeeded(ref dBufferColorHandles[1], in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, s_DBufferNames[1]);
		}
		if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
		{
			RenderTextureDescriptor descriptor3 = cameraData.cameraTargetDescriptor;
			descriptor3.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
			descriptor3.depthBufferBits = 0;
			descriptor3.msaaSamples = 1;
			RenderingUtils.ReAllocateIfNeeded(ref dBufferColorHandles[2], in descriptor3, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, s_DBufferNames[2]);
		}
		depthHandle = depthTextureHandle;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ConfigureTarget(dBufferColorHandles, depthHandle);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		SortingCriteria defaultOpaqueSortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
		DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, defaultOpaqueSortFlags);
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			commandBuffer.SetGlobalTexture(dBufferColorHandles[0].name, dBufferColorHandles[0].nameID);
			if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormal || m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
			{
				commandBuffer.SetGlobalTexture(dBufferColorHandles[1].name, dBufferColorHandles[1].nameID);
			}
			if (m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS)
			{
				commandBuffer.SetGlobalTexture(dBufferColorHandles[2].name, dBufferColorHandles[2].nameID);
			}
			CoreUtils.SetKeyword(commandBuffer, "_DBUFFER_MRT1", m_Settings.surfaceData == DecalSurfaceData.Albedo);
			CoreUtils.SetKeyword(commandBuffer, "_DBUFFER_MRT2", m_Settings.surfaceData == DecalSurfaceData.AlbedoNormal);
			CoreUtils.SetKeyword(commandBuffer, "_DBUFFER_MRT3", m_Settings.surfaceData == DecalSurfaceData.AlbedoNormalMAOS);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_LAYERS", m_DecalLayers);
			using (new ProfilingScope(commandBuffer, m_DBufferClearSampler))
			{
				Blitter.BlitTexture(commandBuffer, dBufferColorHandles[0], new Vector4(1f, 1f, 0f, 0f), m_DBufferClear, 0);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			m_DrawSystem.Execute(commandBuffer);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		CoreUtils.SetKeyword(cmd, "_DBUFFER_MRT1", state: false);
		CoreUtils.SetKeyword(cmd, "_DBUFFER_MRT2", state: false);
		CoreUtils.SetKeyword(cmd, "_DBUFFER_MRT3", state: false);
		CoreUtils.SetKeyword(cmd, "_DECAL_LAYERS", state: false);
	}
}
