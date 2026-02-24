using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class CapturePass : ScriptableRenderPass
{
	private RTHandle m_CameraColorHandle;

	private const string m_ProfilerTag = "Capture Pass";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Capture Pass");

	public CapturePass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("CapturePass");
		base.renderPassEvent = evt;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		m_CameraColorHandle = renderingData.cameraData.renderer.GetCameraColorBackBuffer(commandBuffer);
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			RenderTargetIdentifier nameID = m_CameraColorHandle.nameID;
			IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> captureActions = renderingData.cameraData.captureActions;
			captureActions.Reset();
			while (captureActions.MoveNext())
			{
				captureActions.Current(nameID, renderingData.commandBuffer);
			}
		}
	}
}
