using System;
using System.ComponentModel;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

[ExcludeFromPreset]
public class UniversalRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver
{
	[Serializable]
	[ReloadGroup]
	public sealed class TextureResources
	{
		[Reload("Textures/BlueNoise64/L/LDR_LLL1_0.png", ReloadAttribute.Package.Root)]
		public Texture2D blueNoise64LTex;

		[Reload("Textures/BayerMatrix.png", ReloadAttribute.Package.Root)]
		public Texture2D bayerMatrixTex;

		public bool NeedsReload()
		{
			if (!(blueNoise64LTex == null))
			{
				return bayerMatrixTex == null;
			}
			return true;
		}
	}

	private Shader m_DefaultShader;

	private ScriptableRenderer[] m_Renderers = new ScriptableRenderer[1];

	[SerializeField]
	private int k_AssetVersion = 11;

	[SerializeField]
	private int k_AssetPreviousVersion = 11;

	[SerializeField]
	private RendererType m_RendererType = RendererType.UniversalRenderer;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use m_RendererDataList instead.")]
	[SerializeField]
	internal ScriptableRendererData m_RendererData;

	[SerializeField]
	internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];

	[SerializeField]
	internal int m_DefaultRendererIndex;

	[SerializeField]
	private bool m_RequireDepthTexture;

	[SerializeField]
	private bool m_RequireOpaqueTexture;

	[SerializeField]
	private Downsampling m_OpaqueDownsampling = Downsampling._2xBilinear;

	[SerializeField]
	private bool m_SupportsTerrainHoles = true;

	[SerializeField]
	private bool m_SupportsHDR = true;

	[SerializeField]
	private HDRColorBufferPrecision m_HDRColorBufferPrecision;

	[SerializeField]
	private MsaaQuality m_MSAA = MsaaQuality.Disabled;

	[SerializeField]
	private float m_RenderScale = 1f;

	[SerializeField]
	private UpscalingFilterSelection m_UpscalingFilter;

	[SerializeField]
	private bool m_FsrOverrideSharpness;

	[SerializeField]
	private float m_FsrSharpness = 0.92f;

	[SerializeField]
	private bool m_EnableLODCrossFade = true;

	[SerializeField]
	private LODCrossFadeDitheringType m_LODCrossFadeDitheringType = LODCrossFadeDitheringType.BlueNoise;

	[SerializeField]
	private ShEvalMode m_ShEvalMode;

	[SerializeField]
	private LightRenderingMode m_MainLightRenderingMode = LightRenderingMode.PerPixel;

	[SerializeField]
	private bool m_MainLightShadowsSupported = true;

	[SerializeField]
	private ShadowResolution m_MainLightShadowmapResolution = ShadowResolution._2048;

	[SerializeField]
	private LightRenderingMode m_AdditionalLightsRenderingMode = LightRenderingMode.PerPixel;

	[SerializeField]
	private int m_AdditionalLightsPerObjectLimit = 4;

	[SerializeField]
	private bool m_AdditionalLightShadowsSupported;

	[SerializeField]
	private ShadowResolution m_AdditionalLightsShadowmapResolution = ShadowResolution._2048;

	[SerializeField]
	private int m_AdditionalLightsShadowResolutionTierLow = AdditionalLightsDefaultShadowResolutionTierLow;

	[SerializeField]
	private int m_AdditionalLightsShadowResolutionTierMedium = AdditionalLightsDefaultShadowResolutionTierMedium;

	[SerializeField]
	private int m_AdditionalLightsShadowResolutionTierHigh = AdditionalLightsDefaultShadowResolutionTierHigh;

	[SerializeField]
	private bool m_ReflectionProbeBlending;

	[SerializeField]
	private bool m_ReflectionProbeBoxProjection;

	[SerializeField]
	private float m_ShadowDistance = 50f;

	[SerializeField]
	private int m_ShadowCascadeCount = 1;

	[SerializeField]
	private float m_Cascade2Split = 0.25f;

	[SerializeField]
	private Vector2 m_Cascade3Split = new Vector2(0.1f, 0.3f);

	[SerializeField]
	private Vector3 m_Cascade4Split = new Vector3(0.067f, 0.2f, 0.467f);

	[SerializeField]
	private float m_CascadeBorder = 0.2f;

	[SerializeField]
	private float m_ShadowDepthBias = 1f;

	[SerializeField]
	private float m_ShadowNormalBias = 1f;

	[SerializeField]
	private bool m_SoftShadowsSupported;

	[SerializeField]
	private bool m_ConservativeEnclosingSphere;

	[SerializeField]
	private int m_NumIterationsEnclosingSphere = 64;

	[SerializeField]
	private SoftShadowQuality m_SoftShadowQuality = SoftShadowQuality.Medium;

	[SerializeField]
	private LightCookieResolution m_AdditionalLightsCookieResolution = LightCookieResolution._2048;

	[SerializeField]
	private LightCookieFormat m_AdditionalLightsCookieFormat = LightCookieFormat.ColorHigh;

	[SerializeField]
	private bool m_UseSRPBatcher = true;

	[SerializeField]
	private bool m_SupportsDynamicBatching;

	[SerializeField]
	private bool m_MixedLightingSupported = true;

	[SerializeField]
	private bool m_SupportsLightCookies = true;

	[SerializeField]
	private bool m_SupportsLightLayers;

	[SerializeField]
	[Obsolete]
	private PipelineDebugLevel m_DebugLevel;

	[SerializeField]
	private StoreActionsOptimization m_StoreActionsOptimization;

	[SerializeField]
	private bool m_EnableRenderGraph;

	[SerializeField]
	private bool m_UseAdaptivePerformance = true;

	[SerializeField]
	private ColorGradingMode m_ColorGradingMode;

	[SerializeField]
	private int m_ColorGradingLutSize = 32;

	[SerializeField]
	private bool m_UseFastSRGBLinearConversion;

	[SerializeField]
	private bool m_SupportDataDrivenLensFlare = true;

	[SerializeField]
	private ShadowQuality m_ShadowType = ShadowQuality.HardShadows;

	[SerializeField]
	private bool m_LocalShadowsSupported;

	[SerializeField]
	private ShadowResolution m_LocalShadowsAtlasResolution = ShadowResolution._256;

	[SerializeField]
	private int m_MaxPixelLights;

	[SerializeField]
	private ShadowResolution m_ShadowAtlasResolution = ShadowResolution._256;

	[SerializeField]
	private VolumeFrameworkUpdateMode m_VolumeFrameworkUpdateMode;

	[SerializeField]
	private TextureResources m_Textures;

	public const int k_MinLutSize = 16;

	public const int k_MaxLutSize = 65;

	internal const int k_ShadowCascadeMinCount = 1;

	internal const int k_ShadowCascadeMaxCount = 4;

	public static readonly int AdditionalLightsDefaultShadowResolutionTierLow = 256;

	public static readonly int AdditionalLightsDefaultShadowResolutionTierMedium = 512;

	public static readonly int AdditionalLightsDefaultShadowResolutionTierHigh = 1024;

	private static GraphicsFormat[][] s_LightCookieFormatList = new GraphicsFormat[5][]
	{
		new GraphicsFormat[1] { GraphicsFormat.R8_UNorm },
		new GraphicsFormat[1] { GraphicsFormat.R16_UNorm },
		new GraphicsFormat[4]
		{
			GraphicsFormat.R5G6B5_UNormPack16,
			GraphicsFormat.B5G6R5_UNormPack16,
			GraphicsFormat.R5G5B5A1_UNormPack16,
			GraphicsFormat.B5G5R5A1_UNormPack16
		},
		new GraphicsFormat[3]
		{
			GraphicsFormat.A2B10G10R10_UNormPack32,
			GraphicsFormat.R8G8B8A8_SRGB,
			GraphicsFormat.B8G8R8A8_SRGB
		},
		new GraphicsFormat[1] { GraphicsFormat.B10G11R11_UFloatPack32 }
	};

	[SerializeField]
	private int m_ShaderVariantLogLevel;

	[Obsolete("This is obsolete, please use shadowCascadeCount instead.", false)]
	[SerializeField]
	private ShadowCascadesOption m_ShadowCascades;

	public ScriptableRenderer scriptableRenderer
	{
		get
		{
			if (m_RendererDataList?.Length > m_DefaultRendererIndex && m_RendererDataList[m_DefaultRendererIndex] == null)
			{
				Debug.LogError("Default renderer is missing from the current Pipeline Asset.", this);
				return null;
			}
			if (scriptableRendererData.isInvalidated || m_Renderers[m_DefaultRendererIndex] == null)
			{
				DestroyRenderer(ref m_Renderers[m_DefaultRendererIndex]);
				m_Renderers[m_DefaultRendererIndex] = scriptableRendererData.InternalCreateRenderer();
			}
			return m_Renderers[m_DefaultRendererIndex];
		}
	}

	internal ScriptableRendererData scriptableRendererData
	{
		get
		{
			if (m_RendererDataList[m_DefaultRendererIndex] == null)
			{
				CreatePipeline();
			}
			return m_RendererDataList[m_DefaultRendererIndex];
		}
	}

	internal GraphicsFormat additionalLightsCookieFormat
	{
		get
		{
			GraphicsFormat graphicsFormat = GraphicsFormat.None;
			GraphicsFormat[] array = s_LightCookieFormatList[(int)m_AdditionalLightsCookieFormat];
			foreach (GraphicsFormat graphicsFormat2 in array)
			{
				if (SystemInfo.IsFormatSupported(graphicsFormat2, FormatUsage.Render))
				{
					graphicsFormat = graphicsFormat2;
					break;
				}
			}
			if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
			{
				graphicsFormat = GraphicsFormatUtility.GetLinearFormat(graphicsFormat);
			}
			if (graphicsFormat == GraphicsFormat.None)
			{
				graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
				Debug.LogWarning($"Additional Lights Cookie Format ({m_AdditionalLightsCookieFormat.ToString()}) is not supported by the platform. Falling back to {GraphicsFormatUtility.GetBlockSize(graphicsFormat) * 8}-bit format ({GraphicsFormatUtility.GetFormatString(graphicsFormat)})");
			}
			return graphicsFormat;
		}
	}

	internal Vector2Int additionalLightsCookieResolution => new Vector2Int((int)m_AdditionalLightsCookieResolution, (int)m_AdditionalLightsCookieResolution);

	internal int[] rendererIndexList
	{
		get
		{
			int[] array = new int[m_RendererDataList.Length + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = i - 1;
			}
			return array;
		}
	}

	public bool supportsCameraDepthTexture
	{
		get
		{
			return m_RequireDepthTexture;
		}
		set
		{
			m_RequireDepthTexture = value;
		}
	}

	public bool supportsCameraOpaqueTexture
	{
		get
		{
			return m_RequireOpaqueTexture;
		}
		set
		{
			m_RequireOpaqueTexture = value;
		}
	}

	public Downsampling opaqueDownsampling => m_OpaqueDownsampling;

	public bool supportsTerrainHoles => m_SupportsTerrainHoles;

	public StoreActionsOptimization storeActionsOptimization
	{
		get
		{
			return m_StoreActionsOptimization;
		}
		set
		{
			m_StoreActionsOptimization = value;
		}
	}

	public bool supportsHDR
	{
		get
		{
			return m_SupportsHDR;
		}
		set
		{
			m_SupportsHDR = value;
		}
	}

	public HDRColorBufferPrecision hdrColorBufferPrecision
	{
		get
		{
			return m_HDRColorBufferPrecision;
		}
		set
		{
			m_HDRColorBufferPrecision = value;
		}
	}

	public int msaaSampleCount
	{
		get
		{
			return (int)m_MSAA;
		}
		set
		{
			m_MSAA = (MsaaQuality)value;
		}
	}

	public float renderScale
	{
		get
		{
			return m_RenderScale;
		}
		set
		{
			m_RenderScale = ValidateRenderScale(value);
		}
	}

	public bool enableLODCrossFade => m_EnableLODCrossFade;

	public LODCrossFadeDitheringType lodCrossFadeDitheringType => m_LODCrossFadeDitheringType;

	public UpscalingFilterSelection upscalingFilter
	{
		get
		{
			return m_UpscalingFilter;
		}
		set
		{
			m_UpscalingFilter = value;
		}
	}

	public bool fsrOverrideSharpness
	{
		get
		{
			return m_FsrOverrideSharpness;
		}
		set
		{
			m_FsrOverrideSharpness = value;
		}
	}

	public float fsrSharpness
	{
		get
		{
			return m_FsrSharpness;
		}
		set
		{
			m_FsrSharpness = value;
		}
	}

	public ShEvalMode shEvalMode
	{
		get
		{
			return m_ShEvalMode;
		}
		internal set
		{
			m_ShEvalMode = value;
		}
	}

	public LightRenderingMode mainLightRenderingMode
	{
		get
		{
			return m_MainLightRenderingMode;
		}
		internal set
		{
			m_MainLightRenderingMode = value;
		}
	}

	public bool supportsMainLightShadows
	{
		get
		{
			return m_MainLightShadowsSupported;
		}
		internal set
		{
			m_MainLightShadowsSupported = value;
		}
	}

	public int mainLightShadowmapResolution
	{
		get
		{
			return (int)m_MainLightShadowmapResolution;
		}
		set
		{
			m_MainLightShadowmapResolution = (ShadowResolution)value;
		}
	}

	public LightRenderingMode additionalLightsRenderingMode
	{
		get
		{
			return m_AdditionalLightsRenderingMode;
		}
		internal set
		{
			m_AdditionalLightsRenderingMode = value;
		}
	}

	public int maxAdditionalLightsCount
	{
		get
		{
			return m_AdditionalLightsPerObjectLimit;
		}
		set
		{
			m_AdditionalLightsPerObjectLimit = ValidatePerObjectLights(value);
		}
	}

	public bool supportsAdditionalLightShadows
	{
		get
		{
			return m_AdditionalLightShadowsSupported;
		}
		internal set
		{
			m_AdditionalLightShadowsSupported = value;
		}
	}

	public int additionalLightsShadowmapResolution
	{
		get
		{
			return (int)m_AdditionalLightsShadowmapResolution;
		}
		set
		{
			m_AdditionalLightsShadowmapResolution = (ShadowResolution)value;
		}
	}

	public int additionalLightsShadowResolutionTierLow
	{
		get
		{
			return m_AdditionalLightsShadowResolutionTierLow;
		}
		internal set
		{
			m_AdditionalLightsShadowResolutionTierLow = value;
		}
	}

	public int additionalLightsShadowResolutionTierMedium
	{
		get
		{
			return m_AdditionalLightsShadowResolutionTierMedium;
		}
		internal set
		{
			m_AdditionalLightsShadowResolutionTierMedium = value;
		}
	}

	public int additionalLightsShadowResolutionTierHigh
	{
		get
		{
			return m_AdditionalLightsShadowResolutionTierHigh;
		}
		internal set
		{
			m_AdditionalLightsShadowResolutionTierHigh = value;
		}
	}

	public bool reflectionProbeBlending
	{
		get
		{
			return m_ReflectionProbeBlending;
		}
		internal set
		{
			m_ReflectionProbeBlending = value;
		}
	}

	public bool reflectionProbeBoxProjection
	{
		get
		{
			return m_ReflectionProbeBoxProjection;
		}
		internal set
		{
			m_ReflectionProbeBoxProjection = value;
		}
	}

	public float shadowDistance
	{
		get
		{
			return m_ShadowDistance;
		}
		set
		{
			m_ShadowDistance = Mathf.Max(0f, value);
		}
	}

	public int shadowCascadeCount
	{
		get
		{
			return m_ShadowCascadeCount;
		}
		set
		{
			if (value < 1 || value > 4)
			{
				throw new ArgumentException($"Value ({value}) needs to be between {1} and {4}.");
			}
			m_ShadowCascadeCount = value;
		}
	}

	public float cascade2Split
	{
		get
		{
			return m_Cascade2Split;
		}
		internal set
		{
			m_Cascade2Split = value;
		}
	}

	public Vector2 cascade3Split
	{
		get
		{
			return m_Cascade3Split;
		}
		internal set
		{
			m_Cascade3Split = value;
		}
	}

	public Vector3 cascade4Split
	{
		get
		{
			return m_Cascade4Split;
		}
		internal set
		{
			m_Cascade4Split = value;
		}
	}

	public float cascadeBorder
	{
		get
		{
			return m_CascadeBorder;
		}
		set
		{
			m_CascadeBorder = value;
		}
	}

	public float shadowDepthBias
	{
		get
		{
			return m_ShadowDepthBias;
		}
		set
		{
			m_ShadowDepthBias = ValidateShadowBias(value);
		}
	}

	public float shadowNormalBias
	{
		get
		{
			return m_ShadowNormalBias;
		}
		set
		{
			m_ShadowNormalBias = ValidateShadowBias(value);
		}
	}

	public bool supportsSoftShadows
	{
		get
		{
			return m_SoftShadowsSupported;
		}
		internal set
		{
			m_SoftShadowsSupported = value;
		}
	}

	internal SoftShadowQuality softShadowQuality
	{
		get
		{
			return m_SoftShadowQuality;
		}
		set
		{
			m_SoftShadowQuality = value;
		}
	}

	public bool supportsDynamicBatching
	{
		get
		{
			return m_SupportsDynamicBatching;
		}
		set
		{
			m_SupportsDynamicBatching = value;
		}
	}

	public bool supportsMixedLighting => m_MixedLightingSupported;

	public bool supportsLightCookies => m_SupportsLightCookies;

	[Obsolete("This is obsolete, UnityEngine.Rendering.ShaderVariantLogLevel instead.", false)]
	public bool supportsLightLayers => m_SupportsLightLayers;

	public bool useRenderingLayers => m_SupportsLightLayers;

	public VolumeFrameworkUpdateMode volumeFrameworkUpdateMode => m_VolumeFrameworkUpdateMode;

	[Obsolete("PipelineDebugLevel is deprecated and replaced to use the profiler. Calling debugLevel is not necessary.", false)]
	public PipelineDebugLevel debugLevel => PipelineDebugLevel.Disabled;

	public bool useSRPBatcher
	{
		get
		{
			return m_UseSRPBatcher;
		}
		set
		{
			m_UseSRPBatcher = value;
		}
	}

	internal bool enableRenderGraph
	{
		get
		{
			return m_EnableRenderGraph;
		}
		set
		{
			m_EnableRenderGraph = value;
		}
	}

	public ColorGradingMode colorGradingMode
	{
		get
		{
			return m_ColorGradingMode;
		}
		set
		{
			m_ColorGradingMode = value;
		}
	}

	public int colorGradingLutSize
	{
		get
		{
			return m_ColorGradingLutSize;
		}
		set
		{
			m_ColorGradingLutSize = Mathf.Clamp(value, 16, 65);
		}
	}

	public bool useFastSRGBLinearConversion => m_UseFastSRGBLinearConversion;

	public bool supportDataDrivenLensFlare => m_SupportDataDrivenLensFlare;

	public bool useAdaptivePerformance
	{
		get
		{
			return m_UseAdaptivePerformance;
		}
		set
		{
			m_UseAdaptivePerformance = value;
		}
	}

	public bool conservativeEnclosingSphere
	{
		get
		{
			return m_ConservativeEnclosingSphere;
		}
		set
		{
			m_ConservativeEnclosingSphere = value;
		}
	}

	public int numIterationsEnclosingSphere
	{
		get
		{
			return m_NumIterationsEnclosingSphere;
		}
		set
		{
			m_NumIterationsEnclosingSphere = value;
		}
	}

	public override Material defaultMaterial => GetMaterial(DefaultMaterialType.Standard);

	public override Material defaultParticleMaterial => GetMaterial(DefaultMaterialType.Particle);

	public override Material defaultLineMaterial => GetMaterial(DefaultMaterialType.Particle);

	public override Material defaultTerrainMaterial => GetMaterial(DefaultMaterialType.Terrain);

	public override Material defaultUIMaterial => GetMaterial(DefaultMaterialType.UnityBuiltinDefault);

	public override Material defaultUIOverdrawMaterial => GetMaterial(DefaultMaterialType.UnityBuiltinDefault);

	public override Material defaultUIETC1SupportedMaterial => GetMaterial(DefaultMaterialType.UnityBuiltinDefault);

	public override Material default2DMaterial => GetMaterial(DefaultMaterialType.Sprite);

	public override Material default2DMaskMaterial => GetMaterial(DefaultMaterialType.SpriteMask);

	public Material decalMaterial => GetMaterial(DefaultMaterialType.Decal);

	public override Shader defaultShader
	{
		get
		{
			if (m_DefaultShader == null)
			{
				m_DefaultShader = Shader.Find(ShaderUtils.GetShaderPath(ShaderPathID.Lit));
			}
			return m_DefaultShader;
		}
	}

	public override string[] renderingLayerMaskNames => UniversalRenderPipelineGlobalSettings.instance.renderingLayerMaskNames;

	public override string[] prefixedRenderingLayerMaskNames => UniversalRenderPipelineGlobalSettings.instance.prefixedRenderingLayerMaskNames;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string[] lightLayerMaskNames => new string[0];

	public TextureResources textures
	{
		get
		{
			if (m_Textures == null)
			{
				m_Textures = new TextureResources();
			}
			return m_Textures;
		}
	}

	[Obsolete("Use UniversalRenderPipelineGlobalSettings.instance.shaderVariantLogLevel", false)]
	public ShaderVariantLogLevel shaderVariantLogLevel
	{
		get
		{
			return (ShaderVariantLogLevel)UniversalRenderPipelineGlobalSettings.instance.shaderVariantLogLevel;
		}
		set
		{
			UniversalRenderPipelineGlobalSettings.instance.shaderVariantLogLevel = (UnityEngine.Rendering.ShaderVariantLogLevel)value;
		}
	}

	[Obsolete("This is obsolete, please use shadowCascadeCount instead.", false)]
	public ShadowCascadesOption shadowCascadeOption
	{
		get
		{
			return shadowCascadeCount switch
			{
				1 => ShadowCascadesOption.NoCascades, 
				2 => ShadowCascadesOption.TwoCascades, 
				4 => ShadowCascadesOption.FourCascades, 
				_ => throw new InvalidOperationException("Cascade count is not compatible with obsolete API, please use shadowCascadeCount instead."), 
			};
		}
		set
		{
			switch (value)
			{
			case ShadowCascadesOption.NoCascades:
				shadowCascadeCount = 1;
				break;
			case ShadowCascadesOption.TwoCascades:
				shadowCascadeCount = 2;
				break;
			case ShadowCascadesOption.FourCascades:
				shadowCascadeCount = 4;
				break;
			default:
				throw new InvalidOperationException("Cascade count is not compatible with obsolete API, please use shadowCascadeCount instead.");
			}
		}
	}

	public ScriptableRendererData LoadBuiltinRendererData(RendererType type = RendererType.UniversalRenderer)
	{
		m_RendererDataList[0] = null;
		return m_RendererDataList[0];
	}

	protected override RenderPipeline CreatePipeline()
	{
		if (m_RendererDataList == null)
		{
			m_RendererDataList = new ScriptableRendererData[1];
		}
		if (m_RendererDataList[m_DefaultRendererIndex] == null)
		{
			if (k_AssetPreviousVersion != k_AssetVersion)
			{
				return null;
			}
			if (m_RendererDataList[m_DefaultRendererIndex].GetType().ToString().Contains("Universal.ForwardRendererData"))
			{
				return null;
			}
			Debug.LogError("Default Renderer is missing, make sure there is a Renderer assigned as the default on the current Universal RP asset:" + UniversalRenderPipeline.asset.name, this);
			return null;
		}
		DestroyRenderers();
		UniversalRenderPipeline result = new UniversalRenderPipeline(this);
		CreateRenderers();
		ScriptableRendererData[] rendererDataList = m_RendererDataList;
		for (int i = 0; i < rendererDataList.Length; i++)
		{
			if (rendererDataList[i] is UniversalRendererData universalRendererData)
			{
				Blitter.Initialize(universalRendererData.shaders.coreBlitPS, universalRendererData.shaders.coreBlitColorAndDepthPS);
				break;
			}
		}
		return result;
	}

	internal void DestroyRenderers()
	{
		if (m_Renderers != null)
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				DestroyRenderer(ref m_Renderers[i]);
			}
		}
	}

	private void DestroyRenderer(ref ScriptableRenderer renderer)
	{
		if (renderer != null)
		{
			renderer.Dispose();
			renderer = null;
		}
	}

	protected override void OnDisable()
	{
		DestroyRenderers();
		base.OnDisable();
	}

	private void CreateRenderers()
	{
		if (m_Renderers != null)
		{
			for (int i = 0; i < m_Renderers.Length; i++)
			{
				if (m_Renderers[i] != null)
				{
					Debug.LogError($"Creating renderers but previous instance wasn't properly destroyed: m_Renderers[{i}]");
				}
			}
		}
		if (m_Renderers == null || m_Renderers.Length != m_RendererDataList.Length)
		{
			m_Renderers = new ScriptableRenderer[m_RendererDataList.Length];
		}
		for (int j = 0; j < m_RendererDataList.Length; j++)
		{
			if (m_RendererDataList[j] != null)
			{
				m_Renderers[j] = m_RendererDataList[j].InternalCreateRenderer();
			}
		}
	}

	private Material GetMaterial(DefaultMaterialType materialType)
	{
		return null;
	}

	public ScriptableRenderer GetRenderer(int index)
	{
		if (index == -1)
		{
			index = m_DefaultRendererIndex;
		}
		if (index >= m_RendererDataList.Length || index < 0 || m_RendererDataList[index] == null)
		{
			Debug.LogWarning("Renderer at index " + index + " is missing, falling back to Default Renderer " + m_RendererDataList[m_DefaultRendererIndex].name, this);
			index = m_DefaultRendererIndex;
		}
		if (m_Renderers == null || m_Renderers.Length < m_RendererDataList.Length)
		{
			DestroyRenderers();
			CreateRenderers();
		}
		if (m_RendererDataList[index].isInvalidated || m_Renderers[index] == null)
		{
			DestroyRenderer(ref m_Renderers[index]);
			m_Renderers[index] = m_RendererDataList[index].InternalCreateRenderer();
		}
		return m_Renderers[index];
	}

	internal int GetAdditionalLightsShadowResolution(int additionalLightsShadowResolutionTier)
	{
		if (additionalLightsShadowResolutionTier <= UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierLow)
		{
			return additionalLightsShadowResolutionTierLow;
		}
		if (additionalLightsShadowResolutionTier == UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierMedium)
		{
			return additionalLightsShadowResolutionTierMedium;
		}
		if (additionalLightsShadowResolutionTier >= UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierHigh)
		{
			return additionalLightsShadowResolutionTierHigh;
		}
		return additionalLightsShadowResolutionTierMedium;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (k_AssetVersion < 3)
		{
			m_SoftShadowsSupported = m_ShadowType == ShadowQuality.SoftShadows;
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 3;
		}
		if (k_AssetVersion < 4)
		{
			m_AdditionalLightShadowsSupported = m_LocalShadowsSupported;
			m_AdditionalLightsShadowmapResolution = m_LocalShadowsAtlasResolution;
			m_AdditionalLightsPerObjectLimit = m_MaxPixelLights;
			m_MainLightShadowmapResolution = m_ShadowAtlasResolution;
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 4;
		}
		if (k_AssetVersion < 5)
		{
			if (m_RendererType == RendererType.Custom)
			{
				m_RendererDataList[0] = m_RendererData;
			}
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 5;
		}
		if (k_AssetVersion < 6)
		{
			int shadowCascades = (int)m_ShadowCascades;
			if (shadowCascades == 2)
			{
				m_ShadowCascadeCount = 4;
			}
			else
			{
				m_ShadowCascadeCount = shadowCascades + 1;
			}
			k_AssetVersion = 6;
		}
		if (k_AssetVersion < 7)
		{
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 7;
		}
		if (k_AssetVersion < 8)
		{
			k_AssetPreviousVersion = k_AssetVersion;
			m_CascadeBorder = 0.1f;
			k_AssetVersion = 8;
		}
		if (k_AssetVersion < 9)
		{
			if (m_AdditionalLightsShadowResolutionTierHigh == AdditionalLightsDefaultShadowResolutionTierHigh && m_AdditionalLightsShadowResolutionTierMedium == AdditionalLightsDefaultShadowResolutionTierMedium && m_AdditionalLightsShadowResolutionTierLow == AdditionalLightsDefaultShadowResolutionTierLow)
			{
				m_AdditionalLightsShadowResolutionTierHigh = (int)m_AdditionalLightsShadowmapResolution;
				m_AdditionalLightsShadowResolutionTierMedium = Mathf.Max(m_AdditionalLightsShadowResolutionTierHigh / 2, UniversalAdditionalLightData.AdditionalLightsShadowMinimumResolution);
				m_AdditionalLightsShadowResolutionTierLow = Mathf.Max(m_AdditionalLightsShadowResolutionTierMedium / 2, UniversalAdditionalLightData.AdditionalLightsShadowMinimumResolution);
			}
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 9;
		}
		if (k_AssetVersion < 10)
		{
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 10;
		}
		if (k_AssetVersion < 11)
		{
			k_AssetPreviousVersion = k_AssetVersion;
			k_AssetVersion = 11;
		}
	}

	private float ValidateShadowBias(float value)
	{
		return Mathf.Max(0f, Mathf.Min(value, UniversalRenderPipeline.maxShadowBias));
	}

	private int ValidatePerObjectLights(int value)
	{
		return Math.Max(0, Math.Min(value, UniversalRenderPipeline.maxPerObjectLights));
	}

	private float ValidateRenderScale(float value)
	{
		return Mathf.Max(UniversalRenderPipeline.minRenderScale, Mathf.Min(value, UniversalRenderPipeline.maxRenderScale));
	}

	internal bool ValidateRendererDataList(bool partial = false)
	{
		int num = 0;
		for (int i = 0; i < m_RendererDataList.Length; i++)
		{
			num += ((!ValidateRendererData(i)) ? 1 : 0);
		}
		if (partial)
		{
			return num == 0;
		}
		return num != m_RendererDataList.Length;
	}

	internal bool ValidateRendererData(int index)
	{
		if (index == -1)
		{
			index = m_DefaultRendererIndex;
		}
		if (index >= m_RendererDataList.Length)
		{
			return false;
		}
		return m_RendererDataList[index] != null;
	}
}
