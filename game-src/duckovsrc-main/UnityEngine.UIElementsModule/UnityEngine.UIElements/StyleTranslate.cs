using System;

namespace UnityEngine.UIElements;

public struct StyleTranslate : IStyleValue<Translate>, IEquatable<StyleTranslate>
{
	private Translate m_Value;

	private StyleKeyword m_Keyword;

	public Translate value
	{
		get
		{
			StyleKeyword styleKeyword = m_Keyword;
			if (1 == 0)
			{
			}
			Translate result = styleKeyword switch
			{
				StyleKeyword.Undefined => m_Value, 
				StyleKeyword.Null => Translate.None(), 
				StyleKeyword.None => Translate.None(), 
				StyleKeyword.Initial => Translate.None(), 
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

	public StyleTranslate(Translate v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleTranslate(StyleKeyword keyword)
		: this(default(Translate), keyword)
	{
	}

	internal StyleTranslate(Translate v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleTranslate lhs, StyleTranslate rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleTranslate lhs, StyleTranslate rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleTranslate(StyleKeyword keyword)
	{
		return new StyleTranslate(keyword);
	}

	public static implicit operator StyleTranslate(Translate v)
	{
		return new StyleTranslate(v);
	}

	public bool Equals(StyleTranslate other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleTranslate other && Equals(other);
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
