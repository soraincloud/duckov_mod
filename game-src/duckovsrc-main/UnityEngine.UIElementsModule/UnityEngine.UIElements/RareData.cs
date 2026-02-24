using System;

namespace UnityEngine.UIElements;

internal struct RareData : IStyleDataGroup<RareData>, IEquatable<RareData>
{
	public Cursor cursor;

	public TextOverflow textOverflow;

	public Color unityBackgroundImageTintColor;

	public OverflowClipBox unityOverflowClipBox;

	public int unitySliceBottom;

	public int unitySliceLeft;

	public int unitySliceRight;

	public float unitySliceScale;

	public int unitySliceTop;

	public TextOverflowPosition unityTextOverflowPosition;

	public RareData Copy()
	{
		return this;
	}

	public void CopyFrom(ref RareData other)
	{
		this = other;
	}

	public static bool operator ==(RareData lhs, RareData rhs)
	{
		return lhs.cursor == rhs.cursor && lhs.textOverflow == rhs.textOverflow && lhs.unityBackgroundImageTintColor == rhs.unityBackgroundImageTintColor && lhs.unityOverflowClipBox == rhs.unityOverflowClipBox && lhs.unitySliceBottom == rhs.unitySliceBottom && lhs.unitySliceLeft == rhs.unitySliceLeft && lhs.unitySliceRight == rhs.unitySliceRight && lhs.unitySliceScale == rhs.unitySliceScale && lhs.unitySliceTop == rhs.unitySliceTop && lhs.unityTextOverflowPosition == rhs.unityTextOverflowPosition;
	}

	public static bool operator !=(RareData lhs, RareData rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(RareData other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is RareData && Equals((RareData)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = cursor.GetHashCode();
		hashCode = (hashCode * 397) ^ (int)textOverflow;
		hashCode = (hashCode * 397) ^ unityBackgroundImageTintColor.GetHashCode();
		hashCode = (hashCode * 397) ^ (int)unityOverflowClipBox;
		hashCode = (hashCode * 397) ^ unitySliceBottom;
		hashCode = (hashCode * 397) ^ unitySliceLeft;
		hashCode = (hashCode * 397) ^ unitySliceRight;
		hashCode = (hashCode * 397) ^ unitySliceScale.GetHashCode();
		hashCode = (hashCode * 397) ^ unitySliceTop;
		return (hashCode * 397) ^ (int)unityTextOverflowPosition;
	}
}
