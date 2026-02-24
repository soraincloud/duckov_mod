using System;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class CopyDepthPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal CommandBuffer cmd;

		internal CameraData cameraData;

		internal Material copyDepthMaterial;

		internal int msaaSamples;

		internal bool copyResolvedDepth;

		internal bool copyToDepth;
	}

	private Material m_CopyDepthMaterial;

	internal bool m_CopyResolvedDepth;

	internal bool m_ShouldClear;

	private PassData m_PassData;

	private RTHandle source { get; set; }

	private RTHandle destination { get; set; }

	internal int MssaSamples { get; set; }

	internal bool CopyToDepth { get; set; }

	public CopyDepthPass(RenderPassEvent evt, Material copyDepthMaterial, bool shouldClear = false, bool copyToDepth = false, bool copyResolvedDepth = false)
	{
		base.profilingSampler = new ProfilingSampler("CopyDepthPass");
		m_PassData = new PassData();
		CopyToDepth = copyToDepth;
		m_CopyDepthMaterial = copyDepthMaterial;
		base.renderPassEvent = evt;
		m_CopyResolvedDepth = copyResolvedDepth;
		m_ShouldClear = shouldClear;
	}

	public void Setup(RTHandle source, RTHandle destination)
	{
		this.source = source;
		this.destination = destination;
		MssaSamples = -1;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		bool flag = (bool)destination.rt && destination.rt.graphicsFormat == GraphicsFormat.None;
		cameraTargetDescriptor.graphicsFormat = (flag ? GraphicsFormat.D32_SFloat_S8_UInt : GraphicsFormat.R32_SFloat);
		cameraTargetDescriptor.msaaSamples = 1;
		ConfigureTarget(destination);
		if (m_ShouldClear)
		{
			ConfigureClear(ClearFlag.All, Color.black);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.copyDepthMaterial = m_CopyDepthMaterial;
		m_PassData.msaaSamples = MssaSamples;
		m_PassData.copyResolvedDepth = m_CopyResolvedDepth;
		m_PassData.copyToDepth = CopyToDepth || !RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R32_SFloat, FormatUsage.Render);
		renderingData.commandBuffer.SetGlobalTexture("_CameraDepthAttachment", source.nameID);
		ExecutePass(context, m_PassData, ref renderingData.commandBuffer, ref renderingData.cameraData, source, destination);
	}

	private static void ExecutePass(ScriptableRenderContext context, PassData passData, ref CommandBuffer cmd, ref CameraData cameraData, RTHandle source, RTHandle destination)
	{
		Material copyDepthMaterial = passData.copyDepthMaterial;
		int msaaSamples = passData.msaaSamples;
		bool copyResolvedDepth = passData.copyResolvedDepth;
		bool copyToDepth = passData.copyToDepth;
		if (copyDepthMaterial == null)
		{
			Debug.LogErrorFormat("Missing {0}. Copy Depth render pass will not execute. Check for missing reference in the renderer resources.", copyDepthMaterial);
			return;
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.CopyDepth)))
		{
			int num = 0;
			if (msaaSamples == -1)
			{
				RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
				num = cameraTargetDescriptor.msaaSamples;
			}
			else
			{
				num = msaaSamples;
			}
			if (SystemInfo.supportsMultisampledTextures == 0 || copyResolvedDepth)
			{
				num = 1;
			}
			switch (num)
			{
			case 8:
				cmd.DisableShaderKeyword("_DEPTH_MSAA_2");
				cmd.DisableShaderKeyword("_DEPTH_MSAA_4");
				cmd.EnableShaderKeyword("_DEPTH_MSAA_8");
				break;
			case 4:
				cmd.DisableShaderKeyword("_DEPTH_MSAA_2");
				cmd.EnableShaderKeyword("_DEPTH_MSAA_4");
				cmd.DisableShaderKeyword("_DEPTH_MSAA_8");
				break;
			case 2:
				cmd.EnableShaderKeyword("_DEPTH_MSAA_2");
				cmd.DisableShaderKeyword("_DEPTH_MSAA_4");
				cmd.DisableShaderKeyword("_DEPTH_MSAA_8");
				break;
			default:
				cmd.DisableShaderKeyword("_DEPTH_MSAA_2");
				cmd.DisableShaderKeyword("_DEPTH_MSAA_4");
				cmd.DisableShaderKeyword("_DEPTH_MSAA_8");
				break;
			}
			if (copyToDepth || destination.rt.graphicsFormat == GraphicsFormat.None)
			{
				cmd.EnableShaderKeyword("_OUTPUT_DEPTH");
			}
			else
			{
				cmd.DisableShaderKeyword("_OUTPUT_DEPTH");
			}
			Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
			bool flag = cameraData.cameraType == CameraType.Game && destination.nameID == BuiltinRenderTextureType.CameraTarget;
			if (cameraData.xr.enabled)
			{
				if (cameraData.xr.supportsFoveatedRendering)
				{
					cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
				}
				flag |= new RenderTargetIdentifier(destination.nameID, 0) == new RenderTargetIdentifier(cameraData.xr.renderTarget, 0);
			}
			Vector4 scaleBias = ((cameraData.IsHandleYFlipped(source) != cameraData.IsHandleYFlipped(destination)) ? new Vector4(vector.x, 0f - vector.y, 0f, vector.y) : new Vector4(vector.x, vector.y, 0f, 0f));
			if (flag)
			{
				cmd.SetViewport(cameraData.pixelRect);
			}
			Blitter.BlitTexture(cmd, source, scaleBias, copyDepthMaterial, 0);
		}
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		destination = ScriptableRenderPass.k_CameraTarget;
	}

	internal void Render(RenderGraph renderGraph, out TextureHandle destination, in TextureHandle source, ref RenderingData renderingData)
	{
		MssaSamples = -1;
		PassData passData;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Setup Global Depth", out passData, base.profilingSampler))
		{
			passData.source = renderGraphBuilder.ReadTexture(in source);
			renderGraphBuilder.AllowPassCulling(value: false);
			renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				context.cmd.SetGlobalTexture("_CameraDepthAttachment", data.source);
			});
		}
		PassData passData2;
		using (RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<PassData>("Copy Depth", out passData2, base.profilingSampler))
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			cameraTargetDescriptor.graphicsFormat = GraphicsFormat.R32_SFloat;
			cameraTargetDescriptor.depthStencilFormat = GraphicsFormat.None;
			cameraTargetDescriptor.depthBufferBits = 0;
			cameraTargetDescriptor.msaaSamples = 1;
			destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_CameraDepthTexture", clear: true);
			passData2.copyDepthMaterial = m_CopyDepthMaterial;
			passData2.msaaSamples = MssaSamples;
			passData2.cameraData = renderingData.cameraData;
			passData2.cmd = renderingData.commandBuffer;
			passData2.copyResolvedDepth = m_CopyResolvedDepth;
			passData2.copyToDepth = CopyToDepth;
			passData2.source = renderGraphBuilder2.ReadTexture(in source);
			passData2.destination = renderGraphBuilder2.UseColorBuffer(in destination, 0);
			renderGraphBuilder2.AllowPassCulling(value: false);
			renderGraphBuilder2.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				ExecutePass(context.renderContext, data, ref data.cmd, ref data.cameraData, data.source, data.destination);
			});
		}
		PassData passData3;
		using RenderGraphBuilder renderGraphBuilder3 = renderGraph.AddRenderPass<PassData>("Setup Global Copy Depth", out passData3, base.profilingSampler);
		passData3.cmd = renderingData.commandBuffer;
		passData3.destination = renderGraphBuilder3.UseColorBuffer(in destination, 0);
		renderGraphBuilder3.AllowPassCulling(value: false);
		renderGraphBuilder3.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			data.cmd.SetGlobalTexture("_CameraDepthTexture", data.destination);
		});
	}
}
