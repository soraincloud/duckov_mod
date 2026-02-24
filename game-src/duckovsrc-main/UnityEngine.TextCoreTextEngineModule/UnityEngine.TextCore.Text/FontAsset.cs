using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine.Serialization;
using UnityEngine.TextCore.LowLevel;

namespace UnityEngine.TextCore.Text;

[Serializable]
[ExcludeFromPreset]
public class FontAsset : TextAsset
{
	[SerializeField]
	internal string m_SourceFontFileGUID;

	[SerializeField]
	internal FontAssetCreationEditorSettings m_fontAssetCreationEditorSettings;

	[SerializeField]
	private Font m_SourceFontFile;

	[SerializeField]
	private string m_SourceFontFilePath;

	[SerializeField]
	private AtlasPopulationMode m_AtlasPopulationMode;

	[SerializeField]
	internal bool InternalDynamicOS;

	[SerializeField]
	internal FaceInfo m_FaceInfo;

	private int m_FamilyNameHashCode;

	private int m_StyleNameHashCode;

	[SerializeField]
	internal List<Glyph> m_GlyphTable = new List<Glyph>();

	internal Dictionary<uint, Glyph> m_GlyphLookupDictionary;

	[SerializeField]
	internal List<Character> m_CharacterTable = new List<Character>();

	internal Dictionary<uint, Character> m_CharacterLookupDictionary;

	internal Texture2D m_AtlasTexture;

	[SerializeField]
	internal Texture2D[] m_AtlasTextures;

	[SerializeField]
	internal int m_AtlasTextureIndex;

	[SerializeField]
	private bool m_IsMultiAtlasTexturesEnabled;

	[SerializeField]
	private bool m_ClearDynamicDataOnBuild;

	[SerializeField]
	internal int m_AtlasWidth;

	[SerializeField]
	internal int m_AtlasHeight;

	[SerializeField]
	internal int m_AtlasPadding;

	[SerializeField]
	internal GlyphRenderMode m_AtlasRenderMode;

	[SerializeField]
	private List<GlyphRect> m_UsedGlyphRects;

	[SerializeField]
	private List<GlyphRect> m_FreeGlyphRects;

	[SerializeField]
	internal FontFeatureTable m_FontFeatureTable = new FontFeatureTable();

	[SerializeField]
	internal List<FontAsset> m_FallbackFontAssetTable;

	[SerializeField]
	private FontWeightPair[] m_FontWeightTable = new FontWeightPair[10];

	[SerializeField]
	[FormerlySerializedAs("normalStyle")]
	internal float m_RegularStyleWeight = 0f;

	[SerializeField]
	[FormerlySerializedAs("normalSpacingOffset")]
	internal float m_RegularStyleSpacing = 0f;

	[FormerlySerializedAs("boldStyle")]
	[SerializeField]
	internal float m_BoldStyleWeight = 0.75f;

	[FormerlySerializedAs("boldSpacing")]
	[SerializeField]
	internal float m_BoldStyleSpacing = 7f;

	[FormerlySerializedAs("italicStyle")]
	[SerializeField]
	internal byte m_ItalicStyleSlant = 35;

	[FormerlySerializedAs("tabSize")]
	[SerializeField]
	internal byte m_TabMultiple = 10;

	internal bool IsFontAssetLookupTablesDirty;

	private static ProfilerMarker k_ReadFontAssetDefinitionMarker = new ProfilerMarker("FontAsset.ReadFontAssetDefinition");

	private static ProfilerMarker k_AddSynthesizedCharactersMarker = new ProfilerMarker("FontAsset.AddSynthesizedCharacters");

	private static ProfilerMarker k_TryAddCharacterMarker = new ProfilerMarker("FontAsset.TryAddCharacter");

	private static ProfilerMarker k_TryAddCharactersMarker = new ProfilerMarker("FontAsset.TryAddCharacters");

	private static ProfilerMarker k_UpdateGlyphAdjustmentRecordsMarker = new ProfilerMarker("FontAsset.UpdateGlyphAdjustmentRecords");

	private static ProfilerMarker k_UpdateDiacriticalMarkAdjustmentRecordsMarker = new ProfilerMarker("FontAsset.UpdateDiacriticalAdjustmentRecords");

	private static ProfilerMarker k_ClearFontAssetDataMarker = new ProfilerMarker("FontAsset.ClearFontAssetData");

	private static ProfilerMarker k_UpdateFontAssetDataMarker = new ProfilerMarker("FontAsset.UpdateFontAssetData");

	private static ProfilerMarker k_TryAddGlyphMarker = new ProfilerMarker("FontAsset.TryAddGlyphMarker");

	private static string s_DefaultMaterialSuffix = " Atlas Material";

	private static HashSet<int> k_SearchedFontAssetLookup;

	private static List<FontAsset> k_FontAssets_FontFeaturesUpdateQueue = new List<FontAsset>();

	private static HashSet<int> k_FontAssets_FontFeaturesUpdateQueueLookup = new HashSet<int>();

	private static List<Texture2D> k_FontAssets_AtlasTexturesUpdateQueue = new List<Texture2D>();

	private static HashSet<int> k_FontAssets_AtlasTexturesUpdateQueueLookup = new HashSet<int>();

	private List<Glyph> m_GlyphsToRender = new List<Glyph>();

	private List<Glyph> m_GlyphsRendered = new List<Glyph>();

	private List<uint> m_GlyphIndexList = new List<uint>();

	private List<uint> m_GlyphIndexListNewlyAdded = new List<uint>();

	internal List<uint> m_GlyphsToAdd = new List<uint>();

	internal HashSet<uint> m_GlyphsToAddLookup = new HashSet<uint>();

	internal List<Character> m_CharactersToAdd = new List<Character>();

	internal HashSet<uint> m_CharactersToAddLookup = new HashSet<uint>();

	internal List<uint> s_MissingCharacterList = new List<uint>();

	internal HashSet<uint> m_MissingUnicodesFromFontFile = new HashSet<uint>();

	internal static uint[] k_GlyphIndexArray;

	public FontAssetCreationEditorSettings fontAssetCreationEditorSettings
	{
		get
		{
			return m_fontAssetCreationEditorSettings;
		}
		set
		{
			m_fontAssetCreationEditorSettings = value;
		}
	}

	public Font sourceFontFile
	{
		get
		{
			return m_SourceFontFile;
		}
		internal set
		{
			m_SourceFontFile = value;
		}
	}

	public AtlasPopulationMode atlasPopulationMode
	{
		get
		{
			return m_AtlasPopulationMode;
		}
		set
		{
			m_AtlasPopulationMode = value;
		}
	}

	public FaceInfo faceInfo
	{
		get
		{
			return m_FaceInfo;
		}
		set
		{
			m_FaceInfo = value;
		}
	}

	internal int familyNameHashCode
	{
		get
		{
			if (m_FamilyNameHashCode == 0)
			{
				m_FamilyNameHashCode = TextUtilities.GetHashCodeCaseInSensitive(m_FaceInfo.familyName);
			}
			return m_FamilyNameHashCode;
		}
		set
		{
			m_FamilyNameHashCode = value;
		}
	}

	internal int styleNameHashCode
	{
		get
		{
			if (m_StyleNameHashCode == 0)
			{
				m_StyleNameHashCode = TextUtilities.GetHashCodeCaseInSensitive(m_FaceInfo.styleName);
			}
			return m_StyleNameHashCode;
		}
		set
		{
			m_StyleNameHashCode = value;
		}
	}

	public List<Glyph> glyphTable
	{
		get
		{
			return m_GlyphTable;
		}
		internal set
		{
			m_GlyphTable = value;
		}
	}

	public Dictionary<uint, Glyph> glyphLookupTable
	{
		get
		{
			if (m_GlyphLookupDictionary == null)
			{
				ReadFontAssetDefinition();
			}
			return m_GlyphLookupDictionary;
		}
	}

	public List<Character> characterTable
	{
		get
		{
			return m_CharacterTable;
		}
		internal set
		{
			m_CharacterTable = value;
		}
	}

	public Dictionary<uint, Character> characterLookupTable
	{
		get
		{
			if (m_CharacterLookupDictionary == null)
			{
				ReadFontAssetDefinition();
			}
			return m_CharacterLookupDictionary;
		}
	}

	public Texture2D atlasTexture
	{
		get
		{
			if (m_AtlasTexture == null)
			{
				m_AtlasTexture = atlasTextures[0];
			}
			return m_AtlasTexture;
		}
	}

	public Texture2D[] atlasTextures
	{
		get
		{
			if (m_AtlasTextures == null)
			{
			}
			return m_AtlasTextures;
		}
		set
		{
			m_AtlasTextures = value;
		}
	}

	public int atlasTextureCount => m_AtlasTextureIndex + 1;

	public bool isMultiAtlasTexturesEnabled
	{
		get
		{
			return m_IsMultiAtlasTexturesEnabled;
		}
		set
		{
			m_IsMultiAtlasTexturesEnabled = value;
		}
	}

	internal bool clearDynamicDataOnBuild
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

	public int atlasWidth
	{
		get
		{
			return m_AtlasWidth;
		}
		internal set
		{
			m_AtlasWidth = value;
		}
	}

	public int atlasHeight
	{
		get
		{
			return m_AtlasHeight;
		}
		internal set
		{
			m_AtlasHeight = value;
		}
	}

	public int atlasPadding
	{
		get
		{
			return m_AtlasPadding;
		}
		internal set
		{
			m_AtlasPadding = value;
		}
	}

	public GlyphRenderMode atlasRenderMode
	{
		get
		{
			return m_AtlasRenderMode;
		}
		internal set
		{
			m_AtlasRenderMode = value;
		}
	}

	internal List<GlyphRect> usedGlyphRects
	{
		get
		{
			return m_UsedGlyphRects;
		}
		set
		{
			m_UsedGlyphRects = value;
		}
	}

	internal List<GlyphRect> freeGlyphRects
	{
		get
		{
			return m_FreeGlyphRects;
		}
		set
		{
			m_FreeGlyphRects = value;
		}
	}

	public FontFeatureTable fontFeatureTable
	{
		get
		{
			return m_FontFeatureTable;
		}
		internal set
		{
			m_FontFeatureTable = value;
		}
	}

	public List<FontAsset> fallbackFontAssetTable
	{
		get
		{
			return m_FallbackFontAssetTable;
		}
		set
		{
			m_FallbackFontAssetTable = value;
		}
	}

	public FontWeightPair[] fontWeightTable
	{
		get
		{
			return m_FontWeightTable;
		}
		internal set
		{
			m_FontWeightTable = value;
		}
	}

	public float regularStyleWeight
	{
		get
		{
			return m_RegularStyleWeight;
		}
		set
		{
			m_RegularStyleWeight = value;
		}
	}

	public float regularStyleSpacing
	{
		get
		{
			return m_RegularStyleSpacing;
		}
		set
		{
			m_RegularStyleSpacing = value;
		}
	}

	public float boldStyleWeight
	{
		get
		{
			return m_BoldStyleWeight;
		}
		set
		{
			m_BoldStyleWeight = value;
		}
	}

	public float boldStyleSpacing
	{
		get
		{
			return m_BoldStyleSpacing;
		}
		set
		{
			m_BoldStyleSpacing = value;
		}
	}

	public byte italicStyleSlant
	{
		get
		{
			return m_ItalicStyleSlant;
		}
		set
		{
			m_ItalicStyleSlant = value;
		}
	}

	public byte tabMultiple
	{
		get
		{
			return m_TabMultiple;
		}
		set
		{
			m_TabMultiple = value;
		}
	}

	public static FontAsset CreateFontAsset(string familyName, string styleName, int pointSize = 90)
	{
		if (FontEngine.TryGetSystemFontReference(familyName, styleName, out var fontRef))
		{
			return CreateFontAsset(fontRef.filePath, fontRef.faceIndex, pointSize, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.DynamicOS, enableMultiAtlasSupport: true);
		}
		Debug.Log("Unable to find a font file with the specified Family Name [" + familyName + "] and Style [" + styleName + "].");
		return null;
	}

	public static FontAsset CreateFontAsset(string fontFilePath, int faceIndex, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight)
	{
		return CreateFontAsset(fontFilePath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight, AtlasPopulationMode.Dynamic);
	}

	private static FontAsset CreateFontAsset(string fontFilePath, int faceIndex, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.DynamicOS, bool enableMultiAtlasSupport = true)
	{
		if (FontEngine.LoadFontFace(fontFilePath, samplingPointSize, faceIndex) != FontEngineError.Success)
		{
			Debug.Log("Unable to load font face from [" + fontFilePath + "].");
			return null;
		}
		FontAsset fontAsset = CreateFontAssetInstance(null, atlasPadding, renderMode, atlasWidth, atlasHeight, atlasPopulationMode, enableMultiAtlasSupport);
		fontAsset.m_SourceFontFilePath = fontFilePath;
		return fontAsset;
	}

	public static FontAsset CreateFontAsset(Font font)
	{
		return CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
	}

	public static FontAsset CreateFontAsset(Font font, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.Dynamic, bool enableMultiAtlasSupport = true)
	{
		return CreateFontAsset(font, 0, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight, atlasPopulationMode, enableMultiAtlasSupport);
	}

	private static FontAsset CreateFontAsset(Font font, int faceIndex, int samplingPointSize, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode = AtlasPopulationMode.Dynamic, bool enableMultiAtlasSupport = true)
	{
		if (FontEngine.LoadFontFace(font, samplingPointSize, faceIndex) != FontEngineError.Success)
		{
			if (font.name == "LegacyRuntime")
			{
				return CreateFontAsset("Arial", "Regular");
			}
			Debug.LogWarning("Unable to load font face for [" + font.name + "]. Make sure \"Include Font Data\" is enabled in the Font Import Settings.", font);
			return null;
		}
		return CreateFontAssetInstance(font, atlasPadding, renderMode, atlasWidth, atlasHeight, atlasPopulationMode, enableMultiAtlasSupport);
	}

	private static FontAsset CreateFontAssetInstance(Font font, int atlasPadding, GlyphRenderMode renderMode, int atlasWidth, int atlasHeight, AtlasPopulationMode atlasPopulationMode, bool enableMultiAtlasSupport)
	{
		FontAsset fontAsset = ScriptableObject.CreateInstance<FontAsset>();
		fontAsset.m_Version = "1.1.0";
		fontAsset.faceInfo = FontEngine.GetFaceInfo();
		if (atlasPopulationMode == AtlasPopulationMode.Dynamic && font != null)
		{
			fontAsset.sourceFontFile = font;
		}
		fontAsset.atlasPopulationMode = atlasPopulationMode;
		fontAsset.atlasWidth = atlasWidth;
		fontAsset.atlasHeight = atlasHeight;
		fontAsset.atlasPadding = atlasPadding;
		fontAsset.atlasRenderMode = renderMode;
		fontAsset.atlasTextures = new Texture2D[1];
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.Alpha8, mipChain: false);
		fontAsset.atlasTextures[0] = texture2D;
		fontAsset.isMultiAtlasTexturesEnabled = enableMultiAtlasSupport;
		int num;
		if ((renderMode & (GlyphRenderMode)16) == (GlyphRenderMode)16)
		{
			num = 0;
			Material material = new Material(TextShaderUtilities.ShaderRef_MobileBitmap);
			material.SetTexture(TextShaderUtilities.ID_MainTex, texture2D);
			material.SetFloat(TextShaderUtilities.ID_TextureWidth, atlasWidth);
			material.SetFloat(TextShaderUtilities.ID_TextureHeight, atlasHeight);
			fontAsset.material = material;
		}
		else
		{
			num = 1;
			Material material2 = new Material(TextShaderUtilities.ShaderRef_MobileSDF);
			material2.SetTexture(TextShaderUtilities.ID_MainTex, texture2D);
			material2.SetFloat(TextShaderUtilities.ID_TextureWidth, atlasWidth);
			material2.SetFloat(TextShaderUtilities.ID_TextureHeight, atlasHeight);
			material2.SetFloat(TextShaderUtilities.ID_GradientScale, atlasPadding + num);
			material2.SetFloat(TextShaderUtilities.ID_WeightNormal, fontAsset.regularStyleWeight);
			material2.SetFloat(TextShaderUtilities.ID_WeightBold, fontAsset.boldStyleWeight);
			fontAsset.material = material2;
		}
		fontAsset.freeGlyphRects = new List<GlyphRect>(8)
		{
			new GlyphRect(0, 0, atlasWidth - num, atlasHeight - num)
		};
		fontAsset.usedGlyphRects = new List<GlyphRect>(8);
		fontAsset.ReadFontAssetDefinition();
		return fontAsset;
	}

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		DestroyAtlasTextures();
		Object.DestroyImmediate(m_Material);
	}

	public void ReadFontAssetDefinition()
	{
		InitializeDictionaryLookupTables();
		AddSynthesizedCharactersAndFaceMetrics();
		if (m_FaceInfo.capLine == 0f && m_CharacterLookupDictionary.ContainsKey(88u))
		{
			uint glyphIndex = m_CharacterLookupDictionary[88u].glyphIndex;
			m_FaceInfo.capLine = m_GlyphLookupDictionary[glyphIndex].metrics.horizontalBearingY;
		}
		if (m_FaceInfo.meanLine == 0f && m_CharacterLookupDictionary.ContainsKey(120u))
		{
			uint glyphIndex2 = m_CharacterLookupDictionary[120u].glyphIndex;
			m_FaceInfo.meanLine = m_GlyphLookupDictionary[glyphIndex2].metrics.horizontalBearingY;
		}
		if (m_FaceInfo.scale == 0f)
		{
			m_FaceInfo.scale = 1f;
		}
		if (m_FaceInfo.strikethroughOffset == 0f)
		{
			m_FaceInfo.strikethroughOffset = m_FaceInfo.capLine / 2.5f;
		}
		if (m_AtlasPadding == 0 && base.material.HasProperty(TextShaderUtilities.ID_GradientScale))
		{
			m_AtlasPadding = (int)base.material.GetFloat(TextShaderUtilities.ID_GradientScale) - 1;
		}
		base.hashCode = TextUtilities.GetHashCodeCaseInSensitive(base.name);
		familyNameHashCode = TextUtilities.GetHashCodeCaseInSensitive(m_FaceInfo.familyName);
		styleNameHashCode = TextUtilities.GetHashCodeCaseInSensitive(m_FaceInfo.styleName);
		base.materialHashCode = TextUtilities.GetHashCodeCaseInSensitive(base.name + s_DefaultMaterialSuffix);
		TextResourceManager.AddFontAsset(this);
		IsFontAssetLookupTablesDirty = false;
	}

	internal void InitializeDictionaryLookupTables()
	{
		InitializeGlyphLookupDictionary();
		InitializeCharacterLookupDictionary();
		InitializeLigatureSubstitutionLookupDictionary();
		InitializeGlyphPaidAdjustmentRecordsLookupDictionary();
		InitializeMarkToBaseAdjustmentRecordsLookupDictionary();
		InitializeMarkToMarkAdjustmentRecordsLookupDictionary();
	}

	internal void InitializeGlyphLookupDictionary()
	{
		if (m_GlyphLookupDictionary == null)
		{
			m_GlyphLookupDictionary = new Dictionary<uint, Glyph>();
		}
		else
		{
			m_GlyphLookupDictionary.Clear();
		}
		if (m_GlyphIndexList == null)
		{
			m_GlyphIndexList = new List<uint>();
		}
		else
		{
			m_GlyphIndexList.Clear();
		}
		if (m_GlyphIndexListNewlyAdded == null)
		{
			m_GlyphIndexListNewlyAdded = new List<uint>();
		}
		else
		{
			m_GlyphIndexListNewlyAdded.Clear();
		}
		int count = m_GlyphTable.Count;
		for (int i = 0; i < count; i++)
		{
			Glyph glyph = m_GlyphTable[i];
			uint index = glyph.index;
			if (!m_GlyphLookupDictionary.ContainsKey(index))
			{
				m_GlyphLookupDictionary.Add(index, glyph);
				m_GlyphIndexList.Add(index);
			}
		}
	}

	internal void InitializeCharacterLookupDictionary()
	{
		if (m_CharacterLookupDictionary == null)
		{
			m_CharacterLookupDictionary = new Dictionary<uint, Character>();
		}
		else
		{
			m_CharacterLookupDictionary.Clear();
		}
		for (int i = 0; i < m_CharacterTable.Count; i++)
		{
			Character character = m_CharacterTable[i];
			uint unicode = character.unicode;
			uint glyphIndex = character.glyphIndex;
			if (!m_CharacterLookupDictionary.ContainsKey(unicode))
			{
				m_CharacterLookupDictionary.Add(unicode, character);
				character.textAsset = this;
				character.glyph = m_GlyphLookupDictionary[glyphIndex];
			}
		}
	}

	internal void InitializeLigatureSubstitutionLookupDictionary()
	{
		if (m_FontFeatureTable.m_LigatureSubstitutionRecordLookup == null)
		{
			m_FontFeatureTable.m_LigatureSubstitutionRecordLookup = new Dictionary<uint, List<LigatureSubstitutionRecord>>();
		}
		else
		{
			m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.Clear();
		}
		List<LigatureSubstitutionRecord> ligatureSubstitutionRecords = m_FontFeatureTable.m_LigatureSubstitutionRecords;
		if (ligatureSubstitutionRecords == null)
		{
			return;
		}
		for (int i = 0; i < ligatureSubstitutionRecords.Count; i++)
		{
			LigatureSubstitutionRecord item = ligatureSubstitutionRecords[i];
			if (item.componentGlyphIDs != null && item.componentGlyphIDs.Length != 0)
			{
				uint key = item.componentGlyphIDs[0];
				if (!m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.ContainsKey(key))
				{
					m_FontFeatureTable.m_LigatureSubstitutionRecordLookup.Add(key, new List<LigatureSubstitutionRecord> { item });
				}
				else
				{
					m_FontFeatureTable.m_LigatureSubstitutionRecordLookup[key].Add(item);
				}
			}
		}
	}

	internal void InitializeGlyphPaidAdjustmentRecordsLookupDictionary()
	{
		if (m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup == null)
		{
			m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup = new Dictionary<uint, GlyphPairAdjustmentRecord>();
		}
		else
		{
			m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Clear();
		}
		List<GlyphPairAdjustmentRecord> glyphPairAdjustmentRecords = m_FontFeatureTable.m_GlyphPairAdjustmentRecords;
		if (glyphPairAdjustmentRecords == null)
		{
			return;
		}
		for (int i = 0; i < glyphPairAdjustmentRecords.Count; i++)
		{
			GlyphPairAdjustmentRecord value = glyphPairAdjustmentRecords[i];
			uint key = (value.secondAdjustmentRecord.glyphIndex << 16) | value.firstAdjustmentRecord.glyphIndex;
			if (!m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.ContainsKey(key))
			{
				m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Add(key, value);
			}
		}
	}

	internal void InitializeMarkToBaseAdjustmentRecordsLookupDictionary()
	{
		if (m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup == null)
		{
			m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup = new Dictionary<uint, MarkToBaseAdjustmentRecord>();
		}
		else
		{
			m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.Clear();
		}
		List<MarkToBaseAdjustmentRecord> markToBaseAdjustmentRecords = m_FontFeatureTable.m_MarkToBaseAdjustmentRecords;
		if (markToBaseAdjustmentRecords == null)
		{
			return;
		}
		for (int i = 0; i < markToBaseAdjustmentRecords.Count; i++)
		{
			MarkToBaseAdjustmentRecord value = markToBaseAdjustmentRecords[i];
			uint key = (value.markGlyphID << 16) | value.baseGlyphID;
			if (!m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.ContainsKey(key))
			{
				m_FontFeatureTable.m_MarkToBaseAdjustmentRecordLookup.Add(key, value);
			}
		}
	}

	internal void InitializeMarkToMarkAdjustmentRecordsLookupDictionary()
	{
		if (m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup == null)
		{
			m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup = new Dictionary<uint, MarkToMarkAdjustmentRecord>();
		}
		else
		{
			m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.Clear();
		}
		List<MarkToMarkAdjustmentRecord> markToMarkAdjustmentRecords = m_FontFeatureTable.m_MarkToMarkAdjustmentRecords;
		if (markToMarkAdjustmentRecords == null)
		{
			return;
		}
		for (int i = 0; i < markToMarkAdjustmentRecords.Count; i++)
		{
			MarkToMarkAdjustmentRecord value = markToMarkAdjustmentRecords[i];
			uint key = (value.combiningMarkGlyphID << 16) | value.baseMarkGlyphID;
			if (!m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.ContainsKey(key))
			{
				m_FontFeatureTable.m_MarkToMarkAdjustmentRecordLookup.Add(key, value);
			}
		}
	}

	internal void AddSynthesizedCharactersAndFaceMetrics()
	{
		bool flag = false;
		if (m_AtlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS)
		{
			flag = LoadFontFace() == FontEngineError.Success;
			if (!flag && !InternalDynamicOS)
			{
				Debug.LogWarning("Unable to load font face for [" + base.name + "] font asset.", this);
			}
		}
		AddSynthesizedCharacter(3u, flag, addImmediately: true);
		AddSynthesizedCharacter(9u, flag, addImmediately: true);
		AddSynthesizedCharacter(10u, flag);
		AddSynthesizedCharacter(11u, flag);
		AddSynthesizedCharacter(13u, flag);
		AddSynthesizedCharacter(1564u, flag);
		AddSynthesizedCharacter(8203u, flag);
		AddSynthesizedCharacter(8206u, flag);
		AddSynthesizedCharacter(8207u, flag);
		AddSynthesizedCharacter(8232u, flag);
		AddSynthesizedCharacter(8233u, flag);
		AddSynthesizedCharacter(8288u, flag);
	}

	private void AddSynthesizedCharacter(uint unicode, bool isFontFaceLoaded, bool addImmediately = false)
	{
		if (m_CharacterLookupDictionary.ContainsKey(unicode))
		{
			return;
		}
		if (isFontFaceLoaded && FontEngine.GetGlyphIndex(unicode) != 0)
		{
			if (addImmediately)
			{
				GlyphLoadFlags flags = (((m_AtlasRenderMode & (GlyphRenderMode)4) == (GlyphRenderMode)4) ? (GlyphLoadFlags.LOAD_NO_HINTING | GlyphLoadFlags.LOAD_NO_BITMAP) : GlyphLoadFlags.LOAD_NO_BITMAP);
				if (FontEngine.TryGetGlyphWithUnicodeValue(unicode, flags, out var glyph))
				{
					m_CharacterLookupDictionary.Add(unicode, new Character(unicode, this, glyph));
				}
			}
		}
		else
		{
			Glyph glyph = new Glyph(0u, new GlyphMetrics(0f, 0f, 0f, 0f, 0f), GlyphRect.zero, 1f, 0);
			m_CharacterLookupDictionary.Add(unicode, new Character(unicode, this, glyph));
		}
	}

	internal void AddCharacterToLookupCache(uint unicode, Character character)
	{
		m_CharacterLookupDictionary.Add(unicode, character);
	}

	private FontEngineError LoadFontFace()
	{
		if (m_AtlasPopulationMode == AtlasPopulationMode.Dynamic)
		{
			if (FontEngine.LoadFontFace(m_SourceFontFile, m_FaceInfo.pointSize, m_FaceInfo.faceIndex) == FontEngineError.Success)
			{
				return FontEngineError.Success;
			}
			if (!string.IsNullOrEmpty(m_SourceFontFilePath))
			{
				return FontEngine.LoadFontFace(m_SourceFontFilePath, m_FaceInfo.pointSize, m_FaceInfo.faceIndex);
			}
			return FontEngineError.Invalid_Face;
		}
		return FontEngine.LoadFontFace(m_FaceInfo.familyName, m_FaceInfo.styleName, m_FaceInfo.pointSize);
	}

	internal void SortCharacterTable()
	{
		if (m_CharacterTable != null && m_CharacterTable.Count > 0)
		{
			m_CharacterTable = m_CharacterTable.OrderBy((Character c) => c.unicode).ToList();
		}
	}

	internal void SortGlyphTable()
	{
		if (m_GlyphTable != null && m_GlyphTable.Count > 0)
		{
			m_GlyphTable = m_GlyphTable.OrderBy((Glyph c) => c.index).ToList();
		}
	}

	internal void SortFontFeatureTable()
	{
		m_FontFeatureTable.SortGlyphPairAdjustmentRecords();
		m_FontFeatureTable.SortMarkToBaseAdjustmentRecords();
		m_FontFeatureTable.SortMarkToMarkAdjustmentRecords();
	}

	internal void SortAllTables()
	{
		SortGlyphTable();
		SortCharacterTable();
		SortFontFeatureTable();
	}

	public bool HasCharacter(int character)
	{
		if (m_CharacterLookupDictionary == null)
		{
			return false;
		}
		return m_CharacterLookupDictionary.ContainsKey((uint)character);
	}

	public bool HasCharacter(char character, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		return HasCharacter((uint)character, searchFallbacks, tryAddCharacter);
	}

	public bool HasCharacter(uint character, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		if (m_CharacterLookupDictionary == null)
		{
			ReadFontAssetDefinition();
			if (m_CharacterLookupDictionary == null)
			{
				return false;
			}
		}
		if (m_CharacterLookupDictionary.ContainsKey(character))
		{
			return true;
		}
		if (tryAddCharacter && (m_AtlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && TryAddCharacterInternal(character, out var _))
		{
			return true;
		}
		if (searchFallbacks)
		{
			if (k_SearchedFontAssetLookup == null)
			{
				k_SearchedFontAssetLookup = new HashSet<int>();
			}
			else
			{
				k_SearchedFontAssetLookup.Clear();
			}
			k_SearchedFontAssetLookup.Add(GetInstanceID());
			if (fallbackFontAssetTable != null && fallbackFontAssetTable.Count > 0)
			{
				for (int i = 0; i < fallbackFontAssetTable.Count && fallbackFontAssetTable[i] != null; i++)
				{
					FontAsset fontAsset = fallbackFontAssetTable[i];
					int item = fontAsset.GetInstanceID();
					if (k_SearchedFontAssetLookup.Add(item) && fontAsset.HasCharacter_Internal(character, searchFallbacks: true, tryAddCharacter))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool HasCharacter_Internal(uint character, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		if (m_CharacterLookupDictionary == null)
		{
			ReadFontAssetDefinition();
			if (m_CharacterLookupDictionary == null)
			{
				return false;
			}
		}
		if (m_CharacterLookupDictionary.ContainsKey(character))
		{
			return true;
		}
		if (tryAddCharacter && (atlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && TryAddCharacterInternal(character, out var _))
		{
			return true;
		}
		if (searchFallbacks)
		{
			if (fallbackFontAssetTable == null || fallbackFontAssetTable.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < fallbackFontAssetTable.Count && fallbackFontAssetTable[i] != null; i++)
			{
				FontAsset fontAsset = fallbackFontAssetTable[i];
				int item = fontAsset.GetInstanceID();
				if (k_SearchedFontAssetLookup.Add(item) && fontAsset.HasCharacter_Internal(character, searchFallbacks: true, tryAddCharacter))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasCharacters(string text, out List<char> missingCharacters)
	{
		if (m_CharacterLookupDictionary == null)
		{
			missingCharacters = null;
			return false;
		}
		missingCharacters = new List<char>();
		for (int i = 0; i < text.Length; i++)
		{
			if (!m_CharacterLookupDictionary.ContainsKey(text[i]))
			{
				missingCharacters.Add(text[i]);
			}
		}
		if (missingCharacters.Count == 0)
		{
			return true;
		}
		return false;
	}

	public bool HasCharacters(string text, out uint[] missingCharacters, bool searchFallbacks = false, bool tryAddCharacter = false)
	{
		missingCharacters = null;
		if (m_CharacterLookupDictionary == null)
		{
			ReadFontAssetDefinition();
			if (m_CharacterLookupDictionary == null)
			{
				return false;
			}
		}
		s_MissingCharacterList.Clear();
		for (int i = 0; i < text.Length; i++)
		{
			bool flag = true;
			uint num = text[i];
			if (m_CharacterLookupDictionary.ContainsKey(num) || (tryAddCharacter && (atlasPopulationMode == AtlasPopulationMode.Dynamic || m_AtlasPopulationMode == AtlasPopulationMode.DynamicOS) && TryAddCharacterInternal(num, out var _)))
			{
				continue;
			}
			if (searchFallbacks)
			{
				if (k_SearchedFontAssetLookup == null)
				{
					k_SearchedFontAssetLookup = new HashSet<int>();
				}
				else
				{
					k_SearchedFontAssetLookup.Clear();
				}
				k_SearchedFontAssetLookup.Add(GetInstanceID());
				if (fallbackFontAssetTable != null && fallbackFontAssetTable.Count > 0)
				{
					for (int j = 0; j < fallbackFontAssetTable.Count && fallbackFontAssetTable[j] != null; j++)
					{
						FontAsset fontAsset = fallbackFontAssetTable[j];
						int item = fontAsset.GetInstanceID();
						if (k_SearchedFontAssetLookup.Add(item) && fontAsset.HasCharacter_Internal(num, searchFallbacks: true, tryAddCharacter))
						{
							flag = false;
							break;
						}
					}
				}
			}
			if (flag)
			{
				s_MissingCharacterList.Add(num);
			}
		}
		if (s_MissingCharacterList.Count > 0)
		{
			missingCharacters = s_MissingCharacterList.ToArray();
			return false;
		}
		return true;
	}

	public bool HasCharacters(string text)
	{
		if (m_CharacterLookupDictionary == null)
		{
			return false;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!m_CharacterLookupDictionary.ContainsKey(text[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static string GetCharacters(FontAsset fontAsset)
	{
		string text = string.Empty;
		for (int i = 0; i < fontAsset.characterTable.Count; i++)
		{
			text += (char)fontAsset.characterTable[i].unicode;
		}
		return text;
	}

	public static int[] GetCharactersArray(FontAsset fontAsset)
	{
		int[] array = new int[fontAsset.characterTable.Count];
		for (int i = 0; i < fontAsset.characterTable.Count; i++)
		{
			array[i] = (int)fontAsset.characterTable[i].unicode;
		}
		return array;
	}

	internal uint GetGlyphIndex(uint unicode)
	{
		if (m_CharacterLookupDictionary.ContainsKey(unicode))
		{
			return m_CharacterLookupDictionary[unicode].glyphIndex;
		}
		return (LoadFontFace() == FontEngineError.Success) ? FontEngine.GetGlyphIndex(unicode) : 0u;
	}

	internal static void RegisterFontAssetForFontFeatureUpdate(FontAsset fontAsset)
	{
		int item = fontAsset.instanceID;
		if (k_FontAssets_FontFeaturesUpdateQueueLookup.Add(item))
		{
			k_FontAssets_FontFeaturesUpdateQueue.Add(fontAsset);
		}
	}

	internal static void UpdateFontFeaturesForFontAssetsInQueue()
	{
		int count = k_FontAssets_FontFeaturesUpdateQueue.Count;
		for (int i = 0; i < count; i++)
		{
			k_FontAssets_FontFeaturesUpdateQueue[i].UpdateAllFontFeatures();
		}
		if (count > 0)
		{
			k_FontAssets_FontFeaturesUpdateQueue.Clear();
			k_FontAssets_FontFeaturesUpdateQueueLookup.Clear();
		}
	}

	internal static void RegisterAtlasTextureForApply(Texture2D texture)
	{
		int item = texture.GetInstanceID();
		if (k_FontAssets_AtlasTexturesUpdateQueueLookup.Add(item))
		{
			k_FontAssets_AtlasTexturesUpdateQueue.Add(texture);
		}
	}

	internal static void UpdateAtlasTexturesInQueue()
	{
		int count = k_FontAssets_AtlasTexturesUpdateQueueLookup.Count;
		for (int i = 0; i < count; i++)
		{
			k_FontAssets_AtlasTexturesUpdateQueue[i].Apply(updateMipmaps: false, makeNoLongerReadable: false);
		}
		if (count > 0)
		{
			k_FontAssets_AtlasTexturesUpdateQueue.Clear();
			k_FontAssets_AtlasTexturesUpdateQueueLookup.Clear();
		}
	}

	internal static void UpdateFontAssetsInUpdateQueue()
	{
		UpdateAtlasTexturesInQueue();
		UpdateFontFeaturesForFontAssetsInQueue();
	}

	public bool TryAddCharacters(uint[] unicodes, bool includeFontFeatures = false)
	{
		uint[] missingUnicodes;
		return TryAddCharacters(unicodes, out missingUnicodes, includeFontFeatures);
	}

	public bool TryAddCharacters(uint[] unicodes, out uint[] missingUnicodes, bool includeFontFeatures = false)
	{
		if (unicodes == null || unicodes.Length == 0 || m_AtlasPopulationMode == AtlasPopulationMode.Static)
		{
			if (m_AtlasPopulationMode == AtlasPopulationMode.Static)
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because its AtlasPopulationMode is set to Static.", this);
			}
			else
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because the provided Unicode list is Null or Empty.", this);
			}
			missingUnicodes = null;
			return false;
		}
		if (LoadFontFace() != FontEngineError.Success)
		{
			missingUnicodes = unicodes.ToArray();
			return false;
		}
		if (m_CharacterLookupDictionary == null || m_GlyphLookupDictionary == null)
		{
			ReadFontAssetDefinition();
		}
		m_GlyphsToAdd.Clear();
		m_GlyphsToAddLookup.Clear();
		m_CharactersToAdd.Clear();
		m_CharactersToAddLookup.Clear();
		s_MissingCharacterList.Clear();
		bool flag = false;
		int num = unicodes.Length;
		for (int i = 0; i < num; i++)
		{
			uint num2 = unicodes[i];
			if (m_CharacterLookupDictionary.ContainsKey(num2))
			{
				continue;
			}
			uint glyphIndex = FontEngine.GetGlyphIndex(num2);
			if (glyphIndex == 0)
			{
				switch (num2)
				{
				case 160u:
					glyphIndex = FontEngine.GetGlyphIndex(32u);
					break;
				case 173u:
				case 8209u:
					glyphIndex = FontEngine.GetGlyphIndex(45u);
					break;
				}
				if (glyphIndex == 0)
				{
					s_MissingCharacterList.Add(num2);
					flag = true;
					continue;
				}
			}
			Character character = new Character(num2, glyphIndex);
			if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
			{
				character.glyph = m_GlyphLookupDictionary[glyphIndex];
				character.textAsset = this;
				m_CharacterTable.Add(character);
				m_CharacterLookupDictionary.Add(num2, character);
				continue;
			}
			if (m_GlyphsToAddLookup.Add(glyphIndex))
			{
				m_GlyphsToAdd.Add(glyphIndex);
			}
			if (m_CharactersToAddLookup.Add(num2))
			{
				m_CharactersToAdd.Add(character);
			}
		}
		if (m_GlyphsToAdd.Count == 0)
		{
			missingUnicodes = unicodes;
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width != m_AtlasWidth || m_AtlasTextures[m_AtlasTextureIndex].height != m_AtlasHeight)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		Glyph[] glyphs;
		bool flag2 = FontEngine.TryAddGlyphsToTexture(m_GlyphsToAdd, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyphs);
		for (int j = 0; j < glyphs.Length && glyphs[j] != null; j++)
		{
			Glyph glyph = glyphs[j];
			uint index = glyph.index;
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(index, glyph);
			m_GlyphIndexListNewlyAdded.Add(index);
			m_GlyphIndexList.Add(index);
		}
		m_GlyphsToAdd.Clear();
		for (int k = 0; k < m_CharactersToAdd.Count; k++)
		{
			Character character2 = m_CharactersToAdd[k];
			if (!m_GlyphLookupDictionary.TryGetValue(character2.glyphIndex, out var value))
			{
				m_GlyphsToAdd.Add(character2.glyphIndex);
				continue;
			}
			character2.glyph = value;
			character2.textAsset = this;
			m_CharacterTable.Add(character2);
			m_CharacterLookupDictionary.Add(character2.unicode, character2);
			m_CharactersToAdd.RemoveAt(k);
			k--;
		}
		if (m_IsMultiAtlasTexturesEnabled && !flag2)
		{
			while (!flag2)
			{
				flag2 = TryAddGlyphsToNewAtlasTexture();
			}
		}
		if (includeFontFeatures)
		{
			UpdateAllFontFeatures();
		}
		for (int l = 0; l < m_CharactersToAdd.Count; l++)
		{
			Character character3 = m_CharactersToAdd[l];
			s_MissingCharacterList.Add(character3.unicode);
		}
		missingUnicodes = null;
		if (s_MissingCharacterList.Count > 0)
		{
			missingUnicodes = s_MissingCharacterList.ToArray();
		}
		return flag2 && !flag;
	}

	public bool TryAddCharacters(string characters, bool includeFontFeatures = false)
	{
		string missingCharacters;
		return TryAddCharacters(characters, out missingCharacters, includeFontFeatures);
	}

	public bool TryAddCharacters(string characters, out string missingCharacters, bool includeFontFeatures = false)
	{
		if (string.IsNullOrEmpty(characters) || m_AtlasPopulationMode == AtlasPopulationMode.Static)
		{
			if (m_AtlasPopulationMode == AtlasPopulationMode.Static)
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because its AtlasPopulationMode is set to Static.", this);
			}
			else
			{
				Debug.LogWarning("Unable to add characters to font asset [" + base.name + "] because the provided character list is Null or Empty.", this);
			}
			missingCharacters = characters;
			return false;
		}
		if (LoadFontFace() != FontEngineError.Success)
		{
			missingCharacters = characters;
			return false;
		}
		if (m_CharacterLookupDictionary == null || m_GlyphLookupDictionary == null)
		{
			ReadFontAssetDefinition();
		}
		m_GlyphsToAdd.Clear();
		m_GlyphsToAddLookup.Clear();
		m_CharactersToAdd.Clear();
		m_CharactersToAddLookup.Clear();
		s_MissingCharacterList.Clear();
		bool flag = false;
		int length = characters.Length;
		for (int i = 0; i < length; i++)
		{
			uint num = characters[i];
			if (m_CharacterLookupDictionary.ContainsKey(num))
			{
				continue;
			}
			uint glyphIndex = FontEngine.GetGlyphIndex(num);
			if (glyphIndex == 0)
			{
				switch (num)
				{
				case 160u:
					glyphIndex = FontEngine.GetGlyphIndex(32u);
					break;
				case 173u:
				case 8209u:
					glyphIndex = FontEngine.GetGlyphIndex(45u);
					break;
				}
				if (glyphIndex == 0)
				{
					s_MissingCharacterList.Add(num);
					flag = true;
					continue;
				}
			}
			Character character = new Character(num, glyphIndex);
			if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
			{
				character.glyph = m_GlyphLookupDictionary[glyphIndex];
				character.textAsset = this;
				m_CharacterTable.Add(character);
				m_CharacterLookupDictionary.Add(num, character);
				continue;
			}
			if (m_GlyphsToAddLookup.Add(glyphIndex))
			{
				m_GlyphsToAdd.Add(glyphIndex);
			}
			if (m_CharactersToAddLookup.Add(num))
			{
				m_CharactersToAdd.Add(character);
			}
		}
		if (m_GlyphsToAdd.Count == 0)
		{
			missingCharacters = characters;
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width != m_AtlasWidth || m_AtlasTextures[m_AtlasTextureIndex].height != m_AtlasHeight)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		Glyph[] glyphs;
		bool flag2 = FontEngine.TryAddGlyphsToTexture(m_GlyphsToAdd, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyphs);
		for (int j = 0; j < glyphs.Length && glyphs[j] != null; j++)
		{
			Glyph glyph = glyphs[j];
			uint index = glyph.index;
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(index, glyph);
			m_GlyphIndexListNewlyAdded.Add(index);
			m_GlyphIndexList.Add(index);
		}
		m_GlyphsToAdd.Clear();
		for (int k = 0; k < m_CharactersToAdd.Count; k++)
		{
			Character character2 = m_CharactersToAdd[k];
			if (!m_GlyphLookupDictionary.TryGetValue(character2.glyphIndex, out var value))
			{
				m_GlyphsToAdd.Add(character2.glyphIndex);
				continue;
			}
			character2.glyph = value;
			character2.textAsset = this;
			m_CharacterTable.Add(character2);
			m_CharacterLookupDictionary.Add(character2.unicode, character2);
			m_CharactersToAdd.RemoveAt(k);
			k--;
		}
		if (m_IsMultiAtlasTexturesEnabled && !flag2)
		{
			while (!flag2)
			{
				flag2 = TryAddGlyphsToNewAtlasTexture();
			}
		}
		if (includeFontFeatures)
		{
			UpdateAllFontFeatures();
		}
		missingCharacters = string.Empty;
		for (int l = 0; l < m_CharactersToAdd.Count; l++)
		{
			Character character3 = m_CharactersToAdd[l];
			s_MissingCharacterList.Add(character3.unicode);
		}
		if (s_MissingCharacterList.Count > 0)
		{
			missingCharacters = s_MissingCharacterList.UintToString();
		}
		return flag2 && !flag;
	}

	internal bool TryAddGlyphInternal(uint glyphIndex, out Glyph glyph)
	{
		using (k_TryAddGlyphMarker.Auto())
		{
			glyph = null;
			if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
			{
				glyph = m_GlyphLookupDictionary[glyphIndex];
				return true;
			}
			if (LoadFontFace() != FontEngineError.Success)
			{
				return false;
			}
			if (!m_AtlasTextures[m_AtlasTextureIndex].isReadable)
			{
				Debug.LogWarning("Unable to add the requested glyph to font asset [" + base.name + "]'s atlas texture. Please make the texture [" + m_AtlasTextures[m_AtlasTextureIndex].name + "] readable.", m_AtlasTextures[m_AtlasTextureIndex]);
				return false;
			}
			if (m_AtlasTextures[m_AtlasTextureIndex].width != m_AtlasWidth || m_AtlasTextures[m_AtlasTextureIndex].height != m_AtlasHeight)
			{
				m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
				FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
			}
			FontEngine.SetTextureUploadMode(shouldUploadImmediately: false);
			if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
			{
				glyph.atlasIndex = m_AtlasTextureIndex;
				m_GlyphTable.Add(glyph);
				m_GlyphLookupDictionary.Add(glyphIndex, glyph);
				m_GlyphIndexList.Add(glyphIndex);
				m_GlyphIndexListNewlyAdded.Add(glyphIndex);
				return true;
			}
			if (m_IsMultiAtlasTexturesEnabled)
			{
				SetupNewAtlasTexture();
				if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
				{
					glyph.atlasIndex = m_AtlasTextureIndex;
					m_GlyphTable.Add(glyph);
					m_GlyphLookupDictionary.Add(glyphIndex, glyph);
					m_GlyphIndexList.Add(glyphIndex);
					m_GlyphIndexListNewlyAdded.Add(glyphIndex);
					return true;
				}
			}
		}
		return false;
	}

	internal bool TryAddCharacterInternal(uint unicode, out Character character, bool shouldGetFontFeatures = false)
	{
		character = null;
		if (m_MissingUnicodesFromFontFile.Contains(unicode))
		{
			return false;
		}
		if (LoadFontFace() != FontEngineError.Success)
		{
			return false;
		}
		uint glyphIndex = FontEngine.GetGlyphIndex(unicode);
		if (glyphIndex == 0)
		{
			switch (unicode)
			{
			case 160u:
				glyphIndex = FontEngine.GetGlyphIndex(32u);
				break;
			case 173u:
			case 8209u:
				glyphIndex = FontEngine.GetGlyphIndex(45u);
				break;
			}
			if (glyphIndex == 0)
			{
				m_MissingUnicodesFromFontFile.Add(unicode);
				return false;
			}
		}
		if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
		{
			character = new Character(unicode, this, m_GlyphLookupDictionary[glyphIndex]);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			return true;
		}
		Glyph glyph = null;
		if (!m_AtlasTextures[m_AtlasTextureIndex].isReadable)
		{
			Debug.LogWarning("Unable to add the requested character to font asset [" + base.name + "]'s atlas texture. Please make the texture [" + m_AtlasTextures[m_AtlasTextureIndex].name + "] readable.", m_AtlasTextures[m_AtlasTextureIndex]);
			return false;
		}
		if (m_AtlasTextures[m_AtlasTextureIndex].width != m_AtlasWidth || m_AtlasTextures[m_AtlasTextureIndex].height != m_AtlasHeight)
		{
			m_AtlasTextures[m_AtlasTextureIndex].Reinitialize(m_AtlasWidth, m_AtlasHeight);
			FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		}
		FontEngine.SetTextureUploadMode(shouldUploadImmediately: false);
		if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
		{
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(glyphIndex, glyph);
			character = new Character(unicode, this, glyph);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			m_GlyphIndexList.Add(glyphIndex);
			m_GlyphIndexListNewlyAdded.Add(glyphIndex);
			if (shouldGetFontFeatures)
			{
				RegisterFontAssetForFontFeatureUpdate(this);
			}
			RegisterAtlasTextureForApply(m_AtlasTextures[m_AtlasTextureIndex]);
			FontEngine.SetTextureUploadMode(shouldUploadImmediately: true);
			return true;
		}
		if (m_IsMultiAtlasTexturesEnabled)
		{
			SetupNewAtlasTexture();
			if (FontEngine.TryAddGlyphToTexture(glyphIndex, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyph))
			{
				glyph.atlasIndex = m_AtlasTextureIndex;
				m_GlyphTable.Add(glyph);
				m_GlyphLookupDictionary.Add(glyphIndex, glyph);
				character = new Character(unicode, this, glyph);
				m_CharacterTable.Add(character);
				m_CharacterLookupDictionary.Add(unicode, character);
				m_GlyphIndexList.Add(glyphIndex);
				m_GlyphIndexListNewlyAdded.Add(glyphIndex);
				if (shouldGetFontFeatures)
				{
					RegisterFontAssetForFontFeatureUpdate(this);
				}
				RegisterAtlasTextureForApply(m_AtlasTextures[m_AtlasTextureIndex]);
				FontEngine.SetTextureUploadMode(shouldUploadImmediately: true);
				return true;
			}
		}
		return false;
	}

	internal bool TryGetCharacter_and_QueueRenderToTexture(uint unicode, out Character character, bool shouldGetFontFeatures = false)
	{
		character = null;
		if (m_MissingUnicodesFromFontFile.Contains(unicode))
		{
			return false;
		}
		if (LoadFontFace() != FontEngineError.Success)
		{
			return false;
		}
		uint glyphIndex = FontEngine.GetGlyphIndex(unicode);
		if (glyphIndex == 0)
		{
			switch (unicode)
			{
			case 160u:
				glyphIndex = FontEngine.GetGlyphIndex(32u);
				break;
			case 173u:
			case 8209u:
				glyphIndex = FontEngine.GetGlyphIndex(45u);
				break;
			}
			if (glyphIndex == 0)
			{
				m_MissingUnicodesFromFontFile.Add(unicode);
				return false;
			}
		}
		if (m_GlyphLookupDictionary.ContainsKey(glyphIndex))
		{
			character = new Character(unicode, this, m_GlyphLookupDictionary[glyphIndex]);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			return true;
		}
		GlyphLoadFlags flags = ((((GlyphRenderMode)4 & m_AtlasRenderMode) == (GlyphRenderMode)4) ? (GlyphLoadFlags.LOAD_NO_HINTING | GlyphLoadFlags.LOAD_NO_BITMAP) : GlyphLoadFlags.LOAD_NO_BITMAP);
		Glyph glyph = null;
		if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, flags, out glyph))
		{
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(glyphIndex, glyph);
			character = new Character(unicode, this, glyph);
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(unicode, character);
			m_GlyphIndexList.Add(glyphIndex);
			m_GlyphIndexListNewlyAdded.Add(glyphIndex);
			if (shouldGetFontFeatures)
			{
				RegisterFontAssetForFontFeatureUpdate(this);
			}
			m_GlyphsToRender.Add(glyph);
			return true;
		}
		return false;
	}

	internal void TryAddGlyphsToAtlasTextures()
	{
	}

	private bool TryAddGlyphsToNewAtlasTexture()
	{
		SetupNewAtlasTexture();
		Glyph[] glyphs;
		bool result = FontEngine.TryAddGlyphsToTexture(m_GlyphsToAdd, m_AtlasPadding, GlyphPackingMode.BestShortSideFit, m_FreeGlyphRects, m_UsedGlyphRects, m_AtlasRenderMode, m_AtlasTextures[m_AtlasTextureIndex], out glyphs);
		for (int i = 0; i < glyphs.Length && glyphs[i] != null; i++)
		{
			Glyph glyph = glyphs[i];
			uint index = glyph.index;
			glyph.atlasIndex = m_AtlasTextureIndex;
			m_GlyphTable.Add(glyph);
			m_GlyphLookupDictionary.Add(index, glyph);
			m_GlyphIndexListNewlyAdded.Add(index);
			m_GlyphIndexList.Add(index);
		}
		m_GlyphsToAdd.Clear();
		for (int j = 0; j < m_CharactersToAdd.Count; j++)
		{
			Character character = m_CharactersToAdd[j];
			if (!m_GlyphLookupDictionary.TryGetValue(character.glyphIndex, out var value))
			{
				m_GlyphsToAdd.Add(character.glyphIndex);
				continue;
			}
			character.glyph = value;
			character.textAsset = this;
			m_CharacterTable.Add(character);
			m_CharacterLookupDictionary.Add(character.unicode, character);
			m_CharactersToAdd.RemoveAt(j);
			j--;
		}
		return result;
	}

	private void SetupNewAtlasTexture()
	{
		m_AtlasTextureIndex++;
		if (m_AtlasTextures.Length == m_AtlasTextureIndex)
		{
			Array.Resize(ref m_AtlasTextures, m_AtlasTextures.Length * 2);
		}
		m_AtlasTextures[m_AtlasTextureIndex] = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, mipChain: false);
		FontEngine.ResetAtlasTexture(m_AtlasTextures[m_AtlasTextureIndex]);
		int num = (((m_AtlasRenderMode & (GlyphRenderMode)16) != (GlyphRenderMode)16) ? 1 : 0);
		m_FreeGlyphRects.Clear();
		m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - num, m_AtlasHeight - num));
		m_UsedGlyphRects.Clear();
	}

	private void UpdateAllFontFeatures()
	{
		UpdateGlyphAdjustmentRecords();
		m_GlyphIndexListNewlyAdded.Clear();
	}

	internal void UpdateGlyphAdjustmentRecords()
	{
		using (k_UpdateGlyphAdjustmentRecordsMarker.Auto())
		{
			int recordCount;
			GlyphPairAdjustmentRecord[] glyphPairAdjustmentRecords = FontEngine.GetGlyphPairAdjustmentRecords(m_GlyphIndexList, out recordCount);
			m_GlyphIndexListNewlyAdded.Clear();
			if (glyphPairAdjustmentRecords == null || glyphPairAdjustmentRecords.Length == 0)
			{
				return;
			}
			if (m_FontFeatureTable == null)
			{
				m_FontFeatureTable = new FontFeatureTable();
			}
			for (int i = 0; i < glyphPairAdjustmentRecords.Length && glyphPairAdjustmentRecords[i].firstAdjustmentRecord.glyphIndex != 0; i++)
			{
				uint key = (glyphPairAdjustmentRecords[i].secondAdjustmentRecord.glyphIndex << 16) | glyphPairAdjustmentRecords[i].firstAdjustmentRecord.glyphIndex;
				if (!m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.ContainsKey(key))
				{
					GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = glyphPairAdjustmentRecords[i];
					m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Add(glyphPairAdjustmentRecord);
					m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Add(key, glyphPairAdjustmentRecord);
				}
			}
		}
	}

	internal void UpdateGlyphAdjustmentRecords(uint[] glyphIndexes)
	{
		using (k_UpdateGlyphAdjustmentRecordsMarker.Auto())
		{
			GlyphPairAdjustmentRecord[] glyphPairAdjustmentTable = FontEngine.GetGlyphPairAdjustmentTable(glyphIndexes);
			if (glyphPairAdjustmentTable == null || glyphPairAdjustmentTable.Length == 0)
			{
				return;
			}
			if (m_FontFeatureTable == null)
			{
				m_FontFeatureTable = new FontFeatureTable();
			}
			for (int i = 0; i < glyphPairAdjustmentTable.Length && glyphPairAdjustmentTable[i].firstAdjustmentRecord.glyphIndex != 0; i++)
			{
				uint key = (glyphPairAdjustmentTable[i].secondAdjustmentRecord.glyphIndex << 16) | glyphPairAdjustmentTable[i].firstAdjustmentRecord.glyphIndex;
				if (!m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.ContainsKey(key))
				{
					GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = glyphPairAdjustmentTable[i];
					m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Add(glyphPairAdjustmentRecord);
					m_FontFeatureTable.m_GlyphPairAdjustmentRecordLookup.Add(key, glyphPairAdjustmentRecord);
				}
			}
		}
	}

	internal void UpdateGlyphAdjustmentRecords(List<uint> glyphIndexes)
	{
	}

	internal void UpdateGlyphAdjustmentRecords(List<uint> newGlyphIndexes, List<uint> allGlyphIndexes)
	{
	}

	private void CopyListDataToArray<T>(List<T> srcList, ref T[] dstArray)
	{
		int count = srcList.Count;
		if (dstArray == null)
		{
			dstArray = new T[count];
		}
		else
		{
			Array.Resize(ref dstArray, count);
		}
		for (int i = 0; i < count; i++)
		{
			dstArray[i] = srcList[i];
		}
	}

	public void ClearFontAssetData(bool setAtlasSizeToZero = false)
	{
		using (k_ClearFontAssetDataMarker.Auto())
		{
			ClearFontAssetTables(clearFontFeatures: true);
			ClearAtlasTextures(setAtlasSizeToZero);
			ReadFontAssetDefinition();
		}
	}

	internal void ClearFontAssetDataInternal(bool clearFontFeatures = false)
	{
		ClearFontAssetTables(clearFontFeatures);
		ClearAtlasTextures(setAtlasSizeToZero: true);
	}

	internal void UpdateFontAssetData()
	{
		using (k_UpdateFontAssetDataMarker.Auto())
		{
			uint[] array = new uint[m_CharacterTable.Count];
			for (int i = 0; i < m_CharacterTable.Count; i++)
			{
				array[i] = m_CharacterTable[i].unicode;
			}
			ClearFontAssetTables(clearFontFeatures: true);
			ClearAtlasTextures(setAtlasSizeToZero: true);
			ReadFontAssetDefinition();
			if (array.Length != 0)
			{
				TryAddCharacters(array, includeFontFeatures: true);
			}
		}
	}

	internal void ClearFontAssetTables(bool clearFontFeatures)
	{
		if (m_GlyphTable != null)
		{
			m_GlyphTable.Clear();
		}
		if (m_CharacterTable != null)
		{
			m_CharacterTable.Clear();
		}
		if (m_UsedGlyphRects != null)
		{
			m_UsedGlyphRects.Clear();
		}
		if (m_FreeGlyphRects != null)
		{
			int num = (((m_AtlasRenderMode & (GlyphRenderMode)16) != (GlyphRenderMode)16) ? 1 : 0);
			m_FreeGlyphRects.Clear();
			m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - num, m_AtlasHeight - num));
		}
		if (m_GlyphsToRender != null)
		{
			m_GlyphsToRender.Clear();
		}
		if (m_GlyphsRendered != null)
		{
			m_GlyphsRendered.Clear();
		}
		if (clearFontFeatures)
		{
			if (m_FontFeatureTable != null && m_FontFeatureTable.m_LigatureSubstitutionRecords != null)
			{
				m_FontFeatureTable.m_LigatureSubstitutionRecords.Clear();
			}
			if (m_FontFeatureTable != null && m_FontFeatureTable.m_GlyphPairAdjustmentRecords != null)
			{
				m_FontFeatureTable.m_GlyphPairAdjustmentRecords.Clear();
			}
			if (m_FontFeatureTable != null && m_FontFeatureTable.m_MarkToBaseAdjustmentRecords != null)
			{
				m_FontFeatureTable.m_MarkToBaseAdjustmentRecords.Clear();
			}
			if (m_FontFeatureTable != null && m_FontFeatureTable.m_MarkToMarkAdjustmentRecords != null)
			{
				m_FontFeatureTable.m_MarkToMarkAdjustmentRecords.Clear();
			}
		}
	}

	internal void ClearAtlasTextures(bool setAtlasSizeToZero = false)
	{
		m_AtlasTextureIndex = 0;
		if (m_AtlasTextures == null)
		{
			return;
		}
		Texture2D texture2D = null;
		for (int i = 1; i < m_AtlasTextures.Length; i++)
		{
			texture2D = m_AtlasTextures[i];
			if (!(texture2D == null))
			{
				Object.DestroyImmediate(texture2D, allowDestroyingAssets: true);
			}
		}
		Array.Resize(ref m_AtlasTextures, 1);
		texture2D = (m_AtlasTexture = m_AtlasTextures[0]);
		if (!texture2D.isReadable)
		{
		}
		if (setAtlasSizeToZero)
		{
			texture2D.Reinitialize(1, 1, TextureFormat.Alpha8, hasMipMap: false);
		}
		else if (texture2D.width != m_AtlasWidth || texture2D.height != m_AtlasHeight)
		{
			texture2D.Reinitialize(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, hasMipMap: false);
		}
		FontEngine.ResetAtlasTexture(texture2D);
		texture2D.Apply();
	}

	private void DestroyAtlasTextures()
	{
		if (m_AtlasTextures == null)
		{
			return;
		}
		for (int i = 0; i < m_AtlasTextures.Length; i++)
		{
			Texture2D texture2D = m_AtlasTextures[i];
			if (texture2D != null)
			{
				Object.DestroyImmediate(texture2D);
			}
		}
	}
}
