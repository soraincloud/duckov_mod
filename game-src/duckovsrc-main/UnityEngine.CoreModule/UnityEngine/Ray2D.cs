using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace UnityEngine;

public struct Ray2D : IFormattable
{
	private Vector2 m_Origin;

	private Vector2 m_Direction;

	public Vector2 origin
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Origin;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Origin = value;
		}
	}

	public Vector2 direction
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Direction;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Direction = value.normalized;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Ray2D(Vector2 origin, Vector2 direction)
	{
		m_Origin = origin;
		m_Direction = direction.normalized;
	}

	public Vector2 GetPoint(float distance)
	{
		return m_Origin + m_Direction * distance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return ToString(null, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format)
	{
		return ToString(format, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (string.IsNullOrEmpty(format))
		{
			format = "F2";
		}
		if (formatProvider == null)
		{
			formatProvider = CultureInfo.InvariantCulture.NumberFormat;
		}
		return UnityString.Format("Origin: {0}, Dir: {1}", m_Origin.ToString(format, formatProvider), m_Direction.ToString(format, formatProvider));
	}
}
