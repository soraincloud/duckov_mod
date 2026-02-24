using System;

namespace UnityEngine.Rendering.Universal;

[DisallowMultipleRendererFeature("Screen Space Ambient Occlusion")]
[Tooltip("The Ambient Occlusion effect darkens creases, holes, intersections and surfaces that are close to each other.")]
internal class ScreenSpaceAmbientOcclusion : ScriptableRendererFeature
{
	internal class ScreenSpaceAmbientOcclusionPass : ScriptableRenderPass
	{
		private enum BlurTypes
		{
			Bilateral,
			Gaussian,
			Kawase
		}

		private enum ShaderPasses
		{
			AmbientOcclusion,
			BilateralBlurHorizontal,
			BilateralBlurVertical,
			BilateralBlurFinal,
			BilateralAfterOpaque,
			GaussianBlurHorizontal,
			GaussianBlurVertical,
			GaussianAfterOpaque,
			KawaseBlur,
			KawaseAfterOpaque
		}

		internal string profilerTag;

		private bool m_SupportsR8RenderTextureFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8);

		private int m_BlueNoiseTextureIndex;

		private float m_BlurRandomOffsetX;

		private float m_BlurRandomOffsetY;

		private Material m_Material;

		private Texture2D[] m_BlueNoiseTextures;

		private Vector4[] m_CameraTopLeftCorner = new Vector4[2];

		private Vector4[] m_CameraXExtent = new Vector4[2];

		private Vector4[] m_CameraYExtent = new Vector4[2];

		private Vector4[] m_CameraZExtent = new Vector4[2];

		private RTHandle[] m_SSAOTextures = new RTHandle[4];

		private BlurTypes m_BlurType;

		private Matrix4x4[] m_CameraViewProjections = new Matrix4x4[2];

		private ProfilingSampler m_ProfilingSampler = ProfilingSampler.Get(URPProfileId.SSAO);

		private ScriptableRenderer m_Renderer;

		private RenderTextureDescriptor m_AOPassDescriptor;

		private ScreenSpaceAmbientOcclusionSettings m_CurrentSettings;

		private const int k_FinalTexID = 3;

		private const string k_SSAOTextureName = "_ScreenSpaceOcclusionTexture";

		private const string k_AmbientOcclusionParamName = "_AmbientOcclusionParam";

		internal static readonly int s_AmbientOcclusionParamID = Shader.PropertyToID("_AmbientOcclusionParam");

		private static readonly int s_SSAOParamsID = Shader.PropertyToID("_SSAOParams");

		private static readonly int s_SSAOBlueNoiseParamsID = Shader.PropertyToID("_SSAOBlueNoiseParams");

		private static readonly int s_LastKawasePass = Shader.PropertyToID("_LastKawasePass");

		private static readonly int s_BlueNoiseTextureID = Shader.PropertyToID("_BlueNoiseTexture");

		private static readonly int s_CameraViewXExtentID = Shader.PropertyToID("_CameraViewXExtent");

		private static readonly int s_CameraViewYExtentID = Shader.PropertyToID("_CameraViewYExtent");

		private static readonly int s_CameraViewZExtentID = Shader.PropertyToID("_CameraViewZExtent");

		private static readonly int s_ProjectionParams2ID = Shader.PropertyToID("_ProjectionParams2");

		private static readonly int s_KawaseBlurIterationID = Shader.PropertyToID("_KawaseBlurIteration");

		private static readonly int s_CameraViewProjectionsID = Shader.PropertyToID("_CameraViewProjections");

		private static readonly int s_CameraViewTopLeftCornerID = Shader.PropertyToID("_CameraViewTopLeftCorner");

		private static readonly int[] m_BilateralTexturesIndices = new int[4] { 0, 1, 2, 3 };

		private static readonly ShaderPasses[] m_BilateralPasses = new ShaderPasses[3]
		{
			ShaderPasses.BilateralBlurHorizontal,
			ShaderPasses.BilateralBlurVertical,
			ShaderPasses.BilateralBlurFinal
		};

		private static readonly ShaderPasses[] m_BilateralAfterOpaquePasses = new ShaderPasses[3]
		{
			ShaderPasses.BilateralBlurHorizontal,
			ShaderPasses.BilateralBlurVertical,
			ShaderPasses.BilateralAfterOpaque
		};

		private static readonly int[] m_GaussianTexturesIndices = new int[4] { 0, 1, 3, 3 };

		private static readonly ShaderPasses[] m_GaussianPasses = new ShaderPasses[2]
		{
			ShaderPasses.GaussianBlurHorizontal,
			ShaderPasses.GaussianBlurVertical
		};

		private static readonly ShaderPasses[] m_GaussianAfterOpaquePasses = new ShaderPasses[2]
		{
			ShaderPasses.GaussianBlurHorizontal,
			ShaderPasses.GaussianAfterOpaque
		};

		private static readonly int[] m_KawaseTexturesIndices = new int[2] { 0, 3 };

		private static readonly ShaderPasses[] m_KawasePasses = new ShaderPasses[1] { ShaderPasses.KawaseBlur };

		private static readonly ShaderPasses[] m_KawaseAfterOpaquePasses = new ShaderPasses[1] { ShaderPasses.KawaseAfterOpaque };

		private bool isRendererDeferred
		{
			get
			{
				if (m_Renderer != null && m_Renderer is UniversalRenderer)
				{
					return ((UniversalRenderer)m_Renderer).renderingModeRequested == RenderingMode.Deferred;
				}
				return false;
			}
		}

		internal ScreenSpaceAmbientOcclusionPass()
		{
			m_CurrentSettings = new ScreenSpaceAmbientOcclusionSettings();
		}

		internal bool Setup(ref ScreenSpaceAmbientOcclusionSettings featureSettings, ref ScriptableRenderer renderer, ref Material material, ref Texture2D[] blueNoiseTextures)
		{
			m_BlueNoiseTextures = blueNoiseTextures;
			m_Material = material;
			m_Renderer = renderer;
			m_CurrentSettings = featureSettings;
			if (isRendererDeferred)
			{
				base.renderPassEvent = (m_CurrentSettings.AfterOpaque ? RenderPassEvent.AfterRenderingOpaques : RenderPassEvent.AfterRenderingGbuffer);
				m_CurrentSettings.Source = ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals;
			}
			else
			{
				base.renderPassEvent = (m_CurrentSettings.AfterOpaque ? RenderPassEvent.AfterRenderingOpaques : ((RenderPassEvent)201));
			}
			switch (m_CurrentSettings.Source)
			{
			case ScreenSpaceAmbientOcclusionSettings.DepthSource.Depth:
				ConfigureInput(ScriptableRenderPassInput.Depth);
				break;
			case ScreenSpaceAmbientOcclusionSettings.DepthSource.DepthNormals:
				ConfigureInput(ScriptableRenderPassInput.Normal);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			switch (m_CurrentSettings.BlurQuality)
			{
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.High:
				m_BlurType = BlurTypes.Bilateral;
				break;
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Medium:
				m_BlurType = BlurTypes.Gaussian;
				break;
			case ScreenSpaceAmbientOcclusionSettings.BlurQualityOptions.Low:
				m_BlurType = BlurTypes.Kawase;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (m_Material != null && m_CurrentSettings.Intensity > 0f && m_CurrentSettings.Radius > 0f)
			{
				return m_CurrentSettings.Falloff > 0f;
			}
			return false;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			int num = ((!m_CurrentSettings.Downsample) ? 1 : 2);
			int num2 = ((!renderingData.cameraData.xr.enabled || !renderingData.cameraData.xr.singlePassEnabled) ? 1 : 2);
			for (int i = 0; i < num2; i++)
			{
				Matrix4x4 viewMatrix = renderingData.cameraData.GetViewMatrix(i);
				Matrix4x4 projectionMatrix = renderingData.cameraData.GetProjectionMatrix(i);
				m_CameraViewProjections[i] = projectionMatrix * viewMatrix;
				Matrix4x4 matrix4x = viewMatrix;
				matrix4x.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
				Matrix4x4 inverse = (projectionMatrix * matrix4x).inverse;
				Vector4 vector = inverse.MultiplyPoint(new Vector4(-1f, 1f, -1f, 1f));
				Vector4 vector2 = inverse.MultiplyPoint(new Vector4(1f, 1f, -1f, 1f));
				Vector4 vector3 = inverse.MultiplyPoint(new Vector4(-1f, -1f, -1f, 1f));
				Vector4 vector4 = inverse.MultiplyPoint(new Vector4(0f, 0f, 1f, 1f));
				m_CameraTopLeftCorner[i] = vector;
				m_CameraXExtent[i] = vector2 - vector;
				m_CameraYExtent[i] = vector3 - vector;
				m_CameraZExtent[i] = vector4;
			}
			m_Material.SetVector(s_ProjectionParams2ID, new Vector4(1f / renderingData.cameraData.camera.nearClipPlane, 0f, 0f, 0f));
			m_Material.SetMatrixArray(s_CameraViewProjectionsID, m_CameraViewProjections);
			m_Material.SetVectorArray(s_CameraViewTopLeftCornerID, m_CameraTopLeftCorner);
			m_Material.SetVectorArray(s_CameraViewXExtentID, m_CameraXExtent);
			m_Material.SetVectorArray(s_CameraViewYExtentID, m_CameraYExtent);
			m_Material.SetVectorArray(s_CameraViewZExtentID, m_CameraZExtent);
			CoreUtils.SetKeyword(m_Material, "_ORTHOGRAPHIC", renderingData.cameraData.camera.orthographic);
			CoreUtils.SetKeyword(m_Material, "_BLUE_NOISE", state: false);
			CoreUtils.SetKeyword(m_Material, "_INTERLEAVED_GRADIENT", state: false);
			switch (m_CurrentSettings.AOMethod)
			{
			case ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.BlueNoise:
			{
				CoreUtils.SetKeyword(m_Material, "_BLUE_NOISE", state: true);
				m_BlueNoiseTextureIndex = (m_BlueNoiseTextureIndex + 1) % m_BlueNoiseTextures.Length;
				m_BlurRandomOffsetX = Random.value;
				m_BlurRandomOffsetY = Random.value;
				Texture2D texture2D = m_BlueNoiseTextures[m_BlueNoiseTextureIndex];
				m_Material.SetTexture(s_BlueNoiseTextureID, texture2D);
				m_Material.SetVector(s_SSAOParamsID, new Vector4(m_CurrentSettings.Intensity, m_CurrentSettings.Radius * 1.5f, 1f / (float)num, m_CurrentSettings.Falloff));
				m_Material.SetVector(s_SSAOBlueNoiseParamsID, new Vector4((float)renderingData.cameraData.pixelWidth / (float)texture2D.width, (float)renderingData.cameraData.pixelHeight / (float)texture2D.height, m_BlurRandomOffsetX, m_BlurRandomOffsetY));
				break;
			}
			case ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.InterleavedGradient:
				CoreUtils.SetKeyword(m_Material, "_INTERLEAVED_GRADIENT", state: true);
				m_Material.SetVector(s_SSAOParamsID, new Vector4(m_CurrentSettings.Intensity, m_CurrentSettings.Radius, 1f / (float)num, m_CurrentSettings.Falloff));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_LOW", state: false);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_MEDIUM", state: false);
			CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_HIGH", state: false);
			switch (m_CurrentSettings.Samples)
			{
			case ScreenSpaceAmbientOcclusionSettings.AOSampleOption.High:
				CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_HIGH", state: true);
				break;
			case ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Medium:
				CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_MEDIUM", state: true);
				break;
			default:
				CoreUtils.SetKeyword(m_Material, "_SAMPLE_COUNT_LOW", state: true);
				break;
			}
			CoreUtils.SetKeyword(m_Material, "_ORTHOGRAPHIC", renderingData.cameraData.camera.orthographic);
			if (m_CurrentSettings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.Depth)
			{
				CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_NORMALS", state: false);
				switch (m_CurrentSettings.NormalSamples)
				{
				case ScreenSpaceAmbientOcclusionSettings.NormalQuality.Low:
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_LOW", state: true);
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_MEDIUM", state: false);
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_HIGH", state: false);
					break;
				case ScreenSpaceAmbientOcclusionSettings.NormalQuality.Medium:
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_LOW", state: false);
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_MEDIUM", state: true);
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_HIGH", state: false);
					break;
				case ScreenSpaceAmbientOcclusionSettings.NormalQuality.High:
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_LOW", state: false);
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_MEDIUM", state: false);
					CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_HIGH", state: true);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_LOW", state: false);
				CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_MEDIUM", state: false);
				CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_HIGH", state: false);
				CoreUtils.SetKeyword(m_Material, "_SOURCE_DEPTH_NORMALS", state: true);
			}
			RenderTextureDescriptor aOPassDescriptor = cameraTargetDescriptor;
			aOPassDescriptor.msaaSamples = 1;
			aOPassDescriptor.depthBufferBits = 0;
			m_AOPassDescriptor = aOPassDescriptor;
			m_AOPassDescriptor.width /= num;
			m_AOPassDescriptor.height /= num;
			bool flag = m_SupportsR8RenderTextureFormat && m_BlurType > BlurTypes.Bilateral;
			m_AOPassDescriptor.colorFormat = (flag ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
			RenderingUtils.ReAllocateIfNeeded(ref m_SSAOTextures[0], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSAO_OcclusionTexture0");
			RenderingUtils.ReAllocateIfNeeded(ref m_SSAOTextures[1], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSAO_OcclusionTexture1");
			RenderingUtils.ReAllocateIfNeeded(ref m_SSAOTextures[2], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSAO_OcclusionTexture2");
			m_AOPassDescriptor.width *= num;
			m_AOPassDescriptor.height *= num;
			m_AOPassDescriptor.colorFormat = (m_SupportsR8RenderTextureFormat ? RenderTextureFormat.R8 : RenderTextureFormat.ARGB32);
			RenderingUtils.ReAllocateIfNeeded(ref m_SSAOTextures[3], in m_AOPassDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_SSAO_OcclusionTexture");
			ConfigureTarget(m_CurrentSettings.AfterOpaque ? m_Renderer.cameraColorTargetHandle : m_SSAOTextures[3]);
			ConfigureClear(ClearFlag.None, Color.white);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (m_Material == null)
			{
				Debug.LogErrorFormat("{0}.Execute(): Missing material. ScreenSpaceAmbientOcclusion pass will not execute. Check for missing reference in the renderer resources.", GetType().Name);
				return;
			}
			CommandBuffer cmd = renderingData.commandBuffer;
			using (new ProfilingScope(cmd, m_ProfilingSampler))
			{
				if (!m_CurrentSettings.AfterOpaque)
				{
					CoreUtils.SetKeyword(cmd, "_SCREEN_SPACE_OCCLUSION", state: true);
				}
				PostProcessUtils.SetSourceSize(cmd, m_AOPassDescriptor);
				cmd.SetGlobalTexture("_ScreenSpaceOcclusionTexture", m_SSAOTextures[3]);
				if (renderingData.cameraData.xr.supportsFoveatedRendering)
				{
					if (m_CurrentSettings.Downsample || SystemInfo.foveatedRenderingCaps == FoveatedRenderingCaps.NonUniformRaster || (SystemInfo.foveatedRenderingCaps == FoveatedRenderingCaps.FoveationImage && m_CurrentSettings.Source == ScreenSpaceAmbientOcclusionSettings.DepthSource.Depth))
					{
						cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
					}
					else if (SystemInfo.foveatedRenderingCaps == FoveatedRenderingCaps.FoveationImage)
					{
						cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
					}
				}
				if (m_BlurType == BlurTypes.Kawase)
				{
					cmd.SetGlobalInt(s_LastKawasePass, 1);
					cmd.SetGlobalFloat(s_KawaseBlurIterationID, 0f);
				}
				GetPassOrder(m_BlurType, m_CurrentSettings.AfterOpaque, out var textureIndices, out var shaderPasses);
				RTHandle baseMap = m_Renderer.cameraDepthTargetHandle;
				RenderAndSetBaseMap(ref cmd, ref renderingData, ref m_Renderer, ref m_Material, ref baseMap, ref m_SSAOTextures[0], ShaderPasses.AmbientOcclusion);
				for (int i = 0; i < shaderPasses.Length; i++)
				{
					int num = textureIndices[i];
					int num2 = textureIndices[i + 1];
					RenderAndSetBaseMap(ref cmd, ref renderingData, ref m_Renderer, ref m_Material, ref m_SSAOTextures[num], ref m_SSAOTextures[num2], shaderPasses[i]);
				}
				cmd.SetGlobalVector(s_AmbientOcclusionParamID, new Vector4(1f, 0f, 0f, m_CurrentSettings.DirectLightingStrength));
			}
		}

		private static void RenderAndSetBaseMap(ref CommandBuffer cmd, ref RenderingData renderingData, ref ScriptableRenderer renderer, ref Material mat, ref RTHandle baseMap, ref RTHandle target, ShaderPasses pass)
		{
			if (IsAfterOpaquePass(ref pass))
			{
				Blitter.BlitCameraTexture(cmd, baseMap, renderer.cameraColorTargetHandle, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, mat, (int)pass);
			}
			else if (baseMap.rt == null)
			{
				Vector2 vector = (baseMap.useScaling ? new Vector2(baseMap.rtHandleProperties.rtHandleScale.x, baseMap.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				CoreUtils.SetRenderTarget(cmd, target);
				Blitter.BlitTexture(cmd, baseMap.nameID, vector, mat, (int)pass);
			}
			else
			{
				Blitter.BlitCameraTexture(cmd, baseMap, target, mat, (int)pass);
			}
		}

		private static void GetPassOrder(BlurTypes blurType, bool isAfterOpaque, out int[] textureIndices, out ShaderPasses[] shaderPasses)
		{
			switch (blurType)
			{
			case BlurTypes.Bilateral:
				textureIndices = m_BilateralTexturesIndices;
				shaderPasses = (isAfterOpaque ? m_BilateralAfterOpaquePasses : m_BilateralPasses);
				break;
			case BlurTypes.Gaussian:
				textureIndices = m_GaussianTexturesIndices;
				shaderPasses = (isAfterOpaque ? m_GaussianAfterOpaquePasses : m_GaussianPasses);
				break;
			case BlurTypes.Kawase:
				textureIndices = m_KawaseTexturesIndices;
				shaderPasses = (isAfterOpaque ? m_KawaseAfterOpaquePasses : m_KawasePasses);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private static bool IsAfterOpaquePass(ref ShaderPasses pass)
		{
			if (pass != ShaderPasses.BilateralAfterOpaque && pass != ShaderPasses.GaussianAfterOpaque)
			{
				return pass == ShaderPasses.KawaseAfterOpaque;
			}
			return true;
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			if (cmd == null)
			{
				throw new ArgumentNullException("cmd");
			}
			if (!m_CurrentSettings.AfterOpaque)
			{
				CoreUtils.SetKeyword(cmd, "_SCREEN_SPACE_OCCLUSION", state: false);
			}
		}

		public void Dispose()
		{
			m_SSAOTextures[0]?.Release();
			m_SSAOTextures[1]?.Release();
			m_SSAOTextures[2]?.Release();
			m_SSAOTextures[3]?.Release();
		}
	}

	[SerializeField]
	private ScreenSpaceAmbientOcclusionSettings m_Settings = new ScreenSpaceAmbientOcclusionSettings();

	[SerializeField]
	[HideInInspector]
	[Reload("Textures/BlueNoise256/LDR_LLL1_{0}.png", 0, 7, ReloadAttribute.Package.Root)]
	internal Texture2D[] m_BlueNoise256Textures;

	[SerializeField]
	[HideInInspector]
	[Reload("Shaders/Utils/ScreenSpaceAmbientOcclusion.shader", ReloadAttribute.Package.Root)]
	private Shader m_Shader;

	private Material m_Material;

	private ScreenSpaceAmbientOcclusionPass m_SSAOPass;

	internal const string k_AOInterleavedGradientKeyword = "_INTERLEAVED_GRADIENT";

	internal const string k_AOBlueNoiseKeyword = "_BLUE_NOISE";

	internal const string k_OrthographicCameraKeyword = "_ORTHOGRAPHIC";

	internal const string k_SourceDepthLowKeyword = "_SOURCE_DEPTH_LOW";

	internal const string k_SourceDepthMediumKeyword = "_SOURCE_DEPTH_MEDIUM";

	internal const string k_SourceDepthHighKeyword = "_SOURCE_DEPTH_HIGH";

	internal const string k_SourceDepthNormalsKeyword = "_SOURCE_DEPTH_NORMALS";

	internal const string k_SampleCountLowKeyword = "_SAMPLE_COUNT_LOW";

	internal const string k_SampleCountMediumKeyword = "_SAMPLE_COUNT_MEDIUM";

	internal const string k_SampleCountHighKeyword = "_SAMPLE_COUNT_HIGH";

	internal ref ScreenSpaceAmbientOcclusionSettings settings => ref m_Settings;

	public override void Create()
	{
		if (m_SSAOPass == null)
		{
			m_SSAOPass = new ScreenSpaceAmbientOcclusionPass();
		}
		if (m_Settings.SampleCount > 0)
		{
			m_Settings.AOMethod = ScreenSpaceAmbientOcclusionSettings.AOMethodOptions.InterleavedGradient;
			if (m_Settings.SampleCount > 11)
			{
				m_Settings.Samples = ScreenSpaceAmbientOcclusionSettings.AOSampleOption.High;
			}
			else if (m_Settings.SampleCount > 8)
			{
				m_Settings.Samples = ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Medium;
			}
			else
			{
				m_Settings.Samples = ScreenSpaceAmbientOcclusionSettings.AOSampleOption.Low;
			}
			m_Settings.SampleCount = -1;
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
		{
			if (!GetMaterials())
			{
				Debug.LogErrorFormat("{0}.AddRenderPasses(): Missing material. {1} render pass will not be added.", GetType().Name, base.name);
			}
			else if (m_SSAOPass.Setup(ref m_Settings, ref renderer, ref m_Material, ref m_BlueNoise256Textures))
			{
				renderer.EnqueuePass(m_SSAOPass);
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		m_SSAOPass?.Dispose();
		m_SSAOPass = null;
		CoreUtils.Destroy(m_Material);
	}

	private bool GetMaterials()
	{
		if (m_Material == null && m_Shader != null)
		{
			m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
		}
		return m_Material != null;
	}
}
