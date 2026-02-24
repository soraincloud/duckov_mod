using System;
using Unity.Collections;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

internal class DeferredLights
{
	internal static class ShaderConstants
	{
		public static readonly int _LitStencilRef = Shader.PropertyToID("_LitStencilRef");

		public static readonly int _LitStencilReadMask = Shader.PropertyToID("_LitStencilReadMask");

		public static readonly int _LitStencilWriteMask = Shader.PropertyToID("_LitStencilWriteMask");

		public static readonly int _SimpleLitStencilRef = Shader.PropertyToID("_SimpleLitStencilRef");

		public static readonly int _SimpleLitStencilReadMask = Shader.PropertyToID("_SimpleLitStencilReadMask");

		public static readonly int _SimpleLitStencilWriteMask = Shader.PropertyToID("_SimpleLitStencilWriteMask");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilReadMask = Shader.PropertyToID("_StencilReadMask");

		public static readonly int _StencilWriteMask = Shader.PropertyToID("_StencilWriteMask");

		public static readonly int _LitPunctualStencilRef = Shader.PropertyToID("_LitPunctualStencilRef");

		public static readonly int _LitPunctualStencilReadMask = Shader.PropertyToID("_LitPunctualStencilReadMask");

		public static readonly int _LitPunctualStencilWriteMask = Shader.PropertyToID("_LitPunctualStencilWriteMask");

		public static readonly int _SimpleLitPunctualStencilRef = Shader.PropertyToID("_SimpleLitPunctualStencilRef");

		public static readonly int _SimpleLitPunctualStencilReadMask = Shader.PropertyToID("_SimpleLitPunctualStencilReadMask");

		public static readonly int _SimpleLitPunctualStencilWriteMask = Shader.PropertyToID("_SimpleLitPunctualStencilWriteMask");

		public static readonly int _LitDirStencilRef = Shader.PropertyToID("_LitDirStencilRef");

		public static readonly int _LitDirStencilReadMask = Shader.PropertyToID("_LitDirStencilReadMask");

		public static readonly int _LitDirStencilWriteMask = Shader.PropertyToID("_LitDirStencilWriteMask");

		public static readonly int _SimpleLitDirStencilRef = Shader.PropertyToID("_SimpleLitDirStencilRef");

		public static readonly int _SimpleLitDirStencilReadMask = Shader.PropertyToID("_SimpleLitDirStencilReadMask");

		public static readonly int _SimpleLitDirStencilWriteMask = Shader.PropertyToID("_SimpleLitDirStencilWriteMask");

		public static readonly int _ClearStencilRef = Shader.PropertyToID("_ClearStencilRef");

		public static readonly int _ClearStencilReadMask = Shader.PropertyToID("_ClearStencilReadMask");

		public static readonly int _ClearStencilWriteMask = Shader.PropertyToID("_ClearStencilWriteMask");

		public static readonly int _ScreenToWorld = Shader.PropertyToID("_ScreenToWorld");

		public static int _MainLightPosition = Shader.PropertyToID("_MainLightPosition");

		public static int _MainLightColor = Shader.PropertyToID("_MainLightColor");

		public static int _MainLightLayerMask = Shader.PropertyToID("_MainLightLayerMask");

		public static int _SpotLightScale = Shader.PropertyToID("_SpotLightScale");

		public static int _SpotLightBias = Shader.PropertyToID("_SpotLightBias");

		public static int _SpotLightGuard = Shader.PropertyToID("_SpotLightGuard");

		public static int _LightPosWS = Shader.PropertyToID("_LightPosWS");

		public static int _LightColor = Shader.PropertyToID("_LightColor");

		public static int _LightAttenuation = Shader.PropertyToID("_LightAttenuation");

		public static int _LightOcclusionProbInfo = Shader.PropertyToID("_LightOcclusionProbInfo");

		public static int _LightDirection = Shader.PropertyToID("_LightDirection");

		public static int _LightFlags = Shader.PropertyToID("_LightFlags");

		public static int _ShadowLightIndex = Shader.PropertyToID("_ShadowLightIndex");

		public static int _LightLayerMask = Shader.PropertyToID("_LightLayerMask");

		public static int _CookieLightIndex = Shader.PropertyToID("_CookieLightIndex");
	}

	internal enum StencilDeferredPasses
	{
		StencilVolume,
		PunctualLit,
		PunctualSimpleLit,
		DirectionalLit,
		DirectionalSimpleLit,
		ClearStencilPartial,
		Fog,
		SSAOOnly
	}

	internal struct InitParams
	{
		public Material stencilDeferredMaterial;

		public LightCookieManager lightCookieManager;
	}

	internal static readonly string[] k_GBufferNames = new string[7] { "_GBuffer0", "_GBuffer1", "_GBuffer2", "_GBuffer3", "_GBuffer4", "_GBuffer5", "_GBuffer6" };

	private static readonly string[] k_StencilDeferredPassNames = new string[8] { "Stencil Volume", "Deferred Punctual Light (Lit)", "Deferred Punctual Light (SimpleLit)", "Deferred Directional Light (Lit)", "Deferred Directional Light (SimpleLit)", "ClearStencilPartial", "Fog", "SSAOOnly" };

	private static readonly ushort k_InvalidLightOffset = ushort.MaxValue;

	private static readonly string k_SetupLights = "SetupLights";

	private static readonly string k_DeferredPass = "Deferred Pass";

	private static readonly string k_DeferredStencilPass = "Deferred Shading (Stencil)";

	private static readonly string k_DeferredFogPass = "Deferred Fog";

	private static readonly string k_ClearStencilPartial = "Clear Stencil Partial";

	private static readonly string k_SetupLightConstants = "Setup Light Constants";

	private static readonly float kStencilShapeGuard = 1.06067f;

	private static readonly ProfilingSampler m_ProfilingSetupLights = new ProfilingSampler(k_SetupLights);

	private static readonly ProfilingSampler m_ProfilingDeferredPass = new ProfilingSampler(k_DeferredPass);

	private static readonly ProfilingSampler m_ProfilingSetupLightConstants = new ProfilingSampler(k_SetupLightConstants);

	private RTHandle[] GbufferRTHandles;

	private NativeArray<ushort> m_stencilVisLights;

	private NativeArray<ushort> m_stencilVisLightOffsets;

	private AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;

	private Mesh m_SphereMesh;

	private Mesh m_HemisphereMesh;

	private Mesh m_FullscreenMesh;

	private Material m_StencilDeferredMaterial;

	private int[] m_StencilDeferredPasses;

	private Matrix4x4[] m_ScreenToWorld = new Matrix4x4[2];

	private ProfilingSampler m_ProfilingSamplerDeferredStencilPass = new ProfilingSampler(k_DeferredStencilPass);

	private ProfilingSampler m_ProfilingSamplerDeferredFogPass = new ProfilingSampler(k_DeferredFogPass);

	private ProfilingSampler m_ProfilingSamplerClearStencilPartialPass = new ProfilingSampler(k_ClearStencilPartial);

	private LightCookieManager m_LightCookieManager;

	internal int GBufferAlbedoIndex => 0;

	internal int GBufferSpecularMetallicIndex => 1;

	internal int GBufferNormalSmoothnessIndex => 2;

	internal int GBufferLightingIndex => 3;

	internal int GbufferDepthIndex
	{
		get
		{
			if (!UseRenderPass)
			{
				return -1;
			}
			return GBufferLightingIndex + 1;
		}
	}

	internal int GBufferRenderingLayers
	{
		get
		{
			if (!UseRenderingLayers)
			{
				return -1;
			}
			return GBufferLightingIndex + (UseRenderPass ? 1 : 0) + 1;
		}
	}

	internal int GBufferShadowMask
	{
		get
		{
			if (!UseShadowMask)
			{
				return -1;
			}
			return GBufferLightingIndex + (UseRenderPass ? 1 : 0) + (UseRenderingLayers ? 1 : 0) + 1;
		}
	}

	internal int GBufferSliceCount => 4 + (UseRenderPass ? 1 : 0) + (UseShadowMask ? 1 : 0) + (UseRenderingLayers ? 1 : 0);

	internal bool UseShadowMask => MixedLightingSetup != MixedLightingSetup.None;

	internal bool UseRenderingLayers
	{
		get
		{
			if (!UseLightLayers)
			{
				return UseDecalLayers;
			}
			return true;
		}
	}

	internal RenderingLayerUtils.MaskSize RenderingLayerMaskSize { get; set; }

	internal bool UseDecalLayers { get; set; }

	internal bool UseLightLayers => UniversalRenderPipeline.asset.useRenderingLayers;

	internal bool UseRenderPass { get; set; }

	internal bool HasDepthPrepass { get; set; }

	internal bool HasNormalPrepass { get; set; }

	internal bool HasRenderingLayerPrepass { get; set; }

	internal bool IsOverlay { get; set; }

	internal bool AccurateGbufferNormals { get; set; }

	internal MixedLightingSetup MixedLightingSetup { get; set; }

	internal bool UseJobSystem { get; set; }

	internal int RenderWidth { get; set; }

	internal int RenderHeight { get; set; }

	internal RTHandle[] GbufferAttachments { get; set; }

	internal TextureHandle[] GbufferTextureHandles { get; set; }

	internal RTHandle[] DeferredInputAttachments { get; set; }

	internal bool[] DeferredInputIsTransient { get; set; }

	internal RTHandle DepthAttachment { get; set; }

	internal RTHandle DepthCopyTexture { get; set; }

	internal GraphicsFormat[] GbufferFormats { get; set; }

	internal RTHandle DepthAttachmentHandle { get; set; }

	internal GraphicsFormat GetGBufferFormat(int index)
	{
		if (index == GBufferAlbedoIndex)
		{
			if (QualitySettings.activeColorSpace != ColorSpace.Linear)
			{
				return GraphicsFormat.R8G8B8A8_UNorm;
			}
			return GraphicsFormat.R8G8B8A8_SRGB;
		}
		if (index == GBufferSpecularMetallicIndex)
		{
			return GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (index == GBufferNormalSmoothnessIndex)
		{
			if (!AccurateGbufferNormals)
			{
				return DepthNormalOnlyPass.GetGraphicsFormat();
			}
			return GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (index == GBufferLightingIndex)
		{
			return GraphicsFormat.None;
		}
		if (index == GbufferDepthIndex)
		{
			return GraphicsFormat.R32_SFloat;
		}
		if (index == GBufferShadowMask)
		{
			return GraphicsFormat.B8G8R8A8_UNorm;
		}
		if (index == GBufferRenderingLayers)
		{
			return RenderingLayerUtils.GetFormat(RenderingLayerMaskSize);
		}
		return GraphicsFormat.None;
	}

	internal DeferredLights(InitParams initParams, bool useNativeRenderPass = false)
	{
		DeferredConfig.IsOpenGL = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3;
		DeferredConfig.IsDX10 = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 && SystemInfo.graphicsShaderLevel <= 40;
		m_StencilDeferredMaterial = initParams.stencilDeferredMaterial;
		m_StencilDeferredPasses = new int[k_StencilDeferredPassNames.Length];
		InitStencilDeferredMaterial();
		AccurateGbufferNormals = true;
		UseJobSystem = true;
		UseRenderPass = useNativeRenderPass;
		m_LightCookieManager = initParams.lightCookieManager;
	}

	internal void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera camera = renderingData.cameraData.camera;
		RenderWidth = (camera.allowDynamicResolution ? Mathf.CeilToInt(ScalableBufferManager.widthScaleFactor * (float)renderingData.cameraData.cameraTargetDescriptor.width) : renderingData.cameraData.cameraTargetDescriptor.width);
		RenderHeight = (camera.allowDynamicResolution ? Mathf.CeilToInt(ScalableBufferManager.heightScaleFactor * (float)renderingData.cameraData.cameraTargetDescriptor.height) : renderingData.cameraData.cameraTargetDescriptor.height);
		PrecomputeLights(out m_stencilVisLights, out m_stencilVisLightOffsets, ref renderingData.lightData.visibleLights, renderingData.lightData.additionalLightsCount != 0 || renderingData.lightData.mainLightIndex >= 0, renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.orthographic, renderingData.cameraData.camera.nearClipPlane);
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingSetupLightConstants))
		{
			SetupShaderLightConstants(commandBuffer, ref renderingData);
			bool supportsMixedLighting = renderingData.lightData.supportsMixedLighting;
			CoreUtils.SetKeyword(commandBuffer, "_GBUFFER_NORMALS_OCT", AccurateGbufferNormals);
			bool flag = supportsMixedLighting && MixedLightingSetup == MixedLightingSetup.ShadowMask;
			bool flag2 = flag && QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask;
			bool flag3 = supportsMixedLighting && MixedLightingSetup == MixedLightingSetup.Subtractive;
			CoreUtils.SetKeyword(commandBuffer, "LIGHTMAP_SHADOW_MIXING", flag3 || flag2);
			CoreUtils.SetKeyword(commandBuffer, "SHADOWS_SHADOWMASK", flag);
			CoreUtils.SetKeyword(commandBuffer, "_MIXED_LIGHTING_SUBTRACTIVE", flag3);
			CoreUtils.SetKeyword(commandBuffer, "_RENDER_PASS_ENABLED", UseRenderPass && renderingData.cameraData.cameraType == CameraType.Game);
			CoreUtils.SetKeyword(commandBuffer, "_LIGHT_LAYERS", UseLightLayers && !CoreUtils.IsSceneLightingDisabled(camera));
			RenderingLayerUtils.SetupProperties(commandBuffer, RenderingLayerMaskSize);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	internal void ResolveMixedLightingMode(ref RenderingData renderingData)
	{
		MixedLightingSetup = MixedLightingSetup.None;
		if (renderingData.lightData.supportsMixedLighting)
		{
			NativeArray<VisibleLight> visibleLights = renderingData.lightData.visibleLights;
			for (int i = 0; i < renderingData.lightData.visibleLights.Length; i++)
			{
				if (MixedLightingSetup != MixedLightingSetup.None)
				{
					break;
				}
				Light light = visibleLights.UnsafeElementAtMutable(i).light;
				if (light != null && light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && light.shadows != LightShadows.None)
				{
					switch (light.bakingOutput.mixedLightingMode)
					{
					case MixedLightingMode.Subtractive:
						MixedLightingSetup = MixedLightingSetup.Subtractive;
						break;
					case MixedLightingMode.Shadowmask:
						MixedLightingSetup = MixedLightingSetup.ShadowMask;
						break;
					}
				}
			}
		}
		CreateGbufferResources();
	}

	internal void DisableFramebufferFetchInput()
	{
		UseRenderPass = false;
		CreateGbufferResources();
	}

	internal void ReleaseGbufferResources()
	{
		if (GbufferRTHandles != null)
		{
			for (int i = 0; i < GbufferRTHandles.Length; i++)
			{
				RTHandles.Release(GbufferRTHandles[i]);
			}
		}
	}

	internal void ReAllocateGBufferIfNeeded(RenderTextureDescriptor gbufferSlice, int gbufferIndex)
	{
		if (GbufferRTHandles != null && GbufferRTHandles[gbufferIndex].GetInstanceID() == GbufferAttachments[gbufferIndex].GetInstanceID())
		{
			gbufferSlice.depthBufferBits = 0;
			gbufferSlice.stencilFormat = GraphicsFormat.None;
			gbufferSlice.graphicsFormat = GetGBufferFormat(gbufferIndex);
			RenderingUtils.ReAllocateIfNeeded(ref GbufferRTHandles[gbufferIndex], in gbufferSlice, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, k_GBufferNames[gbufferIndex]);
			GbufferAttachments[gbufferIndex] = GbufferRTHandles[gbufferIndex];
		}
	}

	internal void CreateGbufferResources()
	{
		int gBufferSliceCount = GBufferSliceCount;
		if (GbufferRTHandles == null || GbufferRTHandles.Length != gBufferSliceCount)
		{
			ReleaseGbufferResources();
			GbufferAttachments = new RTHandle[gBufferSliceCount];
			GbufferRTHandles = new RTHandle[gBufferSliceCount];
			GbufferFormats = new GraphicsFormat[gBufferSliceCount];
			GbufferTextureHandles = new TextureHandle[gBufferSliceCount];
			for (int i = 0; i < gBufferSliceCount; i++)
			{
				GbufferRTHandles[i] = RTHandles.Alloc(k_GBufferNames[i], k_GBufferNames[i]);
				GbufferAttachments[i] = GbufferRTHandles[i];
				GbufferFormats[i] = GetGBufferFormat(i);
			}
		}
	}

	internal void UpdateDeferredInputAttachments()
	{
		DeferredInputAttachments[0] = GbufferAttachments[0];
		DeferredInputAttachments[1] = GbufferAttachments[1];
		DeferredInputAttachments[2] = GbufferAttachments[2];
		DeferredInputAttachments[3] = GbufferAttachments[4];
		if (UseShadowMask)
		{
			DeferredInputAttachments[4] = GbufferAttachments[GBufferShadowMask];
		}
	}

	internal bool IsRuntimeSupportedThisFrame()
	{
		if (GBufferSliceCount <= SystemInfo.supportedRenderTargetCount && !DeferredConfig.IsOpenGL)
		{
			return !DeferredConfig.IsDX10;
		}
		return false;
	}

	public void Setup(ref RenderingData renderingData, AdditionalLightsShadowCasterPass additionalLightsShadowCasterPass, bool hasDepthPrepass, bool hasNormalPrepass, bool hasRenderingLayerPrepass, RTHandle depthCopyTexture, RTHandle depthAttachment, RTHandle colorAttachment)
	{
		m_AdditionalLightsShadowCasterPass = additionalLightsShadowCasterPass;
		HasDepthPrepass = hasDepthPrepass;
		HasNormalPrepass = hasNormalPrepass;
		HasRenderingLayerPrepass = hasRenderingLayerPrepass;
		DepthCopyTexture = depthCopyTexture;
		GbufferAttachments[GBufferLightingIndex] = colorAttachment;
		DepthAttachment = depthAttachment;
		int num = 4 + (UseShadowMask ? 1 : 0);
		if ((DeferredInputAttachments == null && UseRenderPass && GbufferAttachments.Length >= 3) || (DeferredInputAttachments != null && num != DeferredInputAttachments.Length))
		{
			DeferredInputAttachments = new RTHandle[num];
			DeferredInputIsTransient = new bool[num];
			int num2 = 0;
			int num3 = 0;
			while (num3 < num)
			{
				if (num2 == GBufferLightingIndex)
				{
					num2++;
				}
				DeferredInputAttachments[num3] = GbufferAttachments[num2];
				DeferredInputIsTransient[num3] = num2 != GbufferDepthIndex;
				num3++;
				num2++;
			}
		}
		DepthAttachmentHandle = DepthAttachment;
	}

	internal void Setup(AdditionalLightsShadowCasterPass additionalLightsShadowCasterPass)
	{
		m_AdditionalLightsShadowCasterPass = additionalLightsShadowCasterPass;
	}

	public void OnCameraCleanup(CommandBuffer cmd)
	{
		CoreUtils.SetKeyword(cmd, "_GBUFFER_NORMALS_OCT", state: false);
		if (m_stencilVisLights.IsCreated)
		{
			m_stencilVisLights.Dispose();
		}
		if (m_stencilVisLightOffsets.IsCreated)
		{
			m_stencilVisLightOffsets.Dispose();
		}
	}

	internal static StencilState OverwriteStencil(StencilState s, int stencilWriteMask)
	{
		if (!s.enabled)
		{
			return new StencilState(enabled: true, 0, (byte)stencilWriteMask, CompareFunction.Always, StencilOp.Replace, StencilOp.Keep, StencilOp.Keep, CompareFunction.Always, StencilOp.Replace, StencilOp.Keep, StencilOp.Keep);
		}
		CompareFunction compareFunctionFront = ((s.compareFunctionFront != CompareFunction.Disabled) ? s.compareFunctionFront : CompareFunction.Always);
		CompareFunction compareFunctionBack = ((s.compareFunctionBack != CompareFunction.Disabled) ? s.compareFunctionBack : CompareFunction.Always);
		StencilOp passOperationFront = s.passOperationFront;
		StencilOp failOperationFront = s.failOperationFront;
		StencilOp zFailOperationFront = s.zFailOperationFront;
		StencilOp passOperationBack = s.passOperationBack;
		StencilOp failOperationBack = s.failOperationBack;
		StencilOp zFailOperationBack = s.zFailOperationBack;
		return new StencilState(enabled: true, (byte)(s.readMask & 0xF), (byte)(s.writeMask | stencilWriteMask), compareFunctionFront, passOperationFront, failOperationFront, zFailOperationFront, compareFunctionBack, passOperationBack, failOperationBack, zFailOperationBack);
	}

	internal static RenderStateBlock OverwriteStencil(RenderStateBlock block, int stencilWriteMask, int stencilRef)
	{
		if (!block.stencilState.enabled)
		{
			block.stencilState = new StencilState(enabled: true, 0, (byte)stencilWriteMask, CompareFunction.Always, StencilOp.Replace, StencilOp.Keep, StencilOp.Keep, CompareFunction.Always, StencilOp.Replace, StencilOp.Keep, StencilOp.Keep);
		}
		else
		{
			StencilState stencilState = block.stencilState;
			CompareFunction compareFunctionFront = ((stencilState.compareFunctionFront != CompareFunction.Disabled) ? stencilState.compareFunctionFront : CompareFunction.Always);
			CompareFunction compareFunctionBack = ((stencilState.compareFunctionBack != CompareFunction.Disabled) ? stencilState.compareFunctionBack : CompareFunction.Always);
			StencilOp passOperationFront = stencilState.passOperationFront;
			StencilOp failOperationFront = stencilState.failOperationFront;
			StencilOp zFailOperationFront = stencilState.zFailOperationFront;
			StencilOp passOperationBack = stencilState.passOperationBack;
			StencilOp failOperationBack = stencilState.failOperationBack;
			StencilOp zFailOperationBack = stencilState.zFailOperationBack;
			block.stencilState = new StencilState(enabled: true, (byte)(stencilState.readMask & 0xF), (byte)(stencilState.writeMask | stencilWriteMask), compareFunctionFront, passOperationFront, failOperationFront, zFailOperationFront, compareFunctionBack, passOperationBack, failOperationBack, zFailOperationBack);
		}
		block.mask |= RenderStateMask.Stencil;
		block.stencilReference = (block.stencilReference & 0xF) | stencilRef;
		return block;
	}

	internal void ClearStencilPartial(CommandBuffer cmd)
	{
		if (m_FullscreenMesh == null)
		{
			m_FullscreenMesh = CreateFullscreenMesh();
		}
		using (new ProfilingScope(cmd, m_ProfilingSamplerClearStencilPartialPass))
		{
			cmd.DrawMesh(m_FullscreenMesh, Matrix4x4.identity, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[5]);
		}
	}

	internal void ExecuteDeferredPass(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_StencilDeferredPasses[0] < 0)
		{
			InitStencilDeferredMaterial();
		}
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, m_ProfilingDeferredPass))
		{
			CoreUtils.SetKeyword(commandBuffer, "_DEFERRED_MIXED_LIGHTING", UseShadowMask);
			SetupMatrixConstants(commandBuffer, ref renderingData);
			if (!HasStencilLightsOfType(LightType.Directional))
			{
				RenderSSAOBeforeShading(commandBuffer, ref renderingData);
			}
			RenderStencilLights(context, commandBuffer, ref renderingData);
			CoreUtils.SetKeyword(commandBuffer, "_DEFERRED_MIXED_LIGHTING", state: false);
			RenderFog(context, commandBuffer, ref renderingData);
		}
		CoreUtils.SetKeyword(commandBuffer, "_ADDITIONAL_LIGHT_SHADOWS", renderingData.shadowData.isKeywordAdditionalLightShadowsEnabled);
		ShadowUtils.SetSoftShadowQualityShaderKeywords(commandBuffer, ref renderingData.shadowData);
		CoreUtils.SetKeyword(commandBuffer, "_LIGHT_COOKIES", m_LightCookieManager != null && m_LightCookieManager.IsKeywordLightCookieEnabled);
	}

	private void SetupShaderLightConstants(CommandBuffer cmd, ref RenderingData renderingData)
	{
		SetupMainLightConstants(cmd, ref renderingData.lightData);
	}

	private void SetupMainLightConstants(CommandBuffer cmd, ref LightData lightData)
	{
		if (lightData.mainLightIndex >= 0)
		{
			UniversalRenderPipeline.InitializeLightConstants_Common(lightData.visibleLights, lightData.mainLightIndex, out var lightPos, out var lightColor, out var _, out var _, out var _);
			uint value = RenderingLayerUtils.ToValidRenderingLayers(lightData.visibleLights[lightData.mainLightIndex].light.GetUniversalAdditionalLightData().renderingLayers);
			cmd.SetGlobalVector(ShaderConstants._MainLightPosition, lightPos);
			cmd.SetGlobalVector(ShaderConstants._MainLightColor, lightColor);
			cmd.SetGlobalInt(ShaderConstants._MainLightLayerMask, (int)value);
		}
	}

	private void SetupMatrixConstants(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		int num = ((!cameraData.xr.enabled || !cameraData.xr.singlePassEnabled) ? 1 : 2);
		Matrix4x4[] screenToWorld = m_ScreenToWorld;
		for (int i = 0; i < num; i++)
		{
			Matrix4x4 projectionMatrix = cameraData.GetProjectionMatrix(i);
			Matrix4x4 viewMatrix = cameraData.GetViewMatrix(i);
			Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, renderIntoTexture: false);
			Matrix4x4 matrix4x = new Matrix4x4(new Vector4(0.5f * (float)RenderWidth, 0f, 0f, 0f), new Vector4(0f, 0.5f * (float)RenderHeight, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0.5f * (float)RenderWidth, 0.5f * (float)RenderHeight, 0f, 1f));
			Matrix4x4 matrix4x2 = Matrix4x4.identity;
			if (DeferredConfig.IsOpenGL)
			{
				matrix4x2 = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 0.5f, 0f), new Vector4(0f, 0f, 0.5f, 1f));
			}
			screenToWorld[i] = Matrix4x4.Inverse(matrix4x * matrix4x2 * gPUProjectionMatrix * viewMatrix);
		}
		cmd.SetGlobalMatrixArray(ShaderConstants._ScreenToWorld, screenToWorld);
	}

	private void PrecomputeLights(out NativeArray<ushort> stencilVisLights, out NativeArray<ushort> stencilVisLightOffsets, ref NativeArray<VisibleLight> visibleLights, bool hasAdditionalLights, Matrix4x4 view, bool isOrthographic, float zNear)
	{
		if (!hasAdditionalLights)
		{
			stencilVisLights = new NativeArray<ushort>(0, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			stencilVisLightOffsets = new NativeArray<ushort>(5, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < 5; i++)
			{
				stencilVisLightOffsets[i] = k_InvalidLightOffset;
			}
			return;
		}
		NativeArray<int> nativeArray = new NativeArray<int>(5, Allocator.Temp);
		stencilVisLightOffsets = new NativeArray<ushort>(5, Allocator.Temp);
		int length = visibleLights.Length;
		for (ushort num = 0; num < length; num++)
		{
			int lightType = (int)visibleLights.UnsafeElementAtMutable(num).lightType;
			ushort value = (ushort)(stencilVisLightOffsets[lightType] + 1);
			stencilVisLightOffsets[lightType] = value;
		}
		int length2 = stencilVisLightOffsets[0] + stencilVisLightOffsets[1] + stencilVisLightOffsets[2];
		stencilVisLights = new NativeArray<ushort>(length2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		int j = 0;
		int num2 = 0;
		for (; j < stencilVisLightOffsets.Length; j++)
		{
			if (stencilVisLightOffsets[j] == 0)
			{
				stencilVisLightOffsets[j] = k_InvalidLightOffset;
				continue;
			}
			int num3 = stencilVisLightOffsets[j];
			stencilVisLightOffsets[j] = (ushort)num2;
			num2 += num3;
		}
		for (ushort num4 = 0; num4 < length; num4++)
		{
			ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(num4);
			int num5 = nativeArray[(int)reference.lightType]++;
			stencilVisLights[stencilVisLightOffsets[(int)reference.lightType] + num5] = num4;
		}
		nativeArray.Dispose();
	}

	private bool HasStencilLightsOfType(LightType type)
	{
		return m_stencilVisLightOffsets[(int)type] != k_InvalidLightOffset;
	}

	private void RenderStencilLights(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (m_stencilVisLights.Length == 0)
		{
			return;
		}
		if (m_StencilDeferredMaterial == null)
		{
			Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_StencilDeferredMaterial, GetType().Name);
			return;
		}
		using (new ProfilingScope(cmd, m_ProfilingSamplerDeferredStencilPass))
		{
			NativeArray<VisibleLight> visibleLights = renderingData.lightData.visibleLights;
			if (HasStencilLightsOfType(LightType.Directional))
			{
				RenderStencilDirectionalLights(cmd, ref renderingData, visibleLights, renderingData.lightData.mainLightIndex);
			}
			if (HasStencilLightsOfType(LightType.Point))
			{
				RenderStencilPointLights(cmd, ref renderingData, visibleLights);
			}
			if (HasStencilLightsOfType(LightType.Spot))
			{
				RenderStencilSpotLights(cmd, ref renderingData, visibleLights);
			}
		}
	}

	private void SetAdditionalLightsShadowsKeyword(ref CommandBuffer cmd, ref RenderingData renderingData, bool hasDeferredShadows)
	{
		bool additionalLightShadowsEnabled = renderingData.shadowData.additionalLightShadowsEnabled;
		bool flag = !renderingData.cameraData.renderer.stripShadowsOffVariants;
		bool state = additionalLightShadowsEnabled && (!flag || hasDeferredShadows);
		CoreUtils.SetKeyword(cmd, "_ADDITIONAL_LIGHT_SHADOWS", state);
	}

	private void RenderStencilDirectionalLights(CommandBuffer cmd, ref RenderingData renderingData, NativeArray<VisibleLight> visibleLights, int mainLightIndex)
	{
		if (m_FullscreenMesh == null)
		{
			m_FullscreenMesh = CreateFullscreenMesh();
		}
		cmd.EnableShaderKeyword("_DIRECTIONAL");
		bool state = true;
		for (int i = m_stencilVisLightOffsets[1]; i < m_stencilVisLights.Length; i++)
		{
			ushort num = m_stencilVisLights[i];
			ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(num);
			if (reference.lightType != LightType.Directional)
			{
				break;
			}
			Light light = reference.light;
			UniversalRenderPipeline.InitializeLightConstants_Common(visibleLights, num, out var lightPos, out var lightColor, out var _, out var _, out var _);
			int num2 = 0;
			if (light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed)
			{
				num2 |= 4;
			}
			uint value = RenderingLayerUtils.ToValidRenderingLayers(light.GetUniversalAdditionalLightData().renderingLayers);
			bool flag;
			if (num == mainLightIndex)
			{
				flag = (bool)light && light.shadows != LightShadows.None;
			}
			else
			{
				int num3 = ((m_AdditionalLightsShadowCasterPass != null) ? m_AdditionalLightsShadowCasterPass.GetShadowLightIndexFromLightIndex(num) : (-1));
				flag = (bool)light && light.shadows != LightShadows.None && num3 >= 0;
				cmd.SetGlobalInt(ShaderConstants._ShadowLightIndex, num3);
			}
			SetAdditionalLightsShadowsKeyword(ref cmd, ref renderingData, flag);
			bool hasSoftShadows = flag && renderingData.shadowData.supportsSoftShadows && light.shadows == LightShadows.Soft;
			ShadowUtils.SetPerLightSoftShadowKeyword(cmd, hasSoftShadows);
			CoreUtils.SetKeyword(cmd, "_DEFERRED_FIRST_LIGHT", state);
			CoreUtils.SetKeyword(cmd, "_DEFERRED_MAIN_LIGHT", num == mainLightIndex);
			cmd.SetGlobalVector(ShaderConstants._LightColor, lightColor);
			cmd.SetGlobalVector(ShaderConstants._LightDirection, lightPos);
			cmd.SetGlobalInt(ShaderConstants._LightFlags, num2);
			cmd.SetGlobalInt(ShaderConstants._LightLayerMask, (int)value);
			cmd.DrawMesh(m_FullscreenMesh, Matrix4x4.identity, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[3]);
			cmd.DrawMesh(m_FullscreenMesh, Matrix4x4.identity, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[4]);
			state = false;
		}
		cmd.DisableShaderKeyword("_DIRECTIONAL");
	}

	private void RenderStencilPointLights(CommandBuffer cmd, ref RenderingData renderingData, NativeArray<VisibleLight> visibleLights)
	{
		if (m_SphereMesh == null)
		{
			m_SphereMesh = CreateSphereMesh();
		}
		cmd.EnableShaderKeyword("_POINT");
		for (int i = m_stencilVisLightOffsets[2]; i < m_stencilVisLights.Length; i++)
		{
			ushort num = m_stencilVisLights[i];
			ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(num);
			if (reference.lightType != LightType.Point)
			{
				break;
			}
			Light light = reference.light;
			Vector3 vector = reference.localToWorldMatrix.GetColumn(3);
			Matrix4x4 matrix = new Matrix4x4(new Vector4(reference.range, 0f, 0f, 0f), new Vector4(0f, reference.range, 0f, 0f), new Vector4(0f, 0f, reference.range, 0f), new Vector4(vector.x, vector.y, vector.z, 1f));
			UniversalRenderPipeline.InitializeLightConstants_Common(visibleLights, num, out var lightPos, out var lightColor, out var lightAttenuation, out var _, out var lightOcclusionProbeChannel);
			uint value = RenderingLayerUtils.ToValidRenderingLayers(light.GetUniversalAdditionalLightData().renderingLayers);
			int num2 = 0;
			if (light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed)
			{
				num2 |= 4;
			}
			int num3 = ((m_AdditionalLightsShadowCasterPass != null) ? m_AdditionalLightsShadowCasterPass.GetShadowLightIndexFromLightIndex(num) : (-1));
			bool flag = (bool)light && light.shadows != LightShadows.None && num3 >= 0;
			SetAdditionalLightsShadowsKeyword(ref cmd, ref renderingData, flag);
			bool hasSoftShadows = flag && renderingData.shadowData.supportsSoftShadows && light.shadows == LightShadows.Soft;
			ShadowUtils.SetPerLightSoftShadowKeyword(cmd, hasSoftShadows);
			if (m_LightCookieManager != null)
			{
				int lightCookieShaderDataIndex = m_LightCookieManager.GetLightCookieShaderDataIndex(num);
				CoreUtils.SetKeyword(cmd, "_LIGHT_COOKIES", lightCookieShaderDataIndex >= 0);
				cmd.SetGlobalInt(ShaderConstants._CookieLightIndex, lightCookieShaderDataIndex);
			}
			cmd.SetGlobalVector(ShaderConstants._LightPosWS, lightPos);
			cmd.SetGlobalVector(ShaderConstants._LightColor, lightColor);
			cmd.SetGlobalVector(ShaderConstants._LightAttenuation, lightAttenuation);
			cmd.SetGlobalVector(ShaderConstants._LightOcclusionProbInfo, lightOcclusionProbeChannel);
			cmd.SetGlobalInt(ShaderConstants._LightFlags, num2);
			cmd.SetGlobalInt(ShaderConstants._ShadowLightIndex, num3);
			cmd.SetGlobalInt(ShaderConstants._LightLayerMask, (int)value);
			cmd.DrawMesh(m_SphereMesh, matrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[0]);
			cmd.DrawMesh(m_SphereMesh, matrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[1]);
			cmd.DrawMesh(m_SphereMesh, matrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[2]);
		}
		cmd.DisableShaderKeyword("_POINT");
	}

	private void RenderStencilSpotLights(CommandBuffer cmd, ref RenderingData renderingData, NativeArray<VisibleLight> visibleLights)
	{
		if (m_HemisphereMesh == null)
		{
			m_HemisphereMesh = CreateHemisphereMesh();
		}
		cmd.EnableShaderKeyword("_SPOT");
		for (int i = m_stencilVisLightOffsets[0]; i < m_stencilVisLights.Length; i++)
		{
			ushort num = m_stencilVisLights[i];
			ref VisibleLight reference = ref visibleLights.UnsafeElementAtMutable(num);
			if (reference.lightType != LightType.Spot)
			{
				break;
			}
			Light light = reference.light;
			float f = MathF.PI / 180f * reference.spotAngle * 0.5f;
			float num2 = Mathf.Cos(f);
			float num3 = Mathf.Sin(f);
			float num4 = Mathf.Lerp(1f, kStencilShapeGuard, num3);
			UniversalRenderPipeline.InitializeLightConstants_Common(visibleLights, num, out var lightPos, out var lightColor, out var lightAttenuation, out var lightSpotDir, out var lightOcclusionProbeChannel);
			uint value = RenderingLayerUtils.ToValidRenderingLayers(light.GetUniversalAdditionalLightData().renderingLayers);
			int num5 = 0;
			if (light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed)
			{
				num5 |= 4;
			}
			int num6 = ((m_AdditionalLightsShadowCasterPass != null) ? m_AdditionalLightsShadowCasterPass.GetShadowLightIndexFromLightIndex(num) : (-1));
			bool flag = (bool)light && light.shadows != LightShadows.None && num6 >= 0;
			SetAdditionalLightsShadowsKeyword(ref cmd, ref renderingData, flag);
			bool hasSoftShadows = flag && renderingData.shadowData.supportsSoftShadows && light.shadows == LightShadows.Soft;
			ShadowUtils.SetPerLightSoftShadowKeyword(cmd, hasSoftShadows);
			if (m_LightCookieManager != null)
			{
				int lightCookieShaderDataIndex = m_LightCookieManager.GetLightCookieShaderDataIndex(num);
				CoreUtils.SetKeyword(cmd, "_LIGHT_COOKIES", lightCookieShaderDataIndex >= 0);
				cmd.SetGlobalInt(ShaderConstants._CookieLightIndex, lightCookieShaderDataIndex);
			}
			cmd.SetGlobalVector(ShaderConstants._SpotLightScale, new Vector4(num3, num3, 1f - num2, reference.range));
			cmd.SetGlobalVector(ShaderConstants._SpotLightBias, new Vector4(0f, 0f, num2, 0f));
			cmd.SetGlobalVector(ShaderConstants._SpotLightGuard, new Vector4(num4, num4, num4, num2 * reference.range));
			cmd.SetGlobalVector(ShaderConstants._LightPosWS, lightPos);
			cmd.SetGlobalVector(ShaderConstants._LightColor, lightColor);
			cmd.SetGlobalVector(ShaderConstants._LightAttenuation, lightAttenuation);
			cmd.SetGlobalVector(ShaderConstants._LightDirection, new Vector3(lightSpotDir.x, lightSpotDir.y, lightSpotDir.z));
			cmd.SetGlobalVector(ShaderConstants._LightOcclusionProbInfo, lightOcclusionProbeChannel);
			cmd.SetGlobalInt(ShaderConstants._LightFlags, num5);
			cmd.SetGlobalInt(ShaderConstants._ShadowLightIndex, num6);
			cmd.SetGlobalInt(ShaderConstants._LightLayerMask, (int)value);
			cmd.DrawMesh(m_HemisphereMesh, reference.localToWorldMatrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[0]);
			cmd.DrawMesh(m_HemisphereMesh, reference.localToWorldMatrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[1]);
			cmd.DrawMesh(m_HemisphereMesh, reference.localToWorldMatrix, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[2]);
		}
		cmd.DisableShaderKeyword("_SPOT");
	}

	private void RenderSSAOBeforeShading(CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (m_FullscreenMesh == null)
		{
			m_FullscreenMesh = CreateFullscreenMesh();
		}
		cmd.DrawMesh(m_FullscreenMesh, Matrix4x4.identity, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[7]);
	}

	private void RenderFog(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData)
	{
		if (!RenderSettings.fog || renderingData.cameraData.camera.orthographic)
		{
			return;
		}
		if (m_FullscreenMesh == null)
		{
			m_FullscreenMesh = CreateFullscreenMesh();
		}
		using (new ProfilingScope(cmd, m_ProfilingSamplerDeferredFogPass))
		{
			cmd.DrawMesh(m_FullscreenMesh, Matrix4x4.identity, m_StencilDeferredMaterial, 0, m_StencilDeferredPasses[6]);
		}
	}

	private void InitStencilDeferredMaterial()
	{
		if (!(m_StencilDeferredMaterial == null))
		{
			for (int i = 0; i < k_StencilDeferredPassNames.Length; i++)
			{
				m_StencilDeferredPasses[i] = m_StencilDeferredMaterial.FindPass(k_StencilDeferredPassNames[i]);
			}
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._StencilRef, 0f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._StencilReadMask, 96f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._StencilWriteMask, 16f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._LitPunctualStencilRef, 48f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._LitPunctualStencilReadMask, 112f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._LitPunctualStencilWriteMask, 16f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._SimpleLitPunctualStencilRef, 80f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._SimpleLitPunctualStencilReadMask, 112f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._SimpleLitPunctualStencilWriteMask, 16f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._LitDirStencilRef, 32f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._LitDirStencilReadMask, 96f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._LitDirStencilWriteMask, 0f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._SimpleLitDirStencilRef, 64f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._SimpleLitDirStencilReadMask, 96f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._SimpleLitDirStencilWriteMask, 0f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._ClearStencilRef, 0f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._ClearStencilReadMask, 96f);
			m_StencilDeferredMaterial.SetFloat(ShaderConstants._ClearStencilWriteMask, 96f);
		}
	}

	private static Mesh CreateSphereMesh()
	{
		Vector3[] vertices = new Vector3[42]
		{
			new Vector3(0f, 0f, -1.07f),
			new Vector3(0.174f, -0.535f, -0.91f),
			new Vector3(-0.455f, -0.331f, -0.91f),
			new Vector3(0.562f, 0f, -0.91f),
			new Vector3(-0.455f, 0.331f, -0.91f),
			new Vector3(0.174f, 0.535f, -0.91f),
			new Vector3(-0.281f, -0.865f, -0.562f),
			new Vector3(0.736f, -0.535f, -0.562f),
			new Vector3(0.296f, -0.91f, -0.468f),
			new Vector3(-0.91f, 0f, -0.562f),
			new Vector3(-0.774f, -0.562f, -0.478f),
			new Vector3(0f, -1.07f, 0f),
			new Vector3(-0.629f, -0.865f, 0f),
			new Vector3(0.629f, -0.865f, 0f),
			new Vector3(-1.017f, -0.331f, 0f),
			new Vector3(0.957f, 0f, -0.478f),
			new Vector3(0.736f, 0.535f, -0.562f),
			new Vector3(1.017f, -0.331f, 0f),
			new Vector3(1.017f, 0.331f, 0f),
			new Vector3(-0.296f, -0.91f, 0.478f),
			new Vector3(0.281f, -0.865f, 0.562f),
			new Vector3(0.774f, -0.562f, 0.478f),
			new Vector3(-0.736f, -0.535f, 0.562f),
			new Vector3(0.91f, 0f, 0.562f),
			new Vector3(0.455f, -0.331f, 0.91f),
			new Vector3(-0.174f, -0.535f, 0.91f),
			new Vector3(0.629f, 0.865f, 0f),
			new Vector3(0.774f, 0.562f, 0.478f),
			new Vector3(0.455f, 0.331f, 0.91f),
			new Vector3(0f, 0f, 1.07f),
			new Vector3(-0.562f, 0f, 0.91f),
			new Vector3(-0.957f, 0f, 0.478f),
			new Vector3(0.281f, 0.865f, 0.562f),
			new Vector3(-0.174f, 0.535f, 0.91f),
			new Vector3(0.296f, 0.91f, -0.478f),
			new Vector3(-1.017f, 0.331f, 0f),
			new Vector3(-0.736f, 0.535f, 0.562f),
			new Vector3(-0.296f, 0.91f, 0.478f),
			new Vector3(0f, 1.07f, 0f),
			new Vector3(-0.281f, 0.865f, -0.562f),
			new Vector3(-0.774f, 0.562f, -0.478f),
			new Vector3(-0.629f, 0.865f, 0f)
		};
		int[] triangles = new int[240]
		{
			0, 1, 2, 0, 3, 1, 2, 4, 0, 0,
			5, 3, 0, 4, 5, 1, 6, 2, 3, 7,
			1, 1, 8, 6, 1, 7, 8, 9, 4, 2,
			2, 6, 10, 10, 9, 2, 8, 11, 6, 6,
			12, 10, 11, 12, 6, 7, 13, 8, 8, 13,
			11, 10, 14, 9, 10, 12, 14, 3, 15, 7,
			5, 16, 3, 3, 16, 15, 15, 17, 7, 17,
			13, 7, 16, 18, 15, 15, 18, 17, 11, 19,
			12, 13, 20, 11, 11, 20, 19, 17, 21, 13,
			13, 21, 20, 12, 19, 22, 12, 22, 14, 17,
			23, 21, 18, 23, 17, 21, 24, 20, 23, 24,
			21, 20, 25, 19, 19, 25, 22, 24, 25, 20,
			26, 18, 16, 18, 27, 23, 26, 27, 18, 28,
			24, 23, 27, 28, 23, 24, 29, 25, 28, 29,
			24, 25, 30, 22, 25, 29, 30, 14, 22, 31,
			22, 30, 31, 32, 28, 27, 26, 32, 27, 33,
			29, 28, 30, 29, 33, 33, 28, 32, 34, 26,
			16, 5, 34, 16, 14, 31, 35, 14, 35, 9,
			31, 30, 36, 30, 33, 36, 35, 31, 36, 37,
			33, 32, 36, 33, 37, 38, 32, 26, 34, 38,
			26, 38, 37, 32, 5, 39, 34, 39, 38, 34,
			4, 39, 5, 9, 40, 4, 9, 35, 40, 4,
			40, 39, 35, 36, 41, 41, 36, 37, 41, 37,
			38, 40, 35, 41, 40, 41, 39, 41, 38, 39
		};
		return new Mesh
		{
			indexFormat = IndexFormat.UInt16,
			vertices = vertices,
			triangles = triangles
		};
	}

	private static Mesh CreateHemisphereMesh()
	{
		Vector3[] vertices = new Vector3[42]
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0.92388f, 0.382683f, 0f),
			new Vector3(0.707107f, 0.707107f, 0f),
			new Vector3(0.382683f, 0.92388f, 0f),
			new Vector3(-0f, 1f, 0f),
			new Vector3(-0.382684f, 0.92388f, 0f),
			new Vector3(-0.707107f, 0.707107f, 0f),
			new Vector3(-0.92388f, 0.382683f, 0f),
			new Vector3(-1f, -0f, 0f),
			new Vector3(-0.92388f, -0.382683f, 0f),
			new Vector3(-0.707107f, -0.707107f, 0f),
			new Vector3(-0.382683f, -0.92388f, 0f),
			new Vector3(0f, -1f, 0f),
			new Vector3(0.382684f, -0.923879f, 0f),
			new Vector3(0.707107f, -0.707107f, 0f),
			new Vector3(0.92388f, -0.382683f, 0f),
			new Vector3(0f, 0f, 1f),
			new Vector3(0.707107f, 0f, 0.707107f),
			new Vector3(0f, -0.707107f, 0.707107f),
			new Vector3(0f, 0.707107f, 0.707107f),
			new Vector3(-0.707107f, 0f, 0.707107f),
			new Vector3(0.816497f, -0.408248f, 0.408248f),
			new Vector3(0.408248f, -0.408248f, 0.816497f),
			new Vector3(0.408248f, -0.816497f, 0.408248f),
			new Vector3(0.408248f, 0.816497f, 0.408248f),
			new Vector3(0.408248f, 0.408248f, 0.816497f),
			new Vector3(0.816497f, 0.408248f, 0.408248f),
			new Vector3(-0.816497f, 0.408248f, 0.408248f),
			new Vector3(-0.408248f, 0.408248f, 0.816497f),
			new Vector3(-0.408248f, 0.816497f, 0.408248f),
			new Vector3(-0.408248f, -0.816497f, 0.408248f),
			new Vector3(-0.408248f, -0.408248f, 0.816497f),
			new Vector3(-0.816497f, -0.408248f, 0.408248f),
			new Vector3(0f, -0.92388f, 0.382683f),
			new Vector3(0.92388f, 0f, 0.382683f),
			new Vector3(0f, -0.382683f, 0.92388f),
			new Vector3(0.382683f, 0f, 0.92388f),
			new Vector3(0f, 0.92388f, 0.382683f),
			new Vector3(0f, 0.382683f, 0.92388f),
			new Vector3(-0.92388f, 0f, 0.382683f),
			new Vector3(-0.382683f, 0f, 0.92388f)
		};
		int[] triangles = new int[240]
		{
			0, 2, 1, 0, 3, 2, 0, 4, 3, 0,
			5, 4, 0, 6, 5, 0, 7, 6, 0, 8,
			7, 0, 9, 8, 0, 10, 9, 0, 11, 10,
			0, 12, 11, 0, 13, 12, 0, 14, 13, 0,
			15, 14, 0, 16, 15, 0, 1, 16, 22, 23,
			24, 25, 26, 27, 28, 29, 30, 31, 32, 33,
			14, 24, 34, 35, 22, 16, 36, 23, 37, 2,
			27, 35, 38, 25, 4, 37, 26, 39, 6, 30,
			38, 40, 28, 8, 39, 29, 41, 10, 33, 40,
			34, 31, 12, 41, 32, 36, 15, 22, 24, 18,
			23, 22, 19, 24, 23, 3, 25, 27, 20, 26,
			25, 18, 27, 26, 7, 28, 30, 21, 29, 28,
			20, 30, 29, 11, 31, 33, 19, 32, 31, 21,
			33, 32, 13, 14, 34, 15, 24, 14, 19, 34,
			24, 1, 35, 16, 18, 22, 35, 15, 16, 22,
			17, 36, 37, 19, 23, 36, 18, 37, 23, 1,
			2, 35, 3, 27, 2, 18, 35, 27, 5, 38,
			4, 20, 25, 38, 3, 4, 25, 17, 37, 39,
			18, 26, 37, 20, 39, 26, 5, 6, 38, 7,
			30, 6, 20, 38, 30, 9, 40, 8, 21, 28,
			40, 7, 8, 28, 17, 39, 41, 20, 29, 39,
			21, 41, 29, 9, 10, 40, 11, 33, 10, 21,
			40, 33, 13, 34, 12, 19, 31, 34, 11, 12,
			31, 17, 41, 36, 21, 32, 41, 19, 36, 32
		};
		return new Mesh
		{
			indexFormat = IndexFormat.UInt16,
			vertices = vertices,
			triangles = triangles
		};
	}

	private static Mesh CreateFullscreenMesh()
	{
		Vector3[] vertices = new Vector3[3]
		{
			new Vector3(-1f, 1f, 0f),
			new Vector3(-1f, -3f, 0f),
			new Vector3(3f, 1f, 0f)
		};
		int[] triangles = new int[3] { 0, 1, 2 };
		return new Mesh
		{
			indexFormat = IndexFormat.UInt16,
			vertices = vertices,
			triangles = triangles
		};
	}
}
