using System;

namespace UnityEngine.UIElements;

public struct BackgroundPosition : IEquatable<BackgroundPosition>
{
	public BackgroundPositionKeyword keyword;

	public Length offset;

	public BackgroundPosition(BackgroundPositionKeyword keyword)
	{
		this.keyword = keyword;
		offset = new Length(0f);
	}

	public BackgroundPosition(BackgroundPositionKeyword keyword, Length offset)
	{
		this.keyword = keyword;
		this.offset = offset;
	}

	internal static BackgroundPosition Initial()
	{
		return BackgroundPropertyHelper.ConvertScaleModeToBackgroundPosition();
	}

	public override bool Equals(object obj)
	{
		return obj is BackgroundPosition && Equals((BackgroundPosition)obj);
	}

	public bool Equals(BackgroundPosition other)
	{
		return other.offset == offset && other.keyword == keyword;
	}

	public override int GetHashCode()
	{
		int num = 1500536833;
		num = num * -1521134295 + keyword.GetHashCode();
		return num * -1521134295 + offset.GetHashCode();
	}

	public static bool operator ==(BackgroundPosition style1, BackgroundPosition style2)
	{
		return style1.Equals(style2);
	}

	public static bool operator !=(BackgroundPosition style1, BackgroundPosition style2)
	{
		return !(style1 == style2);
	}

	public override string ToString()
	{
		return $"(type:{keyword} x:{offset})";
	}
}
