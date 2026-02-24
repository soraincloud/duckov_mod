using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

public struct StyleList<T> : IStyleValue<List<T>>, IEquatable<StyleList<T>>
{
	private StyleKeyword m_Keyword;

	private List<T> m_Value;

	public List<T> value
	{
		get
		{
			return (m_Keyword == StyleKeyword.Undefined) ? m_Value : null;
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

	public StyleList(List<T> v)
		: this(v, StyleKeyword.Undefined)
	{
	}

	public StyleList(StyleKeyword keyword)
		: this(null, keyword)
	{
	}

	internal StyleList(List<T> v, StyleKeyword keyword)
	{
		m_Keyword = keyword;
		m_Value = v;
	}

	public static bool operator ==(StyleList<T> lhs, StyleList<T> rhs)
	{
		if (lhs.m_Keyword != rhs.m_Keyword)
		{
			return false;
		}
		List<T> list = lhs.m_Value;
		List<T> list2 = rhs.m_Value;
		if (list == list2)
		{
			return true;
		}
		if (list == null || list2 == null)
		{
			return false;
		}
		return list.Count == list2.Count && list.SequenceEqual(list2);
	}

	public static bool operator !=(StyleList<T> lhs, StyleList<T> rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator StyleList<T>(StyleKeyword keyword)
	{
		return new StyleList<T>(keyword);
	}

	public static implicit operator StyleList<T>(List<T> v)
	{
		return new StyleList<T>(v);
	}

	public bool Equals(StyleList<T> other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is StyleList<T> other && Equals(other);
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (m_Value != null && m_Value.Count > 0)
		{
			num = EqualityComparer<T>.Default.GetHashCode(m_Value[0]);
			for (int i = 1; i < m_Value.Count; i++)
			{
				num = (num * 397) ^ EqualityComparer<T>.Default.GetHashCode(m_Value[i]);
			}
		}
		return (num * 397) ^ (int)m_Keyword;
	}

	public override string ToString()
	{
		return this.DebugString();
	}
}
