using System;

namespace UnityEngine.UIElements;

public struct StyleBackgroundRepeat : IStyleValue<BackgroundRepeat>, IEquatable<StyleBackgroundRepeat>
{
	private BackgroundRepeat m_Value;

	private StyleKeyword m_Keyword;

	public BackgroundRepeat value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(BackgroundRepeat);
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

	public StyleBackgroundRepeat(BackgroundRepeat v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleBackgroundRepeat(StyleKeyword keyword)
		: this(default(BackgroundRepeat), keyword)
	{
	}

	internal StyleBackgroundRepeat(BackgroundRepeat v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleBackgroundRepeat lhs, StyleBackgroundRepeat rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleBackgroundRepeat lhs, StyleBackgroundRepeat rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleBackgroundRepeat(StyleKeyword keyword)
	{
		return new StyleBackgroundRepeat(keyword);
	}

	public static implicit operator StyleBackgroundRepeat(BackgroundRepeat v)
	{
		return new StyleBackgroundRepeat(v);
	}

	public bool Equals(StyleBackgroundRepeat other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleBackgroundRepeat other && Equals(other);
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
