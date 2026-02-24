using System;

namespace UnityEngine.UIElements;

public class TextField : TextInputBaseField<string>
{
	public new class UxmlFactory : UxmlFactory<TextField, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextInputBaseField<string>.UxmlTraits
	{
		private static readonly UxmlStringAttributeDescription k_Value = new UxmlStringAttributeDescription
		{
			name = "value",
			obsoleteNames = new string[1] { "text" }
		};

		private UxmlBoolAttributeDescription m_Multiline = new UxmlBoolAttributeDescription
		{
			name = "multiline"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			TextField textField = (TextField)ve;
			textField.multiline = m_Multiline.GetValueFromBag(bag, cc);
			base.Init(ve, bag, cc);
			string value = string.Empty;
			if (k_Value.TryGetValueFromBag(bag, cc, ref value))
			{
				textField.SetValueWithoutNotify(value);
			}
		}
	}

	private class TextInput : TextInputBase
	{
		private TextField parentTextField => (TextField)base.parent;

		public bool multiline
		{
			get
			{
				return base.textEdition.multiline;
			}
			set
			{
				if (base.textEdition.multiline != value)
				{
					base.textEdition.multiline = value;
					if (value)
					{
						SetMultiline();
						return;
					}
					base.text = base.text.Replace("\n", "");
					SetSingleLine();
				}
			}
		}

		public override bool isPasswordField
		{
			set
			{
				base.isPasswordField = value;
				if (value)
				{
					multiline = false;
				}
			}
		}

		protected override string StringToValue(string str)
		{
			return str;
		}
	}

	public new static readonly string ussClassName = "unity-text-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private TextInput textInput => (TextInput)base.textInputBase;

	public bool multiline
	{
		get
		{
			return textInput.multiline;
		}
		set
		{
			textInput.multiline = value;
		}
	}

	public override string value
	{
		get
		{
			return base.value;
		}
		set
		{
			base.value = value;
			base.textEdition.UpdateText(base.rawValue);
		}
	}

	public TextField()
		: this(null)
	{
	}

	public TextField(int maxLength, bool multiline, bool isPasswordField, char maskChar)
		: this(null, maxLength, multiline, isPasswordField, maskChar)
	{
	}

	public TextField(string label)
		: this(label, -1, multiline: false, isPasswordField: false, '*')
	{
	}

	public TextField(string label, int maxLength, bool multiline, bool isPasswordField, char maskChar)
		: base(label, maxLength, maskChar, (TextInputBase)new TextInput())
	{
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.pickingMode = PickingMode.Ignore;
		SetValueWithoutNotify("");
		this.multiline = multiline;
		base.isPasswordField = isPasswordField;
	}

	public override void SetValueWithoutNotify(string newValue)
	{
		base.SetValueWithoutNotify(newValue);
		((INotifyValueChanged<string>)textInput.textElement).SetValueWithoutNotify(base.rawValue);
	}

	internal override void UpdateTextFromValue()
	{
		SetValueWithoutNotify(base.rawValue);
	}

	[EventInterest(new Type[] { typeof(BlurEvent) })]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (base.isDelayed && evt?.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			value = base.text;
		}
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		base.text = base.rawValue;
	}

	protected override string ValueToString(string value)
	{
		return value;
	}

	protected override string StringToValue(string str)
	{
		return str;
	}
}
