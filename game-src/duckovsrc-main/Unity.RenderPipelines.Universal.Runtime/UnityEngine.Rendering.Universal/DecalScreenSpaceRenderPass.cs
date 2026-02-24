using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class DecalScreenSpaceRenderPass : ScriptableRenderPass
{
	private FilteringSettings m_FilteringSettings;

	private ProfilingSampler m_ProfilingSampler;

	private List<ShaderTagId> m_ShaderTagIdList;

	private DecalDrawScreenSpaceSystem m_DrawSystem;

	private DecalScreenSpaceSettings m_Settings;

	private bool m_DecalLayers;

	public DecalScreenSpaceRenderPass(DecalScreenSpaceSettings settings, DecalDrawScreenSpaceSystem drawSystem, bool decalLayers)
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
		ScriptableRenderPassInput passInput = ScriptableRenderPassInput.Depth;
		ConfigureInput(passInput);
		m_DrawSystem = drawSystem;
		m_Settings = settings;
		m_ProfilingSampler = new ProfilingSampler("Decal Screen Space Render");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_DecalLayers = decalLayers;
		m_ShaderTagIdList = new List<ShaderTagId>();
		if (m_DrawSystem == null)
		{
			m_ShaderTagIdList.Add(new ShaderTagId("DecalScreenSpaceProjector"));
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("DecalScreenSpaceMesh"));
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		SortingCriteria sortingCriteria = SortingCriteria.None;
		DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			RenderingUtils.SetScaleBiasRt(commandBuffer, in renderingData);
			NormalReconstruction.SetupProperties(commandBuffer, in renderingData.cameraData);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_NORMAL_BLEND_LOW", m_Settings.normalBlend == DecalNormalBlend.Low);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_NORMAL_BLEND_MEDIUM", m_Settings.normalBlend == DecalNormalBlend.Medium);
			CoreUtils.SetKeyword(commandBuffer, "_DECAL_NORMAL_BLEND_HIGH", m_Settings.normalBlend == DecalNormalBlend.High);
			if (!DecalRendererFeature.isGLDevice)
			{
				CoreUtils.SetKeyword(commandBuffer, "_DECAL_LAYERS", m_DecalLayers);
			}
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
