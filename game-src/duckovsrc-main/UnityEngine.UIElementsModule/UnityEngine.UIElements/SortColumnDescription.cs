using System;

namespace UnityEngine.UIElements;

[Serializable]
public class SortColumnDescription
{
	internal class UxmlObjectFactory<T> : UxmlObjectFactory<T, UxmlObjectTraits<T>> where T : SortColumnDescription, new()
	{
	}

	internal class UxmlObjectTraits<T> : UnityEngine.UIElements.UxmlObjectTraits<T> where T : SortColumnDescription
	{
		private readonly UxmlStringAttributeDescription m_ColumnName = new UxmlStringAttributeDescription
		{
			name = "column-name"
		};

		private readonly UxmlIntAttributeDescription m_ColumnIndex = new UxmlIntAttributeDescription
		{
			name = "column-index",
			defaultValue = -1
		};

		private readonly UxmlEnumAttributeDescription<SortDirection> m_SortDescription = new UxmlEnumAttributeDescription<SortDirection>
		{
			name = "direction",
			defaultValue = SortDirection.Ascending
		};

		public override void Init(ref T obj, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ref obj, bag, cc);
			string valueFromBag = m_ColumnName.GetValueFromBag(bag, cc);
			obj.columnName = valueFromBag;
			int valueFromBag2 = m_ColumnIndex.GetValueFromBag(bag, cc);
			obj.columnIndex = valueFromBag2;
			SortDirection valueFromBag3 = m_SortDescription.GetValueFromBag(bag, cc);
			obj.direction = valueFromBag3;
		}
	}

	[SerializeField]
	private int m_ColumnIndex = -1;

	[SerializeField]
	private string m_ColumnName;

	[SerializeField]
	private SortDirection m_SortDirection;

	public string columnName
	{
		get
		{
			return m_ColumnName;
		}
		set
		{
			if (!(m_ColumnName == value))
			{
				m_ColumnName = value;
				this.changed?.Invoke(this);
			}
		}
	}

	public int columnIndex
	{
		get
		{
			return m_ColumnIndex;
		}
		set
		{
			if (m_ColumnIndex != value)
			{
				m_ColumnIndex = value;
				this.changed?.Invoke(this);
			}
		}
	}

	public Column column { get; internal set; }

	public SortDirection direction
	{
		get
		{
			return m_SortDirection;
		}
		set
		{
			if (m_SortDirection != value)
			{
				m_SortDirection = value;
				this.changed?.Invoke(this);
			}
		}
	}

	internal event Action<SortColumnDescription> changed;

	public SortColumnDescription()
	{
	}

	public SortColumnDescription(int columnIndex, SortDirection direction)
	{
		this.columnIndex = columnIndex;
		this.direction = direction;
	}

	public SortColumnDescription(string columnName, SortDirection direction)
	{
		this.columnName = columnName;
		this.direction = direction;
	}
}
