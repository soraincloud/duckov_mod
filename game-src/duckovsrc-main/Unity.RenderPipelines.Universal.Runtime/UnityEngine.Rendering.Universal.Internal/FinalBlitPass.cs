using System;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class FinalBlitPass : ScriptableRenderPass
{
	private static class BlitPassNames
	{
		public const string NearestSampler = "NearestDebugDraw";

		public const string BilinearSampler = "BilinearDebugDraw";
	}

	private enum BlitType
	{
		Core,
		HDR,
		Count
	}

	private struct BlitMaterialData
	{
		public Material material;

		public int nearestSamplerPass;

		public int bilinearSamplerPass;
	}

	private class PassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal int sourceID;

		internal Vector4 hdrOutputLuminanceParams;

		internal bool requireSrgbConversion;

		internal BlitMaterialData blitMaterialData;

		internal RenderingData renderingData;
	}

	private RTHandle m_Source;

	private PassData m_PassData;

	private BlitMaterialData[] m_BlitMaterialData;

	public FinalBlitPass(RenderPassEvent evt, Material blitMaterial, Material blitHDRMaterial)
	{
		base.profilingSampler = new ProfilingSampler("FinalBlitPass");
		base.useNativeRenderPass = false;
		m_PassData = new PassData();
		base.renderPassEvent = evt;
		m_BlitMaterialData = new BlitMaterialData[2];
		for (int i = 0; i < 2; i++)
		{
			m_BlitMaterialData[i].material = ((i == 0) ? blitMaterial : blitHDRMaterial);
			m_BlitMaterialData[i].nearestSamplerPass = m_BlitMaterialData[i].material?.FindPass("NearestDebugDraw") ?? (-1);
			m_BlitMaterialData[i].bilinearSamplerPass = m_BlitMaterialData[i].material?.FindPass("BilinearDebugDraw") ?? (-1);
		}
	}

	public void Dispose()
	{
	}

	[Obsolete("Use RTHandles for colorHandle")]
	public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
	{
		if (m_Source?.nameID != colorHandle.Identifier())
		{
			m_Source = RTHandles.Alloc(colorHandle.Identifier());
		}
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, RTHandle colorHandle)
	{
		m_Source = colorHandle;
	}

	private static void SetupHDROutput(ColorGamut hdrDisplayColorGamut, Material material, HDROutputUtils.Operation hdrOperation, Vector4 hdrOutputParameters)
	{
		material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, hdrOutputParameters);
		HDROutputUtils.ConfigureHDROutput(material, hdrDisplayColorGamut, hdrOperation);
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
		if (activeDebugHandler != null && activeDebugHandler.WriteToDebugScreenTexture(ref cameraData))
		{
			ConfigureTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		bool isHDROutputActive = renderingData.cameraData.isHDROutputActive;
		InitPassData(ref renderingData, ref m_PassData, isHDROutputActive ? BlitType.HDR : BlitType.Core);
		if (m_PassData.blitMaterialData.material == null)
		{
			Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_PassData.blitMaterialData, GetType().Name);
			return;
		}
		ref CameraData cameraData = ref renderingData.cameraData;
		RenderTargetIdentifier cameraTargetIdentifier = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
		bool flag = activeDebugHandler?.WriteToDebugScreenTexture(ref cameraData) ?? false;
		RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTargetIdentifier);
		RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (m_Source == cameraData.renderer.GetCameraColorFrontBuffer(commandBuffer))
		{
			m_Source = renderingData.cameraData.renderer.cameraColorTargetHandle;
		}
		using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.FinalBlit)))
		{
			m_PassData.blitMaterialData.material.enabledKeywords = null;
			CoreUtils.SetKeyword(commandBuffer, "_LINEAR_TO_SRGB_CONVERSION", m_PassData.requireSrgbConversion);
			if (isHDROutputActive)
			{
				Tonemapping component = VolumeManager.instance.stack.GetComponent<Tonemapping>();
				UniversalRenderPipeline.GetHDROutputLuminanceParameters(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, component, out var hdrOutputParameters);
				HDROutputUtils.Operation operation = HDROutputUtils.Operation.None;
				if (activeDebugHandler == null || !activeDebugHandler.HDRDebugViewIsActive(ref cameraData))
				{
					operation |= HDROutputUtils.Operation.ColorEncoding;
				}
				if (!cameraData.postProcessEnabled)
				{
					operation |= HDROutputUtils.Operation.ColorConversion;
				}
				SetupHDROutput(cameraData.hdrDisplayColorGamut, m_PassData.blitMaterialData.material, operation, hdrOutputParameters);
			}
			if (flag)
			{
				RenderTexture rt = m_Source.rt;
				int pass = (((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? m_PassData.blitMaterialData.bilinearSamplerPass : m_PassData.blitMaterialData.nearestSamplerPass);
				Vector2 vector = (m_Source.useScaling ? new Vector2(m_Source.rtHandleProperties.rtHandleScale.x, m_Source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(commandBuffer, m_Source, vector, m_PassData.blitMaterialData.material, pass);
				cameraData.renderer.ConfigureCameraTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
			}
			else
			{
				ExecutePass(ref renderingData, in m_PassData.blitMaterialData, s_RTHandleWrapper, m_Source);
				cameraData.renderer.ConfigureCameraTarget(s_RTHandleWrapper, s_RTHandleWrapper);
			}
		}
	}

	private static void ExecutePass(ref RenderingData renderingData, in BlitMaterialData blitMaterialData, RTHandle cameraTarget, RTHandle source)
	{
		CameraData cameraData = renderingData.cameraData;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
		if (!cameraData.isSceneViewCamera && !cameraData.isDefaultViewport)
		{
			loadAction = RenderBufferLoadAction.Load;
		}
		if (cameraData.xr.enabled)
		{
			loadAction = RenderBufferLoadAction.Load;
		}
		RenderTexture rt = source.rt;
		RenderingUtils.FinalBlit(passIndex: ((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? blitMaterialData.bilinearSamplerPass : blitMaterialData.nearestSamplerPass, cmd: commandBuffer, cameraData: ref cameraData, source: source, destination: cameraTarget, loadAction: loadAction, storeAction: RenderBufferStoreAction.Store, material: blitMaterialData.material);
	}

	private void InitPassData(ref RenderingData renderingData, ref PassData passData, BlitType blitType)
	{
		passData.renderingData = renderingData;
		passData.requireSrgbConversion = renderingData.cameraData.requireSrgbConversion;
		passData.blitMaterialData = m_BlitMaterialData[(int)blitType];
	}

	internal void Render(RenderGraph renderGraph, ref RenderingData renderingData, TextureHandle src, TextureHandle dest, TextureHandle overlayUITexture)
	{
		PassData passData;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Final Blit", out passData, base.profilingSampler);
		bool isHDROutputActive = renderingData.cameraData.isHDROutputActive;
		passData.source = src;
		passData.destination = dest;
		passData.sourceID = ShaderPropertyId.sourceTex;
		InitPassData(ref renderingData, ref passData, isHDROutputActive ? BlitType.HDR : BlitType.Core);
		renderGraphBuilder.UseColorBuffer(in passData.destination, 0);
		renderGraphBuilder.ReadTexture(in passData.source);
		if (isHDROutputActive && overlayUITexture.IsValid())
		{
			Tonemapping component = VolumeManager.instance.stack.GetComponent<Tonemapping>();
			ref CameraData cameraData = ref renderingData.cameraData;
			UniversalRenderPipeline.GetHDROutputLuminanceParameters(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, component, out passData.hdrOutputLuminanceParams);
			renderGraphBuilder.ReadTexture(in overlayUITexture);
		}
		else
		{
			passData.hdrOutputLuminanceParams = new Vector4(-1f, -1f, -1f, -1f);
		}
		renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			data.blitMaterialData.material.enabledKeywords = null;
			CoreUtils.SetKeyword(context.cmd, "_LINEAR_TO_SRGB_CONVERSION", data.requireSrgbConversion);
			data.blitMaterialData.material.SetTexture(data.sourceID, data.source);
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref data.renderingData);
			bool num = activeDebugHandler?.WriteToDebugScreenTexture(ref data.renderingData.cameraData) ?? false;
			if (data.hdrOutputLuminanceParams.w >= 0f)
			{
				HDROutputUtils.Operation operation = HDROutputUtils.Operation.None;
				if (activeDebugHandler == null || !activeDebugHandler.HDRDebugViewIsActive(ref data.renderingData.cameraData))
				{
					operation |= HDROutputUtils.Operation.ColorEncoding;
				}
				if (!data.renderingData.cameraData.postProcessEnabled)
				{
					operation |= HDROutputUtils.Operation.ColorConversion;
				}
				SetupHDROutput(data.renderingData.cameraData.hdrDisplayColorGamut, data.blitMaterialData.material, operation, data.hdrOutputLuminanceParams);
			}
			if (num)
			{
				RTHandle rTHandle = data.source;
				Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				RenderTexture rt = rTHandle.rt;
				int pass = (((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? data.blitMaterialData.bilinearSamplerPass : data.blitMaterialData.nearestSamplerPass);
				Blitter.BlitTexture(context.cmd, rTHandle, vector, data.blitMaterialData.material, pass);
			}
			else
			{
				ExecutePass(ref data.renderingData, in data.blitMaterialData, data.destination, data.source);
			}
		});
	}
}
