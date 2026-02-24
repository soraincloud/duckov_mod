using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class RadioButtonGroup : BaseField<int>, IGroupBox
{
	public new class UxmlFactory : UxmlFactory<RadioButtonGroup, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseFieldTraits<int, UxmlIntAttributeDescription>
	{
		private UxmlStringAttributeDescription m_Choices = new UxmlStringAttributeDescription
		{
			name = "choices"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			RadioButtonGroup radioButtonGroup = (RadioButtonGroup)ve;
			radioButtonGroup.choices = BaseField<int>.UxmlTraits.ParseChoiceList(m_Choices.GetValueFromBag(bag, cc));
		}
	}

	public new static readonly string ussClassName = "unity-radio-button-group";

	public static readonly string containerUssClassName = ussClassName + "__container";

	private List<RadioButton> m_RadioButtons = new List<RadioButton>();

	private EventCallback<ChangeEvent<bool>> m_RadioButtonValueChangedCallback;

	private VisualElement m_RadioButtonContainer;

	public IEnumerable<string> choices
	{
		get
		{
			foreach (RadioButton radioButton in m_RadioButtons)
			{
				yield return radioButton.text;
			}
		}
		set
		{
			if (!value.HasValues())
			{
				m_RadioButtonContainer.Clear();
				if (base.panel != null)
				{
					return;
				}
				foreach (RadioButton radioButton2 in m_RadioButtons)
				{
					radioButton2.UnregisterValueChangedCallback(m_RadioButtonValueChangedCallback);
				}
				m_RadioButtons.Clear();
				return;
			}
			int num = 0;
			foreach (string item in value)
			{
				if (num < m_RadioButtons.Count)
				{
					m_RadioButtons[num].text = item;
					m_RadioButtonContainer.Insert(num, m_RadioButtons[num]);
				}
				else
				{
					RadioButton radioButton = new RadioButton
					{
						text = item
					};
					radioButton.RegisterValueChangedCallback(m_RadioButtonValueChangedCallback);
					m_RadioButtons.Add(radioButton);
					m_RadioButtonContainer.Add(radioButton);
				}
				num++;
			}
			int num2 = m_RadioButtons.Count - 1;
			for (int num3 = num2; num3 >= num; num3--)
			{
				m_RadioButtons[num3].RemoveFromHierarchy();
			}
			UpdateRadioButtons();
		}
	}

	public override VisualElement contentContainer => m_RadioButtonContainer ?? this;

	public RadioButtonGroup()
		: this(null)
	{
	}

	public RadioButtonGroup(string label, List<string> radioButtonChoices = null)
		: base(label, (VisualElement)null)
	{
		AddToClassList(ussClassName);
		VisualElement visualElement = base.visualInput;
		VisualElement obj = new VisualElement
		{
			name = containerUssClassName
		};
		VisualElement child = obj;
		m_RadioButtonContainer = obj;
		visualElement.Add(child);
		m_RadioButtonContainer.AddToClassList(containerUssClassName);
		m_RadioButtonValueChangedCallback = RadioButtonValueChangedCallback;
		choices = radioButtonChoices;
		value = -1;
		base.visualInput.focusable = false;
		base.delegatesFocus = true;
	}

	private void RadioButtonValueChangedCallback(ChangeEvent<bool> evt)
	{
		if (evt.newValue)
		{
			value = m_RadioButtons.IndexOf(evt.target as RadioButton);
			evt.StopPropagation();
		}
	}

	public override void SetValueWithoutNotify(int newValue)
	{
		base.SetValueWithoutNotify(newValue);
		UpdateRadioButtons();
	}

	private void UpdateRadioButtons()
	{
		if (value >= 0 && value < m_RadioButtons.Count)
		{
			m_RadioButtons[value].value = true;
			return;
		}
		foreach (RadioButton radioButton in m_RadioButtons)
		{
			radioButton.value = false;
		}
	}

	void IGroupBox.OnOptionAdded(IGroupBoxOption option)
	{
		if (!(option is RadioButton radioButton))
		{
			throw new ArgumentException("[UI Toolkit] Internal group box error. Expected a radio button element. Please report this using Help -> Report a bug...");
		}
		if (!m_RadioButtons.Contains(radioButton))
		{
			radioButton.RegisterValueChangedCallback(m_RadioButtonValueChangedCallback);
			int num = m_RadioButtonContainer.IndexOf(radioButton);
			if (num < 0 || num > m_RadioButtons.Count)
			{
				m_RadioButtons.Add(radioButton);
				m_RadioButtonContainer.Add(radioButton);
			}
			else
			{
				m_RadioButtons.Insert(num, radioButton);
			}
		}
	}

	void IGroupBox.OnOptionRemoved(IGroupBoxOption option)
	{
		if (!(option is RadioButton radioButton))
		{
			throw new ArgumentException("[UI Toolkit] Internal group box error. Expected a radio button element. Please report this using Help -> Report a bug...");
		}
		int num = m_RadioButtons.IndexOf(radioButton);
		radioButton.UnregisterValueChangedCallback(m_RadioButtonValueChangedCallback);
		m_RadioButtons.Remove(radioButton);
		if (value == num)
		{
			value = -1;
		}
	}
}
