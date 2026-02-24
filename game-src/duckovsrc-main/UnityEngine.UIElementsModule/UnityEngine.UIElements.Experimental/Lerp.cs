using System;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements.Experimental;

internal static class Lerp
{
	public static float Interpolate(float start, float end, float ratio)
	{
		return Mathf.LerpUnclamped(start, end, ratio);
	}

	public static int Interpolate(int start, int end, float ratio)
	{
		return Mathf.RoundToInt(Mathf.LerpUnclamped(start, end, ratio));
	}

	public static Rect Interpolate(Rect r1, Rect r2, float ratio)
	{
		return new Rect(Mathf.LerpUnclamped(r1.x, r2.x, ratio), Mathf.LerpUnclamped(r1.y, r2.y, ratio), Mathf.LerpUnclamped(r1.width, r2.width, ratio), Mathf.LerpUnclamped(r1.height, r2.height, ratio));
	}

	public static Color Interpolate(Color start, Color end, float ratio)
	{
		return Color.LerpUnclamped(start, end, ratio);
	}

	public static Vector2 Interpolate(Vector2 start, Vector2 end, float ratio)
	{
		return Vector2.LerpUnclamped(start, end, ratio);
	}

	public static Vector3 Interpolate(Vector3 start, Vector3 end, float ratio)
	{
		return Vector3.LerpUnclamped(start, end, ratio);
	}

	public static Quaternion Interpolate(Quaternion start, Quaternion end, float ratio)
	{
		return Quaternion.SlerpUnclamped(start, end, ratio);
	}

	internal static StyleValues Interpolate(StyleValues start, StyleValues end, float ratio)
	{
		StyleValues result = default(StyleValues);
		if (end.m_StyleValues != null)
		{
			foreach (StyleValue value2 in end.m_StyleValues.m_Values)
			{
				StyleValue value = default(StyleValue);
				if (!start.m_StyleValues.TryGetStyleValue(value2.id, ref value))
				{
					throw new ArgumentException("Start StyleValues must contain the same values as end values. Missing property:" + value2.id);
				}
				switch (value2.id)
				{
				case StylePropertyId.FontSize:
				case StylePropertyId.BorderBottomWidth:
				case StylePropertyId.BorderLeftWidth:
				case StylePropertyId.BorderRightWidth:
				case StylePropertyId.BorderTopWidth:
				case StylePropertyId.Bottom:
				case StylePropertyId.FlexBasis:
				case StylePropertyId.FlexGrow:
				case StylePropertyId.FlexShrink:
				case StylePropertyId.Height:
				case StylePropertyId.Left:
				case StylePropertyId.MarginBottom:
				case StylePropertyId.MarginLeft:
				case StylePropertyId.MarginRight:
				case StylePropertyId.MarginTop:
				case StylePropertyId.MaxHeight:
				case StylePropertyId.MaxWidth:
				case StylePropertyId.MinHeight:
				case StylePropertyId.MinWidth:
				case StylePropertyId.PaddingBottom:
				case StylePropertyId.PaddingLeft:
				case StylePropertyId.PaddingRight:
				case StylePropertyId.PaddingTop:
				case StylePropertyId.Right:
				case StylePropertyId.Top:
				case StylePropertyId.Width:
				case StylePropertyId.BorderBottomLeftRadius:
				case StylePropertyId.BorderBottomRightRadius:
				case StylePropertyId.BorderTopLeftRadius:
				case StylePropertyId.BorderTopRightRadius:
				case StylePropertyId.Opacity:
					result.SetValue(value2.id, Interpolate(value.number, value2.number, ratio));
					break;
				case StylePropertyId.Color:
				case StylePropertyId.UnityBackgroundImageTintColor:
				case StylePropertyId.BorderColor:
				case StylePropertyId.BackgroundColor:
					result.SetValue(value2.id, Interpolate(value.color, value2.color, ratio));
					break;
				default:
					throw new ArgumentException("Style Value can't be animated");
				}
			}
		}
		return result;
	}
}
