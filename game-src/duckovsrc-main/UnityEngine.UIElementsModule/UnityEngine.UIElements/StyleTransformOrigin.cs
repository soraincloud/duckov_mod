using System;

namespace UnityEngine.UIElements;

public struct StyleTransformOrigin : IStyleValue<TransformOrigin>, IEquatable<StyleTransformOrigin>
{
	private TransformOrigin m_Value;

	private StyleKeyword m_Keyword;

	public TransformOrigin value
	{
		get
		{
			StyleKeyword styleKeyword = m_Keyword;
			if (1 == 0)
			{
			}
			TransformOrigin result = styleKeyword switch
			{
				StyleKeyword.Undefined => m_Value, 
				StyleKeyword.Null => TransformOrigin.Initial(), 
				StyleKeyword.None => TransformOrigin.Initial(), 
				StyleKeyword.Initial => TransformOrigin.Initial(), 
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

	public StyleTransformOrigin(TransformOrigin v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleTransformOrigin(StyleKeyword keyword)
		: this(default(TransformOrigin), keyword)
	{
	}

	internal StyleTransformOrigin(TransformOrigin v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleTransformOrigin lhs, StyleTransformOrigin rhs)
	{
		return lhs.m_Keyword == rhs.m_Keyword && lhs.m_Value == rhs.m_Value;
	}

	public static bool operator !=(StyleTransformOrigin lhs, StyleTransformOrigin rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleTransformOrigin(StyleKeyword keyword)
	{
		return new StyleTransformOrigin(keyword);
	}

	public static implicit operator StyleTransformOrigin(TransformOrigin v)
	{
		return new StyleTransformOrigin(v);
	}

	public bool Equals(StyleTransformOrigin other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleTransformOrigin other && Equals(other);
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
