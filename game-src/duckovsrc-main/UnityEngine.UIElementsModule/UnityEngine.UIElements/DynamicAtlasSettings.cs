using System;

namespace UnityEngine.UIElements;

[Serializable]
public class DynamicAtlasSettings
{
	[HideInInspector]
	[SerializeField]
	private int m_MinAtlasSize;

	[SerializeField]
	[HideInInspector]
	private int m_MaxAtlasSize;

	[SerializeField]
	[HideInInspector]
	private int m_MaxSubTextureSize;

	[HideInInspector]
	[SerializeField]
	private DynamicAtlasFilters m_ActiveFilters;

	private DynamicAtlasCustomFilter m_CustomFilter;

	public int minAtlasSize
	{
		get
		{
			return m_MinAtlasSize;
		}
		set
		{
			m_MinAtlasSize = value;
		}
	}

	public int maxAtlasSize
	{
		get
		{
			return m_MaxAtlasSize;
		}
		set
		{
			m_MaxAtlasSize = value;
		}
	}

	public int maxSubTextureSize
	{
		get
		{
			return m_MaxSubTextureSize;
		}
		set
		{
			m_MaxSubTextureSize = value;
		}
	}

	public DynamicAtlasFilters activeFilters
	{
		get
		{
			return m_ActiveFilters;
		}
		set
		{
			m_ActiveFilters = value;
		}
	}

	public static DynamicAtlasFilters defaultFilters => DynamicAtlas.defaultFilters;

	public DynamicAtlasCustomFilter customFilter
	{
		get
		{
			return m_CustomFilter;
		}
		set
		{
			m_CustomFilter = value;
		}
	}

	public static DynamicAtlasSettings defaults => new DynamicAtlasSettings
	{
		minAtlasSize = 64,
		maxAtlasSize = 4096,
		maxSubTextureSize = 64,
		activeFilters = defaultFilters,
		customFilter = null
	};
}
