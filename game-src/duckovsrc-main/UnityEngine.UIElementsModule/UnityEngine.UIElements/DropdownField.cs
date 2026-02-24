using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class DropdownField : PopupField<string>
{
	public new class UxmlFactory : UxmlFactory<DropdownField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<string>.UxmlTraits
	{
		private UxmlIntAttributeDescription m_Index = new UxmlIntAttributeDescription
		{
			name = "index",
			defaultValue = -1
		};

		private UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription
		{
			name = "choices"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			DropdownField dropdownField = (DropdownField)ve;
			List<string> list = BaseField<string>.UxmlTraits.ParseChoiceList(m_Choices.GetValueFromBag(bag, cc));
			if (list != null)
			{
				dropdownField.choices = list;
			}
			dropdownField.index = m_Index.GetValueFromBag(bag, cc);
		}
	}

	public DropdownField()
		: this(null)
	{
	}

	public DropdownField(string label)
		: base(label)
	{
	}

	public DropdownField(List<string> choices, string defaultValue, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: this(null, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public DropdownField(string label, List<string> choices, string defaultValue, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: base(label, choices, defaultValue, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public DropdownField(List<string> choices, int defaultIndex, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: this(null, choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
	{
	}

	public DropdownField(string label, List<string> choices, int defaultIndex, Func<string, string> formatSelectedValueCallback = null, Func<string, string> formatListItemCallback = null)
		: base(label, choices, defaultIndex, formatSelectedValueCallback, formatListItemCallback)
	{
	}
}
