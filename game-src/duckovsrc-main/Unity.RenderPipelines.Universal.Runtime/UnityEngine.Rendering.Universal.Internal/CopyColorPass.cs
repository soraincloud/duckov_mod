using System;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class CopyColorPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal bool useProceduralBlit;

		internal bool disableFoveatedRenderingForPass;

		internal CommandBuffer cmd;

		internal Material samplingMaterial;

		internal Material copyColorMaterial;

		internal Downsampling downsamplingMethod;

		internal ClearFlag clearFlag;

		internal Color clearColor;

		internal int sampleOffsetShaderHandle;
	}

	private int m_SampleOffsetShaderHandle;

	private Material m_SamplingMaterial;

	private Downsampling m_DownsamplingMethod;

	private Material m_CopyColorMaterial;

	private PassData m_PassData;

	private RTHandle source { get; set; }

	private RTHandle destination { get; set; }

	private int destinationID { get; set; }

	public CopyColorPass(RenderPassEvent evt, Material samplingMaterial, Material copyColorMaterial = null)
	{
		base.profilingSampler = new ProfilingSampler("CopyColorPass");
		m_PassData = new PassData();
		m_SamplingMaterial = samplingMaterial;
		m_CopyColorMaterial = copyColorMaterial;
		m_SampleOffsetShaderHandle = Shader.PropertyToID("_SampleOffset");
		base.renderPassEvent = evt;
		m_DownsamplingMethod = Downsampling.None;
		base.useNativeRenderPass = false;
	}

	public static void ConfigureDescriptor(Downsampling downsamplingMethod, ref RenderTextureDescriptor descriptor, out FilterMode filterMode)
	{
		descriptor.msaaSamples = 1;
		descriptor.depthBufferBits = 0;
		switch (downsamplingMethod)
		{
		case Downsampling._2xBilinear:
			descriptor.width /= 2;
			descriptor.height /= 2;
			break;
		case Downsampling._4xBox:
		case Downsampling._4xBilinear:
			descriptor.width /= 4;
			descriptor.height /= 4;
			break;
		}
		filterMode = ((downsamplingMethod != Downsampling.None) ? FilterMode.Bilinear : FilterMode.Point);
	}

	[Obsolete("Use RTHandles for source and destination.")]
	public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination, Downsampling downsampling)
	{
		this.source = RTHandles.Alloc(source);
		this.destination = RTHandles.Alloc(destination.Identifier());
		destinationID = destination.id;
		m_DownsamplingMethod = downsampling;
	}

	public void Setup(RTHandle source, RTHandle destination, Downsampling downsampling)
	{
		this.source = source;
		this.destination = destination;
		m_DownsamplingMethod = downsampling;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (destination.rt == null)
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			cameraTargetDescriptor.msaaSamples = 1;
			cameraTargetDescriptor.depthBufferBits = 0;
			if (m_DownsamplingMethod == Downsampling._2xBilinear)
			{
				cameraTargetDescriptor.width /= 2;
				cameraTargetDescriptor.height /= 2;
			}
			else if (m_DownsamplingMethod == Downsampling._4xBox || m_DownsamplingMethod == Downsampling._4xBilinear)
			{
				cameraTargetDescriptor.width /= 4;
				cameraTargetDescriptor.height /= 4;
			}
			cmd.GetTemporaryRT(destinationID, cameraTargetDescriptor, (m_DownsamplingMethod != Downsampling.None) ? FilterMode.Bilinear : FilterMode.Point);
		}
		else
		{
			cmd.SetGlobalTexture(destination.name, destination.nameID);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_PassData.samplingMaterial = m_SamplingMaterial;
		m_PassData.copyColorMaterial = m_CopyColorMaterial;
		m_PassData.downsamplingMethod = m_DownsamplingMethod;
		m_PassData.clearFlag = base.clearFlag;
		m_PassData.clearColor = base.clearColor;
		m_PassData.sampleOffsetShaderHandle = m_SampleOffsetShaderHandle;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (source == renderingData.cameraData.renderer.GetCameraColorFrontBuffer(commandBuffer))
		{
			source = renderingData.cameraData.renderer.cameraColorTargetHandle;
		}
		bool enabled = renderingData.cameraData.xr.enabled;
		bool disableFoveatedRenderingForPass = enabled && renderingData.cameraData.xr.supportsFoveatedRendering;
		ScriptableRenderer.SetRenderTarget(commandBuffer, destination, ScriptableRenderPass.k_CameraTarget, base.clearFlag, base.clearColor);
		ExecutePass(m_PassData, source, destination, ref renderingData.commandBuffer, enabled, disableFoveatedRenderingForPass);
	}

	private static void ExecutePass(PassData passData, RTHandle source, RTHandle destination, ref CommandBuffer cmd, bool useDrawProceduralBlit, bool disableFoveatedRenderingForPass)
	{
		Material samplingMaterial = passData.samplingMaterial;
		Material copyColorMaterial = passData.copyColorMaterial;
		Downsampling downsamplingMethod = passData.downsamplingMethod;
		ClearFlag clearFlag = passData.clearFlag;
		Color color = passData.clearColor;
		int sampleOffsetShaderHandle = passData.sampleOffsetShaderHandle;
		if (disableFoveatedRenderingForPass)
		{
			cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
		}
		if (samplingMaterial == null)
		{
			Debug.LogErrorFormat("Missing {0}. Copy Color render pass will not execute. Check for missing reference in the renderer resources.", samplingMaterial);
			return;
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.CopyColor)))
		{
			ScriptableRenderer.SetRenderTarget(cmd, destination, ScriptableRenderPass.k_CameraTarget, clearFlag, color);
			switch (downsamplingMethod)
			{
			case Downsampling.None:
				Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, copyColorMaterial, 0);
				break;
			case Downsampling._2xBilinear:
				Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, copyColorMaterial, 1);
				break;
			case Downsampling._4xBox:
				samplingMaterial.SetFloat(sampleOffsetShaderHandle, 2f);
				Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, samplingMaterial, 0);
				break;
			case Downsampling._4xBilinear:
				Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, copyColorMaterial, 1);
				break;
			}
		}
	}

	internal TextureHandle Render(RenderGraph renderGraph, out TextureHandle destination, in TextureHandle source, Downsampling downsampling, ref RenderingData renderingData)
	{
		m_DownsamplingMethod = downsampling;
		PassData passData;
		using (RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Copy Color", out passData, base.profilingSampler))
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			ConfigureDescriptor(downsampling, ref descriptor, out var filterMode);
			destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_CameraOpaqueTexture", clear: true, filterMode);
			passData.destination = renderGraphBuilder.UseColorBuffer(in destination, 0);
			passData.source = renderGraphBuilder.ReadTexture(in source);
			passData.cmd = renderingData.commandBuffer;
			passData.useProceduralBlit = renderingData.cameraData.xr.enabled;
			passData.disableFoveatedRenderingForPass = renderingData.cameraData.xr.enabled && renderingData.cameraData.xr.supportsFoveatedRendering;
			passData.samplingMaterial = m_SamplingMaterial;
			passData.copyColorMaterial = m_CopyColorMaterial;
			passData.downsamplingMethod = m_DownsamplingMethod;
			passData.clearFlag = base.clearFlag;
			passData.clearColor = base.clearColor;
			passData.sampleOffsetShaderHandle = m_SampleOffsetShaderHandle;
			renderGraphBuilder.AllowPassCulling(value: false);
			renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				ExecutePass(data, data.source, data.destination, ref data.cmd, data.useProceduralBlit, data.disableFoveatedRenderingForPass);
			});
		}
		PassData passData2;
		using (RenderGraphBuilder renderGraphBuilder2 = renderGraph.AddRenderPass<PassData>("Set Global Copy Color", out passData2, base.profilingSampler))
		{
			RenderTextureDescriptor descriptor2 = renderingData.cameraData.cameraTargetDescriptor;
			ConfigureDescriptor(downsampling, ref descriptor2, out var _);
			passData2.destination = renderGraphBuilder2.UseColorBuffer(in destination, 0);
			passData2.cmd = renderingData.commandBuffer;
			renderGraphBuilder2.AllowPassCulling(value: false);
			renderGraphBuilder2.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				data.cmd.SetGlobalTexture("_CameraOpaqueTexture", data.destination);
			});
		}
		return destination;
	}

	public override void OnCameraCleanup(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		if (destination.rt == null && destinationID != -1)
		{
			cmd.ReleaseTemporaryRT(destinationID);
			destination.Release();
			destination = null;
		}
	}
}
