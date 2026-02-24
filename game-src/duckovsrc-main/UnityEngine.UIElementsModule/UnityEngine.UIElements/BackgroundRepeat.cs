using System;

namespace UnityEngine.UIElements;

public struct BackgroundRepeat : IEquatable<BackgroundRepeat>
{
	public Repeat x;

	public Repeat y;

	public BackgroundRepeat(Repeat repeatX, Repeat repeatY)
	{
		x = repeatX;
		y = repeatY;
	}

	internal static BackgroundRepeat Initial()
	{
		return BackgroundPropertyHelper.ConvertScaleModeToBackgroundRepeat();
	}

	public override bool Equals(object obj)
	{
		return obj is BackgroundRepeat && Equals((BackgroundRepeat)obj);
	}

	public bool Equals(BackgroundRepeat other)
	{
		return other.x == x && other.y == y;
	}

	public override int GetHashCode()
	{
		int num = 1500536833;
		num = num * -1521134295 + x.GetHashCode();
		return num * -1521134295 + y.GetHashCode();
	}

	public static bool operator ==(BackgroundRepeat style1, BackgroundRepeat style2)
	{
		return style1.Equals(style2);
	}

	public static bool operator !=(BackgroundRepeat style1, BackgroundRepeat style2)
	{
		return !(style1 == style2);
	}

	public override string ToString()
	{
		return $"(x:{x}, y:{y})";
	}
}
