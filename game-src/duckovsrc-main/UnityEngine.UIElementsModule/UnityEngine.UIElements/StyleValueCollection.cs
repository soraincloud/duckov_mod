using System.Collections.Generic;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

internal class StyleValueCollection
{
	internal List<StyleValue> m_Values = new List<StyleValue>();

	public StyleLength GetStyleLength(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			return new StyleLength(value.length, value.keyword);
		}
		return StyleKeyword.Null;
	}

	public StyleFloat GetStyleFloat(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			return new StyleFloat(value.number, value.keyword);
		}
		return StyleKeyword.Null;
	}

	public StyleInt GetStyleInt(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			return new StyleInt((int)value.number, value.keyword);
		}
		return StyleKeyword.Null;
	}

	public StyleColor GetStyleColor(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			return new StyleColor(value.color, value.keyword);
		}
		return StyleKeyword.Null;
	}

	public StyleBackground GetStyleBackground(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			Texture2D texture2D = (value.resource.IsAllocated ? (value.resource.Target as Texture2D) : null);
			if (texture2D != null)
			{
				return new StyleBackground(texture2D, value.keyword);
			}
			Sprite sprite = (value.resource.IsAllocated ? (value.resource.Target as Sprite) : null);
			if (sprite != null)
			{
				return new StyleBackground(sprite, value.keyword);
			}
			VectorImage vectorImage = (value.resource.IsAllocated ? (value.resource.Target as VectorImage) : null);
			if (vectorImage != null)
			{
				return new StyleBackground(vectorImage, value.keyword);
			}
		}
		return StyleKeyword.Null;
	}

	public StyleBackgroundPosition GetStyleBackgroundPosition(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			return new StyleBackgroundPosition(value.position);
		}
		return StyleKeyword.Null;
	}

	public StyleBackgroundRepeat GetStyleBackgroundRepeat(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			return new StyleBackgroundRepeat(value.repeat);
		}
		return StyleKeyword.Null;
	}

	public StyleFont GetStyleFont(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			Font v = (value.resource.IsAllocated ? (value.resource.Target as Font) : null);
			return new StyleFont(v, value.keyword);
		}
		return StyleKeyword.Null;
	}

	public StyleFontDefinition GetStyleFontDefinition(StylePropertyId id)
	{
		StyleValue value = default(StyleValue);
		if (TryGetStyleValue(id, ref value))
		{
			object obj = (value.resource.IsAllocated ? value.resource.Target : null);
			return new StyleFontDefinition(obj, value.keyword);
		}
		return StyleKeyword.Null;
	}

	public bool TryGetStyleValue(StylePropertyId id, ref StyleValue value)
	{
		value.id = StylePropertyId.Unknown;
		foreach (StyleValue value2 in m_Values)
		{
			if (value2.id == id)
			{
				value = value2;
				return true;
			}
		}
		return false;
	}

	public void SetStyleValue(StyleValue value)
	{
		for (int i = 0; i < m_Values.Count; i++)
		{
			if (m_Values[i].id == value.id)
			{
				if (value.keyword == StyleKeyword.Null)
				{
					m_Values.RemoveAt(i);
				}
				else
				{
					m_Values[i] = value;
				}
				return;
			}
		}
		m_Values.Add(value);
	}
}
