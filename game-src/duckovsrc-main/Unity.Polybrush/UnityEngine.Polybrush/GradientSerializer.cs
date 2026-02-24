using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Polybrush;

internal static class GradientSerializer
{
	internal static string Serialize(this Gradient gradient)
	{
		StringBuilder stringBuilder = new StringBuilder();
		GradientColorKey[] colorKeys = gradient.colorKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			GradientColorKey gradientColorKey = colorKeys[i];
			Color color = gradientColorKey.color;
			stringBuilder.Append(color.ToString("F8"));
			stringBuilder.Append("&");
			float time = gradientColorKey.time;
			stringBuilder.Append(time.ToString("F8"));
			stringBuilder.Append("|");
		}
		stringBuilder.Append("\n");
		GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
		for (int i = 0; i < alphaKeys.Length; i++)
		{
			GradientAlphaKey gradientAlphaKey = alphaKeys[i];
			float time = gradientAlphaKey.alpha;
			stringBuilder.Append(time.ToString("F8"));
			stringBuilder.Append("&");
			time = gradientAlphaKey.time;
			stringBuilder.Append(time.ToString("F8"));
			stringBuilder.Append("|");
		}
		return stringBuilder.ToString();
	}

	internal static Gradient Deserialize(string str)
	{
		Gradient gradient = new Gradient();
		Deserialize(str, out gradient);
		return gradient;
	}

	internal static bool Deserialize(string str, out Gradient gradient)
	{
		gradient = null;
		string[] array = str.Split('\n');
		if (array.Length < 2)
		{
			return false;
		}
		string[] array2 = array[0].Split('|');
		string[] array3 = array[1].Split('|');
		if (array2.Length < 2 || array3.Length < 2)
		{
			return false;
		}
		List<GradientColorKey> list = new List<GradientColorKey>();
		List<GradientAlphaKey> list2 = new List<GradientAlphaKey>();
		string[] array4 = array2;
		for (int i = 0; i < array4.Length; i++)
		{
			string[] array5 = array4[i].Split('&');
			if (array5.Length >= 2 && TryParseColor(array5[0], out var value) && float.TryParse(array5[1], out var result))
			{
				list.Add(new GradientColorKey(value, result));
			}
		}
		array4 = array3;
		for (int i = 0; i < array4.Length; i++)
		{
			string[] array6 = array4[i].Split('&');
			if (array6.Length >= 2 && float.TryParse(array6[0], out var result2) && float.TryParse(array6[1], out var result3))
			{
				list2.Add(new GradientAlphaKey(result2, result3));
			}
		}
		gradient = new Gradient();
		gradient.SetKeys(list.ToArray(), list2.ToArray());
		return true;
	}

	private static bool TryParseColor(string str, out Color value)
	{
		string[] array = str.Replace("RGBA(", "").Replace(")", "").Split(',');
		value = Color.white;
		if (array.Length != 4)
		{
			return false;
		}
		float result = 1f;
		if (!float.TryParse(array[0], out value.r))
		{
			return false;
		}
		if (!float.TryParse(array[1], out value.g))
		{
			return false;
		}
		if (!float.TryParse(array[2], out value.b))
		{
			return false;
		}
		if (!float.TryParse(array[3], out result))
		{
			return false;
		}
		value.a = result / 255f;
		return true;
	}

	internal static bool CompareContentWith(this Gradient original, Gradient compareWith)
	{
		if (original.alphaKeys.Length != compareWith.alphaKeys.Length)
		{
			return false;
		}
		if (original.colorKeys.Length != compareWith.colorKeys.Length)
		{
			return false;
		}
		for (int i = 0; i < original.alphaKeys.Length; i++)
		{
			if (!original.alphaKeys[i].CompareContentWith(compareWith.alphaKeys[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < original.colorKeys.Length; j++)
		{
			if (!original.colorKeys[j].CompareContentWith(compareWith.colorKeys[j]))
			{
				return false;
			}
		}
		return true;
	}

	internal static bool CompareContentWith(this GradientAlphaKey original, GradientAlphaKey compareWith)
	{
		if (original.alpha != compareWith.alpha)
		{
			return false;
		}
		if (original.time != compareWith.time)
		{
			return false;
		}
		return true;
	}

	internal static bool CompareContentWith(this GradientColorKey original, GradientColorKey compareWith)
	{
		if (original.color != compareWith.color)
		{
			return false;
		}
		if (original.time != compareWith.time)
		{
			return false;
		}
		return true;
	}
}
