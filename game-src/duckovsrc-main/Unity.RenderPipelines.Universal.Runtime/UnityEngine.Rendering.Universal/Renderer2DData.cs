using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[ReloadGroup]
[ExcludeFromPreset]
[MovedFrom("UnityEngine.Experimental.Rendering.Universal")]
public class Renderer2DData : ScriptableRendererData
{
	internal enum Renderer2DDefaultMaterialType
	{
		Lit,
		Unlit,
		Custom
	}

	[SerializeField]
	private TransparencySortMode m_TransparencySortMode;

	[SerializeField]
	private Vector3 m_TransparencySortAxis = Vector3.up;

	[SerializeField]
	private float m_HDREmulationScale = 1f;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float m_LightRenderTextureScale = 0.5f;

	[SerializeField]
	[FormerlySerializedAs("m_LightOperations")]
	private Light2DBlendStyle[] m_LightBlendStyles;

	[SerializeField]
	private bool m_UseDepthStencilBuffer = true;

	[SerializeField]
	private bool m_UseCameraSortingLayersTexture;

	[SerializeField]
	private int m_CameraSortingLayersTextureBound;

	[SerializeField]
	private Downsampling m_CameraSortingLayerDownsamplingMethod;

	[SerializeField]
	private uint m_MaxLightRenderTextureCount = 16u;

	[SerializeField]
	private uint m_MaxShadowRenderTextureCount = 1u;

	[SerializeField]
	[Reload("Shaders/2D/Light2D-Shape.shader", ReloadAttribute.Package.Root)]
	private Shader m_ShapeLightShader;

	[SerializeField]
	[Reload("Shaders/2D/Light2D-Shape-Volumetric.shader", ReloadAttribute.Package.Root)]
	private Shader m_ShapeLightVolumeShader;

	[SerializeField]
	[Reload("Shaders/2D/Light2D-Point.shader", ReloadAttribute.Package.Root)]
	private Shader m_PointLightShader;

	[SerializeField]
	[Reload("Shaders/2D/Light2D-Point-Volumetric.shader", ReloadAttribute.Package.Root)]
	private Shader m_PointLightVolumeShader;

	[SerializeField]
	[Reload("Shaders/Utils/CoreBlit.shader", ReloadAttribute.Package.Root)]
	private Shader m_CoreBlitShader;

	[SerializeField]
	[Reload("Shaders/Utils/BlitHDROverlay.shader", ReloadAttribute.Package.Root)]
	private Shader m_BlitHDROverlay;

	[SerializeField]
	[Reload("Shaders/Utils/CoreBlitColorAndDepth.shader", ReloadAttribute.Package.Root)]
	private Shader m_CoreBlitColorAndDepthPS;

	[SerializeField]
	[Reload("Shaders/Utils/Sampling.shader", ReloadAttribute.Package.Root)]
	private Shader m_SamplingShader;

	[SerializeField]
	[Reload("Shaders/2D/Shadow2D-Projected.shader", ReloadAttribute.Package.Root)]
	private Shader m_ProjectedShadowShader;

	[SerializeField]
	[Reload("Shaders/2D/Shadow2D-Shadow-Sprite.shader", ReloadAttribute.Package.Root)]
	private Shader m_SpriteShadowShader;

	[SerializeField]
	[Reload("Shaders/2D/Shadow2D-Unshadow-Sprite.shader", ReloadAttribute.Package.Root)]
	private Shader m_SpriteUnshadowShader;

	[SerializeField]
	[Reload("Shaders/2D/Shadow2D-Unshadow-Geometry.shader", ReloadAttribute.Package.Root)]
	private Shader m_GeometryUnshadowShader;

	[SerializeField]
	[Reload("Shaders/Utils/FallbackError.shader", ReloadAttribute.Package.Root)]
	private Shader m_FallbackErrorShader;

	[SerializeField]
	private PostProcessData m_PostProcessData;

	[SerializeField]
	[Reload("Runtime/2D/Data/Textures/FalloffLookupTexture.png", ReloadAttribute.Package.Root)]
	[HideInInspector]
	private Texture2D m_FallOffLookup;

	internal RTHandle normalsRenderTarget;

	internal int normalsRenderTargetId;

	internal RTHandle shadowsRenderTarget;

	internal int shadowsRenderTargetId;

	internal RTHandle cameraSortingLayerRenderTarget;

	internal int cameraSortingLayerRenderTargetId;

	public float hdrEmulationScale => m_HDREmulationScale;

	internal float lightRenderTextureScale => m_LightRenderTextureScale;

	public Light2DBlendStyle[] lightBlendStyles => m_LightBlendStyles;

	internal bool useDepthStencilBuffer => m_UseDepthStencilBuffer;

	internal Texture2D fallOffLookup => m_FallOffLookup;

	internal Shader shapeLightShader => m_ShapeLightShader;

	internal Shader shapeLightVolumeShader => m_ShapeLightVolumeShader;

	internal Shader pointLightShader => m_PointLightShader;

	internal Shader pointLightVolumeShader => m_PointLightVolumeShader;

	internal Shader blitShader => m_CoreBlitShader;

	internal Shader blitHDROverlay => m_BlitHDROverlay;

	internal Shader coreBlitPS => m_CoreBlitShader;

	internal Shader coreBlitColorAndDepthPS => m_CoreBlitColorAndDepthPS;

	internal Shader samplingShader => m_SamplingShader;

	internal PostProcessData postProcessData
	{
		get
		{
			return m_PostProcessData;
		}
		set
		{
			m_PostProcessData = value;
		}
	}

	internal Shader spriteShadowShader => m_SpriteShadowShader;

	internal Shader spriteUnshadowShader => m_SpriteUnshadowShader;

	internal Shader geometryUnshadowShader => m_GeometryUnshadowShader;

	internal Shader projectedShadowShader => m_ProjectedShadowShader;

	internal TransparencySortMode transparencySortMode => m_TransparencySortMode;

	internal Vector3 transparencySortAxis => m_TransparencySortAxis;

	internal uint lightRenderTextureMemoryBudget => m_MaxLightRenderTextureCount;

	internal uint shadowRenderTextureMemoryBudget => m_MaxShadowRenderTextureCount;

	internal bool useCameraSortingLayerTexture => m_UseCameraSortingLayersTexture;

	internal int cameraSortingLayerTextureBound => m_CameraSortingLayersTextureBound;

	internal Downsampling cameraSortingLayerDownsamplingMethod => m_CameraSortingLayerDownsamplingMethod;

	internal Dictionary<uint, Material> lightMaterials { get; } = new Dictionary<uint, Material>();

	internal Material[] spriteSelfShadowMaterial { get; set; }

	internal Material[] spriteUnshadowMaterial { get; set; }

	internal Material[] geometryUnshadowMaterial { get; set; }

	internal Material[] projectedShadowMaterial { get; set; }

	internal Material[] stencilOnlyShadowMaterial { get; set; }

	internal bool isNormalsRenderTargetValid { get; set; }

	internal float normalsRenderTargetScale { get; set; }

	internal ILight2DCullResult lightCullResult { get; set; }

	protected override ScriptableRenderer Create()
	{
		return new Renderer2D(this);
	}

	internal void Dispose()
	{
		for (int i = 0; i < m_LightBlendStyles.Length; i++)
		{
			m_LightBlendStyles[i].renderTargetHandle?.Release();
		}
		foreach (KeyValuePair<uint, Material> lightMaterial in lightMaterials)
		{
			CoreUtils.Destroy(lightMaterial.Value);
		}
		lightMaterials.Clear();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < m_LightBlendStyles.Length; i++)
		{
			m_LightBlendStyles[i].renderTargetHandleId = Shader.PropertyToID($"_ShapeLightTexture{i}");
			m_LightBlendStyles[i].renderTargetHandle = RTHandles.Alloc(m_LightBlendStyles[i].renderTargetHandleId, $"_ShapeLightTexture{i}");
		}
		normalsRenderTargetId = Shader.PropertyToID("_NormalMap");
		normalsRenderTarget = RTHandles.Alloc(normalsRenderTargetId, "_NormalMap");
		shadowsRenderTargetId = Shader.PropertyToID("_ShadowTex");
		shadowsRenderTarget = RTHandles.Alloc(shadowsRenderTargetId, "_ShadowTex");
		cameraSortingLayerRenderTargetId = Shader.PropertyToID("_CameraSortingLayerTexture");
		cameraSortingLayerRenderTarget = RTHandles.Alloc(cameraSortingLayerRenderTargetId, "_CameraSortingLayerTexture");
		spriteSelfShadowMaterial = null;
		spriteUnshadowMaterial = null;
		projectedShadowMaterial = null;
		stencilOnlyShadowMaterial = null;
	}
}
