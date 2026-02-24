using System;
using System.Collections.Generic;

namespace Unity.Properties;

public abstract class ContainerPropertyBag<TContainer> : PropertyBag<TContainer>, INamedProperties<TContainer>
{
	private readonly List<IProperty<TContainer>> m_PropertiesList = new List<IProperty<TContainer>>();

	private readonly Dictionary<string, IProperty<TContainer>> m_PropertiesHash = new Dictionary<string, IProperty<TContainer>>();

	static ContainerPropertyBag()
	{
		if (!TypeTraits.IsContainer(typeof(TContainer)))
		{
			throw new InvalidOperationException($"Failed to create a property bag for Type=[{typeof(TContainer)}]. The type is not a valid container type.");
		}
	}

	protected void AddProperty<TValue>(Property<TContainer, TValue> property)
	{
		m_PropertiesList.Add(property);
		m_PropertiesHash.Add(property.Name, property);
	}

	public override PropertyCollection<TContainer> GetProperties()
	{
		return new PropertyCollection<TContainer>(m_PropertiesList);
	}

	public override PropertyCollection<TContainer> GetProperties(ref TContainer container)
	{
		return new PropertyCollection<TContainer>(m_PropertiesList);
	}

	public bool TryGetProperty(ref TContainer container, string name, out IProperty<TContainer> property)
	{
		return m_PropertiesHash.TryGetValue(name, out property);
	}
}
