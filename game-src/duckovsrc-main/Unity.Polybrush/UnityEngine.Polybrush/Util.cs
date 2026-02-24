using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UnityEngine.Polybrush;

internal static class Util
{
	internal static T[] Fill<T>(T value, int count)
	{
		if (count < 0)
		{
			return null;
		}
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = value;
		}
		return array;
	}

	internal static T[] Fill<T>(Func<int, T> constructor, int count)
	{
		if (count < 0)
		{
			return null;
		}
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = constructor(i);
		}
		return array;
	}

	internal static T[] Duplicate<T>(T[] array)
	{
		if (array == null)
		{
			return null;
		}
		T[] array2 = new T[array.Length];
		Array.Copy(array, 0, array2, 0, array.Length);
		return array2;
	}

	internal static string ToString<T>(this IEnumerable<T> enumerable, string delim)
	{
		if (enumerable == null)
		{
			return "";
		}
		return string.Join(delim ?? "", enumerable.Select((T x) => (x == null) ? "" : x.ToString()).ToArray());
	}

	public static AnimationCurve ClampAnimationKeys(AnimationCurve curve, float firstKeyTime, float firstKeyValue, float secondKeyTime, float secondKeyValue)
	{
		Keyframe[] keys = curve.keys;
		int num = curve.length - 1;
		keys[0].time = firstKeyTime;
		keys[0].value = firstKeyValue;
		keys[num].time = secondKeyTime;
		keys[num].value = secondKeyValue;
		curve.keys = keys;
		return new AnimationCurve(keys);
	}

	internal static Dictionary<T, int> GetCommonLookup<T>(this List<List<T>> lists)
	{
		Dictionary<T, int> dictionary = new Dictionary<T, int>();
		int num = 0;
		foreach (List<T> list in lists)
		{
			if (list == null)
			{
				continue;
			}
			foreach (T item in list)
			{
				if (dictionary.ContainsKey(item))
				{
					Debug.LogWarning("Error, duplicated values as keys");
					return null;
				}
				dictionary.Add(item, num);
			}
			num++;
		}
		return dictionary;
	}

	internal static Dictionary<T, int> GetCommonLookup<T>(this T[][] lists)
	{
		Dictionary<T, int> dictionary = new Dictionary<T, int>();
		int num = 0;
		foreach (T[] array in lists)
		{
			if (array == null)
			{
				continue;
			}
			T[] array2 = array;
			foreach (T key in array2)
			{
				if (dictionary.ContainsKey(key))
				{
					Debug.LogWarning("Error, duplicated values as keys");
					return null;
				}
				dictionary.Add(key, num);
			}
			num++;
		}
		return dictionary;
	}

	internal static Color Lerp(Color lhs, Color rhs, ColorMask mask, float alpha)
	{
		return new Color(mask.r ? (lhs.r * (1f - alpha) + rhs.r * alpha) : lhs.r, mask.g ? (lhs.g * (1f - alpha) + rhs.g * alpha) : lhs.g, mask.b ? (lhs.b * (1f - alpha) + rhs.b * alpha) : lhs.b, mask.a ? (lhs.a * (1f - alpha) + rhs.a * alpha) : lhs.a);
	}

	internal static Color32 Lerp(Color32 lhs, Color32 rhs, float alpha)
	{
		return new Color32((byte)((float)(int)lhs.r * (1f - alpha) + (float)(int)rhs.r * alpha), (byte)((float)(int)lhs.g * (1f - alpha) + (float)(int)rhs.g * alpha), (byte)((float)(int)lhs.b * (1f - alpha) + (float)(int)rhs.b * alpha), (byte)((float)(int)lhs.a * (1f - alpha) + (float)(int)rhs.a * alpha));
	}

	internal static bool IsValid<T>(this T target) where T : IValid
	{
		return target?.IsValid ?? false;
	}

	internal static string IncrementPrefix(string prefix, string name)
	{
		string text = name;
		Match match = new Regex("^(" + prefix + "[0-9]*_)").Match(name);
		if (match.Success)
		{
			string s = match.Value.Replace(prefix, "").Replace("_", "");
			int result = 0;
			if (int.TryParse(s, out result))
			{
				return name.Replace(match.Value, prefix + (result + 1) + "_");
			}
			return prefix + "0_" + name;
		}
		return prefix + "0_" + name;
	}

	internal static List<Material> GetMaterials(this GameObject gameObject)
	{
		if (gameObject == null)
		{
			return null;
		}
		List<Material> list = new List<Material>();
		Renderer[] components = gameObject.GetComponents<Renderer>();
		foreach (Renderer renderer in components)
		{
			list.AddRange(renderer.sharedMaterials);
		}
		return list;
	}

	internal static Mesh GetMesh(this GameObject go)
	{
		if (go == null)
		{
			return null;
		}
		MeshFilter component = go.GetComponent<MeshFilter>();
		SkinnedMeshRenderer component2 = go.GetComponent<SkinnedMeshRenderer>();
		PolybrushMesh component3 = go.GetComponent<PolybrushMesh>();
		MeshRenderer component4 = go.GetComponent<MeshRenderer>();
		if (component3 != null && component3.storedMesh != null)
		{
			return component3.storedMesh;
		}
		if (component4 != null && component4.additionalVertexStreams != null)
		{
			return component4.additionalVertexStreams;
		}
		if (component != null && component.sharedMesh != null)
		{
			return component.sharedMesh;
		}
		if (component2 != null && component2.sharedMesh != null)
		{
			return component2.sharedMesh;
		}
		return null;
	}
}
