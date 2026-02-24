using System;

namespace UnityEngine.Rendering.Universal;

internal class UniversalRenderPipelineGlobalSettings : RenderPipelineGlobalSettings, ISerializationCallbackReceiver, IShaderVariantSettings
{
	[SerializeField]
	private int k_AssetVersion = 3;

	private static UniversalRenderPipelineGlobalSettings cachedInstance = null;

	public static readonly string defaultAssetName = "UniversalRenderPipelineGlobalSettings";

	[SerializeField]
	private string[] m_RenderingLayerNames = new string[1] { "Default" };

	[NonSerialized]
	private string[] m_PrefixedRenderingLayerNames;

	[SerializeField]
	private uint m_ValidRenderingLayers;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName0;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName1;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName2;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName3;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName4;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName5;

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string lightLayerName6;

	[Obsolete("This is obsolete, please use renderingLayerNames instead.", false)]
	public string lightLayerName7;

	[SerializeField]
	private bool m_StripDebugVariants = true;

	[SerializeField]
	private bool m_StripUnusedPostProcessingVariants;

	[SerializeField]
	private bool m_StripUnusedVariants = true;

	[SerializeField]
	private bool m_StripUnusedLODCrossFadeVariants = true;

	[SerializeField]
	private bool m_StripScreenCoordOverrideVariants = true;

	[Obsolete("Please use stripRuntimeDebugShaders instead.", false)]
	public bool supportRuntimeDebugDisplay;

	[SerializeField]
	internal UnityEngine.Rendering.ShaderVariantLogLevel m_ShaderVariantLogLevel;

	[SerializeField]
	internal bool m_ExportShaderVariants = true;

	public static UniversalRenderPipelineGlobalSettings instance
	{
		get
		{
			if (cachedInstance == null)
			{
				cachedInstance = GraphicsSettings.GetSettingsForRenderPipeline<UniversalRenderPipeline>() as UniversalRenderPipelineGlobalSettings;
			}
			return cachedInstance;
		}
	}

	private string[] renderingLayerNames
	{
		get
		{
			if (m_RenderingLayerNames == null)
			{
				UpdateRenderingLayerNames();
			}
			return m_RenderingLayerNames;
		}
	}

	private string[] prefixedRenderingLayerNames
	{
		get
		{
			if (m_PrefixedRenderingLayerNames == null)
			{
				UpdateRenderingLayerNames();
			}
			return m_PrefixedRenderingLayerNames;
		}
	}

	public string[] renderingLayerMaskNames => renderingLayerNames;

	public string[] prefixedRenderingLayerMaskNames => prefixedRenderingLayerNames;

	public uint validRenderingLayers
	{
		get
		{
			if (m_PrefixedRenderingLayerNames == null)
			{
				UpdateRenderingLayerNames();
			}
			return m_ValidRenderingLayers;
		}
	}

	[Obsolete("This is obsolete, please use prefixedRenderingLayerMaskNames instead.", false)]
	public string[] prefixedLightLayerNames => new string[0];

	[Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
	public string[] lightLayerNames => new string[0];

	public bool stripDebugVariants
	{
		get
		{
			return m_StripDebugVariants;
		}
		set
		{
			m_StripDebugVariants = value;
		}
	}

	public bool stripUnusedPostProcessingVariants
	{
		get
		{
			return m_StripUnusedPostProcessingVariants;
		}
		set
		{
			m_StripUnusedPostProcessingVariants = value;
		}
	}

	public bool stripUnusedVariants
	{
		get
		{
			return m_StripUnusedVariants;
		}
		set
		{
			m_StripUnusedVariants = value;
		}
	}

	[Obsolete("No longer used as Shader Prefiltering automatically strips out unused LOD Crossfade variants. Please use the LOD Crossfade setting in the URP Asset to disable the feature if not used.", false)]
	public bool stripUnusedLODCrossFadeVariants
	{
		get
		{
			return m_StripUnusedLODCrossFadeVariants;
		}
		set
		{
			m_StripUnusedLODCrossFadeVariants = value;
		}
	}

	public bool stripScreenCoordOverrideVariants
	{
		get
		{
			return m_StripScreenCoordOverrideVariants;
		}
		set
		{
			m_StripScreenCoordOverrideVariants = value;
		}
	}

	public UnityEngine.Rendering.ShaderVariantLogLevel shaderVariantLogLevel
	{
		get
		{
			return m_ShaderVariantLogLevel;
		}
		set
		{
			m_ShaderVariantLogLevel = value;
		}
	}

	public bool exportShaderVariants
	{
		get
		{
			return m_ExportShaderVariants;
		}
		set
		{
			m_ExportShaderVariants = true;
		}
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}

	internal static void UpdateGraphicsSettings(UniversalRenderPipelineGlobalSettings newSettings)
	{
		if (!(newSettings == cachedInstance))
		{
			if (newSettings != null)
			{
				GraphicsSettings.RegisterRenderPipelineSettings<UniversalRenderPipeline>(newSettings);
			}
			else
			{
				GraphicsSettings.UnregisterRenderPipelineSettings<UniversalRenderPipeline>();
			}
			cachedInstance = newSettings;
		}
	}

	private void Reset()
	{
		UpdateRenderingLayerNames();
	}

	internal void UpdateRenderingLayerNames()
	{
		if (m_PrefixedRenderingLayerNames == null)
		{
			m_PrefixedRenderingLayerNames = new string[32];
		}
		for (int i = 0; i < m_PrefixedRenderingLayerNames.Length; i++)
		{
			uint num = (uint)(1 << i);
			m_ValidRenderingLayers = ((i < m_RenderingLayerNames.Length) ? (m_ValidRenderingLayers | num) : (m_ValidRenderingLayers & ~num));
			m_PrefixedRenderingLayerNames[i] = ((i < m_RenderingLayerNames.Length) ? m_RenderingLayerNames[i] : $"Unused Layer {i}");
		}
		DecalProjector.UpdateAllDecalProperties();
	}

	internal void ResetRenderingLayerNames()
	{
		m_RenderingLayerNames = new string[1] { "Default" };
	}
}
