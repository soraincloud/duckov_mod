using System;

namespace UnityEngine.UIElements;

public struct Rotate : IEquatable<Rotate>
{
	private Angle m_Angle;

	private Vector3 m_Axis;

	private bool m_IsNone;

	public Angle angle
	{
		get
		{
			return m_Angle;
		}
		set
		{
			m_Angle = value;
		}
	}

	internal Vector3 axis
	{
		get
		{
			return m_Axis;
		}
		set
		{
			m_Axis = value;
		}
	}

	internal Rotate(Angle angle, Vector3 axis)
	{
		m_Angle = angle;
		m_Axis = axis;
		m_IsNone = false;
	}

	public Rotate(Angle angle)
	{
		m_Angle = angle;
		m_Axis = Vector3.forward;
		m_IsNone = false;
	}

	internal static Rotate Initial()
	{
		return new Rotate(0f);
	}

	public static Rotate None()
	{
		Rotate result = Initial();
		result.m_IsNone = true;
		return result;
	}

	internal bool IsNone()
	{
		return m_IsNone;
	}

	public static bool operator ==(Rotate lhs, Rotate rhs)
	{
		return lhs.m_Angle == rhs.m_Angle && lhs.m_Axis == rhs.m_Axis && lhs.m_IsNone == rhs.m_IsNone;
	}

	public static bool operator !=(Rotate lhs, Rotate rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(Rotate other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		return obj is Rotate other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (m_Angle.GetHashCode() * 793) ^ (m_Axis.GetHashCode() * 791) ^ (m_IsNone.GetHashCode() * 197);
	}

	public override string ToString()
	{
		return m_Angle.ToString() + " " + m_Axis.ToString();
	}

	internal Quaternion ToQuaternion()
	{
		return Quaternion.AngleAxis(m_Angle.ToDegrees(), m_Axis);
	}
}
