using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public class Hash128Field : TextInputBaseField<Hash128>
{
	public new class UxmlFactory : UxmlFactory<Hash128Field, UxmlTraits>
	{
	}

	public new class UxmlTraits : TextValueFieldTraits<Hash128, UxmlHash128AttributeDescription>
	{
	}

	private class Hash128Input : TextInputBase
	{
		private Hash128Field hash128Field => (Hash128Field)base.parent;

		protected string allowedCharacters => "0123456789abcdefABCDEF";

		public string formatString => UINumericFieldsUtils.k_IntFieldFormatString;

		internal Hash128Input()
		{
			base.textEdition.AcceptCharacter = AcceptCharacter;
		}

		internal override bool AcceptCharacter(char c)
		{
			return base.AcceptCharacter(c) && c != 0 && allowedCharacters.IndexOf(c) != -1;
		}

		protected string ValueToString(Hash128 value)
		{
			return value.ToString();
		}

		protected override Hash128 StringToValue(string str)
		{
			return Parse(str);
		}

		internal static Hash128 Parse(string str)
		{
			if (str.Length == 1 && ulong.TryParse(str, out var result))
			{
				return new Hash128(result, 0uL);
			}
			return Hash128.Parse(str);
		}
	}

	public new static readonly string ussClassName = "unity-hash128-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	private Hash128Input integerInput => (Hash128Input)base.textInputBase;

	public override Hash128 value
	{
		get
		{
			return base.value;
		}
		set
		{
			base.value = value;
			if (m_UpdateTextFromValue)
			{
				base.text = base.rawValue.ToString();
			}
		}
	}

	public Hash128Field()
		: this(null)
	{
	}

	public Hash128Field(int maxLength)
		: this(null, maxLength)
	{
	}

	public Hash128Field(string label, int maxLength = -1)
		: base(label, maxLength, '\0', (TextInputBase)new Hash128Input())
	{
		SetValueWithoutNotify(default(Hash128));
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
	}

	internal override void UpdateValueFromText()
	{
		m_UpdateTextFromValue = false;
		try
		{
			value = StringToValue(base.text);
		}
		finally
		{
			m_UpdateTextFromValue = true;
		}
	}

	internal override void UpdateTextFromValue()
	{
		base.text = ValueToString(base.rawValue);
	}

	public override void SetValueWithoutNotify(Hash128 newValue)
	{
		base.SetValueWithoutNotify(newValue);
		if (m_UpdateTextFromValue)
		{
			base.text = base.rawValue.ToString();
		}
	}

	protected override string ValueToString(Hash128 value)
	{
		return value.ToString();
	}

	protected override Hash128 StringToValue(string str)
	{
		return Hash128Input.Parse(str);
	}

	[EventInterest(new Type[] { typeof(BlurEvent) })]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt != null && !base.textEdition.isReadOnly && evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			if (string.IsNullOrEmpty(base.text))
			{
				value = default(Hash128);
				return;
			}
			base.textInputBase.UpdateValueFromText();
			base.textInputBase.UpdateTextFromValue();
		}
	}
}
