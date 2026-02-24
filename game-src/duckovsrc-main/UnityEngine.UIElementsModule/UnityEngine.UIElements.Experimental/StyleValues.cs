using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements.Experimental;

public struct StyleValues
{
	internal StyleValueCollection m_StyleValues;

	public float top
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Top).value;
		}
		set
		{
			SetValue(StylePropertyId.Top, value);
		}
	}

	public float left
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Left).value;
		}
		set
		{
			SetValue(StylePropertyId.Left, value);
		}
	}

	public float width
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Width).value;
		}
		set
		{
			SetValue(StylePropertyId.Width, value);
		}
	}

	public float height
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Height).value;
		}
		set
		{
			SetValue(StylePropertyId.Height, value);
		}
	}

	public float right
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Right).value;
		}
		set
		{
			SetValue(StylePropertyId.Right, value);
		}
	}

	public float bottom
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Bottom).value;
		}
		set
		{
			SetValue(StylePropertyId.Bottom, value);
		}
	}

	public Color color
	{
		get
		{
			return Values().GetStyleColor(StylePropertyId.Color).value;
		}
		set
		{
			SetValue(StylePropertyId.Color, value);
		}
	}

	public Color backgroundColor
	{
		get
		{
			return Values().GetStyleColor(StylePropertyId.BackgroundColor).value;
		}
		set
		{
			SetValue(StylePropertyId.BackgroundColor, value);
		}
	}

	public Color unityBackgroundImageTintColor
	{
		get
		{
			return Values().GetStyleColor(StylePropertyId.UnityBackgroundImageTintColor).value;
		}
		set
		{
			SetValue(StylePropertyId.UnityBackgroundImageTintColor, value);
		}
	}

	public Color borderColor
	{
		get
		{
			return Values().GetStyleColor(StylePropertyId.BorderColor).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderColor, value);
		}
	}

	public float marginLeft
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.MarginLeft).value;
		}
		set
		{
			SetValue(StylePropertyId.MarginLeft, value);
		}
	}

	public float marginTop
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.MarginTop).value;
		}
		set
		{
			SetValue(StylePropertyId.MarginTop, value);
		}
	}

	public float marginRight
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.MarginRight).value;
		}
		set
		{
			SetValue(StylePropertyId.MarginRight, value);
		}
	}

	public float marginBottom
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.MarginBottom).value;
		}
		set
		{
			SetValue(StylePropertyId.MarginBottom, value);
		}
	}

	public float paddingLeft
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.PaddingLeft).value;
		}
		set
		{
			SetValue(StylePropertyId.PaddingLeft, value);
		}
	}

	public float paddingTop
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.PaddingTop).value;
		}
		set
		{
			SetValue(StylePropertyId.PaddingTop, value);
		}
	}

	public float paddingRight
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.PaddingRight).value;
		}
		set
		{
			SetValue(StylePropertyId.PaddingRight, value);
		}
	}

	public float paddingBottom
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.PaddingBottom).value;
		}
		set
		{
			SetValue(StylePropertyId.PaddingBottom, value);
		}
	}

	public float borderLeftWidth
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderLeftWidth).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderLeftWidth, value);
		}
	}

	public float borderRightWidth
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderRightWidth).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderRightWidth, value);
		}
	}

	public float borderTopWidth
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderTopWidth).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderTopWidth, value);
		}
	}

	public float borderBottomWidth
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderBottomWidth).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderBottomWidth, value);
		}
	}

	public float borderTopLeftRadius
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderTopLeftRadius).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderTopLeftRadius, value);
		}
	}

	public float borderTopRightRadius
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderTopRightRadius).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderTopRightRadius, value);
		}
	}

	public float borderBottomLeftRadius
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderBottomLeftRadius).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderBottomLeftRadius, value);
		}
	}

	public float borderBottomRightRadius
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.BorderBottomRightRadius).value;
		}
		set
		{
			SetValue(StylePropertyId.BorderBottomRightRadius, value);
		}
	}

	public float opacity
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.Opacity).value;
		}
		set
		{
			SetValue(StylePropertyId.Opacity, value);
		}
	}

	public float flexGrow
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.FlexGrow).value;
		}
		set
		{
			SetValue(StylePropertyId.FlexGrow, value);
		}
	}

	public float flexShrink
	{
		get
		{
			return Values().GetStyleFloat(StylePropertyId.FlexShrink).value;
		}
		set
		{
			SetValue(StylePropertyId.FlexGrow, value);
		}
	}

	internal void SetValue(StylePropertyId id, float value)
	{
		StyleValue styleValue = new StyleValue
		{
			id = id,
			number = value
		};
		Values().SetStyleValue(styleValue);
	}

	internal void SetValue(StylePropertyId id, Color value)
	{
		StyleValue styleValue = new StyleValue
		{
			id = id,
			color = value
		};
		Values().SetStyleValue(styleValue);
	}

	internal StyleValueCollection Values()
	{
		if (m_StyleValues == null)
		{
			m_StyleValues = new StyleValueCollection();
		}
		return m_StyleValues;
	}
}
