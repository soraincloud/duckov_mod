using System;

namespace UnityEngine.UIElements;

public class Toggle : BaseBoolField
{
	public new class UxmlFactory : UxmlFactory<Toggle, UxmlTraits>
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
			((Toggle)ve).text = m_Text.GetValueFromBag(bag, cc);
		}
	}

	public new static readonly string ussClassName = "unity-toggle";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	[Obsolete]
	public static readonly string noTextVariantUssClassName = ussClassName + "--no-text";

	public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

	public static readonly string textUssClassName = ussClassName + "__text";

	public static readonly string mixedValuesUssClassName = ussClassName + "__mixed-values";

	public Toggle()
		: this(null)
	{
	}

	public Toggle(string label)
		: base(label)
	{
		AddToClassList(ussClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		m_CheckMark.AddToClassList(checkmarkUssClassName);
	}

	protected override void InitLabel()
	{
		base.InitLabel();
		m_Label.AddToClassList(textUssClassName);
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			base.visualInput.pseudoStates &= ~PseudoStates.Checked;
			base.pseudoStates &= ~PseudoStates.Checked;
			m_CheckMark.AddToClassList(mixedValuesUssClassName);
			return;
		}
		m_CheckMark.RemoveFromClassList(mixedValuesUssClassName);
		if (value)
		{
			base.visualInput.pseudoStates |= PseudoStates.Checked;
			base.pseudoStates |= PseudoStates.Checked;
		}
		else
		{
			base.visualInput.pseudoStates &= ~PseudoStates.Checked;
			base.pseudoStates &= ~PseudoStates.Checked;
		}
	}
}
