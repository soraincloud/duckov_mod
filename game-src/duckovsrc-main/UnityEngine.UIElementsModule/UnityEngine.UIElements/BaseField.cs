using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public abstract class BaseField<TValueType> : BindableElement, INotifyValueChanged<TValueType>, IMixedValueSupport, IPrefixLabel, IEditableElement
{
	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription
		{
			name = "label"
		};

		public UxmlTraits()
		{
			base.focusIndex.defaultValue = 0;
			base.focusable.defaultValue = true;
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			((BaseField<TValueType>)ve).label = m_Label.GetValueFromBag(bag, cc);
		}

		internal static List<string> ParseChoiceList(string choicesFromBag)
		{
			if (string.IsNullOrEmpty(choicesFromBag.Trim()))
			{
				return null;
			}
			string[] array = choicesFromBag.Split(',');
			if (array.Length != 0)
			{
				List<string> list = new List<string>();
				string[] array2 = array;
				foreach (string text in array2)
				{
					list.Add(text.Trim());
				}
				return list;
			}
			return null;
		}
	}

	public static readonly string ussClassName = "unity-base-field";

	public static readonly string labelUssClassName = ussClassName + "__label";

	public static readonly string inputUssClassName = ussClassName + "__input";

	public static readonly string noLabelVariantUssClassName = ussClassName + "--no-label";

	public static readonly string labelDraggerVariantUssClassName = labelUssClassName + "--with-dragger";

	public static readonly string mixedValueLabelUssClassName = labelUssClassName + "--mixed-value";

	public static readonly string alignedFieldUssClassName = ussClassName + "__aligned";

	private static readonly string inspectorFieldUssClassName = ussClassName + "__inspector-field";

	protected internal static readonly string mixedValueString = "—";

	protected internal static readonly PropertyName serializedPropertyCopyName = "SerializedPropertyCopyName";

	private static CustomStyleProperty<float> s_LabelWidthRatioProperty = new CustomStyleProperty<float>("--unity-property-field-label-width-ratio");

	private static CustomStyleProperty<float> s_LabelExtraPaddingProperty = new CustomStyleProperty<float>("--unity-property-field-label-extra-padding");

	private static CustomStyleProperty<float> s_LabelBaseMinWidthProperty = new CustomStyleProperty<float>("--unity-property-field-label-base-min-width");

	private static CustomStyleProperty<float> s_LabelExtraContextWidthProperty = new CustomStyleProperty<float>("--unity-base-field-extra-context-width");

	private float m_LabelWidthRatio;

	private float m_LabelExtraPadding;

	private float m_LabelBaseMinWidth;

	private float m_LabelExtraContextWidth;

	private VisualElement m_VisualInput;

	internal Action<ExpressionEvaluator.Expression> expressionEvaluated;

	[SerializeField]
	private TValueType m_Value;

	private bool m_ShowMixedValue;

	private Label m_MixedValueLabel;

	private bool m_SkipValidation;

	private VisualElement m_CachedContextWidthElement;

	private VisualElement m_CachedInspectorElement;

	internal VisualElement visualInput
	{
		get
		{
			return m_VisualInput;
		}
		set
		{
			if (m_VisualInput != null)
			{
				if (m_VisualInput.parent == this)
				{
					m_VisualInput.RemoveFromHierarchy();
				}
				m_VisualInput = null;
			}
			if (value != null)
			{
				m_VisualInput = value;
			}
			else
			{
				m_VisualInput = new VisualElement
				{
					pickingMode = PickingMode.Ignore
				};
			}
			m_VisualInput.focusable = true;
			m_VisualInput.AddToClassList(inputUssClassName);
			Add(m_VisualInput);
		}
	}

	protected TValueType rawValue
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public virtual TValueType value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (EqualsCurrentValue(value) && !showMixedValue)
			{
				return;
			}
			TValueType previousValue = m_Value;
			SetValueWithoutNotify(value);
			showMixedValue = false;
			if (base.panel != null)
			{
				using (ChangeEvent<TValueType> changeEvent = ChangeEvent<TValueType>.GetPooled(previousValue, m_Value))
				{
					changeEvent.target = this;
					SendEvent(changeEvent);
				}
			}
		}
	}

	public Label labelElement { get; private set; }

	public string label
	{
		get
		{
			return labelElement.text;
		}
		set
		{
			if (labelElement.text != value)
			{
				labelElement.text = value;
				if (string.IsNullOrEmpty(labelElement.text))
				{
					AddToClassList(noLabelVariantUssClassName);
					labelElement.RemoveFromHierarchy();
				}
				else if (!Contains(labelElement))
				{
					base.hierarchy.Insert(0, labelElement);
					RemoveFromClassList(noLabelVariantUssClassName);
				}
			}
		}
	}

	public bool showMixedValue
	{
		get
		{
			return m_ShowMixedValue;
		}
		set
		{
			if (value != m_ShowMixedValue && (!value || canSwitchToMixedValue))
			{
				m_ShowMixedValue = value;
				UpdateMixedValueContent();
			}
		}
	}

	private protected virtual bool canSwitchToMixedValue => true;

	protected Label mixedValueLabel
	{
		get
		{
			if (m_MixedValueLabel == null)
			{
				m_MixedValueLabel = new Label(mixedValueString)
				{
					focusable = true,
					tabIndex = -1
				};
				m_MixedValueLabel.AddToClassList(labelUssClassName);
				m_MixedValueLabel.AddToClassList(mixedValueLabelUssClassName);
			}
			return m_MixedValueLabel;
		}
	}

	Action IEditableElement.editingStarted { get; set; }

	Action IEditableElement.editingEnded { get; set; }

	internal event Func<TValueType, TValueType> onValidateValue;

	internal BaseField(string label)
	{
		base.isCompositeRoot = true;
		base.focusable = true;
		base.tabIndex = 0;
		base.excludeFromFocusRing = true;
		base.delegatesFocus = true;
		AddToClassList(ussClassName);
		labelElement = new Label
		{
			focusable = true,
			tabIndex = -1
		};
		labelElement.AddToClassList(labelUssClassName);
		if (label != null)
		{
			this.label = label;
		}
		else
		{
			AddToClassList(noLabelVariantUssClassName);
		}
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		m_VisualInput = null;
	}

	protected BaseField(string label, VisualElement visualInput)
		: this(label)
	{
		this.visualInput = visualInput;
	}

	internal virtual bool EqualsCurrentValue(TValueType value)
	{
		return EqualityComparer<TValueType>.Default.Equals(m_Value, value);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		RegisterEditingCallbacks();
		if (e.destinationPanel == null || e.destinationPanel.contextType == ContextType.Player)
		{
			return;
		}
		for (VisualElement visualElement = base.parent; visualElement != null; visualElement = visualElement.parent)
		{
			if (visualElement.ClassListContains("unity-inspector-element"))
			{
				m_CachedInspectorElement = visualElement;
			}
			if (visualElement.ClassListContains("unity-inspector-main-container"))
			{
				m_CachedContextWidthElement = visualElement;
				break;
			}
		}
		if (m_CachedInspectorElement != null)
		{
			m_LabelWidthRatio = 0.45f;
			m_LabelExtraPadding = 37f;
			m_LabelBaseMinWidth = 123f;
			m_LabelExtraContextWidth = 1f;
			RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
			AddToClassList(inspectorFieldUssClassName);
			RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
		}
	}

	private void OnDetachFromPanel(DetachFromPanelEvent e)
	{
		UnregisterEditingCallbacks();
		this.onValidateValue = null;
	}

	internal virtual void RegisterEditingCallbacks()
	{
		RegisterCallback<FocusInEvent>(StartEditing);
		RegisterCallback<FocusOutEvent>(EndEditing);
	}

	internal virtual void UnregisterEditingCallbacks()
	{
		UnregisterCallback<FocusInEvent>(StartEditing);
		UnregisterCallback<FocusOutEvent>(EndEditing);
	}

	internal void StartEditing(EventBase e)
	{
		((IEditableElement)this).editingStarted?.Invoke();
	}

	internal void EndEditing(EventBase e)
	{
		((IEditableElement)this).editingEnded?.Invoke();
	}

	private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
	{
		if (evt.customStyle.TryGetValue(s_LabelWidthRatioProperty, out var labelWidthRatio))
		{
			m_LabelWidthRatio = labelWidthRatio;
		}
		if (evt.customStyle.TryGetValue(s_LabelExtraPaddingProperty, out var labelExtraPadding))
		{
			m_LabelExtraPadding = labelExtraPadding;
		}
		if (evt.customStyle.TryGetValue(s_LabelBaseMinWidthProperty, out var labelBaseMinWidth))
		{
			m_LabelBaseMinWidth = labelBaseMinWidth;
		}
		if (evt.customStyle.TryGetValue(s_LabelExtraContextWidthProperty, out var labelExtraContextWidth))
		{
			m_LabelExtraContextWidth = labelExtraContextWidth;
		}
		AlignLabel();
	}

	private void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
	{
		AlignLabel();
	}

	private void AlignLabel()
	{
		if (ClassListContains(alignedFieldUssClassName))
		{
			float labelExtraPadding = m_LabelExtraPadding;
			float num = base.worldBound.x - m_CachedInspectorElement.worldBound.x - m_CachedInspectorElement.resolvedStyle.paddingLeft;
			labelExtraPadding += num;
			labelExtraPadding += base.resolvedStyle.paddingLeft;
			float a = m_LabelBaseMinWidth - num - base.resolvedStyle.paddingLeft;
			VisualElement visualElement = m_CachedContextWidthElement ?? m_CachedInspectorElement;
			labelElement.style.minWidth = Mathf.Max(a, 0f);
			float num2 = (visualElement.resolvedStyle.width + m_LabelExtraContextWidth) * m_LabelWidthRatio - labelExtraPadding;
			if (Mathf.Abs(labelElement.resolvedStyle.width - num2) > 1E-30f)
			{
				labelElement.style.width = Mathf.Max(0f, num2);
			}
		}
	}

	internal TValueType ValidatedValue(TValueType value)
	{
		if (this.onValidateValue != null)
		{
			return this.onValidateValue(value);
		}
		return value;
	}

	protected virtual void UpdateMixedValueContent()
	{
		throw new NotImplementedException();
	}

	public virtual void SetValueWithoutNotify(TValueType newValue)
	{
		if (m_SkipValidation)
		{
			m_Value = newValue;
		}
		else
		{
			m_Value = ValidatedValue(newValue);
		}
		if (!string.IsNullOrEmpty(base.viewDataKey))
		{
			SaveViewData();
		}
		MarkDirtyRepaint();
		if (showMixedValue)
		{
			UpdateMixedValueContent();
		}
	}

	internal void SetValueWithoutValidation(TValueType newValue)
	{
		m_SkipValidation = true;
		value = newValue;
		m_SkipValidation = false;
	}

	internal override void OnViewDataReady()
	{
		base.OnViewDataReady();
		if (m_VisualInput == null)
		{
			return;
		}
		string fullHierarchicalViewDataKey = GetFullHierarchicalViewDataKey();
		TValueType val = m_Value;
		OverwriteFromViewData(this, fullHierarchicalViewDataKey);
		if (!EqualityComparer<TValueType>.Default.Equals(val, m_Value))
		{
			using (ChangeEvent<TValueType> changeEvent = ChangeEvent<TValueType>.GetPooled(val, m_Value))
			{
				changeEvent.target = this;
				SetValueWithoutNotify(m_Value);
				SendEvent(changeEvent);
			}
		}
	}

	internal override Rect GetTooltipRect()
	{
		return (!string.IsNullOrEmpty(label)) ? labelElement.worldBound : base.worldBound;
	}
}
