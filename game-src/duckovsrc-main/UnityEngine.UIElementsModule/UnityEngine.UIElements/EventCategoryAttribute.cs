using System;

namespace UnityEngine.UIElements;

[AttributeUsage(AttributeTargets.Class)]
internal class EventCategoryAttribute : Attribute
{
	internal EventCategory category;

	public EventCategoryAttribute(EventCategory category)
	{
		this.category = category;
	}
}
