using System;
using System.Globalization;

namespace UnityEngine.UIElements;

public struct TimeValue : IEquatable<TimeValue>
{
	private float m_Value;

	private TimeUnit m_Unit;

	public float value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public TimeUnit unit
	{
		get
		{
			return m_Unit;
		}
		set
		{
			m_Unit = value;
		}
	}

	public TimeValue(float value)
		: this(value, TimeUnit.Second)
	{
	}

	public TimeValue(float value, TimeUnit unit)
	{
		m_Value = value;
		m_Unit = unit;
	}

	public static implicit operator TimeValue(float value)
	{
		return new TimeValue(value, TimeUnit.Second);
	}

	public static bool operator ==(TimeValue lhs, TimeValue rhs)
	{
		return lhs.m_Value == rhs.m_Value && lhs.m_Unit == rhs.m_Unit;
	}

	public static bool operator !=(TimeValue lhs, TimeValue rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(TimeValue other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is TimeValue other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (m_Value.GetHashCode() * 397) ^ (int)m_Unit;
	}

	public override string ToString()
	{
		string text = value.ToString(CultureInfo.InvariantCulture.NumberFormat);
		string text2 = string.Empty;
		switch (unit)
		{
		case TimeUnit.Second:
			text2 = "s";
			break;
		case TimeUnit.Millisecond:
			text2 = "ms";
			break;
		}
		return text + text2;
	}
}
