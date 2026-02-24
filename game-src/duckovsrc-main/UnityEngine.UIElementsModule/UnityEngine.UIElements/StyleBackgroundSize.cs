using System;

namespace UnityEngine.UIElements;

public struct StyleBackgroundSize : IStyleValue<BackgroundSize>, IEquatable<StyleBackgroundSize>
{
	private BackgroundSize m_Value;

	private StyleKeyword m_Keyword;

	public BackgroundSize value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(BackgroundSize);
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

	public StyleBackgroundSize(BackgroundSize v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackgroundSize(StyleKeyword keyword)
		: this(default(BackgroundSize), keyword)
	{
	}

	internal StyleBackgroundSize(BackgroundSize v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleBackgroundSize lhs, StyleBackgroundSize rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleBackgroundSize lhs, StyleBackgroundSize rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleBackgroundSize(StyleKeyword keyword)
	{
		return new StyleBackgroundSize(keyword);
	}

	public static implicit operator StyleBackgroundSize(BackgroundSize v)
	{
		return new StyleBackgroundSize(v);
	}

	public bool Equals(StyleBackgroundSize other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleBackgroundSize other && Equals(other);
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
