using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal;

public sealed class UniversalRenderPipeline : RenderPipeline
{
	internal static class Profiling
	{
		public static class Pipeline
		{
			public static class Renderer
			{
				private const string k_Name = "ScriptableRenderer";

				public static readonly ProfilingSampler setupCullingParameters = new ProfilingSampler("ScriptableRenderer.SetupCullingParameters");

				public static readonly ProfilingSampler setup = new ProfilingSampler("ScriptableRenderer.Setup");
			}

			public static class Context
			{
				private const string k_Name = "ScriptableRenderContext";

				public static readonly ProfilingSampler submit = new ProfilingSampler("ScriptableRenderContext.Submit");
			}

			public static readonly ProfilingSampler beginContextRendering = new ProfilingSampler("RenderPipeline.BeginContextRendering");

			public static readonly ProfilingSampler endContextRendering = new ProfilingSampler("RenderPipeline.EndContextRendering");

			public static readonly ProfilingSampler beginCameraRendering = new ProfilingSampler("RenderPipeline.BeginCameraRendering");

			public static readonly ProfilingSampler endCameraRendering = new ProfilingSampler("RenderPipeline.EndCameraRendering");

			private const string k_Name = "UniversalRenderPipeline";

			public static readonly ProfilingSampler initializeCameraData = new ProfilingSampler("UniversalRenderPipeline.InitializeCameraData");

			public static readonly ProfilingSampler initializeStackedCameraData = new ProfilingSampler("UniversalRenderPipeline.InitializeStackedCameraData");

			public static readonly ProfilingSampler initializeAdditionalCameraData = new ProfilingSampler("UniversalRenderPipeline.InitializeAdditionalCameraData");

			public static readonly ProfilingSampler initializeRenderingData = new ProfilingSampler("UniversalRenderPipeline.InitializeRenderingData");

			public static readonly ProfilingSampler initializeShadowData = new ProfilingSampler("UniversalRenderPipeline.InitializeShadowData");

			public static readonly ProfilingSampler initializeLightData = new ProfilingSampler("UniversalRenderPipeline.InitializeLightData");

			public static readonly ProfilingSampler getPerObjectLightFlags = new ProfilingSampler("UniversalRenderPipeline.GetPerObjectLightFlags");

			public static readonly ProfilingSampler getMainLightIndex = new ProfilingSampler("UniversalRenderPipeline.GetMainLightIndex");

			public static readonly ProfilingSampler setupPerFrameShaderConstants = new ProfilingSampler("UniversalRenderPipeline.SetupPerFrameShaderConstants");

			public static readonly ProfilingSampler setupPerCameraShaderConstants = new ProfilingSampler("UniversalRenderPipeline.SetupPerCameraShaderConstants");
		}

		private static Dictionary<int, ProfilingSampler> s_HashSamplerCache = new Dictionary<int, ProfilingSampler>();

		public static readonly ProfilingSampler unknownSampler = new ProfilingSampler("Unknown");

		public static ProfilingSampler TryGetOrAddCameraSampler(Camera camera)
		{
			ProfilingSampler value = null;
			int hashCode = camera.GetHashCode();
			if (!s_HashSamplerCache.TryGetValue(hashCode, out value))
			{
				value = new ProfilingSampler("UniversalRenderPipeline.RenderSingleCameraInternal: " + camera.name);
				s_HashSamplerCache.Add(hashCode, value);
			}
			return value;
		}
	}

	public class SingleCameraRequest
	{
		public RenderTexture destination;

		public int mipLevel;

		public CubemapFace face = CubemapFace.Unknown;

		public int slice;
	}

	public const string k_ShaderTagName = "UniversalPipeline";

	internal const int k_DefaultRenderingLayerMask = 1;

	private readonly DebugDisplaySettingsUI m_DebugDisplaySettingsUI = new DebugDisplaySettingsUI();

	private UniversalRenderPipelineGlobalSettings m_GlobalSettings;

	internal static bool cameraStackRequiresDepthForPostprocessing = false;

	internal static RenderGraph s_RenderGraph;

	internal static RTHandleResourcePool s_RTHandlePool;

	private static bool useRenderGraph;

	private readonly UniversalRenderPipelineAsset pipelineAsset;

	internal bool enableHDROnce = true;

	private static Vector4 k_DefaultLightPosition = new Vector4(0f, 0f, 1f, 0f);

	private static Vector4 k_DefaultLightColor = Color.black;

	private static Vector4 k_DefaultLightAttenuation = new Vector4(0f, 1f, 0f, 1f);

	private static Vector4 k_DefaultLightSpotDirection = new Vector4(0f, 0f, 1f, 0f);

	private static Vector4 k_DefaultLightsProbeChannel = new Vector4(0f, 0f, 0f, 0f);

	private static List<Vector4> m_ShadowBiasData = new List<Vector4>();

	private static List<int> m_ShadowResolutionData = new List<int>();

	private Comparison<Camera> cameraComparison = (Camera camera1, Camera camera2) => (int)camera1.depth - (int)camera2.depth;

	private static Lightmapping.RequestLightsDelegate lightsDelegate = delegate(Light[] requests, NativeArray<LightDataGI> lightsOutput)
	{
		LightDataGI value = default(LightDataGI);
		if (!SupportedRenderingFeatures.active.enlighten || (SupportedRenderingFeatures.active.lightmapBakeTypes | LightmapBakeType.Realtime) == (LightmapBakeType)0)
		{
			for (int i = 0; i < requests.Length; i++)
			{
				Light light = requests[i];
				value.InitNoBake(light.GetInstanceID());
				lightsOutput[i] = value;
			}
		}
		else
		{
			for (int j = 0; j < requests.Length; j++)
			{
				Light light2 = requests[j];
				switch (light2.type)
				{
				case LightType.Directional:
				{
					DirectionalLight dir = default(DirectionalLight);
					LightmapperUtils.Extract(light2, ref dir);
					value.Init(ref dir);
					break;
				}
				case LightType.Point:
				{
					PointLight point = default(PointLight);
					LightmapperUtils.Extract(light2, ref point);
					value.Init(ref point);
					break;
				}
				case LightType.Spot:
				{
					SpotLight spot = default(SpotLight);
					LightmapperUtils.Extract(light2, ref spot);
					spot.innerConeAngle = light2.innerSpotAngle * (MathF.PI / 180f);
					spot.angularFalloff = AngularFalloffType.AnalyticAndInnerAngle;
					value.Init(ref spot);
					break;
				}
				case LightType.Area:
					value.InitNoBake(light2.GetInstanceID());
					break;
				case LightType.Disc:
					value.InitNoBake(light2.GetInstanceID());
					break;
				default:
					value.InitNoBake(light2.GetInstanceID());
					break;
				}
				value.falloff = FalloffType.InverseSquared;
				lightsOutput[j] = value;
			}
		}
	};

	public static float maxShadowBias => 10f;

	public static float minRenderScale => 0.1f;

	public static float maxRenderScale => 2f;

	public static int maxNumIterationsEnclosingSphere => 1000;

	public static int maxPerObjectLights
	{
		get
		{
			if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
			{
				return 8;
			}
			return 4;
		}
	}

	public static int maxVisibleAdditionalLights
	{
		get
		{
			bool flag = GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.SHADER_API_MOBILE);
			if (flag && (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30)))
			{
				return 16;
			}
			if (!flag && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
			{
				return 256;
			}
			return 32;
		}
	}

	internal static int lightsPerTile => (maxVisibleAdditionalLights + 31) / 32 * 32;

	internal static int maxZBinWords => 4096;

	internal static int maxTileWords => ((maxVisibleAdditionalLights <= 32) ? 1024 : 4096) * 4;

	internal static int maxVisibleReflectionProbes => Math.Min(maxVisibleAdditionalLights, 64);

	public override RenderPipelineGlobalSettings defaultSettings => m_GlobalSettings;

	public static UniversalRenderPipelineAsset asset => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

	public override string ToString()
	{
		return pipelineAsset?.ToString();
	}

	public UniversalRenderPipeline(UniversalRenderPipelineAsset asset)
	{
		pipelineAsset = asset;
		m_GlobalSettings = UniversalRenderPipelineGlobalSettings.instance;
		SetSupportedRenderingFeatures(pipelineAsset);
		RTHandles.Initialize(Screen.width, Screen.height);
		GraphicsSettings.useScriptableRenderPipelineBatching = asset.useSRPBatcher;
		if (((QualitySettings.antiAliasing <= 0) ? 1 : QualitySettings.antiAliasing) != asset.msaaSampleCount)
		{
			QualitySettings.antiAliasing = asset.msaaSampleCount;
		}
		XRSystem.SetDisplayMSAASamples((MSAASamples)Mathf.Clamp(Mathf.NextPowerOfTwo(QualitySettings.antiAliasing), 1, 8));
		XRSystem.SetRenderScale(asset.renderScale);
		Shader.globalRenderPipeline = "UniversalPipeline";
		Lightmapping.SetDelegate(lightsDelegate);
		CameraCaptureBridge.enabled = true;
		RenderingUtils.ClearSystemInfoCache();
		DecalProjector.defaultMaterial = asset.decalMaterial;
		s_RenderGraph = new RenderGraph("URPRenderGraph");
		useRenderGraph = false;
		s_RTHandlePool = new RTHandleResourcePool();
		DebugManager.instance.RefreshEditor();
		m_DebugDisplaySettingsUI.RegisterDebug(DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance);
		QualitySettings.enableLODCrossFade = asset.enableLODCrossFade;
	}

	protected override void Dispose(bool disposing)
	{
		m_DebugDisplaySettingsUI.UnregisterDebug();
		Blitter.Cleanup();
		base.Dispose(disposing);
		pipelineAsset.DestroyRenderers();
		Shader.globalRenderPipeline = string.Empty;
		SupportedRenderingFeatures.active = new SupportedRenderingFeatures();
		ShaderData.instance.Dispose();
		XRSystem.Dispose();
		s_RenderGraph.Cleanup();
		s_RenderGraph = null;
		s_RTHandlePool.Cleanup();
		s_RTHandlePool = null;
		Lightmapping.ResetDelegate();
		CameraCaptureBridge.enabled = false;
		DisposeAdditionalCameraData();
	}

	private void DisposeAdditionalCameraData()
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			if (allCameras[i].TryGetComponent<UniversalAdditionalCameraData>(out var component))
			{
				component.taaPersistentData?.DeallocateTargets();
			}
		}
	}

	protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
	{
		Render(renderContext, new List<Camera>(cameras));
	}

	protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
	{
		useRenderGraph = false;
		SetHDRState(cameras);
		SupportedRenderingFeatures.active.rendersUIOverlay = HDROutputForAnyDisplayIsActive();
		using (new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.UniversalRenderTotal)))
		{
			using (new ProfilingScope(null, Profiling.Pipeline.beginContextRendering))
			{
				RenderPipeline.BeginContextRendering(renderContext, cameras);
			}
			GraphicsSettings.lightsUseLinearIntensity = QualitySettings.activeColorSpace == ColorSpace.Linear;
			GraphicsSettings.lightsUseColorTemperature = true;
			GraphicsSettings.defaultRenderingLayerMask = 1u;
			SetupPerFrameShaderConstants();
			XRSystem.SetDisplayMSAASamples((MSAASamples)asset.msaaSampleCount);
			RTHandles.SetHardwareDynamicResolutionState(hwDynamicResRequested: true);
			SortCameras(cameras);
			for (int i = 0; i < cameras.Count; i++)
			{
				Camera camera = cameras[i];
				if (IsGameCamera(camera))
				{
					RenderCameraStack(renderContext, camera);
					continue;
				}
				using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
				{
					RenderPipeline.BeginCameraRendering(renderContext, camera);
				}
				UpdateVolumeFramework(camera, null);
				RenderSingleCameraInternal(renderContext, camera);
				using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
				{
					RenderPipeline.EndCameraRendering(renderContext, camera);
				}
			}
			s_RenderGraph.EndFrame();
			s_RTHandlePool.PurgeUnusedResources(Time.frameCount);
			using (new ProfilingScope(null, Profiling.Pipeline.endContextRendering))
			{
				RenderPipeline.EndContextRendering(renderContext, cameras);
			}
		}
	}

	protected override bool IsRenderRequestSupported<RequestData>(Camera camera, RequestData data)
	{
		if (data is StandardRequest)
		{
			return true;
		}
		if (data is SingleCameraRequest)
		{
			return true;
		}
		return false;
	}

	protected override void ProcessRenderRequests<RequestData>(ScriptableRenderContext context, Camera camera, RequestData renderRequest)
	{
		StandardRequest standardRequest = renderRequest as StandardRequest;
		SingleCameraRequest singleCameraRequest = renderRequest as SingleCameraRequest;
		if (standardRequest != null || singleCameraRequest != null)
		{
			RenderTexture renderTexture = ((standardRequest != null) ? standardRequest.destination : singleCameraRequest.destination);
			int num = standardRequest?.mipLevel ?? singleCameraRequest.mipLevel;
			int num2 = standardRequest?.slice ?? singleCameraRequest.slice;
			int num3 = (int)(standardRequest?.face ?? singleCameraRequest.face);
			RenderTexture targetTexture = camera.targetTexture;
			RenderTexture renderTexture2 = null;
			RenderTextureDescriptor desc = renderTexture.descriptor;
			if (renderTexture.dimension == TextureDimension.Cube)
			{
				desc = default(RenderTextureDescriptor);
			}
			desc.colorFormat = renderTexture.format;
			desc.volumeDepth = 1;
			desc.msaaSamples = renderTexture.descriptor.msaaSamples;
			desc.dimension = TextureDimension.Tex2D;
			desc.width = renderTexture.width / (int)Math.Pow(2.0, num);
			desc.height = renderTexture.height / (int)Math.Pow(2.0, num);
			desc.width = Mathf.Max(1, desc.width);
			desc.height = Mathf.Max(1, desc.height);
			if (renderTexture.dimension != TextureDimension.Tex2D || num != 0)
			{
				renderTexture2 = RenderTexture.GetTemporary(desc);
			}
			camera.targetTexture = (renderTexture2 ? renderTexture2 : renderTexture);
			List<Camera> value;
			using (ListPool<Camera>.Get(out value))
			{
				value.Add(camera);
				if (standardRequest != null)
				{
					Render(context, value.ToArray());
				}
				else
				{
					using (new ProfilingScope(null, Profiling.Pipeline.beginContextRendering))
					{
						RenderPipeline.BeginContextRendering(context, value);
					}
					using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
					{
						RenderPipeline.BeginCameraRendering(context, camera);
					}
					camera.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var component);
					RenderSingleCameraInternal(context, camera, ref component);
					using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
					{
						RenderPipeline.EndCameraRendering(context, camera);
					}
					using (new ProfilingScope(null, Profiling.Pipeline.endContextRendering))
					{
						RenderPipeline.EndContextRendering(context, value);
					}
				}
			}
			if ((bool)renderTexture2)
			{
				switch (renderTexture.dimension)
				{
				case TextureDimension.Tex2D:
				case TextureDimension.Tex3D:
				case TextureDimension.Tex2DArray:
					Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, num2, num);
					break;
				case TextureDimension.Cube:
				case TextureDimension.CubeArray:
					Graphics.CopyTexture(renderTexture2, 0, 0, renderTexture, num3 + num2 * 6, num);
					break;
				}
			}
			camera.targetTexture = targetTexture;
			Graphics.SetRenderTarget(targetTexture);
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
		else
		{
			Debug.LogWarning("The given RenderRequest type: " + typeof(RequestData).FullName + ", is either invalid or unsupported by the current pipeline");
		}
	}

	[Obsolete("RenderSingleCamera is obsolete, please use RenderPipeline.SubmitRenderRequest with UniversalRenderer.SingleCameraRequest as RequestData type", false)]
	public static void RenderSingleCamera(ScriptableRenderContext context, Camera camera)
	{
		RenderSingleCameraInternal(context, camera);
	}

	internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera)
	{
		UniversalAdditionalCameraData component = null;
		if (IsGameCamera(camera))
		{
			camera.gameObject.TryGetComponent<UniversalAdditionalCameraData>(out component);
		}
		RenderSingleCameraInternal(context, camera, ref component);
	}

	internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera, ref UniversalAdditionalCameraData additionalCameraData)
	{
		if (additionalCameraData != null && additionalCameraData.renderType != CameraRenderType.Base)
		{
			Debug.LogWarning("Only Base cameras can be rendered with standalone RenderSingleCamera. Camera will be skipped.");
			return;
		}
		InitializeCameraData(camera, additionalCameraData, resolveFinalTarget: true, out var cameraData);
		InitializeAdditionalCameraData(camera, additionalCameraData, resolveFinalTarget: true, ref cameraData);
		RenderSingleCamera(context, ref cameraData);
	}

	private static bool TryGetCullingParameters(CameraData cameraData, out ScriptableCullingParameters cullingParams)
	{
		if (cameraData.xr.enabled)
		{
			cullingParams = cameraData.xr.cullingParams;
			if (!cameraData.camera.usePhysicalProperties && !XRGraphicsAutomatedTests.enabled)
			{
				cameraData.camera.fieldOfView = 57.29578f * Mathf.Atan(1f / cullingParams.stereoProjectionMatrix.m11) * 2f;
			}
			return true;
		}
		return cameraData.camera.TryGetCullingParameters(stereoAware: false, out cullingParams);
	}

	private static void RenderSingleCamera(ScriptableRenderContext context, ref CameraData cameraData)
	{
		Camera camera = cameraData.camera;
		ScriptableRenderer renderer = cameraData.renderer;
		if (renderer == null)
		{
			Debug.LogWarning($"Trying to render {camera.name} with an invalid renderer. Camera rendering will be skipped.");
		}
		else
		{
			if (!TryGetCullingParameters(cameraData, out var cullingParams))
			{
				return;
			}
			ScriptableRenderer.current = renderer;
			_ = cameraData.isSceneViewCamera;
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			CommandBuffer cmd = (cameraData.xr.enabled ? null : commandBuffer);
			ProfilingSampler sampler = Profiling.TryGetOrAddCameraSampler(camera);
			using (new ProfilingScope(cmd, sampler))
			{
				renderer.Clear(cameraData.renderType);
				using (new ProfilingScope(null, Profiling.Pipeline.Renderer.setupCullingParameters))
				{
					renderer.OnPreCullRenderPasses(in cameraData);
					renderer.SetupCullingParameters(ref cullingParams, ref cameraData);
				}
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				SetupPerCameraShaderConstants(commandBuffer);
				if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
				{
					ScriptableRenderContext.EmitGeometryForCamera(camera);
				}
				if (camera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
				{
					component.motionVectorsPersistentData.Update(ref cameraData);
				}
				if (cameraData.taaPersistentData != null)
				{
					UpdateTemporalAATargets(ref cameraData);
				}
				RTHandles.SetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
				CullingResults cullResults = context.Cull(ref cullingParams);
				InitializeRenderingData(asset, ref cameraData, ref cullResults, commandBuffer, out var renderingData);
				renderer.AddRenderPasses(ref renderingData);
				if (useRenderGraph)
				{
					RecordAndExecuteRenderGraph(s_RenderGraph, context, ref renderingData);
					renderer.FinishRenderGraphRendering(context, ref renderingData);
				}
				else
				{
					using (new ProfilingScope(null, Profiling.Pipeline.Renderer.setup))
					{
						renderer.Setup(context, ref renderingData);
					}
					renderer.Execute(context, ref renderingData);
				}
			}
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
			using (new ProfilingScope(null, Profiling.Pipeline.Context.submit))
			{
				if (renderer.useRenderPassEnabled && !context.SubmitForRenderPassValidation())
				{
					renderer.useRenderPassEnabled = false;
					CoreUtils.SetKeyword(commandBuffer, "_RENDER_PASS_ENABLED", state: false);
					Debug.LogWarning("Rendering command not supported inside a native RenderPass found. Falling back to non-RenderPass rendering path");
				}
				context.Submit();
			}
			ScriptableRenderer.current = null;
		}
	}

	private static void RenderCameraStack(ScriptableRenderContext context, Camera baseCamera)
	{
		using (new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.RenderCameraStack)))
		{
			baseCamera.TryGetComponent<UniversalAdditionalCameraData>(out var component);
			if (component != null && component.renderType == CameraRenderType.Overlay)
			{
				return;
			}
			ScriptableRenderer scriptableRenderer = component?.scriptableRenderer;
			List<Camera> list = ((scriptableRenderer == null || !scriptableRenderer.SupportsCameraStackingType(CameraRenderType.Base)) ? null : component?.cameraStack);
			bool flag = component != null && component.renderPostProcessing;
			bool flag2 = HDROutputForMainDisplayIsActive();
			_ = asset.m_RendererDataList.Length;
			int num = -1;
			if (list != null)
			{
				Type type = component?.scriptableRenderer.GetType();
				bool flag3 = false;
				cameraStackRequiresDepthForPostprocessing = false;
				for (int i = 0; i < list.Count; i++)
				{
					Camera camera = list[i];
					if (camera == null)
					{
						flag3 = true;
					}
					else if (camera.isActiveAndEnabled)
					{
						camera.TryGetComponent<UniversalAdditionalCameraData>(out var component2);
						Type type2 = component2?.scriptableRenderer.GetType();
						if (type2 != type)
						{
							Debug.LogWarning("Only cameras with compatible renderer types can be stacked. The camera: " + camera.name + " are using the renderer " + type2.Name + ", but the base camera: " + baseCamera.name + " are using " + type.Name + ". Will skip rendering");
						}
						else if ((component2.scriptableRenderer.SupportedCameraStackingTypes() & 2) == 0)
						{
							Debug.LogWarning("The camera: " + camera.name + " is using a renderer of type " + scriptableRenderer.GetType().Name + " which does not support Overlay cameras in it's current state.");
						}
						else if (component2 == null || component2.renderType != CameraRenderType.Overlay)
						{
							Debug.LogWarning("Stack can only contain Overlay cameras. The camera: " + camera.name + " " + $"has a type {component2.renderType} that is not supported. Will skip rendering.");
						}
						else
						{
							cameraStackRequiresDepthForPostprocessing |= CheckPostProcessForDepth();
							flag |= component2.renderPostProcessing;
							num = i;
						}
					}
				}
				if (flag3)
				{
					component.UpdateCameraStack();
				}
			}
			flag &= SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
			bool flag4 = num != -1;
			bool flag5 = false;
			bool enableXR = component?.allowXRRendering ?? true;
			XRLayout xRLayout = XRSystem.NewLayout();
			xRLayout.AddCamera(baseCamera, enableXR);
			foreach (var activePass in xRLayout.GetActivePasses())
			{
				XRPass item = activePass.Item2;
				if (item.enabled)
				{
					flag5 = true;
					UpdateCameraStereoMatrices(baseCamera, item);
				}
				using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
				{
					RenderPipeline.BeginCameraRendering(context, baseCamera);
				}
				UpdateVolumeFramework(baseCamera, component);
				InitializeCameraData(baseCamera, component, !flag4, out var cameraData);
				RenderTextureDescriptor cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
				if (item.enabled)
				{
					cameraData.xr = item;
					UpdateCameraData(ref cameraData, cameraData.xr);
					xRLayout.ReconfigurePass(cameraData.xr, baseCamera);
					XRSystemUniversal.BeginLateLatching(baseCamera, cameraData.xrUniversal);
				}
				InitializeAdditionalCameraData(baseCamera, component, !flag4, ref cameraData);
				cameraData.postProcessingRequiresDepthTexture |= cameraStackRequiresDepthForPostprocessing;
				bool flag6 = flag2;
				if (item.enabled)
				{
					flag6 = item.isHDRDisplayOutputActive;
				}
				bool stackLastCameraOutputToHDR = asset.supportsHDR && flag6 && baseCamera.targetTexture == null && (baseCamera.cameraType == CameraType.Game || baseCamera.cameraType == CameraType.VR) && cameraData.allowHDROutput;
				cameraData.stackAnyPostProcessingEnabled = flag;
				cameraData.stackLastCameraOutputToHDR = stackLastCameraOutputToHDR;
				RenderSingleCamera(context, ref cameraData);
				using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
				{
					RenderPipeline.EndCameraRendering(context, baseCamera);
				}
				if (cameraData.xr.enabled)
				{
					XRSystemUniversal.EndLateLatching(baseCamera, cameraData.xrUniversal);
				}
				if (flag4)
				{
					for (int j = 0; j < list.Count; j++)
					{
						Camera camera2 = list[j];
						if (!camera2.isActiveAndEnabled)
						{
							continue;
						}
						camera2.TryGetComponent<UniversalAdditionalCameraData>(out var component3);
						if (component3 != null)
						{
							CameraData cameraData2 = cameraData;
							cameraData2.camera = camera2;
							cameraData2.baseCamera = baseCamera;
							UpdateCameraStereoMatrices(component3.camera, item);
							using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering))
							{
								RenderPipeline.BeginCameraRendering(context, camera2);
							}
							UpdateVolumeFramework(camera2, component3);
							bool resolveFinalTarget = j == num;
							InitializeAdditionalCameraData(camera2, component3, resolveFinalTarget, ref cameraData2);
							cameraData2.stackAnyPostProcessingEnabled = flag;
							cameraData2.stackLastCameraOutputToHDR = stackLastCameraOutputToHDR;
							xRLayout.ReconfigurePass(cameraData2.xr, camera2);
							RenderSingleCamera(context, ref cameraData2);
							using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering))
							{
								RenderPipeline.EndCameraRendering(context, camera2);
							}
						}
					}
				}
				if (cameraData.xr.enabled)
				{
					cameraData.cameraTargetDescriptor = cameraTargetDescriptor;
				}
			}
			if (flag5)
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				XRSystem.RenderMirrorView(commandBuffer, baseCamera);
				context.ExecuteCommandBuffer(commandBuffer);
				context.Submit();
				CommandBufferPool.Release(commandBuffer);
			}
			XRSystem.EndLayout();
		}
	}

	private static void UpdateCameraData(ref CameraData baseCameraData, in XRPass xr)
	{
		Rect rect = baseCameraData.camera.rect;
		Rect viewport = xr.GetViewport();
		baseCameraData.pixelRect = new Rect(rect.x * viewport.width + viewport.x, rect.y * viewport.height + viewport.y, rect.width * viewport.width, rect.height * viewport.height);
		Rect pixelRect = baseCameraData.pixelRect;
		baseCameraData.pixelWidth = (int)Math.Round(pixelRect.width + pixelRect.x) - (int)Math.Round(pixelRect.x);
		baseCameraData.pixelHeight = (int)Math.Round(pixelRect.height + pixelRect.y) - (int)Math.Round(pixelRect.y);
		baseCameraData.aspectRatio = (float)baseCameraData.pixelWidth / (float)baseCameraData.pixelHeight;
		RenderTextureDescriptor cameraTargetDescriptor = baseCameraData.cameraTargetDescriptor;
		baseCameraData.cameraTargetDescriptor = xr.renderTargetDesc;
		if (baseCameraData.isHdrEnabled)
		{
			baseCameraData.cameraTargetDescriptor.graphicsFormat = cameraTargetDescriptor.graphicsFormat;
		}
		baseCameraData.cameraTargetDescriptor.msaaSamples = cameraTargetDescriptor.msaaSamples;
		if (baseCameraData.isDefaultViewport)
		{
			baseCameraData.cameraTargetDescriptor.useDynamicScale = true;
			return;
		}
		baseCameraData.cameraTargetDescriptor.width = baseCameraData.pixelWidth;
		baseCameraData.cameraTargetDescriptor.height = baseCameraData.pixelHeight;
		baseCameraData.cameraTargetDescriptor.useDynamicScale = false;
	}

	private static void UpdateVolumeFramework(Camera camera, UniversalAdditionalCameraData additionalCameraData)
	{
		using (new ProfilingScope(null, ProfilingSampler.Get(URPProfileId.UpdateVolumeFramework)))
		{
			if (!((camera.cameraType == CameraType.SceneView) | (additionalCameraData != null && additionalCameraData.requiresVolumeFrameworkUpdate)) && (bool)additionalCameraData)
			{
				if (additionalCameraData.volumeStack == null)
				{
					camera.UpdateVolumeStack(additionalCameraData);
				}
				VolumeManager.instance.stack = additionalCameraData.volumeStack;
				return;
			}
			if ((bool)additionalCameraData && additionalCameraData.volumeStack != null)
			{
				camera.DestroyVolumeStack(additionalCameraData);
			}
			camera.GetVolumeLayerMaskAndTrigger(additionalCameraData, out var layerMask, out var trigger);
			VolumeManager.instance.ResetMainStack();
			VolumeManager.instance.Update(trigger, layerMask);
		}
	}

	private static bool CheckPostProcessForDepth(ref CameraData cameraData)
	{
		if (!cameraData.postProcessEnabled)
		{
			return false;
		}
		if ((cameraData.antialiasing == AntialiasingMode.SubpixelMorphologicalAntiAliasing || cameraData.IsTemporalAAEnabled()) && cameraData.renderType == CameraRenderType.Base)
		{
			return true;
		}
		return CheckPostProcessForDepth();
	}

	private static bool CheckPostProcessForDepth()
	{
		VolumeStack stack = VolumeManager.instance.stack;
		if (stack.GetComponent<DepthOfField>().IsActive())
		{
			return true;
		}
		if (stack.GetComponent<MotionBlur>().IsActive())
		{
			return true;
		}
		return false;
	}

	private static void SetSupportedRenderingFeatures(UniversalRenderPipelineAsset pipelineAsset)
	{
		SupportedRenderingFeatures.active.supportsHDR = pipelineAsset.supportsHDR;
	}

	private static void InitializeCameraData(Camera camera, UniversalAdditionalCameraData additionalCameraData, bool resolveFinalTarget, out CameraData cameraData)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.initializeCameraData))
		{
			cameraData = default(CameraData);
			InitializeStackedCameraData(camera, additionalCameraData, ref cameraData);
			cameraData.camera = camera;
			bool flag = (additionalCameraData?.scriptableRenderer)?.supportedRenderingFeatures.msaa ?? false;
			int msaaSamples = 1;
			if (camera.allowMSAA && asset.msaaSampleCount > 1 && flag)
			{
				msaaSamples = ((camera.targetTexture != null) ? camera.targetTexture.antiAliasing : asset.msaaSampleCount);
			}
			if (cameraData.xrRendering && flag && camera.targetTexture == null)
			{
				msaaSamples = (int)XRSystem.GetDisplayMSAASamples();
			}
			bool preserveFramebufferAlpha = Graphics.preserveFramebufferAlpha;
			cameraData.hdrColorBufferPrecision = (asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits);
			cameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(camera, ref cameraData, cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, msaaSamples, preserveFramebufferAlpha, cameraData.requiresOpaqueTexture);
		}
	}

	private static void InitializeStackedCameraData(Camera baseCamera, UniversalAdditionalCameraData baseAdditionalCameraData, ref CameraData cameraData)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.initializeStackedCameraData))
		{
			UniversalRenderPipelineAsset universalRenderPipelineAsset = asset;
			cameraData.targetTexture = baseCamera.targetTexture;
			cameraData.cameraType = baseCamera.cameraType;
			if (cameraData.isSceneViewCamera)
			{
				cameraData.volumeLayerMask = 1;
				cameraData.volumeTrigger = null;
				cameraData.isStopNaNEnabled = false;
				cameraData.isDitheringEnabled = false;
				cameraData.antialiasing = AntialiasingMode.None;
				cameraData.antialiasingQuality = AntialiasingQuality.High;
				cameraData.xrRendering = false;
				cameraData.allowHDROutput = false;
			}
			else if (baseAdditionalCameraData != null)
			{
				cameraData.volumeLayerMask = baseAdditionalCameraData.volumeLayerMask;
				cameraData.volumeTrigger = ((baseAdditionalCameraData.volumeTrigger == null) ? baseCamera.transform : baseAdditionalCameraData.volumeTrigger);
				cameraData.isStopNaNEnabled = baseAdditionalCameraData.stopNaN && SystemInfo.graphicsShaderLevel >= 35;
				cameraData.isDitheringEnabled = baseAdditionalCameraData.dithering;
				cameraData.antialiasing = baseAdditionalCameraData.antialiasing;
				cameraData.antialiasingQuality = baseAdditionalCameraData.antialiasingQuality;
				cameraData.xrRendering = baseAdditionalCameraData.allowXRRendering && XRSystem.displayActive;
				cameraData.allowHDROutput = baseAdditionalCameraData.allowHDROutput;
			}
			else
			{
				cameraData.volumeLayerMask = 1;
				cameraData.volumeTrigger = null;
				cameraData.isStopNaNEnabled = false;
				cameraData.isDitheringEnabled = false;
				cameraData.antialiasing = AntialiasingMode.None;
				cameraData.antialiasingQuality = AntialiasingQuality.High;
				cameraData.xrRendering = XRSystem.displayActive;
				cameraData.allowHDROutput = true;
			}
			cameraData.isHdrEnabled = baseCamera.allowHDR && universalRenderPipelineAsset.supportsHDR;
			cameraData.allowHDROutput &= universalRenderPipelineAsset.supportsHDR;
			Rect rect = baseCamera.rect;
			cameraData.pixelRect = baseCamera.pixelRect;
			cameraData.pixelWidth = baseCamera.pixelWidth;
			cameraData.pixelHeight = baseCamera.pixelHeight;
			cameraData.aspectRatio = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
			cameraData.isDefaultViewport = !(Math.Abs(rect.x) > 0f) && !(Math.Abs(rect.y) > 0f) && !(Math.Abs(rect.width) < 1f) && !(Math.Abs(rect.height) < 1f);
			bool flag = cameraData.cameraType == CameraType.SceneView || cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection;
			bool flag2 = Mathf.Abs(1f - universalRenderPipelineAsset.renderScale) < 0.05f || flag;
			cameraData.renderScale = (flag2 ? 1f : universalRenderPipelineAsset.renderScale);
			cameraData.upscalingFilter = ResolveUpscalingFilterSelection(new Vector2(cameraData.pixelWidth, cameraData.pixelHeight), cameraData.renderScale, universalRenderPipelineAsset.upscalingFilter);
			if (cameraData.renderScale > 1f)
			{
				cameraData.imageScalingMode = ImageScalingMode.Downscaling;
			}
			else if (cameraData.renderScale < 1f || (!flag && cameraData.upscalingFilter == ImageUpscalingFilter.FSR))
			{
				cameraData.imageScalingMode = ImageScalingMode.Upscaling;
			}
			else
			{
				cameraData.imageScalingMode = ImageScalingMode.None;
			}
			cameraData.fsrOverrideSharpness = universalRenderPipelineAsset.fsrOverrideSharpness;
			cameraData.fsrSharpness = universalRenderPipelineAsset.fsrSharpness;
			cameraData.xr = XRSystem.emptyPass;
			XRSystem.SetRenderScale(cameraData.renderScale);
			SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
			SortingCriteria sortingCriteria2 = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
			bool hasHiddenSurfaceRemovalOnGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
			bool flag3 = (baseCamera.opaqueSortMode == OpaqueSortMode.Default && hasHiddenSurfaceRemovalOnGPU) || baseCamera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
			cameraData.defaultOpaqueSortFlags = (flag3 ? sortingCriteria2 : sortingCriteria);
			cameraData.captureActions = CameraCaptureBridge.GetCaptureActions(baseCamera);
		}
	}

	private static void InitializeAdditionalCameraData(Camera camera, UniversalAdditionalCameraData additionalCameraData, bool resolveFinalTarget, ref CameraData cameraData)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.initializeAdditionalCameraData))
		{
			UniversalRenderPipelineAsset universalRenderPipelineAsset = asset;
			bool flag = universalRenderPipelineAsset.supportsMainLightShadows || universalRenderPipelineAsset.supportsAdditionalLightShadows;
			cameraData.maxShadowDistance = Mathf.Min(universalRenderPipelineAsset.shadowDistance, camera.farClipPlane);
			cameraData.maxShadowDistance = ((flag && cameraData.maxShadowDistance >= camera.nearClipPlane) ? cameraData.maxShadowDistance : 0f);
			bool isSceneViewCamera = cameraData.isSceneViewCamera;
			if (isSceneViewCamera)
			{
				cameraData.renderType = CameraRenderType.Base;
				cameraData.clearDepth = true;
				cameraData.postProcessEnabled = CoreUtils.ArePostProcessesEnabled(camera);
				cameraData.requiresDepthTexture = universalRenderPipelineAsset.supportsCameraDepthTexture;
				cameraData.requiresOpaqueTexture = universalRenderPipelineAsset.supportsCameraOpaqueTexture;
				cameraData.renderer = asset.scriptableRenderer;
				cameraData.useScreenCoordOverride = false;
				cameraData.screenSizeOverride = cameraData.pixelRect.size;
				cameraData.screenCoordScaleBias = Vector2.one;
			}
			else if (additionalCameraData != null)
			{
				cameraData.renderType = additionalCameraData.renderType;
				cameraData.clearDepth = additionalCameraData.renderType == CameraRenderType.Base || additionalCameraData.clearDepth;
				cameraData.postProcessEnabled = additionalCameraData.renderPostProcessing;
				cameraData.maxShadowDistance = (additionalCameraData.renderShadows ? cameraData.maxShadowDistance : 0f);
				cameraData.requiresDepthTexture = additionalCameraData.requiresDepthTexture;
				cameraData.requiresOpaqueTexture = additionalCameraData.requiresColorTexture;
				cameraData.renderer = additionalCameraData.scriptableRenderer;
				cameraData.useScreenCoordOverride = additionalCameraData.useScreenCoordOverride;
				cameraData.screenSizeOverride = additionalCameraData.screenSizeOverride;
				cameraData.screenCoordScaleBias = additionalCameraData.screenCoordScaleBias;
			}
			else
			{
				cameraData.renderType = CameraRenderType.Base;
				cameraData.clearDepth = true;
				cameraData.postProcessEnabled = false;
				cameraData.requiresDepthTexture = universalRenderPipelineAsset.supportsCameraDepthTexture;
				cameraData.requiresOpaqueTexture = universalRenderPipelineAsset.supportsCameraOpaqueTexture;
				cameraData.renderer = asset.scriptableRenderer;
				cameraData.useScreenCoordOverride = false;
				cameraData.screenSizeOverride = cameraData.pixelRect.size;
				cameraData.screenCoordScaleBias = Vector2.one;
			}
			cameraData.postProcessEnabled &= SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2;
			cameraData.requiresDepthTexture |= isSceneViewCamera;
			cameraData.postProcessingRequiresDepthTexture = CheckPostProcessForDepth(ref cameraData);
			cameraData.resolveFinalTarget = resolveFinalTarget;
			bool num = cameraData.renderType == CameraRenderType.Overlay;
			if (num)
			{
				cameraData.requiresOpaqueTexture = false;
			}
			if (additionalCameraData != null)
			{
				UpdateTemporalAAData(ref cameraData, additionalCameraData);
			}
			Matrix4x4 projectionMatrix = camera.projectionMatrix;
			if (num && !camera.orthographic && cameraData.pixelRect != camera.pixelRect)
			{
				float m = camera.projectionMatrix.m00 * camera.aspect / cameraData.aspectRatio;
				projectionMatrix.m00 = m;
			}
			ApplyTaaRenderingDebugOverrides(ref cameraData.taaSettings);
			Matrix4x4 jitterMatrix = TemporalAA.CalculateJitterMatrix(ref cameraData);
			cameraData.SetViewProjectionAndJitterMatrix(camera.worldToCameraMatrix, projectionMatrix, jitterMatrix);
			cameraData.worldSpaceCameraPos = camera.transform.position;
			Color backgroundColor = camera.backgroundColor;
			cameraData.backgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColor);
			cameraData.stackAnyPostProcessingEnabled = cameraData.postProcessEnabled;
			cameraData.stackLastCameraOutputToHDR = cameraData.isHDROutputActive;
		}
	}

	private static void InitializeRenderingData(UniversalRenderPipelineAsset settings, ref CameraData cameraData, ref CullingResults cullResults, CommandBuffer cmd, out RenderingData renderingData)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.initializeRenderingData))
		{
			bool flag = cameraData.renderer is UniversalRenderer universalRenderer && universalRenderer.renderingModeActual == RenderingMode.ForwardPlus;
			NativeArray<VisibleLight> visibleLights = cullResults.visibleLights;
			int mainLightIndex = GetMainLightIndex(settings, visibleLights);
			bool mainLightCastShadows = false;
			bool flag2 = false;
			if (cameraData.maxShadowDistance > 0f)
			{
				mainLightCastShadows = mainLightIndex != -1 && visibleLights[mainLightIndex].light != null && visibleLights[mainLightIndex].light.shadows != LightShadows.None;
				if (settings.supportsAdditionalLightShadows && (settings.additionalLightsRenderingMode == LightRenderingMode.PerPixel || flag))
				{
					for (int i = 0; i < visibleLights.Length; i++)
					{
						if (i != mainLightIndex)
						{
							ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(i);
							Light light = reference.light;
							if ((reference.lightType == LightType.Spot || reference.lightType == LightType.Point) && light != null && light.shadows != LightShadows.None)
							{
								flag2 = true;
								break;
							}
						}
					}
				}
			}
			renderingData.cullResults = cullResults;
			renderingData.cameraData = cameraData;
			InitializeLightData(settings, visibleLights, mainLightIndex, out renderingData.lightData);
			InitializeShadowData(settings, visibleLights, mainLightCastShadows, flag2 && !renderingData.lightData.shadeAdditionalLightsPerVertex, flag, out renderingData.shadowData);
			InitializePostProcessingData(settings, cameraData.stackLastCameraOutputToHDR, out renderingData.postProcessingData);
			renderingData.supportsDynamicBatching = settings.supportsDynamicBatching;
			renderingData.perObjectData = GetPerObjectLightFlags(renderingData.lightData.additionalLightsCount, flag);
			renderingData.postProcessingEnabled = cameraData.stackAnyPostProcessingEnabled;
			renderingData.commandBuffer = cmd;
			CheckAndApplyDebugSettings(ref renderingData);
		}
	}

	private static void InitializeShadowData(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights, bool mainLightCastShadows, bool additionalLightsCastShadows, bool isForwardPlus, out ShadowData shadowData)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.initializeShadowData))
		{
			m_ShadowBiasData.Clear();
			m_ShadowResolutionData.Clear();
			for (int i = 0; i < visibleLights.Length; i++)
			{
				Light light = visibleLights.UnsafeElementAtMutable(i).light;
				UniversalAdditionalLightData component = null;
				if (light != null)
				{
					light.gameObject.TryGetComponent<UniversalAdditionalLightData>(out component);
				}
				if ((bool)component && !component.usePipelineSettings)
				{
					m_ShadowBiasData.Add(new Vector4(light.shadowBias, light.shadowNormalBias, 0f, 0f));
				}
				else
				{
					m_ShadowBiasData.Add(new Vector4(settings.shadowDepthBias, settings.shadowNormalBias, 0f, 0f));
				}
				if ((bool)component && component.additionalLightsShadowResolutionTier == UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierCustom)
				{
					m_ShadowResolutionData.Add((int)light.shadowResolution);
				}
				else if ((bool)component && component.additionalLightsShadowResolutionTier != UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierCustom)
				{
					int additionalLightsShadowResolutionTier = Mathf.Clamp(component.additionalLightsShadowResolutionTier, UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierLow, UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierHigh);
					m_ShadowResolutionData.Add(settings.GetAdditionalLightsShadowResolution(additionalLightsShadowResolutionTier));
				}
				else
				{
					m_ShadowResolutionData.Add(settings.GetAdditionalLightsShadowResolution(UniversalAdditionalLightData.AdditionalLightsShadowDefaultResolutionTier));
				}
			}
			shadowData.bias = m_ShadowBiasData;
			shadowData.resolution = m_ShadowResolutionData;
			shadowData.mainLightShadowsEnabled = settings.supportsMainLightShadows && settings.mainLightRenderingMode == LightRenderingMode.PerPixel;
			shadowData.supportsMainLightShadows = SystemInfo.supportsShadows && shadowData.mainLightShadowsEnabled && mainLightCastShadows;
			shadowData.requiresScreenSpaceShadowResolve = false;
			shadowData.mainLightShadowCascadesCount = ((SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2) ? 1 : settings.shadowCascadeCount);
			shadowData.mainLightShadowmapWidth = settings.mainLightShadowmapResolution;
			shadowData.mainLightShadowmapHeight = settings.mainLightShadowmapResolution;
			switch (shadowData.mainLightShadowCascadesCount)
			{
			case 1:
				shadowData.mainLightShadowCascadesSplit = new Vector3(1f, 0f, 0f);
				break;
			case 2:
				shadowData.mainLightShadowCascadesSplit = new Vector3(settings.cascade2Split, 1f, 0f);
				break;
			case 3:
				shadowData.mainLightShadowCascadesSplit = new Vector3(settings.cascade3Split.x, settings.cascade3Split.y, 0f);
				break;
			default:
				shadowData.mainLightShadowCascadesSplit = settings.cascade4Split;
				break;
			}
			shadowData.mainLightShadowCascadeBorder = settings.cascadeBorder;
			shadowData.additionalLightShadowsEnabled = settings.supportsAdditionalLightShadows && (settings.additionalLightsRenderingMode == LightRenderingMode.PerPixel || isForwardPlus);
			shadowData.supportsAdditionalLightShadows = SystemInfo.supportsShadows && shadowData.additionalLightShadowsEnabled && additionalLightsCastShadows;
			shadowData.additionalLightsShadowmapWidth = (shadowData.additionalLightsShadowmapHeight = settings.additionalLightsShadowmapResolution);
			shadowData.supportsSoftShadows = settings.supportsSoftShadows && (shadowData.supportsMainLightShadows || shadowData.supportsAdditionalLightShadows);
			shadowData.shadowmapDepthBufferBits = 16;
			shadowData.isKeywordAdditionalLightShadowsEnabled = false;
			shadowData.isKeywordSoftShadowsEnabled = false;
		}
	}

	private static void InitializePostProcessingData(UniversalRenderPipelineAsset settings, bool isHDROutputActive, out PostProcessingData postProcessingData)
	{
		postProcessingData.gradingMode = (settings.supportsHDR ? settings.colorGradingMode : ColorGradingMode.LowDynamicRange);
		if (isHDROutputActive)
		{
			postProcessingData.gradingMode = ColorGradingMode.HighDynamicRange;
		}
		postProcessingData.lutSize = settings.colorGradingLutSize;
		postProcessingData.useFastSRGBLinearConversion = settings.useFastSRGBLinearConversion;
		postProcessingData.supportDataDrivenLensFlare = settings.supportDataDrivenLensFlare;
	}

	private static void InitializeLightData(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights, int mainLightIndex, out LightData lightData)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.initializeLightData))
		{
			int val = maxPerObjectLights;
			int val2 = maxVisibleAdditionalLights;
			lightData.mainLightIndex = mainLightIndex;
			if (settings.additionalLightsRenderingMode != LightRenderingMode.Disabled)
			{
				lightData.additionalLightsCount = Math.Min((mainLightIndex != -1) ? (visibleLights.Length - 1) : visibleLights.Length, val2);
				lightData.maxPerObjectAdditionalLightsCount = Math.Min(settings.maxAdditionalLightsCount, val);
			}
			else
			{
				lightData.additionalLightsCount = 0;
				lightData.maxPerObjectAdditionalLightsCount = 0;
			}
			lightData.supportsAdditionalLights = settings.additionalLightsRenderingMode != LightRenderingMode.Disabled;
			lightData.shadeAdditionalLightsPerVertex = settings.additionalLightsRenderingMode == LightRenderingMode.PerVertex;
			lightData.visibleLights = visibleLights;
			lightData.supportsMixedLighting = settings.supportsMixedLighting;
			lightData.reflectionProbeBlending = settings.reflectionProbeBlending;
			lightData.reflectionProbeBoxProjection = settings.reflectionProbeBoxProjection;
			lightData.supportsLightLayers = RenderingUtils.SupportsLightLayers(SystemInfo.graphicsDeviceType) && settings.useRenderingLayers;
		}
	}

	private static void ApplyTaaRenderingDebugOverrides(ref TemporalAA.Settings taaSettings)
	{
		switch (DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance.renderingSettings.taaDebugMode)
		{
		case DebugDisplaySettingsRendering.TaaDebugMode.ShowClampedHistory:
			taaSettings.m_FrameInfluence = 0f;
			break;
		case DebugDisplaySettingsRendering.TaaDebugMode.ShowRawFrame:
			taaSettings.m_FrameInfluence = 1f;
			break;
		case DebugDisplaySettingsRendering.TaaDebugMode.ShowRawFrameNoJitter:
			taaSettings.m_FrameInfluence = 1f;
			taaSettings.jitterScale = 0f;
			break;
		}
	}

	private static void UpdateTemporalAAData(ref CameraData cameraData, UniversalAdditionalCameraData additionalCameraData)
	{
		ref RenderTextureDescriptor cameraTargetDescriptor = ref cameraData.cameraTargetDescriptor;
		cameraData.taaPersistentData = additionalCameraData.taaPersistentData;
		cameraData.taaPersistentData.Init(cameraTargetDescriptor.width, cameraTargetDescriptor.height, cameraTargetDescriptor.volumeDepth, cameraTargetDescriptor.graphicsFormat, cameraTargetDescriptor.vrUsage, cameraTargetDescriptor.dimension);
		ref TemporalAA.Settings taaSettings = ref additionalCameraData.taaSettings;
		cameraData.taaSettings = taaSettings;
		taaSettings.resetHistoryFrames -= ((taaSettings.resetHistoryFrames > 0) ? 1 : 0);
	}

	private static void UpdateTemporalAATargets(ref CameraData cameraData)
	{
		if (cameraData.IsTemporalAAEnabled())
		{
			bool flag = false;
			flag = cameraData.xr.enabled && !cameraData.xr.singlePassEnabled;
			if (cameraData.taaPersistentData.AllocateTargets(flag))
			{
				cameraData.taaSettings.resetHistoryFrames += ((!flag) ? 1 : 2);
			}
		}
		else
		{
			cameraData.taaPersistentData.DeallocateTargets();
		}
	}

	private static void UpdateCameraStereoMatrices(Camera camera, XRPass xr)
	{
		if (!xr.enabled)
		{
			return;
		}
		if (xr.singlePassEnabled)
		{
			for (int i = 0; i < Mathf.Min(2, xr.viewCount); i++)
			{
				camera.SetStereoProjectionMatrix((Camera.StereoscopicEye)i, xr.GetProjMatrix(i));
				camera.SetStereoViewMatrix((Camera.StereoscopicEye)i, xr.GetViewMatrix(i));
			}
		}
		else
		{
			camera.SetStereoProjectionMatrix((Camera.StereoscopicEye)xr.multipassId, xr.GetProjMatrix());
			camera.SetStereoViewMatrix((Camera.StereoscopicEye)xr.multipassId, xr.GetViewMatrix());
		}
	}

	private static PerObjectData GetPerObjectLightFlags(int additionalLightsCount, bool isForwardPlus)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.getPerObjectLightFlags))
		{
			PerObjectData perObjectData = PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask;
			if (!isForwardPlus)
			{
				perObjectData |= PerObjectData.ReflectionProbes | PerObjectData.LightData;
			}
			if (additionalLightsCount > 0 && !isForwardPlus && !RenderingUtils.useStructuredBuffer)
			{
				perObjectData |= PerObjectData.LightIndices;
			}
			return perObjectData;
		}
	}

	private static int GetMainLightIndex(UniversalRenderPipelineAsset settings, NativeArray<VisibleLight> visibleLights)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.getMainLightIndex))
		{
			int length = visibleLights.Length;
			if (length == 0 || settings.mainLightRenderingMode != LightRenderingMode.PerPixel)
			{
				return -1;
			}
			Light sun = RenderSettings.sun;
			int result = -1;
			float num = 0f;
			for (int i = 0; i < length; i++)
			{
				ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(i);
				Light light = reference.light;
				if (light == null)
				{
					break;
				}
				if (reference.lightType == LightType.Directional)
				{
					if (light == sun)
					{
						return i;
					}
					if (light.intensity > num)
					{
						num = light.intensity;
						result = i;
					}
				}
			}
			return result;
		}
	}

	private static void SetupPerFrameShaderConstants()
	{
		using (new ProfilingScope(null, Profiling.Pipeline.setupPerFrameShaderConstants))
		{
			Shader.SetGlobalColor(ShaderPropertyId.rendererColor, Color.white);
			if (asset.lodCrossFadeDitheringType == LODCrossFadeDitheringType.BayerMatrix)
			{
				Shader.SetGlobalFloat(ShaderPropertyId.ditheringTextureInvSize, 1f / (float)asset.textures.bayerMatrixTex.width);
				Shader.SetGlobalTexture(ShaderPropertyId.ditheringTexture, asset.textures.bayerMatrixTex);
			}
			else if (asset.lodCrossFadeDitheringType == LODCrossFadeDitheringType.BlueNoise)
			{
				Shader.SetGlobalFloat(ShaderPropertyId.ditheringTextureInvSize, 1f / (float)asset.textures.blueNoise64LTex.width);
				Shader.SetGlobalTexture(ShaderPropertyId.ditheringTexture, asset.textures.blueNoise64LTex);
			}
		}
	}

	private static void SetupPerCameraShaderConstants(CommandBuffer cmd)
	{
		using (new ProfilingScope(null, Profiling.Pipeline.setupPerCameraShaderConstants))
		{
			SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
			Color color = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
			cmd.SetGlobalVector(ShaderPropertyId.glossyEnvironmentColor, color);
			cmd.SetGlobalTexture(ShaderPropertyId.glossyEnvironmentCubeMap, ReflectionProbe.defaultTexture);
			cmd.SetGlobalVector(ShaderPropertyId.glossyEnvironmentCubeMapHDR, ReflectionProbe.defaultTextureHDRDecodeValues);
			cmd.SetGlobalVector(ShaderPropertyId.ambientSkyColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientSkyColor));
			cmd.SetGlobalVector(ShaderPropertyId.ambientEquatorColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientEquatorColor));
			cmd.SetGlobalVector(ShaderPropertyId.ambientGroundColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.ambientGroundColor));
			cmd.SetGlobalVector(ShaderPropertyId.subtractiveShadowColor, CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.subtractiveShadowColor));
		}
	}

	private static void CheckAndApplyDebugSettings(ref RenderingData renderingData)
	{
		UniversalRenderPipelineDebugDisplaySettings instance = DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance;
		ref CameraData cameraData = ref renderingData.cameraData;
		if (instance.AreAnySettingsActive && !cameraData.isPreviewCamera)
		{
			DebugDisplaySettingsRendering renderingSettings = instance.renderingSettings;
			int msaaSamples = cameraData.cameraTargetDescriptor.msaaSamples;
			if (!renderingSettings.enableMsaa)
			{
				msaaSamples = 1;
			}
			if (!renderingSettings.enableHDR)
			{
				cameraData.isHdrEnabled = false;
			}
			if (!instance.IsPostProcessingAllowed)
			{
				cameraData.postProcessEnabled = false;
			}
			cameraData.hdrColorBufferPrecision = (asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits);
			cameraData.cameraTargetDescriptor.graphicsFormat = MakeRenderTextureGraphicsFormat(cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, needsAlpha: true);
			cameraData.cameraTargetDescriptor.msaaSamples = msaaSamples;
		}
	}

	private static ImageUpscalingFilter ResolveUpscalingFilterSelection(Vector2 imageSize, float renderScale, UpscalingFilterSelection selection)
	{
		ImageUpscalingFilter result = ImageUpscalingFilter.Linear;
		if (selection == UpscalingFilterSelection.FSR && !FSRUtils.IsSupported())
		{
			selection = UpscalingFilterSelection.Auto;
		}
		switch (selection)
		{
		case UpscalingFilterSelection.Auto:
		{
			float num = 1f / renderScale;
			if (Mathf.Approximately(num - Mathf.Floor(num), 0f))
			{
				float num2 = imageSize.x / num;
				float num3 = imageSize.y / num;
				if (Mathf.Approximately(num2 - Mathf.Floor(num2), 0f) && Mathf.Approximately(num3 - Mathf.Floor(num3), 0f))
				{
					result = ImageUpscalingFilter.Point;
				}
			}
			break;
		}
		case UpscalingFilterSelection.Point:
			result = ImageUpscalingFilter.Point;
			break;
		case UpscalingFilterSelection.FSR:
			result = ImageUpscalingFilter.FSR;
			break;
		}
		return result;
	}

	internal static bool HDROutputForMainDisplayIsActive()
	{
		bool flag = SystemInfo.hdrDisplaySupportFlags.HasFlag(HDRDisplaySupportFlags.Supported) && asset.supportsHDR;
		bool flag2 = HDROutputSettings.main.available && HDROutputSettings.main.active;
		return !Application.isMobilePlatform && flag && flag2;
	}

	internal static bool HDROutputForAnyDisplayIsActive()
	{
		bool flag = HDROutputForMainDisplayIsActive();
		if (XRSystem.displayActive)
		{
			flag |= XRSystem.isHDRDisplayOutputActive;
		}
		return flag;
	}

	private void SetHDRState(List<Camera> cameras)
	{
		bool flag = HDROutputSettings.main.available && HDROutputSettings.main.active;
		if (SystemInfo.hdrDisplaySupportFlags.HasFlag(HDRDisplaySupportFlags.RuntimeSwitchable) && !asset.supportsHDR && flag)
		{
			HDROutputSettings.main.RequestHDRModeChange(enabled: false);
		}
		if (flag)
		{
			HDROutputSettings.main.automaticHDRTonemapping = false;
		}
	}

	internal static void GetHDROutputLuminanceParameters(HDROutputUtils.HDRDisplayInformation hdrDisplayInformation, ColorGamut hdrDisplayColorGamut, Tonemapping tonemapping, out Vector4 hdrOutputParameters)
	{
		float x = hdrDisplayInformation.minToneMapLuminance;
		float y = hdrDisplayInformation.maxToneMapLuminance;
		float num = hdrDisplayInformation.paperWhiteNits;
		if (!tonemapping.detectPaperWhite.value)
		{
			num = tonemapping.paperWhite.value;
		}
		if (!tonemapping.detectBrightnessLimits.value)
		{
			x = tonemapping.minNits.value;
			y = tonemapping.maxNits.value;
		}
		hdrOutputParameters = new Vector4(x, y, num, 1f / num);
	}

	internal static void GetHDROutputGradingParameters(Tonemapping tonemapping, out Vector4 hdrOutputParameters)
	{
		int num = 0;
		float y = 0f;
		switch (tonemapping.mode.value)
		{
		case TonemappingMode.Neutral:
			num = (int)tonemapping.neutralHDRRangeReductionMode.value;
			y = tonemapping.hueShiftAmount.value;
			break;
		case TonemappingMode.ACES:
			num = (int)tonemapping.acesPreset.value;
			break;
		}
		hdrOutputParameters = new Vector4(num, y, 0f, 0f);
	}

	public static bool IsGameCamera(Camera camera)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		if (camera.cameraType != CameraType.Game)
		{
			return camera.cameraType == CameraType.VR;
		}
		return true;
	}

	[Obsolete("Please use CameraData.xr.enabled instead.", true)]
	public static bool IsStereoEnabled(Camera camera)
	{
		if (camera == null)
		{
			throw new ArgumentNullException("camera");
		}
		if (IsGameCamera(camera))
		{
			return camera.stereoTargetEye == StereoTargetEyeMask.Both;
		}
		return false;
	}

	private void SortCameras(List<Camera> cameras)
	{
		if (cameras.Count > 1)
		{
			cameras.Sort(cameraComparison);
		}
	}

	internal static GraphicsFormat MakeRenderTextureGraphicsFormat(bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, bool needsAlpha)
	{
		if (isHdrEnabled)
		{
			if (!needsAlpha && requestHDRColorBufferPrecision != HDRColorBufferPrecision._64Bits && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
			{
				return GraphicsFormat.B10G11R11_UFloatPack32;
			}
			if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Blend))
			{
				return GraphicsFormat.R16G16B16A16_SFloat;
			}
			return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
		}
		return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
	}

	internal static GraphicsFormat MakeUnormRenderTextureGraphicsFormat()
	{
		if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.A2B10G10R10_UNormPack32, FormatUsage.Blend))
		{
			return GraphicsFormat.A2B10G10R10_UNormPack32;
		}
		return GraphicsFormat.R8G8B8A8_UNorm;
	}

	internal static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera, ref CameraData cameraData, bool isHdrEnabled, HDRColorBufferPrecision requestHDRColorBufferPrecision, int msaaSamples, bool needsAlpha, bool requiresOpaqueTexture)
	{
		RenderTextureDescriptor renderTextureDescriptor;
		if (camera.targetTexture == null)
		{
			renderTextureDescriptor = new RenderTextureDescriptor(cameraData.scaledWidth, cameraData.scaledHeight);
			renderTextureDescriptor.graphicsFormat = MakeRenderTextureGraphicsFormat(isHdrEnabled, requestHDRColorBufferPrecision, needsAlpha);
			renderTextureDescriptor.depthBufferBits = 32;
			renderTextureDescriptor.msaaSamples = msaaSamples;
			renderTextureDescriptor.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
		}
		else
		{
			renderTextureDescriptor = camera.targetTexture.descriptor;
			renderTextureDescriptor.msaaSamples = msaaSamples;
			renderTextureDescriptor.width = cameraData.scaledWidth;
			renderTextureDescriptor.height = cameraData.scaledHeight;
			if (camera.cameraType == CameraType.SceneView && !isHdrEnabled)
			{
				renderTextureDescriptor.graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.LDR);
			}
		}
		renderTextureDescriptor.enableRandomWrite = false;
		renderTextureDescriptor.bindMS = false;
		renderTextureDescriptor.useDynamicScale = camera.allowDynamicResolution;
		renderTextureDescriptor.msaaSamples = SystemInfo.GetRenderTextureSupportedMSAASampleCount(renderTextureDescriptor);
		if (!SystemInfo.supportsStoreAndResolveAction)
		{
			renderTextureDescriptor.msaaSamples = 1;
		}
		return renderTextureDescriptor;
	}

	public static void GetLightAttenuationAndSpotDirection(LightType lightType, float lightRange, Matrix4x4 lightLocalToWorldMatrix, float spotAngle, float? innerSpotAngle, out Vector4 lightAttenuation, out Vector4 lightSpotDir)
	{
		lightAttenuation = k_DefaultLightAttenuation;
		lightSpotDir = k_DefaultLightSpotDirection;
		if (lightType != LightType.Directional)
		{
			GetPunctualLightDistanceAttenuation(lightRange, ref lightAttenuation);
			if (lightType == LightType.Spot)
			{
				GetSpotDirection(ref lightLocalToWorldMatrix, out lightSpotDir);
				GetSpotAngleAttenuation(spotAngle, innerSpotAngle, ref lightAttenuation);
			}
		}
	}

	internal static void GetPunctualLightDistanceAttenuation(float lightRange, ref Vector4 lightAttenuation)
	{
		float num = lightRange * lightRange;
		float num2 = 0.64000005f * num - num;
		float y = (0f - num) / num2;
		float x = 1f / Mathf.Max(0.0001f, num);
		lightAttenuation.x = x;
		lightAttenuation.y = y;
	}

	internal static void GetSpotAngleAttenuation(float spotAngle, float? innerSpotAngle, ref Vector4 lightAttenuation)
	{
		float num = Mathf.Cos(MathF.PI / 180f * spotAngle * 0.5f);
		float num2 = ((!innerSpotAngle.HasValue) ? Mathf.Cos(2f * Mathf.Atan(Mathf.Tan(spotAngle * 0.5f * (MathF.PI / 180f)) * 46f / 64f) * 0.5f) : Mathf.Cos(innerSpotAngle.Value * (MathF.PI / 180f) * 0.5f));
		float num3 = Mathf.Max(0.001f, num2 - num);
		float num4 = 1f / num3;
		float w = (0f - num) * num4;
		lightAttenuation.z = num4;
		lightAttenuation.w = w;
	}

	internal static void GetSpotDirection(ref Matrix4x4 lightLocalToWorldMatrix, out Vector4 lightSpotDir)
	{
		Vector4 column = lightLocalToWorldMatrix.GetColumn(2);
		lightSpotDir = new Vector4(0f - column.x, 0f - column.y, 0f - column.z, 0f);
	}

	public static void InitializeLightConstants_Common(NativeArray<VisibleLight> lights, int lightIndex, out Vector4 lightPos, out Vector4 lightColor, out Vector4 lightAttenuation, out Vector4 lightSpotDir, out Vector4 lightOcclusionProbeChannel)
	{
		lightPos = k_DefaultLightPosition;
		lightColor = k_DefaultLightColor;
		lightOcclusionProbeChannel = k_DefaultLightsProbeChannel;
		lightAttenuation = k_DefaultLightAttenuation;
		lightSpotDir = k_DefaultLightSpotDirection;
		if (lightIndex < 0)
		{
			return;
		}
		ref VisibleLight reference = ref lights.UnsafeElementAtMutable(lightIndex);
		Light light = reference.light;
		Matrix4x4 lightLocalToWorldMatrix = reference.localToWorldMatrix;
		LightType lightType = reference.lightType;
		if (lightType == LightType.Directional)
		{
			Vector4 vector = -lightLocalToWorldMatrix.GetColumn(2);
			lightPos = new Vector4(vector.x, vector.y, vector.z, 0f);
		}
		else
		{
			Vector4 column = lightLocalToWorldMatrix.GetColumn(3);
			lightPos = new Vector4(column.x, column.y, column.z, 1f);
			GetPunctualLightDistanceAttenuation(reference.range, ref lightAttenuation);
			if (lightType == LightType.Spot)
			{
				GetSpotAngleAttenuation(reference.spotAngle, light?.innerSpotAngle, ref lightAttenuation);
				GetSpotDirection(ref lightLocalToWorldMatrix, out lightSpotDir);
			}
		}
		lightColor = reference.finalColor;
		if (light != null && light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && 0 <= light.bakingOutput.occlusionMaskChannel && light.bakingOutput.occlusionMaskChannel < 4)
		{
			lightOcclusionProbeChannel[light.bakingOutput.occlusionMaskChannel] = 1f;
		}
	}

	private static void RecordRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		renderingData.cameraData.renderer.RecordRenderGraph(renderGraph, context, ref renderingData);
	}

	private static void RecordAndExecuteRenderGraph(RenderGraph renderGraph, ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		Camera camera = renderingData.cameraData.camera;
		RenderGraphParameters parameters = new RenderGraphParameters
		{
			executionName = Profiling.TryGetOrAddCameraSampler(camera).name,
			commandBuffer = commandBuffer,
			scriptableRenderContext = context,
			currentFrameIndex = Time.frameCount
		};
		using (renderGraph.RecordAndExecute(in parameters))
		{
			RecordRenderGraph(renderGraph, context, ref renderingData);
		}
	}
}
