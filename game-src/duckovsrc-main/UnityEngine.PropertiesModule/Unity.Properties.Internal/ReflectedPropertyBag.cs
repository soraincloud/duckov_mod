using UnityEngine;

namespace Unity.Properties.Internal;

[ReflectedPropertyBag]
internal class ReflectedPropertyBag<TContainer> : ContainerPropertyBag<TContainer>
{
	internal new void AddProperty<TValue>(Property<TContainer, TValue> property)
	{
		TContainer container = default(TContainer);
		if (TryGetProperty(ref container, property.Name, out var property2))
		{
			if (!(property2.DeclaredValueType() == typeof(TValue)))
			{
				Debug.LogWarning("Detected multiple return types for PropertyBag=[" + TypeUtility.GetTypeDisplayName(typeof(TContainer)) + "] Property=[" + property.Name + "]. The property will use the most derived Type=[" + TypeUtility.GetTypeDisplayName(property2.DeclaredValueType()) + "] and IgnoreType=[" + TypeUtility.GetTypeDisplayName(property.DeclaredValueType()) + "].");
			}
		}
		else
		{
			base.AddProperty(property);
		}
	}
}
