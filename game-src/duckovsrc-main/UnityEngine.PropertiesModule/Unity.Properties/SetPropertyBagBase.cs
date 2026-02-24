using System;
using System.Collections.Generic;

namespace Unity.Properties;

public class SetPropertyBagBase<TSet, TElement> : PropertyBag<TSet>, ISetPropertyBag<TSet, TElement>, ICollectionPropertyBag<TSet, TElement>, IPropertyBag<TSet>, IPropertyBag, ICollectionPropertyBagAccept<TSet>, ISetPropertyBagAccept<TSet>, ISetPropertyAccept<TSet>, IKeyedProperties<TSet, object> where TSet : ISet<TElement>
{
	private class SetElementProperty : Property<TSet, TElement>, ISetElementProperty<TElement>, ISetElementProperty, ICollectionElementProperty
	{
		internal TElement m_Value;

		public override string Name => m_Value.ToString();

		public override bool IsReadOnly => true;

		public TElement Key => m_Value;

		public object ObjectKey => m_Value;

		public override TElement GetValue(ref TSet container)
		{
			return m_Value;
		}

		public override void SetValue(ref TSet container, TElement value)
		{
			throw new InvalidOperationException("Property is ReadOnly.");
		}
	}

	private readonly SetElementProperty m_Property = new SetElementProperty();

	public override PropertyCollection<TSet> GetProperties()
	{
		return PropertyCollection<TSet>.Empty;
	}

	public override PropertyCollection<TSet> GetProperties(ref TSet container)
	{
		return new PropertyCollection<TSet>(GetPropertiesEnumerable(container));
	}

	private IEnumerable<IProperty<TSet>> GetPropertiesEnumerable(TSet container)
	{
		foreach (TElement element in container)
		{
			m_Property.m_Value = element;
			yield return m_Property;
		}
	}

	void ICollectionPropertyBagAccept<TSet>.Accept(ICollectionPropertyBagVisitor visitor, ref TSet container)
	{
		visitor.Visit(this, ref container);
	}

	void ISetPropertyBagAccept<TSet>.Accept(ISetPropertyBagVisitor visitor, ref TSet container)
	{
		visitor.Visit(this, ref container);
	}

	void ISetPropertyAccept<TSet>.Accept<TContainer>(ISetPropertyVisitor visitor, Property<TContainer, TSet> property, ref TContainer container, ref TSet dictionary)
	{
		using (new AttributesScope(m_Property, property))
		{
			visitor.Visit<TContainer, TSet, TElement>(property, ref container, ref dictionary);
		}
	}

	public bool TryGetProperty(ref TSet container, object key, out IProperty<TSet> property)
	{
		TElement item = (TElement)key;
		if (container.Contains(item))
		{
			property = new SetElementProperty
			{
				m_Value = (TElement)key
			};
			return true;
		}
		property = null;
		return false;
	}
}
