using System;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

public sealed class UniversalRenderer : ScriptableRenderer
{
	private static class Profiling
	{
		private const string k_Name = "UniversalRenderer";

		public static readonly ProfilingSampler createCameraRenderTarget = new ProfilingSampler("UniversalRenderer.CreateCameraRenderTarget");
	}

	private struct RenderPassInputSummary
	{
		internal bool requiresDepthTexture;

		internal bool requiresDepthPrepass;

		internal bool requiresNormalsTexture;

		internal bool requiresColorTexture;

		internal bool requiresColorTextureCreated;

		internal bool requiresMotionVectors;

		internal RenderPassEvent requiresDepthNormalAtEvent;

		internal RenderPassEvent requiresDepthTextureEarliestEvent;
	}

	internal class RenderGraphFrameResources
	{
		internal TextureHandle backBufferColor;

		internal TextureHandle cameraColor;

		internal TextureHandle cameraDepth;

		internal TextureHandle mainShadowsTexture;

		internal TextureHandle additionalShadowsTexture;

		internal TextureHandle[] gbuffer;

		internal TextureHandle cameraOpaqueTexture;

		internal TextureHandle cameraDepthTexture;

		internal TextureHandle cameraNormalsTexture;

		internal TextureHandle motionVectorColor;

		internal TextureHandle motionVectorDepth;

		internal TextureHandle internalColorLut;

		internal TextureHandle overlayUITexture;
	}

	private const GraphicsFormat k_DepthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;

	private const int k_DepthBufferBits = 32;

	private const int k_FinalBlitPassQueueOffset = 1;

	private const int k_AfterFinalBlitPassQueueOffset = 2;

	private static readonly List<ShaderTagId> k_DepthNormalsOnly = new List<ShaderTagId>
	{
		new ShaderTagId("DepthNormalsOnly")
	};

	private bool m_Clustering;

	private DepthOnlyPass m_DepthPrepass;

	private DepthNormalOnlyPass m_DepthNormalPrepass;

	private CopyDepthPass m_PrimedDepthCopyPass;

	private MotionVectorRenderPass m_MotionVectorPass;

	private MainLightShadowCasterPass m_MainLightShadowCasterPass;

	private AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;

	private GBufferPass m_GBufferPass;

	private CopyDepthPass m_GBufferCopyDepthPass;

	private DeferredPass m_DeferredPass;

	private DrawObjectsPass m_RenderOpaqueForwardOnlyPass;

	private DrawObjectsPass m_RenderOpaqueForwardPass;

	private DrawObjectsWithRenderingLayersPass m_RenderOpaqueForwardWithRenderingLayersPass;

	private DrawSkyboxPass m_DrawSkyboxPass;

	private CopyDepthPass m_CopyDepthPass;

	private CopyColorPass m_CopyColorPass;

	private TransparentSettingsPass m_TransparentSettingsPass;

	private DrawObjectsPass m_RenderTransparentForwardPass;

	private InvokeOnRenderObjectCallbackPass m_OnRenderObjectCallbackPass;

	private FinalBlitPass m_FinalBlitPass;

	private CapturePass m_CapturePass;

	private XROcclusionMeshPass m_XROcclusionMeshPass;

	private CopyDepthPass m_XRCopyDepthPass;

	private DrawScreenSpaceUIPass m_DrawOffscreenUIPass;

	private DrawScreenSpaceUIPass m_DrawOverlayUIPass;

	internal RenderTargetBufferSystem m_ColorBufferSystem;

	internal RTHandle m_ActiveCameraColorAttachment;

	private RTHandle m_ColorFrontBuffer;

	internal RTHandle m_ActiveCameraDepthAttachment;

	internal RTHandle m_CameraDepthAttachment;

	private RTHandle m_XRTargetHandleAlias;

	internal RTHandle m_DepthTexture;

	private RTHandle m_NormalsTexture;

	private RTHandle m_DecalLayersTexture;

	private RTHandle m_OpaqueColor;

	private RTHandle m_MotionVectorColor;

	private RTHandle m_MotionVectorDepth;

	private ForwardLights m_ForwardLights;

	private DeferredLights m_DeferredLights;

	private RenderingMode m_RenderingMode;

	private DepthPrimingMode m_DepthPrimingMode;

	private CopyDepthMode m_CopyDepthMode;

	private bool m_DepthPrimingRecommended;

	private StencilState m_DefaultStencilState;

	private LightCookieManager m_LightCookieManager;

	private IntermediateTextureMode m_IntermediateTextureMode;

	private bool m_VulkanEnablePreTransform;

	private Material m_BlitMaterial;

	private Material m_BlitHDRMaterial;

	private Material m_CopyDepthMaterial;

	private Material m_SamplingMaterial;

	private Material m_StencilDeferredMaterial;

	private Material m_CameraMotionVecMaterial;

	private Material m_ObjectMotionVecMaterial;

	private PostProcessPasses m_PostProcessPasses;

	private static RTHandle m_RenderGraphCameraColorHandle;

	private static RTHandle m_RenderGraphCameraDepthHandle;

	internal static TextureHandle m_ActiveRenderGraphColor;

	internal static TextureHandle m_ActiveRenderGraphDepth;

	internal bool m_TargetIsBackbuffer;

	internal RenderGraphFrameResources frameResources = new RenderGraphFrameResources();

	private static bool m_UseIntermediateTexture = false;

	internal RenderingMode renderingModeRequested => m_RenderingMode;

	internal RenderingMode renderingModeActual
	{
		get
		{
			if (renderingModeRequested != RenderingMode.Deferred || (!GL.wireframe && (base.DebugHandler == null || !base.DebugHandler.IsActiveModeUnsupportedForDeferred) && m_DeferredLights != null && m_DeferredLights.IsRuntimeSupportedThisFrame() && !m_DeferredLights.IsOverlay))
			{
				return renderingModeRequested;
			}
			return RenderingMode.Forward;
		}
	}

	internal bool accurateGbufferNormals
	{
		get
		{
			if (m_DeferredLights == null)
			{
				return false;
			}
			return m_DeferredLights.AccurateGbufferNormals;
		}
	}

	public DepthPrimingMode depthPrimingMode
	{
		get
		{
			return m_DepthPrimingMode;
		}
		set
		{
			m_DepthPrimingMode = value;
		}
	}

	internal ColorGradingLutPass colorGradingLutPass => m_PostProcessPasses.colorGradingLutPass;

	internal PostProcessPass postProcessPass => m_PostProcessPasses.postProcessPass;

	internal PostProcessPass finalPostProcessPass => m_PostProcessPasses.finalPostProcessPass;

	internal RTHandle colorGradingLut => m_PostProcessPasses.colorGradingLut;

	internal DeferredLights deferredLights => m_DeferredLights;

	public override int SupportedCameraStackingTypes()
	{
		switch (m_RenderingMode)
		{
		case RenderingMode.Forward:
		case RenderingMode.ForwardPlus:
			return 3;
		case RenderingMode.Deferred:
			return 1;
		default:
			return 0;
		}
	}

	public UniversalRenderer(UniversalRendererData data)
		: base(data)
	{
		PlatformAutoDetect.Initialize();
		XRSystem.Initialize(XRPassUniversal.Create, data.xrSystemData.shaders.xrOcclusionMeshPS, data.xrSystemData.shaders.xrMirrorViewPS);
		m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.coreBlitPS);
		m_BlitHDRMaterial = CoreUtils.CreateEngineMaterial(data.shaders.blitHDROverlay);
		m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(data.shaders.copyDepthPS);
		m_SamplingMaterial = CoreUtils.CreateEngineMaterial(data.shaders.samplingPS);
		m_StencilDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.stencilDeferredPS);
		m_CameraMotionVecMaterial = CoreUtils.CreateEngineMaterial(data.shaders.cameraMotionVector);
		m_ObjectMotionVecMaterial = CoreUtils.CreateEngineMaterial(data.shaders.objectMotionVector);
		StencilStateData defaultStencilState = data.defaultStencilState;
		m_DefaultStencilState = StencilState.defaultValue;
		m_DefaultStencilState.enabled = defaultStencilState.overrideStencilState;
		m_DefaultStencilState.SetCompareFunction(defaultStencilState.stencilCompareFunction);
		m_DefaultStencilState.SetPassOperation(defaultStencilState.passOperation);
		m_DefaultStencilState.SetFailOperation(defaultStencilState.failOperation);
		m_DefaultStencilState.SetZFailOperation(defaultStencilState.zFailOperation);
		m_IntermediateTextureMode = data.intermediateTextureMode;
		UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
		if ((object)asset != null && asset.supportsLightCookies)
		{
			LightCookieManager.Settings settings = LightCookieManager.Settings.Create();
			UniversalRenderPipelineAsset asset2 = UniversalRenderPipeline.asset;
			if ((bool)asset2)
			{
				settings.atlas.format = asset2.additionalLightsCookieFormat;
				settings.atlas.resolution = asset2.additionalLightsCookieResolution;
			}
			m_LightCookieManager = new LightCookieManager(ref settings);
		}
		base.stripShadowsOffVariants = true;
		base.stripAdditionalLightOffVariants = true;
		ForwardLights.InitParams initParams = default(ForwardLights.InitParams);
		initParams.lightCookieManager = m_LightCookieManager;
		initParams.forwardPlus = data.renderingMode == RenderingMode.ForwardPlus;
		m_Clustering = data.renderingMode == RenderingMode.ForwardPlus;
		m_ForwardLights = new ForwardLights(initParams);
		m_RenderingMode = data.renderingMode;
		m_DepthPrimingMode = data.depthPrimingMode;
		m_CopyDepthMode = data.copyDepthMode;
		m_DepthPrimingRecommended = true;
		m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
		m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
		m_XROcclusionMeshPass = new XROcclusionMeshPass(RenderPassEvent.BeforeRenderingOpaques);
		m_XRCopyDepthPass = new CopyDepthPass((RenderPassEvent)1002, m_CopyDepthMaterial);
		m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.opaque, data.opaqueLayerMask);
		m_DepthNormalPrepass = new DepthNormalOnlyPass(RenderPassEvent.BeforeRenderingPrePasses, RenderQueueRange.opaque, data.opaqueLayerMask);
		if (renderingModeRequested == RenderingMode.Forward || renderingModeRequested == RenderingMode.ForwardPlus)
		{
			m_PrimedDepthCopyPass = new CopyDepthPass(RenderPassEvent.AfterRenderingPrePasses, m_CopyDepthMaterial, shouldClear: true);
		}
		if (renderingModeRequested == RenderingMode.Deferred)
		{
			m_DeferredLights = new DeferredLights(new DeferredLights.InitParams
			{
				stencilDeferredMaterial = m_StencilDeferredMaterial,
				lightCookieManager = m_LightCookieManager
			}, useRenderPassEnabled);
			m_DeferredLights.AccurateGbufferNormals = data.accurateGbufferNormals;
			m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingGbuffer, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, defaultStencilState.stencilReference, m_DeferredLights);
			StencilState stencilState = DeferredLights.OverwriteStencil(m_DefaultStencilState, 96);
			ShaderTagId[] shaderTagIds = new ShaderTagId[3]
			{
				new ShaderTagId("UniversalForwardOnly"),
				new ShaderTagId("SRPDefaultUnlit"),
				new ShaderTagId("LightweightForward")
			};
			int stencilReference = defaultStencilState.stencilReference | 0;
			m_GBufferCopyDepthPass = new CopyDepthPass((RenderPassEvent)211, m_CopyDepthMaterial, shouldClear: true);
			m_DeferredPass = new DeferredPass(RenderPassEvent.BeforeRenderingDeferredLights, m_DeferredLights);
			m_RenderOpaqueForwardOnlyPass = new DrawObjectsPass("Render Opaques Forward Only", shaderTagIds, opaque: true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, stencilState, stencilReference);
		}
		m_RenderOpaqueForwardPass = new DrawObjectsPass(URPProfileId.DrawOpaqueObjects, opaque: true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, defaultStencilState.stencilReference);
		m_RenderOpaqueForwardWithRenderingLayersPass = new DrawObjectsWithRenderingLayersPass(URPProfileId.DrawOpaqueObjects, opaque: true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, defaultStencilState.stencilReference);
		bool flag = m_CopyDepthMode == CopyDepthMode.AfterTransparents;
		RenderPassEvent renderPassEvent = (flag ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingSkybox);
		m_CopyDepthPass = new CopyDepthPass(renderPassEvent, m_CopyDepthMaterial, shouldClear: true, copyToDepth: false, RenderingUtils.MultisampleDepthResolveSupported() && SystemInfo.supportsMultisampleAutoResolve && flag);
		m_MotionVectorPass = new MotionVectorRenderPass(renderPassEvent + 1, m_CameraMotionVecMaterial, m_ObjectMotionVecMaterial, data.opaqueLayerMask);
		m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
		m_CopyColorPass = new CopyColorPass(RenderPassEvent.AfterRenderingSkybox, m_SamplingMaterial, m_BlitMaterial);
		m_TransparentSettingsPass = new TransparentSettingsPass(RenderPassEvent.BeforeRenderingTransparents, data.shadowTransparentReceive);
		m_RenderTransparentForwardPass = new DrawObjectsPass(URPProfileId.DrawTransparentObjects, opaque: false, RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent, data.transparentLayerMask, m_DefaultStencilState, defaultStencilState.stencilReference);
		m_OnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
		m_DrawOffscreenUIPass = new DrawScreenSpaceUIPass(RenderPassEvent.BeforeRenderingPostProcessing, renderOffscreen: true);
		m_DrawOverlayUIPass = new DrawScreenSpaceUIPass((RenderPassEvent)1002, renderOffscreen: false);
		PostProcessParams postProcessParams = PostProcessParams.Create();
		postProcessParams.blitMaterial = m_BlitMaterial;
		postProcessParams.requestHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		UniversalRenderPipelineAsset asset3 = UniversalRenderPipeline.asset;
		if ((bool)asset3)
		{
			postProcessParams.requestHDRFormat = UniversalRenderPipeline.MakeRenderTextureGraphicsFormat(asset3.supportsHDR, asset3.hdrColorBufferPrecision, needsAlpha: false);
		}
		m_PostProcessPasses = new PostProcessPasses(data.postProcessData, ref postProcessParams);
		m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
		m_FinalBlitPass = new FinalBlitPass((RenderPassEvent)1001, m_BlitMaterial, m_BlitHDRMaterial);
		m_ColorBufferSystem = new RenderTargetBufferSystem("_CameraColorAttachment");
		base.supportedRenderingFeatures = new RenderingFeatures();
		if (renderingModeRequested == RenderingMode.Deferred)
		{
			base.supportedRenderingFeatures.msaa = false;
			base.unsupportedGraphicsDeviceTypes = new GraphicsDeviceType[3]
			{
				GraphicsDeviceType.OpenGLCore,
				GraphicsDeviceType.OpenGLES2,
				GraphicsDeviceType.OpenGLES3
			};
		}
		LensFlareCommonSRP.mergeNeeded = 0;
		LensFlareCommonSRP.maxLensFlareWithOcclusionTemporalSample = 1;
		LensFlareCommonSRP.Initialize();
		m_VulkanEnablePreTransform = GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION);
	}

	protected override void Dispose(bool disposing)
	{
		m_ForwardLights.Cleanup();
		m_GBufferPass?.Dispose();
		m_PostProcessPasses.Dispose();
		m_FinalBlitPass?.Dispose();
		m_DrawOffscreenUIPass?.Dispose();
		m_DrawOverlayUIPass?.Dispose();
		m_XRTargetHandleAlias?.Release();
		ReleaseRenderTargets();
		base.Dispose(disposing);
		CoreUtils.Destroy(m_BlitMaterial);
		CoreUtils.Destroy(m_BlitHDRMaterial);
		CoreUtils.Destroy(m_CopyDepthMaterial);
		CoreUtils.Destroy(m_SamplingMaterial);
		CoreUtils.Destroy(m_StencilDeferredMaterial);
		CoreUtils.Destroy(m_CameraMotionVecMaterial);
		CoreUtils.Destroy(m_ObjectMotionVecMaterial);
		CleanupRenderGraphResources();
		LensFlareCommonSRP.Dispose();
	}

	internal override void ReleaseRenderTargets()
	{
		m_ColorBufferSystem.Dispose();
		if (m_DeferredLights != null && !m_DeferredLights.UseRenderPass)
		{
			m_GBufferPass?.Dispose();
		}
		m_PostProcessPasses.ReleaseRenderTargets();
		m_MainLightShadowCasterPass?.Dispose();
		m_AdditionalLightsShadowCasterPass?.Dispose();
		m_CameraDepthAttachment?.Release();
		m_DepthTexture?.Release();
		m_NormalsTexture?.Release();
		m_DecalLayersTexture?.Release();
		m_OpaqueColor?.Release();
		m_MotionVectorColor?.Release();
		m_MotionVectorDepth?.Release();
		hasReleasedRTs = true;
	}

	private void SetupFinalPassDebug(ref CameraData cameraData)
	{
		if (base.DebugHandler == null || !base.DebugHandler.IsActiveForCamera(ref cameraData))
		{
			return;
		}
		if (base.DebugHandler.TryGetFullscreenDebugMode(out var debugFullScreenMode, out var textureHeightPercent) && (debugFullScreenMode != DebugFullScreenMode.ReflectionProbeAtlas || m_Clustering))
		{
			Camera camera = cameraData.camera;
			float num = camera.pixelWidth;
			float num2 = camera.pixelHeight;
			float num3 = Mathf.Clamp01((float)textureHeightPercent / 100f);
			float num4 = num3 * num2;
			float num5 = num3 * num;
			if (debugFullScreenMode == DebugFullScreenMode.ReflectionProbeAtlas)
			{
				RenderTexture atlasRT = m_ForwardLights.reflectionProbeManager.atlasRT;
				float num6 = num4 * (float)atlasRT.width / (float)atlasRT.height;
				if (num6 > num5)
				{
					num4 = num5 * (float)atlasRT.height / (float)atlasRT.width;
				}
				else
				{
					num5 = num6;
				}
			}
			float num7 = num5 / num;
			float num8 = num4 / num2;
			Rect displayRect = new Rect(1f - num7, 1f - num8, num7, num8);
			switch (debugFullScreenMode)
			{
			case DebugFullScreenMode.Depth:
				base.DebugHandler.SetDebugRenderTarget(m_DepthTexture.nameID, displayRect, supportsStereo: true);
				break;
			case DebugFullScreenMode.AdditionalLightsShadowMap:
				base.DebugHandler.SetDebugRenderTarget(m_AdditionalLightsShadowCasterPass.m_AdditionalLightsShadowmapHandle, displayRect, supportsStereo: false);
				break;
			case DebugFullScreenMode.MainLightShadowMap:
				base.DebugHandler.SetDebugRenderTarget(m_MainLightShadowCasterPass.m_MainLightShadowmapTexture, displayRect, supportsStereo: false);
				break;
			case DebugFullScreenMode.ReflectionProbeAtlas:
				base.DebugHandler.SetDebugRenderTarget(m_ForwardLights.reflectionProbeManager.atlasRT, displayRect, supportsStereo: false);
				break;
			}
		}
		else
		{
			base.DebugHandler.ResetDebugRenderTarget();
		}
	}

	public static bool IsOffscreenDepthTexture(in CameraData cameraData)
	{
		if (cameraData.targetTexture != null)
		{
			return cameraData.targetTexture.format == RenderTextureFormat.Depth;
		}
		return false;
	}

	private bool IsDepthPrimingEnabled(ref CameraData cameraData)
	{
		if (!CanCopyDepth(ref cameraData))
		{
			return false;
		}
		bool flag = !IsWebGL();
		bool num = (m_DepthPrimingRecommended && m_DepthPrimingMode == DepthPrimingMode.Auto) || m_DepthPrimingMode == DepthPrimingMode.Forced;
		bool flag2 = m_RenderingMode == RenderingMode.Forward || m_RenderingMode == RenderingMode.ForwardPlus;
		bool flag3 = cameraData.renderType == CameraRenderType.Base || cameraData.clearDepth;
		bool flag4 = cameraData.cameraType != CameraType.Reflection;
		bool flag5 = !IsOffscreenDepthTexture(in cameraData);
		return num && flag2 && flag3 && flag4 && flag5 && flag;
	}

	private bool IsWebGL()
	{
		return false;
	}

	private bool IsGLESDevice()
	{
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
		{
			return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3;
		}
		return true;
	}

	private bool IsGLDevice()
	{
		if (!IsGLESDevice())
		{
			return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
		}
		return true;
	}

	public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_ForwardLights.PreSetup(ref renderingData);
		ref CameraData cameraData = ref renderingData.cameraData;
		Camera camera = cameraData.camera;
		RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (base.DebugHandler != null && base.DebugHandler.IsActiveForCamera(ref cameraData))
		{
			if (base.DebugHandler.WriteToDebugScreenTexture(ref cameraData))
			{
				RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
				DebugHandler.ConfigureColorDescriptorForDebugScreen(ref descriptor2, cameraData.pixelWidth, cameraData.pixelHeight);
				RenderingUtils.ReAllocateIfNeeded(ref base.DebugHandler.DebugScreenColorHandle, in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_DebugScreenColor");
				RenderTextureDescriptor descriptor3 = cameraData.cameraTargetDescriptor;
				DebugHandler.ConfigureDepthDescriptorForDebugScreen(ref descriptor3, 32, cameraData.pixelWidth, cameraData.pixelHeight);
				RenderingUtils.ReAllocateIfNeeded(ref base.DebugHandler.DebugScreenDepthHandle, in descriptor3, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_DebugScreenDepth");
			}
			if (base.DebugHandler.HDRDebugViewIsActive(ref cameraData))
			{
				base.DebugHandler.hdrDebugViewPass.Setup(ref cameraData, base.DebugHandler.DebugDisplaySettings.lightingSettings.hdrDebugMode);
				EnqueuePass(base.DebugHandler.hdrDebugViewPass);
			}
		}
		if (cameraData.cameraType != CameraType.Game)
		{
			useRenderPassEnabled = false;
		}
		base.useDepthPriming = IsDepthPrimingEnabled(ref cameraData);
		if (IsOffscreenDepthTexture(in cameraData))
		{
			ConfigureCameraTarget(ScriptableRenderer.k_CameraTarget, ScriptableRenderer.k_CameraTarget);
			SetupRenderPasses(in renderingData);
			EnqueuePass(m_RenderOpaqueForwardPass);
			EnqueuePass(m_RenderTransparentForwardPass);
			return;
		}
		bool isPreviewCamera = cameraData.isPreviewCamera;
		bool flag = (base.rendererFeatures.Count != 0 && m_IntermediateTextureMode == IntermediateTextureMode.Always && !isPreviewCamera) || (Application.isEditor && m_Clustering);
		RenderPassInputSummary renderPassInputs = GetRenderPassInputs(ref renderingData);
		RenderingLayerUtils.Event combinedEvent;
		RenderingLayerUtils.MaskSize combinedMaskSize;
		bool flag2 = RenderingLayerUtils.RequireRenderingLayers(this, base.rendererFeatures, descriptor.msaaSamples, out combinedEvent, out combinedMaskSize);
		if (IsGLDevice())
		{
			flag2 = false;
		}
		bool flag3 = false;
		bool flag4 = false;
		if (flag2 && renderingModeActual != RenderingMode.Deferred)
		{
			switch (combinedEvent)
			{
			case RenderingLayerUtils.Event.DepthNormalPrePass:
				flag3 = true;
				break;
			case RenderingLayerUtils.Event.Opaque:
				flag4 = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		if (flag3)
		{
			renderPassInputs.requiresNormalsTexture = true;
		}
		if (m_DeferredLights != null)
		{
			m_DeferredLights.RenderingLayerMaskSize = combinedMaskSize;
			m_DeferredLights.UseDecalLayers = flag2;
			m_DeferredLights.HasNormalPrepass = renderPassInputs.requiresNormalsTexture;
			m_DeferredLights.ResolveMixedLightingMode(ref renderingData);
			m_DeferredLights.IsOverlay = cameraData.renderType == CameraRenderType.Overlay;
			if (m_DeferredLights.UseRenderPass)
			{
				foreach (ScriptableRenderPass item in base.activeRenderPassQueue)
				{
					if (item.renderPassEvent >= RenderPassEvent.AfterRenderingGbuffer && item.renderPassEvent <= RenderPassEvent.BeforeRenderingDeferredLights)
					{
						m_DeferredLights.DisableFramebufferFetchInput();
						break;
					}
				}
			}
		}
		bool flag5 = cameraData.postProcessEnabled && m_PostProcessPasses.isCreated;
		bool flag6 = renderingData.postProcessingEnabled && m_PostProcessPasses.isCreated;
		bool flag7 = flag5 && cameraData.postProcessingRequiresDepthTexture;
		bool flag8 = cameraData.postProcessEnabled && m_PostProcessPasses.isCreated;
		bool flag9 = cameraData.isSceneViewCamera || cameraData.isPreviewCamera;
		bool num = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || m_DepthPrimingMode == DepthPrimingMode.Forced;
		bool flag10 = false;
		bool flag11 = m_MainLightShadowCasterPass.Setup(ref renderingData);
		bool flag12 = m_AdditionalLightsShadowCasterPass.Setup(ref renderingData);
		bool flag13 = m_TransparentSettingsPass.Setup();
		bool flag14 = m_CopyDepthMode == CopyDepthMode.ForcePrepass;
		bool flag15 = (num || flag7) && (!CanCopyDepth(ref renderingData.cameraData) || flag14);
		flag15 = flag15 || flag9;
		flag15 = flag15 || flag10;
		flag15 = flag15 || isPreviewCamera;
		flag15 |= renderPassInputs.requiresDepthPrepass;
		flag15 |= renderPassInputs.requiresNormalsTexture;
		if (flag15 && renderingModeActual == RenderingMode.Deferred && !renderPassInputs.requiresNormalsTexture)
		{
			flag15 = false;
		}
		flag15 |= base.useDepthPriming;
		if (num)
		{
			RenderPassEvent renderPassEvent = ((m_CopyDepthMode == CopyDepthMode.AfterTransparents) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques);
			if (renderPassInputs.requiresDepthTexture)
			{
				renderPassEvent = (RenderPassEvent)Mathf.Min(500, (int)(renderPassInputs.requiresDepthTextureEarliestEvent - 1));
			}
			m_CopyDepthPass.renderPassEvent = renderPassEvent;
			if (renderPassEvent < RenderPassEvent.AfterRenderingTransparents)
			{
				m_CopyDepthPass.m_CopyResolvedDepth = false;
				m_CopyDepthMode = CopyDepthMode.AfterOpaques;
			}
		}
		else if (flag7 || flag9 || flag10)
		{
			m_CopyDepthPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
		}
		flag |= RequiresIntermediateColorTexture(ref cameraData);
		flag |= renderPassInputs.requiresColorTexture;
		flag |= renderPassInputs.requiresColorTextureCreated;
		flag = flag && !isPreviewCamera;
		bool flag16 = (num || flag7) && !flag15;
		flag16 |= !cameraData.resolveFinalTarget;
		flag16 |= renderingModeActual == RenderingMode.Deferred && !useRenderPassEnabled;
		flag16 |= base.useDepthPriming;
		flag16 = flag16 || flag4;
		if (cameraData.xr.enabled)
		{
			flag = flag || flag16;
		}
		if (RTHandles.rtHandleProperties.rtHandleScale.x != 1f || RTHandles.rtHandleProperties.rtHandleScale.y != 1f)
		{
			flag = flag || flag16;
		}
		if (useRenderPassEnabled || base.useDepthPriming)
		{
			flag = flag || flag16;
		}
		RenderTextureDescriptor desc = descriptor;
		desc.useMipMap = false;
		desc.autoGenerateMips = false;
		desc.depthBufferBits = 0;
		m_ColorBufferSystem.SetCameraSettings(desc, FilterMode.Bilinear);
		if (cameraData.renderType == CameraRenderType.Base)
		{
			bool flag17 = camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered;
			bool flag18 = (flag || flag16) && !flag17;
			flag16 = flag16 || flag;
			RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
			if (cameraData.xr.enabled)
			{
				renderTargetIdentifier = cameraData.xr.renderTarget;
			}
			if (m_XRTargetHandleAlias == null)
			{
				m_XRTargetHandleAlias = RTHandles.Alloc(renderTargetIdentifier);
			}
			else if (m_XRTargetHandleAlias.nameID != renderTargetIdentifier)
			{
				RTHandleStaticHelpers.SetRTHandleUserManagedWrapper(ref m_XRTargetHandleAlias, renderTargetIdentifier);
			}
			if (flag18)
			{
				CreateCameraRenderTarget(context, ref descriptor, base.useDepthPriming, commandBuffer, ref cameraData);
			}
			m_RenderOpaqueForwardPass.m_IsActiveTargetBackBuffer = !flag18;
			m_RenderTransparentForwardPass.m_IsActiveTargetBackBuffer = !flag18;
			m_DrawSkyboxPass.m_IsActiveTargetBackBuffer = !flag18;
			m_XROcclusionMeshPass.m_IsActiveTargetBackBuffer = !flag18;
			m_ActiveCameraColorAttachment = (flag ? m_ColorBufferSystem.PeekBackBuffer() : m_XRTargetHandleAlias);
			m_ActiveCameraDepthAttachment = (flag16 ? m_CameraDepthAttachment : m_XRTargetHandleAlias);
		}
		else
		{
			cameraData.baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
			UniversalRenderer universalRenderer = (UniversalRenderer)component.scriptableRenderer;
			if (m_ColorBufferSystem != universalRenderer.m_ColorBufferSystem)
			{
				m_ColorBufferSystem.Dispose();
				m_ColorBufferSystem = universalRenderer.m_ColorBufferSystem;
			}
			m_ActiveCameraColorAttachment = m_ColorBufferSystem.PeekBackBuffer();
			m_ActiveCameraDepthAttachment = universalRenderer.m_ActiveCameraDepthAttachment;
			m_XRTargetHandleAlias = universalRenderer.m_XRTargetHandleAlias;
		}
		if (base.rendererFeatures.Count != 0 && !isPreviewCamera)
		{
			ConfigureCameraColorTarget(m_ColorBufferSystem.PeekBackBuffer());
		}
		bool flag19 = renderingData.cameraData.requiresOpaqueTexture || renderPassInputs.requiresColorTexture;
		flag19 = flag19 && !isPreviewCamera;
		ConfigureCameraTarget(m_ActiveCameraColorAttachment, m_ActiveCameraDepthAttachment);
		bool flag20 = base.activeRenderPassQueue.Find((ScriptableRenderPass x) => x.renderPassEvent == RenderPassEvent.AfterRenderingPostProcessing) != null;
		if (flag11)
		{
			EnqueuePass(m_MainLightShadowCasterPass);
		}
		if (flag12)
		{
			EnqueuePass(m_AdditionalLightsShadowCasterPass);
		}
		bool flag21 = !flag15 && (renderingData.cameraData.requiresDepthTexture || flag7 || renderPassInputs.requiresDepthTexture) && flag16;
		if (base.DebugHandler != null && base.DebugHandler.IsActiveForCamera(ref cameraData))
		{
			base.DebugHandler.TryGetFullscreenDebugMode(out var debugFullScreenMode);
			if (debugFullScreenMode == DebugFullScreenMode.Depth)
			{
				flag15 = true;
			}
			if (!base.DebugHandler.IsLightingActive)
			{
				flag11 = false;
				flag12 = false;
				if (!flag9)
				{
					flag15 = false;
					base.useDepthPriming = false;
					flag8 = false;
					flag19 = false;
					flag21 = false;
				}
			}
			if (useRenderPassEnabled)
			{
				useRenderPassEnabled = base.DebugHandler.IsRenderPassSupported;
			}
		}
		cameraData.renderer.useDepthPriming = base.useDepthPriming;
		if (renderingModeActual == RenderingMode.Deferred && m_DeferredLights.UseRenderPass && (RenderPassEvent.AfterRenderingGbuffer == renderPassInputs.requiresDepthNormalAtEvent || !useRenderPassEnabled))
		{
			m_DeferredLights.DisableFramebufferFetchInput();
		}
		if ((renderingModeActual == RenderingMode.Deferred && !useRenderPassEnabled) || flag15 || flag21)
		{
			RenderTextureDescriptor descriptor4 = descriptor;
			if ((flag15 && renderingModeActual != RenderingMode.Deferred) || !RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R32_SFloat, FormatUsage.Render))
			{
				descriptor4.graphicsFormat = GraphicsFormat.None;
				descriptor4.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
				descriptor4.depthBufferBits = 32;
			}
			else
			{
				descriptor4.graphicsFormat = GraphicsFormat.R32_SFloat;
				descriptor4.depthStencilFormat = GraphicsFormat.None;
				descriptor4.depthBufferBits = 0;
			}
			descriptor4.msaaSamples = 1;
			RenderingUtils.ReAllocateIfNeeded(ref m_DepthTexture, in descriptor4, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraDepthTexture");
			commandBuffer.SetGlobalTexture(m_DepthTexture.name, m_DepthTexture.nameID);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
		if (flag2 || (renderingModeActual == RenderingMode.Deferred && m_DeferredLights.UseRenderingLayers))
		{
			ref RTHandle reference = ref m_DecalLayersTexture;
			string name = "_CameraRenderingLayersTexture";
			if (renderingModeActual == RenderingMode.Deferred && m_DeferredLights.UseRenderingLayers)
			{
				reference = ref m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferRenderingLayers];
				name = reference.name;
			}
			RenderTextureDescriptor descriptor5 = descriptor;
			descriptor5.depthBufferBits = 0;
			if (!flag4)
			{
				descriptor5.msaaSamples = 1;
			}
			if (renderingModeActual == RenderingMode.Deferred && m_DeferredLights.UseRenderingLayers)
			{
				descriptor5.graphicsFormat = m_DeferredLights.GetGBufferFormat(m_DeferredLights.GBufferRenderingLayers);
			}
			else
			{
				descriptor5.graphicsFormat = RenderingLayerUtils.GetFormat(combinedMaskSize);
			}
			if (renderingModeActual == RenderingMode.Deferred && m_DeferredLights.UseRenderingLayers)
			{
				m_DeferredLights.ReAllocateGBufferIfNeeded(descriptor5, m_DeferredLights.GBufferRenderingLayers);
			}
			else
			{
				RenderingUtils.ReAllocateIfNeeded(ref reference, in descriptor5, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, name);
			}
			commandBuffer.SetGlobalTexture(reference.name, reference.nameID);
			RenderingLayerUtils.SetupProperties(commandBuffer, combinedMaskSize);
			if (renderingModeActual == RenderingMode.Deferred)
			{
				commandBuffer.SetGlobalTexture("_CameraRenderingLayersTexture", reference.nameID);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
		if (flag15 && renderPassInputs.requiresNormalsTexture)
		{
			ref RTHandle reference2 = ref m_NormalsTexture;
			string name2 = "_CameraNormalsTexture";
			if (renderingModeActual == RenderingMode.Deferred)
			{
				reference2 = ref m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferNormalSmoothnessIndex];
				name2 = reference2.name;
			}
			RenderTextureDescriptor descriptor6 = descriptor;
			descriptor6.depthBufferBits = 0;
			descriptor6.msaaSamples = ((!base.useDepthPriming) ? 1 : descriptor.msaaSamples);
			if (renderingModeActual == RenderingMode.Deferred)
			{
				descriptor6.graphicsFormat = m_DeferredLights.GetGBufferFormat(m_DeferredLights.GBufferNormalSmoothnessIndex);
			}
			else
			{
				descriptor6.graphicsFormat = DepthNormalOnlyPass.GetGraphicsFormat();
			}
			if (renderingModeActual == RenderingMode.Deferred)
			{
				m_DeferredLights.ReAllocateGBufferIfNeeded(descriptor6, m_DeferredLights.GBufferNormalSmoothnessIndex);
			}
			else
			{
				RenderingUtils.ReAllocateIfNeeded(ref reference2, in descriptor6, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, name2);
			}
			commandBuffer.SetGlobalTexture(reference2.name, reference2.nameID);
			if (renderingModeActual == RenderingMode.Deferred)
			{
				commandBuffer.SetGlobalTexture("_CameraNormalsTexture", reference2.nameID);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
		if (flag15)
		{
			if (renderPassInputs.requiresNormalsTexture)
			{
				if (renderingModeActual == RenderingMode.Deferred)
				{
					int gBufferNormalSmoothnessIndex = m_DeferredLights.GBufferNormalSmoothnessIndex;
					if (m_DeferredLights.UseRenderingLayers)
					{
						m_DepthNormalPrepass.Setup(m_ActiveCameraDepthAttachment, m_DeferredLights.GbufferAttachments[gBufferNormalSmoothnessIndex], m_DeferredLights.GbufferAttachments[m_DeferredLights.GBufferRenderingLayers]);
					}
					else if (flag3)
					{
						m_DepthNormalPrepass.Setup(m_ActiveCameraDepthAttachment, m_DeferredLights.GbufferAttachments[gBufferNormalSmoothnessIndex], m_DecalLayersTexture);
					}
					else
					{
						m_DepthNormalPrepass.Setup(m_ActiveCameraDepthAttachment, m_DeferredLights.GbufferAttachments[gBufferNormalSmoothnessIndex]);
					}
					if (RenderPassEvent.AfterRenderingGbuffer <= renderPassInputs.requiresDepthNormalAtEvent && renderPassInputs.requiresDepthNormalAtEvent <= RenderPassEvent.BeforeRenderingOpaques)
					{
						m_DepthNormalPrepass.shaderTagIds = k_DepthNormalsOnly;
					}
				}
				else if (flag3)
				{
					m_DepthNormalPrepass.Setup(m_DepthTexture, m_NormalsTexture, m_DecalLayersTexture);
				}
				else
				{
					m_DepthNormalPrepass.Setup(m_DepthTexture, m_NormalsTexture);
				}
				EnqueuePass(m_DepthNormalPrepass);
			}
			else if (renderingModeActual != RenderingMode.Deferred)
			{
				m_DepthPrepass.Setup(descriptor, m_DepthTexture);
				EnqueuePass(m_DepthPrepass);
			}
		}
		if (base.useDepthPriming)
		{
			m_PrimedDepthCopyPass.Setup(m_ActiveCameraDepthAttachment, m_DepthTexture);
			EnqueuePass(m_PrimedDepthCopyPass);
		}
		if (flag8)
		{
			colorGradingLutPass.ConfigureDescriptor(in renderingData.postProcessingData, out var descriptor7, out var filterMode);
			RenderingUtils.ReAllocateIfNeeded(ref m_PostProcessPasses.m_ColorGradingLut, in descriptor7, filterMode, TextureWrapMode.Clamp, isShadowMap: false, 0, 0f, "_InternalGradingLut");
			colorGradingLutPass.Setup(colorGradingLut);
			EnqueuePass(colorGradingLutPass);
		}
		if (cameraData.xr.hasValidOcclusionMesh)
		{
			EnqueuePass(m_XROcclusionMeshPass);
		}
		bool resolveFinalTarget = cameraData.resolveFinalTarget;
		if (renderingModeActual == RenderingMode.Deferred)
		{
			if (m_DeferredLights.UseRenderPass && (RenderPassEvent.AfterRenderingGbuffer == renderPassInputs.requiresDepthNormalAtEvent || !useRenderPassEnabled))
			{
				m_DeferredLights.DisableFramebufferFetchInput();
			}
			EnqueueDeferred(ref renderingData, flag15, renderPassInputs.requiresNormalsTexture, flag3, flag11, flag12);
		}
		else
		{
			RenderBufferStoreAction storeAction = RenderBufferStoreAction.Store;
			if (descriptor.msaaSamples > 1)
			{
				storeAction = (flag19 ? RenderBufferStoreAction.StoreAndResolve : RenderBufferStoreAction.Store);
			}
			RenderBufferStoreAction renderBufferStoreAction = ((!(flag19 || flag21) && resolveFinalTarget) ? RenderBufferStoreAction.DontCare : RenderBufferStoreAction.Store);
			if (cameraData.xr.enabled && cameraData.xr.copyDepth)
			{
				renderBufferStoreAction = RenderBufferStoreAction.Store;
			}
			if (flag21 && descriptor.msaaSamples > 1 && RenderingUtils.MultisampleDepthResolveSupported() && m_CopyDepthPass.renderPassEvent == RenderPassEvent.AfterRenderingTransparents && !flag19)
			{
				switch (renderBufferStoreAction)
				{
				case RenderBufferStoreAction.Store:
					renderBufferStoreAction = RenderBufferStoreAction.StoreAndResolve;
					break;
				case RenderBufferStoreAction.DontCare:
					renderBufferStoreAction = RenderBufferStoreAction.Resolve;
					break;
				}
			}
			DrawObjectsPass drawObjectsPass = null;
			if (flag4)
			{
				drawObjectsPass = m_RenderOpaqueForwardWithRenderingLayersPass;
				m_RenderOpaqueForwardWithRenderingLayersPass.Setup(m_ActiveCameraColorAttachment, m_DecalLayersTexture, m_ActiveCameraDepthAttachment);
			}
			else
			{
				drawObjectsPass = m_RenderOpaqueForwardPass;
			}
			drawObjectsPass.ConfigureColorStoreAction(storeAction);
			drawObjectsPass.ConfigureDepthStoreAction(renderBufferStoreAction);
			ClearFlag clearFlag = ((base.activeRenderPassQueue.Find((ScriptableRenderPass x) => x.renderPassEvent <= RenderPassEvent.BeforeRenderingOpaques && !x.overrideCameraTarget) == null && cameraData.renderType == CameraRenderType.Base) ? ClearFlag.Color : ClearFlag.None);
			if (SystemInfo.usesLoadStoreActions)
			{
				drawObjectsPass.ConfigureClear(clearFlag, Color.black);
			}
			EnqueuePass(drawObjectsPass);
		}
		if (camera.clearFlags == CameraClearFlags.Skybox && cameraData.renderType != CameraRenderType.Overlay && (RenderSettings.skybox != null || (camera.TryGetComponent<Skybox>(out var component2) && component2.material != null)))
		{
			EnqueuePass(m_DrawSkyboxPass);
		}
		if (flag21 && (renderingModeActual != RenderingMode.Deferred || !useRenderPassEnabled || renderPassInputs.requiresDepthTexture))
		{
			m_CopyDepthPass.Setup(m_ActiveCameraDepthAttachment, m_DepthTexture);
			EnqueuePass(m_CopyDepthPass);
		}
		if (cameraData.renderType == CameraRenderType.Base && !flag15 && !flag21)
		{
			Shader.SetGlobalTexture("_CameraDepthTexture", SystemInfo.usesReversedZBuffer ? Texture2D.blackTexture : Texture2D.whiteTexture);
		}
		if (flag19)
		{
			Downsampling opaqueDownsampling = UniversalRenderPipeline.asset.opaqueDownsampling;
			RenderTextureDescriptor descriptor8 = descriptor;
			CopyColorPass.ConfigureDescriptor(opaqueDownsampling, ref descriptor8, out var filterMode2);
			RenderingUtils.ReAllocateIfNeeded(ref m_OpaqueColor, in descriptor8, filterMode2, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraOpaqueTexture");
			m_CopyColorPass.Setup(m_ActiveCameraColorAttachment, m_OpaqueColor, opaqueDownsampling);
			EnqueuePass(m_CopyColorPass);
		}
		if (renderPassInputs.requiresMotionVectors)
		{
			RenderTextureDescriptor descriptor9 = descriptor;
			descriptor9.graphicsFormat = GraphicsFormat.R16G16_SFloat;
			descriptor9.depthBufferBits = 0;
			descriptor9.msaaSamples = 1;
			RenderingUtils.ReAllocateIfNeeded(ref m_MotionVectorColor, in descriptor9, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_MotionVectorTexture");
			RenderTextureDescriptor descriptor10 = descriptor;
			descriptor10.graphicsFormat = GraphicsFormat.None;
			descriptor10.msaaSamples = 1;
			RenderingUtils.ReAllocateIfNeeded(ref m_MotionVectorDepth, in descriptor10, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_MotionVectorDepthTexture");
			m_MotionVectorPass.Setup(m_MotionVectorColor, m_MotionVectorDepth);
			EnqueuePass(m_MotionVectorPass);
		}
		if (flag13)
		{
			EnqueuePass(m_TransparentSettingsPass);
		}
		RenderBufferStoreAction storeAction2 = ((descriptor.msaaSamples > 1 && resolveFinalTarget) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.Store);
		RenderBufferStoreAction storeAction3 = (resolveFinalTarget ? RenderBufferStoreAction.DontCare : RenderBufferStoreAction.Store);
		if (flag21 && m_CopyDepthPass.renderPassEvent >= RenderPassEvent.AfterRenderingTransparents)
		{
			storeAction3 = RenderBufferStoreAction.Store;
			if (descriptor.msaaSamples > 1 && RenderingUtils.MultisampleDepthResolveSupported())
			{
				storeAction3 = RenderBufferStoreAction.Resolve;
			}
		}
		m_RenderTransparentForwardPass.ConfigureColorStoreAction(storeAction2);
		m_RenderTransparentForwardPass.ConfigureDepthStoreAction(storeAction3);
		EnqueuePass(m_RenderTransparentForwardPass);
		EnqueuePass(m_OnRenderObjectCallbackPass);
		bool rendersOverlayUI = cameraData.rendersOverlayUI;
		bool isHDROutputActive = cameraData.isHDROutputActive;
		if (rendersOverlayUI && isHDROutputActive)
		{
			m_DrawOffscreenUIPass.Setup(ref cameraData, 32);
			EnqueuePass(m_DrawOffscreenUIPass);
		}
		bool flag22 = renderingData.cameraData.captureActions != null && resolveFinalTarget;
		bool flag23 = flag6 && resolveFinalTarget && (renderingData.cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing || (renderingData.cameraData.imageScalingMode == ImageScalingMode.Upscaling && renderingData.cameraData.upscalingFilter != ImageUpscalingFilter.Linear) || (renderingData.cameraData.IsTemporalAAEnabled() && renderingData.cameraData.taaSettings.contrastAdaptiveSharpening > 0f));
		bool flag24 = !flag22 && !flag20 && !flag23;
		bool flag25 = base.DebugHandler == null || !base.DebugHandler.HDRDebugViewIsActive(ref cameraData);
		if (flag5)
		{
			RenderTextureDescriptor descriptor11 = PostProcessPass.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
			RenderingUtils.ReAllocateIfNeeded(ref m_PostProcessPasses.m_AfterPostProcessColor, in descriptor11, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_AfterPostProcessTexture");
		}
		if (resolveFinalTarget)
		{
			SetupFinalPassDebug(ref cameraData);
			if (flag5)
			{
				bool enableColorEncoding = flag24 && flag25;
				postProcessPass.Setup(in descriptor, in m_ActiveCameraColorAttachment, flag24, in m_ActiveCameraDepthAttachment, colorGradingLut, in m_MotionVectorColor, flag23, enableColorEncoding);
				EnqueuePass(postProcessPass);
			}
			RTHandle source = m_ActiveCameraColorAttachment;
			if (flag23)
			{
				finalPostProcessPass.SetupFinalPass(in source, useSwapBuffer: true, flag25);
				EnqueuePass(finalPostProcessPass);
			}
			if (renderingData.cameraData.captureActions != null)
			{
				EnqueuePass(m_CapturePass);
			}
			if (!flag23 && (!flag5 || flag20 || flag22) && !(m_ActiveCameraColorAttachment.nameID == m_XRTargetHandleAlias.nameID))
			{
				m_FinalBlitPass.Setup(descriptor, source);
				EnqueuePass(m_FinalBlitPass);
			}
			if (rendersOverlayUI && !isHDROutputActive)
			{
				EnqueuePass(m_DrawOverlayUIPass);
			}
			if (cameraData.xr.enabled && !(m_ActiveCameraDepthAttachment.nameID == cameraData.xr.renderTarget) && cameraData.xr.copyDepth)
			{
				m_XRCopyDepthPass.Setup(m_ActiveCameraDepthAttachment, m_XRTargetHandleAlias);
				m_XRCopyDepthPass.CopyToDepth = true;
				EnqueuePass(m_XRCopyDepthPass);
			}
		}
		else if (flag5)
		{
			postProcessPass.Setup(in descriptor, in m_ActiveCameraColorAttachment, resolveToScreen: false, in m_ActiveCameraDepthAttachment, colorGradingLut, in m_MotionVectorColor, hasFinalPass: false, enableColorEncoding: false);
			EnqueuePass(postProcessPass);
		}
	}

	public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_ForwardLights.Setup(context, ref renderingData);
		if (renderingModeActual == RenderingMode.Deferred)
		{
			m_DeferredLights.SetupLights(context, ref renderingData);
		}
	}

	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters, ref CameraData cameraData)
	{
		if (renderingModeActual == RenderingMode.ForwardPlus)
		{
			cullingParameters.cullingOptions |= CullingOptions.DisablePerObjectCulling;
		}
		bool num = !UniversalRenderPipeline.asset.supportsMainLightShadows && !UniversalRenderPipeline.asset.supportsAdditionalLightShadows;
		bool flag = Mathf.Approximately(cameraData.maxShadowDistance, 0f);
		if (num || flag)
		{
			cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
		}
		if (renderingModeActual == RenderingMode.Deferred)
		{
			cullingParameters.maximumVisibleLights = 65535;
		}
		else if (renderingModeActual == RenderingMode.ForwardPlus)
		{
			cullingParameters.maximumVisibleLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
			cullingParameters.reflectionProbeSortingCriteria = ReflectionProbeSortingCriteria.None;
		}
		else
		{
			cullingParameters.maximumVisibleLights = UniversalRenderPipeline.maxVisibleAdditionalLights + 1;
		}
		cullingParameters.shadowDistance = cameraData.maxShadowDistance;
		cullingParameters.conservativeEnclosingSphere = UniversalRenderPipeline.asset.conservativeEnclosingSphere;
		cullingParameters.numIterationsEnclosingSphere = UniversalRenderPipeline.asset.numIterationsEnclosingSphere;
	}

	public override void FinishRendering(CommandBuffer cmd)
	{
		m_ColorBufferSystem.Clear();
		m_ActiveCameraColorAttachment = null;
		m_ActiveCameraDepthAttachment = null;
	}

	private void EnqueueDeferred(ref RenderingData renderingData, bool hasDepthPrepass, bool hasNormalPrepass, bool hasRenderingLayerPrepass, bool applyMainShadow, bool applyAdditionalShadow)
	{
		m_DeferredLights.Setup(ref renderingData, applyAdditionalShadow ? m_AdditionalLightsShadowCasterPass : null, hasDepthPrepass, hasNormalPrepass, hasRenderingLayerPrepass, m_DepthTexture, m_ActiveCameraDepthAttachment, m_ActiveCameraColorAttachment);
		if (useRenderPassEnabled && m_DeferredLights.UseRenderPass)
		{
			m_GBufferPass.Configure(null, renderingData.cameraData.cameraTargetDescriptor);
			m_DeferredPass.Configure(null, renderingData.cameraData.cameraTargetDescriptor);
		}
		EnqueuePass(m_GBufferPass);
		if (!useRenderPassEnabled || !m_DeferredLights.UseRenderPass)
		{
			m_GBufferCopyDepthPass.Setup(m_CameraDepthAttachment, m_DepthTexture);
			EnqueuePass(m_GBufferCopyDepthPass);
		}
		EnqueuePass(m_DeferredPass);
		EnqueuePass(m_RenderOpaqueForwardOnlyPass);
	}

	private RenderPassInputSummary GetRenderPassInputs(ref RenderingData renderingData)
	{
		RenderPassEvent renderPassEvent = ((m_RenderingMode == RenderingMode.Deferred) ? RenderPassEvent.BeforeRenderingGbuffer : RenderPassEvent.BeforeRenderingOpaques);
		RenderPassInputSummary result = new RenderPassInputSummary
		{
			requiresDepthNormalAtEvent = RenderPassEvent.BeforeRenderingOpaques,
			requiresDepthTextureEarliestEvent = RenderPassEvent.BeforeRenderingPostProcessing
		};
		for (int i = 0; i < base.activeRenderPassQueue.Count; i++)
		{
			ScriptableRenderPass scriptableRenderPass = base.activeRenderPassQueue[i];
			bool flag = (scriptableRenderPass.input & ScriptableRenderPassInput.Depth) != 0;
			bool flag2 = (scriptableRenderPass.input & ScriptableRenderPassInput.Normal) != 0;
			bool flag3 = (scriptableRenderPass.input & ScriptableRenderPassInput.Color) != 0;
			bool flag4 = (scriptableRenderPass.input & ScriptableRenderPassInput.Motion) != 0;
			bool flag5 = scriptableRenderPass.renderPassEvent <= renderPassEvent;
			if (scriptableRenderPass is DBufferRenderPass)
			{
				result.requiresColorTextureCreated = true;
			}
			result.requiresDepthTexture |= flag;
			result.requiresDepthPrepass |= flag2 || (flag && flag5);
			result.requiresNormalsTexture |= flag2;
			result.requiresColorTexture |= flag3;
			result.requiresMotionVectors |= flag4;
			if (flag)
			{
				result.requiresDepthTextureEarliestEvent = (RenderPassEvent)Mathf.Min((int)scriptableRenderPass.renderPassEvent, (int)result.requiresDepthTextureEarliestEvent);
			}
			if (flag2 || flag)
			{
				result.requiresDepthNormalAtEvent = (RenderPassEvent)Mathf.Min((int)scriptableRenderPass.renderPassEvent, (int)result.requiresDepthNormalAtEvent);
			}
		}
		if (renderingData.cameraData.IsTemporalAAEnabled())
		{
			result.requiresMotionVectors = true;
		}
		if (result.requiresMotionVectors)
		{
			result.requiresDepthTexture = true;
			result.requiresDepthTextureEarliestEvent = (RenderPassEvent)Mathf.Min((int)m_MotionVectorPass.renderPassEvent, (int)result.requiresDepthTextureEarliestEvent);
		}
		return result;
	}

	private void CreateCameraRenderTarget(ScriptableRenderContext context, ref RenderTextureDescriptor descriptor, bool primedDepth, CommandBuffer cmd, ref CameraData cameraData)
	{
		using (new ProfilingScope(null, Profiling.createCameraRenderTarget))
		{
			if (m_ColorBufferSystem.PeekBackBuffer() == null || m_ColorBufferSystem.PeekBackBuffer().nameID != BuiltinRenderTextureType.CameraTarget)
			{
				m_ActiveCameraColorAttachment = m_ColorBufferSystem.GetBackBuffer(cmd);
				ConfigureCameraColorTarget(m_ActiveCameraColorAttachment);
				cmd.SetGlobalTexture("_CameraColorTexture", m_ActiveCameraColorAttachment.nameID);
				cmd.SetGlobalTexture("_AfterPostProcessTexture", m_ActiveCameraColorAttachment.nameID);
			}
			if (m_CameraDepthAttachment == null || m_CameraDepthAttachment.nameID != BuiltinRenderTextureType.CameraTarget)
			{
				RenderTextureDescriptor descriptor2 = descriptor;
				descriptor2.useMipMap = false;
				descriptor2.autoGenerateMips = false;
				descriptor2.bindMS = false;
				if (descriptor2.msaaSamples > 1 && SystemInfo.supportsMultisampledTextures != 0)
				{
					if (IsDepthPrimingEnabled(ref cameraData))
					{
						descriptor2.bindMS = true;
					}
					else
					{
						descriptor2.bindMS = !RenderingUtils.MultisampleDepthResolveSupported() || !SystemInfo.supportsMultisampleAutoResolve || m_CopyDepthMode != CopyDepthMode.AfterTransparents;
					}
				}
				if (IsGLESDevice())
				{
					descriptor2.bindMS = false;
				}
				descriptor2.graphicsFormat = GraphicsFormat.None;
				descriptor2.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
				RenderingUtils.ReAllocateIfNeeded(ref m_CameraDepthAttachment, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraDepthAttachment");
				cmd.SetGlobalTexture(m_CameraDepthAttachment.name, m_CameraDepthAttachment.nameID);
				descriptor.depthStencilFormat = descriptor2.depthStencilFormat;
				descriptor.depthBufferBits = descriptor2.depthBufferBits;
			}
		}
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}

	private bool PlatformRequiresExplicitMsaaResolve()
	{
		if (!SystemInfo.supportsMultisampleAutoResolve || !Application.isMobilePlatform)
		{
			return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal;
		}
		return false;
	}

	private bool RequiresIntermediateColorTexture(ref CameraData cameraData)
	{
		if (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget)
		{
			return true;
		}
		if (renderingModeActual == RenderingMode.Deferred)
		{
			return true;
		}
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		int msaaSamples = cameraTargetDescriptor.msaaSamples;
		bool flag = cameraData.imageScalingMode != ImageScalingMode.None;
		bool flag2 = cameraTargetDescriptor.dimension == TextureDimension.Tex2D;
		bool flag3 = msaaSamples > 1 && PlatformRequiresExplicitMsaaResolve();
		bool num = cameraData.targetTexture != null && !isSceneViewCamera;
		bool flag4 = cameraData.captureActions != null;
		if (cameraData.xr.enabled)
		{
			flag = false;
			flag2 = cameraData.xr.renderTargetDesc.dimension == cameraTargetDescriptor.dimension;
		}
		bool flag5 = (cameraData.postProcessEnabled && m_PostProcessPasses.isCreated) || cameraData.requiresOpaqueTexture || flag3 || !cameraData.isDefaultViewport;
		if (num)
		{
			return flag5;
		}
		if (!(flag5 || isSceneViewCamera || flag || cameraData.isHdrEnabled || !flag2 || flag4))
		{
			return cameraData.requireSrgbConversion;
		}
		return true;
	}

	private bool CanCopyDepth(ref CameraData cameraData)
	{
		bool num = cameraData.cameraTargetDescriptor.msaaSamples > 1;
		bool flag = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
		bool flag2 = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
		bool flag3 = !num && (flag2 || flag);
		bool flag4 = num && SystemInfo.supportsMultisampledTextures != 0;
		if (IsGLESDevice() && flag4)
		{
			return false;
		}
		return flag3 || flag4;
	}

	internal override void SwapColorBuffer(CommandBuffer cmd)
	{
		m_ColorBufferSystem.Swap();
		if (m_ActiveCameraDepthAttachment.nameID != BuiltinRenderTextureType.CameraTarget)
		{
			ConfigureCameraTarget(m_ColorBufferSystem.GetBackBuffer(cmd), m_ActiveCameraDepthAttachment);
		}
		else
		{
			ConfigureCameraColorTarget(m_ColorBufferSystem.GetBackBuffer(cmd));
		}
		m_ActiveCameraColorAttachment = m_ColorBufferSystem.GetBackBuffer(cmd);
		cmd.SetGlobalTexture("_CameraColorTexture", m_ActiveCameraColorAttachment.nameID);
		cmd.SetGlobalTexture("_AfterPostProcessTexture", m_ActiveCameraColorAttachment.nameID);
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

	private void CleanupRenderGraphResources()
	{
		m_RenderGraphCameraColorHandle?.Release();
		m_RenderGraphCameraDepthHandle?.Release();
	}

	internal static TextureHandle CreateRenderGraphTexture(RenderGraph renderGraph, RenderTextureDescriptor desc, string name, bool clear, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
	{
		TextureDesc desc2 = new TextureDesc(desc.width, desc.height);
		desc2.dimension = desc.dimension;
		desc2.clearBuffer = clear;
		desc2.bindTextureMS = desc.bindMS;
		desc2.colorFormat = desc.graphicsFormat;
		desc2.depthBufferBits = (DepthBits)desc.depthBufferBits;
		desc2.slices = desc.volumeDepth;
		desc2.msaaSamples = (MSAASamples)desc.msaaSamples;
		desc2.name = name;
		desc2.enableRandomWrite = false;
		desc2.filterMode = filterMode;
		desc2.wrapMode = wrapMode;
		return renderGraph.CreateTexture(in desc2);
	}

	private bool RequiresColorAndDepthTextures(out bool createColorTexture, out bool createDepthTexture, ref RenderingData renderingData, RenderPassInputSummary renderPassInputs)
	{
		bool isPreviewCamera = renderingData.cameraData.isPreviewCamera;
		bool flag = renderingData.cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || m_DepthPrimingMode == DepthPrimingMode.Forced;
		bool flag2 = false;
		bool flag3 = flag && !CanCopyDepth(ref renderingData.cameraData);
		flag3 |= renderingData.cameraData.isSceneViewCamera;
		flag3 = flag3 || flag2;
		flag3 = flag3 || isPreviewCamera;
		flag3 |= renderPassInputs.requiresDepthPrepass;
		flag3 |= renderPassInputs.requiresNormalsTexture;
		createColorTexture = base.rendererFeatures.Count != 0 && m_IntermediateTextureMode == IntermediateTextureMode.Always && !isPreviewCamera;
		createColorTexture |= RequiresIntermediateColorTexture(ref renderingData.cameraData);
		createColorTexture &= !isPreviewCamera;
		createDepthTexture = flag && !flag3;
		createDepthTexture |= !renderingData.cameraData.resolveFinalTarget;
		createDepthTexture |= renderingModeActual == RenderingMode.Deferred && !useRenderPassEnabled;
		createDepthTexture |= m_DepthPrimingMode == DepthPrimingMode.Forced;
		if (renderingData.cameraData.xr.enabled)
		{
			createColorTexture |= createDepthTexture;
		}
		bool flag4 = IsDepthPrimingEnabled(ref renderingData.cameraData);
		flag4 &= flag3 && (createDepthTexture | createColorTexture) && m_RenderingMode == RenderingMode.Forward && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth);
		if (useRenderPassEnabled || flag4)
		{
			createColorTexture |= createDepthTexture;
		}
		if (renderingData.cameraData.renderType == CameraRenderType.Base)
		{
			bool flag5 = renderingData.cameraData.camera.sceneViewFilterMode == Camera.SceneViewFilterMode.ShowFiltered;
			bool flag6 = (createColorTexture | createDepthTexture) && !flag5;
			createDepthTexture = flag6;
		}
		return createColorTexture | createDepthTexture;
	}

	private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		RenderTargetIdentifier rt = ((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
		if (cameraData.xr.enabled)
		{
			rt = cameraData.xr.renderTarget;
		}
		frameResources.backBufferColor = renderGraph.ImportBackbuffer(rt);
		RenderPassInputSummary renderPassInputs = GetRenderPassInputs(ref renderingData);
		bool createColorTexture = false;
		bool createDepthTexture = false;
		if (cameraData.renderType == CameraRenderType.Base)
		{
			m_UseIntermediateTexture = RequiresColorAndDepthTextures(out createColorTexture, out createDepthTexture, ref renderingData, renderPassInputs);
		}
		if (m_UseIntermediateTexture)
		{
			RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
			descriptor.useMipMap = false;
			descriptor.autoGenerateMips = false;
			descriptor.depthBufferBits = 0;
			RenderingUtils.ReAllocateIfNeeded(ref m_RenderGraphCameraColorHandle, in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraTargetAttachment");
		}
		if (m_UseIntermediateTexture)
		{
			RenderTextureDescriptor descriptor2 = cameraData.cameraTargetDescriptor;
			descriptor2.useMipMap = false;
			descriptor2.autoGenerateMips = false;
			descriptor2.bindMS = false;
			if (descriptor2.msaaSamples > 1 && SystemInfo.supportsMultisampledTextures != 0)
			{
				descriptor2.bindMS = true;
			}
			if (IsGLESDevice())
			{
				descriptor2.bindMS = false;
			}
			descriptor2.graphicsFormat = GraphicsFormat.None;
			descriptor2.depthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
			RenderingUtils.ReAllocateIfNeeded(ref m_RenderGraphCameraDepthHandle, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_CameraDepthAttachment");
		}
		if (m_UseIntermediateTexture)
		{
			frameResources.cameraDepth = renderGraph.ImportTexture(m_RenderGraphCameraDepthHandle);
			frameResources.cameraColor = renderGraph.ImportTexture(m_RenderGraphCameraColorHandle);
			if (frameResources.cameraColor.IsValid())
			{
				m_ActiveRenderGraphColor = frameResources.cameraColor;
				m_TargetIsBackbuffer = false;
			}
			if (frameResources.cameraDepth.IsValid())
			{
				m_ActiveRenderGraphDepth = frameResources.cameraDepth;
			}
		}
		else
		{
			m_ActiveRenderGraphColor = frameResources.backBufferColor;
			m_ActiveRenderGraphDepth = frameResources.backBufferColor;
			m_TargetIsBackbuffer = true;
		}
		SupportedRenderingFeatures.active.motionVectors = true;
		RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.graphicsFormat = GraphicsFormat.R16G16_SFloat;
		cameraTargetDescriptor.depthBufferBits = 0;
		cameraTargetDescriptor.msaaSamples = 1;
		frameResources.motionVectorColor = CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor, "_MotionVectorTexture", clear: true);
		RenderTextureDescriptor cameraTargetDescriptor2 = cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor2.graphicsFormat = GraphicsFormat.None;
		cameraTargetDescriptor2.depthBufferBits = ((cameraTargetDescriptor2.depthBufferBits != 0) ? cameraTargetDescriptor2.depthBufferBits : 32);
		cameraTargetDescriptor2.msaaSamples = 1;
		frameResources.motionVectorDepth = CreateRenderGraphTexture(renderGraph, cameraTargetDescriptor2, "_MotionVectorDepthTexture", clear: true);
	}

	internal override void OnRecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		useRenderPassEnabled = false;
		CreateRenderGraphCameraRenderTargets(renderGraph, context, ref renderingData);
		SetupRenderGraphCameraProperties(renderGraph, ref renderingData, m_TargetIsBackbuffer);
		OnBeforeRendering(renderGraph, context, ref renderingData);
		OnMainRendering(renderGraph, context, ref renderingData);
		OnAfterRendering(renderGraph, context, ref renderingData);
	}

	internal override void OnFinishRenderGraphRendering(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (renderingModeActual == RenderingMode.Deferred)
		{
			m_DeferredPass.OnCameraCleanup(renderingData.commandBuffer);
		}
		m_CopyDepthPass.OnCameraCleanup(renderingData.commandBuffer);
		m_DepthNormalPrepass.OnCameraCleanup(renderingData.commandBuffer);
	}

	private void OnBeforeRendering(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		bool flag = false;
		if (m_MainLightShadowCasterPass.Setup(ref renderingData))
		{
			flag = true;
			frameResources.mainShadowsTexture = m_MainLightShadowCasterPass.Render(renderGraph, ref renderingData);
		}
		if (m_AdditionalLightsShadowCasterPass.Setup(ref renderingData))
		{
			flag = true;
			frameResources.additionalShadowsTexture = m_AdditionalLightsShadowCasterPass.Render(renderGraph, ref renderingData);
		}
		if (flag)
		{
			SetupRenderGraphCameraProperties(renderGraph, ref renderingData, m_TargetIsBackbuffer);
		}
	}

	private void OnMainRendering(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		RTClearFlags rTClearFlags = RTClearFlags.None;
		if (renderingData.cameraData.renderType == CameraRenderType.Base)
		{
			rTClearFlags = RTClearFlags.All;
		}
		else if (renderingData.cameraData.clearDepth)
		{
			rTClearFlags = RTClearFlags.Depth;
		}
		if (rTClearFlags != RTClearFlags.None)
		{
			ClearTargetsPass.Render(renderGraph, this, rTClearFlags, renderingData.cameraData.backgroundColor);
		}
		RecordCustomRenderGraphPasses(renderGraph, context, ref renderingData, RenderPassEvent.BeforeRenderingPrePasses);
		m_DepthNormalPrepass.Render(renderGraph, out frameResources.cameraDepthTexture, out frameResources.cameraNormalsTexture, ref renderingData);
		m_DepthPrepass.Render(renderGraph, out frameResources.cameraDepthTexture, ref renderingData);
		if (m_PostProcessPasses.isCreated)
		{
			m_PostProcessPasses.colorGradingLutPass.Render(renderGraph, out frameResources.internalColorLut, ref renderingData);
		}
		if (renderingData.cameraData.xr.hasValidOcclusionMesh)
		{
			m_XROcclusionMeshPass.m_IsActiveTargetBackBuffer = m_TargetIsBackbuffer;
			m_XROcclusionMeshPass.Render(renderGraph, in frameResources.cameraColor, in frameResources.cameraDepth, ref renderingData);
		}
		if (renderingModeActual == RenderingMode.Deferred)
		{
			m_DeferredLights.Setup(m_AdditionalLightsShadowCasterPass);
			if (m_DeferredLights != null)
			{
				m_DeferredLights.UseRenderPass = false;
				m_DeferredLights.ResolveMixedLightingMode(ref renderingData);
				m_DeferredLights.IsOverlay = renderingData.cameraData.renderType == CameraRenderType.Overlay;
			}
			m_GBufferPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, ref renderingData, ref frameResources);
			m_GBufferCopyDepthPass.Render(renderGraph, out frameResources.cameraDepthTexture, in frameResources.cameraDepth, ref renderingData);
			m_DeferredPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, frameResources.gbuffer, ref renderingData);
			m_RenderOpaqueForwardOnlyPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, frameResources.mainShadowsTexture, frameResources.additionalShadowsTexture, ref renderingData);
		}
		else
		{
			m_RenderOpaqueForwardPass.m_IsActiveTargetBackBuffer = m_TargetIsBackbuffer;
			m_RenderOpaqueForwardPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, frameResources.mainShadowsTexture, frameResources.additionalShadowsTexture, ref renderingData);
		}
		if (renderingData.cameraData.renderType == CameraRenderType.Base)
		{
			m_DrawSkyboxPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, ref renderingData);
		}
		m_CopyDepthPass.Render(renderGraph, out frameResources.cameraDepthTexture, in m_ActiveRenderGraphDepth, ref renderingData);
		Downsampling opaqueDownsampling = UniversalRenderPipeline.asset.opaqueDownsampling;
		m_CopyColorPass.Render(renderGraph, out frameResources.cameraOpaqueTexture, in m_ActiveRenderGraphColor, opaqueDownsampling, ref renderingData);
		m_MotionVectorPass.Render(renderGraph, ref frameResources.cameraDepth, in frameResources.motionVectorColor, in frameResources.motionVectorDepth, ref renderingData);
		m_RenderTransparentForwardPass.m_ShouldTransparentsReceiveShadows = !m_TransparentSettingsPass.Setup();
		m_RenderTransparentForwardPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, frameResources.mainShadowsTexture, frameResources.additionalShadowsTexture, ref renderingData);
		m_OnRenderObjectCallbackPass.Render(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, ref renderingData);
	}

	private void OnAfterRendering(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		bool num = DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance.renderingSettings.sceneOverrideMode == DebugSceneOverrideMode.None;
		if (num)
		{
			DrawRenderGraphGizmos(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, GizmoSubset.PreImageEffects, ref renderingData);
		}
		if (num)
		{
			DrawRenderGraphGizmos(renderGraph, m_ActiveRenderGraphColor, m_ActiveRenderGraphDepth, GizmoSubset.PostImageEffects, ref renderingData);
		}
		if (!m_TargetIsBackbuffer && renderingData.cameraData.resolveFinalTarget)
		{
			m_FinalBlitPass.Render(renderGraph, ref renderingData, frameResources.cameraColor, frameResources.backBufferColor, frameResources.overlayUITexture);
		}
	}
}
