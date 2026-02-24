using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public struct StylePropertyNameCollection : IEnumerable<StylePropertyName>, IEnumerable
{
	public struct Enumerator : IEnumerator<StylePropertyName>, IEnumerator, IDisposable
	{
		private List<StylePropertyName>.Enumerator m_Enumerator;

		public StylePropertyName Current => m_Enumerator.Current;

		object IEnumerator.Current => Current;

		internal Enumerator(List<StylePropertyName>.Enumerator enumerator)
		{
			m_Enumerator = enumerator;
		}

		public bool MoveNext()
		{
			return m_Enumerator.MoveNext();
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
			m_Enumerator.Dispose();
		}
	}

	internal List<StylePropertyName> propertiesList;

	internal StylePropertyNameCollection(List<StylePropertyName> list)
	{
		propertiesList = list;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(propertiesList.GetEnumerator());
	}

	IEnumerator<StylePropertyName> IEnumerable<StylePropertyName>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(StylePropertyName stylePropertyName)
	{
		foreach (StylePropertyName properties in propertiesList)
		{
			if (properties == stylePropertyName)
			{
				return true;
			}
		}
		return false;
	}
}
