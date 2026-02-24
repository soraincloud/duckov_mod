using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class ClearTargetsPass
{
	private class PassData
	{
		internal TextureHandle color;

		internal TextureHandle depth;

		internal RTClearFlags clearFlags;

		internal Color clearColor;
	}

	private static ProfilingSampler s_ClearProfilingSampler = new ProfilingSampler("Clear Targets");

	internal static void Render(RenderGraph graph, UniversalRenderer renderer, RTClearFlags clearFlags, Color clearColor)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = graph.AddRenderPass<PassData>("Clear Targets Pass", out passData, s_ClearProfilingSampler);
		passData.color = renderGraphBuilder.UseColorBuffer(in UniversalRenderer.m_ActiveRenderGraphColor, 0);
		passData.depth = renderGraphBuilder.UseDepthBuffer(in UniversalRenderer.m_ActiveRenderGraphDepth, DepthAccess.Write);
		passData.clearFlags = clearFlags;
		passData.clearColor = clearColor;
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			context.cmd.ClearRenderTarget(data.clearFlags, data.clearColor, 1f, 0u);
		});
	}
}
