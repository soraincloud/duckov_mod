using System;

namespace UnityEngine.UIElements;

public struct StyleLength : IStyleValue<Length>, IEquatable<StyleLength>
{
	private Length m_Value;

	private StyleKeyword m_Keyword;

	public Length value
	{
		get
		{
			if (m_Keyword == StyleKeyword.Auto || m_Keyword == StyleKeyword.None || m_Keyword == StyleKeyword.Undefined)
			{
				return m_Value;
			}
			return default(Length);
		}
		set
		{
			if (value.IsAuto())
			{
				m_Keyword = StyleKeyword.Auto;
			}
			else if (value.IsNone())
			{
				m_Keyword = StyleKeyword.None;
			}
			else
			{
				m_Keyword = StyleKeyword.Undefined;
			}
			m_Value = value;
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
			if (m_Keyword == StyleKeyword.Auto)
			{
				m_Value = Length.Auto();
			}
			else if (m_Keyword == StyleKeyword.None)
			{
				m_Value = Length.None();
			}
			else if (m_Keyword != StyleKeyword.Undefined)
			{
				m_Value = default(Length);
			}
		}
	}

	public StyleLength(float v)
		: this(new Length(v, LengthUnit.Pixel), StyleKeyword.Undefined)
	{
	}

	public StyleLength(Length v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleLength(StyleKeyword keyword)
		: this(default(Length), keyword)
	{
	}

	internal StyleLength(Length v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
		if (v.IsAuto())
		{
			m_Keyword = StyleKeyword.Auto;
		}
		else if (v.IsNone())
		{
			m_Keyword = StyleKeyword.None;
		}
	}

	public static bool operator ==(StyleLength lhs, StyleLength rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleLength lhs, StyleLength rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleLength(StyleKeyword keyword)
	{
		return new StyleLength(keyword);
	}

	public static implicit operator StyleLength(float v)
	{
		return new StyleLength(v);
	}

	public static implicit operator StyleLength(Length v)
	{
		return new StyleLength(v);
	}

	public bool Equals(StyleLength other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleLength other && Equals(other);
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
