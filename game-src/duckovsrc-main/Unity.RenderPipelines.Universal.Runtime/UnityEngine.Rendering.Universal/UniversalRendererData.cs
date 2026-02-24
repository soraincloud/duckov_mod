using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[ReloadGroup]
[ExcludeFromPreset]
public class UniversalRendererData : ScriptableRendererData, ISerializationCallbackReceiver
{
	[Serializable]
	[ReloadGroup]
	public sealed class ShaderResources
	{
		[Reload("Shaders/Utils/Blit.shader", ReloadAttribute.Package.Root)]
		public Shader blitPS;

		[Reload("Shaders/Utils/CopyDepth.shader", ReloadAttribute.Package.Root)]
		public Shader copyDepthPS;

		[Obsolete("Obsolete, this feature will be supported by new 'ScreenSpaceShadows' renderer feature")]
		public Shader screenSpaceShadowPS;

		[Reload("Shaders/Utils/Sampling.shader", ReloadAttribute.Package.Root)]
		public Shader samplingPS;

		[Reload("Shaders/Utils/StencilDeferred.shader", ReloadAttribute.Package.Root)]
		public Shader stencilDeferredPS;

		[Reload("Shaders/Utils/FallbackError.shader", ReloadAttribute.Package.Root)]
		public Shader fallbackErrorPS;

		[Reload("Shaders/Utils/FallbackLoading.shader", ReloadAttribute.Package.Root)]
		public Shader fallbackLoadingPS;

		[Obsolete("Use fallbackErrorPS instead")]
		[Reload("Shaders/Utils/MaterialError.shader", ReloadAttribute.Package.Root)]
		public Shader materialErrorPS;

		[Reload("Shaders/Utils/CoreBlit.shader", ReloadAttribute.Package.Root)]
		[SerializeField]
		internal Shader coreBlitPS;

		[Reload("Shaders/Utils/CoreBlitColorAndDepth.shader", ReloadAttribute.Package.Root)]
		[SerializeField]
		internal Shader coreBlitColorAndDepthPS;

		[Reload("Shaders/Utils/BlitHDROverlay.shader", ReloadAttribute.Package.Root)]
		[SerializeField]
		internal Shader blitHDROverlay;

		[Reload("Shaders/CameraMotionVectors.shader", ReloadAttribute.Package.Root)]
		public Shader cameraMotionVector;

		[Reload("Shaders/ObjectMotionVectors.shader", ReloadAttribute.Package.Root)]
		public Shader objectMotionVector;

		[Reload("Shaders/PostProcessing/LensFlareDataDriven.shader", ReloadAttribute.Package.Root)]
		public Shader dataDrivenLensFlare;
	}

	public PostProcessData postProcessData;

	[Reload("Runtime/Data/XRSystemData.asset", ReloadAttribute.Package.Root)]
	public XRSystemData xrSystemData;

	public ShaderResources shaders;

	private const int k_LatestAssetVersion = 2;

	[SerializeField]
	private int m_AssetVersion;

	[SerializeField]
	private LayerMask m_OpaqueLayerMask = -1;

	[SerializeField]
	private LayerMask m_TransparentLayerMask = -1;

	[SerializeField]
	private StencilStateData m_DefaultStencilState = new StencilStateData
	{
		passOperation = StencilOp.Replace
	};

	[SerializeField]
	private bool m_ShadowTransparentReceive = true;

	[SerializeField]
	private RenderingMode m_RenderingMode;

	[SerializeField]
	private DepthPrimingMode m_DepthPrimingMode;

	[SerializeField]
	private CopyDepthMode m_CopyDepthMode = CopyDepthMode.AfterTransparents;

	[SerializeField]
	private bool m_AccurateGbufferNormals;

	[SerializeField]
	private IntermediateTextureMode m_IntermediateTextureMode = IntermediateTextureMode.Always;

	public LayerMask opaqueLayerMask
	{
		get
		{
			return m_OpaqueLayerMask;
		}
		set
		{
			SetDirty();
			m_OpaqueLayerMask = value;
		}
	}

	public LayerMask transparentLayerMask
	{
		get
		{
			return m_TransparentLayerMask;
		}
		set
		{
			SetDirty();
			m_TransparentLayerMask = value;
		}
	}

	public StencilStateData defaultStencilState
	{
		get
		{
			return m_DefaultStencilState;
		}
		set
		{
			SetDirty();
			m_DefaultStencilState = value;
		}
	}

	public bool shadowTransparentReceive
	{
		get
		{
			return m_ShadowTransparentReceive;
		}
		set
		{
			SetDirty();
			m_ShadowTransparentReceive = value;
		}
	}

	public RenderingMode renderingMode
	{
		get
		{
			return m_RenderingMode;
		}
		set
		{
			SetDirty();
			m_RenderingMode = value;
		}
	}

	public DepthPrimingMode depthPrimingMode
	{
		get
		{
			return m_DepthPrimingMode;
		}
		set
		{
			SetDirty();
			m_DepthPrimingMode = value;
		}
	}

	public CopyDepthMode copyDepthMode
	{
		get
		{
			return m_CopyDepthMode;
		}
		set
		{
			SetDirty();
			m_CopyDepthMode = value;
		}
	}

	public bool accurateGbufferNormals
	{
		get
		{
			return m_AccurateGbufferNormals;
		}
		set
		{
			SetDirty();
			m_AccurateGbufferNormals = value;
		}
	}

	public IntermediateTextureMode intermediateTextureMode
	{
		get
		{
			return m_IntermediateTextureMode;
		}
		set
		{
			SetDirty();
			m_IntermediateTextureMode = value;
		}
	}

	protected override ScriptableRenderer Create()
	{
		if (!Application.isPlaying)
		{
			ReloadAllNullProperties();
		}
		return new UniversalRenderer(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (shaders != null)
		{
			ReloadAllNullProperties();
		}
	}

	private void ReloadAllNullProperties()
	{
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		m_AssetVersion = 2;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (m_AssetVersion <= 1)
		{
			m_CopyDepthMode = CopyDepthMode.AfterOpaques;
		}
		m_AssetVersion = 2;
	}
}
