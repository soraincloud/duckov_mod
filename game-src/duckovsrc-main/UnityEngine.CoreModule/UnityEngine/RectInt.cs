using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
public struct RectInt : IEquatable<RectInt>, IFormattable
{
	public struct PositionEnumerator : IEnumerator<Vector2Int>, IEnumerator, IDisposable
	{
		private readonly Vector2Int _min;

		private readonly Vector2Int _max;

		private Vector2Int _current;

		public Vector2Int Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _current;
			}
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return Current;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PositionEnumerator(Vector2Int min, Vector2Int max)
		{
			_min = (_current = min);
			_max = max;
			Reset();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PositionEnumerator GetEnumerator()
		{
			return this;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_current.y >= _max.y)
			{
				return false;
			}
			_current.x++;
			if (_current.x >= _max.x)
			{
				_current.x = _min.x;
				if (_current.x >= _max.x)
				{
					return false;
				}
				_current.y++;
				if (_current.y >= _max.y)
				{
					return false;
				}
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_current = _min;
			_current.x--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void IDisposable.Dispose()
		{
		}
	}

	private int m_XMin;

	private int m_YMin;

	private int m_Width;

	private int m_Height;

	public int x
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_XMin;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_XMin = value;
		}
	}

	public int y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_YMin;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_YMin = value;
		}
	}

	public Vector2 center
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Vector2((float)x + (float)m_Width / 2f, (float)y + (float)m_Height / 2f);
		}
	}

	public Vector2Int min
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Vector2Int(xMin, yMin);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			xMin = value.x;
			yMin = value.y;
		}
	}

	public Vector2Int max
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Vector2Int(xMax, yMax);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			xMax = value.x;
			yMax = value.y;
		}
	}

	public int width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Width;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Width = value;
		}
	}

	public int height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Height;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Height = value;
		}
	}

	public int xMin
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Math.Min(m_XMin, m_XMin + m_Width);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			int num = xMax;
			m_XMin = value;
			m_Width = num - m_XMin;
		}
	}

	public int yMin
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Math.Min(m_YMin, m_YMin + m_Height);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			int num = yMax;
			m_YMin = value;
			m_Height = num - m_YMin;
		}
	}

	public int xMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Math.Max(m_XMin, m_XMin + m_Width);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Width = value - m_XMin;
		}
	}

	public int yMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Math.Max(m_YMin, m_YMin + m_Height);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Height = value - m_YMin;
		}
	}

	public Vector2Int position
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Vector2Int(m_XMin, m_YMin);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_XMin = value.x;
			m_YMin = value.y;
		}
	}

	public Vector2Int size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Vector2Int(m_Width, m_Height);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Width = value.x;
			m_Height = value.y;
		}
	}

	public PositionEnumerator allPositionsWithin
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new PositionEnumerator(min, max);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetMinMax(Vector2Int minPosition, Vector2Int maxPosition)
	{
		min = minPosition;
		max = maxPosition;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RectInt(int xMin, int yMin, int width, int height)
	{
		m_XMin = xMin;
		m_YMin = yMin;
		m_Width = width;
		m_Height = height;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RectInt(Vector2Int position, Vector2Int size)
	{
		m_XMin = position.x;
		m_YMin = position.y;
		m_Width = size.x;
		m_Height = size.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ClampToBounds(RectInt bounds)
	{
		position = new Vector2Int(Math.Max(Math.Min(bounds.xMax, position.x), bounds.xMin), Math.Max(Math.Min(bounds.yMax, position.y), bounds.yMin));
		size = new Vector2Int(Math.Min(bounds.xMax - position.x, size.x), Math.Min(bounds.yMax - position.y, size.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(Vector2Int position)
	{
		return position.x >= xMin && position.y >= yMin && position.x < xMax && position.y < yMax;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Overlaps(RectInt other)
	{
		return other.xMin < xMax && other.xMax > xMin && other.yMin < yMax && other.yMax > yMin;
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
		if (formatProvider == null)
		{
			formatProvider = CultureInfo.InvariantCulture.NumberFormat;
		}
		return UnityString.Format("(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), width.ToString(format, formatProvider), height.ToString(format, formatProvider));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(RectInt other)
	{
		return m_XMin == other.m_XMin && m_YMin == other.m_YMin && m_Width == other.m_Width && m_Height == other.m_Height;
	}
}
