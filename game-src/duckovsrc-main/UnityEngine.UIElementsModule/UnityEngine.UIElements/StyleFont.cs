using System;

namespace UnityEngine.UIElements;

public struct StyleFont : IStyleValue<Font>, IEquatable<StyleFont>
{
	private Font m_Value;

	private StyleKeyword m_Keyword;

	public Font value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : null;
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

	public StyleFont(Font v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleFont(StyleKeyword keyword)
		: this(null, keyword)
	{
	}

	internal StyleFont(Font v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleFont lhs, StyleFont rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleFont lhs, StyleFont rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleFont(StyleKeyword keyword)
	{
		return new StyleFont(keyword);
	}

	public static implicit operator StyleFont(Font v)
	{
		return new StyleFont(v);
	}

	public bool Equals(StyleFont other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleFont other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (((m_Value != null) ? m_Value.GetHashCode() : 0) * 397) ^ (int)m_Keyword;
	}

	public override string ToString()
	{
		return this.DebugString();
	}
}
