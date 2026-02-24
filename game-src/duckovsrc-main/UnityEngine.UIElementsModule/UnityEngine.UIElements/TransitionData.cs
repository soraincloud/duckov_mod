using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal struct TransitionData : IStyleDataGroup<TransitionData>, IEquatable<TransitionData>
{
	public List<TimeValue> transitionDelay;

	public List<TimeValue> transitionDuration;

	public List<StylePropertyName> transitionProperty;

	public List<EasingFunction> transitionTimingFunction;

	public TransitionData Copy()
	{
		return new TransitionData
		{
			transitionDelay = new List<TimeValue>(transitionDelay),
			transitionDuration = new List<TimeValue>(transitionDuration),
			transitionProperty = new List<StylePropertyName>(transitionProperty),
			transitionTimingFunction = new List<EasingFunction>(transitionTimingFunction)
		};
	}

	public void CopyFrom(ref TransitionData other)
	{
		if (transitionDelay != other.transitionDelay)
		{
			transitionDelay.Clear();
			transitionDelay.AddRange(other.transitionDelay);
		}
		if (transitionDuration != other.transitionDuration)
		{
			transitionDuration.Clear();
			transitionDuration.AddRange(other.transitionDuration);
		}
		if (transitionProperty != other.transitionProperty)
		{
			transitionProperty.Clear();
			transitionProperty.AddRange(other.transitionProperty);
		}
		if (transitionTimingFunction != other.transitionTimingFunction)
		{
			transitionTimingFunction.Clear();
			transitionTimingFunction.AddRange(other.transitionTimingFunction);
		}
	}

	public static bool operator ==(TransitionData lhs, TransitionData rhs)
	{
		return lhs.transitionDelay == rhs.transitionDelay && lhs.transitionDuration == rhs.transitionDuration && lhs.transitionProperty == rhs.transitionProperty && lhs.transitionTimingFunction == rhs.transitionTimingFunction;
	}

	public static bool operator !=(TransitionData lhs, TransitionData rhs)
	{
		return !(lhs == rhs);
	}

	public bool Equals(TransitionData other)
	{
		return other == this;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is TransitionData && Equals((TransitionData)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = transitionDelay.GetHashCode();
		hashCode = (hashCode * 397) ^ transitionDuration.GetHashCode();
		hashCode = (hashCode * 397) ^ transitionProperty.GetHashCode();
		return (hashCode * 397) ^ transitionTimingFunction.GetHashCode();
	}
}
