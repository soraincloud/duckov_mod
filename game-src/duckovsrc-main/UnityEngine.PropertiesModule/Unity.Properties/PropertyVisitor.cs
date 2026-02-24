using System.Collections.Generic;
using Unity.Properties.Internal;

namespace Unity.Properties;

public abstract class PropertyVisitor : IPropertyBagVisitor, IListPropertyBagVisitor, IDictionaryPropertyBagVisitor, IPropertyVisitor, ICollectionPropertyVisitor, IListPropertyVisitor, ISetPropertyVisitor, IDictionaryPropertyVisitor
{
	private readonly List<IPropertyVisitorAdapter> m_Adapters = new List<IPropertyVisitorAdapter>();

	public void AddAdapter(IPropertyVisitorAdapter adapter)
	{
		m_Adapters.Add(adapter);
	}

	public void RemoveAdapter(IPropertyVisitorAdapter adapter)
	{
		m_Adapters.Remove(adapter);
	}

	void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
	{
		foreach (IProperty<TContainer> property in properties.GetProperties(ref container))
		{
			property.Accept(this, ref container);
		}
	}

	void IListPropertyBagVisitor.Visit<TList, TElement>(IListPropertyBag<TList, TElement> properties, ref TList container)
	{
		foreach (IProperty<TList> property in properties.GetProperties(ref container))
		{
			property.Accept(this, ref container);
		}
	}

	void IDictionaryPropertyBagVisitor.Visit<TDictionary, TKey, TValue>(IDictionaryPropertyBag<TDictionary, TKey, TValue> properties, ref TDictionary container)
	{
		foreach (IProperty<TDictionary> property in properties.GetProperties(ref container))
		{
			property.Accept(this, ref container);
		}
	}

	void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
	{
		TValue value = property.GetValue(ref container);
		if (!IsExcluded(property, new ReadOnlyAdapterCollection(m_Adapters).GetEnumerator(), ref container, ref value) && !IsExcluded(property, ref container, ref value))
		{
			ContinueVisitation(property, new ReadOnlyAdapterCollection(m_Adapters).GetEnumerator(), ref container, ref value);
			if (!property.IsReadOnly)
			{
				property.SetValue(ref container, value);
			}
		}
	}

	internal void ContinueVisitation<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
	{
		if (PropertyBagStore.TryGetPropertyBagForValue(ref value, out var propertyBag))
		{
			IPropertyBag propertyBag2 = propertyBag;
			IPropertyBag propertyBag3 = propertyBag2;
			if (propertyBag3 is IDictionaryPropertyAccept<TValue> dictionaryPropertyAccept)
			{
				dictionaryPropertyAccept.Accept(this, property, ref container, ref value);
				return;
			}
			if (propertyBag3 is IListPropertyAccept<TValue> listPropertyAccept)
			{
				listPropertyAccept.Accept(this, property, ref container, ref value);
				return;
			}
			if (propertyBag3 is ISetPropertyAccept<TValue> setPropertyAccept)
			{
				setPropertyAccept.Accept(this, property, ref container, ref value);
				return;
			}
			if (propertyBag3 is ICollectionPropertyAccept<TValue> collectionPropertyAccept)
			{
				collectionPropertyAccept.Accept(this, property, ref container, ref value);
				return;
			}
		}
		VisitProperty(property, ref container, ref value);
	}

	void ICollectionPropertyVisitor.Visit<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection collection)
	{
		VisitCollection<TContainer, TCollection, TElement>(property, ref container, ref collection);
	}

	void IListPropertyVisitor.Visit<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList list)
	{
		VisitList<TContainer, TList, TElement>(property, ref container, ref list);
	}

	void ISetPropertyVisitor.Visit<TContainer, TSet, TElement>(Property<TContainer, TSet> property, ref TContainer container, ref TSet set)
	{
		VisitSet<TContainer, TSet, TElement>(property, ref container, ref set);
	}

	void IDictionaryPropertyVisitor.Visit<TContainer, TDictionary, TKey, TValue>(Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary dictionary)
	{
		VisitDictionary<TContainer, TDictionary, TKey, TValue>(property, ref container, ref dictionary);
	}

	protected virtual bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
	{
		return false;
	}

	protected virtual void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
	{
		PropertyContainer.TryAccept(this, ref value);
	}

	protected virtual void VisitCollection<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection value) where TCollection : ICollection<TElement>
	{
		VisitProperty(property, ref container, ref value);
	}

	protected virtual void VisitList<TContainer, TList, TElement>(Property<TContainer, TList> property, ref TContainer container, ref TList value) where TList : IList<TElement>
	{
		VisitCollection<TContainer, TList, TElement>(property, ref container, ref value);
	}

	protected virtual void VisitSet<TContainer, TSet, TValue>(Property<TContainer, TSet> property, ref TContainer container, ref TSet value) where TSet : ISet<TValue>
	{
		VisitCollection<TContainer, TSet, TValue>(property, ref container, ref value);
	}

	protected virtual void VisitDictionary<TContainer, TDictionary, TKey, TValue>(Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary value) where TDictionary : IDictionary<TKey, TValue>
	{
		VisitCollection<TContainer, TDictionary, KeyValuePair<TKey, TValue>>(property, ref container, ref value);
	}

	private bool IsExcluded<TContainer, TValue>(Property<TContainer, TValue> property, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container, ref TValue value)
	{
		while (enumerator.MoveNext())
		{
			IPropertyVisitorAdapter current = enumerator.Current;
			IPropertyVisitorAdapter propertyVisitorAdapter = current;
			IPropertyVisitorAdapter propertyVisitorAdapter2 = propertyVisitorAdapter;
			if (!(propertyVisitorAdapter2 is IExcludePropertyAdapter<TContainer, TValue> excludePropertyAdapter))
			{
				if (!(propertyVisitorAdapter2 is IExcludeContravariantPropertyAdapter<TContainer, TValue> excludeContravariantPropertyAdapter))
				{
					if (!(propertyVisitorAdapter2 is IExcludePropertyAdapter<TValue> excludePropertyAdapter2))
					{
						if (!(propertyVisitorAdapter2 is IExcludeContravariantPropertyAdapter<TValue> excludeContravariantPropertyAdapter2))
						{
							if (!(propertyVisitorAdapter2 is IExcludePropertyAdapter excludePropertyAdapter3) || !excludePropertyAdapter3.IsExcluded(ExcludeContext<TContainer, TValue>.FromProperty(this, property), ref container, ref value))
							{
								continue;
							}
							return true;
						}
						bool flag = excludeContravariantPropertyAdapter2.IsExcluded(ExcludeContext<TContainer>.FromProperty(this, property), ref container, value);
						value = property.GetValue(ref container);
						if (flag)
						{
							return true;
						}
					}
					else if (excludePropertyAdapter2.IsExcluded(ExcludeContext<TContainer, TValue>.FromProperty(this, property), ref container, ref value))
					{
						return true;
					}
				}
				else
				{
					bool flag2 = excludeContravariantPropertyAdapter.IsExcluded(ExcludeContext<TContainer>.FromProperty(this, property), ref container, value);
					value = property.GetValue(ref container);
					if (flag2)
					{
						return true;
					}
				}
			}
			else if (excludePropertyAdapter.IsExcluded(ExcludeContext<TContainer, TValue>.FromProperty(this, property), ref container, ref value))
			{
				return true;
			}
		}
		return false;
	}

	internal void ContinueVisitation<TContainer, TValue>(Property<TContainer, TValue> property, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container, ref TValue value)
	{
		while (enumerator.MoveNext())
		{
			IPropertyVisitorAdapter current = enumerator.Current;
			IPropertyVisitorAdapter propertyVisitorAdapter = current;
			IPropertyVisitorAdapter propertyVisitorAdapter2 = propertyVisitorAdapter;
			if (!(propertyVisitorAdapter2 is IVisitPropertyAdapter<TContainer, TValue> visitPropertyAdapter))
			{
				if (!(propertyVisitorAdapter2 is IVisitContravariantPropertyAdapter<TContainer, TValue> visitContravariantPropertyAdapter))
				{
					if (!(propertyVisitorAdapter2 is IVisitPropertyAdapter<TValue> visitPropertyAdapter2))
					{
						if (!(propertyVisitorAdapter2 is IVisitContravariantPropertyAdapter<TValue> visitContravariantPropertyAdapter2))
						{
							if (!(propertyVisitorAdapter2 is IVisitPropertyAdapter visitPropertyAdapter3))
							{
								continue;
							}
							visitPropertyAdapter3.Visit(VisitContext<TContainer, TValue>.FromProperty(this, enumerator, property), ref container, ref value);
							return;
						}
						visitContravariantPropertyAdapter2.Visit(VisitContext<TContainer>.FromProperty(this, enumerator, property), ref container, value);
						value = property.GetValue(ref container);
						return;
					}
					visitPropertyAdapter2.Visit(VisitContext<TContainer, TValue>.FromProperty(this, enumerator, property), ref container, ref value);
					return;
				}
				visitContravariantPropertyAdapter.Visit(VisitContext<TContainer>.FromProperty(this, enumerator, property), ref container, value);
				value = property.GetValue(ref container);
				return;
			}
			visitPropertyAdapter.Visit(VisitContext<TContainer, TValue>.FromProperty(this, enumerator, property), ref container, ref value);
			return;
		}
		ContinueVisitationWithoutAdapters(property, enumerator, ref container, ref value);
	}

	internal void ContinueVisitationWithoutAdapters<TContainer, TValue>(Property<TContainer, TValue> property, ReadOnlyAdapterCollection.Enumerator enumerator, ref TContainer container, ref TValue value)
	{
		ContinueVisitation(property, ref container, ref value);
	}
}
