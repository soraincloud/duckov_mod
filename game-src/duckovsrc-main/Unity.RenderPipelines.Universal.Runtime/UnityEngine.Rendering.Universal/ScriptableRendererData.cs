using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

public abstract class ScriptableRendererData : ScriptableObject
{
	[Serializable]
	[ReloadGroup]
	public sealed class DebugShaderResources
	{
		[Reload("Shaders/Debug/DebugReplacement.shader", ReloadAttribute.Package.Root)]
		public Shader debugReplacementPS;

		[Reload("Shaders/Debug/HDRDebugView.shader", ReloadAttribute.Package.Root)]
		public Shader hdrDebugViewPS;
	}

	public DebugShaderResources debugShaders;

	[SerializeField]
	internal List<ScriptableRendererFeature> m_RendererFeatures = new List<ScriptableRendererFeature>(10);

	[SerializeField]
	internal List<long> m_RendererFeatureMap = new List<long>(10);

	[SerializeField]
	private bool m_UseNativeRenderPass;

	internal bool isInvalidated { get; set; }

	public List<ScriptableRendererFeature> rendererFeatures => m_RendererFeatures;

	public bool useNativeRenderPass
	{
		get
		{
			return m_UseNativeRenderPass;
		}
		set
		{
			SetDirty();
			m_UseNativeRenderPass = value;
		}
	}

	protected abstract ScriptableRenderer Create();

	public new void SetDirty()
	{
		isInvalidated = true;
	}

	internal ScriptableRenderer InternalCreateRenderer()
	{
		isInvalidated = false;
		return Create();
	}

	protected virtual void OnValidate()
	{
		SetDirty();
	}

	protected virtual void OnEnable()
	{
		SetDirty();
	}

	internal bool TryGetRendererFeature<T>(out T rendererFeature) where T : ScriptableRendererFeature
	{
		foreach (ScriptableRendererFeature rendererFeature2 in rendererFeatures)
		{
			if (rendererFeature2.GetType() == typeof(T))
			{
				rendererFeature = rendererFeature2 as T;
				return true;
			}
		}
		rendererFeature = null;
		return false;
	}
}
