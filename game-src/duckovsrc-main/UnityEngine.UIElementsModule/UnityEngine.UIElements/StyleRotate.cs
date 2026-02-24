using System;

namespace UnityEngine.UIElements;

public struct StyleRotate : IStyleValue<Rotate>, IEquatable<StyleRotate>
{
	private Rotate m_Value;

	private StyleKeyword m_Keyword;

	public Rotate value
	{
		get
		{
			StyleKeyword styleKeyword = m_Keyword;
			if (1 == 0)
			{
			}
			Rotate result = styleKeyword switch
			{
				StyleKeyword.Undefined => m_Value, 
				StyleKeyword.Null => Rotate.None(), 
				StyleKeyword.None => Rotate.None(), 
				StyleKeyword.Initial => Rotate.Initial(), 
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

	public StyleRotate(Rotate v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleRotate(StyleKeyword keyword)
		: this(default(Rotate), keyword)
	{
	}

	internal StyleRotate(Rotate v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleRotate lhs, StyleRotate rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleRotate lhs, StyleRotate rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleRotate(StyleKeyword keyword)
	{
		return new StyleRotate(keyword);
	}

	public static implicit operator StyleRotate(Rotate v)
	{
		return new StyleRotate(v);
	}

	public bool Equals(StyleRotate other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleRotate other && Equals(other);
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
