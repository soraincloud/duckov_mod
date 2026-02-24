using System;

namespace UnityEngine.UIElements;

public class Foldout : BindableElement, INotifyValueChanged<bool>
{
	public new class UxmlFactory : UxmlFactory<Foldout, UxmlTraits>
	{
	}

	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "text"
		};

		private UxmlBoolAttributeDescription m_Value = new UxmlBoolAttributeDescription
		{
			name = "value",
			defaultValue = true
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			if (ve is Foldout foldout)
			{
				foldout.text = m_Text.GetValueFromBag(bag, cc);
				foldout.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
			}
		}
	}

	private Toggle m_Toggle = new Toggle();

	private VisualElement m_Container;

	[SerializeField]
	private bool m_Value;

	public static readonly string ussClassName = "unity-foldout";

	public static readonly string toggleUssClassName = ussClassName + "__toggle";

	public static readonly string contentUssClassName = ussClassName + "__content";

	public static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

	public static readonly string textUssClassName = ussClassName + "__text";

	internal static readonly string toggleInspectorUssClassName = toggleUssClassName + "--inspector";

	internal static readonly string ussFoldoutDepthClassName = ussClassName + "--depth-";

	internal static readonly int ussFoldoutMaxDepth = 4;

	private KeyboardNavigationManipulator m_NavigationManipulator;

	internal Toggle toggle => m_Toggle;

	public override VisualElement contentContainer => m_Container;

	public string text
	{
		get
		{
			return m_Toggle.text;
		}
		set
		{
			m_Toggle.text = value;
			m_Toggle.visualInput.Q(null, Toggle.textUssClassName)?.AddToClassList(textUssClassName);
		}
	}

	public bool value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (m_Value == value)
			{
				return;
			}
			using ChangeEvent<bool> changeEvent = ChangeEvent<bool>.GetPooled(m_Value, value);
			changeEvent.target = this;
			SetValueWithoutNotify(value);
			SendEvent(changeEvent);
			SaveViewData();
		}
	}

	public void SetValueWithoutNotify(bool newValue)
	{
		m_Value = newValue;
		m_Toggle.SetValueWithoutNotify(m_Value);
		contentContainer.style.display = ((!newValue) ? DisplayStyle.None : DisplayStyle.Flex);
		if (m_Value)
		{
			base.pseudoStates |= PseudoStates.Checked;
		}
		else
		{
			base.pseudoStates &= ~PseudoStates.Checked;
		}
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		SetValueWithoutNotify(m_Value);
	}

	private void Apply(KeyboardNavigationOperation op, EventBase sourceEvent)
	{
		if (Apply(op))
		{
			sourceEvent.StopPropagation();
		}
	}

	private bool Apply(KeyboardNavigationOperation op)
	{
		switch (op)
		{
		case KeyboardNavigationOperation.MoveRight:
			SetValueWithoutNotify(newValue: true);
			return true;
		case KeyboardNavigationOperation.MoveLeft:
			SetValueWithoutNotify(newValue: false);
			return true;
		default:
			throw new ArgumentOutOfRangeException("op", op, null);
		case KeyboardNavigationOperation.SelectAll:
		case KeyboardNavigationOperation.Cancel:
		case KeyboardNavigationOperation.Submit:
		case KeyboardNavigationOperation.Previous:
		case KeyboardNavigationOperation.Next:
		case KeyboardNavigationOperation.PageUp:
		case KeyboardNavigationOperation.PageDown:
		case KeyboardNavigationOperation.Begin:
		case KeyboardNavigationOperation.End:
			return false;
		}
	}

	public Foldout()
	{
		AddToClassList(ussClassName);
		base.delegatesFocus = true;
		m_Container = new VisualElement
		{
			name = "unity-content"
		};
		m_Toggle.RegisterValueChangedCallback(delegate(ChangeEvent<bool> evt)
		{
			value = m_Toggle.value;
			evt.StopPropagation();
		});
		m_Toggle.AddToClassList(toggleUssClassName);
		m_Toggle.visualInput.AddToClassList(inputUssClassName);
		m_Toggle.visualInput.Q(null, Toggle.checkmarkUssClassName).AddToClassList(checkmarkUssClassName);
		m_Toggle.AddManipulator(m_NavigationManipulator = new KeyboardNavigationManipulator(Apply));
		base.hierarchy.Add(m_Toggle);
		m_Container.AddToClassList(contentUssClassName);
		base.hierarchy.Add(m_Container);
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		SetValueWithoutNotify(newValue: true);
	}

	private void OnAttachToPanel(AttachToPanelEvent evt)
	{
		for (int i = 0; i <= ussFoldoutMaxDepth; i++)
		{
			RemoveFromClassList(ussFoldoutDepthClassName + i);
		}
		RemoveFromClassList(ussFoldoutDepthClassName + "max");
		m_Toggle.AssignInspectorStyleIfNecessary(toggleInspectorUssClassName);
		int foldoutDepth = this.GetFoldoutDepth();
		if (foldoutDepth > ussFoldoutMaxDepth)
		{
			AddToClassList(ussFoldoutDepthClassName + "max");
		}
		else
		{
			AddToClassList(ussFoldoutDepthClassName + foldoutDepth);
		}
	}
}
