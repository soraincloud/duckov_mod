using System;

namespace UnityEngine.UIElements;

public class RadioButton : BaseBoolField, IGroupBoxOption
{
	public new class UxmlFactory : UxmlFactory<RadioButton, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseFieldTraits<bool, UxmlBoolAttributeDescription>
	{
		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "text"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			((RadioButton)ve).text = m_Text.GetValueFromBag(bag, cc);
		}
	}

	public new static readonly string ussClassName = "unity-radio-button";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string checkmarkBackgroundUssClassName = ussClassName + "__checkmark-background";

	public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

	public static readonly string textUssClassName = ussClassName + "__text";

	private VisualElement m_CheckmarkBackground;

	public override bool value
	{
		get
		{
			return base.value;
		}
		set
		{
			if (base.value != value)
			{
				base.value = value;
				UpdateCheckmark();
				if (value)
				{
					this.OnOptionSelected();
				}
			}
		}
	}

	public RadioButton()
		: this(null)
	{
	}

	public RadioButton(string label)
		: base(label)
	{
		AddToClassList(ussClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		m_CheckMark.RemoveFromHierarchy();
		m_CheckmarkBackground = new VisualElement
		{
			pickingMode = PickingMode.Ignore
		};
		m_CheckmarkBackground.Add(m_CheckMark);
		m_CheckmarkBackground.AddToClassList(checkmarkBackgroundUssClassName);
		m_CheckMark.AddToClassList(checkmarkUssClassName);
		base.visualInput.Add(m_CheckmarkBackground);
		UpdateCheckmark();
		RegisterCallback<AttachToPanelEvent>(OnOptionAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnOptionDetachFromPanel);
	}

	private void OnOptionAttachToPanel(AttachToPanelEvent evt)
	{
		this.RegisterGroupBoxOption();
	}

	private void OnOptionDetachFromPanel(DetachFromPanelEvent evt)
	{
		this.UnregisterGroupBoxOption();
	}

	protected override void InitLabel()
	{
		base.InitLabel();
		m_Label.AddToClassList(textUssClassName);
	}

	protected override void ToggleValue()
	{
		if (!value)
		{
			value = true;
		}
	}

	[Obsolete("[UI Toolkit] Please set the value property instead.", false)]
	public void SetSelected(bool selected)
	{
		((IGroupBoxOption)this).SetSelected(selected);
	}

	void IGroupBoxOption.SetSelected(bool selected)
	{
		value = selected;
	}

	public override void SetValueWithoutNotify(bool newValue)
	{
		base.SetValueWithoutNotify(newValue);
		UpdateCheckmark();
	}

	private void UpdateCheckmark()
	{
		m_CheckMark.style.display = ((!value) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	protected override void UpdateMixedValueContent()
	{
		base.UpdateMixedValueContent();
		if (base.showMixedValue)
		{
			m_CheckmarkBackground.RemoveFromHierarchy();
			return;
		}
		m_CheckmarkBackground.Add(m_CheckMark);
		base.visualInput.Add(m_CheckmarkBackground);
	}
}
