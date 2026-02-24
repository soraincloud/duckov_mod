using System;

namespace UnityEngine.Rendering.Universal;

internal class DebugRenderSetup : IDisposable
{
	private readonly DebugHandler m_DebugHandler;

	private readonly ScriptableRenderContext m_Context;

	private readonly CommandBuffer m_CommandBuffer;

	private readonly int m_Index;

	private readonly FilteringSettings m_FilteringSettings;

	private DebugDisplaySettingsMaterial MaterialSettings => m_DebugHandler.DebugDisplaySettings.materialSettings;

	private DebugDisplaySettingsRendering RenderingSettings => m_DebugHandler.DebugDisplaySettings.renderingSettings;

	private DebugDisplaySettingsLighting LightingSettings => m_DebugHandler.DebugDisplaySettings.lightingSettings;

	private void Begin()
	{
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Wireframe:
			m_Context.Submit();
			GL.wireframe = true;
			break;
		case DebugSceneOverrideMode.SolidWireframe:
		case DebugSceneOverrideMode.ShadedWireframe:
			if (m_Index == 1)
			{
				m_Context.Submit();
				GL.wireframe = true;
			}
			break;
		}
		m_Context.ExecuteCommandBuffer(m_CommandBuffer);
		m_CommandBuffer.Clear();
	}

	private void End()
	{
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Wireframe:
			m_Context.Submit();
			GL.wireframe = false;
			break;
		case DebugSceneOverrideMode.SolidWireframe:
		case DebugSceneOverrideMode.ShadedWireframe:
			if (m_Index == 1)
			{
				m_Context.Submit();
				GL.wireframe = false;
			}
			break;
		}
	}

	internal DebugRenderSetup(DebugHandler debugHandler, ScriptableRenderContext context, CommandBuffer commandBuffer, int index, FilteringSettings filteringSettings)
	{
		m_DebugHandler = debugHandler;
		m_Context = context;
		m_CommandBuffer = commandBuffer;
		m_Index = index;
		m_FilteringSettings = filteringSettings;
		Begin();
	}

	internal DrawingSettings CreateDrawingSettings(DrawingSettings drawingSettings)
	{
		if (MaterialSettings.vertexAttributeDebugMode != DebugVertexAttributeMode.None)
		{
			Material replacementMaterial = m_DebugHandler.ReplacementMaterial;
			DrawingSettings result = drawingSettings;
			result.overrideMaterial = replacementMaterial;
			result.overrideMaterialPassIndex = 0;
			return result;
		}
		return drawingSettings;
	}

	internal RenderStateBlock GetRenderStateBlock(RenderStateBlock renderStateBlock)
	{
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Overdraw:
		{
			bool num = m_FilteringSettings.renderQueueRange == RenderQueueRange.opaque || m_FilteringSettings.renderQueueRange == RenderQueueRange.all;
			bool flag = m_FilteringSettings.renderQueueRange == RenderQueueRange.transparent || m_FilteringSettings.renderQueueRange == RenderQueueRange.all;
			bool flag2 = m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.Opaque || m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.All;
			bool flag3 = m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.Transparent || m_DebugHandler.DebugDisplaySettings.renderingSettings.overdrawMode == DebugOverdrawMode.All;
			BlendMode destinationColorBlendMode = (((num && flag2) || (flag && flag3)) ? BlendMode.One : BlendMode.Zero);
			RenderTargetBlendState blendState = new RenderTargetBlendState(ColorWriteMask.All, BlendMode.One, destinationColorBlendMode);
			renderStateBlock.blendState = new BlendState
			{
				blendState0 = blendState
			};
			renderStateBlock.mask = RenderStateMask.Blend;
			break;
		}
		case DebugSceneOverrideMode.SolidWireframe:
		case DebugSceneOverrideMode.ShadedWireframe:
			if (m_Index == 1)
			{
				renderStateBlock.rasterState = new RasterState(CullMode.Back, -1, -1f);
				renderStateBlock.mask = RenderStateMask.Raster;
			}
			break;
		}
		return renderStateBlock;
	}

	public void Dispose()
	{
		End();
	}
}
