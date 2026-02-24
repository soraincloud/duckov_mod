namespace Unity.Properties;

internal readonly struct IndexedCollectionPropertyBagEnumerable<TContainer>
{
	private readonly IIndexedCollectionPropertyBagEnumerator<TContainer> m_Impl;

	private readonly TContainer m_Container;

	public IndexedCollectionPropertyBagEnumerable(IIndexedCollectionPropertyBagEnumerator<TContainer> impl, TContainer container)
	{
		m_Impl = impl;
		m_Container = container;
	}

	public IndexedCollectionPropertyBagEnumerator<TContainer> GetEnumerator()
	{
		return new IndexedCollectionPropertyBagEnumerator<TContainer>(m_Impl, m_Container);
	}
}
