using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.TextCore.LowLevel;

[Serializable]
[UsedByNativeCode]
internal struct GlyphAnchorPoint
{
	[SerializeField]
	[NativeName("xPositionAdjustment")]
	private float m_XCoordinate;

	[NativeName("yPositionAdjustment")]
	[SerializeField]
	private float m_YCoordinate;

	public float xCoordinate
	{
		get
		{
			return m_XCoordinate;
		}
		set
		{
			m_XCoordinate = value;
		}
	}

	public float yCoordinate
	{
		get
		{
			return m_YCoordinate;
		}
		set
		{
			m_YCoordinate = value;
		}
	}
}
