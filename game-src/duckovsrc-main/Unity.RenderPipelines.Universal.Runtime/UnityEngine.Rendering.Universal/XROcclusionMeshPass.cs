using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public class XROcclusionMeshPass : ScriptableRenderPass
{
	private class PassData
	{
		internal RenderingData renderingData;

		internal TextureHandle cameraColorAttachment;

		internal TextureHandle cameraDepthAttachment;

		internal bool isActiveTargetBackBuffer;
	}

	private PassData m_PassData;

	public bool m_IsActiveTargetBackBuffer;

	public XROcclusionMeshPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("XROcclusionMeshPass");
		base.renderPassEvent = evt;
		m_PassData = new PassData();
		m_IsActiveTargetBackBuffer = false;
		base.profilingSampler = new ProfilingSampler("XR Occlusion Pass");
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData data)
	{
		CommandBuffer commandBuffer = data.renderingData.commandBuffer;
		if (data.renderingData.cameraData.xr.hasValidOcclusionMesh)
		{
			if (data.isActiveTargetBackBuffer)
			{
				commandBuffer.SetViewport(data.renderingData.cameraData.xr.GetViewport());
			}
			data.renderingData.cameraData.xr.RenderOcclusionMesh(commandBuffer, !data.isActiveTargetBackBuffer);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.renderingData = renderingData;
		m_PassData.isActiveTargetBackBuffer = m_IsActiveTargetBackBuffer;
		ExecutePass(context, m_PassData);
	}

	internal void Render(RenderGraph renderGraph, in TextureHandle cameraColorAttachment, in TextureHandle cameraDepthAttachment, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("XR Occlusion Pass", out passData, base.profilingSampler);
		passData.renderingData = renderingData;
		passData.cameraColorAttachment = renderGraphBuilder.UseColorBuffer(in cameraColorAttachment, 0);
		passData.cameraDepthAttachment = renderGraphBuilder.UseDepthBuffer(in cameraDepthAttachment, DepthAccess.Write);
		passData.isActiveTargetBackBuffer = m_IsActiveTargetBackBuffer;
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			ExecutePass(context.renderContext, data);
		});
	}
}
