using System;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal class PostProcessPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		public readonly Material stopNaN;

		public readonly Material subpixelMorphologicalAntialiasing;

		public readonly Material gaussianDepthOfField;

		public readonly Material bokehDepthOfField;

		public readonly Material cameraMotionBlur;

		public readonly Material paniniProjection;

		public readonly Material bloom;

		public readonly Material temporalAntialiasing;

		public readonly Material scalingSetup;

		public readonly Material easu;

		public readonly Material uber;

		public readonly Material finalPass;

		public readonly Material lensFlareDataDriven;

		public MaterialLibrary(PostProcessData data)
		{
			stopNaN = Load(data.shaders.stopNanPS);
			subpixelMorphologicalAntialiasing = Load(data.shaders.subpixelMorphologicalAntialiasingPS);
			gaussianDepthOfField = Load(data.shaders.gaussianDepthOfFieldPS);
			bokehDepthOfField = Load(data.shaders.bokehDepthOfFieldPS);
			cameraMotionBlur = Load(data.shaders.cameraMotionBlurPS);
			paniniProjection = Load(data.shaders.paniniProjectionPS);
			bloom = Load(data.shaders.bloomPS);
			temporalAntialiasing = Load(data.shaders.temporalAntialiasingPS);
			scalingSetup = Load(data.shaders.scalingSetupPS);
			easu = Load(data.shaders.easuPS);
			uber = Load(data.shaders.uberPostPS);
			finalPass = Load(data.shaders.finalPostPassPS);
			lensFlareDataDriven = Load(data.shaders.LensFlareDataDrivenPS);
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogErrorFormat("Missing shader. PostProcessing render passes will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			if (!shader.isSupported)
			{
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}

		internal void Cleanup()
		{
			CoreUtils.Destroy(stopNaN);
			CoreUtils.Destroy(subpixelMorphologicalAntialiasing);
			CoreUtils.Destroy(gaussianDepthOfField);
			CoreUtils.Destroy(bokehDepthOfField);
			CoreUtils.Destroy(cameraMotionBlur);
			CoreUtils.Destroy(paniniProjection);
			CoreUtils.Destroy(bloom);
			CoreUtils.Destroy(temporalAntialiasing);
			CoreUtils.Destroy(scalingSetup);
			CoreUtils.Destroy(easu);
			CoreUtils.Destroy(uber);
			CoreUtils.Destroy(finalPass);
			CoreUtils.Destroy(lensFlareDataDriven);
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");

		public static readonly int _TempTarget2 = Shader.PropertyToID("_TempTarget2");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

		public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");

		public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");

		public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");

		public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");

		public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");

		public static readonly int _BokehConstants = Shader.PropertyToID("_BokehConstants");

		public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");

		public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");

		public static readonly int _Metrics = Shader.PropertyToID("_Metrics");

		public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");

		public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");

		public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");

		public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

		public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");

		public static readonly int _Params = Shader.PropertyToID("_Params");

		public static readonly int _SourceTexLowMip = Shader.PropertyToID("_SourceTexLowMip");

		public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");

		public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");

		public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");

		public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");

		public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");

		public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");

		public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");

		public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");

		public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");

		public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");

		public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");

		public static readonly int _Vignette_ParamsXR = Shader.PropertyToID("_Vignette_ParamsXR");

		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");

		public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");

		public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		public static readonly int _DownSampleScaleFactor = Shader.PropertyToID("_DownSampleScaleFactor");

		public static readonly int _FlareOcclusionRemapTex = Shader.PropertyToID("_FlareOcclusionRemapTex");

		public static readonly int _FlareOcclusionTex = Shader.PropertyToID("_FlareOcclusionTex");

		public static readonly int _FlareOcclusionIndex = Shader.PropertyToID("_FlareOcclusionIndex");

		public static readonly int _FlareTex = Shader.PropertyToID("_FlareTex");

		public static readonly int _FlareColorValue = Shader.PropertyToID("_FlareColorValue");

		public static readonly int _FlareData0 = Shader.PropertyToID("_FlareData0");

		public static readonly int _FlareData1 = Shader.PropertyToID("_FlareData1");

		public static readonly int _FlareData2 = Shader.PropertyToID("_FlareData2");

		public static readonly int _FlareData3 = Shader.PropertyToID("_FlareData3");

		public static readonly int _FlareData4 = Shader.PropertyToID("_FlareData4");

		public static readonly int _FlareData5 = Shader.PropertyToID("_FlareData5");

		public static readonly int _FullscreenProjMat = Shader.PropertyToID("_FullscreenProjMat");

		public static int[] _BloomMipUp;

		public static int[] _BloomMipDown;
	}

	private RenderTextureDescriptor m_Descriptor;

	private RTHandle m_Source;

	private RTHandle m_Destination;

	private RTHandle m_Depth;

	private RTHandle m_InternalLut;

	private RTHandle m_MotionVectors;

	private RTHandle m_FullCoCTexture;

	private RTHandle m_HalfCoCTexture;

	private RTHandle m_PingTexture;

	private RTHandle m_PongTexture;

	private RTHandle[] m_BloomMipDown;

	private RTHandle[] m_BloomMipUp;

	private RTHandle m_BlendTexture;

	private RTHandle m_EdgeColorTexture;

	private RTHandle m_EdgeStencilTexture;

	private RTHandle m_TempTarget;

	private RTHandle m_TempTarget2;

	private const string k_RenderPostProcessingTag = "Render PostProcessing Effects";

	private const string k_RenderFinalPostProcessingTag = "Render Final PostProcessing Pass";

	private static readonly ProfilingSampler m_ProfilingRenderPostProcessing = new ProfilingSampler("Render PostProcessing Effects");

	private static readonly ProfilingSampler m_ProfilingRenderFinalPostProcessing = new ProfilingSampler("Render Final PostProcessing Pass");

	private MaterialLibrary m_Materials;

	private PostProcessData m_Data;

	private DepthOfField m_DepthOfField;

	private MotionBlur m_MotionBlur;

	private PaniniProjection m_PaniniProjection;

	private Bloom m_Bloom;

	private LensDistortion m_LensDistortion;

	private ChromaticAberration m_ChromaticAberration;

	private Vignette m_Vignette;

	private ColorLookup m_ColorLookup;

	private ColorAdjustments m_ColorAdjustments;

	private Tonemapping m_Tonemapping;

	private FilmGrain m_FilmGrain;

	private const int k_MaxPyramidSize = 16;

	private readonly GraphicsFormat m_DefaultHDRFormat;

	private bool m_UseRGBM;

	private readonly GraphicsFormat m_SMAAEdgeFormat;

	private readonly GraphicsFormat m_GaussianCoCFormat;

	private int m_DitheringTextureIndex;

	private RenderTargetIdentifier[] m_MRT2;

	private Vector4[] m_BokehKernel;

	private int m_BokehHash;

	private float m_BokehMaxRadius;

	private float m_BokehRCPAspect;

	private bool m_IsFinalPass;

	private bool m_HasFinalPass;

	private bool m_EnableColorEncodingIfNeeded;

	private bool m_UseFastSRGBLinearConversion;

	private bool m_SupportDataDrivenLensFlare;

	private bool m_ResolveToScreen;

	private bool m_UseSwapBuffer;

	private RTHandle m_ScalingSetupTarget;

	private RTHandle m_UpscaledTarget;

	private Material m_BlitMaterial;

	internal static readonly int k_ShaderPropertyId_ViewProjM = Shader.PropertyToID("_ViewProjM");

	internal static readonly int k_ShaderPropertyId_PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");

	internal static readonly int k_ShaderPropertyId_ViewProjMStereo = Shader.PropertyToID("_ViewProjMStereo");

	internal static readonly int k_ShaderPropertyId_PrevViewProjMStereo = Shader.PropertyToID("_PrevViewProjMStereo");

	public PostProcessPass(RenderPassEvent evt, PostProcessData data, ref PostProcessParams postProcessParams)
	{
		base.profilingSampler = new ProfilingSampler("PostProcessPass");
		base.renderPassEvent = evt;
		m_Data = data;
		m_Materials = new MaterialLibrary(data);
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, FormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
		}
		else
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, FormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, FormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
		}
		else
		{
			m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;
		}
		ShaderConstants._BloomMipUp = new int[16];
		ShaderConstants._BloomMipDown = new int[16];
		m_BloomMipUp = new RTHandle[16];
		m_BloomMipDown = new RTHandle[16];
		for (int i = 0; i < 16; i++)
		{
			ShaderConstants._BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
			ShaderConstants._BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
			m_BloomMipUp[i] = RTHandles.Alloc(ShaderConstants._BloomMipUp[i], "_BloomMipUp" + i);
			m_BloomMipDown[i] = RTHandles.Alloc(ShaderConstants._BloomMipDown[i], "_BloomMipDown" + i);
		}
		m_MRT2 = new RenderTargetIdentifier[2];
		base.useNativeRenderPass = false;
		m_BlitMaterial = postProcessParams.blitMaterial;
		if (SystemInfo.IsFormatSupported(postProcessParams.requestHDRFormat, FormatUsage.Blend))
		{
			m_DefaultHDRFormat = postProcessParams.requestHDRFormat;
			m_UseRGBM = false;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
		{
			m_DefaultHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
			m_UseRGBM = false;
		}
		else
		{
			m_DefaultHDRFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			m_UseRGBM = true;
		}
	}

	public void Cleanup()
	{
		m_Materials.Cleanup();
	}

	public void Dispose()
	{
		RTHandle[] bloomMipDown = m_BloomMipDown;
		for (int i = 0; i < bloomMipDown.Length; i++)
		{
			bloomMipDown[i]?.Release();
		}
		bloomMipDown = m_BloomMipUp;
		for (int i = 0; i < bloomMipDown.Length; i++)
		{
			bloomMipDown[i]?.Release();
		}
		m_ScalingSetupTarget?.Release();
		m_UpscaledTarget?.Release();
		m_FullCoCTexture?.Release();
		m_HalfCoCTexture?.Release();
		m_PingTexture?.Release();
		m_PongTexture?.Release();
		m_BlendTexture?.Release();
		m_EdgeColorTexture?.Release();
		m_EdgeStencilTexture?.Release();
		m_TempTarget?.Release();
		m_TempTarget2?.Release();
	}

	public void Setup(in RenderTextureDescriptor baseDescriptor, in RTHandle source, bool resolveToScreen, in RTHandle depth, in RTHandle internalLut, in RTHandle motionVectors, bool hasFinalPass, bool enableColorEncoding)
	{
		m_Descriptor = baseDescriptor;
		m_Descriptor.useMipMap = false;
		m_Descriptor.autoGenerateMips = false;
		m_Source = source;
		m_Depth = depth;
		m_InternalLut = internalLut;
		m_MotionVectors = motionVectors;
		m_IsFinalPass = false;
		m_HasFinalPass = hasFinalPass;
		m_EnableColorEncodingIfNeeded = enableColorEncoding;
		m_ResolveToScreen = resolveToScreen;
		m_Destination = ScriptableRenderPass.k_CameraTarget;
		m_UseSwapBuffer = true;
	}

	public void Setup(in RenderTextureDescriptor baseDescriptor, in RTHandle source, RTHandle destination, in RTHandle depth, in RTHandle internalLut, bool hasFinalPass, bool enableColorEncoding)
	{
		m_Descriptor = baseDescriptor;
		m_Descriptor.useMipMap = false;
		m_Descriptor.autoGenerateMips = false;
		m_Source = source;
		m_Destination = destination;
		m_Depth = depth;
		m_InternalLut = internalLut;
		m_IsFinalPass = false;
		m_HasFinalPass = hasFinalPass;
		m_EnableColorEncodingIfNeeded = enableColorEncoding;
		m_UseSwapBuffer = true;
	}

	public void SetupFinalPass(in RTHandle source, bool useSwapBuffer = false, bool enableColorEncoding = true)
	{
		m_Source = source;
		m_Destination = ScriptableRenderPass.k_CameraTarget;
		m_IsFinalPass = true;
		m_HasFinalPass = false;
		m_EnableColorEncodingIfNeeded = enableColorEncoding;
		m_UseSwapBuffer = useSwapBuffer;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		base.overrideCameraTarget = true;
	}

	public bool CanRunOnTile()
	{
		return false;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		VolumeStack stack = VolumeManager.instance.stack;
		m_DepthOfField = stack.GetComponent<DepthOfField>();
		m_MotionBlur = stack.GetComponent<MotionBlur>();
		m_PaniniProjection = stack.GetComponent<PaniniProjection>();
		m_Bloom = stack.GetComponent<Bloom>();
		m_LensDistortion = stack.GetComponent<LensDistortion>();
		m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
		m_Vignette = stack.GetComponent<Vignette>();
		m_ColorLookup = stack.GetComponent<ColorLookup>();
		m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
		m_Tonemapping = stack.GetComponent<Tonemapping>();
		m_FilmGrain = stack.GetComponent<FilmGrain>();
		m_UseFastSRGBLinearConversion = renderingData.postProcessingData.useFastSRGBLinearConversion;
		m_SupportDataDrivenLensFlare = renderingData.postProcessingData.supportDataDrivenLensFlare;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		if (m_IsFinalPass)
		{
			using (new ProfilingScope(commandBuffer, m_ProfilingRenderFinalPostProcessing))
			{
				RenderFinalPass(commandBuffer, ref renderingData);
				return;
			}
		}
		if (!CanRunOnTile())
		{
			using (new ProfilingScope(commandBuffer, m_ProfilingRenderPostProcessing))
			{
				Render(commandBuffer, ref renderingData);
			}
		}
	}

	private RenderTextureDescriptor GetCompatibleDescriptor()
	{
		return GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat);
	}

	private RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
	{
		return GetCompatibleDescriptor(m_Descriptor, width, height, format, depthBufferBits);
	}

	internal static RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor desc, int width, int height, GraphicsFormat format, DepthBits depthBufferBits = DepthBits.None)
	{
		desc.depthBufferBits = (int)depthBufferBits;
		desc.msaaSamples = 1;
		desc.width = width;
		desc.height = height;
		desc.graphicsFormat = format;
		return desc;
	}

	private bool RequireSRGBConversionBlitToBackBuffer(ref CameraData cameraData)
	{
		if (cameraData.requireSrgbConversion)
		{
			return m_EnableColorEncodingIfNeeded;
		}
		return false;
	}

	private bool RequireHDROutput(ref CameraData cameraData)
	{
		if (cameraData.isHDROutputActive)
		{
			return cameraData.captureActions == null;
		}
		return false;
	}

	private void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		ref ScriptableRenderer renderer = ref cameraData.renderer;
		bool isSceneViewCamera = cameraData.isSceneViewCamera;
		bool flag = cameraData.isStopNaNEnabled && m_Materials.stopNaN != null;
		bool flag2 = cameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
		Material material = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? m_Materials.gaussianDepthOfField : m_Materials.bokehDepthOfField);
		bool flag3 = m_DepthOfField.IsActive() && !isSceneViewCamera && material != null;
		bool flag4 = !LensFlareCommonSRP.Instance.IsEmpty() && m_SupportDataDrivenLensFlare;
		bool flag5 = m_MotionBlur.IsActive() && !isSceneViewCamera;
		bool flag6 = m_PaniniProjection.IsActive() && !isSceneViewCamera;
		flag5 = flag5 && Application.isPlaying;
		bool flag7 = cameraData.IsTemporalAAEnabled();
		if (cameraData.antialiasing == AntialiasingMode.TemporalAntiAliasing && !flag7)
		{
			TemporalAA.ValidateAndWarn(ref cameraData);
		}
		int amountOfPassesRemaining = (flag ? 1 : 0) + (flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0) + (flag7 ? 1 : 0) + (flag5 ? 1 : 0) + (flag6 ? 1 : 0);
		if (m_UseSwapBuffer && amountOfPassesRemaining > 0)
		{
			renderer.EnableSwapBufferMSAA(enable: false);
		}
		RTHandle source = (m_UseSwapBuffer ? renderer.cameraColorTargetHandle : m_Source);
		RTHandle destination = (m_UseSwapBuffer ? renderer.GetCameraColorFrontBuffer(cmd) : null);
		cmd.SetGlobalMatrix(ShaderConstants._FullscreenProjMat, GL.GetGPUProjectionMatrix(Matrix4x4.identity, renderIntoTexture: true));
		if (flag)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.StopNaNs)))
			{
				Blitter.BlitCameraTexture(cmd, GetSource(), GetDestination(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, m_Materials.stopNaN, 0);
				Swap(ref renderer);
			}
		}
		if (flag2)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.SMAA)))
			{
				DoSubpixelMorphologicalAntialiasing(ref cameraData, cmd, GetSource(), GetDestination());
				Swap(ref renderer);
			}
		}
		if (flag3)
		{
			URPProfileId marker = ((m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian) ? URPProfileId.GaussianDepthOfField : URPProfileId.BokehDepthOfField);
			using (new ProfilingScope(cmd, ProfilingSampler.Get(marker)))
			{
				DoDepthOfField(cameraData.camera, cmd, GetSource(), GetDestination(), cameraData.pixelRect);
				Swap(ref renderer);
			}
		}
		if (flag7)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.TemporalAA)))
			{
				TemporalAA.ExecutePass(cmd, m_Materials.temporalAntialiasing, ref cameraData, source, destination, m_MotionVectors.rt);
				Swap(ref renderer);
			}
		}
		if (flag5)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.MotionBlur)))
			{
				DoMotionBlur(cmd, GetSource(), GetDestination(), ref cameraData);
				Swap(ref renderer);
			}
		}
		if (flag6)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.PaniniProjection)))
			{
				DoPaniniProjection(cameraData.camera, cmd, GetSource(), GetDestination());
				Swap(ref renderer);
			}
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UberPostProcess)))
		{
			m_Materials.uber.shaderKeywords = null;
			if (m_Bloom.IsActive())
			{
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.Bloom)))
				{
					SetupBloom(cmd, GetSource(), m_Materials.uber);
				}
			}
			if (flag4)
			{
				bool usePanini;
				float paniniDistance;
				float paniniCropToFit;
				if (m_PaniniProjection.IsActive())
				{
					usePanini = true;
					paniniDistance = m_PaniniProjection.distance.value;
					paniniCropToFit = m_PaniniProjection.cropToFit.value;
				}
				else
				{
					usePanini = false;
					paniniDistance = 1f;
					paniniCropToFit = 1f;
				}
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.LensFlareDataDrivenComputeOcclusion)))
				{
					LensFlareDataDrivenComputeOcclusion(cameraData.camera, cmd, GetSource(), usePanini, paniniDistance, paniniCropToFit);
				}
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.LensFlareDataDriven)))
				{
					LensFlareDataDriven(cameraData.camera, cmd, GetSource(), usePanini, paniniDistance, paniniCropToFit);
				}
			}
			SetupLensDistortion(m_Materials.uber, isSceneViewCamera);
			SetupChromaticAberration(m_Materials.uber);
			SetupVignette(m_Materials.uber, cameraData.xr);
			SetupColorGrading(cmd, ref renderingData, m_Materials.uber);
			SetupGrain(ref cameraData, m_Materials.uber);
			SetupDithering(ref cameraData, m_Materials.uber);
			if (RequireSRGBConversionBlitToBackBuffer(ref cameraData))
			{
				m_Materials.uber.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
			}
			if (RequireHDROutput(ref cameraData))
			{
				HDROutputUtils.Operation hdrOperations = ((!m_HasFinalPass && m_EnableColorEncodingIfNeeded) ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
				SetupHDROutput(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, m_Materials.uber, hdrOperations);
			}
			if (m_UseFastSRGBLinearConversion)
			{
				m_Materials.uber.EnableKeyword("_USE_FAST_SRGB_LINEAR_CONVERSION");
			}
			DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
			bool flag8 = activeDebugHandler?.WriteToDebugScreenTexture(ref cameraData) ?? false;
			RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
			if (m_Destination == ScriptableRenderPass.k_CameraTarget && !cameraData.isDefaultViewport)
			{
				loadAction = RenderBufferLoadAction.Load;
			}
			RenderTargetIdentifier renderTargetIdentifier = BuiltinRenderTextureType.CameraTarget;
			if (cameraData.xr.enabled)
			{
				renderTargetIdentifier = cameraData.xr.renderTarget;
			}
			if (!m_UseSwapBuffer)
			{
				m_ResolveToScreen = cameraData.resolveFinalTarget || m_Destination.nameID == renderTargetIdentifier || m_HasFinalPass;
			}
			if (m_UseSwapBuffer && !m_ResolveToScreen)
			{
				if (!m_HasFinalPass)
				{
					renderer.EnableSwapBufferMSAA(enable: true);
					destination = renderer.GetCameraColorFrontBuffer(cmd);
				}
				Blitter.BlitCameraTexture(cmd, GetSource(), destination, loadAction, RenderBufferStoreAction.Store, m_Materials.uber, 0);
				renderer.ConfigureCameraColorTarget(destination);
				Swap(ref renderer);
			}
			else if (!m_UseSwapBuffer)
			{
				RTHandle source2 = GetSource();
				Blitter.BlitCameraTexture(cmd, source2, GetDestination(), loadAction, RenderBufferStoreAction.Store, m_Materials.uber, 0);
				CommandBuffer cmd2 = cmd;
				RTHandle source3 = GetDestination();
				RTHandle destination2 = m_Destination;
				Material blitMaterial = m_BlitMaterial;
				RenderTexture rt = m_Destination.rt;
				Blitter.BlitCameraTexture(cmd2, source3, destination2, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, blitMaterial, ((object)rt != null && rt.filterMode == FilterMode.Bilinear) ? 1 : 0);
			}
			else if (m_ResolveToScreen)
			{
				if (flag8)
				{
					Blitter.BlitCameraTexture(cmd, GetSource(), activeDebugHandler.DebugScreenColorHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, m_Materials.uber, 0);
					renderer.ConfigureCameraTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
					return;
				}
				RTHandleStaticHelpers.SetRTHandleStaticWrapper((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : renderTargetIdentifier);
				RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
				RenderingUtils.FinalBlit(cmd, ref cameraData, GetSource(), s_RTHandleWrapper, loadAction, RenderBufferStoreAction.Store, m_Materials.uber, 0);
				renderer.ConfigureCameraColorTarget(s_RTHandleWrapper);
			}
		}
		RTHandle GetDestination()
		{
			if (destination == null)
			{
				RenderingUtils.ReAllocateIfNeeded(ref m_TempTarget, GetCompatibleDescriptor(), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_TempTarget");
				destination = m_TempTarget;
			}
			else if (destination == m_Source && m_Descriptor.msaaSamples > 1)
			{
				RenderingUtils.ReAllocateIfNeeded(ref m_TempTarget2, GetCompatibleDescriptor(), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_TempTarget2");
				destination = m_TempTarget2;
			}
			return destination;
		}
		RTHandle GetSource()
		{
			return source;
		}
		void Swap(ref ScriptableRenderer r)
		{
			int num = amountOfPassesRemaining - 1;
			amountOfPassesRemaining = num;
			if (m_UseSwapBuffer)
			{
				r.SwapColorBuffer(cmd);
				source = r.cameraColorTargetHandle;
				if (amountOfPassesRemaining == 0 && !m_HasFinalPass)
				{
					r.EnableSwapBufferMSAA(enable: true);
				}
				destination = r.GetCameraColorFrontBuffer(cmd);
			}
			else
			{
				CoreUtils.Swap(ref source, ref destination);
			}
		}
	}

	private void DoSubpixelMorphologicalAntialiasing(ref CameraData cameraData, CommandBuffer cmd, RTHandle source, RTHandle destination)
	{
		Rect viewport = new Rect(Vector2.zero, new Vector2(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height));
		Material subpixelMorphologicalAntialiasing = m_Materials.subpixelMorphologicalAntialiasing;
		RTHandle destinationDepthStencil;
		if (m_Depth.nameID == BuiltinRenderTextureType.CameraTarget || m_Descriptor.msaaSamples > 1)
		{
			RenderingUtils.ReAllocateIfNeeded(ref m_EdgeStencilTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.None, DepthBits.Depth24), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_EdgeStencilTexture");
			destinationDepthStencil = m_EdgeStencilTexture;
		}
		else
		{
			destinationDepthStencil = m_Depth;
		}
		RenderingUtils.ReAllocateIfNeeded(ref m_EdgeColorTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_SMAAEdgeFormat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_EdgeColorTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_BlendTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8G8B8A8_UNorm), FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_BlendTexture");
		Vector2Int vector2Int = (m_EdgeColorTexture.useScaling ? m_EdgeColorTexture.rtHandleProperties.currentRenderTargetSize : new Vector2Int(m_EdgeColorTexture.rt.width, m_EdgeColorTexture.rt.height));
		subpixelMorphologicalAntialiasing.SetVector(ShaderConstants._Metrics, new Vector4(1f / (float)vector2Int.x, 1f / (float)vector2Int.y, vector2Int.x, vector2Int.y));
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._AreaTexture, m_Data.textures.smaaAreaTex);
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._SearchTexture, m_Data.textures.smaaSearchTex);
		subpixelMorphologicalAntialiasing.SetFloat(ShaderConstants._StencilRef, 64f);
		subpixelMorphologicalAntialiasing.SetFloat(ShaderConstants._StencilMask, 64f);
		subpixelMorphologicalAntialiasing.shaderKeywords = null;
		switch (cameraData.antialiasingQuality)
		{
		case AntialiasingQuality.Low:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_LOW");
			break;
		case AntialiasingQuality.Medium:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_MEDIUM");
			break;
		case AntialiasingQuality.High:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_HIGH");
			break;
		}
		RenderingUtils.Blit(cmd, source, viewport, m_EdgeColorTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, destinationDepthStencil, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.ColorStencil, Color.clear, subpixelMorphologicalAntialiasing);
		RenderingUtils.Blit(cmd, m_EdgeColorTexture, viewport, m_BlendTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, ClearFlag.Color, Color.clear, subpixelMorphologicalAntialiasing, 1);
		cmd.SetGlobalTexture(ShaderConstants._BlendTexture, m_BlendTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, subpixelMorphologicalAntialiasing, 2);
	}

	private void DoDepthOfField(Camera camera, CommandBuffer cmd, RTHandle source, RTHandle destination, Rect pixelRect)
	{
		if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
		{
			DoGaussianDepthOfField(camera, cmd, source, destination, pixelRect);
		}
		else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
		{
			DoBokehDepthOfField(cmd, source, destination, pixelRect);
		}
	}

	private void DoGaussianDepthOfField(Camera camera, CommandBuffer cmd, RTHandle source, RTHandle destination, Rect pixelRect)
	{
		int num = 2;
		Material gaussianDepthOfField = m_Materials.gaussianDepthOfField;
		int num2 = m_Descriptor.width / num;
		int height = m_Descriptor.height / num;
		float value = m_DepthOfField.gaussianStart.value;
		float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
		float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num2 / 1080f);
		a = Mathf.Min(a, 2f);
		CoreUtils.SetKeyword(gaussianDepthOfField, "_HIGH_QUALITY_SAMPLING", m_DepthOfField.highQualitySampling.value);
		gaussianDepthOfField.SetVector(ShaderConstants._CoCParams, new Vector3(value, y, a));
		RenderingUtils.ReAllocateIfNeeded(ref m_FullCoCTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_FullCoCTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_HalfCoCTexture, GetCompatibleDescriptor(num2, height, m_GaussianCoCFormat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_HalfCoCTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_PingTexture, GetCompatibleDescriptor(num2, height, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_PingTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_PongTexture, GetCompatibleDescriptor(num2, height, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_PongTexture");
		PostProcessUtils.SetSourceSize(cmd, m_Descriptor);
		cmd.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)num, 1f / (float)num, num, num));
		Blitter.BlitCameraTexture(cmd, source, m_FullCoCTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 0);
		m_MRT2[0] = m_HalfCoCTexture.nameID;
		m_MRT2[1] = m_PingTexture.nameID;
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, m_FullCoCTexture.nameID);
		CoreUtils.SetRenderTarget(cmd, m_MRT2, m_HalfCoCTexture);
		Vector2 vector = (source.useScaling ? new Vector2(source.rtHandleProperties.rtHandleScale.x, source.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		Blitter.BlitTexture(cmd, source, vector, gaussianDepthOfField, 1);
		cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, m_HalfCoCTexture.nameID);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		Blitter.BlitCameraTexture(cmd, m_PingTexture, m_PongTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 2);
		Blitter.BlitCameraTexture(cmd, m_PongTexture, m_PingTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, m_PingTexture.nameID);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, m_FullCoCTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, gaussianDepthOfField, 4);
	}

	private void PrepareBokehKernel(float maxRadius, float rcpAspect)
	{
		if (m_BokehKernel == null)
		{
			m_BokehKernel = new Vector4[42];
		}
		int num = 0;
		float num2 = m_DepthOfField.bladeCount.value;
		float p = 1f - m_DepthOfField.bladeCurvature.value;
		float num3 = m_DepthOfField.bladeRotation.value * (MathF.PI / 180f);
		for (int i = 1; i < 4; i++)
		{
			float num4 = 1f / 7f;
			float num5 = ((float)i + num4) / (3f + num4);
			int num6 = i * 7;
			for (int j = 0; j < num6; j++)
			{
				float num7 = MathF.PI * 2f * (float)j / (float)num6;
				float num8 = Mathf.Cos(MathF.PI / num2);
				float num9 = Mathf.Cos(num7 - MathF.PI * 2f / num2 * Mathf.Floor((num2 * num7 + MathF.PI) / (MathF.PI * 2f)));
				float num10 = num5 * Mathf.Pow(num8 / num9, p);
				float num11 = num10 * Mathf.Cos(num7 - num3);
				float num12 = num10 * Mathf.Sin(num7 - num3);
				float num13 = num11 * maxRadius;
				float num14 = num12 * maxRadius;
				float num15 = num13 * num13;
				float num16 = num14 * num14;
				float z = Mathf.Sqrt(num15 + num16);
				float w = num13 * rcpAspect;
				m_BokehKernel[num] = new Vector4(num13, num14, z, w);
				num++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetMaxBokehRadiusInPixels(float viewportHeight)
	{
		return Mathf.Min(0.05f, 14f / viewportHeight);
	}

	private void DoBokehDepthOfField(CommandBuffer cmd, RTHandle source, RTHandle destination, Rect pixelRect)
	{
		int num = 2;
		Material bokehDepthOfField = m_Materials.bokehDepthOfField;
		int num2 = m_Descriptor.width / num;
		int num3 = m_Descriptor.height / num;
		float num4 = m_DepthOfField.focalLength.value / 1000f;
		float num5 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
		float value = m_DepthOfField.focusDistance.value;
		float y = num5 * num4 / (value - num4);
		float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(m_Descriptor.height);
		float num6 = 1f / ((float)num2 / (float)num3);
		CoreUtils.SetKeyword(bokehDepthOfField, "_USE_FAST_SRGB_LINEAR_CONVERSION", m_UseFastSRGBLinearConversion);
		cmd.SetGlobalVector(ShaderConstants._CoCParams, new Vector4(value, y, maxBokehRadiusInPixels, num6));
		int hashCode = m_DepthOfField.GetHashCode();
		if (hashCode != m_BokehHash || maxBokehRadiusInPixels != m_BokehMaxRadius || num6 != m_BokehRCPAspect)
		{
			m_BokehHash = hashCode;
			m_BokehMaxRadius = maxBokehRadiusInPixels;
			m_BokehRCPAspect = num6;
			PrepareBokehKernel(maxBokehRadiusInPixels, num6);
		}
		cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, m_BokehKernel);
		RenderingUtils.ReAllocateIfNeeded(ref m_FullCoCTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_FullCoCTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_PingTexture, GetCompatibleDescriptor(num2, num3, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_PingTexture");
		RenderingUtils.ReAllocateIfNeeded(ref m_PongTexture, GetCompatibleDescriptor(num2, num3, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_PongTexture");
		PostProcessUtils.SetSourceSize(cmd, m_Descriptor);
		cmd.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1f / (float)num, 1f / (float)num, num, num));
		float num7 = 1f / (float)m_Descriptor.height * (float)num;
		cmd.SetGlobalVector(ShaderConstants._BokehConstants, new Vector4(num7, num7 * 2f));
		Blitter.BlitCameraTexture(cmd, source, m_FullCoCTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 0);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, m_FullCoCTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, m_PingTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 1);
		Blitter.BlitCameraTexture(cmd, m_PingTexture, m_PongTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 2);
		Blitter.BlitCameraTexture(cmd, m_PongTexture, m_PingTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._DofTexture, m_PingTexture.nameID);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bokehDepthOfField, 4);
	}

	private static float GetLensFlareLightAttenuation(Light light, Camera cam, Vector3 wo)
	{
		if (light != null)
		{
			return light.type switch
			{
				LightType.Directional => LensFlareCommonSRP.ShapeAttenuationDirLight(light.transform.forward, wo), 
				LightType.Point => LensFlareCommonSRP.ShapeAttenuationPointLight(), 
				LightType.Spot => LensFlareCommonSRP.ShapeAttenuationSpotConeLight(light.transform.forward, wo, light.spotAngle, light.innerSpotAngle / 180f), 
				_ => 1f, 
			};
		}
		return 1f;
	}

	private void LensFlareDataDrivenComputeOcclusion(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, bool usePanini, float paniniDistance, float paniniCropToFit)
	{
		Matrix4x4 viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
		cmd.SetGlobalTexture(m_Depth.name, m_Depth.nameID);
		LensFlareCommonSRP.ComputeOcclusion(m_Materials.lensFlareDataDriven, camera, m_Descriptor.width, m_Descriptor.height, usePanini, paniniDistance, paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, cmd, taaEnabled: false, hasCloudLayer: false, null, null, ShaderConstants._FlareOcclusionTex, -1, ShaderConstants._FlareOcclusionIndex, ShaderConstants._FlareTex, ShaderConstants._FlareColorValue, -1, ShaderConstants._FlareData0, ShaderConstants._FlareData1, ShaderConstants._FlareData2, ShaderConstants._FlareData3, ShaderConstants._FlareData4);
	}

	private void LensFlareDataDriven(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, bool usePanini, float paniniDistance, float paniniCropToFit)
	{
		Matrix4x4 viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, renderIntoTexture: true) * camera.worldToCameraMatrix;
		LensFlareCommonSRP.DoLensFlareDataDrivenCommon(m_Materials.lensFlareDataDriven, camera, m_Descriptor.width, m_Descriptor.height, usePanini, paniniDistance, paniniCropToFit, isCameraRelative: true, camera.transform.position, viewProjMatrix, cmd, taaEnabled: false, hasCloudLayer: false, null, null, source, (Light light, Camera cam, Vector3 wo) => GetLensFlareLightAttenuation(light, cam, wo), ShaderConstants._FlareOcclusionRemapTex, ShaderConstants._FlareOcclusionTex, ShaderConstants._FlareOcclusionIndex, 0, 0, ShaderConstants._FlareTex, ShaderConstants._FlareColorValue, ShaderConstants._FlareData0, ShaderConstants._FlareData1, ShaderConstants._FlareData2, ShaderConstants._FlareData3, ShaderConstants._FlareData4, debugView: false);
	}

	internal static void UpdateMotionBlurMatrices(ref Material material, Camera camera, XRPass xr)
	{
		MotionVectorsPersistentData motionVectorsPersistentData = null;
		if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
		{
			motionVectorsPersistentData = component.motionVectorsPersistentData;
		}
		if (motionVectorsPersistentData == null)
		{
			return;
		}
		if (xr.enabled && xr.singlePassEnabled)
		{
			material.SetMatrixArray(k_ShaderPropertyId_PrevViewProjMStereo, motionVectorsPersistentData.previousViewProjectionStereo);
			material.SetMatrixArray(k_ShaderPropertyId_ViewProjMStereo, motionVectorsPersistentData.viewProjectionStereo);
			return;
		}
		int num = 0;
		if (xr.enabled)
		{
			num = xr.multipassId;
		}
		material.SetMatrix(k_ShaderPropertyId_PrevViewProjM, motionVectorsPersistentData.previousViewProjectionStereo[num]);
		material.SetMatrix(k_ShaderPropertyId_ViewProjM, motionVectorsPersistentData.viewProjectionStereo[num]);
	}

	private void DoMotionBlur(CommandBuffer cmd, RTHandle source, RTHandle destination, ref CameraData cameraData)
	{
		Material material = m_Materials.cameraMotionBlur;
		UpdateMotionBlurMatrices(ref material, cameraData.camera, cameraData.xr);
		material.SetFloat("_Intensity", m_MotionBlur.intensity.value);
		material.SetFloat("_Clamp", m_MotionBlur.clamp.value);
		PostProcessUtils.SetSourceSize(cmd, m_Descriptor);
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, material, (int)m_MotionBlur.quality.value);
	}

	private void DoPaniniProjection(Camera camera, CommandBuffer cmd, RTHandle source, RTHandle destination)
	{
		float value = m_PaniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera);
		Vector2 vector2 = CalcCropExtents(camera, value);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), m_PaniniProjection.cropToFit.value);
		Material paniniProjection = m_Materials.paniniProjection;
		paniniProjection.SetVector(ShaderConstants._Params, new Vector4(vector.x, vector.y, num, w));
		paniniProjection.EnableKeyword((1f - Mathf.Abs(num) > float.Epsilon) ? "_GENERIC" : "_UNIT_DISTANCE");
		Blitter.BlitCameraTexture(cmd, source, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, paniniProjection, 0);
	}

	private Vector2 CalcViewExtents(Camera camera)
	{
		float num = camera.fieldOfView * (MathF.PI / 180f);
		float num2 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private Vector2 CalcCropExtents(Camera camera, float d)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(camera);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}

	private void SetupBloom(CommandBuffer cmd, RTHandle source, Material uberMaterial)
	{
		int num = 1;
		num = m_Bloom.downscale.value switch
		{
			BloomDownscaleMode.Half => 1, 
			BloomDownscaleMode.Quarter => 2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		int num2 = m_Descriptor.width >> num;
		int num3 = m_Descriptor.height >> num;
		int num4 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num2, num3), 2f) - 1f), 1, m_Bloom.maxIterations.value);
		float value = m_Bloom.clamp.value;
		float num5 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
		float w = num5 * 0.5f;
		float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
		Material bloom = m_Materials.bloom;
		bloom.SetVector(ShaderConstants._Params, new Vector4(x, value, num5, w));
		CoreUtils.SetKeyword(bloom, "_BLOOM_HQ", m_Bloom.highQualityFiltering.value);
		CoreUtils.SetKeyword(bloom, "_USE_RGBM", m_UseRGBM);
		RenderTextureDescriptor descriptor = GetCompatibleDescriptor(num2, num3, m_DefaultHDRFormat);
		for (int i = 0; i < num4; i++)
		{
			RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipUp[i], in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, m_BloomMipUp[i].name);
			RenderingUtils.ReAllocateIfNeeded(ref m_BloomMipDown[i], in descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, m_BloomMipDown[i].name);
			descriptor.width = Mathf.Max(1, descriptor.width >> 1);
			descriptor.height = Mathf.Max(1, descriptor.height >> 1);
		}
		Blitter.BlitCameraTexture(cmd, source, m_BloomMipDown[0], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 0);
		RTHandle source2 = m_BloomMipDown[0];
		for (int j = 1; j < num4; j++)
		{
			Blitter.BlitCameraTexture(cmd, source2, m_BloomMipUp[j], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 1);
			Blitter.BlitCameraTexture(cmd, m_BloomMipUp[j], m_BloomMipDown[j], RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 2);
			source2 = m_BloomMipDown[j];
		}
		for (int num6 = num4 - 2; num6 >= 0; num6--)
		{
			RTHandle rTHandle = ((num6 == num4 - 2) ? m_BloomMipDown[num6 + 1] : m_BloomMipUp[num6 + 1]);
			RTHandle source3 = m_BloomMipDown[num6];
			RTHandle destination = m_BloomMipUp[num6];
			cmd.SetGlobalTexture(ShaderConstants._SourceTexLowMip, rTHandle);
			Blitter.BlitCameraTexture(cmd, source3, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, bloom, 3);
		}
		Color color = m_Bloom.tint.value.linear;
		float num7 = ColorUtils.Luminance(in color);
		color = ((num7 > 0f) ? (color * (1f / num7)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
		cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, m_BloomMipUp[0]);
		Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
		float num8 = (float)texture.width / (float)texture.height;
		float num9 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 value2 = new Vector4(1f, 1f, 0f, 0f);
		float value3 = m_Bloom.dirtIntensity.value;
		if (num8 > num9)
		{
			value2.x = num9 / num8;
			value2.z = (1f - value2.x) * 0.5f;
		}
		else if (num9 > num8)
		{
			value2.y = num8 / num9;
			value2.w = (1f - value2.y) * 0.5f;
		}
		uberMaterial.SetVector(ShaderConstants._LensDirt_Params, value2);
		uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, value3);
		uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, texture);
		if (m_Bloom.highQualityFiltering.value)
		{
			uberMaterial.EnableKeyword((value3 > 0f) ? "_BLOOM_HQ_DIRT" : "_BLOOM_HQ");
		}
		else
		{
			uberMaterial.EnableKeyword((value3 > 0f) ? "_BLOOM_LQ_DIRT" : "_BLOOM_LQ");
		}
	}

	private void SetupLensDistortion(Material material, bool isSceneView)
	{
		float b = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
		float num = MathF.PI / 180f * Mathf.Min(160f, b);
		float y = 2f * Mathf.Tan(num * 0.5f);
		Vector2 vector = m_LensDistortion.center.value * 2f - Vector2.one;
		Vector4 value = new Vector4(vector.x, vector.y, Mathf.Max(m_LensDistortion.xMultiplier.value, 0.0001f), Mathf.Max(m_LensDistortion.yMultiplier.value, 0.0001f));
		Vector4 value2 = new Vector4((m_LensDistortion.intensity.value >= 0f) ? num : (1f / num), y, 1f / m_LensDistortion.scale.value, m_LensDistortion.intensity.value * 100f);
		material.SetVector(ShaderConstants._Distortion_Params1, value);
		material.SetVector(ShaderConstants._Distortion_Params2, value2);
		if (m_LensDistortion.IsActive() && !isSceneView)
		{
			material.EnableKeyword("_DISTORTION");
		}
	}

	private void SetupChromaticAberration(Material material)
	{
		material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);
		if (m_ChromaticAberration.IsActive())
		{
			material.EnableKeyword("_CHROMATIC_ABERRATION");
		}
	}

	private void SetupVignette(Material material, XRPass xrPass)
	{
		Color value = m_Vignette.color.value;
		Vector2 center = m_Vignette.center.value;
		float num = (float)m_Descriptor.width / (float)m_Descriptor.height;
		if (xrPass != null && xrPass.enabled)
		{
			if (xrPass.singlePassEnabled)
			{
				material.SetVector(ShaderConstants._Vignette_ParamsXR, xrPass.ApplyXRViewCenterOffset(center));
			}
			else
			{
				center = xrPass.ApplyXRViewCenterOffset(center);
			}
		}
		Vector4 value2 = new Vector4(value.r, value.g, value.b, m_Vignette.rounded.value ? num : 1f);
		Vector4 value3 = new Vector4(center.x, center.y, m_Vignette.intensity.value * 3f, m_Vignette.smoothness.value * 5f);
		material.SetVector(ShaderConstants._Vignette_Params1, value2);
		material.SetVector(ShaderConstants._Vignette_Params2, value3);
	}

	private void SetupColorGrading(CommandBuffer cmd, ref RenderingData renderingData, Material material)
	{
		ref PostProcessingData postProcessingData = ref renderingData.postProcessingData;
		bool flag = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
		int lutSize = postProcessingData.lutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
		cmd.SetGlobalTexture(ShaderConstants._InternalLut, m_InternalLut.nameID);
		material.SetVector(ShaderConstants._Lut_Params, new Vector4(1f / (float)num, 1f / (float)lutSize, (float)lutSize - 1f, w));
		material.SetTexture(ShaderConstants._UserLut, m_ColorLookup.texture.value);
		material.SetVector(ShaderConstants._UserLut_Params, (!m_ColorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)m_ColorLookup.texture.value.width, 1f / (float)m_ColorLookup.texture.value.height, (float)m_ColorLookup.texture.value.height - 1f, m_ColorLookup.contribution.value));
		if (flag)
		{
			material.EnableKeyword("_HDR_GRADING");
			return;
		}
		switch (m_Tonemapping.mode.value)
		{
		case TonemappingMode.Neutral:
			material.EnableKeyword("_TONEMAP_NEUTRAL");
			break;
		case TonemappingMode.ACES:
			material.EnableKeyword("_TONEMAP_ACES");
			break;
		}
	}

	private void SetupGrain(ref CameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && m_FilmGrain.IsActive())
		{
			material.EnableKeyword("_FILM_GRAIN");
			PostProcessUtils.ConfigureFilmGrain(m_Data, m_FilmGrain, cameraData.pixelWidth, cameraData.pixelHeight, material);
		}
	}

	private void SetupDithering(ref CameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && cameraData.isDitheringEnabled)
		{
			material.EnableKeyword("_DITHERING");
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Data, m_DitheringTextureIndex, cameraData.pixelWidth, cameraData.pixelHeight, material);
		}
	}

	private void SetupHDROutput(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Material material, HDROutputUtils.Operation hdrOperations)
	{
		UniversalRenderPipeline.GetHDROutputLuminanceParameters(hdrDisplayInformation, hdrDisplayColorGamut, m_Tonemapping, out var hdrOutputParameters);
		material.SetVector(ShaderPropertyId.hdrOutputLuminanceParams, hdrOutputParameters);
		HDROutputUtils.ConfigureHDROutput(material, hdrDisplayColorGamut, hdrOperations);
	}

	private void RenderFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		Material finalPass = m_Materials.finalPass;
		finalPass.shaderKeywords = null;
		PostProcessUtils.SetSourceSize(cmd, cameraData.cameraTargetDescriptor);
		SetupGrain(ref cameraData, finalPass);
		SetupDithering(ref cameraData, finalPass);
		if (RequireSRGBConversionBlitToBackBuffer(ref cameraData))
		{
			finalPass.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		HDROutputUtils.Operation operation = HDROutputUtils.Operation.None;
		bool flag = RequireHDROutput(ref cameraData);
		if (flag)
		{
			operation = (m_EnableColorEncodingIfNeeded ? HDROutputUtils.Operation.ColorEncoding : HDROutputUtils.Operation.None);
			if (!cameraData.postProcessEnabled)
			{
				operation |= HDROutputUtils.Operation.ColorConversion;
			}
			SetupHDROutput(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, finalPass, operation);
		}
		DebugHandler activeDebugHandler = ScriptableRenderPass.GetActiveDebugHandler(ref renderingData);
		bool flag2 = activeDebugHandler?.WriteToDebugScreenTexture(ref cameraData) ?? false;
		if (m_UseSwapBuffer)
		{
			m_Source = cameraData.renderer.GetCameraColorBackBuffer(cmd);
		}
		RTHandle source = m_Source;
		RenderBufferLoadAction loadAction = (cameraData.isDefaultViewport ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		bool flag3 = cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing;
		bool flag4 = cameraData.imageScalingMode == ImageScalingMode.Upscaling && cameraData.upscalingFilter == ImageUpscalingFilter.FSR;
		bool flag5 = cameraData.IsTemporalAAEnabled() && cameraData.taaSettings.contrastAdaptiveSharpening > 0f && !flag4;
		if (cameraData.imageScalingMode != ImageScalingMode.None)
		{
			bool num = flag3 || flag4;
			RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
			descriptor.msaaSamples = 1;
			descriptor.depthBufferBits = 0;
			if (!flag)
			{
				descriptor.graphicsFormat = UniversalRenderPipeline.MakeUnormRenderTextureGraphicsFormat();
			}
			m_Materials.scalingSetup.shaderKeywords = null;
			if (num)
			{
				if (flag)
				{
					SetupHDROutput(cameraData.hdrDisplayInformation, cameraData.hdrDisplayColorGamut, m_Materials.scalingSetup, HDROutputUtils.Operation.None);
				}
				if (flag3 && flag4)
				{
					m_Materials.scalingSetup.EnableKeyword("_FXAA_AND_GAMMA_20");
				}
				else if (flag3)
				{
					m_Materials.scalingSetup.EnableKeyword("_FXAA");
				}
				else if (flag4)
				{
					m_Materials.scalingSetup.EnableKeyword("_GAMMA_20");
				}
				RenderingUtils.ReAllocateIfNeeded(ref m_ScalingSetupTarget, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_ScalingSetupTexture");
				Blitter.BlitCameraTexture(cmd, m_Source, m_ScalingSetupTarget, loadAction, RenderBufferStoreAction.Store, m_Materials.scalingSetup, 0);
				source = m_ScalingSetupTarget;
			}
			switch (cameraData.imageScalingMode)
			{
			case ImageScalingMode.Upscaling:
				switch (cameraData.upscalingFilter)
				{
				case ImageUpscalingFilter.Point:
					if (!flag5)
					{
						finalPass.EnableKeyword("_POINT_SAMPLING");
					}
					break;
				case ImageUpscalingFilter.FSR:
				{
					m_Materials.easu.shaderKeywords = null;
					RenderTextureDescriptor descriptor2 = descriptor;
					descriptor2.width = cameraData.pixelWidth;
					descriptor2.height = cameraData.pixelHeight;
					RenderingUtils.ReAllocateIfNeeded(ref m_UpscaledTarget, in descriptor2, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_UpscaledTexture");
					Vector2 vector = new Vector2(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
					Vector2 outputImageSizeInPixels = new Vector2(cameraData.pixelWidth, cameraData.pixelHeight);
					FSRUtils.SetEasuConstants(cmd, vector, vector, outputImageSizeInPixels);
					Blitter.BlitCameraTexture(cmd, source, m_UpscaledTarget, loadAction, RenderBufferStoreAction.Store, m_Materials.easu, 0);
					float sharpnessLinear = (cameraData.fsrOverrideSharpness ? cameraData.fsrSharpness : 0.92f);
					if (cameraData.fsrSharpness > 0f)
					{
						finalPass.EnableKeyword(flag ? "_EASU_RCAS_AND_HDR_INPUT" : "_RCAS");
						FSRUtils.SetRcasConstantsLinear(cmd, sharpnessLinear);
					}
					source = m_UpscaledTarget;
					PostProcessUtils.SetSourceSize(cmd, descriptor2);
					break;
				}
				}
				break;
			case ImageScalingMode.Downscaling:
				flag5 = false;
				break;
			}
		}
		else if (flag3)
		{
			finalPass.EnableKeyword("_FXAA");
		}
		if (flag5)
		{
			finalPass.EnableKeyword("_RCAS");
			FSRUtils.SetRcasConstantsLinear(cmd, cameraData.taaSettings.contrastAdaptiveSharpening);
		}
		RenderTargetIdentifier cameraTargetIdentifier = RenderingUtils.GetCameraTargetIdentifier(ref renderingData);
		if (flag2)
		{
			Blitter.BlitCameraTexture(cmd, source, activeDebugHandler.DebugScreenColorHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, finalPass, 0);
			cameraData.renderer.ConfigureCameraTarget(activeDebugHandler.DebugScreenColorHandle, activeDebugHandler.DebugScreenDepthHandle);
		}
		else
		{
			RTHandleStaticHelpers.SetRTHandleStaticWrapper(cameraTargetIdentifier);
			RTHandle s_RTHandleWrapper = RTHandleStaticHelpers.s_RTHandleWrapper;
			RenderingUtils.FinalBlit(cmd, ref cameraData, source, s_RTHandleWrapper, loadAction, RenderBufferStoreAction.Store, finalPass, 0);
		}
	}
}
