using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace UnityEngine.Rendering.Universal.Internal;

public class MainLightShadowCasterPass : ScriptableRenderPass
{
	private static class MainLightShadowConstantBuffer
	{
		public static int _WorldToShadow;

		public static int _ShadowParams;

		public static int _CascadeShadowSplitSpheres0;

		public static int _CascadeShadowSplitSpheres1;

		public static int _CascadeShadowSplitSpheres2;

		public static int _CascadeShadowSplitSpheres3;

		public static int _CascadeShadowSplitSphereRadii;

		public static int _ShadowOffset0;

		public static int _ShadowOffset1;

		public static int _ShadowmapSize;
	}

	private class PassData
	{
		internal MainLightShadowCasterPass pass;

		internal RenderGraph graph;

		internal TextureHandle shadowmapTexture;

		internal RenderingData renderingData;

		internal int shadowmapID;

		internal bool emptyShadowmap;
	}

	private const int k_MaxCascades = 4;

	private const int k_ShadowmapBufferBits = 16;

	private float m_CascadeBorder;

	private float m_MaxShadowDistanceSq;

	private int m_ShadowCasterCascadesCount;

	private int m_MainLightShadowmapID;

	internal RTHandle m_MainLightShadowmapTexture;

	private RTHandle m_EmptyMainLightShadowmapTexture;

	private const int k_EmptyShadowMapDimensions = 1;

	private const string k_MainLightShadowMapTextureName = "_MainLightShadowmapTexture";

	private const string k_EmptyMainLightShadowMapTextureName = "_EmptyMainLightShadowmapTexture";

	private static readonly Vector4 s_EmptyShadowParams = new Vector4(1f, 0f, 1f, 0f);

	private static readonly Vector4 s_EmptyShadowmapSize = (s_EmptyShadowmapSize = new Vector4(1f, 1f, 1f, 1f));

	private Matrix4x4[] m_MainLightShadowMatrices;

	private ShadowSliceData[] m_CascadeSlices;

	private Vector4[] m_CascadeSplitDistances;

	private bool m_CreateEmptyShadowmap;

	private int renderTargetWidth;

	private int renderTargetHeight;

	private ProfilingSampler m_ProfilingSetupSampler = new ProfilingSampler("Setup Main Shadowmap");

	public MainLightShadowCasterPass(RenderPassEvent evt)
	{
		base.profilingSampler = new ProfilingSampler("MainLightShadowCasterPass");
		base.renderPassEvent = evt;
		m_MainLightShadowMatrices = new Matrix4x4[5];
		m_CascadeSlices = new ShadowSliceData[4];
		m_CascadeSplitDistances = new Vector4[4];
		MainLightShadowConstantBuffer._WorldToShadow = Shader.PropertyToID("_MainLightWorldToShadow");
		MainLightShadowConstantBuffer._ShadowParams = Shader.PropertyToID("_MainLightShadowParams");
		MainLightShadowConstantBuffer._CascadeShadowSplitSpheres0 = Shader.PropertyToID("_CascadeShadowSplitSpheres0");
		MainLightShadowConstantBuffer._CascadeShadowSplitSpheres1 = Shader.PropertyToID("_CascadeShadowSplitSpheres1");
		MainLightShadowConstantBuffer._CascadeShadowSplitSpheres2 = Shader.PropertyToID("_CascadeShadowSplitSpheres2");
		MainLightShadowConstantBuffer._CascadeShadowSplitSpheres3 = Shader.PropertyToID("_CascadeShadowSplitSpheres3");
		MainLightShadowConstantBuffer._CascadeShadowSplitSphereRadii = Shader.PropertyToID("_CascadeShadowSplitSphereRadii");
		MainLightShadowConstantBuffer._ShadowOffset0 = Shader.PropertyToID("_MainLightShadowOffset0");
		MainLightShadowConstantBuffer._ShadowOffset1 = Shader.PropertyToID("_MainLightShadowOffset1");
		MainLightShadowConstantBuffer._ShadowmapSize = Shader.PropertyToID("_MainLightShadowmapSize");
		m_MainLightShadowmapID = Shader.PropertyToID("_MainLightShadowmapTexture");
	}

	public void Dispose()
	{
		m_MainLightShadowmapTexture?.Release();
		m_EmptyMainLightShadowmapTexture?.Release();
	}

	public bool Setup(ref RenderingData renderingData)
	{
		if (!renderingData.shadowData.mainLightShadowsEnabled)
		{
			return false;
		}
		using (new ProfilingScope(null, m_ProfilingSetupSampler))
		{
			if (!renderingData.shadowData.supportsMainLightShadows)
			{
				return SetupForEmptyRendering(ref renderingData);
			}
			Clear();
			int mainLightIndex = renderingData.lightData.mainLightIndex;
			if (mainLightIndex == -1)
			{
				return SetupForEmptyRendering(ref renderingData);
			}
			VisibleLight visibleLight = renderingData.lightData.visibleLights[mainLightIndex];
			Light light = visibleLight.light;
			if (light.shadows == LightShadows.None)
			{
				return SetupForEmptyRendering(ref renderingData);
			}
			if (visibleLight.lightType != LightType.Directional)
			{
				Debug.LogWarning("Only directional lights are supported as main light.");
			}
			if (!renderingData.cullResults.GetShadowCasterBounds(mainLightIndex, out var _))
			{
				return SetupForEmptyRendering(ref renderingData);
			}
			m_ShadowCasterCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;
			int maxTileResolutionInAtlas = ShadowUtils.GetMaxTileResolutionInAtlas(renderingData.shadowData.mainLightShadowmapWidth, renderingData.shadowData.mainLightShadowmapHeight, m_ShadowCasterCascadesCount);
			renderTargetWidth = renderingData.shadowData.mainLightShadowmapWidth;
			renderTargetHeight = ((m_ShadowCasterCascadesCount == 2) ? (renderingData.shadowData.mainLightShadowmapHeight >> 1) : renderingData.shadowData.mainLightShadowmapHeight);
			for (int i = 0; i < m_ShadowCasterCascadesCount; i++)
			{
				if (!ShadowUtils.ExtractDirectionalLightMatrix(ref renderingData.cullResults, ref renderingData.shadowData, mainLightIndex, i, renderTargetWidth, renderTargetHeight, maxTileResolutionInAtlas, light.shadowNearPlane, out m_CascadeSplitDistances[i], out m_CascadeSlices[i]))
				{
					return SetupForEmptyRendering(ref renderingData);
				}
			}
			m_MaxShadowDistanceSq = renderingData.cameraData.maxShadowDistance * renderingData.cameraData.maxShadowDistance;
			m_CascadeBorder = renderingData.shadowData.mainLightShadowCascadeBorder;
			m_CreateEmptyShadowmap = false;
			base.useNativeRenderPass = true;
			ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_MainLightShadowmapTexture, renderTargetWidth, renderTargetHeight, 16, 1, 0f, "_MainLightShadowmapTexture");
			return true;
		}
	}

	private bool SetupForEmptyRendering(ref RenderingData renderingData)
	{
		if (!renderingData.cameraData.renderer.stripShadowsOffVariants)
		{
			return false;
		}
		m_CreateEmptyShadowmap = true;
		base.useNativeRenderPass = false;
		ShadowUtils.ShadowRTReAllocateIfNeeded(ref m_EmptyMainLightShadowmapTexture, 1, 1, 16, 1, 0f, "_EmptyMainLightShadowmapTexture");
		return true;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (Application.platform == RuntimePlatform.Android && PlatformAutoDetect.isRunningOnPowerVRGPU)
		{
			ResetTarget();
		}
		if (m_CreateEmptyShadowmap)
		{
			ConfigureTarget(m_EmptyMainLightShadowmapTexture);
		}
		else
		{
			ConfigureTarget(m_MainLightShadowmapTexture);
		}
		ConfigureClear(ClearFlag.All, Color.black);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_CreateEmptyShadowmap)
		{
			SetEmptyMainLightCascadeShadowmap(ref context, ref renderingData);
			renderingData.commandBuffer.SetGlobalTexture(m_MainLightShadowmapID, m_EmptyMainLightShadowmapTexture.nameID);
		}
		else
		{
			RenderMainLightCascadeShadowmap(ref context, ref renderingData);
			renderingData.commandBuffer.SetGlobalTexture(m_MainLightShadowmapID, m_MainLightShadowmapTexture.nameID);
		}
	}

	private void Clear()
	{
		for (int i = 0; i < m_MainLightShadowMatrices.Length; i++)
		{
			m_MainLightShadowMatrices[i] = Matrix4x4.identity;
		}
		for (int j = 0; j < m_CascadeSplitDistances.Length; j++)
		{
			m_CascadeSplitDistances[j] = new Vector4(0f, 0f, 0f, 0f);
		}
		for (int k = 0; k < m_CascadeSlices.Length; k++)
		{
			m_CascadeSlices[k].Clear();
		}
	}

	private void SetEmptyMainLightCascadeShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state: true);
		SetEmptyMainLightShadowParams(commandBuffer);
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	internal static void SetEmptyMainLightShadowParams(CommandBuffer cmd)
	{
		cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowParams, s_EmptyShadowParams);
		cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowmapSize, s_EmptyShadowmapSize);
	}

	private void RenderMainLightCascadeShadowmap(ref ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CullingResults cullResults = renderingData.cullResults;
		LightData lightData = renderingData.lightData;
		int mainLightIndex = lightData.mainLightIndex;
		if (mainLightIndex == -1)
		{
			return;
		}
		VisibleLight shadowLight = lightData.visibleLights[mainLightIndex];
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		using (new ProfilingScope(commandBuffer, ProfilingSampler.Get(URPProfileId.MainLightShadow)))
		{
			ShadowUtils.SetCameraPosition(commandBuffer, renderingData.cameraData.worldSpaceCameraPos);
			ShadowUtils.SetWorldToCameraMatrix(commandBuffer, renderingData.cameraData.GetViewMatrix());
			ShadowDrawingSettings settings = new ShadowDrawingSettings(cullResults, mainLightIndex, BatchCullingProjectionType.Orthographic);
			settings.useRenderingLayerMaskTest = UniversalRenderPipeline.asset.useRenderingLayers;
			for (int i = 0; i < m_ShadowCasterCascadesCount; i++)
			{
				settings.splitData = m_CascadeSlices[i].splitData;
				Vector4 shadowBias = ShadowUtils.GetShadowBias(ref shadowLight, mainLightIndex, ref renderingData.shadowData, m_CascadeSlices[i].projectionMatrix, m_CascadeSlices[i].resolution);
				ShadowUtils.SetupShadowCasterConstantBuffer(commandBuffer, ref shadowLight, shadowBias);
				CoreUtils.SetKeyword(commandBuffer, "_CASTING_PUNCTUAL_LIGHT_SHADOW", state: false);
				ShadowUtils.RenderShadowSlice(commandBuffer, ref context, ref m_CascadeSlices[i], ref settings, m_CascadeSlices[i].projectionMatrix, m_CascadeSlices[i].viewMatrix);
			}
			renderingData.shadowData.isKeywordSoftShadowsEnabled = shadowLight.light.shadows == LightShadows.Soft && renderingData.shadowData.supportsSoftShadows;
			CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", renderingData.shadowData.mainLightShadowCascadesCount == 1);
			CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", renderingData.shadowData.mainLightShadowCascadesCount > 1);
			ShadowUtils.SetSoftShadowQualityShaderKeywords(commandBuffer, ref renderingData.shadowData);
			SetupMainLightShadowReceiverConstants(commandBuffer, ref shadowLight, ref renderingData.shadowData);
		}
	}

	private void SetupMainLightShadowReceiverConstants(CommandBuffer cmd, ref VisibleLight shadowLight, ref ShadowData shadowData)
	{
		Light light = shadowLight.light;
		bool softShadowsEnabled = shadowLight.light.shadows == LightShadows.Soft && shadowData.supportsSoftShadows;
		int shadowCasterCascadesCount = m_ShadowCasterCascadesCount;
		for (int i = 0; i < shadowCasterCascadesCount; i++)
		{
			m_MainLightShadowMatrices[i] = m_CascadeSlices[i].shadowTransform;
		}
		Matrix4x4 zero = Matrix4x4.zero;
		zero.m22 = (SystemInfo.usesReversedZBuffer ? 1f : 0f);
		for (int j = shadowCasterCascadesCount; j <= 4; j++)
		{
			m_MainLightShadowMatrices[j] = zero;
		}
		float num = 1f / (float)renderTargetWidth;
		float num2 = 1f / (float)renderTargetHeight;
		float num3 = 0.5f * num;
		float num4 = 0.5f * num2;
		float y = ShadowUtils.SoftShadowQualityToShaderProperty(light, softShadowsEnabled);
		ShadowUtils.GetScaleAndBiasForLinearDistanceFade(m_MaxShadowDistanceSq, m_CascadeBorder, out var scale, out var bias);
		cmd.SetGlobalMatrixArray(MainLightShadowConstantBuffer._WorldToShadow, m_MainLightShadowMatrices);
		cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowParams, new Vector4(light.shadowStrength, y, scale, bias));
		if (m_ShadowCasterCascadesCount > 1)
		{
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres0, m_CascadeSplitDistances[0]);
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres1, m_CascadeSplitDistances[1]);
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres2, m_CascadeSplitDistances[2]);
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSpheres3, m_CascadeSplitDistances[3]);
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._CascadeShadowSplitSphereRadii, new Vector4(m_CascadeSplitDistances[0].w * m_CascadeSplitDistances[0].w, m_CascadeSplitDistances[1].w * m_CascadeSplitDistances[1].w, m_CascadeSplitDistances[2].w * m_CascadeSplitDistances[2].w, m_CascadeSplitDistances[3].w * m_CascadeSplitDistances[3].w));
		}
		if (shadowData.supportsSoftShadows)
		{
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset0, new Vector4(0f - num3, 0f - num4, num3, 0f - num4));
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowOffset1, new Vector4(0f - num3, num4, num3, num4));
			cmd.SetGlobalVector(MainLightShadowConstantBuffer._ShadowmapSize, new Vector4(num, num2, renderTargetWidth, renderTargetHeight));
		}
	}

	internal TextureHandle Render(RenderGraph graph, ref RenderingData renderingData)
	{
		PassData passData;
		TextureHandle shadowmapTexture;
		using (RenderGraphBuilder renderGraphBuilder = graph.AddRenderPass<PassData>("Main Light Shadowmap", out passData, base.profilingSampler))
		{
			InitPassData(ref passData, ref renderingData, ref graph);
			if (!m_CreateEmptyShadowmap)
			{
				passData.shadowmapTexture = UniversalRenderer.CreateRenderGraphTexture(graph, m_MainLightShadowmapTexture.rt.descriptor, "Main Shadowmap", clear: true, (!ShadowUtils.m_ForceShadowPointSampling) ? FilterMode.Bilinear : FilterMode.Point);
				renderGraphBuilder.UseDepthBuffer(in passData.shadowmapTexture, DepthAccess.Write);
			}
			renderGraphBuilder.AllowPassCulling(value: false);
			renderGraphBuilder.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
			{
				if (!data.emptyShadowmap)
				{
					data.pass.RenderMainLightCascadeShadowmap(ref context.renderContext, ref data.renderingData);
				}
			});
			shadowmapTexture = passData.shadowmapTexture;
		}
		PassData passData2;
		using RenderGraphBuilder renderGraphBuilder2 = graph.AddRenderPass<PassData>("Set Main Shadow Globals", out passData2, base.profilingSampler);
		InitPassData(ref passData2, ref renderingData, ref graph);
		passData2.shadowmapTexture = shadowmapTexture;
		if (shadowmapTexture.IsValid())
		{
			renderGraphBuilder2.UseDepthBuffer(in shadowmapTexture, DepthAccess.Read);
		}
		renderGraphBuilder2.AllowPassCulling(value: false);
		renderGraphBuilder2.SetRenderFunc(delegate(PassData data, RenderGraphContext context)
		{
			if (data.emptyShadowmap)
			{
				data.pass.SetEmptyMainLightCascadeShadowmap(ref context.renderContext, ref data.renderingData);
				data.shadowmapTexture = data.graph.defaultResources.defaultShadowTexture;
			}
			data.renderingData.commandBuffer.SetGlobalTexture(data.shadowmapID, data.shadowmapTexture);
		});
		return passData2.shadowmapTexture;
	}

	private void InitPassData(ref PassData passData, ref RenderingData renderingData, ref RenderGraph graph)
	{
		passData.pass = this;
		passData.graph = graph;
		passData.emptyShadowmap = m_CreateEmptyShadowmap;
		passData.shadowmapID = m_MainLightShadowmapID;
		passData.renderingData = renderingData;
	}
}
