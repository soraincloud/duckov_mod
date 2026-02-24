using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

internal class Renderer2D : ScriptableRenderer
{
	private struct RenderPassInputSummary
	{
		internal bool requiresDepthTexture;

		internal bool requiresColorTexture;
	}

	internal const int k_DepthBufferBits = 32;

	private const int k_FinalBlitPassQueueOffset = 1;

	private const int k_AfterFinalBlitPassQueueOffset = 2;

	private Render2DLightingPass m_Render2DLightingPass;

	private PixelPerfectBackgroundPass m_PixelPerfectBackgroundPass;

	private UpscalePass m_UpscalePass;

	private FinalBlitPass m_FinalBlitPass;

	private DrawScreenSpaceUIPass m_DrawOffscreenUIPass;

	private DrawScreenSpaceUIPass m_DrawOverlayUIPass;

	private Light2DCullResult m_LightCullResult;

	internal RenderTargetBufferSystem m_ColorBufferSystem;

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Create Camera Textures");

	private bool m_UseDepthStencilBuffer = true;

	private bool m_CreateColorTexture;

	private bool m_CreateDepthTexture;

	private RTHandle m_ColorTextureHandle;

	private RTHandle m_DepthTextureHandle;

	private Material m_BlitMaterial;

	private Material m_BlitHDRMaterial;

	private Material m_SamplingMaterial;

	private Renderer2DData m_Renderer2DData;

	private PostProcessPasses m_PostProcessPasses;

	internal bool createColorTexture => m_CreateColorTexture;

	internal bool createDepthTexture => m_CreateDepthTexture;

	internal ColorGradingLutPass colorGradingLutPass => m_PostProcessPasses.colorGradingLutPass;

	internal PostProcessPass postProcessPass => m_PostProcessPasses.postProcessPass;

	internal PostProcessPass finalPostProcessPass => m_PostProcessPasses.finalPostProcessPass;

	internal RTHandle afterPostProcessColorHandle => m_PostProcessPasses.afterPostProcessColor;

	internal RTHandle colorGradingLutHandle => m_PostProcessPasses.colorGradingLut;

	public override int SupportedCameraStackingTypes()
	{
		return 3;
	}

	public Renderer2D(Renderer2DData data)
		: base(data)
	{
		m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.coreBlitPS);
		m_BlitHDRMaterial = CoreUtils.CreateEngineMaterial(data.blitHDROverlay);
		m_SamplingMaterial = CoreUtils.CreateEngineMaterial(data.samplingShader);
		m_Render2DLightingPass = new Render2DLightingPass(data, m_BlitMaterial, m_SamplingMaterial);
		m_PixelPerfectBackgroundPass = new PixelPerfectBackgroundPass(RenderPassEvent.AfterRenderingTransparents);
		m_UpscalePass = new UpscalePass(RenderPassEvent.AfterRenderingPostProcessing, m_BlitMaterial);
		m_FinalBlitPass = new FinalBlitPass((RenderPassEvent)1001, m_BlitMaterial, m_BlitHDRMaterial);
		m_DrawOffscreenUIPass = new DrawScreenSpaceUIPass(RenderPassEvent.BeforeRenderingPostProcessing, renderOffscreen: true);
		m_DrawOverlayUIPass = new DrawScreenSpaceUIPass((RenderPassEvent)1002, renderOffscreen: false);
		m_ColorBufferSystem = new RenderTargetBufferSystem("_CameraColorAttachment");
		PostProcessParams postProcessParams = PostProcessParams.Create();
		postProcessParams.blitMaterial = m_BlitMaterial;
		postProcessParams.requestHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		m_PostProcessPasses = new PostProcessPasses(data.postProcessData, ref postProcessParams);
		m_UseDepthStencilBuffer = data.useDepthStencilBuffer;
		m_Renderer2DData = data;
		base.supportedRenderingFeatures = new RenderingFeatures();
		m_LightCullResult = new Light2DCullResult();
		m_Renderer2DData.lightCullResult = m_LightCullResult;
		bool flag = Blitter.GetBlitMaterial(TextureDimension.Tex2D) == null;
		UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
		if (asset != null)
		{
			ScriptableRendererData[] rendererDataList = asset.m_RendererDataList;
			for (int i = 0; i < rendererDataList.Length; i++)
			{
				if (rendererDataList[i] is UniversalRendererData)
				{
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			Blitter.Initialize(data.coreBlitPS, data.coreBlitColorAndDepthPS);
		}
		LensFlareCommonSRP.mergeNeeded = 0;
		LensFlareCommonSRP.maxLensFlareWithOcclusionTemporalSample = 1;
		LensFlareCommonSRP.Initialize();
	}

	protected override void Dispose(bool disposing)
	{
		m_Renderer2DData.Dispose();
		m_PostProcessPasses.Dispose();
		m_ColorTextureHandle?.Release();
		m_DepthTextureHandle?.Release();
		ReleaseRenderTargets();
		m_UpscalePass.Dispose();
		m_FinalBlitPass?.Dispose();
		m_DrawOffscreenUIPass?.Dispose();
		m_DrawOverlayUIPass?.Dispose();
		CoreUtils.Destroy(m_BlitMaterial);
		CoreUtils.Destroy(m_BlitHDRMaterial);
		CoreUtils.Destroy(m_SamplingMaterial);
		Blitter.Cleanup();
		base.Dispose(disposing);
	}

	internal override void ReleaseRenderTargets()
	{
		m_ColorBufferSystem.Dispose();
		m_PostProcessPasses.ReleaseRenderTargets();
	}

	public Renderer2DData GetRenderer2DData()
	{
		return m_Renderer2DData;
	}

	private RenderPassInputSummary GetRenderPassInputs(ref RenderingData renderingData, ref CameraData cameraData)
	{
		RenderPassInputSummary result = default(RenderPassInputSummary);
		for (int i = 0; i < base.activeRenderPassQueue.Count; i++)
		{
			ScriptableRenderPass scriptableRenderPass = base.activeRenderPassQueue[i];
			bool flag = (scriptableRenderPass.input & ScriptableRenderPassInput.Depth) != 0;
			bool flag2 = (scriptableRenderPass.input & ScriptableRenderPassInput.Color) != 0;
			result.requiresDepthTexture |= flag;
			result.requiresColorTexture |= flag2;
		}
		result.requiresColorTexture |= cameraData.postProcessEnabled || cameraData.isHdrEnabled || cameraData.isSceneViewCamera || !cameraData.isDefaultViewport || cameraData.requireSrgbConversion || !cameraData.resolveFinalTarget || m_Renderer2DData.useCameraSortingLayerTexture || !Mathf.Approximately(cameraData.renderScale, 1f) || (base.DebugHandler != null && base.DebugHandler.WriteToDebugScreenTexture(ref cameraData));
		result.requiresDepthTexture |= !cameraData.resolveFinalTarget && m_UseDepthStencilBuffer;
		return result;
	}

	private void CreateRenderTextures(ref RenderPassInputSummary renderPassInputs, CommandBuffer cmd, ref CameraData cameraData, bool forceCreateColorTexture, FilterMode colorTextureFilterMode, out RTHandle colorTargetHandle, out RTHandle depthTargetHandle)
	{
		ref RenderTextureDescriptor cameraTargetDescriptor = ref cameraData.cameraTargetDescriptor;
		RenderTextureDescriptor desc = cameraTargetDescriptor;
		desc.depthBufferBits = 0;
		m_ColorBufferSystem.SetCameraSettings(desc, colorTextureFilterMode);
		if (cameraData.renderType == CameraRenderType.Base)
		{
			m_CreateColorTexture = renderPassInputs.requiresColorTexture;
			m_CreateDepthTexture = renderPassInputs.requiresDepthTexture;
			m_CreateColorTexture |= forceCreateColorTexture;
			m_CreateDepthTexture |= createColorTexture;
			if (createColorTexture)
			{
				if (m_ColorBufferSystem.PeekBackBuffer() == null || m_ColorBufferSystem.PeekBackBuffer().nameID != BuiltinRenderTextureType.CameraTarget)
				{
					m_ColorTextureHandle = m_ColorBufferSystem.GetBackBuffer(cmd);
					cmd.SetGlobalTexture("_CameraColorTexture", m_ColorTextureHandle.nameID);
					cmd.SetGlobalTexture("_AfterPostProcessTexture", m_ColorTextureHandle.nameID);
				}
				m_ColorTextureHandle = m_ColorBufferSystem.PeekBackBuffer();
			}
			if (createDepthTexture)
			{
				RenderTextureDescriptor descriptor = cameraTargetDescriptor;
				descriptor.colorFormat = RenderTextureFormat.Depth;
				descriptor.graphicsFormat = GraphicsFormat.None;
				descriptor.depthBufferBits = 32;
				if (!cameraData.resolveFinalTarget && m_UseDepthStencilBuffer)
				{
					descriptor.bindMS = descriptor.msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && SystemInfo.supportsMultisampledTextures != 0;
				}
				RenderingUtils.ReAllocateIfNeeded(ref m_DepthTextureHandle, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraDepthAttachment");
			}
			colorTargetHandle = (createColorTexture ? m_ColorTextureHandle : ScriptableRenderer.k_CameraTarget);
			depthTargetHandle = (createDepthTexture ? m_DepthTextureHandle : ScriptableRenderer.k_CameraTarget);
		}
		else
		{
			cameraData.baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
			Renderer2D renderer2D = (Renderer2D)component.scriptableRenderer;
			if (m_ColorBufferSystem != renderer2D.m_ColorBufferSystem)
			{
				m_ColorBufferSystem.Dispose();
				m_ColorBufferSystem = renderer2D.m_ColorBufferSystem;
			}
			m_CreateColorTexture = true;
			m_CreateDepthTexture = true;
			m_ColorTextureHandle = renderer2D.m_ColorTextureHandle;
			m_DepthTextureHandle = renderer2D.m_DepthTextureHandle;
			colorTargetHandle = m_ColorTextureHandle;
			depthTargetHandle = m_DepthTextureHandle;
		}
	}

	public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		ref RenderTextureDescriptor cameraTargetDescriptor = ref cameraData.cameraTargetDescriptor;
		bool flag = renderingData.postProcessingEnabled && m_PostProcessPasses.isCreated;
		bool flag2 = renderingData.cameraData.postProcessEnabled && m_PostProcessPasses.isCreated;
		bool resolveFinalTarget = cameraData.resolveFinalTarget;
		FilterMode colorTextureFilterMode = FilterMode.Bilinear;
		PixelPerfectCamera component = null;
		bool forceCreateColorTexture = false;
		bool flag3 = false;
		if (base.DebugHandler != null)
		{
			if (base.DebugHandler.AreAnySettingsActive)
			{
				flag = flag && base.DebugHandler.IsPostProcessingAllowed;
				flag2 = flag2 && base.DebugHandler.IsPostProcessingAllowed;
			}
			if (base.DebugHandler.IsActiveForCamera(ref cameraData))
			{
				if (base.DebugHandler.WriteToDebugScreenTexture(ref cameraData))
				{
					RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
					DebugHandler.ConfigureColorDescriptorForDebugScreen(ref descriptor, cameraData.pixelWidth, cameraData.pixelHeight);
					RenderingUtils.ReAllocateIfNeeded(ref base.DebugHandler.DebugScreenColorHandle, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_DebugScreenColor");
					RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
					DebugHandler.ConfigureDepthDescriptorForDebugScreen(ref descriptor2, 32, cameraData.pixelWidth, cameraData.pixelHeight);
					RenderingUtils.ReAllocateIfNeeded(ref base.DebugHandler.DebugScreenDepthHandle, in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_DebugScreenDepth");
				}
				if (base.DebugHandler.HDRDebugViewIsActive(ref cameraData))
				{
					base.DebugHandler.hdrDebugViewPass.Setup(ref cameraData, base.DebugHandler.DebugDisplaySettings.lightingSettings.hdrDebugMode);
					EnqueuePass(base.DebugHandler.hdrDebugViewPass);
				}
			}
		}
		if (cameraData.renderType == CameraRenderType.Base && resolveFinalTarget)
		{
			cameraData.camera.TryGetComponent<PixelPerfectCamera>(out component);
			if (component != null && component.enabled)
			{
				if (component.offscreenRTSize != Vector2Int.zero)
				{
					forceCreateColorTexture = true;
					cameraTargetDescriptor.width = component.offscreenRTSize.x;
					cameraTargetDescriptor.height = component.offscreenRTSize.y;
					(base.activeRenderPassQueue.Find((ScriptableRenderPass x) => x is FullScreenPassRendererFeature.FullScreenRenderPass) as FullScreenPassRendererFeature.FullScreenRenderPass)?.ReAllocate(cameraTargetDescriptor);
				}
				colorTextureFilterMode = FilterMode.Point;
				flag3 = component.gridSnapping == PixelPerfectCamera.GridSnapping.UpscaleRenderTexture || component.requiresUpscalePass;
			}
		}
		RenderPassInputSummary renderPassInputs = GetRenderPassInputs(ref renderingData, ref cameraData);
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		RTHandle source;
		RTHandle depth;
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			CreateRenderTextures(ref renderPassInputs, commandBuffer, ref cameraData, forceCreateColorTexture, colorTextureFilterMode, out source, out depth);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
		ConfigureCameraTarget(source, depth);
		if (flag2)
		{
			colorGradingLutPass.ConfigureDescriptor(in renderingData.postProcessingData, out var descriptor3, out var filterMode);
			RenderingUtils.ReAllocateIfNeeded(ref m_PostProcessPasses.m_ColorGradingLut, in descriptor3, filterMode, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_InternalGradingLut");
			colorGradingLutPass.Setup(colorGradingLutHandle);
			EnqueuePass(colorGradingLutPass);
		}
		m_Render2DLightingPass.Setup(renderPassInputs.requiresDepthTexture || m_UseDepthStencilBuffer);
		m_Render2DLightingPass.ConfigureTarget(source, depth);
		EnqueuePass(m_Render2DLightingPass);
		bool rendersOverlayUI = cameraData.rendersOverlayUI;
		bool isHDROutputActive = cameraData.isHDROutputActive;
		if (rendersOverlayUI && isHDROutputActive)
		{
			m_DrawOffscreenUIPass.Setup(ref cameraData, 32);
			EnqueuePass(m_DrawOffscreenUIPass);
		}
		bool flag4 = cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing && !isHDROutputActive;
		bool flag5 = resolveFinalTarget && !flag3 && flag && flag4;
		bool useSwapBuffer = base.activeRenderPassQueue.Find((ScriptableRenderPass x) => x.renderPassEvent == RenderPassEvent.AfterRenderingPostProcessing) != null;
		bool flag6 = base.DebugHandler == null || !base.DebugHandler.HDRDebugViewIsActive(ref cameraData);
		if (flag2)
		{
			RenderTextureDescriptor descriptor4 = PostProcessPass.GetCompatibleDescriptor(cameraTargetDescriptor, cameraTargetDescriptor.width, cameraTargetDescriptor.height, cameraTargetDescriptor.graphicsFormat);
			RenderingUtils.ReAllocateIfNeeded(ref m_PostProcessPasses.m_AfterPostProcessColor, in descriptor4, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_AfterPostProcessTexture");
			postProcessPass.Setup(in cameraTargetDescriptor, in source, afterPostProcessColorHandle, in depth, colorGradingLutHandle, flag5, afterPostProcessColorHandle.nameID == ScriptableRenderer.k_CameraTarget.nameID && flag6);
			EnqueuePass(postProcessPass);
		}
		RTHandle upscaleHandle = source;
		if (component != null && component.enabled && component.cropFrame != PixelPerfectCamera.CropFrame.None)
		{
			EnqueuePass(m_PixelPerfectBackgroundPass);
			if (component.requiresUpscalePass)
			{
				int width = component.refResolutionX * component.pixelRatio;
				int height = component.refResolutionY * component.pixelRatio;
				m_UpscalePass.Setup(source, width, height, component.finalBlitFilterMode, ref renderingData, out upscaleHandle);
				EnqueuePass(m_UpscalePass);
			}
		}
		if (flag5)
		{
			finalPostProcessPass.SetupFinalPass(in upscaleHandle, useSwapBuffer, flag6);
			EnqueuePass(finalPostProcessPass);
		}
		else if (resolveFinalTarget && upscaleHandle != ScriptableRenderer.k_CameraTarget)
		{
			m_FinalBlitPass.Setup(cameraTargetDescriptor, upscaleHandle);
			EnqueuePass(m_FinalBlitPass);
		}
		if (rendersOverlayUI && !isHDROutputActive)
		{
			EnqueuePass(m_DrawOverlayUIPass);
		}
	}

	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
		cullingParameters.cullingOptions = CullingOptions.None;
		cullingParameters.isOrthographic = cameraData.camera.orthographic;
		cullingParameters.shadowDistance = 0f;
		m_LightCullResult.SetupCulling(ref cullingParameters, cameraData.camera);
	}

	internal override void SwapColorBuffer(CommandBuffer cmd)
	{
		m_ColorBufferSystem.Swap();
		if (m_DepthTextureHandle.nameID != BuiltinRenderTextureType.CameraTarget)
		{
			ConfigureCameraTarget(m_ColorBufferSystem.GetBackBuffer(cmd), m_DepthTextureHandle);
		}
		else
		{
			ConfigureCameraColorTarget(m_ColorBufferSystem.GetBackBuffer(cmd));
		}
		m_ColorTextureHandle = m_ColorBufferSystem.GetBackBuffer(cmd);
		cmd.SetGlobalTexture("_CameraColorTexture", m_ColorTextureHandle.nameID);
		cmd.SetGlobalTexture("_AfterPostProcessTexture", m_ColorTextureHandle.nameID);
	}

	internal override RTHandle GetCameraColorFrontBuffer(CommandBuffer cmd)
	{
		return m_ColorBufferSystem.GetFrontBuffer(cmd);
	}

	internal override RTHandle GetCameraColorBackBuffer(CommandBuffer cmd)
	{
		return m_ColorBufferSystem.GetBackBuffer(cmd);
	}

	internal override void EnableSwapBufferMSAA(bool enable)
	{
		m_ColorBufferSystem.EnableMSAA(enable);
	}
}
