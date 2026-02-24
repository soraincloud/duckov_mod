using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.UIElements;

[MovedFrom(true, "UnityEditor.UIElements", "UnityEditor.UIElementsModule", null)]
public abstract class TextValueField<TValueType> : TextInputBaseField<TValueType>, IValueField<TValueType>
{
	protected abstract class TextValueInput : TextInputBase
	{
		private TextValueField<TValueType> textValueFieldParent => (TextValueField<TValueType>)base.parent;

		protected abstract string allowedCharacters { get; }

		public string formatString { get; set; }

		protected TextValueInput()
		{
			base.textEdition.AcceptCharacter = AcceptCharacter;
		}

		internal override bool AcceptCharacter(char c)
		{
			return base.AcceptCharacter(c) && c != 0 && allowedCharacters.IndexOf(c) != -1;
		}

		public abstract void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, TValueType startValue);

		public void StartDragging()
		{
			base.isDragging = true;
			SelectNone();
			MarkDirtyRepaint();
		}

		public void StopDragging()
		{
			if (textValueFieldParent.isDelayed)
			{
				UpdateValueFromText();
			}
			base.isDragging = false;
			SelectAll();
			MarkDirtyRepaint();
		}

		protected abstract string ValueToString(TValueType value);

		protected override TValueType StringToValue(string str)
		{
			return base.StringToValue(str);
		}
	}

	private BaseFieldMouseDragger m_Dragger;

	private bool m_ForceUpdateDisplay;

	internal const int kMaxValueFieldLength = 1000;

	private TextValueInput textValueInput => (TextValueInput)base.textInputBase;

	public string formatString
	{
		get
		{
			return textValueInput.formatString;
		}
		set
		{
			if (textValueInput.formatString != value)
			{
				textValueInput.formatString = value;
				base.textEdition.UpdateText(ValueToString(base.rawValue));
			}
		}
	}

	public override TValueType value
	{
		get
		{
			return base.value;
		}
		set
		{
			base.value = value;
		}
	}

	protected TextValueField(int maxLength, TextValueInput textValueInput)
		: this((string)null, maxLength, textValueInput)
	{
	}

	protected TextValueField(string label, int maxLength, TextValueInput textValueInput)
		: base(label, maxLength, '\0', (TextInputBase)textValueInput)
	{
		base.textEdition.UpdateText(ValueToString(base.rawValue));
		base.onIsReadOnlyChanged += OnIsReadOnlyChanged;
	}

	public abstract void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, TValueType startValue);

	public void StartDragging()
	{
		if (base.showMixedValue)
		{
			value = default(TValueType);
		}
		textValueInput.StartDragging();
	}

	public void StopDragging()
	{
		textValueInput.StopDragging();
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
		if (m_UpdateTextFromValue)
		{
			base.text = ValueToString(base.rawValue);
		}
	}

	private void OnIsReadOnlyChanged(bool newValue)
	{
		EnableLabelDragger(!newValue);
	}

	internal virtual bool CanTryParse(string textString)
	{
		return false;
	}

	protected void AddLabelDragger<TDraggerType>()
	{
		m_Dragger = new FieldMouseDragger<TDraggerType>((IValueField<TDraggerType>)this);
		EnableLabelDragger(!base.isReadOnly);
	}

	private void EnableLabelDragger(bool enable)
	{
		if (m_Dragger != null)
		{
			m_Dragger.SetDragZone(enable ? base.labelElement : null);
			base.labelElement.EnableInClassList(BaseField<TValueType>.labelDraggerVariantUssClassName, enable);
		}
	}

	public override void SetValueWithoutNotify(TValueType newValue)
	{
		bool flag = m_ForceUpdateDisplay || (m_UpdateTextFromValue && !EqualityComparer<TValueType>.Default.Equals(base.rawValue, newValue));
		base.SetValueWithoutNotify(newValue);
		if (flag)
		{
			base.textEdition.UpdateText(ValueToString(base.rawValue));
		}
		m_ForceUpdateDisplay = false;
	}

	[EventInterest(new Type[]
	{
		typeof(BlurEvent),
		typeof(FocusEvent)
	})]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		base.ExecuteDefaultAction(evt);
		if (evt == null)
		{
			return;
		}
		if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			if (base.showMixedValue)
			{
				UpdateMixedValueContent();
				return;
			}
			if (string.IsNullOrEmpty(base.text))
			{
				base.textInputBase.UpdateTextFromValue();
				return;
			}
			base.textInputBase.UpdateValueFromText();
			base.textInputBase.UpdateTextFromValue();
		}
		else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId() && base.showMixedValue && base.textInputBase.textElement.hasFocus)
		{
			base.textInputBase.text = "";
		}
	}

	internal override void OnViewDataReady()
	{
		m_ForceUpdateDisplay = true;
		base.OnViewDataReady();
	}

	internal override void RegisterEditingCallbacks()
	{
		base.RegisterEditingCallbacks();
		base.labelElement.RegisterCallback<PointerDownEvent>(base.StartEditing, TrickleDown.TrickleDown);
		base.labelElement.RegisterCallback<PointerUpEvent>(base.EndEditing);
	}

	internal override void UnregisterEditingCallbacks()
	{
		base.UnregisterEditingCallbacks();
		base.labelElement.UnregisterCallback<PointerDownEvent>(base.StartEditing, TrickleDown.TrickleDown);
		base.labelElement.UnregisterCallback<PointerUpEvent>(base.EndEditing);
	}
}
