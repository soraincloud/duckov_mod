using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal class DynamicAtlas : AtlasBase
{
	private class TextureInfo : LinkedPoolItem<TextureInfo>
	{
		public DynamicAtlasPage page;

		public int counter;

		public Allocator2D.Alloc2D alloc;

		public RectInt rect;

		public static readonly LinkedPool<TextureInfo> pool = new LinkedPool<TextureInfo>(Create, Reset, 1024);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TextureInfo Create()
		{
			return new TextureInfo();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Reset(TextureInfo info)
		{
			info.page = null;
			info.counter = 0;
			info.alloc = default(Allocator2D.Alloc2D);
			info.rect = default(RectInt);
		}
	}

	private Dictionary<Texture, TextureInfo> m_Database = new Dictionary<Texture, TextureInfo>();

	private DynamicAtlasPage m_PointPage;

	private DynamicAtlasPage m_BilinearPage;

	private ColorSpace m_ColorSpace;

	private List<IPanel> m_Panels = new List<IPanel>(1);

	private int m_MinAtlasSize = 64;

	private int m_MaxAtlasSize = 4096;

	private int m_MaxSubTextureSize = 64;

	private DynamicAtlasFilters m_ActiveFilters = defaultFilters;

	private DynamicAtlasCustomFilter m_CustomFilter;

	internal bool isInitialized => m_PointPage != null || m_BilinearPage != null;

	public int minAtlasSize
	{
		get
		{
			return m_MinAtlasSize;
		}
		set
		{
			if (m_MinAtlasSize != value)
			{
				m_MinAtlasSize = value;
				Reset();
			}
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
			if (m_MaxAtlasSize != value)
			{
				m_MaxAtlasSize = value;
				Reset();
			}
		}
	}

	public static DynamicAtlasFilters defaultFilters => DynamicAtlasFilters.Readability | DynamicAtlasFilters.Size | DynamicAtlasFilters.Format | DynamicAtlasFilters.ColorSpace | DynamicAtlasFilters.FilterMode;

	public DynamicAtlasFilters activeFilters
	{
		get
		{
			return m_ActiveFilters;
		}
		set
		{
			if (m_ActiveFilters != value)
			{
				m_ActiveFilters = value;
				Reset();
			}
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
			if (m_MaxSubTextureSize != value)
			{
				m_MaxSubTextureSize = value;
				Reset();
			}
		}
	}

	public DynamicAtlasCustomFilter customFilter
	{
		get
		{
			return m_CustomFilter;
		}
		set
		{
			if (m_CustomFilter != value)
			{
				m_CustomFilter = value;
				Reset();
			}
		}
	}

	protected override void OnAssignedToPanel(IPanel panel)
	{
		base.OnAssignedToPanel(panel);
		m_Panels.Add(panel);
		if (m_Panels.Count == 1)
		{
			m_ColorSpace = QualitySettings.activeColorSpace;
		}
	}

	protected override void OnRemovedFromPanel(IPanel panel)
	{
		m_Panels.Remove(panel);
		if (m_Panels.Count == 0 && isInitialized)
		{
			DestroyPages();
		}
		base.OnRemovedFromPanel(panel);
	}

	public override void Reset()
	{
		if (isInitialized)
		{
			DestroyPages();
			int i = 0;
			for (int count = m_Panels.Count; i < count; i++)
			{
				AtlasBase.RepaintTexturedElements(m_Panels[i]);
			}
		}
	}

	private void InitPages()
	{
		int value = Mathf.Max(m_MaxSubTextureSize, 1);
		value = Mathf.NextPowerOfTwo(value);
		int value2 = Mathf.Max(m_MaxAtlasSize, 1);
		value2 = Mathf.NextPowerOfTwo(value2);
		value2 = Mathf.Min(value2, SystemInfo.maxRenderTextureSize);
		int value3 = Mathf.Max(m_MinAtlasSize, 1);
		value3 = Mathf.NextPowerOfTwo(value3);
		value3 = Mathf.Min(value3, value2);
		Vector2Int minSize = new Vector2Int(value3, value3);
		Vector2Int maxSize = new Vector2Int(value2, value2);
		m_PointPage = new DynamicAtlasPage(RenderTextureFormat.ARGB32, FilterMode.Point, minSize, maxSize);
		m_BilinearPage = new DynamicAtlasPage(RenderTextureFormat.ARGB32, FilterMode.Bilinear, minSize, maxSize);
	}

	private void DestroyPages()
	{
		m_PointPage.Dispose();
		m_PointPage = null;
		m_BilinearPage.Dispose();
		m_BilinearPage = null;
		m_Database.Clear();
	}

	public override bool TryGetAtlas(VisualElement ve, Texture2D src, out TextureId atlas, out RectInt atlasRect)
	{
		if (m_Panels.Count == 0 || src == null)
		{
			atlas = TextureId.invalid;
			atlasRect = default(RectInt);
			return false;
		}
		if (!isInitialized)
		{
			InitPages();
		}
		if (m_Database.TryGetValue(src, out var value))
		{
			atlas = value.page.textureId;
			atlasRect = value.rect;
			value.counter++;
			return true;
		}
		if (IsTextureValid(src, FilterMode.Bilinear) && m_BilinearPage.TryAdd(src, out var alloc, out atlasRect))
		{
			value = TextureInfo.pool.Get();
			value.alloc = alloc;
			value.counter = 1;
			value.page = m_BilinearPage;
			value.rect = atlasRect;
			m_Database[src] = value;
			atlas = m_BilinearPage.textureId;
			return true;
		}
		if (IsTextureValid(src, FilterMode.Point) && m_PointPage.TryAdd(src, out alloc, out atlasRect))
		{
			value = TextureInfo.pool.Get();
			value.alloc = alloc;
			value.counter = 1;
			value.page = m_PointPage;
			value.rect = atlasRect;
			m_Database[src] = value;
			atlas = m_PointPage.textureId;
			return true;
		}
		atlas = TextureId.invalid;
		atlasRect = default(RectInt);
		return false;
	}

	public override void ReturnAtlas(VisualElement ve, Texture2D src, TextureId atlas)
	{
		if (m_Database.TryGetValue(src, out var value))
		{
			value.counter--;
			if (value.counter == 0)
			{
				value.page.Remove(value.alloc);
				m_Database.Remove(src);
				TextureInfo.pool.Return(value);
			}
		}
	}

	protected override void OnUpdateDynamicTextures(IPanel panel)
	{
		if (m_PointPage != null)
		{
			m_PointPage.Commit();
			SetDynamicTexture(m_PointPage.textureId, m_PointPage.atlas);
		}
		if (m_BilinearPage != null)
		{
			m_BilinearPage.Commit();
			SetDynamicTexture(m_BilinearPage.textureId, m_BilinearPage.atlas);
		}
	}

	internal static bool IsTextureFormatSupported(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
		case TextureFormat.ARGB4444:
		case TextureFormat.RGB24:
		case TextureFormat.RGBA32:
		case TextureFormat.ARGB32:
		case TextureFormat.RGB565:
		case TextureFormat.R16:
		case TextureFormat.DXT1:
		case TextureFormat.DXT5:
		case TextureFormat.RGBA4444:
		case TextureFormat.BGRA32:
		case TextureFormat.BC7:
		case TextureFormat.BC4:
		case TextureFormat.BC5:
		case TextureFormat.DXT1Crunched:
		case TextureFormat.DXT5Crunched:
		case TextureFormat.PVRTC_RGB2:
		case TextureFormat.PVRTC_RGBA2:
		case TextureFormat.PVRTC_RGB4:
		case TextureFormat.PVRTC_RGBA4:
		case TextureFormat.ETC_RGB4:
		case TextureFormat.EAC_R:
		case TextureFormat.EAC_R_SIGNED:
		case TextureFormat.EAC_RG:
		case TextureFormat.EAC_RG_SIGNED:
		case TextureFormat.ETC2_RGB:
		case TextureFormat.ETC2_RGBA1:
		case TextureFormat.ETC2_RGBA8:
		case TextureFormat.ASTC_4x4:
		case TextureFormat.ASTC_5x5:
		case TextureFormat.ASTC_6x6:
		case TextureFormat.ASTC_8x8:
		case TextureFormat.ASTC_10x10:
		case TextureFormat.ASTC_12x12:
		case TextureFormat.ASTC_RGBA_4x4:
		case TextureFormat.ASTC_RGBA_5x5:
		case TextureFormat.ASTC_RGBA_6x6:
		case TextureFormat.ASTC_RGBA_8x8:
		case TextureFormat.ASTC_RGBA_10x10:
		case TextureFormat.ASTC_RGBA_12x12:
		case TextureFormat.ETC_RGB4_3DS:
		case TextureFormat.ETC_RGBA8_3DS:
		case TextureFormat.RG16:
		case TextureFormat.R8:
		case TextureFormat.ETC_RGB4Crunched:
		case TextureFormat.ETC2_RGBA8Crunched:
			return true;
		case TextureFormat.RHalf:
		case TextureFormat.RGHalf:
		case TextureFormat.RGBAHalf:
		case TextureFormat.RFloat:
		case TextureFormat.RGFloat:
		case TextureFormat.RGBAFloat:
		case TextureFormat.YUY2:
		case TextureFormat.RGB9e5Float:
		case TextureFormat.BC6H:
		case TextureFormat.ASTC_HDR_4x4:
		case TextureFormat.ASTC_HDR_5x5:
		case TextureFormat.ASTC_HDR_6x6:
		case TextureFormat.ASTC_HDR_8x8:
		case TextureFormat.ASTC_HDR_10x10:
		case TextureFormat.ASTC_HDR_12x12:
		case TextureFormat.RG32:
		case TextureFormat.RGB48:
		case TextureFormat.RGBA64:
			return false;
		default:
			return false;
		}
	}

	public virtual bool IsTextureValid(Texture2D texture, FilterMode atlasFilterMode)
	{
		DynamicAtlasFilters filtersToApply = m_ActiveFilters;
		if (m_CustomFilter != null && !m_CustomFilter(texture, ref filtersToApply))
		{
			return false;
		}
		bool flag = (filtersToApply & DynamicAtlasFilters.Readability) != 0;
		bool flag2 = (filtersToApply & DynamicAtlasFilters.Size) != 0;
		bool flag3 = (filtersToApply & DynamicAtlasFilters.Format) != 0;
		bool flag4 = (filtersToApply & DynamicAtlasFilters.ColorSpace) != 0;
		bool flag5 = (filtersToApply & DynamicAtlasFilters.FilterMode) != 0;
		if (flag && texture.isReadable)
		{
			return false;
		}
		if (flag2 && (texture.width > maxSubTextureSize || texture.height > maxSubTextureSize))
		{
			return false;
		}
		if (flag3 && !IsTextureFormatSupported(texture.format))
		{
			return false;
		}
		if (flag4 && m_ColorSpace == ColorSpace.Linear && texture.activeTextureColorSpace != ColorSpace.Gamma)
		{
			return false;
		}
		if (flag5 && texture.filterMode != atlasFilterMode)
		{
			return false;
		}
		return true;
	}

	public void SetDirty(Texture2D tex)
	{
		if (!(tex == null) && m_Database.TryGetValue(tex, out var value))
		{
			value.page.Update(tex, value.rect);
		}
	}
}
