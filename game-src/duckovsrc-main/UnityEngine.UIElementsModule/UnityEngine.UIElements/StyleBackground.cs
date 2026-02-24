using System;

namespace UnityEngine.UIElements;

public struct StyleBackground : IStyleValue<Background>, IEquatable<StyleBackground>
{
	private Background m_Value;

	private StyleKeyword m_Keyword;

	public Background value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(Background);
		}
		set
		{
			m_Value = value;
			m_Keyword = StyleKeyword.Undefined;
		}
	}

	public StyleKeyword keyword
	{
		get
		{
			return m_Keyword;
		}
		set
		{
			m_Keyword = value;
		}
	}

	public StyleBackground(Background v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(Texture2D v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(Sprite v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(VectorImage v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackground(StyleKeyword keyword)
		: this(default(Background), keyword)
	{
	}

	internal StyleBackground(Texture2D v, StyleKeyword keyword)
		: this(Background.FromTexture2D(v), keyword)
	{
	}

	internal StyleBackground(Sprite v, StyleKeyword keyword)
		: this(Background.FromSprite(v), keyword)
	{
	}

	internal StyleBackground(VectorImage v, StyleKeyword keyword)
		: this(Background.FromVectorImage(v), keyword)
	{
	}

	internal StyleBackground(Background v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleBackground lhs, StyleBackground rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleBackground lhs, StyleBackground rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleBackground(StyleKeyword keyword)
	{
		return new StyleBackground(keyword);
	}

	public static implicit operator StyleBackground(Background v)
	{
		return new StyleBackground(v);
	}

	public static implicit operator StyleBackground(Texture2D v)
	{
		return new StyleBackground(v);
	}

	public bool Equals(StyleBackground other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleBackground other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (m_Value.GetHashCode() * 397) ^ (int)m_Keyword;
	}

	public override string ToString()
	{
		return this.DebugString();
	}
}
