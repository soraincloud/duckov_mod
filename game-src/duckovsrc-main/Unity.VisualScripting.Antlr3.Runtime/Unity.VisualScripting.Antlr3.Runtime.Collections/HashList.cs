using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Unity.VisualScripting.Antlr3.Runtime.Collections;

public sealed class HashList : IDictionary, ICollection, IEnumerable
{
	private sealed class HashListEnumerator : IDictionaryEnumerator, IEnumerator
	{
		internal enum EnumerationMode
		{
			Key,
			Value,
			Entry
		}

		private HashList _hashList;

		private List<object> _orderList;

		private EnumerationMode _mode;

		private int _index;

		private int _version;

		private object _key;

		private object _value;

		public object Key
		{
			get
			{
				if (_key == null)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _key;
			}
		}

		public object Value
		{
			get
			{
				if (_key == null)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return _value;
			}
		}

		public DictionaryEntry Entry
		{
			get
			{
				if (_key == null)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				return new DictionaryEntry(_key, _value);
			}
		}

		public object Current
		{
			get
			{
				if (_key == null)
				{
					throw new InvalidOperationException("Enumeration has either not started or has already finished.");
				}
				if (_mode == EnumerationMode.Key)
				{
					return _key;
				}
				if (_mode == EnumerationMode.Value)
				{
					return _value;
				}
				return new DictionaryEntry(_key, _value);
			}
		}

		internal HashListEnumerator()
		{
			_index = 0;
			_key = null;
			_value = null;
		}

		internal HashListEnumerator(HashList hashList, EnumerationMode mode)
		{
			_hashList = hashList;
			_mode = mode;
			_version = hashList._version;
			_orderList = hashList._insertionOrderList;
			_index = 0;
			_key = null;
			_value = null;
		}

		public void Reset()
		{
			if (_version != _hashList._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			_index = 0;
			_key = null;
			_value = null;
		}

		public bool MoveNext()
		{
			if (_version != _hashList._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}
			if (_index < _orderList.Count)
			{
				_key = _orderList[_index];
				_value = _hashList[_key];
				_index++;
				return true;
			}
			_key = null;
			return false;
		}
	}

	private sealed class KeyCollection : ICollection, IEnumerable
	{
		private HashList _hashList;

		public bool IsSynchronized => _hashList.IsSynchronized;

		public int Count => _hashList.Count;

		public object SyncRoot => _hashList.SyncRoot;

		internal KeyCollection()
		{
		}

		internal KeyCollection(HashList hashList)
		{
			_hashList = hashList;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			List<object> insertionOrderList = _hashList._insertionOrderList;
			for (int i = 0; i < insertionOrderList.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(insertionOrderList[i]);
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public override bool Equals(object o)
		{
			if (o is KeyCollection)
			{
				KeyCollection keyCollection = (KeyCollection)o;
				if (Count == 0 && keyCollection.Count == 0)
				{
					return true;
				}
				if (Count == keyCollection.Count)
				{
					for (int i = 0; i < Count; i++)
					{
						if (_hashList._insertionOrderList[i] == keyCollection._hashList._insertionOrderList[i] || _hashList._insertionOrderList[i].Equals(keyCollection._hashList._insertionOrderList[i]))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _hashList._insertionOrderList.GetHashCode();
		}

		public void CopyTo(Array array, int index)
		{
			_hashList.CopyKeysTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return new HashListEnumerator(_hashList, HashListEnumerator.EnumerationMode.Key);
		}
	}

	private sealed class ValueCollection : ICollection, IEnumerable
	{
		private HashList _hashList;

		public bool IsSynchronized => _hashList.IsSynchronized;

		public int Count => _hashList.Count;

		public object SyncRoot => _hashList.SyncRoot;

		internal ValueCollection()
		{
		}

		internal ValueCollection(HashList hashList)
		{
			_hashList = hashList;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			IEnumerator enumerator = new HashListEnumerator(_hashList, HashListEnumerator.EnumerationMode.Value);
			if (enumerator.MoveNext())
			{
				stringBuilder.Append((enumerator.Current == null) ? "null" : enumerator.Current);
				while (enumerator.MoveNext())
				{
					stringBuilder.Append(", ");
					stringBuilder.Append((enumerator.Current == null) ? "null" : enumerator.Current);
				}
			}
			stringBuilder.Append("]");
			return stringBuilder.ToString();
		}

		public void CopyTo(Array array, int index)
		{
			_hashList.CopyValuesTo(array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return new HashListEnumerator(_hashList, HashListEnumerator.EnumerationMode.Value);
		}
	}

	private Hashtable _dictionary = new Hashtable();

	private List<object> _insertionOrderList = new List<object>();

	private int _version;

	public bool IsReadOnly => _dictionary.IsReadOnly;

	public object this[object key]
	{
		get
		{
			return _dictionary[key];
		}
		set
		{
			bool flag = !_dictionary.Contains(key);
			_dictionary[key] = value;
			if (flag)
			{
				_insertionOrderList.Add(key);
			}
			_version++;
		}
	}

	public ICollection Values => new ValueCollection(this);

	public ICollection Keys => new KeyCollection(this);

	public bool IsFixedSize => _dictionary.IsFixedSize;

	public bool IsSynchronized => _dictionary.IsSynchronized;

	public int Count => _dictionary.Count;

	public object SyncRoot => _dictionary.SyncRoot;

	public HashList()
		: this(-1)
	{
	}

	public HashList(int capacity)
	{
		if (capacity < 0)
		{
			_dictionary = new Hashtable();
			_insertionOrderList = new List<object>();
		}
		else
		{
			_dictionary = new Hashtable(capacity);
			_insertionOrderList = new List<object>(capacity);
		}
		_version = 0;
	}

	public IDictionaryEnumerator GetEnumerator()
	{
		return new HashListEnumerator(this, HashListEnumerator.EnumerationMode.Entry);
	}

	public void Remove(object key)
	{
		_dictionary.Remove(key);
		_insertionOrderList.Remove(key);
		_version++;
	}

	public bool Contains(object key)
	{
		return _dictionary.Contains(key);
	}

	public void Clear()
	{
		_dictionary.Clear();
		_insertionOrderList.Clear();
		_version++;
	}

	public void Add(object key, object value)
	{
		_dictionary.Add(key, value);
		_insertionOrderList.Add(key);
		_version++;
	}

	public void CopyTo(Array array, int index)
	{
		int count = _insertionOrderList.Count;
		for (int i = 0; i < count; i++)
		{
			DictionaryEntry dictionaryEntry = new DictionaryEntry(_insertionOrderList[i], _dictionary[_insertionOrderList[i]]);
			array.SetValue(dictionaryEntry, index++);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new HashListEnumerator(this, HashListEnumerator.EnumerationMode.Entry);
	}

	private void CopyKeysTo(Array array, int index)
	{
		int count = _insertionOrderList.Count;
		for (int i = 0; i < count; i++)
		{
			array.SetValue(_insertionOrderList[i], index++);
		}
	}

	private void CopyValuesTo(Array array, int index)
	{
		int count = _insertionOrderList.Count;
		for (int i = 0; i < count; i++)
		{
			array.SetValue(_dictionary[_insertionOrderList[i]], index++);
		}
	}
}
