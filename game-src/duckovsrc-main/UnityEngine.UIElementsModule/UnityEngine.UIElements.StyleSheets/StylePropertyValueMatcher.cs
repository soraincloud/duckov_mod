using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UIElements.StyleSheets.Syntax;

namespace UnityEngine.UIElements.StyleSheets;

internal class StylePropertyValueMatcher : BaseStyleMatcher
{
	private List<StylePropertyValue> m_Values;

	private StylePropertyValue current => base.hasCurrent ? m_Values[base.currentIndex] : default(StylePropertyValue);

	public override int valueCount => m_Values.Count;

	public override bool isCurrentVariable => false;

	public override bool isCurrentComma => base.hasCurrent && m_Values[base.currentIndex].handle.valueType == StyleValueType.CommaSeparator;

	public MatchResult Match(Expression exp, List<StylePropertyValue> values)
	{
		MatchResult result = new MatchResult
		{
			errorCode = MatchResultErrorCode.None
		};
		if (values == null || values.Count == 0)
		{
			result.errorCode = MatchResultErrorCode.EmptyValue;
			return result;
		}
		Initialize();
		m_Values = values;
		bool flag = false;
		StyleValueHandle handle = m_Values[0].handle;
		if (handle.valueType == StyleValueType.Keyword && handle.valueIndex == 1)
		{
			MoveNext();
			flag = true;
		}
		else
		{
			flag = Match(exp);
		}
		if (!flag)
		{
			StyleSheet sheet = current.sheet;
			result.errorCode = MatchResultErrorCode.Syntax;
			result.errorValue = sheet.ReadAsString(current.handle);
		}
		else if (base.hasCurrent)
		{
			StyleSheet sheet2 = current.sheet;
			result.errorCode = MatchResultErrorCode.ExpectedEndOfValue;
			result.errorValue = sheet2.ReadAsString(current.handle);
		}
		return result;
	}

	protected override bool MatchKeyword(string keyword)
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Keyword)
		{
			StyleValueKeyword valueIndex = (StyleValueKeyword)stylePropertyValue.handle.valueIndex;
			return valueIndex.ToUssString() == keyword.ToLowerInvariant();
		}
		if (stylePropertyValue.handle.valueType == StyleValueType.Enum)
		{
			string text = stylePropertyValue.sheet.ReadEnum(stylePropertyValue.handle);
			return text == keyword.ToLowerInvariant();
		}
		return false;
	}

	protected override bool MatchNumber()
	{
		return current.handle.valueType == StyleValueType.Float;
	}

	protected override bool MatchInteger()
	{
		return current.handle.valueType == StyleValueType.Float;
	}

	protected override bool MatchLength()
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Dimension)
		{
			return stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle).unit == Dimension.Unit.Pixel;
		}
		if (stylePropertyValue.handle.valueType == StyleValueType.Float)
		{
			float b = stylePropertyValue.sheet.ReadFloat(stylePropertyValue.handle);
			return Mathf.Approximately(0f, b);
		}
		return false;
	}

	protected override bool MatchPercentage()
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Dimension)
		{
			return stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle).unit == Dimension.Unit.Percent;
		}
		if (stylePropertyValue.handle.valueType == StyleValueType.Float)
		{
			float b = stylePropertyValue.sheet.ReadFloat(stylePropertyValue.handle);
			return Mathf.Approximately(0f, b);
		}
		return false;
	}

	protected override bool MatchColor()
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Color)
		{
			return true;
		}
		if (stylePropertyValue.handle.valueType == StyleValueType.Enum)
		{
			Color color = Color.clear;
			string text = stylePropertyValue.sheet.ReadAsString(stylePropertyValue.handle);
			if (StyleSheetColor.TryGetColor(text.ToLowerInvariant(), out color))
			{
				return true;
			}
		}
		return false;
	}

	protected override bool MatchResource()
	{
		return current.handle.valueType == StyleValueType.ResourcePath;
	}

	protected override bool MatchUrl()
	{
		StyleValueType valueType = current.handle.valueType;
		return valueType == StyleValueType.AssetReference || valueType == StyleValueType.ScalableImage;
	}

	protected override bool MatchTime()
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Dimension)
		{
			Dimension dimension = stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle);
			return dimension.unit == Dimension.Unit.Second || dimension.unit == Dimension.Unit.Millisecond;
		}
		return false;
	}

	protected override bool MatchCustomIdent()
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Enum)
		{
			string text = stylePropertyValue.sheet.ReadAsString(stylePropertyValue.handle);
			Match match = BaseStyleMatcher.s_CustomIdentRegex.Match(text);
			return match.Success && match.Length == text.Length;
		}
		return false;
	}

	protected override bool MatchAngle()
	{
		StylePropertyValue stylePropertyValue = current;
		if (stylePropertyValue.handle.valueType == StyleValueType.Dimension)
		{
			Dimension.Unit unit = stylePropertyValue.sheet.ReadDimension(stylePropertyValue.handle).unit;
			Dimension.Unit unit2 = unit;
			if ((uint)(unit2 - 5) <= 3u)
			{
				return true;
			}
		}
		if (stylePropertyValue.handle.valueType == StyleValueType.Float)
		{
			float b = stylePropertyValue.sheet.ReadFloat(stylePropertyValue.handle);
			return Mathf.Approximately(0f, b);
		}
		return false;
	}
}
