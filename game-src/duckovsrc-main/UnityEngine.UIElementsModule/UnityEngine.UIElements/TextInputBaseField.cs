using System;

namespace UnityEngine.UIElements;

public abstract class TextInputBaseField<TValueType> : BaseField<TValueType>
{
	public new class UxmlTraits : BaseFieldTraits<string, UxmlStringAttributeDescription>
	{
		private UxmlIntAttributeDescription m_MaxLength = new UxmlIntAttributeDescription
		{
			name = "max-length",
			obsoleteNames = new string[1] { "maxLength" },
			defaultValue = -1
		};

		private UxmlBoolAttributeDescription m_Password = new UxmlBoolAttributeDescription
		{
			name = "password"
		};

		private UxmlStringAttributeDescription m_MaskCharacter = new UxmlStringAttributeDescription
		{
			name = "mask-character",
			obsoleteNames = new string[1] { "maskCharacter" },
			defaultValue = '*'.ToString()
		};

		private UxmlBoolAttributeDescription m_IsReadOnly = new UxmlBoolAttributeDescription
		{
			name = "readonly"
		};

		private UxmlBoolAttributeDescription m_IsDelayed = new UxmlBoolAttributeDescription
		{
			name = "is-delayed"
		};

		private UxmlBoolAttributeDescription m_HideMobileInput = new UxmlBoolAttributeDescription
		{
			name = "hide-mobile-input"
		};

		private UxmlEnumAttributeDescription<TouchScreenKeyboardType> m_KeyboardType = new UxmlEnumAttributeDescription<TouchScreenKeyboardType>
		{
			name = "keyboard-type"
		};

		private UxmlBoolAttributeDescription m_AutoCorrection = new UxmlBoolAttributeDescription
		{
			name = "auto-correction"
		};

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			TextInputBaseField<TValueType> textInputBaseField = (TextInputBaseField<TValueType>)ve;
			textInputBaseField.maxLength = m_MaxLength.GetValueFromBag(bag, cc);
			textInputBaseField.isPasswordField = m_Password.GetValueFromBag(bag, cc);
			textInputBaseField.isReadOnly = m_IsReadOnly.GetValueFromBag(bag, cc);
			textInputBaseField.isDelayed = m_IsDelayed.GetValueFromBag(bag, cc);
			textInputBaseField.hideMobileInput = m_HideMobileInput.GetValueFromBag(bag, cc);
			textInputBaseField.keyboardType = m_KeyboardType.GetValueFromBag(bag, cc);
			textInputBaseField.autoCorrection = m_AutoCorrection.GetValueFromBag(bag, cc);
			string valueFromBag = m_MaskCharacter.GetValueFromBag(bag, cc);
			textInputBaseField.maskChar = (string.IsNullOrEmpty(valueFromBag) ? '*' : valueFromBag[0]);
		}
	}

	protected internal abstract class TextInputBase : VisualElement
	{
		internal ScrollView scrollView;

		internal VisualElement multilineContainer;

		public static readonly string innerComponentsModifierName = "--inner-input-field-component";

		public static readonly string innerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName;

		internal static readonly string innerTextElementWithScrollViewUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--scroll-view";

		public static readonly string horizontalVariantInnerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--horizontal";

		public static readonly string verticalVariantInnerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--vertical";

		public static readonly string verticalHorizontalVariantInnerTextElementUssClassName = TextElement.ussClassName + innerComponentsModifierName + "--vertical-horizontal";

		public static readonly string innerScrollviewUssClassName = ScrollView.ussClassName + innerComponentsModifierName;

		public static readonly string innerViewportUssClassName = ScrollView.viewportUssClassName + innerComponentsModifierName;

		public static readonly string innerContentContainerUssClassName = ScrollView.contentUssClassName + innerComponentsModifierName;

		internal Vector2 scrollOffset = Vector2.zero;

		private bool m_ScrollViewWasClamped;

		private Vector2 lastCursorPos = Vector2.zero;

		private ScrollerVisibility m_VerticalScrollerVisibility = ScrollerVisibility.Hidden;

		internal TextElement textElement { get; private set; }

		public ITextSelection textSelection => textElement.selection;

		public ITextEdition textEdition => textElement.edition;

		internal string originalText => textElement.originalText;

		public bool isReadOnly
		{
			get
			{
				return textEdition.isReadOnly;
			}
			set
			{
				textEdition.isReadOnly = value;
			}
		}

		public int maxLength
		{
			get
			{
				return textEdition.maxLength;
			}
			set
			{
				textEdition.maxLength = value;
			}
		}

		public char maskChar
		{
			get
			{
				return textEdition.maskChar;
			}
			set
			{
				textEdition.maskChar = value;
			}
		}

		public virtual bool isPasswordField
		{
			get
			{
				return textEdition.isPassword;
			}
			set
			{
				textEdition.isPassword = value;
			}
		}

		internal bool isDelayed
		{
			get
			{
				return textEdition.isDelayed;
			}
			set
			{
				textEdition.isDelayed = value;
			}
		}

		internal bool isDragging { get; set; }

		public Color selectionColor
		{
			get
			{
				return textSelection.selectionColor;
			}
			set
			{
				textSelection.selectionColor = value;
			}
		}

		public Color cursorColor
		{
			get
			{
				return textSelection.cursorColor;
			}
			set
			{
				textSelection.cursorColor = value;
			}
		}

		public int cursorIndex => textSelection.cursorIndex;

		public int selectIndex => textSelection.selectIndex;

		public bool doubleClickSelectsWord
		{
			get
			{
				return textSelection.doubleClickSelectsWord;
			}
			set
			{
				textSelection.doubleClickSelectsWord = value;
			}
		}

		public bool tripleClickSelectsLine
		{
			get
			{
				return textSelection.tripleClickSelectsLine;
			}
			set
			{
				textSelection.tripleClickSelectsLine = value;
			}
		}

		public string text
		{
			get
			{
				return textElement.text;
			}
			set
			{
				if (!(textElement.text == value))
				{
					textElement.text = value;
				}
			}
		}

		public void SelectAll()
		{
			textSelection.SelectAll();
		}

		internal void SelectNone()
		{
			textSelection.SelectNone();
		}

		protected virtual TValueType StringToValue(string str)
		{
			throw new NotSupportedException();
		}

		internal void UpdateValueFromText()
		{
			TextInputBaseField<TValueType> textInputBaseField = (TextInputBaseField<TValueType>)base.parent;
			textInputBaseField.UpdateValueFromText();
		}

		internal void UpdateTextFromValue()
		{
			TextInputBaseField<TValueType> textInputBaseField = (TextInputBaseField<TValueType>)base.parent;
			textInputBaseField.UpdateTextFromValue();
		}

		internal void MoveFocusToCompositeRoot()
		{
			TextInputBaseField<TValueType> textInputBaseField = (TextInputBaseField<TValueType>)base.parent;
			textInputBaseField.Focus();
		}

		public void ResetValueAndText()
		{
			textEdition.ResetValueAndText();
		}

		internal TextInputBase()
		{
			base.delegatesFocus = true;
			textElement = new TextElement();
			textElement.parseEscapeSequences = false;
			textElement.selection.isSelectable = true;
			textEdition.isReadOnly = false;
			textEdition.keyboardType = TouchScreenKeyboardType.Default;
			textEdition.autoCorrection = false;
			textSelection.isSelectable = true;
			textElement.enableRichText = false;
			textSelection.selectAllOnFocus = true;
			textSelection.selectAllOnMouseUp = true;
			textElement.tabIndex = 0;
			ITextEdition obj = textEdition;
			obj.AcceptCharacter = (Func<char, bool>)Delegate.Combine(obj.AcceptCharacter, new Func<char, bool>(AcceptCharacter));
			ITextEdition obj2 = textEdition;
			obj2.UpdateScrollOffset = (Action<bool>)Delegate.Combine(obj2.UpdateScrollOffset, new Action<bool>(UpdateScrollOffset));
			ITextEdition obj3 = textEdition;
			obj3.UpdateValueFromText = (Action)Delegate.Combine(obj3.UpdateValueFromText, new Action(UpdateValueFromText));
			ITextEdition obj4 = textEdition;
			obj4.UpdateTextFromValue = (Action)Delegate.Combine(obj4.UpdateTextFromValue, new Action(UpdateTextFromValue));
			ITextEdition obj5 = textEdition;
			obj5.MoveFocusToCompositeRoot = (Action)Delegate.Combine(obj5.MoveFocusToCompositeRoot, new Action(MoveFocusToCompositeRoot));
			AddToClassList(TextInputBaseField<TValueType>.inputUssClassName);
			base.name = TextInputBaseField<string>.textInputUssName;
			SetSingleLine();
			RegisterCallback<CustomStyleResolvedEvent>(OnInputCustomStyleResolved);
			base.tabIndex = -1;
		}

		private void MakeSureScrollViewDoesNotLeakEvents(ChangeEvent<float> evt)
		{
			evt.StopPropagation();
		}

		internal void SetSingleLine()
		{
			base.hierarchy.Clear();
			RemoveMultilineComponents();
			Add(textElement);
			AddToClassList(TextInputBaseField<TValueType>.singleLineInputUssClassName);
			textElement.AddToClassList(innerTextElementUssClassName);
			textElement.RegisterCallback<GeometryChangedEvent>(TextElementOnGeometryChangedEvent);
			if (scrollOffset != Vector2.zero)
			{
				scrollOffset.y = 0f;
				UpdateScrollOffset();
			}
		}

		internal void SetMultiline()
		{
			if (textEdition.multiline)
			{
				RemoveSingleLineComponents();
				RemoveMultilineComponents();
				if (m_VerticalScrollerVisibility != ScrollerVisibility.Hidden && scrollView == null)
				{
					scrollView = new ScrollView();
					scrollView.Add(textElement);
					Add(scrollView);
					SetScrollViewMode();
					scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
					scrollView.verticalScrollerVisibility = m_VerticalScrollerVisibility;
					scrollView.AddToClassList(innerScrollviewUssClassName);
					scrollView.contentViewport.AddToClassList(innerViewportUssClassName);
					scrollView.contentContainer.AddToClassList(innerContentContainerUssClassName);
					scrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(ScrollViewOnGeometryChangedEvent);
					scrollView.verticalScroller.slider.RegisterValueChangedCallback(MakeSureScrollViewDoesNotLeakEvents);
					scrollView.verticalScroller.slider.focusable = false;
					scrollView.horizontalScroller.slider.RegisterValueChangedCallback(MakeSureScrollViewDoesNotLeakEvents);
					scrollView.horizontalScroller.slider.focusable = false;
					AddToClassList(TextInputBaseField<TValueType>.multilineInputWithScrollViewUssClassName);
					textElement.AddToClassList(innerTextElementWithScrollViewUssClassName);
				}
				else if (multilineContainer == null)
				{
					textElement.RegisterCallback<GeometryChangedEvent>(TextElementOnGeometryChangedEvent);
					multilineContainer = new VisualElement
					{
						classList = { TextInputBaseField<TValueType>.multilineContainerClassName }
					};
					multilineContainer.Add(textElement);
					Add(multilineContainer);
					SetMultilineContainerStyle();
					AddToClassList(TextInputBaseField<TValueType>.multilineInputUssClassName);
					textElement.AddToClassList(innerTextElementUssClassName);
				}
			}
		}

		private void ScrollViewOnGeometryChangedEvent(GeometryChangedEvent e)
		{
			if (!(e.oldRect.size == e.newRect.size))
			{
				UpdateScrollOffset();
			}
		}

		private void TextElementOnGeometryChangedEvent(GeometryChangedEvent e)
		{
			if (!(e.oldRect.size == e.newRect.size))
			{
				bool widthChanged = Math.Abs(e.oldRect.size.x - e.newRect.size.x) > 1E-30f;
				UpdateScrollOffset(isBackspace: false, widthChanged);
			}
		}

		internal void OnInputCustomStyleResolved(CustomStyleResolvedEvent e)
		{
			ICustomStyle customStyle = e.customStyle;
			if (customStyle.TryGetValue(TextInputBaseField<TValueType>.s_SelectionColorProperty, out var value))
			{
				textSelection.selectionColor = value;
			}
			if (customStyle.TryGetValue(TextInputBaseField<TValueType>.s_CursorColorProperty, out var value2))
			{
				textSelection.cursorColor = value2;
			}
			SetScrollViewMode();
			SetMultilineContainerStyle();
		}

		internal virtual bool AcceptCharacter(char c)
		{
			return !isReadOnly && base.enabledInHierarchy;
		}

		internal void UpdateScrollOffset(bool isBackspace = false)
		{
			UpdateScrollOffset(isBackspace, widthChanged: false);
		}

		internal void UpdateScrollOffset(bool isBackspace, bool widthChanged)
		{
			ITextSelection textSelection = this.textSelection;
			if (textSelection.cursorIndex < 0)
			{
				return;
			}
			if (scrollView != null)
			{
				scrollOffset = GetScrollOffset(scrollView.scrollOffset.x, scrollView.scrollOffset.y, scrollView.contentViewport.layout.width, isBackspace, widthChanged);
				scrollView.scrollOffset = scrollOffset;
				m_ScrollViewWasClamped = scrollOffset.x > scrollView.scrollOffset.x || scrollOffset.y > scrollView.scrollOffset.y;
				return;
			}
			Vector3 position = textElement.transform.position;
			scrollOffset = GetScrollOffset(scrollOffset.x, scrollOffset.y, base.contentRect.width, isBackspace, widthChanged);
			position.y = 0f - Mathf.Min(scrollOffset.y, Math.Abs(textElement.contentRect.height - base.contentRect.height));
			position.x = 0f - scrollOffset.x;
			if (!position.Equals(textElement.transform.position))
			{
				textElement.transform.position = position;
			}
		}

		private Vector2 GetScrollOffset(float xOffset, float yOffset, float contentViewportWidth, bool isBackspace, bool widthChanged)
		{
			Vector2 cursorPosition = textSelection.cursorPosition;
			float cursorWidth = textSelection.cursorWidth;
			float num = xOffset;
			float num2 = yOffset;
			if (Math.Abs(lastCursorPos.x - cursorPosition.x) > 0.05f || m_ScrollViewWasClamped || widthChanged)
			{
				if (cursorPosition.x > xOffset + contentViewportWidth - cursorWidth || (xOffset > 0f && widthChanged))
				{
					float a = Mathf.Ceil(cursorPosition.x + cursorWidth - contentViewportWidth);
					num = Mathf.Max(a, 0f);
				}
				else if (cursorPosition.x < xOffset + 5f)
				{
					num = Mathf.Max(cursorPosition.x - 5f, 0f);
				}
			}
			if (textEdition.multiline && (Math.Abs(lastCursorPos.y - cursorPosition.y) > 0.05f || m_ScrollViewWasClamped))
			{
				if (cursorPosition.y > base.contentRect.height + yOffset)
				{
					num2 = cursorPosition.y - base.contentRect.height;
				}
				else if (cursorPosition.y < textSelection.lineHeightAtCursorPosition + yOffset + 0.05f)
				{
					num2 = cursorPosition.y - textSelection.lineHeightAtCursorPosition;
				}
			}
			lastCursorPos = cursorPosition;
			if (Math.Abs(xOffset - num) > 0.05f || Math.Abs(yOffset - num2) > 0.05f)
			{
				return new Vector2(num, num2);
			}
			return (scrollView != null) ? scrollView.scrollOffset : scrollOffset;
		}

		internal void SetScrollViewMode()
		{
			if (scrollView != null)
			{
				textElement.RemoveFromClassList(verticalVariantInnerTextElementUssClassName);
				textElement.RemoveFromClassList(verticalHorizontalVariantInnerTextElementUssClassName);
				textElement.RemoveFromClassList(horizontalVariantInnerTextElementUssClassName);
				if (textEdition.multiline && base.computedStyle.whiteSpace == WhiteSpace.Normal)
				{
					textElement.AddToClassList(verticalVariantInnerTextElementUssClassName);
					scrollView.mode = ScrollViewMode.Vertical;
				}
				else if (textEdition.multiline)
				{
					textElement.AddToClassList(verticalHorizontalVariantInnerTextElementUssClassName);
					scrollView.mode = ScrollViewMode.VerticalAndHorizontal;
				}
				else
				{
					textElement.AddToClassList(horizontalVariantInnerTextElementUssClassName);
					scrollView.mode = ScrollViewMode.Horizontal;
				}
			}
		}

		private void SetMultilineContainerStyle()
		{
			if (multilineContainer != null)
			{
				if (base.computedStyle.whiteSpace == WhiteSpace.Normal)
				{
					base.style.overflow = Overflow.Hidden;
				}
				else
				{
					base.style.overflow = (Overflow)2;
				}
			}
		}

		private void RemoveSingleLineComponents()
		{
			RemoveFromClassList(TextInputBaseField<TValueType>.singleLineInputUssClassName);
			textElement.RemoveFromClassList(innerTextElementUssClassName);
			textElement.RemoveFromHierarchy();
			textElement.UnregisterCallback<GeometryChangedEvent>(TextElementOnGeometryChangedEvent);
		}

		private void RemoveMultilineComponents()
		{
			if (scrollView != null)
			{
				scrollView.RemoveFromHierarchy();
				scrollView.contentContainer.UnregisterCallback<GeometryChangedEvent>(ScrollViewOnGeometryChangedEvent);
				scrollView.verticalScroller.slider.UnregisterValueChangedCallback(MakeSureScrollViewDoesNotLeakEvents);
				scrollView.horizontalScroller.slider.UnregisterValueChangedCallback(MakeSureScrollViewDoesNotLeakEvents);
				scrollView = null;
				textElement.RemoveFromClassList(verticalVariantInnerTextElementUssClassName);
				textElement.RemoveFromClassList(verticalHorizontalVariantInnerTextElementUssClassName);
				textElement.RemoveFromClassList(horizontalVariantInnerTextElementUssClassName);
				RemoveFromClassList(TextInputBaseField<TValueType>.multilineInputWithScrollViewUssClassName);
				textElement.RemoveFromClassList(innerTextElementWithScrollViewUssClassName);
			}
			if (multilineContainer != null)
			{
				textElement.transform.position = Vector3.zero;
				multilineContainer.RemoveFromHierarchy();
				textElement.UnregisterCallback<GeometryChangedEvent>(TextElementOnGeometryChangedEvent);
				multilineContainer = null;
				RemoveFromClassList(TextInputBaseField<TValueType>.multilineInputUssClassName);
			}
		}

		internal bool SetVerticalScrollerVisibility(ScrollerVisibility sv)
		{
			if (textEdition.multiline)
			{
				m_VerticalScrollerVisibility = sv;
				if (scrollView == null)
				{
					SetMultiline();
				}
				else
				{
					scrollView.verticalScrollerVisibility = m_VerticalScrollerVisibility;
				}
				return true;
			}
			Debug.LogWarning("Can't SetVerticalScrollerVisibility as the field isn't multiline.");
			return false;
		}
	}

	private static CustomStyleProperty<Color> s_SelectionColorProperty = new CustomStyleProperty<Color>("--unity-selection-color");

	private static CustomStyleProperty<Color> s_CursorColorProperty = new CustomStyleProperty<Color>("--unity-cursor-color");

	private int m_VisualInputTabIndex;

	private TextInputBase m_TextInputBase;

	internal bool m_UpdateTextFromValue;

	internal const int kMaxLengthNone = -1;

	internal const char kMaskCharDefault = '*';

	public new static readonly string ussClassName = "unity-base-text-field";

	public new static readonly string labelUssClassName = ussClassName + "__label";

	public new static readonly string inputUssClassName = ussClassName + "__input";

	internal static readonly string multilineContainerClassName = ussClassName + "__multiline-container";

	public static readonly string singleLineInputUssClassName = inputUssClassName + "--single-line";

	public static readonly string multilineInputUssClassName = inputUssClassName + "--multiline";

	internal static readonly string multilineInputWithScrollViewUssClassName = multilineInputUssClassName + "--scroll-view";

	public static readonly string textInputUssName = "unity-text-input";

	protected internal TextInputBase textInputBase => m_TextInputBase;

	public string text
	{
		get
		{
			return m_TextInputBase.text;
		}
		protected internal set
		{
			m_TextInputBase.text = value;
		}
	}

	public bool isReadOnly
	{
		get
		{
			return textEdition.isReadOnly;
		}
		set
		{
			textEdition.isReadOnly = value;
			this.onIsReadOnlyChanged?.Invoke(value);
		}
	}

	public bool isPasswordField
	{
		get
		{
			return m_TextInputBase.isPasswordField;
		}
		set
		{
			if (m_TextInputBase.isPasswordField != value)
			{
				m_TextInputBase.isPasswordField = value;
				m_TextInputBase.IncrementVersion(VersionChangeType.Repaint);
			}
		}
	}

	public bool autoCorrection
	{
		get
		{
			return textEdition.autoCorrection;
		}
		set
		{
			textEdition.autoCorrection = value;
		}
	}

	public bool hideMobileInput
	{
		get
		{
			return textEdition.hideMobileInput;
		}
		set
		{
			textEdition.hideMobileInput = value;
		}
	}

	public TouchScreenKeyboardType keyboardType
	{
		get
		{
			return textEdition.keyboardType;
		}
		set
		{
			textEdition.keyboardType = value;
		}
	}

	public TouchScreenKeyboard touchScreenKeyboard => textEdition.touchScreenKeyboard;

	public ITextSelection textSelection => m_TextInputBase.textElement.selection;

	public ITextEdition textEdition => m_TextInputBase.textElement.edition;

	public Color selectionColor => textSelection.selectionColor;

	public Color cursorColor => textSelection.cursorColor;

	public int cursorIndex
	{
		get
		{
			return textSelection.cursorIndex;
		}
		set
		{
			textSelection.cursorIndex = value;
		}
	}

	public Vector2 cursorPosition => textSelection.cursorPosition;

	public int selectIndex
	{
		get
		{
			return textSelection.selectIndex;
		}
		set
		{
			textSelection.selectIndex = value;
		}
	}

	public bool selectAllOnFocus
	{
		get
		{
			return textSelection.selectAllOnFocus;
		}
		set
		{
			textSelection.selectAllOnFocus = value;
		}
	}

	public bool selectAllOnMouseUp
	{
		get
		{
			return textSelection.selectAllOnMouseUp;
		}
		set
		{
			textSelection.selectAllOnMouseUp = value;
		}
	}

	public int maxLength
	{
		get
		{
			return textEdition.maxLength;
		}
		set
		{
			textEdition.maxLength = value;
		}
	}

	public bool doubleClickSelectsWord
	{
		get
		{
			return textSelection.doubleClickSelectsWord;
		}
		set
		{
			textSelection.doubleClickSelectsWord = value;
		}
	}

	public bool tripleClickSelectsLine
	{
		get
		{
			return textSelection.tripleClickSelectsLine;
		}
		set
		{
			textSelection.tripleClickSelectsLine = value;
		}
	}

	public bool isDelayed
	{
		get
		{
			return textEdition.isDelayed;
		}
		set
		{
			textEdition.isDelayed = value;
		}
	}

	public char maskChar
	{
		get
		{
			return textEdition.maskChar;
		}
		set
		{
			textEdition.maskChar = value;
		}
	}

	internal bool hasFocus => textInputBase.textElement.hasFocus;

	private protected override bool canSwitchToMixedValue => !textInputBase.textElement.hasFocus || (textInputBase.textElement.hasFocus && focusController != null && focusController.IsPendingFocus(this));

	protected event Action<bool> onIsReadOnlyChanged;

	public void SelectAll()
	{
		textSelection.SelectAll();
	}

	public void SelectNone()
	{
		textSelection.SelectNone();
	}

	public void SelectRange(int cursorIndex, int selectionIndex)
	{
		textSelection.SelectRange(cursorIndex, selectionIndex);
	}

	public bool SetVerticalScrollerVisibility(ScrollerVisibility sv)
	{
		return textInputBase.SetVerticalScrollerVisibility(sv);
	}

	public Vector2 MeasureTextSize(string textToMeasure, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
	{
		return TextUtilities.MeasureVisualElementTextSize(m_TextInputBase.textElement, textToMeasure, width, widthMode, height, heightMode);
	}

	protected abstract string ValueToString(TValueType value);

	protected abstract TValueType StringToValue(string str);

	protected TextInputBaseField(int maxLength, char maskChar, TextInputBase textInputBase)
		: this((string)null, maxLength, maskChar, textInputBase)
	{
	}

	protected TextInputBaseField(string label, int maxLength, char maskChar, TextInputBase textInputBase)
		: base(label, (VisualElement)textInputBase)
	{
		base.tabIndex = 0;
		base.delegatesFocus = true;
		base.labelElement.tabIndex = -1;
		AddToClassList(ussClassName);
		base.labelElement.AddToClassList(labelUssClassName);
		base.visualInput.AddToClassList(inputUssClassName);
		base.visualInput.AddToClassList(singleLineInputUssClassName);
		m_TextInputBase = textInputBase;
		m_TextInputBase.maxLength = maxLength;
		m_TextInputBase.maskChar = maskChar;
		RegisterCallback<CustomStyleResolvedEvent>(OnFieldCustomStyleResolved);
		m_UpdateTextFromValue = true;
	}

	private void OnFieldCustomStyleResolved(CustomStyleResolvedEvent e)
	{
		m_TextInputBase.OnInputCustomStyleResolved(e);
	}

	[EventInterest(new Type[]
	{
		typeof(NavigationSubmitEvent),
		typeof(FocusInEvent),
		typeof(FocusEvent),
		typeof(BlurEvent)
	})]
	protected override void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		base.ExecuteDefaultActionAtTarget(evt);
		if (textEdition.isReadOnly)
		{
			return;
		}
		if (evt.eventTypeId == EventBase<NavigationSubmitEvent>.TypeId() && evt.leafTarget != textInputBase.textElement)
		{
			textInputBase.textElement.Focus();
		}
		else if (evt.eventTypeId == EventBase<FocusInEvent>.TypeId())
		{
			if (base.showMixedValue)
			{
				((INotifyValueChanged<string>)textInputBase.textElement).SetValueWithoutNotify((string)null);
			}
			if (evt.leafTarget == this || evt.leafTarget == base.labelElement)
			{
				m_VisualInputTabIndex = textInputBase.textElement.tabIndex;
				textInputBase.textElement.tabIndex = -1;
			}
		}
		else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
		{
			base.delegatesFocus = false;
		}
		else if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			if (base.showMixedValue)
			{
				UpdateMixedValueContent();
			}
			base.delegatesFocus = true;
			if (evt.leafTarget == this || evt.leafTarget == base.labelElement)
			{
				textInputBase.textElement.tabIndex = m_VisualInputTabIndex;
			}
		}
	}

	protected override void UpdateMixedValueContent()
	{
		if (base.showMixedValue)
		{
			if (m_UpdateTextFromValue)
			{
				((INotifyValueChanged<string>)textInputBase.textElement).SetValueWithoutNotify(BaseField<TValueType>.mixedValueString);
			}
			AddToClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
			base.visualInput?.AddToClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
		}
		else
		{
			UpdateTextFromValue();
			base.visualInput?.RemoveFromClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
			RemoveFromClassList(BaseField<TValueType>.mixedValueLabelUssClassName);
		}
	}

	internal virtual void UpdateValueFromText()
	{
		value = StringToValue(text);
	}

	internal virtual void UpdateTextFromValue()
	{
	}
}
