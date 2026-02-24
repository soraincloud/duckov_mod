using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class DecalPreviewPass : ScriptableRenderPass
{
	private FilteringSettings m_FilteringSettings;

	private List<ShaderTagId> m_ShaderTagIdList;

	private ProfilingSampler m_ProfilingSampler;

	public DecalPreviewPass()
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		ConfigureInput(ScriptableRenderPassInput.Depth);
		m_ProfilingSampler = new ProfilingSampler("Decal Preview Render");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_ShaderTagIdList = new List<ShaderTagId>();
		m_ShaderTagIdList.Add(new ShaderTagId("DecalScreenSpaceMesh"));
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
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
		}
	}
}
