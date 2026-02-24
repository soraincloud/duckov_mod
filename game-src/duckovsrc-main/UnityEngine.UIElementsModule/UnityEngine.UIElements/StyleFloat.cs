using System;

namespace UnityEngine.UIElements;

public struct StyleFloat : IStyleValue<float>, IEquatable<StyleFloat>
{
	private float m_Value;

	private StyleKeyword m_Keyword;

	public float value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : 0f;
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

	public StyleFloat(float v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleFloat(StyleKeyword keyword)
		: this(0f, keyword)
	{
	}

	internal StyleFloat(float v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleFloat lhs, StyleFloat rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleFloat lhs, StyleFloat rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleFloat(StyleKeyword keyword)
	{
		return new StyleFloat(keyword);
	}

	public static implicit operator StyleFloat(float v)
	{
		return new StyleFloat(v);
	}

	public bool Equals(StyleFloat other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleFloat other && Equals(other);
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
