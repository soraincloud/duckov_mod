using System;

namespace UnityEngine;

public struct JointDrive
{
	private float m_PositionSpring;

	private float m_PositionDamper;

	private float m_MaximumForce;

	private int m_UseAcceleration;

	public float positionSpring
	{
		get
		{
			return m_PositionSpring;
		}
		set
		{
			m_PositionSpring = value;
		}
	}

	public float positionDamper
	{
		get
		{
			return m_PositionDamper;
		}
		set
		{
			m_PositionDamper = value;
		}
	}

	public float maximumForce
	{
		get
		{
			return m_MaximumForce;
		}
		set
		{
			m_MaximumForce = value;
		}
	}

	public bool useAcceleration
	{
		get
		{
			return m_UseAcceleration == 1;
		}
		set
		{
			m_UseAcceleration = (value ? 1 : 0);
		}
	}

	[Obsolete("JointDriveMode is obsolete")]
	public JointDriveMode mode
	{
		get
		{
			return JointDriveMode.None;
		}
		set
		{
		}
	}
}
