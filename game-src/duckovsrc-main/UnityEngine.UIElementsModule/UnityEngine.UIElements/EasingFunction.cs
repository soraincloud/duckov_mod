using System;

namespace UnityEngine.UIElements;

public struct EasingFunction : IEquatable<EasingFunction>
{
	private EasingMode m_Mode;

	public EasingMode mode
	{
		get
		{
			return m_Mode;
		}
		set
		{
			m_Mode = value;
		}
	}

	public EasingFunction(EasingMode mode)
	{
		m_Mode = mode;
	}

	public static implicit operator EasingFunction(EasingMode easingMode)
	{
		return new EasingFunction(easingMode);
	}

	public static bool operator ==(EasingFunction lhs, EasingFunction rhs)
	{
		return lhs.m_Mode == rhs.m_Mode;
	}

	public static bool operator !=(EasingFunction lhs, EasingFunction rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(EasingFunction other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is EasingFunction other && Equals(other);
	}

	public override string ToString()
	{
		return m_Mode.ToString();
	}

	public override int GetHashCode()
	{
		return (int)m_Mode;
	}
}
