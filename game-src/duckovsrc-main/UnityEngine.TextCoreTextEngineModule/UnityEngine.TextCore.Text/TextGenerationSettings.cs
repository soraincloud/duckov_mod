using System;

namespace UnityEngine.TextCore.Text;

internal class TextGenerationSettings : IEquatable<TextGenerationSettings>
{
	public string text;

	public Rect screenRect;

	public Vector4 margins;

	public float scale = 1f;

	public FontAsset fontAsset;

	public Material material;

	public SpriteAsset spriteAsset;

	public TextStyleSheet styleSheet;

	public FontStyles fontStyle = FontStyles.Normal;

	public TextSettings textSettings;

	public TextAlignment textAlignment = TextAlignment.TopLeft;

	public TextOverflowMode overflowMode = TextOverflowMode.Overflow;

	public bool wordWrap = false;

	public float wordWrappingRatio;

	public Color color = Color.white;

	public TextColorGradient fontColorGradient;

	public TextColorGradient fontColorGradientPreset;

	public bool tintSprites;

	public bool overrideRichTextColors;

	public bool shouldConvertToLinearSpace = true;

	public float fontSize = 18f;

	public bool autoSize;

	public float fontSizeMin;

	public float fontSizeMax;

	public bool enableKerning = true;

	public bool richText;

	public bool isRightToLeft;

	public float extraPadding = 6f;

	public bool parseControlCharacters = true;

	public bool isOrthographic = true;

	public bool tagNoParsing = false;

	public float characterSpacing;

	public float wordSpacing;

	public float lineSpacing;

	public float paragraphSpacing;

	public float lineSpacingMax;

	public TextWrappingMode textWrappingMode = TextWrappingMode.Normal;

	public int maxVisibleCharacters = 99999;

	public int maxVisibleWords = 99999;

	public int maxVisibleLines = 99999;

	public int firstVisibleCharacter = 0;

	public bool useMaxVisibleDescender;

	public TextFontWeight fontWeight = TextFontWeight.Regular;

	public int pageToDisplay = 1;

	public TextureMapping horizontalMapping = TextureMapping.Character;

	public TextureMapping verticalMapping = TextureMapping.Character;

	public float uvLineOffset;

	public VertexSortingOrder geometrySortingOrder = VertexSortingOrder.Normal;

	public bool inverseYAxis;

	public float charWidthMaxAdj;

	internal TextInputSource inputSource = TextInputSource.TextString;

	public bool Equals(TextGenerationSettings other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		return text == other.text && screenRect.Equals(other.screenRect) && margins.Equals(other.margins) && scale.Equals(other.scale) && object.Equals(fontAsset, other.fontAsset) && object.Equals(material, other.material) && object.Equals(spriteAsset, other.spriteAsset) && object.Equals(styleSheet, other.styleSheet) && fontStyle == other.fontStyle && object.Equals(textSettings, other.textSettings) && textAlignment == other.textAlignment && overflowMode == other.overflowMode && wordWrap == other.wordWrap && wordWrappingRatio.Equals(other.wordWrappingRatio) && color.Equals(other.color) && object.Equals(fontColorGradient, other.fontColorGradient) && object.Equals(fontColorGradientPreset, other.fontColorGradientPreset) && tintSprites == other.tintSprites && overrideRichTextColors == other.overrideRichTextColors && shouldConvertToLinearSpace == other.shouldConvertToLinearSpace && fontSize.Equals(other.fontSize) && autoSize == other.autoSize && fontSizeMin.Equals(other.fontSizeMin) && fontSizeMax.Equals(other.fontSizeMax) && enableKerning == other.enableKerning && richText == other.richText && isRightToLeft == other.isRightToLeft && extraPadding == other.extraPadding && parseControlCharacters == other.parseControlCharacters && isOrthographic == other.isOrthographic && tagNoParsing == other.tagNoParsing && characterSpacing.Equals(other.characterSpacing) && wordSpacing.Equals(other.wordSpacing) && lineSpacing.Equals(other.lineSpacing) && paragraphSpacing.Equals(other.paragraphSpacing) && lineSpacingMax.Equals(other.lineSpacingMax) && textWrappingMode == other.textWrappingMode && maxVisibleCharacters == other.maxVisibleCharacters && maxVisibleWords == other.maxVisibleWords && maxVisibleLines == other.maxVisibleLines && firstVisibleCharacter == other.firstVisibleCharacter && useMaxVisibleDescender == other.useMaxVisibleDescender && fontWeight == other.fontWeight && pageToDisplay == other.pageToDisplay && horizontalMapping == other.horizontalMapping && verticalMapping == other.verticalMapping && uvLineOffset.Equals(other.uvLineOffset) && geometrySortingOrder == other.geometrySortingOrder && inverseYAxis == other.inverseYAxis && charWidthMaxAdj.Equals(other.charWidthMaxAdj) && inputSource == other.inputSource;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((TextGenerationSettings)obj);
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(text);
		hashCode.Add(screenRect);
		hashCode.Add(margins);
		hashCode.Add(scale);
		hashCode.Add(fontAsset);
		hashCode.Add(material);
		hashCode.Add(spriteAsset);
		hashCode.Add(styleSheet);
		hashCode.Add((int)fontStyle);
		hashCode.Add(textSettings);
		hashCode.Add((int)textAlignment);
		hashCode.Add((int)overflowMode);
		hashCode.Add(wordWrap);
		hashCode.Add(wordWrappingRatio);
		hashCode.Add(color);
		hashCode.Add(fontColorGradient);
		hashCode.Add(fontColorGradientPreset);
		hashCode.Add(tintSprites);
		hashCode.Add(overrideRichTextColors);
		hashCode.Add(shouldConvertToLinearSpace);
		hashCode.Add(fontSize);
		hashCode.Add(autoSize);
		hashCode.Add(fontSizeMin);
		hashCode.Add(fontSizeMax);
		hashCode.Add(enableKerning);
		hashCode.Add(richText);
		hashCode.Add(isRightToLeft);
		hashCode.Add(extraPadding);
		hashCode.Add(parseControlCharacters);
		hashCode.Add(isOrthographic);
		hashCode.Add(tagNoParsing);
		hashCode.Add(characterSpacing);
		hashCode.Add(wordSpacing);
		hashCode.Add(lineSpacing);
		hashCode.Add(paragraphSpacing);
		hashCode.Add(lineSpacingMax);
		hashCode.Add((int)textWrappingMode);
		hashCode.Add(maxVisibleCharacters);
		hashCode.Add(maxVisibleWords);
		hashCode.Add(maxVisibleLines);
		hashCode.Add(firstVisibleCharacter);
		hashCode.Add(useMaxVisibleDescender);
		hashCode.Add((int)fontWeight);
		hashCode.Add(pageToDisplay);
		hashCode.Add((int)horizontalMapping);
		hashCode.Add((int)verticalMapping);
		hashCode.Add(uvLineOffset);
		hashCode.Add((int)geometrySortingOrder);
		hashCode.Add(inverseYAxis);
		hashCode.Add(charWidthMaxAdj);
		hashCode.Add((int)inputSource);
		return hashCode.ToHashCode();
	}

	public static bool operator ==(TextGenerationSettings left, TextGenerationSettings right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(TextGenerationSettings left, TextGenerationSettings right)
	{
		return !object.Equals(left, right);
	}

	public override string ToString()
	{
		return string.Format("{0}: {1}\n {2}: {3}\n {4}: {5}\n {6}: {7}\n {8}: {9}\n {10}: {11}\n {12}: {13}\n {14}: {15}\n {16}: {17}\n {18}: {19}\n {20}: {21}\n {22}: {23}\n {24}: {25}\n {26}: {27}\n {28}: {29}\n {30}: {31}\n {32}: {33}\n {34}: {35}\n {36}: {37}\n {38}: {39}\n {40}: {41}\n {42}: {43}\n {44}: {45}\n {46}: {47}\n {48}: {49}\n {50}: {51}\n {52}: {53}\n {54}: {55}\n {56}: {57}\n {58}: {59}\n {60}: {61}\n {62}: {63}\n {64}: {65}\n {66}: {67}\n {68}: {69}\n {70}: {71}\n {72}: {73}\n {74}: {75}\n {76}: {77}\n {78}: {79}\n {80}: {81}\n {82}: {83}\n {84}: {85}\n {86}: {87}\n {88}: {89}\n {90}: {91}\n {92}: {93}\n {94}: {95}\n {96}: {97}\n {98}: {99}\n {100}: {101}", "text", text, "screenRect", screenRect, "margins", margins, "scale", scale, "fontAsset", fontAsset, "material", material, "spriteAsset", spriteAsset, "styleSheet", styleSheet, "fontStyle", fontStyle, "textSettings", textSettings, "textAlignment", textAlignment, "overflowMode", overflowMode, "wordWrap", wordWrap, "wordWrappingRatio", wordWrappingRatio, "color", color, "fontColorGradient", fontColorGradient, "fontColorGradientPreset", fontColorGradientPreset, "tintSprites", tintSprites, "overrideRichTextColors", overrideRichTextColors, "shouldConvertToLinearSpace", shouldConvertToLinearSpace, "fontSize", fontSize, "autoSize", autoSize, "fontSizeMin", fontSizeMin, "fontSizeMax", fontSizeMax, "enableKerning", enableKerning, "richText", richText, "isRightToLeft", isRightToLeft, "extraPadding", extraPadding, "parseControlCharacters", parseControlCharacters, "isOrthographic", isOrthographic, "tagNoParsing", tagNoParsing, "characterSpacing", characterSpacing, "wordSpacing", wordSpacing, "lineSpacing", lineSpacing, "paragraphSpacing", paragraphSpacing, "lineSpacingMax", lineSpacingMax, "textWrappingMode", textWrappingMode, "maxVisibleCharacters", maxVisibleCharacters, "maxVisibleWords", maxVisibleWords, "maxVisibleLines", maxVisibleLines, "firstVisibleCharacter", firstVisibleCharacter, "useMaxVisibleDescender", useMaxVisibleDescender, "fontWeight", fontWeight, "pageToDisplay", pageToDisplay, "horizontalMapping", horizontalMapping, "verticalMapping", verticalMapping, "uvLineOffset", uvLineOffset, "geometrySortingOrder", geometrySortingOrder, "inverseYAxis", inverseYAxis, "charWidthMaxAdj", charWidthMaxAdj, "inputSource", inputSource);
	}
}
