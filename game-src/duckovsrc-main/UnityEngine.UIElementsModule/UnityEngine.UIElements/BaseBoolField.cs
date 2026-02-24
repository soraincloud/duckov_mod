namespace UnityEngine.UIElements;

public abstract class BaseBoolField : BaseField<bool>
{
	protected Label m_Label;

	protected readonly VisualElement m_CheckMark;

	internal Clickable m_Clickable;

	private string m_OriginalText;

	internal Label boolFieldLabelElement => m_Label;

	public string text
	{
		get
		{
			return m_Label?.text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				if (m_Label == null)
				{
					InitLabel();
				}
				m_Label.text = value;
			}
			else if (m_Label != null)
			{
				m_Label.RemoveFromHierarchy();
				m_Label = null;
			}
		}
	}

	public BaseBoolField(string label)
		: base(label, (VisualElement)null)
	{
		m_CheckMark = new VisualElement
		{
			name = "unity-checkmark",
			pickingMode = PickingMode.Ignore
		};
		base.visualInput.Add(m_CheckMark);
		base.visualInput.pickingMode = PickingMode.Position;
		text = null;
		this.AddManipulator(m_Clickable = new Clickable(OnClickEvent));
		RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
	}

	private void OnNavigationSubmit(NavigationSubmitEvent evt)
	{
		ToggleValue();
		evt.StopPropagation();
	}

	protected virtual void InitLabel()
	{
		m_Label = new Label();
		base.visualInput.Add(m_Label);
	}

	public override void SetValueWithoutNotify(bool newValue)
	{
		if (newValue)
		{
			base.visualInput.pseudoStates |= PseudoStates.Checked;
			base.pseudoStates |= PseudoStates.Checked;
		}
		else
		{
			base.visualInput.pseudoStates &= ~PseudoStates.Checked;
			base.pseudoStates &= ~PseudoStates.Checked;
		}
		base.SetValueWithoutNotify(newValue);
	}

	private void OnClickEvent(EventBase evt)
	{
		if (evt.eventTypeId == EventBase<MouseUpEvent>.TypeId())
		{
			IMouseEvent mouseEvent = (IMouseEvent)evt;
			if (mouseEvent.button == 0)
			{
				ToggleValue();
			}
		}
		else if (evt.eventTypeId == EventBase<PointerUpEvent>.TypeId() || evt.eventTypeId == EventBase<ClickEvent>.TypeId())
		{
			IPointerEvent pointerEvent = (IPointerEvent)evt;
			if (pointerEvent.button == 0)
			{
				ToggleValue();
			}
		}
	}

	protected virtual void ToggleValue()
	{
		value = !value;
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			base.visualInput.pseudoStates &= ~PseudoStates.Checked;
			base.pseudoStates &= ~PseudoStates.Checked;
			m_CheckMark.RemoveFromHierarchy();
			base.visualInput.Add(base.mixedValueLabel);
			m_OriginalText = text;
			text = "";
		}
		else
		{
			base.mixedValueLabel.RemoveFromHierarchy();
			base.visualInput.Add(m_CheckMark);
			if (m_OriginalText != null)
			{
				text = m_OriginalText;
			}
		}
	}

	internal override void RegisterEditingCallbacks()
	{
		RegisterCallback<PointerUpEvent>(base.StartEditing);
		RegisterCallback<FocusOutEvent>(base.EndEditing);
	}

	internal override void UnregisterEditingCallbacks()
	{
		UnregisterCallback<PointerUpEvent>(base.StartEditing);
		UnregisterCallback<FocusOutEvent>(base.EndEditing);
	}
}
