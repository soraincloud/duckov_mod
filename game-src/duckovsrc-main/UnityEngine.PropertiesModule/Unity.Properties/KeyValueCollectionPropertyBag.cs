using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Unity.Properties;

public class KeyValueCollectionPropertyBag<TDictionary, TKey, TValue> : PropertyBag<TDictionary>, IDictionaryPropertyBag<TDictionary, TKey, TValue>, ICollectionPropertyBag<TDictionary, KeyValuePair<TKey, TValue>>, IPropertyBag<TDictionary>, IPropertyBag, ICollectionPropertyBagAccept<TDictionary>, IDictionaryPropertyBagAccept<TDictionary>, IDictionaryPropertyAccept<TDictionary>, IKeyedProperties<TDictionary, object> where TDictionary : IDictionary<TKey, TValue>
{
	private class KeyValuePairProperty : Property<TDictionary, KeyValuePair<TKey, TValue>>, IDictionaryElementProperty<TKey>, IDictionaryElementProperty, ICollectionElementProperty
	{
		public override string Name => Key.ToString();

		public override bool IsReadOnly => false;

		public TKey Key { get; internal set; }

		public object ObjectKey => Key;

		public override KeyValuePair<TKey, TValue> GetValue(ref TDictionary container)
		{
			TKey key = Key;
			TKey key2 = Key;
			return new KeyValuePair<TKey, TValue>(key, container[key2]);
		}

		public override void SetValue(ref TDictionary container, KeyValuePair<TKey, TValue> value)
		{
			TKey key = value.Key;
			TValue value2 = value.Value;
			container[key] = value2;
		}
	}

	private readonly struct Enumerable : IEnumerable<IProperty<TDictionary>>, IEnumerable
	{
		private class Enumerator : IEnumerator<IProperty<TDictionary>>, IEnumerator, IDisposable
		{
			private readonly TDictionary m_Dictionary;

			private readonly KeyValuePairProperty m_Property;

			private readonly TKey m_Previous;

			private readonly List<TKey> m_Keys;

			private int m_Position;

			public IProperty<TDictionary> Current => m_Property;

			object IEnumerator.Current => Current;

			public Enumerator(TDictionary dictionary, KeyValuePairProperty property)
			{
				m_Dictionary = dictionary;
				m_Property = property;
				m_Previous = property.Key;
				m_Position = -1;
				m_Keys = CollectionPool<List<TKey>, TKey>.Get();
				m_Keys.AddRange(m_Dictionary.Keys);
			}

			public bool MoveNext()
			{
				m_Position++;
				if (m_Position < m_Dictionary.Count)
				{
					m_Property.Key = m_Keys[m_Position];
					return true;
				}
				m_Property.Key = m_Previous;
				return false;
			}

			public void Reset()
			{
				m_Position = -1;
				m_Property.Key = m_Previous;
			}

			public void Dispose()
			{
				CollectionPool<List<TKey>, TKey>.Release(m_Keys);
			}
		}

		private readonly TDictionary m_Dictionary;

		private readonly KeyValuePairProperty m_Property;

		public Enumerable(TDictionary dictionary, KeyValuePairProperty property)
		{
			m_Dictionary = dictionary;
			m_Property = property;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(m_Dictionary, m_Property);
		}

		IEnumerator<IProperty<TDictionary>> IEnumerable<IProperty<TDictionary>>.GetEnumerator()
		{
			return new Enumerator(m_Dictionary, m_Property);
		}
	}

	private readonly KeyValuePairProperty m_KeyValuePairProperty = new KeyValuePairProperty();

	public override PropertyCollection<TDictionary> GetProperties()
	{
		return PropertyCollection<TDictionary>.Empty;
	}

	public override PropertyCollection<TDictionary> GetProperties(ref TDictionary container)
	{
		return new PropertyCollection<TDictionary>(new Enumerable(container, m_KeyValuePairProperty));
	}

	void ICollectionPropertyBagAccept<TDictionary>.Accept(ICollectionPropertyBagVisitor visitor, ref TDictionary container)
	{
		visitor.Visit(this, ref container);
	}

	void IDictionaryPropertyBagAccept<TDictionary>.Accept(IDictionaryPropertyBagVisitor visitor, ref TDictionary container)
	{
		visitor.Visit(this, ref container);
	}

	void IDictionaryPropertyAccept<TDictionary>.Accept<TContainer>(IDictionaryPropertyVisitor visitor, Property<TContainer, TDictionary> property, ref TContainer container, ref TDictionary dictionary)
	{
		using (new AttributesScope(m_KeyValuePairProperty, property))
		{
			visitor.Visit<TContainer, TDictionary, TKey, TValue>(property, ref container, ref dictionary);
		}
	}

	bool IKeyedProperties<TDictionary, object>.TryGetProperty(ref TDictionary container, object key, out IProperty<TDictionary> property)
	{
		TKey key2 = (TKey)key;
		if (container.ContainsKey(key2))
		{
			property = new KeyValuePairProperty
			{
				Key = (TKey)key
			};
			return true;
		}
		property = null;
		return false;
	}
}
