using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.TextCore.LowLevel;

namespace UnityEngine.TextCore.Text;

[Serializable]
[ExcludeFromPreset]
[ExcludeFromObjectFactory]
public class TextSettings : ScriptableObject
{
	[Serializable]
	private struct FontReferenceMap
	{
		public Font font;

		public FontAsset fontAsset;

		public FontReferenceMap(Font font, FontAsset fontAsset)
		{
			this.font = font;
			this.fontAsset = fontAsset;
		}
	}

	[SerializeField]
	protected string m_Version;

	[FormerlySerializedAs("m_defaultFontAsset")]
	[SerializeField]
	protected FontAsset m_DefaultFontAsset;

	[FormerlySerializedAs("m_defaultFontAssetPath")]
	[SerializeField]
	protected string m_DefaultFontAssetPath = "Fonts & Materials/";

	[FormerlySerializedAs("m_fallbackFontAssets")]
	[SerializeField]
	protected List<FontAsset> m_FallbackFontAssets;

	[SerializeField]
	[FormerlySerializedAs("m_matchMaterialPreset")]
	protected bool m_MatchMaterialPreset;

	[FormerlySerializedAs("m_missingGlyphCharacter")]
	[SerializeField]
	protected int m_MissingCharacterUnicode;

	[SerializeField]
	protected bool m_ClearDynamicDataOnBuild = true;

	[FormerlySerializedAs("m_defaultSpriteAsset")]
	[SerializeField]
	protected SpriteAsset m_DefaultSpriteAsset;

	[FormerlySerializedAs("m_defaultSpriteAssetPath")]
	[SerializeField]
	protected string m_DefaultSpriteAssetPath = "Sprite Assets/";

	[SerializeField]
	protected List<SpriteAsset> m_FallbackSpriteAssets;

	[SerializeField]
	protected uint m_MissingSpriteCharacterUnicode;

	[FormerlySerializedAs("m_defaultStyleSheet")]
	[SerializeField]
	protected TextStyleSheet m_DefaultStyleSheet;

	[SerializeField]
	protected string m_StyleSheetsResourcePath = "Text Style Sheets/";

	[FormerlySerializedAs("m_defaultColorGradientPresetsPath")]
	[SerializeField]
	protected string m_DefaultColorGradientPresetsPath = "Text Color Gradients/";

	[SerializeField]
	protected UnicodeLineBreakingRules m_UnicodeLineBreakingRules;

	[SerializeField]
	private bool m_UseModernHangulLineBreakingRules;

	[FormerlySerializedAs("m_warningsDisabled")]
	[SerializeField]
	protected bool m_DisplayWarnings = false;

	internal Dictionary<int, FontAsset> m_FontLookup;

	private List<FontReferenceMap> m_FontReferences = new List<FontReferenceMap>();

	public string version
	{
		get
		{
			return m_Version;
		}
		internal set
		{
			m_Version = value;
		}
	}

	public FontAsset defaultFontAsset
	{
		get
		{
			return m_DefaultFontAsset;
		}
		set
		{
			m_DefaultFontAsset = value;
		}
	}

	public string defaultFontAssetPath
	{
		get
		{
			return m_DefaultFontAssetPath;
		}
		set
		{
			m_DefaultFontAssetPath = value;
		}
	}

	public List<FontAsset> fallbackFontAssets
	{
		get
		{
			return m_FallbackFontAssets;
		}
		set
		{
			m_FallbackFontAssets = value;
		}
	}

	public bool matchMaterialPreset
	{
		get
		{
			return m_MatchMaterialPreset;
		}
		set
		{
			m_MatchMaterialPreset = value;
		}
	}

	public int missingCharacterUnicode
	{
		get
		{
			return m_MissingCharacterUnicode;
		}
		set
		{
			m_MissingCharacterUnicode = value;
		}
	}

	public bool clearDynamicDataOnBuild
	{
		get
		{
			return m_ClearDynamicDataOnBuild;
		}
		set
		{
			m_ClearDynamicDataOnBuild = value;
		}
	}

	public SpriteAsset defaultSpriteAsset
	{
		get
		{
			return m_DefaultSpriteAsset;
		}
		set
		{
			m_DefaultSpriteAsset = value;
		}
	}

	public string defaultSpriteAssetPath
	{
		get
		{
			return m_DefaultSpriteAssetPath;
		}
		set
		{
			m_DefaultSpriteAssetPath = value;
		}
	}

	public List<SpriteAsset> fallbackSpriteAssets
	{
		get
		{
			return m_FallbackSpriteAssets;
		}
		set
		{
			m_FallbackSpriteAssets = value;
		}
	}

	public uint missingSpriteCharacterUnicode
	{
		get
		{
			return m_MissingSpriteCharacterUnicode;
		}
		set
		{
			m_MissingSpriteCharacterUnicode = value;
		}
	}

	public TextStyleSheet defaultStyleSheet
	{
		get
		{
			return m_DefaultStyleSheet;
		}
		set
		{
			m_DefaultStyleSheet = value;
		}
	}

	public string styleSheetsResourcePath
	{
		get
		{
			return m_StyleSheetsResourcePath;
		}
		set
		{
			m_StyleSheetsResourcePath = value;
		}
	}

	public string defaultColorGradientPresetsPath
	{
		get
		{
			return m_DefaultColorGradientPresetsPath;
		}
		set
		{
			m_DefaultColorGradientPresetsPath = value;
		}
	}

	public UnicodeLineBreakingRules lineBreakingRules
	{
		get
		{
			if (m_UnicodeLineBreakingRules == null)
			{
				m_UnicodeLineBreakingRules = new UnicodeLineBreakingRules();
				m_UnicodeLineBreakingRules.LoadLineBreakingRules();
			}
			return m_UnicodeLineBreakingRules;
		}
		set
		{
			m_UnicodeLineBreakingRules = value;
		}
	}

	public bool useModernHangulLineBreakingRules
	{
		get
		{
			return m_UseModernHangulLineBreakingRules;
		}
		set
		{
			m_UseModernHangulLineBreakingRules = value;
		}
	}

	public bool displayWarnings
	{
		get
		{
			return m_DisplayWarnings;
		}
		set
		{
			m_DisplayWarnings = value;
		}
	}

	private void OnEnable()
	{
		lineBreakingRules.LoadLineBreakingRules();
	}

	protected void InitializeFontReferenceLookup()
	{
		if (m_FontReferences == null)
		{
			m_FontReferences = new List<FontReferenceMap>();
		}
		for (int i = 0; i < m_FontReferences.Count; i++)
		{
			FontReferenceMap fontReferenceMap = m_FontReferences[i];
			if (fontReferenceMap.font == null || fontReferenceMap.fontAsset == null)
			{
				Debug.Log("Deleting invalid font reference.");
				m_FontReferences.RemoveAt(i);
				i--;
				continue;
			}
			int instanceID = fontReferenceMap.font.GetInstanceID();
			if (!m_FontLookup.ContainsKey(instanceID))
			{
				m_FontLookup.Add(instanceID, fontReferenceMap.fontAsset);
			}
		}
	}

	protected FontAsset GetCachedFontAssetInternal(Font font)
	{
		if (m_FontLookup == null)
		{
			m_FontLookup = new Dictionary<int, FontAsset>();
			InitializeFontReferenceLookup();
		}
		int instanceID = font.GetInstanceID();
		if (m_FontLookup.ContainsKey(instanceID))
		{
			return m_FontLookup[instanceID];
		}
		FontAsset fontAsset = ((!(font.name == "System Normal")) ? FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024) : FontAsset.CreateFontAsset("Lucida Grande", "Regular"));
		if (fontAsset != null)
		{
			fontAsset.hideFlags = HideFlags.DontSave;
			fontAsset.atlasTextures[0].hideFlags = HideFlags.DontSave;
			fontAsset.material.hideFlags = HideFlags.DontSave;
			fontAsset.isMultiAtlasTexturesEnabled = true;
			m_FontReferences.Add(new FontReferenceMap(font, fontAsset));
			m_FontLookup.Add(instanceID, fontAsset);
		}
		return fontAsset;
	}
}
