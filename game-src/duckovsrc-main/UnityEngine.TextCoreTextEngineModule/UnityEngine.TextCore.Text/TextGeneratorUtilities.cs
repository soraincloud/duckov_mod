using System;
using UnityEngine.TextCore.LowLevel;

namespace UnityEngine.TextCore.Text;

internal static class TextGeneratorUtilities
{
	public static readonly Vector2 largePositiveVector2 = new Vector2(2.1474836E+09f, 2.1474836E+09f);

	public static readonly Vector2 largeNegativeVector2 = new Vector2(-214748370f, -214748370f);

	public const float largePositiveFloat = 32767f;

	public const float largeNegativeFloat = -32767f;

	private const int k_DoubleQuotes = 34;

	private const int k_GreaterThan = 62;

	private const int k_ZeroWidthSpace = 8203;

	private const string k_LookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";

	public static bool Approximately(float a, float b)
	{
		return b - 0.0001f < a && a < b + 0.0001f;
	}

	public static Color32 HexCharsToColor(char[] hexChars, int tagCount)
	{
		switch (tagCount)
		{
		case 4:
		{
			byte r8 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
			byte g8 = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
			byte b8 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
			return new Color32(r8, g8, b8, byte.MaxValue);
		}
		case 5:
		{
			byte r7 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
			byte g7 = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
			byte b7 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
			byte a4 = (byte)(HexToInt(hexChars[4]) * 16 + HexToInt(hexChars[4]));
			return new Color32(r7, g7, b7, a4);
		}
		case 7:
		{
			byte r6 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
			byte g6 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
			byte b6 = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
			return new Color32(r6, g6, b6, byte.MaxValue);
		}
		case 9:
		{
			byte r5 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
			byte g5 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
			byte b5 = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
			byte a3 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
			return new Color32(r5, g5, b5, a3);
		}
		case 10:
		{
			byte r4 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
			byte g4 = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
			byte b4 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
			return new Color32(r4, g4, b4, byte.MaxValue);
		}
		case 11:
		{
			byte r3 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
			byte g3 = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
			byte b3 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
			byte a2 = (byte)(HexToInt(hexChars[10]) * 16 + HexToInt(hexChars[10]));
			return new Color32(r3, g3, b3, a2);
		}
		case 13:
		{
			byte r2 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
			byte g2 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
			byte b2 = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
			return new Color32(r2, g2, b2, byte.MaxValue);
		}
		case 15:
		{
			byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
			byte g = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
			byte b = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
			byte a = (byte)(HexToInt(hexChars[13]) * 16 + HexToInt(hexChars[14]));
			return new Color32(r, g, b, a);
		}
		default:
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
	}

	public static Color32 HexCharsToColor(char[] hexChars, int startIndex, int length)
	{
		switch (length)
		{
		case 7:
		{
			byte r2 = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
			byte g2 = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
			byte b2 = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
			return new Color32(r2, g2, b2, byte.MaxValue);
		}
		case 9:
		{
			byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
			byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
			byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
			byte a = (byte)(HexToInt(hexChars[startIndex + 7]) * 16 + HexToInt(hexChars[startIndex + 8]));
			return new Color32(r, g, b, a);
		}
		default:
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
	}

	public static uint HexToInt(char hex)
	{
		return hex switch
		{
			'0' => 0u, 
			'1' => 1u, 
			'2' => 2u, 
			'3' => 3u, 
			'4' => 4u, 
			'5' => 5u, 
			'6' => 6u, 
			'7' => 7u, 
			'8' => 8u, 
			'9' => 9u, 
			'A' => 10u, 
			'B' => 11u, 
			'C' => 12u, 
			'D' => 13u, 
			'E' => 14u, 
			'F' => 15u, 
			'a' => 10u, 
			'b' => 11u, 
			'c' => 12u, 
			'd' => 13u, 
			'e' => 14u, 
			'f' => 15u, 
			_ => 15u, 
		};
	}

	public static float ConvertToFloat(char[] chars, int startIndex, int length)
	{
		int lastIndex;
		return ConvertToFloat(chars, startIndex, length, out lastIndex);
	}

	public static float ConvertToFloat(char[] chars, int startIndex, int length, out int lastIndex)
	{
		if (startIndex == 0)
		{
			lastIndex = 0;
			return -32767f;
		}
		int num = startIndex + length;
		bool flag = true;
		float num2 = 0f;
		int num3 = 1;
		if (chars[startIndex] == '+')
		{
			num3 = 1;
			startIndex++;
		}
		else if (chars[startIndex] == '-')
		{
			num3 = -1;
			startIndex++;
		}
		float num4 = 0f;
		for (int i = startIndex; i < num; i++)
		{
			uint num5 = chars[i];
			if ((num5 >= 48 && num5 <= 57) || num5 == 46)
			{
				if (num5 == 46)
				{
					flag = false;
					num2 = 0.1f;
				}
				else if (flag)
				{
					num4 = num4 * 10f + (float)((num5 - 48) * num3);
				}
				else
				{
					num4 += (float)(num5 - 48) * num2 * (float)num3;
					num2 *= 0.1f;
				}
			}
			else if (num5 == 44)
			{
				if (i + 1 < num && chars[i + 1] == ' ')
				{
					lastIndex = i + 1;
				}
				else
				{
					lastIndex = i;
				}
				return num4;
			}
		}
		lastIndex = num;
		return num4;
	}

	public static Vector2 PackUV(float x, float y, float scale)
	{
		Vector2 result = default(Vector2);
		result.x = (int)(x * 511f);
		result.y = (int)(y * 511f);
		result.x = result.x * 4096f + result.y;
		result.y = scale;
		return result;
	}

	public static void ResizeInternalArray<T>(ref T[] array)
	{
		int newSize = Mathf.NextPowerOfTwo(array.Length + 1);
		Array.Resize(ref array, newSize);
	}

	public static void ResizeInternalArray<T>(ref T[] array, int size)
	{
		size = Mathf.NextPowerOfTwo(size + 1);
		Array.Resize(ref array, size);
	}

	private static bool IsTagName(ref string text, string tag, int index)
	{
		if (text.Length < index + tag.Length)
		{
			return false;
		}
		for (int i = 0; i < tag.Length; i++)
		{
			if (TextUtilities.ToUpperFast(text[index + i]) != tag[i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsTagName(ref int[] text, string tag, int index)
	{
		if (text.Length < index + tag.Length)
		{
			return false;
		}
		for (int i = 0; i < tag.Length; i++)
		{
			if (TextUtilities.ToUpperFast((char)text[index + i]) != tag[i])
			{
				return false;
			}
		}
		return true;
	}

	internal static void InsertOpeningTextStyle(TextStyle style, ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		if (style != null)
		{
			textStyleStackDepth++;
			textStyleStacks[textStyleStackDepth].Push(style.hashCode);
			uint[] styleOpeningTagArray = style.styleOpeningTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
			textStyleStackDepth--;
		}
	}

	internal static void InsertClosingTextStyle(TextStyle style, ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		if (style != null)
		{
			textStyleStackDepth++;
			textStyleStacks[textStyleStackDepth].Push(style.hashCode);
			uint[] styleClosingTagArray = style.styleClosingTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleClosingTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
			textStyleStackDepth--;
		}
	}

	public static bool ReplaceOpeningStyleTag(ref TextBackingContainer sourceText, int srcIndex, out int srcOffset, ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		int styleHashCode = GetStyleHashCode(ref sourceText, srcIndex + 7, out srcOffset);
		TextStyle style = GetStyle(generationSettings, styleHashCode);
		if (style == null || srcOffset == 0)
		{
			return false;
		}
		textStyleStackDepth++;
		textStyleStacks[textStyleStackDepth].Push(style.hashCode);
		uint[] styleOpeningTagArray = style.styleOpeningTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
		textStyleStackDepth--;
		return true;
	}

	public static void ReplaceOpeningStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		int hashCode = textStyleStacks[textStyleStackDepth + 1].Pop();
		TextStyle style = GetStyle(generationSettings, hashCode);
		if (style != null)
		{
			textStyleStackDepth++;
			uint[] styleOpeningTagArray = style.styleOpeningTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
			textStyleStackDepth--;
		}
	}

	private static bool ReplaceOpeningStyleTag(ref uint[] sourceText, int srcIndex, out int srcOffset, ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		int styleHashCode = GetStyleHashCode(ref sourceText, srcIndex + 7, out srcOffset);
		TextStyle style = GetStyle(generationSettings, styleHashCode);
		if (style == null || srcOffset == 0)
		{
			return false;
		}
		textStyleStackDepth++;
		textStyleStacks[textStyleStackDepth].Push(style.hashCode);
		uint[] styleOpeningTagArray = style.styleOpeningTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
		textStyleStackDepth--;
		return true;
	}

	public static void ReplaceClosingStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		int hashCode = textStyleStacks[textStyleStackDepth + 1].Pop();
		TextStyle style = GetStyle(generationSettings, hashCode);
		if (style != null)
		{
			textStyleStackDepth++;
			uint[] styleClosingTagArray = style.styleClosingTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleClosingTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
			textStyleStackDepth--;
		}
	}

	internal static void InsertOpeningStyleTag(TextStyle style, ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		if (style != null)
		{
			textStyleStacks[0].Push(style.hashCode);
			uint[] styleOpeningTagArray = style.styleOpeningTagArray;
			InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleOpeningTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
			textStyleStackDepth = 0;
		}
	}

	internal static void InsertClosingStyleTag(ref TextProcessingElement[] charBuffer, ref int writeIndex, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		int hashCode = textStyleStacks[0].Pop();
		TextStyle style = GetStyle(generationSettings, hashCode);
		uint[] styleClosingTagArray = style.styleClosingTagArray;
		InsertTextStyleInTextProcessingArray(ref charBuffer, ref writeIndex, styleClosingTagArray, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
		textStyleStackDepth = 0;
	}

	private static void InsertTextStyleInTextProcessingArray(ref TextProcessingElement[] charBuffer, ref int writeIndex, uint[] styleDefinition, ref int textStyleStackDepth, ref TextProcessingStack<int>[] textStyleStacks, ref TextGenerationSettings generationSettings)
	{
		bool flag = generationSettings.tagNoParsing;
		int num = styleDefinition.Length;
		if (writeIndex + num >= charBuffer.Length)
		{
			ResizeInternalArray(ref charBuffer, writeIndex + num);
		}
		for (int i = 0; i < num; i++)
		{
			uint num2 = styleDefinition[i];
			if (num2 == 92 && i + 1 < num)
			{
				switch (styleDefinition[i + 1])
				{
				case 92u:
					i++;
					break;
				case 110u:
					num2 = 10u;
					i++;
					break;
				case 117u:
					if (i + 5 < num)
					{
						num2 = GetUTF16(styleDefinition, i + 2);
						i += 5;
					}
					break;
				case 85u:
					if (i + 9 < num)
					{
						num2 = GetUTF32(styleDefinition, i + 2);
						i += 9;
					}
					break;
				}
			}
			if (num2 == 60)
			{
				switch ((MarkupTag)GetMarkupTagHashCode(styleDefinition, i + 1))
				{
				case MarkupTag.NO_PARSE:
					flag = true;
					break;
				case MarkupTag.SLASH_NO_PARSE:
					flag = false;
					break;
				case MarkupTag.BR:
					if (flag)
					{
						break;
					}
					charBuffer[writeIndex].unicode = 10u;
					writeIndex++;
					i += 3;
					continue;
				case MarkupTag.CR:
					if (flag)
					{
						break;
					}
					charBuffer[writeIndex].unicode = 13u;
					writeIndex++;
					i += 3;
					continue;
				case MarkupTag.NBSP:
					if (flag)
					{
						break;
					}
					charBuffer[writeIndex].unicode = 160u;
					writeIndex++;
					i += 5;
					continue;
				case MarkupTag.ZWSP:
					if (flag)
					{
						break;
					}
					charBuffer[writeIndex].unicode = 8203u;
					writeIndex++;
					i += 5;
					continue;
				case MarkupTag.ZWJ:
					if (flag)
					{
						break;
					}
					charBuffer[writeIndex].unicode = 8205u;
					writeIndex++;
					i += 4;
					continue;
				case MarkupTag.SHY:
					if (flag)
					{
						break;
					}
					charBuffer[writeIndex].unicode = 173u;
					writeIndex++;
					i += 4;
					continue;
				case MarkupTag.STYLE:
				{
					if (flag || !ReplaceOpeningStyleTag(ref styleDefinition, i, out var srcOffset, ref charBuffer, ref writeIndex, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings))
					{
						break;
					}
					i = srcOffset;
					continue;
				}
				case MarkupTag.SLASH_STYLE:
					if (flag)
					{
						break;
					}
					ReplaceClosingStyleTag(ref charBuffer, ref writeIndex, ref textStyleStackDepth, ref textStyleStacks, ref generationSettings);
					i += 7;
					continue;
				}
			}
			charBuffer[writeIndex].unicode = num2;
			writeIndex++;
		}
	}

	public static TextStyle GetStyle(TextGenerationSettings generationSetting, int hashCode)
	{
		TextStyle textStyle = null;
		TextStyleSheet styleSheet = generationSetting.styleSheet;
		if (styleSheet != null)
		{
			textStyle = styleSheet.GetStyle(hashCode);
			if (textStyle != null)
			{
				return textStyle;
			}
		}
		styleSheet = generationSetting.textSettings.defaultStyleSheet;
		if (styleSheet != null)
		{
			textStyle = styleSheet.GetStyle(hashCode);
		}
		return textStyle;
	}

	public static int GetStyleHashCode(ref uint[] text, int index, out int closeIndex)
	{
		int num = 0;
		closeIndex = 0;
		for (int i = index; i < text.Length; i++)
		{
			if (text[i] != 34)
			{
				if (text[i] == 62)
				{
					closeIndex = i;
					break;
				}
				num = ((num << 5) + num) ^ ToUpperASCIIFast((char)text[i]);
			}
		}
		return num;
	}

	public static int GetStyleHashCode(ref TextBackingContainer text, int index, out int closeIndex)
	{
		int num = 0;
		closeIndex = 0;
		for (int i = index; i < text.Capacity; i++)
		{
			if (text[i] != 34)
			{
				if (text[i] == 62)
				{
					closeIndex = i;
					break;
				}
				num = ((num << 5) + num) ^ ToUpperASCIIFast((char)text[i]);
			}
		}
		return num;
	}

	public static uint GetUTF16(uint[] text, int i)
	{
		uint num = 0u;
		num += HexToInt((char)text[i]) << 12;
		num += HexToInt((char)text[i + 1]) << 8;
		num += HexToInt((char)text[i + 2]) << 4;
		return num + HexToInt((char)text[i + 3]);
	}

	public static uint GetUTF16(TextBackingContainer text, int i)
	{
		uint num = 0u;
		num += HexToInt((char)text[i]) << 12;
		num += HexToInt((char)text[i + 1]) << 8;
		num += HexToInt((char)text[i + 2]) << 4;
		return num + HexToInt((char)text[i + 3]);
	}

	public static uint GetUTF32(uint[] text, int i)
	{
		uint num = 0u;
		num += HexToInt((char)text[i]) << 28;
		num += HexToInt((char)text[i + 1]) << 24;
		num += HexToInt((char)text[i + 2]) << 20;
		num += HexToInt((char)text[i + 3]) << 16;
		num += HexToInt((char)text[i + 4]) << 12;
		num += HexToInt((char)text[i + 5]) << 8;
		num += HexToInt((char)text[i + 6]) << 4;
		return num + HexToInt((char)text[i + 7]);
	}

	public static uint GetUTF32(TextBackingContainer text, int i)
	{
		uint num = 0u;
		num += HexToInt((char)text[i]) << 28;
		num += HexToInt((char)text[i + 1]) << 24;
		num += HexToInt((char)text[i + 2]) << 20;
		num += HexToInt((char)text[i + 3]) << 16;
		num += HexToInt((char)text[i + 4]) << 12;
		num += HexToInt((char)text[i + 5]) << 8;
		num += HexToInt((char)text[i + 6]) << 4;
		return num + HexToInt((char)text[i + 7]);
	}

	private static int GetTagHashCode(ref int[] text, int index, out int closeIndex)
	{
		int num = 0;
		closeIndex = 0;
		for (int i = index; i < text.Length; i++)
		{
			if (text[i] != 34)
			{
				if (text[i] == 62)
				{
					closeIndex = i;
					break;
				}
				num = ((num << 5) + num) ^ (int)TextUtilities.ToUpperASCIIFast((ushort)text[i]);
			}
		}
		return num;
	}

	private static int GetTagHashCode(ref string text, int index, out int closeIndex)
	{
		int num = 0;
		closeIndex = 0;
		for (int i = index; i < text.Length; i++)
		{
			if (text[i] != '"')
			{
				if (text[i] == '>')
				{
					closeIndex = i;
					break;
				}
				num = ((num << 5) + num) ^ (int)TextUtilities.ToUpperASCIIFast(text[i]);
			}
		}
		return num;
	}

	public static void FillCharacterVertexBuffers(int i, bool convertToLinearSpace, TextGenerationSettings generationSettings, TextInfo textInfo)
	{
		int materialReferenceIndex = textInfo.textElementInfo[i].materialReferenceIndex;
		int vertexCount = textInfo.meshInfo[materialReferenceIndex].vertexCount;
		if (vertexCount >= textInfo.meshInfo[materialReferenceIndex].vertices.Length)
		{
			textInfo.meshInfo[materialReferenceIndex].ResizeMeshInfo(Mathf.NextPowerOfTwo((vertexCount + 4) / 4));
		}
		TextElementInfo[] textElementInfo = textInfo.textElementInfo;
		textInfo.textElementInfo[i].vertexIndex = vertexCount;
		if (generationSettings.inverseYAxis)
		{
			Vector3 vector = default(Vector3);
			vector.x = 0f;
			vector.y = generationSettings.screenRect.y + generationSettings.screenRect.height;
			vector.z = 0f;
			Vector3 position = textElementInfo[i].vertexBottomLeft.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = position + vector;
			position = textElementInfo[i].vertexTopLeft.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = position + vector;
			position = textElementInfo[i].vertexTopRight.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = position + vector;
			position = textElementInfo[i].vertexBottomRight.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = position + vector;
		}
		else
		{
			textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = textElementInfo[i].vertexBottomLeft.position;
			textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = textElementInfo[i].vertexTopLeft.position;
			textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = textElementInfo[i].vertexTopRight.position;
			textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = textElementInfo[i].vertexBottomRight.position;
		}
		textInfo.meshInfo[materialReferenceIndex].uvs0[vertexCount] = textElementInfo[i].vertexBottomLeft.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs0[1 + vertexCount] = textElementInfo[i].vertexTopLeft.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs0[2 + vertexCount] = textElementInfo[i].vertexTopRight.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs0[3 + vertexCount] = textElementInfo[i].vertexBottomRight.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs2[vertexCount] = textElementInfo[i].vertexBottomLeft.uv2;
		textInfo.meshInfo[materialReferenceIndex].uvs2[1 + vertexCount] = textElementInfo[i].vertexTopLeft.uv2;
		textInfo.meshInfo[materialReferenceIndex].uvs2[2 + vertexCount] = textElementInfo[i].vertexTopRight.uv2;
		textInfo.meshInfo[materialReferenceIndex].uvs2[3 + vertexCount] = textElementInfo[i].vertexBottomRight.uv2;
		textInfo.meshInfo[materialReferenceIndex].colors32[vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexBottomLeft.color) : textElementInfo[i].vertexBottomLeft.color);
		textInfo.meshInfo[materialReferenceIndex].colors32[1 + vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexTopLeft.color) : textElementInfo[i].vertexTopLeft.color);
		textInfo.meshInfo[materialReferenceIndex].colors32[2 + vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexTopRight.color) : textElementInfo[i].vertexTopRight.color);
		textInfo.meshInfo[materialReferenceIndex].colors32[3 + vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexBottomRight.color) : textElementInfo[i].vertexBottomRight.color);
		textInfo.meshInfo[materialReferenceIndex].vertexCount = vertexCount + 4;
	}

	public static void FillSpriteVertexBuffers(int i, bool convertToLinearSpace, TextGenerationSettings generationSettings, TextInfo textInfo)
	{
		int materialReferenceIndex = textInfo.textElementInfo[i].materialReferenceIndex;
		int vertexCount = textInfo.meshInfo[materialReferenceIndex].vertexCount;
		TextElementInfo[] textElementInfo = textInfo.textElementInfo;
		textInfo.textElementInfo[i].vertexIndex = vertexCount;
		if (generationSettings.inverseYAxis)
		{
			Vector3 vector = default(Vector3);
			vector.x = 0f;
			vector.y = generationSettings.screenRect.y + generationSettings.screenRect.height;
			vector.z = 0f;
			Vector3 position = textElementInfo[i].vertexBottomLeft.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = position + vector;
			position = textElementInfo[i].vertexTopLeft.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = position + vector;
			position = textElementInfo[i].vertexTopRight.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = position + vector;
			position = textElementInfo[i].vertexBottomRight.position;
			position.y *= -1f;
			textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = position + vector;
		}
		else
		{
			textInfo.meshInfo[materialReferenceIndex].vertices[vertexCount] = textElementInfo[i].vertexBottomLeft.position;
			textInfo.meshInfo[materialReferenceIndex].vertices[1 + vertexCount] = textElementInfo[i].vertexTopLeft.position;
			textInfo.meshInfo[materialReferenceIndex].vertices[2 + vertexCount] = textElementInfo[i].vertexTopRight.position;
			textInfo.meshInfo[materialReferenceIndex].vertices[3 + vertexCount] = textElementInfo[i].vertexBottomRight.position;
		}
		textInfo.meshInfo[materialReferenceIndex].uvs0[vertexCount] = textElementInfo[i].vertexBottomLeft.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs0[1 + vertexCount] = textElementInfo[i].vertexTopLeft.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs0[2 + vertexCount] = textElementInfo[i].vertexTopRight.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs0[3 + vertexCount] = textElementInfo[i].vertexBottomRight.uv;
		textInfo.meshInfo[materialReferenceIndex].uvs2[vertexCount] = textElementInfo[i].vertexBottomLeft.uv2;
		textInfo.meshInfo[materialReferenceIndex].uvs2[1 + vertexCount] = textElementInfo[i].vertexTopLeft.uv2;
		textInfo.meshInfo[materialReferenceIndex].uvs2[2 + vertexCount] = textElementInfo[i].vertexTopRight.uv2;
		textInfo.meshInfo[materialReferenceIndex].uvs2[3 + vertexCount] = textElementInfo[i].vertexBottomRight.uv2;
		textInfo.meshInfo[materialReferenceIndex].colors32[vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexBottomLeft.color) : textElementInfo[i].vertexBottomLeft.color);
		textInfo.meshInfo[materialReferenceIndex].colors32[1 + vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexTopLeft.color) : textElementInfo[i].vertexTopLeft.color);
		textInfo.meshInfo[materialReferenceIndex].colors32[2 + vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexTopRight.color) : textElementInfo[i].vertexTopRight.color);
		textInfo.meshInfo[materialReferenceIndex].colors32[3 + vertexCount] = (convertToLinearSpace ? GammaToLinear(textElementInfo[i].vertexBottomRight.color) : textElementInfo[i].vertexBottomRight.color);
		textInfo.meshInfo[materialReferenceIndex].vertexCount = vertexCount + 4;
	}

	public static void AdjustLineOffset(int startIndex, int endIndex, float offset, TextInfo textInfo)
	{
		Vector3 vector = new Vector3(0f, offset, 0f);
		for (int i = startIndex; i <= endIndex; i++)
		{
			textInfo.textElementInfo[i].bottomLeft -= vector;
			textInfo.textElementInfo[i].topLeft -= vector;
			textInfo.textElementInfo[i].topRight -= vector;
			textInfo.textElementInfo[i].bottomRight -= vector;
			textInfo.textElementInfo[i].ascender -= vector.y;
			textInfo.textElementInfo[i].baseLine -= vector.y;
			textInfo.textElementInfo[i].descender -= vector.y;
			if (textInfo.textElementInfo[i].isVisible)
			{
				textInfo.textElementInfo[i].vertexBottomLeft.position -= vector;
				textInfo.textElementInfo[i].vertexTopLeft.position -= vector;
				textInfo.textElementInfo[i].vertexTopRight.position -= vector;
				textInfo.textElementInfo[i].vertexBottomRight.position -= vector;
			}
		}
	}

	public static void ResizeLineExtents(int size, TextInfo textInfo)
	{
		size = ((size > 1024) ? (size + 256) : Mathf.NextPowerOfTwo(size + 1));
		LineInfo[] array = new LineInfo[size];
		for (int i = 0; i < size; i++)
		{
			if (i < textInfo.lineInfo.Length)
			{
				array[i] = textInfo.lineInfo[i];
				continue;
			}
			array[i].lineExtents.min = largePositiveVector2;
			array[i].lineExtents.max = largeNegativeVector2;
			array[i].ascender = -32767f;
			array[i].descender = 32767f;
		}
		textInfo.lineInfo = array;
	}

	public static FontStyles LegacyStyleToNewStyle(FontStyle fontStyle)
	{
		return fontStyle switch
		{
			FontStyle.Bold => FontStyles.Bold, 
			FontStyle.Italic => FontStyles.Italic, 
			FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic, 
			_ => FontStyles.Normal, 
		};
	}

	public static TextAlignment LegacyAlignmentToNewAlignment(TextAnchor anchor)
	{
		return anchor switch
		{
			TextAnchor.UpperLeft => TextAlignment.TopLeft, 
			TextAnchor.UpperCenter => TextAlignment.TopCenter, 
			TextAnchor.UpperRight => TextAlignment.TopRight, 
			TextAnchor.MiddleLeft => TextAlignment.MiddleLeft, 
			TextAnchor.MiddleCenter => TextAlignment.MiddleCenter, 
			TextAnchor.MiddleRight => TextAlignment.MiddleRight, 
			TextAnchor.LowerLeft => TextAlignment.BottomLeft, 
			TextAnchor.LowerCenter => TextAlignment.BottomCenter, 
			TextAnchor.LowerRight => TextAlignment.BottomRight, 
			_ => TextAlignment.TopLeft, 
		};
	}

	public static uint ConvertToUTF32(uint highSurrogate, uint lowSurrogate)
	{
		return (highSurrogate - 55296) * 1024 + (lowSurrogate - 56320 + 65536);
	}

	public static int GetMarkupTagHashCode(TextBackingContainer styleDefinition, int readIndex)
	{
		int num = 0;
		int num2 = readIndex + 16;
		int capacity = styleDefinition.Capacity;
		while (readIndex < num2 && readIndex < capacity)
		{
			uint num3 = styleDefinition[readIndex];
			if (num3 == 62 || num3 == 61 || num3 == 32)
			{
				return num;
			}
			num = ((num << 5) + num) ^ (int)ToUpperASCIIFast(num3);
			readIndex++;
		}
		return num;
	}

	public static int GetMarkupTagHashCode(uint[] styleDefinition, int readIndex)
	{
		int num = 0;
		int num2 = readIndex + 16;
		int num3 = styleDefinition.Length;
		while (readIndex < num2 && readIndex < num3)
		{
			uint num4 = styleDefinition[readIndex];
			if (num4 == 62 || num4 == 61 || num4 == 32)
			{
				return num;
			}
			num = ((num << 5) + num) ^ (int)ToUpperASCIIFast(num4);
			readIndex++;
		}
		return num;
	}

	public static char ToUpperASCIIFast(char c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-"[c];
	}

	public static uint ToUpperASCIIFast(uint c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-"[(int)c];
	}

	public static char ToUpperFast(char c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-"[c];
	}

	public static int GetAttributeParameters(char[] chars, int startIndex, int length, ref float[] parameters)
	{
		int lastIndex = startIndex;
		int num = 0;
		while (lastIndex < startIndex + length)
		{
			parameters[num] = ConvertToFloat(chars, startIndex, length, out lastIndex);
			length -= lastIndex - startIndex + 1;
			startIndex = lastIndex + 1;
			num++;
		}
		return num;
	}

	public static bool IsBitmapRendering(GlyphRenderMode glyphRenderMode)
	{
		return glyphRenderMode == GlyphRenderMode.RASTER || glyphRenderMode == GlyphRenderMode.RASTER_HINTED || glyphRenderMode == GlyphRenderMode.SMOOTH || glyphRenderMode == GlyphRenderMode.SMOOTH_HINTED;
	}

	public static bool IsBaseGlyph(uint c)
	{
		return (c < 768 || c > 879) && (c < 6832 || c > 6911) && (c < 7616 || c > 7679) && (c < 8400 || c > 8447) && (c < 65056 || c > 65071) && c != 3633 && (c < 3636 || c > 3642) && (c < 3655 || c > 3662) && (c < 1425 || c > 1469) && c != 1471 && (c < 1473 || c > 1474) && (c < 1476 || c > 1477) && c != 1479 && (c < 1552 || c > 1562) && (c < 1611 || c > 1631) && c != 1648 && (c < 1750 || c > 1756) && (c < 1759 || c > 1764) && (c < 1767 || c > 1768) && (c < 1770 || c > 1773) && (c < 2259 || c > 2273) && (c < 2275 || c > 2303) && (c < 64434 || c > 64449);
	}

	public static Color MinAlpha(this Color c1, Color c2)
	{
		float a = ((c1.a < c2.a) ? c1.a : c2.a);
		return new Color(c1.r, c1.g, c1.b, a);
	}

	internal static Color32 GammaToLinear(Color32 c)
	{
		return new Color32(GammaToLinear(c.r), GammaToLinear(c.g), GammaToLinear(c.b), c.a);
	}

	private static byte GammaToLinear(byte value)
	{
		float num = (float)(int)value / 255f;
		if (num <= 0.04045f)
		{
			return (byte)(num / 12.92f * 255f);
		}
		if (num < 1f)
		{
			return (byte)(Mathf.Pow((num + 0.055f) / 1.055f, 2.4f) * 255f);
		}
		if (num == 1f)
		{
			return byte.MaxValue;
		}
		return (byte)(Mathf.Pow(num, 2.2f) * 255f);
	}

	public static bool IsValidUTF16(TextBackingContainer text, int index)
	{
		for (int i = 0; i < 4; i++)
		{
			uint num = text[index + i];
			if ((num < 48 || num > 57) && (num < 97 || num > 102) && (num < 65 || num > 70))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsValidUTF32(TextBackingContainer text, int index)
	{
		for (int i = 0; i < 8; i++)
		{
			uint num = text[index + i];
			if ((num < 48 || num > 57) && (num < 97 || num > 102) && (num < 65 || num > 70))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsEmoji(uint c)
	{
		return c == 8205 || c == 8252 || c == 8265 || c == 8419 || c == 8482 || c == 8505 || (c >= 8596 && c <= 8601) || (c >= 8617 && c <= 8618) || (c >= 8986 && c <= 8987) || c == 9000 || c == 9096 || c == 9167 || (c >= 9193 && c <= 9203) || (c >= 9208 && c <= 9210) || c == 9410 || (c >= 9642 && c <= 9643) || c == 9654 || c == 9664 || (c >= 9723 && c <= 9726) || (c >= 9728 && c <= 9733) || (c >= 9735 && c <= 9746) || (c >= 9748 && c <= 9861) || (c >= 9872 && c <= 9989) || (c >= 9992 && c <= 10002) || c == 10004 || c == 10006 || c == 10013 || c == 10017 || c == 10024 || (c >= 10035 && c <= 10036) || c == 10052 || c == 10055 || c == 10060 || c == 10062 || (c >= 10067 && c <= 10069) || c == 10071 || (c >= 10083 && c <= 10087) || (c >= 10133 && c <= 10135) || c == 10145 || c == 10160 || c == 10175 || (c >= 10548 && c <= 10549) || (c >= 11013 && c <= 11015) || (c >= 11035 && c <= 11036) || c == 11088 || c == 11093 || c == 12336 || c == 12349 || c == 12951 || c == 12953 || c == 65039 || (c >= 126976 && c <= 127231) || (c >= 127245 && c <= 127247) || c == 127279 || (c >= 127340 && c <= 127345) || (c >= 127358 && c <= 127359) || c == 127374 || (c >= 127377 && c <= 127386) || (c >= 127405 && c <= 127487) || (c >= 127489 && c <= 127503) || c == 127514 || c == 127535 || (c >= 127538 && c <= 127546) || (c >= 127548 && c <= 127551) || (c >= 127561 && c <= 128317) || (c >= 128326 && c <= 128591) || (c >= 128640 && c <= 128767) || (c >= 128884 && c <= 128895) || (c >= 128981 && c <= 129023) || (c >= 129036 && c <= 129039) || (c >= 129096 && c <= 129103) || (c >= 129114 && c <= 129119) || (c >= 129160 && c <= 129167) || (c >= 129198 && c <= 129279) || (c >= 129292 && c <= 129338) || (c >= 129340 && c <= 129349) || (c >= 129351 && c <= 129791) || (c >= 130048 && c <= 131069) || (c >= 917536 && c <= 917631);
	}

	internal static bool IsHangul(uint c)
	{
		return (c >= 4352 && c <= 4607) || (c >= 43360 && c <= 43391) || (c >= 55216 && c <= 55295) || (c >= 12592 && c <= 12687) || (c >= 65440 && c <= 65500) || (c >= 44032 && c <= 55215);
	}

	internal static bool IsCJK(uint c)
	{
		return (c >= 12288 && c <= 12351) || (c >= 94176 && c <= 5887) || (c >= 12544 && c <= 12591) || (c >= 12704 && c <= 12735) || (c >= 19968 && c <= 40959) || (c >= 13312 && c <= 19903) || (c >= 131072 && c <= 173791) || (c >= 173824 && c <= 177983) || (c >= 177984 && c <= 178207) || (c >= 178208 && c <= 183983) || (c >= 183984 && c <= 191456) || (c >= 196608 && c <= 201546) || (c >= 63744 && c <= 64255) || (c >= 194560 && c <= 195103) || (c >= 12032 && c <= 12255) || (c >= 11904 && c <= 12031) || (c >= 12736 && c <= 12783) || (c >= 12272 && c <= 12287) || (c >= 12352 && c <= 12447) || (c >= 110848 && c <= 110895) || (c >= 110576 && c <= 110591) || (c >= 110592 && c <= 110847) || (c >= 110896 && c <= 110959) || (c >= 12688 && c <= 12703) || (c >= 12448 && c <= 12543) || (c >= 12784 && c <= 12799) || (c >= 65381 && c <= 65439);
	}
}
