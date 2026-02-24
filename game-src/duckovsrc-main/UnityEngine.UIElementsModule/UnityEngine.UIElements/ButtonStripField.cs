using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class ButtonStripField : BaseField<int>
{
	public new class UxmlFactory : UxmlFactory<ButtonStripField, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseField<int>.UxmlTraits
	{
	}

	public const string className = "unity-button-strip-field";

	private const string k_ButtonClass = "unity-button-strip-field__button";

	private const string k_IconClass = "unity-button-strip-field__button-icon";

	private const string k_ButtonLeftClass = "unity-button-strip-field__button--left";

	private const string k_ButtonMiddleClass = "unity-button-strip-field__button--middle";

	private const string k_ButtonRightClass = "unity-button-strip-field__button--right";

	private const string k_ButtonAloneClass = "unity-button-strip-field__button--alone";

	private List<Button> m_Buttons = new List<Button>();

	public void AddButton(string text, string name = "")
	{
		Button button = CreateButton(name);
		button.text = text;
		Add(button);
	}

	public void AddButton(Background icon, string name = "")
	{
		Button button = CreateButton(name);
		VisualElement visualElement = new VisualElement();
		visualElement.AddToClassList("unity-button-strip-field__button-icon");
		visualElement.style.backgroundImage = icon;
		button.Add(visualElement);
		Add(button);
	}

	private Button CreateButton(string name)
	{
		Button button = new Button
		{
			name = name
		};
		button.AddToClassList("unity-button-strip-field__button");
		button.RegisterCallback<DetachFromPanelEvent>(OnButtonDetachFromPanel);
		button.clicked += delegate
		{
			value = m_Buttons.IndexOf(button);
		};
		m_Buttons.Add(button);
		Add(button);
		RefreshButtonsStyling();
		return button;
	}

	private void OnButtonDetachFromPanel(DetachFromPanelEvent evt)
	{
		if (evt.currentTarget is VisualElement { parent: ButtonStripField buttonStripField })
		{
			buttonStripField.RefreshButtonsStyling();
			buttonStripField.EnsureValueIsValid();
		}
	}

	private void RefreshButtonsStyling()
	{
		for (int i = 0; i < m_Buttons.Count; i++)
		{
			Button button = m_Buttons[i];
			bool flag = m_Buttons.Count == 1;
			bool flag2 = i == 0;
			bool flag3 = i == m_Buttons.Count - 1;
			button.EnableInClassList("unity-button-strip-field__button--alone", flag);
			button.EnableInClassList("unity-button-strip-field__button--left", !flag && flag2);
			button.EnableInClassList("unity-button-strip-field__button--right", !flag && flag3);
			button.EnableInClassList("unity-button-strip-field__button--middle", !flag && !flag2 && !flag3);
		}
	}

	public ButtonStripField()
		: base((string)null)
	{
	}

	public ButtonStripField(string label)
		: base(label)
	{
		AddToClassList("unity-button-strip-field");
	}

	public override void SetValueWithoutNotify(int newValue)
	{
		newValue = Mathf.Clamp(newValue, 0, m_Buttons.Count - 1);
		base.SetValueWithoutNotify(newValue);
		RefreshButtonsState();
	}

	private void EnsureValueIsValid()
	{
		SetValueWithoutNotify(Mathf.Clamp(value, 0, m_Buttons.Count - 1));
	}

	private void RefreshButtonsState()
	{
		for (int i = 0; i < m_Buttons.Count; i++)
		{
			if (i == value)
			{
				m_Buttons[i].pseudoStates |= PseudoStates.Checked;
			}
			else
			{
				m_Buttons[i].pseudoStates &= ~PseudoStates.Checked;
			}
		}
	}
}
