using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

internal class InvokeOnRenderObjectCallbackPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle colorTarget;

		internal TextureHandle depthTarget;
	}

	public InvokeOnRenderObjectCallbackPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("InvokeOnRenderObjectCallbackPass");
		base.renderPassEvent = evt;
		base.useNativeRenderPass = false;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		context.InvokeOnRenderObjectCallback();
	}

	internal void Render(RenderGraph renderGraph, TextureHandle colorTarget, TextureHandle depthTarget, ref RenderingData renderingData)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("OnRenderObject Callback Pass", out passData, base.profilingSampler);
		passData.colorTarget = renderGraphBuilder.UseColorBuffer(in colorTarget, 0);
		passData.depthTarget = renderGraphBuilder.UseDepthBuffer(in depthTarget, DepthAccess.ReadWrite);
		renderGraphBuilder.AllowPassCulling(value: false);
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			context.renderContext.InvokeOnRenderObjectCallback();
		});
	}
}
