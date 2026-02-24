using System;

namespace UnityEngine.UIElements;

internal struct VisualData : IStyleDataGroup<VisualData>, IEquatable<VisualData>
{
	public Color backgroundColor;

	public Background backgroundImage;

	public BackgroundPosition backgroundPositionX;

	public BackgroundPosition backgroundPositionY;

	public BackgroundRepeat backgroundRepeat;

	public BackgroundSize backgroundSize;

	public Color borderBottomColor;

	public Length borderBottomLeftRadius;

	public Length borderBottomRightRadius;

	public Color borderLeftColor;

	public Color borderRightColor;

	public Color borderTopColor;

	public Length borderTopLeftRadius;

	public Length borderTopRightRadius;

	public float opacity;

	public OverflowInternal overflow;

	public VisualData Copy()
	{
		return this;
	}

	public void CopyFrom(ref VisualData other)
	{
		this = other;
	}

	public static bool operator ==(VisualData lhs, VisualData rhs)
	{
		return lhs.backgroundColor == rhs.backgroundColor && lhs.backgroundImage == rhs.backgroundImage && lhs.backgroundPositionX == rhs.backgroundPositionX && lhs.backgroundPositionY == rhs.backgroundPositionY && lhs.backgroundRepeat == rhs.backgroundRepeat && lhs.backgroundSize == rhs.backgroundSize && lhs.borderBottomColor == rhs.borderBottomColor && lhs.borderBottomLeftRadius == rhs.borderBottomLeftRadius && lhs.borderBottomRightRadius == rhs.borderBottomRightRadius && lhs.borderLeftColor == rhs.borderLeftColor && lhs.borderRightColor == rhs.borderRightColor && lhs.borderTopColor == rhs.borderTopColor && lhs.borderTopLeftRadius == rhs.borderTopLeftRadius && lhs.borderTopRightRadius == rhs.borderTopRightRadius && lhs.opacity == rhs.opacity && lhs.overflow == rhs.overflow;
	}

	public static bool operator !=(VisualData lhs, VisualData rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(VisualData other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is VisualData && Equals((VisualData)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = backgroundColor.GetHashCode();
		hashCode = (hashCode * 397) ^ backgroundImage.GetHashCode();
		hashCode = (hashCode * 397) ^ backgroundPositionX.GetHashCode();
		hashCode = (hashCode * 397) ^ backgroundPositionY.GetHashCode();
		hashCode = (hashCode * 397) ^ backgroundRepeat.GetHashCode();
		hashCode = (hashCode * 397) ^ backgroundSize.GetHashCode();
		hashCode = (hashCode * 397) ^ borderBottomColor.GetHashCode();
		hashCode = (hashCode * 397) ^ borderBottomLeftRadius.GetHashCode();
		hashCode = (hashCode * 397) ^ borderBottomRightRadius.GetHashCode();
		hashCode = (hashCode * 397) ^ borderLeftColor.GetHashCode();
		hashCode = (hashCode * 397) ^ borderRightColor.GetHashCode();
		hashCode = (hashCode * 397) ^ borderTopColor.GetHashCode();
		hashCode = (hashCode * 397) ^ borderTopLeftRadius.GetHashCode();
		hashCode = (hashCode * 397) ^ borderTopRightRadius.GetHashCode();
		hashCode = (hashCode * 397) ^ opacity.GetHashCode();
		return (hashCode * 397) ^ (int)overflow;
	}
}
