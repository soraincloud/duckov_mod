using System;
using System.Collections.ObjectModel;

namespace Unity.VisualScripting;

public class WatchedList<T> : Collection<T>, INotifyCollectionChanged<T>
{
	public event Action<T> ItemAdded;

	public event Action<T> ItemRemoved;

	public event Action CollectionChanged;

	protected override void InsertItem(int index, T item)
	{
		base.InsertItem(index, item);
		this.ItemAdded?.Invoke(item);
		this.CollectionChanged?.Invoke();
	}

	protected override void RemoveItem(int index)
	{
		if (index < base.Count)
		{
			T obj = base[index];
			base.RemoveItem(index);
			this.ItemRemoved?.Invoke(obj);
			this.CollectionChanged?.Invoke();
		}
	}

	protected override void ClearItems()
	{
		while (base.Count > 0)
		{
			RemoveItem(0);
		}
	}
}
