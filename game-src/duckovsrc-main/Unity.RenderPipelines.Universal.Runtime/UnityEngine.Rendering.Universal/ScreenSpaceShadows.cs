using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

[DisallowMultipleRendererFeature("Screen Space Shadows")]
[Tooltip("Screen Space Shadows")]
internal class ScreenSpaceShadows : ScriptableRendererFeature
{
	private class ScreenSpaceShadowsPass : ScriptableRenderPass
	{
		private static string m_ProfilerTag = "ScreenSpaceShadows";

		private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

		private Material m_Material;

		private ScreenSpaceShadowsSettings m_CurrentSettings;

		private RTHandle m_RenderTarget;

		internal ScreenSpaceShadowsPass()
		{
			m_CurrentSettings = new ScreenSpaceShadowsSettings();
		}

		public void Dispose()
		{
			m_RenderTarget?.Release();
		}

		internal bool Setup(ScreenSpaceShadowsSettings featureSettings, Material material)
		{
			m_CurrentSettings = featureSettings;
			m_Material = material;
			ConfigureInput(ScriptableRenderPassInput.Depth);
			return m_Material != null;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = 0;
			descriptor.msaaSamples = 1;
			descriptor.graphicsFormat = (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R8_UNorm, FormatUsage.Blend) ? GraphicsFormat.R8_UNorm : GraphicsFormat.B8G8R8A8_UNorm);
			RenderingUtils.ReAllocateIfNeeded(ref m_RenderTarget, in descriptor, FilterMode.Point, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_ScreenSpaceShadowmapTexture");
			cmd.SetGlobalTexture(m_RenderTarget.name, m_RenderTarget.nameID);
			ConfigureTarget(m_RenderTarget);
			ConfigureClear(ClearFlag.None, Color.white);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (m_Material == null)
			{
				Debug.LogErrorFormat("{0}.Execute(): Missing material. ScreenSpaceShadows pass will not execute. Check for missing reference in the renderer resources.", GetType().Name);
				return;
			}
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				Blitter.BlitCameraTexture(commandBuffer, m_RenderTarget, m_RenderTarget, m_Material, 0);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_SCREEN", state: true);
			}
		}
	}

	private class ScreenSpaceShadowsPostPass : ScriptableRenderPass
	{
		private static string m_ProfilerTag = "ScreenSpaceShadows Post";

		private static ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);

		private static readonly RTHandle k_CurrentActive = RTHandles.Alloc(BuiltinRenderTextureType.CurrentActive);

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			ConfigureTarget(k_CurrentActive);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = renderingData.commandBuffer;
			using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
			{
				int mainLightShadowCascadesCount = renderingData.shadowData.mainLightShadowCascadesCount;
				bool supportsMainLightShadows = renderingData.shadowData.supportsMainLightShadows;
				bool state = supportsMainLightShadows && mainLightShadowCascadesCount == 1;
				bool state2 = supportsMainLightShadows && mainLightShadowCascadesCount > 1;
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_SCREEN", state: false);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS", state);
				CoreUtils.SetKeyword(commandBuffer, "_MAIN_LIGHT_SHADOWS_CASCADE", state2);
			}
		}
	}

	[SerializeField]
	[HideInInspector]
	private Shader m_Shader;

	[SerializeField]
	private ScreenSpaceShadowsSettings m_Settings = new ScreenSpaceShadowsSettings();

	private Material m_Material;

	private ScreenSpaceShadowsPass m_SSShadowsPass;

	private ScreenSpaceShadowsPostPass m_SSShadowsPostPass;

	private const string k_ShaderName = "Hidden/Universal Render Pipeline/ScreenSpaceShadows";

	public override void Create()
	{
		if (m_SSShadowsPass == null)
		{
			m_SSShadowsPass = new ScreenSpaceShadowsPass();
		}
		if (m_SSShadowsPostPass == null)
		{
			m_SSShadowsPostPass = new ScreenSpaceShadowsPostPass();
		}
		LoadMaterial();
		m_SSShadowsPass.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
		m_SSShadowsPostPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
		{
			if (!LoadMaterial())
			{
				Debug.LogErrorFormat("{0}.AddRenderPasses(): Missing material. {1} render pass will not be added. Check for missing reference in the renderer resources.", GetType().Name, base.name);
			}
			else if (renderingData.shadowData.supportsMainLightShadows && renderingData.lightData.mainLightIndex != -1 && m_SSShadowsPass.Setup(m_Settings, m_Material))
			{
				bool flag = renderer is UniversalRenderer && ((UniversalRenderer)renderer).renderingModeRequested == RenderingMode.Deferred;
				m_SSShadowsPass.renderPassEvent = (flag ? RenderPassEvent.AfterRenderingGbuffer : ((RenderPassEvent)201));
				renderer.EnqueuePass(m_SSShadowsPass);
				renderer.EnqueuePass(m_SSShadowsPostPass);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_SSShadowsPass?.Dispose();
		m_SSShadowsPass = null;
		CoreUtils.Destroy(m_Material);
	}

	private bool LoadMaterial()
	{
		if (m_Material != null)
		{
			return true;
		}
		if (m_Shader == null)
		{
			m_Shader = Shader.Find("Hidden/Universal Render Pipeline/ScreenSpaceShadows");
			if (m_Shader == null)
			{
				return false;
			}
		}
		m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
		return m_Material != null;
	}
}
