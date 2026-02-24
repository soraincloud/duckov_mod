using System;
using System.Collections.Generic;

namespace UnityEngine.TextCore.Text;

internal static class TextUtilities
{
	private const string k_LookupStringL = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";

	private const string k_LookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";

	internal static void ResizeArray<T>(ref T[] array)
	{
		int newSize = NextPowerOfTwo(array.Length);
		Array.Resize(ref array, newSize);
	}

	internal static void ResizeArray<T>(ref T[] array, int size)
	{
		size = NextPowerOfTwo(size);
		Array.Resize(ref array, size);
	}

	internal static int NextPowerOfTwo(int v)
	{
		v |= v >> 16;
		v |= v >> 8;
		v |= v >> 4;
		v |= v >> 2;
		v |= v >> 1;
		return v + 1;
	}

	internal static char ToLowerFast(char c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-"[c];
	}

	internal static char ToUpperFast(char c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-"[c];
	}

	internal static uint ToUpperASCIIFast(uint c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-"[(int)c];
	}

	internal static uint ToLowerASCIIFast(uint c)
	{
		if (c > "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-".Length - 1)
		{
			return c;
		}
		return "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-"[(int)c];
	}

	public static int GetHashCodeCaseSensitive(string s)
	{
		int num = 0;
		for (int i = 0; i < s.Length; i++)
		{
			num = ((num << 5) + num) ^ s[i];
		}
		return num;
	}

	public static int GetHashCodeCaseInSensitive(string s)
	{
		int num = 0;
		for (int i = 0; i < s.Length; i++)
		{
			num = ((num << 5) + num) ^ ToUpperFast(s[i]);
		}
		return num;
	}

	public static uint GetSimpleHashCodeLowercase(string s)
	{
		uint num = 0u;
		for (int i = 0; i < s.Length; i++)
		{
			num = ((num << 5) + num) ^ ToLowerFast(s[i]);
		}
		return num;
	}

	internal static uint ConvertToUTF32(uint highSurrogate, uint lowSurrogate)
	{
		return (highSurrogate - 55296) * 1024 + (lowSurrogate - 56320 + 65536);
	}

	internal static uint ReadUTF16(uint[] text, int index)
	{
		uint num = 0u;
		num += HexToInt((char)text[index]) << 12;
		num += HexToInt((char)text[index + 1]) << 8;
		num += HexToInt((char)text[index + 2]) << 4;
		return num + HexToInt((char)text[index + 3]);
	}

	internal static uint ReadUTF32(uint[] text, int index)
	{
		uint num = 0u;
		num += HexToInt((char)text[index]) << 30;
		num += HexToInt((char)text[index + 1]) << 24;
		num += HexToInt((char)text[index + 2]) << 20;
		num += HexToInt((char)text[index + 3]) << 16;
		num += HexToInt((char)text[index + 4]) << 12;
		num += HexToInt((char)text[index + 5]) << 8;
		num += HexToInt((char)text[index + 6]) << 4;
		return num + HexToInt((char)text[index + 7]);
	}

	private static uint HexToInt(char hex)
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

	public static uint StringHexToInt(string s)
	{
		uint num = 0u;
		int length = s.Length;
		for (int i = 0; i < length; i++)
		{
			num += HexToInt(s[i]) * (uint)Mathf.Pow(16f, length - 1 - i);
		}
		return num;
	}

	internal static string UintToString(this List<uint> unicodes)
	{
		char[] array = new char[unicodes.Count];
		for (int i = 0; i < unicodes.Count; i++)
		{
			array[i] = (char)unicodes[i];
		}
		return new string(array);
	}
}
