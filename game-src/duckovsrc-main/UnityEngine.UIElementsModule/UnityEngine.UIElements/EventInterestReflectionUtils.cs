using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.UIElements;

internal static class EventInterestReflectionUtils
{
	private struct DefaultEventInterests
	{
		public int DefaultActionCategories;

		public int DefaultActionAtTargetCategories;
	}

	private static readonly Dictionary<Type, DefaultEventInterests> s_DefaultEventInterests = new Dictionary<Type, DefaultEventInterests>();

	private static readonly Dictionary<Type, EventCategory> s_EventCategories = new Dictionary<Type, EventCategory>();

	internal static void GetDefaultEventInterests(Type elementType, out int defaultActionCategories, out int defaultActionAtTargetCategories)
	{
		if (!s_DefaultEventInterests.TryGetValue(elementType, out var value))
		{
			Type baseType = elementType.BaseType;
			if (baseType != null)
			{
				GetDefaultEventInterests(baseType, out value.DefaultActionCategories, out value.DefaultActionAtTargetCategories);
			}
			value.DefaultActionCategories |= ComputeDefaultEventInterests(elementType, "ExecuteDefaultAction") | ComputeDefaultEventInterests(elementType, "ExecuteDefaultActionDisabled");
			value.DefaultActionAtTargetCategories |= ComputeDefaultEventInterests(elementType, "ExecuteDefaultActionAtTarget") | ComputeDefaultEventInterests(elementType, "ExecuteDefaultActionDisabledAtTarget");
			s_DefaultEventInterests.Add(elementType, value);
		}
		defaultActionCategories = value.DefaultActionCategories;
		defaultActionAtTargetCategories = value.DefaultActionAtTargetCategories;
	}

	private static int ComputeDefaultEventInterests(Type elementType, string methodName)
	{
		MethodInfo method = elementType.GetMethod(methodName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (method == null)
		{
			return 0;
		}
		bool flag = false;
		int num = 0;
		object[] customAttributes = method.GetCustomAttributes(typeof(EventInterestAttribute), inherit: false);
		object[] array = customAttributes;
		for (int i = 0; i < array.Length; i++)
		{
			EventInterestAttribute eventInterestAttribute = (EventInterestAttribute)array[i];
			flag = true;
			if (eventInterestAttribute.eventTypes != null)
			{
				Type[] eventTypes = eventInterestAttribute.eventTypes;
				foreach (Type eventType in eventTypes)
				{
					num |= 1 << (int)GetEventCategory(eventType);
				}
			}
			num |= (int)eventInterestAttribute.categoryFlags;
		}
		return flag ? num : (-1);
	}

	internal static EventCategory GetEventCategory(Type eventType)
	{
		if (s_EventCategories.TryGetValue(eventType, out var value))
		{
			return value;
		}
		object[] customAttributes = eventType.GetCustomAttributes(typeof(EventCategoryAttribute), inherit: true);
		object[] array = customAttributes;
		int num = 0;
		if (num < array.Length)
		{
			EventCategoryAttribute eventCategoryAttribute = (EventCategoryAttribute)array[num];
			value = eventCategoryAttribute.category;
			s_EventCategories.Add(eventType, value);
			return value;
		}
		throw new ArgumentOutOfRangeException("eventType", "Type must derive from EventBase<T>");
	}
}
