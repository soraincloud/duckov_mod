using System;

namespace UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class EventInterestAttribute : Attribute
{
	internal Type[] eventTypes;

	internal EventCategoryFlags categoryFlags = EventCategoryFlags.None;

	public EventInterestAttribute(params Type[] eventTypes)
	{
		this.eventTypes = eventTypes;
	}

	public EventInterestAttribute(EventInterestOptions interests)
	{
		categoryFlags = (EventCategoryFlags)interests;
	}

	internal EventInterestAttribute(EventInterestOptionsInternal interests)
	{
		categoryFlags = (EventCategoryFlags)interests;
	}
}
