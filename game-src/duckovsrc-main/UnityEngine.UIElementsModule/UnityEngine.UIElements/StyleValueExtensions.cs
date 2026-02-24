#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine.Yoga;

namespace UnityEngine.UIElements;

internal static class StyleValueExtensions
{
	internal static string DebugString<T>(this IStyleValue<T> styleValue)
	{
		return (styleValue.keyword != StyleKeyword.Undefined) ? $"{styleValue.keyword}" : $"{styleValue.value}";
	}

	internal static YogaValue ToYogaValue(this Length length)
	{
		if (length.IsAuto())
		{
			return YogaValue.Auto();
		}
		if (length.IsNone())
		{
			return float.NaN;
		}
		switch (length.unit)
		{
		case LengthUnit.Pixel:
			return YogaValue.Point(length.value);
		case LengthUnit.Percent:
			return YogaValue.Percent(length.value);
		default:
			Debug.LogAssertion($"Unexpected unit '{length.unit}'");
			return float.NaN;
		}
	}

	internal static Length ToLength(this StyleKeyword keyword)
	{
		switch (keyword)
		{
		case StyleKeyword.Auto:
			return Length.Auto();
		case StyleKeyword.None:
			return Length.None();
		default:
			Debug.LogAssertion("Unexpected StyleKeyword '" + keyword.ToString() + "'");
			return default(Length);
		}
	}

	internal static Rotate ToRotate(this StyleKeyword keyword)
	{
		StyleKeyword styleKeyword = keyword;
		StyleKeyword styleKeyword2 = styleKeyword;
		if (styleKeyword2 == StyleKeyword.None)
		{
			return Rotate.None();
		}
		Debug.LogAssertion("Unexpected StyleKeyword '" + keyword.ToString() + "'");
		return default(Rotate);
	}

	internal static Scale ToScale(this StyleKeyword keyword)
	{
		StyleKeyword styleKeyword = keyword;
		StyleKeyword styleKeyword2 = styleKeyword;
		if (styleKeyword2 == StyleKeyword.None)
		{
			return Scale.None();
		}
		Debug.LogAssertion("Unexpected StyleKeyword '" + keyword.ToString() + "'");
		return default(Scale);
	}

	internal static Translate ToTranslate(this StyleKeyword keyword)
	{
		StyleKeyword styleKeyword = keyword;
		StyleKeyword styleKeyword2 = styleKeyword;
		if (styleKeyword2 == StyleKeyword.None)
		{
			return Translate.None();
		}
		Debug.LogAssertion("Unexpected StyleKeyword '" + keyword.ToString() + "'");
		return default(Translate);
	}

	internal static Length ToLength(this StyleLength styleLength)
	{
		StyleKeyword keyword = styleLength.keyword;
		StyleKeyword styleKeyword = keyword;
		if ((uint)(styleKeyword - 2) <= 1u)
		{
			return styleLength.keyword.ToLength();
		}
		return styleLength.value;
	}

	internal static void CopyFrom<T>(this List<T> list, List<T> other)
	{
		list.Clear();
		list.AddRange(other);
	}
}
