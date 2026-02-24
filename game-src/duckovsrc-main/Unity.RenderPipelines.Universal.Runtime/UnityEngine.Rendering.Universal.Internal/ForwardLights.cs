using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal.Internal;

public class ForwardLights
{
	private static class LightConstantBuffer
	{
		public static int _MainLightPosition;

		public static int _MainLightColor;

		public static int _MainLightOcclusionProbesChannel;

		public static int _MainLightLayerMask;

		public static int _AdditionalLightsCount;

		public static int _AdditionalLightsPosition;

		public static int _AdditionalLightsColor;

		public static int _AdditionalLightsAttenuation;

		public static int _AdditionalLightsSpotDir;

		public static int _AdditionalLightOcclusionProbeChannel;

		public static int _AdditionalLightsLayerMasks;
	}

	internal struct InitParams
	{
		public LightCookieManager lightCookieManager;

		public bool forwardPlus;

		internal static InitParams Create()
		{
			LightCookieManager.Settings settings = LightCookieManager.Settings.Create();
			UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
			if ((bool)asset)
			{
				settings.atlas.format = asset.additionalLightsCookieFormat;
				settings.atlas.resolution = asset.additionalLightsCookieResolution;
			}
			InitParams result = default(InitParams);
			result.lightCookieManager = new LightCookieManager(ref settings);
			result.forwardPlus = false;
			return result;
		}
	}

	private int m_AdditionalLightsBufferId;

	private int m_AdditionalLightsIndicesId;

	private const string k_SetupLightConstants = "Setup Light Constants";

	private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Setup Light Constants");

	private static readonly ProfilingSampler m_ProfilingSamplerFPSetup = new ProfilingSampler("Forward+ Setup");

	private static readonly ProfilingSampler m_ProfilingSamplerFPComplete = new ProfilingSampler("Forward+ Complete");

	private static readonly ProfilingSampler m_ProfilingSamplerFPUpload = new ProfilingSampler("Forward+ Upload");

	private MixedLightingSetup m_MixedLightingSetup;

	private Vector4[] m_AdditionalLightPositions;

	private Vector4[] m_AdditionalLightColors;

	private Vector4[] m_AdditionalLightAttenuations;

	private Vector4[] m_AdditionalLightSpotDirections;

	private Vector4[] m_AdditionalLightOcclusionProbeChannels;

	private float[] m_AdditionalLightsLayerMasks;

	private bool m_UseStructuredBuffer;

	private bool m_UseForwardPlus;

	private int m_DirectionalLightCount;

	private int m_ActualTileWidth;

	private int2 m_TileResolution;

	private JobHandle m_CullingHandle;

	private NativeArray<uint> m_ZBins;

	private GraphicsBuffer m_ZBinsBuffer;

	private NativeArray<uint> m_TileMasks;

	private GraphicsBuffer m_TileMasksBuffer;

	private LightCookieManager m_LightCookieManager;

	private ReflectionProbeManager m_ReflectionProbeManager;

	private int m_WordsPerTile;

	private float m_ZBinScale;

	private float m_ZBinOffset;

	private int m_LightCount;

	private int m_BinCount;

	internal ReflectionProbeManager reflectionProbeManager => m_ReflectionProbeManager;

	public ForwardLights()
		: this(InitParams.Create())
	{
	}

	internal ForwardLights(InitParams initParams)
	{
		m_UseStructuredBuffer = RenderingUtils.useStructuredBuffer;
		m_UseForwardPlus = initParams.forwardPlus;
		LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
		LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
		LightConstantBuffer._MainLightOcclusionProbesChannel = Shader.PropertyToID("_MainLightOcclusionProbes");
		LightConstantBuffer._MainLightLayerMask = Shader.PropertyToID("_MainLightLayerMask");
		LightConstantBuffer._AdditionalLightsCount = Shader.PropertyToID("_AdditionalLightsCount");
		if (m_UseStructuredBuffer)
		{
			m_AdditionalLightsBufferId = Shader.PropertyToID("_AdditionalLightsBuffer");
			m_AdditionalLightsIndicesId = Shader.PropertyToID("_AdditionalLightsIndices");
		}
		else
		{
			LightConstantBuffer._AdditionalLightsPosition = Shader.PropertyToID("_AdditionalLightsPosition");
			LightConstantBuffer._AdditionalLightsColor = Shader.PropertyToID("_AdditionalLightsColor");
			LightConstantBuffer._AdditionalLightsAttenuation = Shader.PropertyToID("_AdditionalLightsAttenuation");
			LightConstantBuffer._AdditionalLightsSpotDir = Shader.PropertyToID("_AdditionalLightsSpotDir");
			LightConstantBuffer._AdditionalLightOcclusionProbeChannel = Shader.PropertyToID("_AdditionalLightsOcclusionProbes");
			LightConstantBuffer._AdditionalLightsLayerMasks = Shader.PropertyToID("_AdditionalLightsLayerMasks");
			int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
			m_AdditionalLightPositions = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightColors = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightAttenuations = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightSpotDirections = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightOcclusionProbeChannels = new Vector4[maxVisibleAdditionalLights];
			m_AdditionalLightsLayerMasks = new float[maxVisibleAdditionalLights];
		}
		if (m_UseForwardPlus)
		{
			CreateForwardPlusBuffers();
			m_ReflectionProbeManager = ReflectionProbeManager.Create();
		}
		m_LightCookieManager = initParams.lightCookieManager;
	}

	private void CreateForwardPlusBuffers()
	{
		m_ZBins = new NativeArray<uint>(UniversalRenderPipeline.maxZBinWords, Allocator.Persistent);
		m_ZBinsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, UniversalRenderPipeline.maxZBinWords / 4, UnsafeUtility.SizeOf<float4>());
		m_ZBinsBuffer.name = "URP Z-Bin Buffer";
		m_TileMasks = new NativeArray<uint>(UniversalRenderPipeline.maxTileWords, Allocator.Persistent);
		m_TileMasksBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Constant, UniversalRenderPipeline.maxTileWords / 4, UnsafeUtility.SizeOf<float4>());
		m_TileMasksBuffer.name = "URP Tile Buffer";
	}

	private static int AlignByteCount(int count, int align)
	{
		return align * ((count + align - 1) / align);
	}

	private void GetViewParams(Camera camera, float4x4 viewToClip, out float viewPlaneBot, out float viewPlaneTop, out float4 viewToViewportScaleBias)
	{
		float2 @float = math.float2(viewToClip[0][0], viewToClip[1][1]);
		float2 float2 = math.rcp(@float);
		float2 float3 = (camera.orthographic ? (-math.float2(viewToClip[3][0], viewToClip[3][1])) : math.float2(viewToClip[2][0], viewToClip[2][1]));
		viewPlaneBot = float3.y * float2.y - float2.y;
		viewPlaneTop = float3.y * float2.y + float2.y;
		viewToViewportScaleBias = math.float4(@float * 0.5f, -float3 * 0.5f + 0.5f);
	}

	internal unsafe void PreSetup(ref RenderingData renderingData)
	{
		if (!m_UseForwardPlus)
		{
			return;
		}
		using (new ProfilingScope(null, m_ProfilingSamplerFPSetup))
		{
			if (!m_CullingHandle.IsCompleted)
			{
				throw new InvalidOperationException("Forward+ jobs have not completed yet.");
			}
			if (m_TileMasks.Length != UniversalRenderPipeline.maxTileWords)
			{
				m_ZBins.Dispose();
				m_ZBinsBuffer.Dispose();
				m_TileMasks.Dispose();
				m_TileMasksBuffer.Dispose();
				CreateForwardPlusBuffers();
			}
			else
			{
				UnsafeUtility.MemClear(m_ZBins.GetUnsafePtr(), m_ZBins.Length * 4);
				UnsafeUtility.MemClear(m_TileMasks.GetUnsafePtr(), m_TileMasks.Length * 4);
			}
			ref CameraData cameraData = ref renderingData.cameraData;
			Camera camera = cameraData.camera;
			int2 @int = math.int2(cameraData.pixelWidth, cameraData.pixelHeight);
			int num = ((!cameraData.xr.enabled || !cameraData.xr.singlePassEnabled) ? 1 : 2);
			m_LightCount = renderingData.lightData.visibleLights.Length;
			int i;
			for (i = 0; i < m_LightCount && renderingData.lightData.visibleLights[i].lightType == LightType.Directional; i++)
			{
			}
			m_LightCount -= i;
			m_DirectionalLightCount = i;
			if (renderingData.lightData.mainLightIndex != -1 && m_DirectionalLightCount != 0)
			{
				m_DirectionalLightCount--;
			}
			NativeArray<VisibleLight> subArray = renderingData.lightData.visibleLights.GetSubArray(i, m_LightCount);
			NativeArray<VisibleReflectionProbe> visibleReflectionProbes = renderingData.cullResults.visibleReflectionProbes;
			int num2 = math.min(visibleReflectionProbes.Length, UniversalRenderPipeline.maxVisibleReflectionProbes);
			int num3 = subArray.Length + num2;
			m_WordsPerTile = (num3 + 31) / 32;
			m_ActualTileWidth = 4;
			do
			{
				m_ActualTileWidth <<= 1;
				m_TileResolution = (@int + m_ActualTileWidth - 1) / m_ActualTileWidth;
			}
			while (m_TileResolution.x * m_TileResolution.y * m_WordsPerTile * num > UniversalRenderPipeline.maxTileWords);
			if (!camera.orthographic)
			{
				m_ZBinScale = (float)(UniversalRenderPipeline.maxZBinWords / num) / ((math.log2(camera.farClipPlane) - math.log2(camera.nearClipPlane)) * (float)(2 + m_WordsPerTile));
				m_ZBinOffset = (0f - math.log2(camera.nearClipPlane)) * m_ZBinScale;
				m_BinCount = (int)(math.log2(camera.farClipPlane) * m_ZBinScale + m_ZBinOffset);
			}
			else
			{
				m_ZBinScale = (float)(UniversalRenderPipeline.maxZBinWords / num) / ((camera.farClipPlane - camera.nearClipPlane) * (float)(2 + m_WordsPerTile));
				m_ZBinOffset = (0f - camera.nearClipPlane) * m_ZBinScale;
				m_BinCount = (int)(camera.farClipPlane * m_ZBinScale + m_ZBinOffset);
			}
			m_BinCount = Math.Max(m_BinCount, 0);
			Fixed2<float4x4> worldToViews = new Fixed2<float4x4>(cameraData.GetViewMatrix(), cameraData.GetViewMatrix(math.min(1, num - 1)));
			Fixed2<float4x4> @fixed = new Fixed2<float4x4>(cameraData.GetProjectionMatrix(), cameraData.GetProjectionMatrix(math.min(1, num - 1)));
			for (int j = 1; j < num2; j++)
			{
				VisibleReflectionProbe visibleReflectionProbe = visibleReflectionProbes[j];
				int num4 = j - 1;
				while (num4 >= 0 && IsProbeGreater(visibleReflectionProbes[num4], visibleReflectionProbe))
				{
					visibleReflectionProbes[num4 + 1] = visibleReflectionProbes[num4];
					num4--;
				}
				visibleReflectionProbes[num4 + 1] = visibleReflectionProbe;
			}
			NativeArray<float2> minMaxZs = new NativeArray<float2>(num3 * num, Allocator.TempJob);
			JobHandle dependency = new LightMinMaxZJob
			{
				worldToViews = worldToViews,
				lights = subArray,
				minMaxZs = minMaxZs.GetSubArray(0, m_LightCount * num)
			}.ScheduleParallel(m_LightCount * num, 32, default(JobHandle));
			JobHandle dependency2 = new ReflectionProbeMinMaxZJob
			{
				worldToViews = worldToViews,
				reflectionProbes = visibleReflectionProbes,
				minMaxZs = minMaxZs.GetSubArray(m_LightCount * num, num2 * num)
			}.ScheduleParallel(num2 * num, 32, dependency);
			int num5 = (m_BinCount + 128 - 1) / 128;
			JobHandle inputDeps = new ZBinningJob
			{
				bins = m_ZBins,
				minMaxZs = minMaxZs,
				zBinScale = m_ZBinScale,
				zBinOffset = m_ZBinOffset,
				binCount = m_BinCount,
				wordsPerTile = m_WordsPerTile,
				lightCount = m_LightCount,
				reflectionProbeCount = num2,
				batchCount = num5,
				viewCount = num,
				isOrthographic = camera.orthographic
			}.ScheduleParallel(num5 * num, 1, dependency2);
			dependency2.Complete();
			GetViewParams(camera, @fixed[0], out var viewPlaneBot, out var viewPlaneTop, out var viewToViewportScaleBias);
			GetViewParams(camera, @fixed[1], out var viewPlaneBot2, out var viewPlaneTop2, out var viewToViewportScaleBias2);
			int num6 = AlignByteCount((1 + m_TileResolution.y) * UnsafeUtility.SizeOf<InclusiveRange>(), 128) / UnsafeUtility.SizeOf<InclusiveRange>();
			NativeArray<InclusiveRange> tileRanges = new NativeArray<InclusiveRange>(num6 * num3 * num, Allocator.TempJob);
			JobHandle dependency3 = new TilingJob
			{
				lights = subArray,
				reflectionProbes = visibleReflectionProbes,
				tileRanges = tileRanges,
				itemsPerTile = num3,
				rangesPerItem = num6,
				worldToViews = worldToViews,
				tileScale = (float2)@int / (float)m_ActualTileWidth,
				tileScaleInv = (float)m_ActualTileWidth / (float2)@int,
				viewPlaneBottoms = new Fixed2<float>(viewPlaneBot, viewPlaneBot2),
				viewPlaneTops = new Fixed2<float>(viewPlaneTop, viewPlaneTop2),
				viewToViewportScaleBiases = new Fixed2<float4>(viewToViewportScaleBias, viewToViewportScaleBias2),
				tileCount = m_TileResolution,
				near = camera.nearClipPlane,
				isOrthographic = camera.orthographic
			}.ScheduleParallel(num3 * num, 1, dependency2);
			JobHandle inputDeps2 = new TileRangeExpansionJob
			{
				tileRanges = tileRanges,
				tileMasks = m_TileMasks,
				rangesPerItem = num6,
				itemsPerTile = num3,
				wordsPerTile = m_WordsPerTile,
				tileResolution = m_TileResolution
			}.ScheduleParallel(m_TileResolution.y * num, 1, dependency3);
			m_CullingHandle = JobHandle.CombineDependencies(minMaxZs.Dispose(inputDeps), tileRanges.Dispose(inputDeps2));
			JobHandle.ScheduleBatchedJobs();
		}
		static bool IsProbeGreater(VisibleReflectionProbe probe, VisibleReflectionProbe otherProbe)
		{
			if (probe.importance >= otherProbe.importance)
			{
				if (probe.importance == otherProbe.importance)
				{
					return probe.bounds.extents.sqrMagnitude > otherProbe.bounds.extents.sqrMagnitude;
				}
				return false;
			}
			return true;
		}
	}

	public void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		int additionalLightsCount = renderingData.lightData.additionalLightsCount;
		bool shadeAdditionalLightsPerVertex = renderingData.lightData.shadeAdditionalLightsPerVertex;
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(null, m_ProfilingSampler))
		{
			if (m_UseForwardPlus)
			{
				m_ReflectionProbeManager.UpdateGpuData(commandBuffer, ref renderingData);
				using (new ProfilingScope(null, m_ProfilingSamplerFPComplete))
				{
					m_CullingHandle.Complete();
				}
				using (new ProfilingScope(null, m_ProfilingSamplerFPUpload))
				{
					m_ZBinsBuffer.SetData(m_ZBins.Reinterpret<float4>(UnsafeUtility.SizeOf<uint>()));
					m_TileMasksBuffer.SetData(m_TileMasks.Reinterpret<float4>(UnsafeUtility.SizeOf<uint>()));
					commandBuffer.SetGlobalConstantBuffer(m_ZBinsBuffer, "URP_ZBinBuffer", 0, UniversalRenderPipeline.maxZBinWords * 4);
					commandBuffer.SetGlobalConstantBuffer(m_TileMasksBuffer, "urp_TileBuffer", 0, UniversalRenderPipeline.maxTileWords * 4);
				}
				commandBuffer.SetGlobalVector("_FPParams0", math.float4(m_ZBinScale, m_ZBinOffset, m_LightCount, m_DirectionalLightCount));
				commandBuffer.SetGlobalVector("_FPParams1", math.float4(renderingData.cameraData.pixelRect.size / m_ActualTileWidth, m_TileResolution.x, m_WordsPerTile));
				commandBuffer.SetGlobalVector("_FPParams2", math.float4(m_BinCount, m_TileResolution.x * m_TileResolution.y, 0f, 0f));
			}
			SetupShaderLightConstants(commandBuffer, ref renderingData);
			bool flag = (renderingData.cameraData.renderer.stripAdditionalLightOffVariants && renderingData.lightData.supportsAdditionalLights) || additionalLightsCount > 0;
			CoreUtils.SetKeyword(commandBuffer, "_ADDITIONAL_LIGHTS_VERTEX", flag && shadeAdditionalLightsPerVertex && !m_UseForwardPlus);
			CoreUtils.SetKeyword(commandBuffer, "_ADDITIONAL_LIGHTS", flag && !shadeAdditionalLightsPerVertex && !m_UseForwardPlus);
			CoreUtils.SetKeyword(commandBuffer, "_FORWARD_PLUS", m_UseForwardPlus);
			bool flag2 = renderingData.lightData.supportsMixedLighting && m_MixedLightingSetup == MixedLightingSetup.ShadowMask;
			bool flag3 = flag2 && QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask;
			bool flag4 = renderingData.lightData.supportsMixedLighting && m_MixedLightingSetup == MixedLightingSetup.Subtractive;
			CoreUtils.SetKeyword(commandBuffer, "LIGHTMAP_SHADOW_MIXING", flag4 || flag3);
			CoreUtils.SetKeyword(commandBuffer, "SHADOWS_SHADOWMASK", flag2);
			CoreUtils.SetKeyword(commandBuffer, "_MIXED_LIGHTING_SUBTRACTIVE", flag4);
			CoreUtils.SetKeyword(commandBuffer, "_REFLECTION_PROBE_BLENDING", renderingData.lightData.reflectionProbeBlending);
			CoreUtils.SetKeyword(commandBuffer, "_REFLECTION_PROBE_BOX_PROJECTION", renderingData.lightData.reflectionProbeBoxProjection);
			ShEvalMode shEvalMode = PlatformAutoDetect.ShAutoDetect(UniversalRenderPipeline.asset.shEvalMode);
			CoreUtils.SetKeyword(commandBuffer, "EVALUATE_SH_MIXED", shEvalMode == ShEvalMode.Mixed);
			CoreUtils.SetKeyword(commandBuffer, "EVALUATE_SH_VERTEX", shEvalMode == ShEvalMode.PerVertex);
			bool supportsLightLayers = renderingData.lightData.supportsLightLayers;
			CoreUtils.SetKeyword(commandBuffer, "_LIGHT_LAYERS", supportsLightLayers && !CoreUtils.IsSceneLightingDisabled(renderingData.cameraData.camera));
			if (m_LightCookieManager != null)
			{
				m_LightCookieManager.Setup(context, commandBuffer, ref renderingData.lightData);
			}
			else
			{
				CoreUtils.SetKeyword(commandBuffer, "_LIGHT_COOKIES", state: false);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	internal void Cleanup()
	{
		if (m_UseForwardPlus)
		{
			m_CullingHandle.Complete();
			m_ZBins.Dispose();
			m_TileMasks.Dispose();
			m_ZBinsBuffer.Dispose();
			m_ZBinsBuffer = null;
			m_TileMasksBuffer.Dispose();
			m_TileMasksBuffer = null;
			m_ReflectionProbeManager.Dispose();
		}
	}

	private void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex, out Vector4 lightPos, out Vector4 lightColor, out Vector4 lightAttenuation, out Vector4 lightSpotDir, out Vector4 lightOcclusionProbeChannel, out uint lightLayerMask, out bool isSubtractive)
	{
		UniversalRenderPipeline.InitializeLightConstants_Common(lights, lightIndex, out lightPos, out lightColor, out lightAttenuation, out lightSpotDir, out lightOcclusionProbeChannel);
		lightLayerMask = 0u;
		isSubtractive = false;
		if (lightIndex < 0)
		{
			return;
		}
		ref VisibleLight reference = ref lights.UnsafeElementAtMutable(lightIndex);
		Light light = reference.light;
		LightBakingOutput bakingOutput = light.bakingOutput;
		isSubtractive = bakingOutput.isBaked && bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && bakingOutput.mixedLightingMode == MixedLightingMode.Subtractive;
		if (light == null)
		{
			return;
		}
		if (bakingOutput.lightmapBakeType == LightmapBakeType.Mixed && reference.light.shadows != LightShadows.None && m_MixedLightingSetup == MixedLightingSetup.None)
		{
			switch (bakingOutput.mixedLightingMode)
			{
			case MixedLightingMode.Subtractive:
				m_MixedLightingSetup = MixedLightingSetup.Subtractive;
				break;
			case MixedLightingMode.Shadowmask:
				m_MixedLightingSetup = MixedLightingSetup.ShadowMask;
				break;
			}
		}
		UniversalAdditionalLightData universalAdditionalLightData = light.GetUniversalAdditionalLightData();
		lightLayerMask = RenderingLayerUtils.ToValidRenderingLayers(universalAdditionalLightData.renderingLayers);
	}

	private void SetupShaderLightConstants(CommandBuffer cmd, ref RenderingData renderingData)
	{
		m_MixedLightingSetup = MixedLightingSetup.None;
		SetupMainLightConstants(cmd, ref renderingData.lightData);
		SetupAdditionalLightConstants(cmd, ref renderingData);
	}

	private void SetupMainLightConstants(CommandBuffer cmd, ref LightData lightData)
	{
		InitializeLightConstants(lightData.visibleLights, lightData.mainLightIndex, out var lightPos, out var lightColor, out var _, out var _, out var lightOcclusionProbeChannel, out var lightLayerMask, out var isSubtractive);
		lightColor.w = (isSubtractive ? 0f : 1f);
		cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition, lightPos);
		cmd.SetGlobalVector(LightConstantBuffer._MainLightColor, lightColor);
		cmd.SetGlobalVector(LightConstantBuffer._MainLightOcclusionProbesChannel, lightOcclusionProbeChannel);
		cmd.SetGlobalInt(LightConstantBuffer._MainLightLayerMask, (int)lightLayerMask);
	}

	private void SetupAdditionalLightConstants(CommandBuffer cmd, ref RenderingData renderingData)
	{
		ref LightData lightData = ref renderingData.lightData;
		CullingResults cullResults = renderingData.cullResults;
		NativeArray<VisibleLight> visibleLights = lightData.visibleLights;
		int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
		int num = SetupPerObjectLightIndices(cullResults, ref lightData);
		if (num > 0)
		{
			if (m_UseStructuredBuffer)
			{
				NativeArray<ShaderInput.LightData> data = new NativeArray<ShaderInput.LightData>(num, Allocator.Temp);
				int i = 0;
				int num2 = 0;
				ShaderInput.LightData value = default(ShaderInput.LightData);
				for (; i < visibleLights.Length; i++)
				{
					if (num2 >= maxVisibleAdditionalLights)
					{
						break;
					}
					_ = visibleLights[i];
					if (lightData.mainLightIndex != i)
					{
						InitializeLightConstants(visibleLights, i, out value.position, out value.color, out value.attenuation, out value.spotDirection, out value.occlusionProbeChannels, out value.layerMask, out var _);
						data[num2] = value;
						num2++;
					}
				}
				ComputeBuffer lightDataBuffer = ShaderData.instance.GetLightDataBuffer(num);
				lightDataBuffer.SetData(data);
				int lightAndReflectionProbeIndexCount = cullResults.lightAndReflectionProbeIndexCount;
				ComputeBuffer lightIndicesBuffer = ShaderData.instance.GetLightIndicesBuffer(lightAndReflectionProbeIndexCount);
				cmd.SetGlobalBuffer(m_AdditionalLightsBufferId, lightDataBuffer);
				cmd.SetGlobalBuffer(m_AdditionalLightsIndicesId, lightIndicesBuffer);
				data.Dispose();
			}
			else
			{
				int j = 0;
				int num3 = 0;
				for (; j < visibleLights.Length; j++)
				{
					if (num3 >= maxVisibleAdditionalLights)
					{
						break;
					}
					if (lightData.mainLightIndex != j)
					{
						InitializeLightConstants(visibleLights, j, out m_AdditionalLightPositions[num3], out m_AdditionalLightColors[num3], out m_AdditionalLightAttenuations[num3], out m_AdditionalLightSpotDirections[num3], out m_AdditionalLightOcclusionProbeChannels[num3], out var lightLayerMask, out var isSubtractive2);
						m_AdditionalLightsLayerMasks[num3] = math.asfloat(lightLayerMask);
						m_AdditionalLightColors[num3].w = (isSubtractive2 ? 1f : 0f);
						num3++;
					}
				}
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition, m_AdditionalLightPositions);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor, m_AdditionalLightColors);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsAttenuation, m_AdditionalLightAttenuations);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsSpotDir, m_AdditionalLightSpotDirections);
				cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightOcclusionProbeChannel, m_AdditionalLightOcclusionProbeChannels);
				cmd.SetGlobalFloatArray(LightConstantBuffer._AdditionalLightsLayerMasks, m_AdditionalLightsLayerMasks);
			}
			cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, new Vector4(lightData.maxPerObjectAdditionalLightsCount, 0f, 0f, 0f));
		}
		else
		{
			cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount, Vector4.zero);
		}
	}

	private int SetupPerObjectLightIndices(CullingResults cullResults, ref LightData lightData)
	{
		if (lightData.additionalLightsCount == 0 || m_UseForwardPlus)
		{
			return lightData.additionalLightsCount;
		}
		NativeArray<int> lightIndexMap = cullResults.GetLightIndexMap(Allocator.Temp);
		int num = 0;
		int num2 = 0;
		int maxVisibleAdditionalLights = UniversalRenderPipeline.maxVisibleAdditionalLights;
		int length = lightData.visibleLights.Length;
		for (int i = 0; i < length; i++)
		{
			if (num2 >= maxVisibleAdditionalLights)
			{
				break;
			}
			if (i == lightData.mainLightIndex)
			{
				lightIndexMap[i] = -1;
				num++;
			}
			else
			{
				lightIndexMap[i] -= num;
				num2++;
			}
		}
		for (int j = num + num2; j < lightIndexMap.Length; j++)
		{
			lightIndexMap[j] = -1;
		}
		cullResults.SetLightIndexMap(lightIndexMap);
		if (m_UseStructuredBuffer && num2 > 0)
		{
			int lightAndReflectionProbeIndexCount = cullResults.lightAndReflectionProbeIndexCount;
			cullResults.FillLightAndReflectionProbeIndices(ShaderData.instance.GetLightIndicesBuffer(lightAndReflectionProbeIndexCount));
		}
		lightIndexMap.Dispose();
		return num2;
	}
}
