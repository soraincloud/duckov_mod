using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class DecalForwardEmissivePass : ScriptableRenderPass
{
	private FilteringSettings m_FilteringSettings;

	private ProfilingSampler m_ProfilingSampler;

	private List<ShaderTagId> m_ShaderTagIdList;

	private DecalDrawFowardEmissiveSystem m_DrawSystem;

	public DecalForwardEmissivePass(DecalDrawFowardEmissiveSystem drawSystem)
	{
		base.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
		ConfigureInput(ScriptableRenderPassInput.Depth);
		m_DrawSystem = drawSystem;
		m_ProfilingSampler = new ProfilingSampler("Decal Forward Emissive Render");
		m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
		m_ShaderTagIdList = new List<ShaderTagId>();
		m_ShaderTagIdList.Add(new ShaderTagId("DecalMeshForwardEmissive"));
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
			m_DrawSystem.Execute(commandBuffer);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);
		}
	}
}
