namespace UnityEngine.UIElements;

[EventCategory(EventCategory.Style)]
public class CustomStyleResolvedEvent : EventBase<CustomStyleResolvedEvent>
{
	public ICustomStyle customStyle => (base.target as VisualElement)?.customStyle;

	static CustomStyleResolvedEvent()
	{
		EventBase<CustomStyleResolvedEvent>.SetCreateFunction(() => new CustomStyleResolvedEvent());
	}
}
