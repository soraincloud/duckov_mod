using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class Columns : ICollection<Column>, IEnumerable<Column>, IEnumerable
{
	public enum StretchMode
	{
		Grow,
		GrowAndFill
	}

	internal class UxmlObjectFactory<T> : UxmlObjectFactory<T, UxmlObjectTraits<T>> where T : Columns, new()
	{
	}

	internal class UxmlObjectTraits<T> : UnityEngine.UIElements.UxmlObjectTraits<T> where T : Columns
	{
		private readonly UxmlStringAttributeDescription m_PrimaryColumnName = new UxmlStringAttributeDescription
		{
			name = "primary-column-name"
		};

		private readonly UxmlEnumAttributeDescription<StretchMode> m_StretchMode = new UxmlEnumAttributeDescription<StretchMode>
		{
			name = "stretch-mode",
			defaultValue = StretchMode.GrowAndFill
		};

		private readonly UxmlBoolAttributeDescription m_Reorderable = new UxmlBoolAttributeDescription
		{
			name = "reorderable",
			defaultValue = true
		};

		private readonly UxmlBoolAttributeDescription m_Resizable = new UxmlBoolAttributeDescription
		{
			name = "resizable",
			defaultValue = true
		};

		private readonly UxmlBoolAttributeDescription m_ResizePreview = new UxmlBoolAttributeDescription
		{
			name = "resize-preview"
		};

		private readonly UxmlObjectListAttributeDescription<Column> m_Columns = new UxmlObjectListAttributeDescription<Column>();

		public override void Init(ref T obj, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ref obj, bag, cc);
			string valueFromBag = m_PrimaryColumnName.GetValueFromBag(bag, cc);
			obj.primaryColumnName = valueFromBag;
			StretchMode valueFromBag2 = m_StretchMode.GetValueFromBag(bag, cc);
			obj.stretchMode = valueFromBag2;
			bool valueFromBag3 = m_Reorderable.GetValueFromBag(bag, cc);
			obj.reorderable = valueFromBag3;
			bool valueFromBag4 = m_Resizable.GetValueFromBag(bag, cc);
			obj.resizable = valueFromBag4;
			bool valueFromBag5 = m_ResizePreview.GetValueFromBag(bag, cc);
			obj.resizePreview = valueFromBag5;
			List<Column> valueFromBag6 = m_Columns.GetValueFromBag(bag, cc);
			if (valueFromBag6 == null)
			{
				return;
			}
			foreach (Column item in valueFromBag6)
			{
				obj.Add(item);
			}
		}
	}

	private IList<Column> m_Columns = new List<Column>();

	private List<Column> m_DisplayColumns;

	private List<Column> m_VisibleColumns;

	private bool m_VisibleColumnsDirty = true;

	private StretchMode m_StretchMode = StretchMode.GrowAndFill;

	private bool m_Reorderable = true;

	private bool m_Resizable = true;

	private bool m_ResizePreview;

	private string m_PrimaryColumnName;

	internal IList<Column> columns => m_Columns;

	public string primaryColumnName
	{
		get
		{
			return m_PrimaryColumnName;
		}
		set
		{
			if (!(m_PrimaryColumnName == value))
			{
				m_PrimaryColumnName = value;
				NotifyChange(ColumnsDataType.PrimaryColumn);
			}
		}
	}

	public bool reorderable
	{
		get
		{
			return m_Reorderable;
		}
		set
		{
			if (m_Reorderable != value)
			{
				m_Reorderable = value;
				NotifyChange(ColumnsDataType.Reorderable);
			}
		}
	}

	public bool resizable
	{
		get
		{
			return m_Resizable;
		}
		set
		{
			if (m_Resizable != value)
			{
				m_Resizable = value;
				NotifyChange(ColumnsDataType.Resizable);
			}
		}
	}

	public bool resizePreview
	{
		get
		{
			return m_ResizePreview;
		}
		set
		{
			if (m_ResizePreview != value)
			{
				m_ResizePreview = value;
				NotifyChange(ColumnsDataType.ResizePreview);
			}
		}
	}

	internal IEnumerable<Column> displayList
	{
		get
		{
			InitOrderColumns();
			return m_DisplayColumns;
		}
	}

	internal IEnumerable<Column> visibleList
	{
		get
		{
			UpdateVisibleColumns();
			return m_VisibleColumns;
		}
	}

	public StretchMode stretchMode
	{
		get
		{
			return m_StretchMode;
		}
		set
		{
			if (m_StretchMode != value)
			{
				m_StretchMode = value;
				NotifyChange(ColumnsDataType.StretchMode);
			}
		}
	}

	public int Count => m_Columns.Count;

	public bool IsReadOnly => m_Columns.IsReadOnly;

	public Column this[int index] => m_Columns[index];

	public Column this[string name]
	{
		get
		{
			foreach (Column column in m_Columns)
			{
				if (column.name == name)
				{
					return column;
				}
			}
			return null;
		}
	}

	internal event Action<ColumnsDataType> changed;

	internal event Action<Column, int> columnAdded;

	internal event Action<Column> columnRemoved;

	internal event Action<Column, ColumnDataType> columnChanged;

	internal event Action<Column> columnResized;

	internal event Action<Column, int, int> columnReordered;

	public bool IsPrimary(Column column)
	{
		return primaryColumnName == column.name || (string.IsNullOrEmpty(primaryColumnName) && column.visibleIndex == 0);
	}

	public IEnumerator<Column> GetEnumerator()
	{
		return m_Columns.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(Column item)
	{
		Insert(m_Columns.Count, item);
	}

	public void Clear()
	{
		while (m_Columns.Count > 0)
		{
			Remove(m_Columns[m_Columns.Count - 1]);
		}
	}

	public bool Contains(Column item)
	{
		return m_Columns.Contains(item);
	}

	public bool Contains(string name)
	{
		foreach (Column column in m_Columns)
		{
			if (column.name == name)
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(Column[] array, int arrayIndex)
	{
		m_Columns.CopyTo(array, arrayIndex);
	}

	public bool Remove(Column column)
	{
		if (column == null)
		{
			throw new ArgumentException("Cannot remove null column");
		}
		if (m_Columns.Remove(column))
		{
			m_DisplayColumns?.Remove(column);
			m_VisibleColumns?.Remove(column);
			column.collection = null;
			column.changed -= OnColumnChanged;
			column.resized -= OnColumnResized;
			this.columnRemoved?.Invoke(column);
			return true;
		}
		return false;
	}

	private void OnColumnChanged(Column column, ColumnDataType type)
	{
		if (type == ColumnDataType.Visibility)
		{
			DirtyVisibleColumns();
		}
		this.columnChanged?.Invoke(column, type);
	}

	private void OnColumnResized(Column column)
	{
		this.columnResized?.Invoke(column);
	}

	public int IndexOf(Column column)
	{
		return m_Columns.IndexOf(column);
	}

	public void Insert(int index, Column column)
	{
		if (column == null)
		{
			throw new ArgumentException("Cannot insert null column");
		}
		if (column.collection == this)
		{
			throw new ArgumentException("Already contains this column");
		}
		if (column.collection != null)
		{
			column.collection.Remove(column);
		}
		m_Columns.Insert(index, column);
		if (m_DisplayColumns != null)
		{
			m_DisplayColumns.Insert(index, column);
			DirtyVisibleColumns();
		}
		column.collection = this;
		column.changed += OnColumnChanged;
		column.resized += OnColumnResized;
		this.columnAdded?.Invoke(column, index);
	}

	public void RemoveAt(int index)
	{
		Remove(m_Columns[index]);
	}

	public void ReorderDisplay(int from, int to)
	{
		InitOrderColumns();
		Column column = m_DisplayColumns[from];
		m_DisplayColumns.RemoveAt(from);
		m_DisplayColumns.Insert(to, column);
		DirtyVisibleColumns();
		this.columnReordered?.Invoke(column, from, to);
	}

	private void InitOrderColumns()
	{
		if (m_DisplayColumns == null)
		{
			m_DisplayColumns = new List<Column>(this);
		}
	}

	private void DirtyVisibleColumns()
	{
		m_VisibleColumnsDirty = true;
		if (m_VisibleColumns != null)
		{
			m_VisibleColumns.Clear();
		}
	}

	private void UpdateVisibleColumns()
	{
		if (m_VisibleColumnsDirty)
		{
			InitOrderColumns();
			if (m_VisibleColumns == null)
			{
				m_VisibleColumns = new List<Column>(m_Columns.Count);
			}
			m_VisibleColumns.AddRange(m_DisplayColumns.FindAll((Column c) => c.visible));
			m_VisibleColumnsDirty = false;
		}
	}

	private void NotifyChange(ColumnsDataType type)
	{
		this.changed?.Invoke(type);
	}
}
