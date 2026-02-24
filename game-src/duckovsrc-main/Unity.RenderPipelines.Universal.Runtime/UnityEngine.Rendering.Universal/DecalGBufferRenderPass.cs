using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class DecalGBufferRenderPass : ScriptableRenderPass
{
	private FilteringSettings m_FilteringSettings;

	private ProfilingSampler m_ProfilingSampler;

	private List<ShaderTagId> m_ShaderTagIdList;

	private DecalDrawGBufferSystem m_DrawSystem;

	private DecalScreenSpaceSettings m_Settings;

	private DeferredLights m_DeferredLights;

	private RTHandle[] m_GbufferAttachments;

	private bool m_DecalLayers;

	public DecalGBufferRenderPass(DecalScreenSpaceSettings settings, DecalDrawGBufferSystem drawSystem, bool decalLayers)
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
		m_DrawSystem = drawSystem;
		m_Settings = settings;
		m_ProfilingSampler = new ProfilingSampler("Decal GBuffer Render");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_DecalLayers = decalLayers;
		m_ShaderTagIdList = new List<ShaderTagId>();
		if (drawSystem == null)
		{
			m_ShaderTagIdList.Add(new ShaderTagId("DecalGBufferProjector"));
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("DecalGBufferMesh"));
		}
		m_GbufferAttachments = new RTHandle[4];
	}

	internal void Setup(DeferredLights deferredLights)
	{
		m_DeferredLights = deferredLights;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (m_DeferredLights.UseRenderPass)
		{
			m_GbufferAttachments[0] = m_DeferredLights.GbufferAttachments[0];
			m_GbufferAttachments[1] = m_DeferredLights.GbufferAttachments[1];
			m_GbufferAttachments[2] = m_DeferredLights.GbufferAttachments[2];
			m_GbufferAttachments[3] = m_DeferredLights.GbufferAttachments[3];
			if (m_DecalLayers)
			{
				RTHandle[] inputs = new RTHandle[2]
				{
					m_DeferredLights.GbufferAttachments[m_DeferredLights.GbufferDepthIndex],
					m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferRenderingLayers]
				};
				bool[] isTransient = new bool[2] { true, false };
				ConfigureInputAttachments(inputs, isTransient);
			}
			else
			{
				RTHandle[] inputs2 = new RTHandle[1] { m_DeferredLights.GbufferAttachments[m_DeferredLights.GbufferDepthIndex] };
				bool[] isTransient2 = new bool[1] { true };
				ConfigureInputAttachments(inputs2, isTransient2);
			}
		}
		else
		{
			m_GbufferAttachments[0] = m_DeferredLights.GbufferAttachments[0];
			m_GbufferAttachments[1] = m_DeferredLights.GbufferAttachments[1];
			m_GbufferAttachments[2] = m_DeferredLights.GbufferAttachments[2];
			m_GbufferAttachments[3] = m_DeferredLights.GbufferAttachments[3];
		}
		ConfigureTarget(m_GbufferAttachments, m_DeferredLights.DepthAttachmentHandle);
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
			NormalReconstruction.SetupProperties(commandBuffer, in renderingData.cameraData);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_NORMAL_BLEND_LOW", m_Settings.normalBlend == DecalNormalBlend.Low);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_NORMAL_BLEND_MEDIUM", m_Settings.normalBlend == DecalNormalBlend.Medium);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_NORMAL_BLEND_HIGH", m_Settings.normalBlend == DecalNormalBlend.High);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_LAYERS", m_DecalLayers);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			m_DrawSystem?.Execute(commandBuffer);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		CoreUtils.SetKeyword(cmd, "_DECAL_NORMAL_BLEND_LOW", state: false);
		CoreUtils.SetKeyword(cmd, "_DECAL_NORMAL_BLEND_MEDIUM", state: false);
		CoreUtils.SetKeyword(cmd, "_DECAL_NORMAL_BLEND_HIGH", state: false);
		CoreUtils.SetKeyword(cmd, "_DECAL_LAYERS", state: false);
	}
}
