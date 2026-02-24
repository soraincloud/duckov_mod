using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

internal static class FontAssetUtilities
{
	private static HashSet<int> k_SearchedAssets;

	internal static Character GetCharacterFromFontAsset(uint unicode, FontAsset sourceFontAsset, bool includeFallbacks, FontStyles fontStyle, TextFontWeight fontWeight, out bool isAlternativeTypeface)
	{
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
		}
		return GetCharacterFromFontAsset_Internal(unicode, sourceFontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);
	}

	private static Character GetCharacterFromFontAsset_Internal(uint unicode, FontAsset sourceFontAsset, bool includeFallbacks, FontStyles fontStyle, TextFontWeight fontWeight, out bool isAlternativeTypeface)
	{
		isAlternativeTypeface = false;
		Character value = null;
		bool flag = (fontStyle & FontStyles.Italic) == FontStyles.Italic;
		if (flag || fontWeight != TextFontWeight.Regular)
		{
			FontWeightPair[] fontWeightTable = sourceFontAsset.fontWeightTable;
			int num = 4;
			switch (fontWeight)
			{
			case TextFontWeight.Thin:
				num = 1;
				break;
			case TextFontWeight.ExtraLight:
				num = 2;
				break;
			case TextFontWeight.Light:
				num = 3;
				break;
			case TextFontWeight.Regular:
				num = 4;
				break;
			case TextFontWeight.Medium:
				num = 5;
				break;
			case TextFontWeight.SemiBold:
				num = 6;
				break;
			case TextFontWeight.Bold:
				num = 7;
				break;
			case TextFontWeight.Heavy:
				num = 8;
				break;
			case TextFontWeight.Black:
				num = 9;
				break;
			}
			FontAsset fontAsset = (flag ? fontWeightTable[num].italicTypeface : fontWeightTable[num].regularTypeface);
			if (fontAsset != null)
			{
				if (fontAsset.characterLookupTable.TryGetValue(unicode, out value))
				{
					if (value.textAsset != null)
					{
						isAlternativeTypeface = true;
						return value;
					}
					fontAsset.characterLookupTable.Remove(unicode);
				}
				if ((fontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic || fontAsset.atlasPopulationMode == AtlasPopulationMode.DynamicOS) && fontAsset.TryAddCharacterInternal(unicode, out value))
				{
					isAlternativeTypeface = true;
					return value;
				}
			}
		}
		if (sourceFontAsset.characterLookupTable.TryGetValue(unicode, out value))
		{
			if (value.textAsset != null)
			{
				return value;
			}
			sourceFontAsset.characterLookupTable.Remove(unicode);
		}
		if ((sourceFontAsset.atlasPopulationMode == AtlasPopulationMode.Dynamic || sourceFontAsset.atlasPopulationMode == AtlasPopulationMode.DynamicOS) && sourceFontAsset.TryAddCharacterInternal(unicode, out value))
		{
			return value;
		}
		if (value == null && includeFallbacks && sourceFontAsset.fallbackFontAssetTable != null)
		{
			List<FontAsset> fallbackFontAssetTable = sourceFontAsset.fallbackFontAssetTable;
			int count = fallbackFontAssetTable.Count;
			if (count == 0)
			{
				return null;
			}
			for (int i = 0; i < count; i++)
			{
				FontAsset fontAsset2 = fallbackFontAssetTable[i];
				if (fontAsset2 == null)
				{
					continue;
				}
				int instanceID = fontAsset2.instanceID;
				if (k_SearchedAssets.Add(instanceID))
				{
					value = GetCharacterFromFontAsset_Internal(unicode, fontAsset2, includeFallbacks: true, fontStyle, fontWeight, out isAlternativeTypeface);
					if (value != null)
					{
						return value;
					}
				}
			}
		}
		return null;
	}

	public static Character GetCharacterFromFontAssets(uint unicode, FontAsset sourceFontAsset, List<FontAsset> fontAssets, bool includeFallbacks, FontStyles fontStyle, TextFontWeight fontWeight, out bool isAlternativeTypeface)
	{
		isAlternativeTypeface = false;
		if (fontAssets == null || fontAssets.Count == 0)
		{
			return null;
		}
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
		}
		int count = fontAssets.Count;
		for (int i = 0; i < count; i++)
		{
			FontAsset fontAsset = fontAssets[i];
			if (!(fontAsset == null))
			{
				Character characterFromFontAsset_Internal = GetCharacterFromFontAsset_Internal(unicode, fontAsset, includeFallbacks, fontStyle, fontWeight, out isAlternativeTypeface);
				if (characterFromFontAsset_Internal != null)
				{
					return characterFromFontAsset_Internal;
				}
			}
		}
		return null;
	}

	public static SpriteCharacter GetSpriteCharacterFromSpriteAsset(uint unicode, SpriteAsset spriteAsset, bool includeFallbacks)
	{
		if (spriteAsset == null)
		{
			return null;
		}
		if (spriteAsset.spriteCharacterLookupTable.TryGetValue(unicode, out var value))
		{
			return value;
		}
		if (includeFallbacks)
		{
			if (k_SearchedAssets == null)
			{
				k_SearchedAssets = new HashSet<int>();
			}
			else
			{
				k_SearchedAssets.Clear();
			}
			k_SearchedAssets.Add(spriteAsset.instanceID);
			List<SpriteAsset> fallbackSpriteAssets = spriteAsset.fallbackSpriteAssets;
			if (fallbackSpriteAssets != null && fallbackSpriteAssets.Count > 0)
			{
				int count = fallbackSpriteAssets.Count;
				for (int i = 0; i < count; i++)
				{
					SpriteAsset spriteAsset2 = fallbackSpriteAssets[i];
					if (spriteAsset2 == null)
					{
						continue;
					}
					int instanceID = spriteAsset2.instanceID;
					if (k_SearchedAssets.Add(instanceID))
					{
						value = GetSpriteCharacterFromSpriteAsset_Internal(unicode, spriteAsset2, includeFallbacks: true);
						if (value != null)
						{
							return value;
						}
					}
				}
			}
		}
		return null;
	}

	private static SpriteCharacter GetSpriteCharacterFromSpriteAsset_Internal(uint unicode, SpriteAsset spriteAsset, bool includeFallbacks)
	{
		if (spriteAsset.spriteCharacterLookupTable.TryGetValue(unicode, out var value))
		{
			return value;
		}
		if (includeFallbacks)
		{
			List<SpriteAsset> fallbackSpriteAssets = spriteAsset.fallbackSpriteAssets;
			if (fallbackSpriteAssets != null && fallbackSpriteAssets.Count > 0)
			{
				int count = fallbackSpriteAssets.Count;
				for (int i = 0; i < count; i++)
				{
					SpriteAsset spriteAsset2 = fallbackSpriteAssets[i];
					if (spriteAsset2 == null)
					{
						continue;
					}
					int instanceID = spriteAsset2.instanceID;
					if (k_SearchedAssets.Add(instanceID))
					{
						value = GetSpriteCharacterFromSpriteAsset_Internal(unicode, spriteAsset2, includeFallbacks: true);
						if (value != null)
						{
							return value;
						}
					}
				}
			}
		}
		return null;
	}
}
