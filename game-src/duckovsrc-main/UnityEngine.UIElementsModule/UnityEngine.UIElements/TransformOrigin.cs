using System;
using System.Globalization;

namespace UnityEngine.UIElements;

public struct TransformOrigin : IEquatable<TransformOrigin>
{
	private Length m_X;

	private Length m_Y;

	private float m_Z;

	public Length x
	{
		get
		{
			return m_X;
		}
		set
		{
			m_X = value;
		}
	}

	public Length y
	{
		get
		{
			return m_Y;
		}
		set
		{
			m_Y = value;
		}
	}

	public float z
	{
		get
		{
			return m_Z;
		}
		set
		{
			m_Z = value;
		}
	}

	public TransformOrigin(Length x, Length y, float z)
	{
		m_X = x;
		m_Y = y;
		m_Z = z;
	}

	public TransformOrigin(Length x, Length y)
		: this(x, y, 0f)
	{
	}

	public static TransformOrigin Initial()
	{
		return new TransformOrigin(Length.Percent(50f), Length.Percent(50f), 0f);
	}

	public static bool operator ==(TransformOrigin lhs, TransformOrigin rhs)
	{
		return lhs.m_X == rhs.m_X && lhs.m_Y == rhs.m_Y && lhs.m_Z == rhs.m_Z;
	}

	public static bool operator !=(TransformOrigin lhs, TransformOrigin rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(TransformOrigin other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is TransformOrigin other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (m_X.GetHashCode() * 793) ^ (m_Y.GetHashCode() * 791) ^ (m_Z.GetHashCode() * 571);
	}

	public override string ToString()
	{
		string text = m_Z.ToString(CultureInfo.InvariantCulture.NumberFormat);
		return m_X.ToString() + " " + m_Y.ToString() + " " + text;
	}
}
