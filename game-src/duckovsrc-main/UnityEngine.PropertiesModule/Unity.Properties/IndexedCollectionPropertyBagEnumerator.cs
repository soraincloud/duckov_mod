using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Properties;

internal struct IndexedCollectionPropertyBagEnumerator<TContainer> : IEnumerator<IProperty<TContainer>>, IEnumerator, IDisposable
{
	private readonly IIndexedCollectionPropertyBagEnumerator<TContainer> m_Impl;

	private readonly IndexedCollectionSharedPropertyState m_Previous;

	private TContainer m_Container;

	private int m_Position;

	public IProperty<TContainer> Current => m_Impl.GetSharedProperty();

	object IEnumerator.Current => Current;

	internal IndexedCollectionPropertyBagEnumerator(IIndexedCollectionPropertyBagEnumerator<TContainer> impl, TContainer container)
	{
		m_Impl = impl;
		m_Container = container;
		m_Previous = impl.GetSharedPropertyState();
		m_Position = -1;
	}

	public bool MoveNext()
	{
		m_Position++;
		if (m_Position < m_Impl.GetCount(ref m_Container))
		{
			m_Impl.SetSharedPropertyState(new IndexedCollectionSharedPropertyState
			{
				Index = m_Position,
				IsReadOnly = false
			});
			return true;
		}
		m_Impl.SetSharedPropertyState(m_Previous);
		return false;
	}

	public void Reset()
	{
		m_Position = -1;
		m_Impl.SetSharedPropertyState(m_Previous);
	}

	public void Dispose()
	{
	}
}
