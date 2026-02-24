using System;

namespace UnityEngine.UIElements;

public struct StyleBackgroundPosition : IStyleValue<BackgroundPosition>, IEquatable<StyleBackgroundPosition>
{
	private BackgroundPosition m_Value;

	private StyleKeyword m_Keyword;

	public BackgroundPosition value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(BackgroundPosition);
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

	public StyleBackgroundPosition(BackgroundPosition v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackgroundPosition(StyleKeyword keyword)
		: this(default(BackgroundPosition), keyword)
	{
	}

	internal StyleBackgroundPosition(BackgroundPosition v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleBackgroundPosition lhs, StyleBackgroundPosition rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleBackgroundPosition lhs, StyleBackgroundPosition rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleBackgroundPosition(StyleKeyword keyword)
	{
		return new StyleBackgroundPosition(keyword);
	}

	public static implicit operator StyleBackgroundPosition(BackgroundPosition v)
	{
		return new StyleBackgroundPosition(v);
	}

	public bool Equals(StyleBackgroundPosition other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleBackgroundPosition other && Equals(other);
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
