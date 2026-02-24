using System;

namespace UnityEngine.UIElements;

public struct StyleTextShadow : IStyleValue<TextShadow>, IEquatable<StyleTextShadow>
{
	private StyleKeyword m_Keyword;

	private TextShadow m_Value;

	public TextShadow value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : default(TextShadow);
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

	public StyleTextShadow(TextShadow v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleTextShadow(StyleKeyword keyword)
		: this(default(TextShadow), keyword)
	{
	}

	internal StyleTextShadow(TextShadow v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleTextShadow lhs, StyleTextShadow rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleTextShadow lhs, StyleTextShadow rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleTextShadow(StyleKeyword keyword)
	{
		return new StyleTextShadow(keyword);
	}

	public static implicit operator StyleTextShadow(TextShadow v)
	{
		return new StyleTextShadow(v);
	}

	public bool Equals(StyleTextShadow other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StyleTextShadow styleTextShadow))
		{
			return false;
		}
		return styleTextShadow == this;
	}

	public override int GetHashCode()
	{
		int num = 917506989;
		num = num * -1521134295 + m_Keyword.GetHashCode();
		return num * -1521134295 + m_Value.GetHashCode();
	}

	public override string ToString()
	{
		return this.DebugString();
	}
}
