using System.Collections.Generic;

namespace UnityEngine.UIElements;

[EventCategory(EventCategory.StyleTransition)]
public abstract class TransitionEventBase<T> : EventBase<T>, ITransitionEvent where T : TransitionEventBase<T>, new()
{
	public StylePropertyNameCollection stylePropertyNames { get; }

	public double elapsedTime { get; protected set; }

	protected TransitionEventBase()
	{
		stylePropertyNames = new StylePropertyNameCollection(new List<StylePropertyName>());
		LocalInit();
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles;
		base.propagateToIMGUI = false;
		stylePropertyNames.propertiesList.Clear();
		elapsedTime = 0.0;
	}

	public static T GetPooled(StylePropertyName stylePropertyName, double elapsedTime)
	{
		T val = EventBase<T>.GetPooled();
		val.stylePropertyNames.propertiesList.Add(stylePropertyName);
		val.elapsedTime = elapsedTime;
		return val;
	}

	public bool AffectsProperty(StylePropertyName stylePropertyName)
	{
		return stylePropertyNames.Contains(stylePropertyName);
	}
}
