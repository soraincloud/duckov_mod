using System;

namespace UnityEngine.UIElements;

public struct BackgroundSize : IEquatable<BackgroundSize>
{
	private BackgroundSizeType m_SizeType;

	private Length m_X;

	private Length m_Y;

	public BackgroundSizeType sizeType
	{
		get
		{
			return m_SizeType;
		}
		set
		{
			m_SizeType = value;
			m_X = new Length(0f);
			m_Y = new Length(0f);
		}
	}

	public Length x
	{
		get
		{
			return m_X;
		}
		set
		{
			m_X = value;
			m_SizeType = BackgroundSizeType.Length;
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
			m_SizeType = BackgroundSizeType.Length;
		}
	}

	public BackgroundSize(Length sizeX, Length sizeY)
	{
		m_SizeType = BackgroundSizeType.Length;
		m_X = sizeX;
		m_Y = sizeY;
	}

	public BackgroundSize(BackgroundSizeType sizeType)
	{
		m_SizeType = sizeType;
		m_X = new Length(0f);
		m_Y = new Length(0f);
	}

	internal static BackgroundSize Initial()
	{
		return BackgroundPropertyHelper.ConvertScaleModeToBackgroundSize();
	}

	public override bool Equals(object obj)
	{
		return obj is BackgroundSize && Equals((BackgroundSize)obj);
	}

	public bool Equals(BackgroundSize other)
	{
		return other.x == x && other.y == y && other.sizeType == sizeType;
	}

	public override int GetHashCode()
	{
		int num = 1500536833;
		num = num * -1521134295 + m_SizeType.GetHashCode();
		num = num * -1521134295 + m_X.GetHashCode();
		return num * -1521134295 + m_Y.GetHashCode();
	}

	public static bool operator ==(BackgroundSize style1, BackgroundSize style2)
	{
		return style1.Equals(style2);
	}

	public static bool operator !=(BackgroundSize style1, BackgroundSize style2)
	{
		return !(style1 == style2);
	}

	public override string ToString()
	{
		return $"(sizeType:{sizeType} x:{x}, y:{y})";
	}
}
