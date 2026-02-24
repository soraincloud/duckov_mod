using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[UsedByNativeCode]
public struct ShadowDrawingSettings : IEquatable<ShadowDrawingSettings>
{
	private CullingResults m_CullingResults;

	private int m_LightIndex;

	private int m_UseRenderingLayerMaskTest;

	private ShadowSplitData m_SplitData;

	private ShadowObjectsFilter m_ObjectsFilter;

	private BatchCullingProjectionType m_ProjectionType;

	public CullingResults cullingResults
	{
		get
		{
			return m_CullingResults;
		}
		set
		{
			m_CullingResults = value;
		}
	}

	public int lightIndex
	{
		get
		{
			return m_LightIndex;
		}
		set
		{
			m_LightIndex = value;
		}
	}

	public bool useRenderingLayerMaskTest
	{
		get
		{
			return m_UseRenderingLayerMaskTest != 0;
		}
		set
		{
			m_UseRenderingLayerMaskTest = (value ? 1 : 0);
		}
	}

	public ShadowSplitData splitData
	{
		get
		{
			return m_SplitData;
		}
		set
		{
			m_SplitData = value;
		}
	}

	public ShadowObjectsFilter objectsFilter
	{
		get
		{
			return m_ObjectsFilter;
		}
		set
		{
			m_ObjectsFilter = value;
		}
	}

	public BatchCullingProjectionType projectionType
	{
		get
		{
			return m_ProjectionType;
		}
		set
		{
			m_ProjectionType = value;
		}
	}

	[Obsolete("ShadowDrawingSettings(CullingResults, int) is deprecated. Use ShadowDrawingSettings(CullingResults, int, BatchCullingProjectionType) instead.")]
	public ShadowDrawingSettings(CullingResults cullingResults, int lightIndex)
		: this(cullingResults, lightIndex, BatchCullingProjectionType.Unknown)
	{
	}

	public ShadowDrawingSettings(CullingResults cullingResults, int lightIndex, BatchCullingProjectionType projectionType)
	{
		m_CullingResults = cullingResults;
		m_LightIndex = lightIndex;
		m_UseRenderingLayerMaskTest = 0;
		m_SplitData = default(ShadowSplitData);
		m_SplitData.shadowCascadeBlendCullingFactor = 1f;
		m_ObjectsFilter = ShadowObjectsFilter.AllObjects;
		m_ProjectionType = projectionType;
	}

	public bool Equals(ShadowDrawingSettings other)
	{
		return m_CullingResults.Equals(other.m_CullingResults) && m_LightIndex == other.m_LightIndex && m_SplitData.Equals(other.m_SplitData) && m_UseRenderingLayerMaskTest.Equals(other.m_UseRenderingLayerMaskTest) && m_ObjectsFilter.Equals(other.m_ObjectsFilter);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is ShadowDrawingSettings && Equals((ShadowDrawingSettings)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = m_CullingResults.GetHashCode();
		hashCode = (hashCode * 397) ^ m_LightIndex;
		hashCode = (hashCode * 397) ^ m_UseRenderingLayerMaskTest;
		hashCode = (hashCode * 397) ^ m_SplitData.GetHashCode();
		return (hashCode * 397) ^ (int)m_ObjectsFilter;
	}

	public static bool operator ==(ShadowDrawingSettings left, ShadowDrawingSettings right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ShadowDrawingSettings left, ShadowDrawingSettings right)
	{
		return !left.Equals(right);
	}
}
