using System.Collections.Generic;

namespace Unity.Properties.Internal;

internal readonly struct ReadOnlyAdapterCollection
{
	public struct Enumerator
	{
		private List<IPropertyVisitorAdapter> m_Adapters;

		private int m_Index;

		public IPropertyVisitorAdapter Current { get; private set; }

		public Enumerator(ReadOnlyAdapterCollection collection)
		{
			m_Adapters = collection.m_Adapters;
			m_Index = 0;
			Current = null;
		}

		public bool MoveNext()
		{
			if (m_Adapters == null)
			{
				return false;
			}
			if (m_Index >= m_Adapters.Count)
			{
				return false;
			}
			Current = m_Adapters[m_Index];
			m_Index++;
			return true;
		}

		private void Reset()
		{
			m_Index = 0;
			Current = null;
		}
	}

	private readonly List<IPropertyVisitorAdapter> m_Adapters;

	public ReadOnlyAdapterCollection(List<IPropertyVisitorAdapter> adapters)
	{
		m_Adapters = adapters;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
