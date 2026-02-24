using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class PopupField<T> : BasePopupField<T, T>
{
	internal const int kPopupFieldDefaultIndex = -1;

	private int m_Index = -1;

	public new static readonly string ussClassName = "unity-popup-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public virtual Func<T, string> formatSelectedValueCallback
	{
		get
		{
			return m_FormatSelectedValueCallback;
		}
		set
		{
			m_FormatSelectedValueCallback = value;
			base.textElement.text = GetValueToDisplay();
		}
	}

	public virtual Func<T, string> formatListItemCallback
	{
		get
		{
			return m_FormatListItemCallback;
		}
		set
		{
			m_FormatListItemCallback = value;
		}
	}

	public override T value
	{
		get
		{
			return base.value;
		}
		set
		{
			m_Index = m_Choices?.IndexOf(value) ?? (-1);
			base.value = value;
		}
	}

	public int index
	{
		get
		{
			return m_Index;
		}
		set
		{
			if (value != m_Index)
			{
				m_Index = value;
				if (m_Index >= 0 && m_Index < m_Choices.Count)
				{
					this.value = m_Choices[m_Index];
				}
				else
				{
					this.value = default(T);
				}
			}
		}
	}

	internal override string GetValueToDisplay()
	{
		if (m_FormatSelectedValueCallback != null)
		{
			return m_FormatSelectedValueCallback(value);
		}
		if (value != null)
		{
			return UIElementsUtility.ParseMenuName(value.ToString());
		}
		return string.Empty;
	}

	internal override string GetListItemToDisplay(T value)
	{
		if (m_FormatListItemCallback != null)
		{
			return m_FormatListItemCallback(value);
		}
		return (value != null && m_Choices.Contains(value)) ? value.ToString() : string.Empty;
	}

	public override void SetValueWithoutNotify(T newValue)
	{
		m_Index = m_Choices?.IndexOf(newValue) ?? (-1);
		base.SetValueWithoutNotify(newValue);
	}

	public PopupField()
		: this((string)null)
	{
	}

	public PopupField(string label = null)
		: base(label)
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}

	public PopupField(List<T> choices, T defaultValue, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
		: this((string)null, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public PopupField(string label, List<T> choices, T defaultValue, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
		: this(label)
	{
		if (defaultValue == null)
		{
			throw new ArgumentNullException("defaultValue");
		}
		this.choices = choices;
		if (!m_Choices.Contains(defaultValue))
		{
			throw new ArgumentException($"Default value {defaultValue} is not present in the list of possible values");
		}
		SetValueWithoutNotify(defaultValue);
		this.formatListItemCallback = formatListItemCallback;
		this.formatSelectedValueCallback = formatSelectedValueCallback;
	}

	public PopupField(List<T> choices, int defaultIndex, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
		: this((string)null, choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public PopupField(string label, List<T> choices, int defaultIndex, Func<T, string> formatSelectedValueCallback = null, Func<T, string> formatListItemCallback = null)
		: this(label)
	{
		this.choices = choices;
		SetIndexWithoutNotify(defaultIndex);
		this.formatListItemCallback = formatListItemCallback;
		this.formatSelectedValueCallback = formatSelectedValueCallback;
	}

	internal override void AddMenuItems(IGenericMenu menu)
	{
		if (menu == null)
		{
			throw new ArgumentNullException("menu");
		}
		if (m_Choices == null)
		{
			return;
		}
		foreach (T item in m_Choices)
		{
			bool isChecked = EqualityComparer<T>.Default.Equals(item, value) && !base.showMixedValue;
			menu.AddItem(GetListItemToDisplay(item), isChecked, delegate
			{
				ChangeValueFromMenu(item);
			});
		}
	}

	internal void SetIndexWithoutNotify(int index)
	{
		m_Index = index;
		if (m_Index >= 0 && m_Index < m_Choices.Count)
		{
			SetValueWithoutNotify(m_Choices[m_Index]);
		}
		else
		{
			SetValueWithoutNotify(default(T));
		}
	}

	private void ChangeValueFromMenu(T menuItem)
	{
		value = menuItem;
	}
}
