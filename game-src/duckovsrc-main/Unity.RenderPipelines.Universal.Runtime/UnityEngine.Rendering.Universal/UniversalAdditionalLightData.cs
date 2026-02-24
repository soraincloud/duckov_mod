using System;

namespace UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public class UniversalAdditionalLightData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData
{
	[SerializeField]
	private int m_Version = 3;

	[Tooltip("Controls if light Shadow Bias parameters use pipeline settings.")]
	[SerializeField]
	private bool m_UsePipelineSettings = true;

	public static readonly int AdditionalLightsShadowResolutionTierCustom = -1;

	public static readonly int AdditionalLightsShadowResolutionTierLow = 0;

	public static readonly int AdditionalLightsShadowResolutionTierMedium = 1;

	public static readonly int AdditionalLightsShadowResolutionTierHigh = 2;

	public static readonly int AdditionalLightsShadowDefaultResolutionTier = AdditionalLightsShadowResolutionTierHigh;

	public static readonly int AdditionalLightsShadowDefaultCustomResolution = 128;

	[NonSerialized]
	private Light m_Light;

	public static readonly int AdditionalLightsShadowMinimumResolution = 128;

	[Tooltip("Controls if light shadow resolution uses pipeline settings.")]
	[SerializeField]
	private int m_AdditionalLightsShadowResolutionTier = AdditionalLightsShadowDefaultResolutionTier;

	[Obsolete("This is obsolete, please use m_RenderingLayerMask instead.", false)]
	[SerializeField]
	private LightLayerEnum m_LightLayerMask = LightLayerEnum.LightLayerDefault;

	[SerializeField]
	private uint m_RenderingLayers = 1u;

	[SerializeField]
	private bool m_CustomShadowLayers;

	[SerializeField]
	private LightLayerEnum m_ShadowLayerMask = LightLayerEnum.LightLayerDefault;

	[SerializeField]
	private uint m_ShadowRenderingLayers = 1u;

	[SerializeField]
	private Vector2 m_LightCookieSize = Vector2.one;

	[SerializeField]
	private Vector2 m_LightCookieOffset = Vector2.zero;

	[SerializeField]
	private SoftShadowQuality m_SoftShadowQuality;

	internal int version => m_Version;

	public bool usePipelineSettings
	{
		get
		{
			return m_UsePipelineSettings;
		}
		set
		{
			m_UsePipelineSettings = value;
		}
	}

	internal Light light
	{
		get
		{
			if (!m_Light)
			{
				TryGetComponent<Light>(out m_Light);
			}
			return m_Light;
		}
	}

	public int additionalLightsShadowResolutionTier => m_AdditionalLightsShadowResolutionTier;

	[Obsolete("This is obsolete, please use renderingLayerMask instead.", false)]
	public LightLayerEnum lightLayerMask
	{
		get
		{
			return m_LightLayerMask;
		}
		set
		{
			m_LightLayerMask = value;
		}
	}

	public uint renderingLayers
	{
		get
		{
			return m_RenderingLayers;
		}
		set
		{
			if (m_RenderingLayers != value)
			{
				m_RenderingLayers = value;
				SyncLightAndShadowLayers();
			}
		}
	}

	public bool customShadowLayers
	{
		get
		{
			return m_CustomShadowLayers;
		}
		set
		{
			if (m_CustomShadowLayers != value)
			{
				m_CustomShadowLayers = value;
				SyncLightAndShadowLayers();
			}
		}
	}

	[Obsolete("This is obsolete, please use shadowRenderingLayerMask instead.", false)]
	public LightLayerEnum shadowLayerMask
	{
		get
		{
			return m_ShadowLayerMask;
		}
		set
		{
			m_ShadowLayerMask = value;
		}
	}

	public uint shadowRenderingLayers
	{
		get
		{
			return m_ShadowRenderingLayers;
		}
		set
		{
			if (value != m_ShadowRenderingLayers)
			{
				m_ShadowRenderingLayers = value;
				SyncLightAndShadowLayers();
			}
		}
	}

	[Tooltip("Controls the size of the cookie mask currently assigned to the light.")]
	public Vector2 lightCookieSize
	{
		get
		{
			return m_LightCookieSize;
		}
		set
		{
			m_LightCookieSize = value;
		}
	}

	[Tooltip("Controls the offset of the cookie mask currently assigned to the light.")]
	public Vector2 lightCookieOffset
	{
		get
		{
			return m_LightCookieOffset;
		}
		set
		{
			m_LightCookieOffset = value;
		}
	}

	[Tooltip("Controls the filtering quality of soft shadows. Higher quality has lower performance.")]
	public SoftShadowQuality softShadowQuality
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

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (m_Version < 2)
		{
			m_RenderingLayers = (uint)m_LightLayerMask;
			m_ShadowRenderingLayers = (uint)m_ShadowLayerMask;
			m_Version = 2;
		}
		if (m_Version < 3)
		{
			m_SoftShadowQuality = (SoftShadowQuality)Math.Clamp((int)(m_SoftShadowQuality + 1), 0, 3);
			m_Version = 3;
		}
	}

	private void SyncLightAndShadowLayers()
	{
		if ((bool)light)
		{
			light.renderingLayerMask = (int)(m_CustomShadowLayers ? m_ShadowRenderingLayers : m_RenderingLayers);
		}
	}
}
