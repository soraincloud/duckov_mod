using System.Diagnostics;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.Universal;

[DisallowMultipleRendererFeature("Decal")]
[Tooltip("With this Renderer Feature, Unity can project specific Materials (decals) onto other objects in the Scene.")]
internal class DecalRendererFeature : ScriptableRendererFeature
{
	[SerializeField]
	private DecalSettings m_Settings = new DecalSettings();

	[SerializeField]
	[HideInInspector]
	[Reload("Shaders/Utils/CopyDepth.shader", ReloadAttribute.Package.Root)]
	private Shader m_CopyDepthPS;

	[SerializeField]
	[HideInInspector]
	[Reload("Runtime/Decal/DBuffer/DBufferClear.shader", ReloadAttribute.Package.Root)]
	private Shader m_DBufferClear;

	private DecalTechnique m_Technique;

	private DBufferSettings m_DBufferSettings;

	private DecalScreenSpaceSettings m_ScreenSpaceSettings;

	private bool m_RecreateSystems;

	private CopyDepthPass m_CopyDepthPass;

	private DecalPreviewPass m_DecalPreviewPass;

	private Material m_CopyDepthMaterial;

	private DecalEntityManager m_DecalEntityManager;

	private DecalUpdateCachedSystem m_DecalUpdateCachedSystem;

	private DecalUpdateCullingGroupSystem m_DecalUpdateCullingGroupSystem;

	private DecalUpdateCulledSystem m_DecalUpdateCulledSystem;

	private DecalCreateDrawCallSystem m_DecalCreateDrawCallSystem;

	private DecalDrawErrorSystem m_DrawErrorSystem;

	private DBufferRenderPass m_DBufferRenderPass;

	private DecalForwardEmissivePass m_ForwardEmissivePass;

	private DecalDrawDBufferSystem m_DecalDrawDBufferSystem;

	private DecalDrawFowardEmissiveSystem m_DecalDrawForwardEmissiveSystem;

	private Material m_DBufferClearMaterial;

	private DecalScreenSpaceRenderPass m_ScreenSpaceDecalRenderPass;

	private DecalDrawScreenSpaceSystem m_DecalDrawScreenSpaceSystem;

	private DecalSkipCulledSystem m_DecalSkipCulledSystem;

	private DecalGBufferRenderPass m_GBufferRenderPass;

	private DecalDrawGBufferSystem m_DrawGBufferSystem;

	private DeferredLights m_DeferredLights;

	private static SharedDecalEntityManager sharedDecalEntityManager { get; } = new SharedDecalEntityManager();

	internal ref DecalSettings settings => ref m_Settings;

	internal bool intermediateRendering => m_Technique == DecalTechnique.DBuffer;

	internal bool requiresDecalLayers => m_Settings.decalLayers;

	internal static bool isGLDevice
	{
		get
		{
			if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
			{
				return SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore;
			}
			return true;
		}
	}

	public override void Create()
	{
		m_DecalPreviewPass = new DecalPreviewPass();
		m_RecreateSystems = true;
	}

	internal override bool RequireRenderingLayers(bool isDeferred, bool needsGBufferAccurateNormals, out RenderingLayerUtils.Event atEvent, out RenderingLayerUtils.MaskSize maskSize)
	{
		bool isPlaying = Application.isPlaying;
		DecalTechnique technique = GetTechnique(isDeferred, needsGBufferAccurateNormals, isPlaying);
		atEvent = ((technique != DecalTechnique.DBuffer) ? RenderingLayerUtils.Event.Opaque : RenderingLayerUtils.Event.DepthNormalPrePass);
		maskSize = RenderingLayerUtils.MaskSize.Bits8;
		return requiresDecalLayers;
	}

	internal DBufferSettings GetDBufferSettings()
	{
		if (m_Settings.technique == DecalTechniqueOption.Automatic)
		{
			return new DBufferSettings
			{
				surfaceData = DecalSurfaceData.AlbedoNormalMAOS
			};
		}
		return m_Settings.dBufferSettings;
	}

	internal DecalScreenSpaceSettings GetScreenSpaceSettings()
	{
		if (m_Settings.technique == DecalTechniqueOption.Automatic)
		{
			return new DecalScreenSpaceSettings
			{
				normalBlend = DecalNormalBlend.Low
			};
		}
		return m_Settings.screenSpaceSettings;
	}

	internal DecalTechnique GetTechnique(ScriptableRendererData renderer)
	{
		UniversalRendererData universalRendererData = renderer as UniversalRendererData;
		if (universalRendererData == null)
		{
			Debug.LogError("Only universal renderer supports Decal renderer feature.");
			return DecalTechnique.Invalid;
		}
		bool isDeferred = universalRendererData.renderingMode == RenderingMode.Deferred;
		return GetTechnique(isDeferred, universalRendererData.accurateGbufferNormals);
	}

	internal DecalTechnique GetTechnique(ScriptableRenderer renderer)
	{
		if (!(renderer is UniversalRenderer universalRenderer))
		{
			Debug.LogError("Only universal renderer supports Decal renderer feature.");
			return DecalTechnique.Invalid;
		}
		bool isDeferred = universalRenderer.renderingModeActual == RenderingMode.Deferred;
		return GetTechnique(isDeferred, universalRenderer.accurateGbufferNormals);
	}

	internal DecalTechnique GetTechnique(bool isDeferred, bool needsGBufferAccurateNormals, bool checkForInvalidTechniques = true)
	{
		DecalTechnique decalTechnique = DecalTechnique.Invalid;
		switch (m_Settings.technique)
		{
		case DecalTechniqueOption.Automatic:
			decalTechnique = ((IsAutomaticDBuffer() || (isDeferred && needsGBufferAccurateNormals)) ? DecalTechnique.DBuffer : ((!isDeferred) ? DecalTechnique.ScreenSpace : DecalTechnique.GBuffer));
			break;
		case DecalTechniqueOption.ScreenSpace:
			decalTechnique = ((!isDeferred) ? DecalTechnique.ScreenSpace : DecalTechnique.GBuffer);
			break;
		case DecalTechniqueOption.DBuffer:
			decalTechnique = DecalTechnique.DBuffer;
			break;
		}
		if (!checkForInvalidTechniques)
		{
			return decalTechnique;
		}
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
		{
			Debug.LogError("Decals are not supported with OpenGLES2.");
			return DecalTechnique.Invalid;
		}
		if (decalTechnique == DecalTechnique.DBuffer && isGLDevice)
		{
			Debug.LogError("Decal DBuffer technique is not supported with OpenGL.");
			return DecalTechnique.Invalid;
		}
		bool flag = SystemInfo.supportedRenderTargetCount >= 4;
		if (decalTechnique == DecalTechnique.DBuffer && !flag)
		{
			Debug.LogError("Decal DBuffer technique requires MRT4 support.");
			return DecalTechnique.Invalid;
		}
		if (decalTechnique == DecalTechnique.GBuffer && !flag)
		{
			Debug.LogError("Decal useGBuffer option requires MRT4 support.");
			return DecalTechnique.Invalid;
		}
		return decalTechnique;
	}

	private bool IsAutomaticDBuffer()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			return false;
		}
		return !GraphicsSettings.HasShaderDefine(BuiltinShaderDefine.SHADER_API_MOBILE);
	}

	private bool RecreateSystemsIfNeeded(ScriptableRenderer renderer, in CameraData cameraData)
	{
		if (!m_RecreateSystems)
		{
			return true;
		}
		m_Technique = GetTechnique(renderer);
		if (m_Technique == DecalTechnique.Invalid)
		{
			return false;
		}
		m_DBufferSettings = GetDBufferSettings();
		m_ScreenSpaceSettings = GetScreenSpaceSettings();
		m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(m_CopyDepthPS);
		m_DBufferClearMaterial = CoreUtils.CreateEngineMaterial(m_DBufferClear);
		if (m_DecalEntityManager == null)
		{
			m_DecalEntityManager = sharedDecalEntityManager.Get();
		}
		m_DecalUpdateCachedSystem = new DecalUpdateCachedSystem(m_DecalEntityManager);
		m_DecalUpdateCulledSystem = new DecalUpdateCulledSystem(m_DecalEntityManager);
		m_DecalCreateDrawCallSystem = new DecalCreateDrawCallSystem(m_DecalEntityManager, m_Settings.maxDrawDistance);
		if (intermediateRendering)
		{
			m_DecalUpdateCullingGroupSystem = new DecalUpdateCullingGroupSystem(m_DecalEntityManager, m_Settings.maxDrawDistance);
		}
		else
		{
			m_DecalSkipCulledSystem = new DecalSkipCulledSystem(m_DecalEntityManager);
		}
		m_DrawErrorSystem = new DecalDrawErrorSystem(m_DecalEntityManager, m_Technique);
		UniversalRenderer universalRenderer = renderer as UniversalRenderer;
		switch (m_Technique)
		{
		case DecalTechnique.ScreenSpace:
			m_DecalDrawScreenSpaceSystem = new DecalDrawScreenSpaceSystem(m_DecalEntityManager);
			m_ScreenSpaceDecalRenderPass = new DecalScreenSpaceRenderPass(m_ScreenSpaceSettings, intermediateRendering ? m_DecalDrawScreenSpaceSystem : null, m_Settings.decalLayers);
			break;
		case DecalTechnique.GBuffer:
			m_DeferredLights = universalRenderer.deferredLights;
			m_DrawGBufferSystem = new DecalDrawGBufferSystem(m_DecalEntityManager);
			m_GBufferRenderPass = new DecalGBufferRenderPass(m_ScreenSpaceSettings, intermediateRendering ? m_DrawGBufferSystem : null, m_Settings.decalLayers);
			break;
		case DecalTechnique.DBuffer:
			m_CopyDepthPass = new CopyDepthPass((RenderPassEvent)201, m_CopyDepthMaterial);
			m_DecalDrawDBufferSystem = new DecalDrawDBufferSystem(m_DecalEntityManager);
			m_DBufferRenderPass = new DBufferRenderPass(m_DBufferClearMaterial, m_DBufferSettings, m_DecalDrawDBufferSystem, m_Settings.decalLayers);
			m_DecalDrawForwardEmissiveSystem = new DecalDrawFowardEmissiveSystem(m_DecalEntityManager);
			m_ForwardEmissivePass = new DecalForwardEmissivePass(m_DecalDrawForwardEmissiveSystem);
			break;
		}
		m_RecreateSystems = false;
		return true;
	}

	public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
	{
		if (cameraData.cameraType == CameraType.Preview || !RecreateSystemsIfNeeded(renderer, in cameraData))
		{
			return;
		}
		m_DecalEntityManager.Update();
		m_DecalUpdateCachedSystem.Execute();
		if (intermediateRendering)
		{
			m_DecalUpdateCullingGroupSystem.Execute(cameraData.camera);
		}
		else
		{
			m_DecalSkipCulledSystem.Execute(cameraData.camera);
			m_DecalCreateDrawCallSystem.Execute();
			if (m_Technique == DecalTechnique.ScreenSpace)
			{
				m_DecalDrawScreenSpaceSystem.Execute(in cameraData);
			}
			else if (m_Technique == DecalTechnique.GBuffer)
			{
				m_DrawGBufferSystem.Execute(in cameraData);
			}
		}
		m_DrawErrorSystem.Execute(in cameraData);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
		{
			return;
		}
		if (renderingData.cameraData.cameraType == CameraType.Preview)
		{
			renderer.EnqueuePass(m_DecalPreviewPass);
		}
		else if (RecreateSystemsIfNeeded(renderer, in renderingData.cameraData))
		{
			if (intermediateRendering)
			{
				m_DecalUpdateCulledSystem.Execute();
				m_DecalCreateDrawCallSystem.Execute();
			}
			switch (m_Technique)
			{
			case DecalTechnique.ScreenSpace:
				renderer.EnqueuePass(m_ScreenSpaceDecalRenderPass);
				break;
			case DecalTechnique.GBuffer:
				m_GBufferRenderPass.Setup(m_DeferredLights);
				renderer.EnqueuePass(m_GBufferRenderPass);
				break;
			case DecalTechnique.DBuffer:
				renderer.EnqueuePass(m_CopyDepthPass);
				renderer.EnqueuePass(m_DBufferRenderPass);
				renderer.EnqueuePass(m_ForwardEmissivePass);
				break;
			}
		}
	}

	internal override bool SupportsNativeRenderPass()
	{
		if (m_Technique != DecalTechnique.GBuffer)
		{
			return m_Technique == DecalTechnique.ScreenSpace;
		}
		return true;
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		if (UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
		{
			return;
		}
		if (m_Technique == DecalTechnique.DBuffer)
		{
			m_DBufferRenderPass.Setup(in renderingData.cameraData);
			UniversalRenderer universalRenderer = renderer as UniversalRenderer;
			if (universalRenderer.renderingModeActual == RenderingMode.Deferred)
			{
				m_DBufferRenderPass.Setup(in renderingData.cameraData, renderer.cameraDepthTargetHandle);
				m_CopyDepthPass.Setup(renderer.cameraDepthTargetHandle, universalRenderer.m_DepthTexture);
				return;
			}
			m_DBufferRenderPass.Setup(in renderingData.cameraData);
			m_CopyDepthPass.Setup(universalRenderer.m_DepthTexture, m_DBufferRenderPass.dBufferDepth);
			m_CopyDepthPass.CopyToDepth = true;
			m_CopyDepthPass.MssaSamples = 1;
		}
		else if (m_Technique == DecalTechnique.GBuffer && m_DeferredLights.UseRenderPass)
		{
			m_GBufferRenderPass.Configure(null, renderingData.cameraData.cameraTargetDescriptor);
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_DBufferRenderPass?.Dispose();
		CoreUtils.Destroy(m_CopyDepthMaterial);
		CoreUtils.Destroy(m_DBufferClearMaterial);
		if (m_DecalEntityManager != null)
		{
			m_DecalEntityManager = null;
			sharedDecalEntityManager.Release(m_DecalEntityManager);
		}
	}

	[Conditional("ADAPTIVE_PERFORMANCE_4_0_0_OR_NEWER")]
	private void ChangeAdaptivePerformanceDrawDistances()
	{
	}
}
