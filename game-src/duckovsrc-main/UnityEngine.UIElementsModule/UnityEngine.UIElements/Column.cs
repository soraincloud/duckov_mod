using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class Column
{
	internal class UxmlObjectFactory<T> : UxmlObjectFactory<T, UxmlObjectTraits<T>> where T : Column, new()
	{
	}

	internal class UxmlObjectTraits<T> : UnityEngine.UIElements.UxmlObjectTraits<T> where T : Column
	{
		internal const string k_HeaderTemplateAttributeName = "header-template";

		internal const string k_CellTemplateAttributeName = "cell-template";

		private UxmlStringAttributeDescription m_Name = new UxmlStringAttributeDescription
		{
			name = "name"
		};

		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "title"
		};

		private UxmlBoolAttributeDescription m_Visible = new UxmlBoolAttributeDescription
		{
			name = "visible",
			defaultValue = true
		};

		private UxmlStringAttributeDescription m_Width = new UxmlStringAttributeDescription
		{
			name = "width"
		};

		private UxmlStringAttributeDescription m_MinWidth = new UxmlStringAttributeDescription
		{
			name = "min-width"
		};

		private UxmlStringAttributeDescription m_MaxWidth = new UxmlStringAttributeDescription
		{
			name = "max-width"
		};

		private UxmlBoolAttributeDescription m_Stretch = new UxmlBoolAttributeDescription
		{
			name = "stretchable"
		};

		private UxmlBoolAttributeDescription m_Sortable = new UxmlBoolAttributeDescription
		{
			name = "sortable",
			defaultValue = true
		};

		private UxmlBoolAttributeDescription m_Optional = new UxmlBoolAttributeDescription
		{
			name = "optional",
			defaultValue = true
		};

		private UxmlBoolAttributeDescription m_Resizable = new UxmlBoolAttributeDescription
		{
			name = "resizable",
			defaultValue = true
		};

		private UxmlStringAttributeDescription m_HeaderTemplateId = new UxmlStringAttributeDescription
		{
			name = "header-template"
		};

		private UxmlStringAttributeDescription m_CellTemplateId = new UxmlStringAttributeDescription
		{
			name = "cell-template"
		};

		private static Length ParseLength(string str, Length defaultValue)
		{
			float value = defaultValue.value;
			LengthUnit unit = defaultValue.unit;
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (char.IsLetter(c) || c == '%')
				{
					num2 = i;
					break;
				}
				num++;
			}
			string s = str.Substring(0, num);
			string text = string.Empty;
			if (num2 > 0)
			{
				text = str.Substring(num2, str.Length - num2).ToLowerInvariant();
			}
			if (float.TryParse(s, out var result))
			{
				value = result;
			}
			string text2 = text;
			string text3 = text2;
			if (!(text3 == "px"))
			{
				if (text3 == "%")
				{
					unit = LengthUnit.Percent;
				}
			}
			else
			{
				unit = LengthUnit.Pixel;
			}
			return new Length(value, unit);
		}

		public override void Init(ref T obj, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ref obj, bag, cc);
			string valueFromBag = m_Name.GetValueFromBag(bag, cc);
			obj.name = valueFromBag;
			string valueFromBag2 = m_Text.GetValueFromBag(bag, cc);
			obj.title = valueFromBag2;
			bool valueFromBag3 = m_Visible.GetValueFromBag(bag, cc);
			obj.visible = valueFromBag3;
			Length width = ParseLength(m_Width.GetValueFromBag(bag, cc), default(Length));
			obj.width = width;
			Length maxWidth = ParseLength(m_MaxWidth.GetValueFromBag(bag, cc), new Length(8388608f));
			obj.maxWidth = maxWidth;
			Length minWidth = ParseLength(m_MinWidth.GetValueFromBag(bag, cc), new Length(35f));
			obj.minWidth = minWidth;
			bool valueFromBag4 = m_Sortable.GetValueFromBag(bag, cc);
			obj.sortable = valueFromBag4;
			bool valueFromBag5 = m_Stretch.GetValueFromBag(bag, cc);
			obj.stretchable = valueFromBag5;
			bool valueFromBag6 = m_Optional.GetValueFromBag(bag, cc);
			obj.optional = valueFromBag6;
			bool valueFromBag7 = m_Resizable.GetValueFromBag(bag, cc);
			obj.resizable = valueFromBag7;
			string valueFromBag8 = m_HeaderTemplateId.GetValueFromBag(bag, cc);
			if (!string.IsNullOrEmpty(valueFromBag8))
			{
				VisualTreeAsset asset = cc.visualTreeAsset?.ResolveTemplate(valueFromBag8);
				Func<VisualElement> makeHeader = () => (asset != null) ? ((BindableElement)asset.Instantiate()) : ((BindableElement)new Label(k_InvalidTemplateError));
				obj.makeHeader = makeHeader;
			}
			string valueFromBag9 = m_CellTemplateId.GetValueFromBag(bag, cc);
			if (!string.IsNullOrEmpty(valueFromBag9))
			{
				VisualTreeAsset asset2 = cc.visualTreeAsset?.ResolveTemplate(valueFromBag9);
				Func<VisualElement> makeCell = () => (asset2 != null) ? ((BindableElement)asset2.Instantiate()) : ((BindableElement)new Label(k_InvalidTemplateError));
				obj.makeCell = makeCell;
			}
		}
	}

	internal const float kDefaultMinWidth = 35f;

	private static readonly string k_InvalidTemplateError = "Not Found";

	private string m_Name;

	private string m_Title;

	private Background m_Icon;

	private bool m_Visible = true;

	private Length m_Width = 0f;

	private Length m_MinWidth = 35f;

	private Length m_MaxWidth = 8388608f;

	private float m_DesiredWidth = float.NaN;

	private bool m_Stretchable;

	private bool m_Sortable = true;

	private bool m_Optional = true;

	private bool m_Resizable = true;

	private Func<VisualElement> m_MakeHeader;

	private Action<VisualElement> m_BindHeader;

	private Action<VisualElement> m_UnbindHeader;

	private Action<VisualElement> m_DestroyHeader;

	private Func<VisualElement> m_MakeCell;

	private Action<VisualElement, int> m_BindCell;

	private Action<VisualElement, int> m_UnbindCellItem;

	public string name
	{
		get
		{
			return m_Name;
		}
		set
		{
			if (!(m_Name == value))
			{
				m_Name = value;
				NotifyChange(ColumnDataType.Name);
			}
		}
	}

	public string title
	{
		get
		{
			return m_Title;
		}
		set
		{
			if (!(m_Title == value))
			{
				m_Title = value;
				NotifyChange(ColumnDataType.Title);
			}
		}
	}

	public Background icon
	{
		get
		{
			return m_Icon;
		}
		set
		{
			if (!(m_Icon == value))
			{
				m_Icon = value;
				NotifyChange(ColumnDataType.Icon);
			}
		}
	}

	internal int index => collection?.IndexOf(this) ?? (-1);

	internal int displayIndex => (collection?.displayList as List<Column>)?.IndexOf(this) ?? (-1);

	internal int visibleIndex => (collection?.visibleList as List<Column>)?.IndexOf(this) ?? (-1);

	public bool visible
	{
		get
		{
			return m_Visible;
		}
		set
		{
			if (m_Visible != value)
			{
				m_Visible = value;
				NotifyChange(ColumnDataType.Visibility);
			}
		}
	}

	public Length width
	{
		get
		{
			return m_Width;
		}
		set
		{
			if (!(m_Width == value))
			{
				m_Width = value;
				desiredWidth = float.NaN;
				NotifyChange(ColumnDataType.Width);
			}
		}
	}

	public Length minWidth
	{
		get
		{
			return m_MinWidth;
		}
		set
		{
			if (!(m_MinWidth == value))
			{
				m_MinWidth = value;
				NotifyChange(ColumnDataType.MinWidth);
			}
		}
	}

	public Length maxWidth
	{
		get
		{
			return m_MaxWidth;
		}
		set
		{
			if (!(m_MaxWidth == value))
			{
				m_MaxWidth = value;
				NotifyChange(ColumnDataType.MaxWidth);
			}
		}
	}

	internal float desiredWidth
	{
		get
		{
			return m_DesiredWidth;
		}
		set
		{
			if (m_DesiredWidth != value)
			{
				m_DesiredWidth = value;
				this.resized?.Invoke(this);
			}
		}
	}

	public bool sortable
	{
		get
		{
			return m_Sortable;
		}
		set
		{
			if (m_Sortable != value)
			{
				m_Sortable = value;
				NotifyChange(ColumnDataType.Sortable);
			}
		}
	}

	public bool stretchable
	{
		get
		{
			return m_Stretchable;
		}
		set
		{
			if (m_Stretchable != value)
			{
				m_Stretchable = value;
				NotifyChange(ColumnDataType.Stretchable);
			}
		}
	}

	public bool optional
	{
		get
		{
			return m_Optional;
		}
		set
		{
			if (m_Optional != value)
			{
				m_Optional = value;
				NotifyChange(ColumnDataType.Optional);
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
				NotifyChange(ColumnDataType.Resizable);
			}
		}
	}

	public Func<VisualElement> makeHeader
	{
		get
		{
			return m_MakeHeader;
		}
		set
		{
			if (m_MakeHeader != value)
			{
				m_MakeHeader = value;
				NotifyChange(ColumnDataType.HeaderTemplate);
			}
		}
	}

	public Action<VisualElement> bindHeader
	{
		get
		{
			return m_BindHeader;
		}
		set
		{
			if (m_BindHeader != value)
			{
				m_BindHeader = value;
				NotifyChange(ColumnDataType.HeaderTemplate);
			}
		}
	}

	public Action<VisualElement> unbindHeader
	{
		get
		{
			return m_UnbindHeader;
		}
		set
		{
			if (m_UnbindHeader != value)
			{
				m_UnbindHeader = value;
				NotifyChange(ColumnDataType.HeaderTemplate);
			}
		}
	}

	public Action<VisualElement> destroyHeader
	{
		get
		{
			return m_DestroyHeader;
		}
		set
		{
			if (m_DestroyHeader != value)
			{
				m_DestroyHeader = value;
				NotifyChange(ColumnDataType.HeaderTemplate);
			}
		}
	}

	public Func<VisualElement> makeCell
	{
		get
		{
			return m_MakeCell;
		}
		set
		{
			if (m_MakeCell != value)
			{
				m_MakeCell = value;
				NotifyChange(ColumnDataType.CellTemplate);
			}
		}
	}

	public Action<VisualElement, int> bindCell
	{
		get
		{
			return m_BindCell;
		}
		set
		{
			if (m_BindCell != value)
			{
				m_BindCell = value;
				NotifyChange(ColumnDataType.CellTemplate);
			}
		}
	}

	public Action<VisualElement, int> unbindCell
	{
		get
		{
			return m_UnbindCellItem;
		}
		set
		{
			if (m_UnbindCellItem != value)
			{
				m_UnbindCellItem = value;
				NotifyChange(ColumnDataType.CellTemplate);
			}
		}
	}

	public Action<VisualElement> destroyCell { get; set; }

	public Columns collection { get; internal set; }

	internal event Action<Column, ColumnDataType> changed;

	internal event Action<Column> resized;

	private void NotifyChange(ColumnDataType type)
	{
		this.changed?.Invoke(this, type);
	}

	internal float GetWidth(float layoutWidth)
	{
		return (width.unit == LengthUnit.Pixel) ? width.value : (width.value * layoutWidth / 100f);
	}

	internal float GetMaxWidth(float layoutWidth)
	{
		return (maxWidth.unit == LengthUnit.Pixel) ? maxWidth.value : (maxWidth.value * layoutWidth / 100f);
	}

	internal float GetMinWidth(float layoutWidth)
	{
		return (minWidth.unit == LengthUnit.Pixel) ? minWidth.value : (minWidth.value * layoutWidth / 100f);
	}
}
