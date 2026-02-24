using System;

namespace UnityEngine.UIElements;

internal struct TransformData : IStyleDataGroup<TransformData>, IEquatable<TransformData>
{
	public Rotate rotate;

	public Scale scale;

	public TransformOrigin transformOrigin;

	public Translate translate;

	public TransformData Copy()
	{
		return this;
	}

	public void CopyFrom(ref TransformData other)
	{
		this = other;
	}

	public static bool operator ==(TransformData lhs, TransformData rhs)
	{
		return lhs.rotate == rhs.rotate && lhs.scale == rhs.scale && lhs.transformOrigin == rhs.transformOrigin && lhs.translate == rhs.translate;
	}

	public static bool operator !=(TransformData lhs, TransformData rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(TransformData other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is TransformData && Equals((TransformData)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = rotate.GetHashCode();
		hashCode = (hashCode * 397) ^ scale.GetHashCode();
		hashCode = (hashCode * 397) ^ transformOrigin.GetHashCode();
		return (hashCode * 397) ^ translate.GetHashCode();
	}
}
