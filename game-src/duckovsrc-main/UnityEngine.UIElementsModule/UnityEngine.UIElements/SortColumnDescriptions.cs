using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

[Serializable]
public class SortColumnDescriptions : ICollection<SortColumnDescription>, IEnumerable<SortColumnDescription>, IEnumerable
{
	internal class UxmlObjectFactory<T> : UxmlObjectFactory<T, UxmlObjectTraits<T>> where T : SortColumnDescriptions, new()
	{
	}

	internal class UxmlObjectTraits<T> : UnityEngine.UIElements.UxmlObjectTraits<T> where T : SortColumnDescriptions
	{
		private readonly UxmlObjectListAttributeDescription<SortColumnDescription> m_SortColumnDescriptions = new UxmlObjectListAttributeDescription<SortColumnDescription>();

		public override void Init(ref T obj, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ref obj, bag, cc);
			List<SortColumnDescription> valueFromBag = m_SortColumnDescriptions.GetValueFromBag(bag, cc);
			if (valueFromBag == null)
			{
				return;
			}
			foreach (SortColumnDescription item in valueFromBag)
			{
				obj.Add(item);
			}
		}
	}

	[SerializeField]
	private readonly IList<SortColumnDescription> m_Descriptions = new List<SortColumnDescription>();

	public int Count => m_Descriptions.Count;

	public bool IsReadOnly => m_Descriptions.IsReadOnly;

	public SortColumnDescription this[int index] => m_Descriptions[index];

	internal event Action changed;

	public IEnumerator<SortColumnDescription> GetEnumerator()
	{
		return m_Descriptions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(SortColumnDescription item)
	{
		Insert(m_Descriptions.Count, item);
	}

	public void Clear()
	{
		while (m_Descriptions.Count > 0)
		{
			Remove(m_Descriptions[0]);
		}
	}

	public bool Contains(SortColumnDescription item)
	{
		return m_Descriptions.Contains(item);
	}

	public void CopyTo(SortColumnDescription[] array, int arrayIndex)
	{
		m_Descriptions.CopyTo(array, arrayIndex);
	}

	public bool Remove(SortColumnDescription desc)
	{
		if (desc == null)
		{
			throw new ArgumentException("Cannot remove null description");
		}
		if (m_Descriptions.Remove(desc))
		{
			desc.column = null;
			desc.changed -= OnDescriptionChanged;
			this.changed?.Invoke();
			return true;
		}
		return false;
	}

	private void OnDescriptionChanged(SortColumnDescription desc)
	{
		this.changed?.Invoke();
	}

	public int IndexOf(SortColumnDescription desc)
	{
		return m_Descriptions.IndexOf(desc);
	}

	public void Insert(int index, SortColumnDescription desc)
	{
		if (desc == null)
		{
			throw new ArgumentException("Cannot insert null description");
		}
		if (Contains(desc))
		{
			throw new ArgumentException("Already contains this description");
		}
		m_Descriptions.Insert(index, desc);
		desc.changed += OnDescriptionChanged;
		this.changed?.Invoke();
	}

	public void RemoveAt(int index)
	{
		Remove(m_Descriptions[index]);
	}
}
