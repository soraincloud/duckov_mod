using System;

namespace UnityEngine.UIElements;

public struct StyleScale : IStyleValue<Scale>, IEquatable<StyleScale>
{
	private Scale m_Value;

	private StyleKeyword m_Keyword;

	public Scale value
	{
		get
		{
			StyleKeyword styleKeyword = m_Keyword;
			if (1 == 0)
			{
			}
			Scale result = styleKeyword switch
			{
				StyleKeyword.Undefined => m_Value, 
				StyleKeyword.Null => Scale.None(), 
				StyleKeyword.None => Scale.None(), 
				StyleKeyword.Initial => Scale.Initial(), 
				_ => throw new NotImplementedException(), 
			};
			if (1 == 0)
			{
			}
			return result;
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

	public StyleScale(Scale v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleScale(StyleKeyword keyword)
		: this(default(Scale), keyword)
	{
	}

	public StyleScale(Vector2 scale)
		: this(new Scale(scale))
	{
	}

	internal StyleScale(Scale v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static implicit operator StyleScale(Vector2 scale)
	{
		return new Scale(scale);
	}

	public static bool operator ==(StyleScale lhs, StyleScale rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleScale lhs, StyleScale rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleScale(StyleKeyword keyword)
	{
		return new StyleScale(keyword);
	}

	public static implicit operator StyleScale(Scale v)
	{
		return new StyleScale(v);
	}

	public bool Equals(StyleScale other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleScale other && Equals(other);
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
