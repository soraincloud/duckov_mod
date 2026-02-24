using System.Collections.Generic;

namespace Unity.Properties;

public class IndexedCollectionPropertyBag<TList, TElement> : PropertyBag<TList>, IListPropertyBag<TList, TElement>, ICollectionPropertyBag<TList, TElement>, IPropertyBag<TList>, IPropertyBag, ICollectionPropertyBagAccept<TList>, IListPropertyBagAccept<TList>, IListPropertyAccept<TList>, IIndexedProperties<TList>, IConstructorWithCount<TList>, IConstructor, IIndexedCollectionPropertyBagEnumerator<TList> where TList : IList<TElement>
{
	private class ListElementProperty : Property<TList, TElement>, IListElementProperty, ICollectionElementProperty
	{
		internal int m_Index;

		internal bool m_IsReadOnly;

		public int Index => m_Index;

		public override string Name => Index.ToString();

		public override bool IsReadOnly => m_IsReadOnly;

		public override TElement GetValue(ref TList container)
		{
			int index = m_Index;
			return container[index];
		}

		public override void SetValue(ref TList container, TElement value)
		{
			int index = m_Index;
			container[index] = value;
		}
	}

	private readonly ListElementProperty m_Property = new ListElementProperty();

	public override PropertyCollection<TList> GetProperties()
	{
		return PropertyCollection<TList>.Empty;
	}

	public override PropertyCollection<TList> GetProperties(ref TList container)
	{
		return new PropertyCollection<TList>(new IndexedCollectionPropertyBagEnumerable<TList>(this, container));
	}

	public bool TryGetProperty(ref TList container, int index, out IProperty<TList> property)
	{
		if ((uint)index >= (uint)container.Count)
		{
			property = null;
			return false;
		}
		property = new ListElementProperty
		{
			m_Index = index,
			m_IsReadOnly = false
		};
		return true;
	}

	void ICollectionPropertyBagAccept<TList>.Accept(ICollectionPropertyBagVisitor visitor, ref TList container)
	{
		visitor.Visit(this, ref container);
	}

	void IListPropertyBagAccept<TList>.Accept(IListPropertyBagVisitor visitor, ref TList list)
	{
		visitor.Visit(this, ref list);
	}

	void IListPropertyAccept<TList>.Accept<TContainer>(IListPropertyVisitor visitor, Property<TContainer, TList> property, ref TContainer container, ref TList list)
	{
		using (new AttributesScope(m_Property, property))
		{
			visitor.Visit<TContainer, TList, TElement>(property, ref container, ref list);
		}
	}

	TList IConstructorWithCount<TList>.InstantiateWithCount(int count)
	{
		return InstantiateWithCount(count);
	}

	protected virtual TList InstantiateWithCount(int count)
	{
		return default(TList);
	}

	int IIndexedCollectionPropertyBagEnumerator<TList>.GetCount(ref TList container)
	{
		return container.Count;
	}

	IProperty<TList> IIndexedCollectionPropertyBagEnumerator<TList>.GetSharedProperty()
	{
		return m_Property;
	}

	IndexedCollectionSharedPropertyState IIndexedCollectionPropertyBagEnumerator<TList>.GetSharedPropertyState()
	{
		return new IndexedCollectionSharedPropertyState
		{
			Index = m_Property.m_Index,
			IsReadOnly = m_Property.IsReadOnly
		};
	}

	void IIndexedCollectionPropertyBagEnumerator<TList>.SetSharedPropertyState(IndexedCollectionSharedPropertyState state)
	{
		m_Property.m_Index = state.Index;
		m_Property.m_IsReadOnly = state.IsReadOnly;
	}
}
