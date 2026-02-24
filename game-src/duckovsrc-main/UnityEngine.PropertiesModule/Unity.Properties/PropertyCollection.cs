using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Properties;

public readonly struct PropertyCollection<TContainer> : IEnumerable<IProperty<TContainer>>, IEnumerable
{
	private enum EnumeratorType
	{
		Empty,
		Enumerable,
		List,
		IndexedCollectionPropertyBag
	}

	public struct Enumerator : IEnumerator<IProperty<TContainer>>, IEnumerator, IDisposable
	{
		private readonly EnumeratorType m_Type;

		private IEnumerator<IProperty<TContainer>> m_Enumerator;

		private List<IProperty<TContainer>>.Enumerator m_Properties;

		private IndexedCollectionPropertyBagEnumerator<TContainer> m_IndexedCollectionPropertyBag;

		public IProperty<TContainer> Current { get; private set; }

		object IEnumerator.Current => Current;

		internal Enumerator(IEnumerator<IProperty<TContainer>> enumerator)
		{
			m_Type = EnumeratorType.Enumerable;
			m_Enumerator = enumerator;
			m_Properties = default(List<IProperty<TContainer>>.Enumerator);
			m_IndexedCollectionPropertyBag = default(IndexedCollectionPropertyBagEnumerator<TContainer>);
			Current = null;
		}

		internal Enumerator(List<IProperty<TContainer>>.Enumerator properties)
		{
			m_Type = EnumeratorType.List;
			m_Enumerator = null;
			m_Properties = properties;
			m_IndexedCollectionPropertyBag = default(IndexedCollectionPropertyBagEnumerator<TContainer>);
			Current = null;
		}

		internal Enumerator(IndexedCollectionPropertyBagEnumerator<TContainer> enumerator)
		{
			m_Type = EnumeratorType.IndexedCollectionPropertyBag;
			m_Enumerator = null;
			m_Properties = default(List<IProperty<TContainer>>.Enumerator);
			m_IndexedCollectionPropertyBag = enumerator;
			Current = null;
		}

		public bool MoveNext()
		{
			bool result;
			switch (m_Type)
			{
			case EnumeratorType.Empty:
				return false;
			case EnumeratorType.Enumerable:
				result = m_Enumerator.MoveNext();
				Current = m_Enumerator.Current;
				break;
			case EnumeratorType.List:
				result = m_Properties.MoveNext();
				Current = m_Properties.Current;
				break;
			case EnumeratorType.IndexedCollectionPropertyBag:
				result = m_IndexedCollectionPropertyBag.MoveNext();
				Current = m_IndexedCollectionPropertyBag.Current;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return result;
		}

		public void Reset()
		{
			switch (m_Type)
			{
			case EnumeratorType.Empty:
				break;
			case EnumeratorType.Enumerable:
				m_Enumerator.Reset();
				break;
			case EnumeratorType.List:
				((IEnumerator)m_Properties).Reset();
				break;
			case EnumeratorType.IndexedCollectionPropertyBag:
				m_IndexedCollectionPropertyBag.Reset();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public void Dispose()
		{
			switch (m_Type)
			{
			case EnumeratorType.Empty:
				break;
			case EnumeratorType.Enumerable:
				m_Enumerator.Dispose();
				break;
			case EnumeratorType.List:
				break;
			case EnumeratorType.IndexedCollectionPropertyBag:
				m_IndexedCollectionPropertyBag.Dispose();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private readonly EnumeratorType m_Type;

	private readonly IEnumerable<IProperty<TContainer>> m_Enumerable;

	private readonly List<IProperty<TContainer>> m_Properties;

	private readonly IndexedCollectionPropertyBagEnumerable<TContainer> m_IndexedCollectionPropertyBag;

	public static PropertyCollection<TContainer> Empty { get; } = default(PropertyCollection<TContainer>);

	public PropertyCollection(IEnumerable<IProperty<TContainer>> enumerable)
	{
		m_Type = EnumeratorType.Enumerable;
		m_Enumerable = enumerable;
		m_Properties = null;
		m_IndexedCollectionPropertyBag = default(IndexedCollectionPropertyBagEnumerable<TContainer>);
	}

	public PropertyCollection(List<IProperty<TContainer>> properties)
	{
		m_Type = EnumeratorType.List;
		m_Enumerable = null;
		m_Properties = properties;
		m_IndexedCollectionPropertyBag = default(IndexedCollectionPropertyBagEnumerable<TContainer>);
	}

	internal PropertyCollection(IndexedCollectionPropertyBagEnumerable<TContainer> enumerable)
	{
		m_Type = EnumeratorType.IndexedCollectionPropertyBag;
		m_Enumerable = null;
		m_Properties = null;
		m_IndexedCollectionPropertyBag = enumerable;
	}

	public Enumerator GetEnumerator()
	{
		return m_Type switch
		{
			EnumeratorType.Empty => default(Enumerator), 
			EnumeratorType.Enumerable => new Enumerator(m_Enumerable.GetEnumerator()), 
			EnumeratorType.List => new Enumerator(m_Properties.GetEnumerator()), 
			EnumeratorType.IndexedCollectionPropertyBag => new Enumerator(m_IndexedCollectionPropertyBag.GetEnumerator()), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	IEnumerator<IProperty<TContainer>> IEnumerable<IProperty<TContainer>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
