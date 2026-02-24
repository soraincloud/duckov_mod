using System;
using System.Collections.Generic;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UIElements.StyleSheets;

namespace UnityEngine.UIElements;

internal static class ComputedTransitionUtils
{
	private static List<ComputedTransitionProperty> s_ComputedTransitionsBuffer = new List<ComputedTransitionProperty>();

	internal static void UpdateComputedTransitions(ref ComputedStyle computedStyle)
	{
		if (computedStyle.computedTransitions == null)
		{
			computedStyle.computedTransitions = GetOrComputeTransitionPropertyData(ref computedStyle);
		}
	}

	internal static bool HasTransitionProperty(this ref ComputedStyle computedStyle, StylePropertyId id)
	{
		for (int num = computedStyle.computedTransitions.Length - 1; num >= 0; num--)
		{
			ComputedTransitionProperty computedTransitionProperty = computedStyle.computedTransitions[num];
			if (computedTransitionProperty.id == id || StylePropertyUtil.IsMatchingShorthand(computedTransitionProperty.id, id))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetTransitionProperty(this ref ComputedStyle computedStyle, StylePropertyId id, out ComputedTransitionProperty result)
	{
		for (int num = computedStyle.computedTransitions.Length - 1; num >= 0; num--)
		{
			ComputedTransitionProperty computedTransitionProperty = computedStyle.computedTransitions[num];
			if (computedTransitionProperty.id == id || StylePropertyUtil.IsMatchingShorthand(computedTransitionProperty.id, id))
			{
				result = computedTransitionProperty;
				return true;
			}
		}
		result = default(ComputedTransitionProperty);
		return false;
	}

	private static ComputedTransitionProperty[] GetOrComputeTransitionPropertyData(ref ComputedStyle computedStyle)
	{
		int transitionHashCode = GetTransitionHashCode(ref computedStyle);
		if (!StyleCache.TryGetValue(transitionHashCode, out ComputedTransitionProperty[] data))
		{
			ComputeTransitionPropertyData(ref computedStyle, s_ComputedTransitionsBuffer);
			data = new ComputedTransitionProperty[s_ComputedTransitionsBuffer.Count];
			s_ComputedTransitionsBuffer.CopyTo(data);
			s_ComputedTransitionsBuffer.Clear();
			StyleCache.SetValue(transitionHashCode, data);
		}
		return data;
	}

	private static int GetTransitionHashCode(ref ComputedStyle cs)
	{
		int num = 0;
		foreach (TimeValue item in cs.transitionDelay)
		{
			num = (num * 397) ^ item.GetHashCode();
		}
		foreach (TimeValue item2 in cs.transitionDuration)
		{
			num = (num * 397) ^ item2.GetHashCode();
		}
		foreach (StylePropertyName item3 in cs.transitionProperty)
		{
			num = (num * 397) ^ item3.GetHashCode();
		}
		foreach (EasingFunction item4 in cs.transitionTimingFunction)
		{
			num = (num * 397) ^ item4.GetHashCode();
		}
		return num;
	}

	internal static bool SameTransitionProperty(ref ComputedStyle x, ref ComputedStyle y)
	{
		if (x.computedTransitions == y.computedTransitions && x.computedTransitions != null)
		{
			return true;
		}
		return SameTransitionProperty(x.transitionProperty, y.transitionProperty) && SameTransitionProperty(x.transitionDuration, y.transitionDuration) && SameTransitionProperty(x.transitionDelay, y.transitionDelay);
	}

	private static bool SameTransitionProperty(List<StylePropertyName> a, List<StylePropertyName> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		int count = a.Count;
		for (int i = 0; i < count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private static bool SameTransitionProperty(List<TimeValue> a, List<TimeValue> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		int count = a.Count;
		for (int i = 0; i < count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private static void ComputeTransitionPropertyData(ref ComputedStyle computedStyle, List<ComputedTransitionProperty> outData)
	{
		List<StylePropertyName> transitionProperty = computedStyle.transitionProperty;
		if (transitionProperty == null || transitionProperty.Count == 0)
		{
			return;
		}
		List<TimeValue> transitionDuration = computedStyle.transitionDuration;
		List<TimeValue> transitionDelay = computedStyle.transitionDelay;
		List<EasingFunction> transitionTimingFunction = computedStyle.transitionTimingFunction;
		int count = transitionProperty.Count;
		for (int i = 0; i < count; i++)
		{
			StylePropertyId id = transitionProperty[i].id;
			if (id != StylePropertyId.Unknown && StylePropertyUtil.IsAnimatable(id))
			{
				int num = ConvertTransitionTime(GetWrappingTransitionData(transitionDuration, i, new TimeValue(0f)));
				int num2 = ConvertTransitionTime(GetWrappingTransitionData(transitionDelay, i, new TimeValue(0f)));
				float num3 = Mathf.Max(0, num) + num2;
				if (!(num3 <= 0f))
				{
					EasingFunction wrappingTransitionData = GetWrappingTransitionData(transitionTimingFunction, i, EasingMode.Ease);
					outData.Add(new ComputedTransitionProperty
					{
						id = id,
						durationMs = num,
						delayMs = num2,
						easingCurve = ConvertTransitionFunction(wrappingTransitionData.mode)
					});
				}
			}
		}
	}

	private static T GetWrappingTransitionData<T>(List<T> list, int i, T defaultValue)
	{
		return (list.Count == 0) ? defaultValue : list[i % list.Count];
	}

	private static int ConvertTransitionTime(TimeValue time)
	{
		return Mathf.RoundToInt((time.unit == TimeUnit.Millisecond) ? time.value : (time.value * 1000f));
	}

	private static Func<float, float> ConvertTransitionFunction(EasingMode mode)
	{
		return mode switch
		{
			EasingMode.EaseIn => (float t) => Easing.InQuad(t), 
			EasingMode.EaseOut => (float t) => Easing.OutQuad(t), 
			EasingMode.EaseInOut => (float t) => Easing.InOutQuad(t), 
			EasingMode.Linear => (float t) => Easing.Linear(t), 
			EasingMode.EaseInSine => (float t) => Easing.InSine(t), 
			EasingMode.EaseOutSine => (float t) => Easing.OutSine(t), 
			EasingMode.EaseInOutSine => (float t) => Easing.InOutSine(t), 
			EasingMode.EaseInCubic => (float t) => Easing.InCubic(t), 
			EasingMode.EaseOutCubic => (float t) => Easing.OutCubic(t), 
			EasingMode.EaseInOutCubic => (float t) => Easing.InOutCubic(t), 
			EasingMode.EaseInCirc => (float t) => Easing.InCirc(t), 
			EasingMode.EaseOutCirc => (float t) => Easing.OutCirc(t), 
			EasingMode.EaseInOutCirc => (float t) => Easing.InOutCirc(t), 
			EasingMode.EaseInElastic => (float t) => Easing.InElastic(t), 
			EasingMode.EaseOutElastic => (float t) => Easing.OutElastic(t), 
			EasingMode.EaseInOutElastic => (float t) => Easing.InOutElastic(t), 
			EasingMode.EaseInBack => (float t) => Easing.InBack(t), 
			EasingMode.EaseOutBack => (float t) => Easing.OutBack(t), 
			EasingMode.EaseInOutBack => (float t) => Easing.InOutBack(t), 
			EasingMode.EaseInBounce => (float t) => Easing.InBounce(t), 
			EasingMode.EaseOutBounce => (float t) => Easing.OutBounce(t), 
			EasingMode.EaseInOutBounce => (float t) => Easing.InOutBounce(t), 
			_ => (float t) => t * (1.8f + t * (-0.6f + t * -0.2f)), 
		};
	}
}
