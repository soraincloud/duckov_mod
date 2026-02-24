using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace UnityEngine.TextCore.Text;

[HelpURL("https://docs.unity3d.com/2022.3/Documentation/Manual/UIE-sprite.html")]
[ExcludeFromPreset]
public class SpriteAsset : TextAsset
{
	internal Dictionary<int, int> m_NameLookup;

	internal Dictionary<uint, int> m_GlyphIndexLookup;

	[SerializeField]
	internal FaceInfo m_FaceInfo;

	[FormerlySerializedAs("spriteSheet")]
	[SerializeField]
	internal Texture m_SpriteAtlasTexture;

	[SerializeField]
	private List<SpriteCharacter> m_SpriteCharacterTable = new List<SpriteCharacter>();

	internal Dictionary<uint, SpriteCharacter> m_SpriteCharacterLookup;

	[SerializeField]
	private List<SpriteGlyph> m_SpriteGlyphTable = new List<SpriteGlyph>();

	internal Dictionary<uint, SpriteGlyph> m_SpriteGlyphLookup;

	[SerializeField]
	public List<SpriteAsset> fallbackSpriteAssets;

	internal bool m_IsSpriteAssetLookupTablesDirty = false;

	private static HashSet<int> k_searchedSpriteAssets;

	public FaceInfo faceInfo
	{
		get
		{
			return m_FaceInfo;
		}
		internal set
		{
			m_FaceInfo = value;
		}
	}

	public Texture spriteSheet
	{
		get
		{
			return m_SpriteAtlasTexture;
		}
		internal set
		{
			m_SpriteAtlasTexture = value;
		}
	}

	public List<SpriteCharacter> spriteCharacterTable
	{
		get
		{
			if (m_GlyphIndexLookup == null)
			{
				UpdateLookupTables();
			}
			return m_SpriteCharacterTable;
		}
		internal set
		{
			m_SpriteCharacterTable = value;
		}
	}

	public Dictionary<uint, SpriteCharacter> spriteCharacterLookupTable
	{
		get
		{
			if (m_SpriteCharacterLookup == null)
			{
				UpdateLookupTables();
			}
			return m_SpriteCharacterLookup;
		}
		internal set
		{
			m_SpriteCharacterLookup = value;
		}
	}

	public List<SpriteGlyph> spriteGlyphTable
	{
		get
		{
			return m_SpriteGlyphTable;
		}
		internal set
		{
			m_SpriteGlyphTable = value;
		}
	}

	private void Awake()
	{
	}

	public void UpdateLookupTables()
	{
		if (m_GlyphIndexLookup == null)
		{
			m_GlyphIndexLookup = new Dictionary<uint, int>();
		}
		else
		{
			m_GlyphIndexLookup.Clear();
		}
		if (m_SpriteGlyphLookup == null)
		{
			m_SpriteGlyphLookup = new Dictionary<uint, SpriteGlyph>();
		}
		else
		{
			m_SpriteGlyphLookup.Clear();
		}
		for (int i = 0; i < m_SpriteGlyphTable.Count; i++)
		{
			SpriteGlyph spriteGlyph = m_SpriteGlyphTable[i];
			uint index = spriteGlyph.index;
			if (!m_GlyphIndexLookup.ContainsKey(index))
			{
				m_GlyphIndexLookup.Add(index, i);
			}
			if (!m_SpriteGlyphLookup.ContainsKey(index))
			{
				m_SpriteGlyphLookup.Add(index, spriteGlyph);
			}
		}
		if (m_NameLookup == null)
		{
			m_NameLookup = new Dictionary<int, int>();
		}
		else
		{
			m_NameLookup.Clear();
		}
		if (m_SpriteCharacterLookup == null)
		{
			m_SpriteCharacterLookup = new Dictionary<uint, SpriteCharacter>();
		}
		else
		{
			m_SpriteCharacterLookup.Clear();
		}
		for (int j = 0; j < m_SpriteCharacterTable.Count; j++)
		{
			SpriteCharacter spriteCharacter = m_SpriteCharacterTable[j];
			if (spriteCharacter == null)
			{
				continue;
			}
			uint glyphIndex = spriteCharacter.glyphIndex;
			if (m_SpriteGlyphLookup.ContainsKey(glyphIndex))
			{
				spriteCharacter.glyph = m_SpriteGlyphLookup[glyphIndex];
				spriteCharacter.textAsset = this;
				int hashCodeCaseInSensitive = TextUtilities.GetHashCodeCaseInSensitive(m_SpriteCharacterTable[j].name);
				if (!m_NameLookup.ContainsKey(hashCodeCaseInSensitive))
				{
					m_NameLookup.Add(hashCodeCaseInSensitive, j);
				}
				uint unicode = m_SpriteCharacterTable[j].unicode;
				if (unicode != 65534 && !m_SpriteCharacterLookup.ContainsKey(unicode))
				{
					m_SpriteCharacterLookup.Add(unicode, spriteCharacter);
				}
			}
		}
		m_IsSpriteAssetLookupTablesDirty = false;
	}

	public int GetSpriteIndexFromHashcode(int hashCode)
	{
		if (m_NameLookup == null)
		{
			UpdateLookupTables();
		}
		if (m_NameLookup.TryGetValue(hashCode, out var value))
		{
			return value;
		}
		return -1;
	}

	public int GetSpriteIndexFromUnicode(uint unicode)
	{
		if (m_SpriteCharacterLookup == null)
		{
			UpdateLookupTables();
		}
		if (m_SpriteCharacterLookup.TryGetValue(unicode, out var value))
		{
			return (int)value.glyphIndex;
		}
		return -1;
	}

	public int GetSpriteIndexFromName(string name)
	{
		if (m_NameLookup == null)
		{
			UpdateLookupTables();
		}
		int hashCodeCaseInSensitive = TextUtilities.GetHashCodeCaseInSensitive(name);
		return GetSpriteIndexFromHashcode(hashCodeCaseInSensitive);
	}

	public static SpriteAsset SearchForSpriteByUnicode(SpriteAsset spriteAsset, uint unicode, bool includeFallbacks, out int spriteIndex)
	{
		if (spriteAsset == null)
		{
			spriteIndex = -1;
			return null;
		}
		spriteIndex = spriteAsset.GetSpriteIndexFromUnicode(unicode);
		if (spriteIndex != -1)
		{
			return spriteAsset;
		}
		if (k_searchedSpriteAssets == null)
		{
			k_searchedSpriteAssets = new HashSet<int>();
		}
		else
		{
			k_searchedSpriteAssets.Clear();
		}
		int item = spriteAsset.GetInstanceID();
		k_searchedSpriteAssets.Add(item);
		if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
		{
			return SearchForSpriteByUnicodeInternal(spriteAsset.fallbackSpriteAssets, unicode, includeFallbacks: true, out spriteIndex);
		}
		spriteIndex = -1;
		return null;
	}

	private static SpriteAsset SearchForSpriteByUnicodeInternal(List<SpriteAsset> spriteAssets, uint unicode, bool includeFallbacks, out int spriteIndex)
	{
		for (int i = 0; i < spriteAssets.Count; i++)
		{
			SpriteAsset spriteAsset = spriteAssets[i];
			if (spriteAsset == null)
			{
				continue;
			}
			int item = spriteAsset.GetInstanceID();
			if (k_searchedSpriteAssets.Add(item))
			{
				spriteAsset = SearchForSpriteByUnicodeInternal(spriteAsset, unicode, includeFallbacks, out spriteIndex);
				if (spriteAsset != null)
				{
					return spriteAsset;
				}
			}
		}
		spriteIndex = -1;
		return null;
	}

	private static SpriteAsset SearchForSpriteByUnicodeInternal(SpriteAsset spriteAsset, uint unicode, bool includeFallbacks, out int spriteIndex)
	{
		spriteIndex = spriteAsset.GetSpriteIndexFromUnicode(unicode);
		if (spriteIndex != -1)
		{
			return spriteAsset;
		}
		if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
		{
			return SearchForSpriteByUnicodeInternal(spriteAsset.fallbackSpriteAssets, unicode, includeFallbacks: true, out spriteIndex);
		}
		spriteIndex = -1;
		return null;
	}

	public static SpriteAsset SearchForSpriteByHashCode(SpriteAsset spriteAsset, int hashCode, bool includeFallbacks, out int spriteIndex, TextSettings textSettings = null)
	{
		if (spriteAsset == null)
		{
			spriteIndex = -1;
			return null;
		}
		spriteIndex = spriteAsset.GetSpriteIndexFromHashcode(hashCode);
		if (spriteIndex != -1)
		{
			return spriteAsset;
		}
		if (k_searchedSpriteAssets == null)
		{
			k_searchedSpriteAssets = new HashSet<int>();
		}
		else
		{
			k_searchedSpriteAssets.Clear();
		}
		int item = spriteAsset.instanceID;
		k_searchedSpriteAssets.Add(item);
		if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
		{
			SpriteAsset result = SearchForSpriteByHashCodeInternal(spriteAsset.fallbackSpriteAssets, hashCode, searchFallbacks: true, out spriteIndex);
			if (spriteIndex != -1)
			{
				return result;
			}
		}
		if (textSettings == null)
		{
			spriteIndex = -1;
			return null;
		}
		if (includeFallbacks && textSettings.defaultSpriteAsset != null)
		{
			SpriteAsset result = SearchForSpriteByHashCodeInternal(textSettings.defaultSpriteAsset, hashCode, searchFallbacks: true, out spriteIndex);
			if (spriteIndex != -1)
			{
				return result;
			}
		}
		k_searchedSpriteAssets.Clear();
		uint missingSpriteCharacterUnicode = textSettings.missingSpriteCharacterUnicode;
		spriteIndex = spriteAsset.GetSpriteIndexFromUnicode(missingSpriteCharacterUnicode);
		if (spriteIndex != -1)
		{
			return spriteAsset;
		}
		k_searchedSpriteAssets.Add(item);
		if (includeFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
		{
			SpriteAsset result = SearchForSpriteByUnicodeInternal(spriteAsset.fallbackSpriteAssets, missingSpriteCharacterUnicode, includeFallbacks: true, out spriteIndex);
			if (spriteIndex != -1)
			{
				return result;
			}
		}
		if (includeFallbacks && textSettings.defaultSpriteAsset != null)
		{
			SpriteAsset result = SearchForSpriteByUnicodeInternal(textSettings.defaultSpriteAsset, missingSpriteCharacterUnicode, includeFallbacks: true, out spriteIndex);
			if (spriteIndex != -1)
			{
				return result;
			}
		}
		spriteIndex = -1;
		return null;
	}

	private static SpriteAsset SearchForSpriteByHashCodeInternal(List<SpriteAsset> spriteAssets, int hashCode, bool searchFallbacks, out int spriteIndex)
	{
		for (int i = 0; i < spriteAssets.Count; i++)
		{
			SpriteAsset spriteAsset = spriteAssets[i];
			if (spriteAsset == null)
			{
				continue;
			}
			int item = spriteAsset.instanceID;
			if (k_searchedSpriteAssets.Add(item))
			{
				spriteAsset = SearchForSpriteByHashCodeInternal(spriteAsset, hashCode, searchFallbacks, out spriteIndex);
				if (spriteAsset != null)
				{
					return spriteAsset;
				}
			}
		}
		spriteIndex = -1;
		return null;
	}

	private static SpriteAsset SearchForSpriteByHashCodeInternal(SpriteAsset spriteAsset, int hashCode, bool searchFallbacks, out int spriteIndex)
	{
		spriteIndex = spriteAsset.GetSpriteIndexFromHashcode(hashCode);
		if (spriteIndex != -1)
		{
			return spriteAsset;
		}
		if (searchFallbacks && spriteAsset.fallbackSpriteAssets != null && spriteAsset.fallbackSpriteAssets.Count > 0)
		{
			return SearchForSpriteByHashCodeInternal(spriteAsset.fallbackSpriteAssets, hashCode, searchFallbacks: true, out spriteIndex);
		}
		spriteIndex = -1;
		return null;
	}

	public void SortGlyphTable()
	{
		if (m_SpriteGlyphTable != null && m_SpriteGlyphTable.Count != 0)
		{
			m_SpriteGlyphTable = m_SpriteGlyphTable.OrderBy((SpriteGlyph item) => item.index).ToList();
		}
	}

	internal void SortCharacterTable()
	{
		if (m_SpriteCharacterTable != null && m_SpriteCharacterTable.Count > 0)
		{
			m_SpriteCharacterTable = m_SpriteCharacterTable.OrderBy((SpriteCharacter c) => c.unicode).ToList();
		}
	}

	internal void SortGlyphAndCharacterTables()
	{
		SortGlyphTable();
		SortCharacterTable();
	}
}
