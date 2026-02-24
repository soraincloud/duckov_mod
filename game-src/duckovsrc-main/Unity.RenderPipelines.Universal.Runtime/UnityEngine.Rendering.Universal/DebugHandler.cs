using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityEngine.Rendering.Universal;

internal class DebugHandler : IDebugDisplaySettingsQuery
{
	private class DebugRenderPassEnumerable : IEnumerable<DebugRenderSetup>, IEnumerable
	{
		private class Enumerator : IEnumerator<DebugRenderSetup>, IEnumerator, IDisposable
		{
			private readonly DebugHandler m_DebugHandler;

			private readonly ScriptableRenderContext m_Context;

			private readonly CommandBuffer m_CommandBuffer;

			private readonly FilteringSettings m_FilteringSettings;

			private readonly int m_NumIterations;

			private int m_Index;

			public DebugRenderSetup Current { get; private set; }

			object IEnumerator.Current => Current;

			public Enumerator(DebugHandler debugHandler, ScriptableRenderContext context, CommandBuffer commandBuffer, FilteringSettings filteringSettings)
			{
				DebugSceneOverrideMode sceneOverrideMode = debugHandler.DebugDisplaySettings.renderingSettings.sceneOverrideMode;
				m_DebugHandler = debugHandler;
				m_Context = context;
				m_CommandBuffer = commandBuffer;
				m_FilteringSettings = filteringSettings;
				m_NumIterations = ((sceneOverrideMode != DebugSceneOverrideMode.SolidWireframe && sceneOverrideMode != DebugSceneOverrideMode.ShadedWireframe) ? 1 : 2);
				m_Index = -1;
			}

			public bool MoveNext()
			{
				Current?.Dispose();
				if (++m_Index >= m_NumIterations)
				{
					return false;
				}
				Current = new DebugRenderSetup(m_DebugHandler, m_Context, m_CommandBuffer, m_Index, m_FilteringSettings);
				return true;
			}

			public void Reset()
			{
				if (Current != null)
				{
					Current.Dispose();
					Current = null;
				}
				m_Index = -1;
			}

			public void Dispose()
			{
				Current?.Dispose();
			}
		}

		private readonly DebugHandler m_DebugHandler;

		private readonly ScriptableRenderContext m_Context;

		private readonly CommandBuffer m_CommandBuffer;

		private readonly FilteringSettings m_FilteringSettings;

		public DebugRenderPassEnumerable(DebugHandler debugHandler, ScriptableRenderContext context, CommandBuffer commandBuffer, FilteringSettings filteringSettings)
		{
			m_DebugHandler = debugHandler;
			m_Context = context;
			m_CommandBuffer = commandBuffer;
			m_FilteringSettings = filteringSettings;
		}

		public IEnumerator<DebugRenderSetup> GetEnumerator()
		{
			return new Enumerator(m_DebugHandler, m_Context, m_CommandBuffer, m_FilteringSettings);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	internal delegate void DrawFunction(ScriptableRenderContext context, ref RenderingData renderingData, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock);

	private static readonly int k_DebugColorInvalidModePropertyId = Shader.PropertyToID("_DebugColorInvalidMode");

	private static readonly int k_DebugColorPropertyId = Shader.PropertyToID("_DebugColor");

	private static readonly int k_DebugTexturePropertyId = Shader.PropertyToID("_DebugTexture");

	private static readonly int k_DebugTextureNoStereoPropertyId = Shader.PropertyToID("_DebugTextureNoStereo");

	private static readonly int k_DebugTextureDisplayRect = Shader.PropertyToID("_DebugTextureDisplayRect");

	private static readonly int k_DebugRenderTargetSupportsStereo = Shader.PropertyToID("_DebugRenderTargetSupportsStereo");

	private static readonly int k_DebugMaterialModeId = Shader.PropertyToID("_DebugMaterialMode");

	private static readonly int k_DebugVertexAttributeModeId = Shader.PropertyToID("_DebugVertexAttributeMode");

	private static readonly int k_DebugMaterialValidationModeId = Shader.PropertyToID("_DebugMaterialValidationMode");

	private static readonly int k_DebugMipInfoModeId = Shader.PropertyToID("_DebugMipInfoMode");

	private static readonly int k_DebugSceneOverrideModeId = Shader.PropertyToID("_DebugSceneOverrideMode");

	private static readonly int k_DebugFullScreenModeId = Shader.PropertyToID("_DebugFullScreenMode");

	private static readonly int k_DebugValidationModeId = Shader.PropertyToID("_DebugValidationMode");

	private static readonly int k_DebugValidateBelowMinThresholdColorPropertyId = Shader.PropertyToID("_DebugValidateBelowMinThresholdColor");

	private static readonly int k_DebugValidateAboveMaxThresholdColorPropertyId = Shader.PropertyToID("_DebugValidateAboveMaxThresholdColor");

	private static readonly int k_DebugMaxPixelCost = Shader.PropertyToID("_DebugMaxPixelCost");

	private static readonly int k_DebugLightingModeId = Shader.PropertyToID("_DebugLightingMode");

	private static readonly int k_DebugLightingFeatureFlagsId = Shader.PropertyToID("_DebugLightingFeatureFlags");

	private static readonly int k_DebugValidateAlbedoMinLuminanceId = Shader.PropertyToID("_DebugValidateAlbedoMinLuminance");

	private static readonly int k_DebugValidateAlbedoMaxLuminanceId = Shader.PropertyToID("_DebugValidateAlbedoMaxLuminance");

	private static readonly int k_DebugValidateAlbedoSaturationToleranceId = Shader.PropertyToID("_DebugValidateAlbedoSaturationTolerance");

	private static readonly int k_DebugValidateAlbedoHueToleranceId = Shader.PropertyToID("_DebugValidateAlbedoHueTolerance");

	private static readonly int k_DebugValidateAlbedoCompareColorId = Shader.PropertyToID("_DebugValidateAlbedoCompareColor");

	private static readonly int k_DebugValidateMetallicMinValueId = Shader.PropertyToID("_DebugValidateMetallicMinValue");

	private static readonly int k_DebugValidateMetallicMaxValueId = Shader.PropertyToID("_DebugValidateMetallicMaxValue");

	private static readonly int k_ValidationChannelsId = Shader.PropertyToID("_ValidationChannels");

	private static readonly int k_RangeMinimumId = Shader.PropertyToID("_RangeMinimum");

	private static readonly int k_RangeMaximumId = Shader.PropertyToID("_RangeMaximum");

	private readonly Material m_ReplacementMaterial;

	private readonly Material m_HDRDebugViewMaterial;

	private HDRDebugViewPass m_HDRDebugViewPass;

	private RTHandle m_DebugScreenColorHandle;

	private RTHandle m_DebugScreenDepthHandle;

	private bool m_HasDebugRenderTarget;

	private bool m_DebugRenderTargetSupportsStereo;

	private Vector4 m_DebugRenderTargetPixelRect;

	private RenderTargetIdentifier m_DebugRenderTargetIdentifier;

	private readonly UniversalRenderPipelineDebugDisplaySettings m_DebugDisplaySettings;

	private DebugDisplaySettingsLighting LightingSettings => m_DebugDisplaySettings.lightingSettings;

	private DebugDisplaySettingsMaterial MaterialSettings => m_DebugDisplaySettings.materialSettings;

	private DebugDisplaySettingsRendering RenderingSettings => m_DebugDisplaySettings.renderingSettings;

	public bool AreAnySettingsActive => m_DebugDisplaySettings.AreAnySettingsActive;

	public bool IsPostProcessingAllowed => m_DebugDisplaySettings.IsPostProcessingAllowed;

	public bool IsLightingActive => m_DebugDisplaySettings.IsLightingActive;

	internal bool IsActiveModeUnsupportedForDeferred
	{
		get
		{
			if (m_DebugDisplaySettings.lightingSettings.lightingDebugMode == DebugLightingMode.None && m_DebugDisplaySettings.lightingSettings.lightingFeatureFlags == DebugLightingFeatureFlags.None && m_DebugDisplaySettings.renderingSettings.sceneOverrideMode == DebugSceneOverrideMode.None && m_DebugDisplaySettings.materialSettings.materialDebugMode == DebugMaterialMode.None && m_DebugDisplaySettings.materialSettings.vertexAttributeDebugMode == DebugVertexAttributeMode.None)
			{
				return m_DebugDisplaySettings.materialSettings.materialValidationMode != DebugMaterialValidationMode.None;
			}
			return true;
		}
	}

	internal Material ReplacementMaterial => m_ReplacementMaterial;

	internal UniversalRenderPipelineDebugDisplaySettings DebugDisplaySettings => m_DebugDisplaySettings;

	internal ref RTHandle DebugScreenColorHandle => ref m_DebugScreenColorHandle;

	internal ref RTHandle DebugScreenDepthHandle => ref m_DebugScreenDepthHandle;

	internal HDRDebugViewPass hdrDebugViewPass => m_HDRDebugViewPass;

	internal bool IsScreenClearNeeded
	{
		get
		{
			Color color = Color.black;
			return TryGetScreenClearColor(ref color);
		}
	}

	internal bool IsRenderPassSupported
	{
		get
		{
			if (RenderingSettings.sceneOverrideMode != DebugSceneOverrideMode.None)
			{
				return RenderingSettings.sceneOverrideMode == DebugSceneOverrideMode.Overdraw;
			}
			return true;
		}
	}

	public bool TryGetScreenClearColor(ref Color color)
	{
		return m_DebugDisplaySettings.TryGetScreenClearColor(ref color);
	}

	internal bool HDRDebugViewIsActive(ref CameraData cameraData)
	{
		if (DebugDisplaySettings.lightingSettings.hdrDebugMode != HDRDebugMode.None)
		{
			return cameraData.resolveFinalTarget;
		}
		return false;
	}

	internal bool WriteToDebugScreenTexture(ref CameraData cameraData)
	{
		return HDRDebugViewIsActive(ref cameraData);
	}

	internal DebugHandler(ScriptableRendererData scriptableRendererData)
	{
		Shader debugReplacementPS = scriptableRendererData.debugShaders.debugReplacementPS;
		Shader hdrDebugViewPS = scriptableRendererData.debugShaders.hdrDebugViewPS;
		m_DebugDisplaySettings = DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>.Instance;
		m_ReplacementMaterial = ((debugReplacementPS == null) ? null : CoreUtils.CreateEngineMaterial(debugReplacementPS));
		m_HDRDebugViewMaterial = ((hdrDebugViewPS == null) ? null : CoreUtils.CreateEngineMaterial(hdrDebugViewPS));
		m_HDRDebugViewPass = new HDRDebugViewPass(m_HDRDebugViewMaterial);
	}

	public void Dispose()
	{
		m_HDRDebugViewPass.Dispose();
		m_DebugScreenColorHandle?.Release();
		m_DebugScreenDepthHandle?.Release();
		CoreUtils.Destroy(m_HDRDebugViewMaterial);
		CoreUtils.Destroy(m_ReplacementMaterial);
	}

	internal bool IsActiveForCamera(ref CameraData cameraData)
	{
		if (!cameraData.isPreviewCamera)
		{
			return AreAnySettingsActive;
		}
		return false;
	}

	internal bool TryGetFullscreenDebugMode(out DebugFullScreenMode debugFullScreenMode)
	{
		int textureHeightPercent;
		return TryGetFullscreenDebugMode(out debugFullScreenMode, out textureHeightPercent);
	}

	internal bool TryGetFullscreenDebugMode(out DebugFullScreenMode debugFullScreenMode, out int textureHeightPercent)
	{
		debugFullScreenMode = RenderingSettings.fullScreenDebugMode;
		textureHeightPercent = RenderingSettings.fullScreenDebugModeOutputSizeScreenPercent;
		return debugFullScreenMode != DebugFullScreenMode.None;
	}

	internal static void ConfigureColorDescriptorForDebugScreen(ref RenderTextureDescriptor descriptor, int cameraWidth, int cameraHeight)
	{
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.useDynamicScale = true;
		descriptor.depthBufferBits = 0;
	}

	internal static void ConfigureDepthDescriptorForDebugScreen(ref RenderTextureDescriptor descriptor, int depthBufferBits, int cameraWidth, int cameraHeight)
	{
		descriptor.width = cameraWidth;
		descriptor.height = cameraHeight;
		descriptor.useMipMap = false;
		descriptor.autoGenerateMips = false;
		descriptor.useDynamicScale = true;
		descriptor.depthBufferBits = depthBufferBits;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void SetupShaderProperties(CommandBuffer cmd, int passIndex = 0)
	{
		if (LightingSettings.lightingDebugMode == DebugLightingMode.ShadowCascades)
		{
			cmd.EnableShaderKeyword("_DEBUG_ENVIRONMENTREFLECTIONS_OFF");
		}
		else
		{
			cmd.DisableShaderKeyword("_DEBUG_ENVIRONMENTREFLECTIONS_OFF");
		}
		switch (RenderingSettings.sceneOverrideMode)
		{
		case DebugSceneOverrideMode.Overdraw:
		{
			float num = 1f / (float)RenderingSettings.maxOverdrawCount;
			cmd.SetGlobalColor(k_DebugColorPropertyId, new Color(num, num, num, 1f));
			break;
		}
		case DebugSceneOverrideMode.Wireframe:
			cmd.SetGlobalColor(k_DebugColorPropertyId, Color.black);
			break;
		case DebugSceneOverrideMode.SolidWireframe:
			cmd.SetGlobalColor(k_DebugColorPropertyId, (passIndex == 0) ? Color.white : Color.black);
			break;
		case DebugSceneOverrideMode.ShadedWireframe:
			switch (passIndex)
			{
			case 0:
				cmd.DisableShaderKeyword("DEBUG_DISPLAY");
				break;
			case 1:
				cmd.SetGlobalColor(k_DebugColorPropertyId, Color.black);
				cmd.EnableShaderKeyword("DEBUG_DISPLAY");
				break;
			}
			break;
		}
		switch (MaterialSettings.materialValidationMode)
		{
		case DebugMaterialValidationMode.Albedo:
			cmd.SetGlobalFloat(k_DebugValidateAlbedoMinLuminanceId, MaterialSettings.albedoMinLuminance);
			cmd.SetGlobalFloat(k_DebugValidateAlbedoMaxLuminanceId, MaterialSettings.albedoMaxLuminance);
			cmd.SetGlobalFloat(k_DebugValidateAlbedoSaturationToleranceId, MaterialSettings.albedoSaturationTolerance);
			cmd.SetGlobalFloat(k_DebugValidateAlbedoHueToleranceId, MaterialSettings.albedoHueTolerance);
			cmd.SetGlobalColor(k_DebugValidateAlbedoCompareColorId, MaterialSettings.albedoCompareColor.linear);
			break;
		case DebugMaterialValidationMode.Metallic:
			cmd.SetGlobalFloat(k_DebugValidateMetallicMinValueId, MaterialSettings.metallicMinValue);
			cmd.SetGlobalFloat(k_DebugValidateMetallicMaxValueId, MaterialSettings.metallicMaxValue);
			break;
		}
	}

	internal void SetDebugRenderTarget(RenderTargetIdentifier renderTargetIdentifier, Rect displayRect, bool supportsStereo)
	{
		m_HasDebugRenderTarget = true;
		m_DebugRenderTargetSupportsStereo = supportsStereo;
		m_DebugRenderTargetIdentifier = renderTargetIdentifier;
		m_DebugRenderTargetPixelRect = new Vector4(displayRect.x, displayRect.y, displayRect.width, displayRect.height);
	}

	internal void ResetDebugRenderTarget()
	{
		m_HasDebugRenderTarget = false;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void UpdateShaderGlobalPropertiesForFinalValidationPass(CommandBuffer cmd, ref CameraData cameraData, bool isFinalPass)
	{
		if (!isFinalPass || !cameraData.resolveFinalTarget)
		{
			cmd.DisableShaderKeyword("DEBUG_DISPLAY");
			return;
		}
		if (IsActiveForCamera(ref cameraData))
		{
			cmd.EnableShaderKeyword("DEBUG_DISPLAY");
		}
		else
		{
			cmd.DisableShaderKeyword("DEBUG_DISPLAY");
		}
		if (m_HasDebugRenderTarget)
		{
			cmd.SetGlobalTexture(m_DebugRenderTargetSupportsStereo ? k_DebugTexturePropertyId : k_DebugTextureNoStereoPropertyId, m_DebugRenderTargetIdentifier);
			cmd.SetGlobalVector(k_DebugTextureDisplayRect, m_DebugRenderTargetPixelRect);
			cmd.SetGlobalInteger(k_DebugRenderTargetSupportsStereo, m_DebugRenderTargetSupportsStereo ? 1 : 0);
		}
		DebugDisplaySettingsRendering renderingSettings = m_DebugDisplaySettings.renderingSettings;
		if (renderingSettings.validationMode == DebugValidationMode.HighlightOutsideOfRange)
		{
			cmd.SetGlobalInteger(k_ValidationChannelsId, (int)renderingSettings.validationChannels);
			cmd.SetGlobalFloat(k_RangeMinimumId, renderingSettings.validationRangeMin);
			cmd.SetGlobalFloat(k_RangeMaximumId, renderingSettings.validationRangeMax);
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	internal void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = renderingData.commandBuffer;
		CameraData cameraData = renderingData.cameraData;
		if (IsActiveForCamera(ref cameraData))
		{
			commandBuffer.EnableShaderKeyword("DEBUG_DISPLAY");
			commandBuffer.SetGlobalFloat(k_DebugMaterialModeId, (float)MaterialSettings.materialDebugMode);
			commandBuffer.SetGlobalFloat(k_DebugVertexAttributeModeId, (float)MaterialSettings.vertexAttributeDebugMode);
			commandBuffer.SetGlobalInteger(k_DebugMaterialValidationModeId, (int)MaterialSettings.materialValidationMode);
			commandBuffer.SetGlobalInteger(k_DebugMipInfoModeId, (int)RenderingSettings.mipInfoMode);
			commandBuffer.SetGlobalInteger(k_DebugSceneOverrideModeId, (int)RenderingSettings.sceneOverrideMode);
			commandBuffer.SetGlobalInteger(k_DebugFullScreenModeId, (int)RenderingSettings.fullScreenDebugMode);
			commandBuffer.SetGlobalInteger(k_DebugMaxPixelCost, RenderingSettings.maxOverdrawCount);
			commandBuffer.SetGlobalInteger(k_DebugValidationModeId, (int)RenderingSettings.validationMode);
			commandBuffer.SetGlobalColor(k_DebugValidateBelowMinThresholdColorPropertyId, Color.red);
			commandBuffer.SetGlobalColor(k_DebugValidateAboveMaxThresholdColorPropertyId, Color.blue);
			commandBuffer.SetGlobalFloat(k_DebugLightingModeId, (float)LightingSettings.lightingDebugMode);
			commandBuffer.SetGlobalInteger(k_DebugLightingFeatureFlagsId, (int)LightingSettings.lightingFeatureFlags);
			commandBuffer.SetGlobalColor(k_DebugColorInvalidModePropertyId, Color.red);
		}
		else
		{
			commandBuffer.DisableShaderKeyword("DEBUG_DISPLAY");
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
	}

	internal IEnumerable<DebugRenderSetup> CreateDebugRenderSetupEnumerable(ScriptableRenderContext context, CommandBuffer commandBuffer, FilteringSettings filteringSettings)
	{
		return new DebugRenderPassEnumerable(this, context, commandBuffer, filteringSettings);
	}

	internal void DrawWithDebugRenderState(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings, ref RenderStateBlock renderStateBlock, DrawFunction func)
	{
		foreach (DebugRenderSetup item in CreateDebugRenderSetupEnumerable(context, cmd, filteringSettings))
		{
			DrawingSettings drawingSettings2 = item.CreateDrawingSettings(drawingSettings);
			RenderStateBlock renderStateBlock2 = item.GetRenderStateBlock(renderStateBlock);
			func(context, ref renderingData, ref drawingSettings2, ref filteringSettings, ref renderStateBlock2);
		}
	}
}
