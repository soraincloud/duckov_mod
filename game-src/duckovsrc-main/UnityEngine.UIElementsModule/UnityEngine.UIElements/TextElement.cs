using System;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;

namespace UnityEngine.UIElements;

public class TextElement : BindableElement, ITextElement, INotifyValueChanged<string>, ITextEdition, ITextElementExperimentalFeatures, IExperimentalFeatures, ITextSelection
{
	public new class UxmlFactory : UxmlFactory<TextElement, UxmlTraits>
	{
	}

	public new class UxmlTraits : BindableElement.UxmlTraits
	{
		private UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription
		{
			name = "text"
		};

		private UxmlBoolAttributeDescription m_EnableRichText = new UxmlBoolAttributeDescription
		{
			name = "enable-rich-text",
			defaultValue = true
		};

		private UxmlBoolAttributeDescription m_ParseEscapeSequences = new UxmlBoolAttributeDescription
		{
			name = "parse-escape-sequences",
			defaultValue = false
		};

		private UxmlBoolAttributeDescription m_DisplayTooltipWhenElided = new UxmlBoolAttributeDescription
		{
			name = "display-tooltip-when-elided"
		};

		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
		{
			base.Init(ve, bag, cc);
			TextElement textElement = (TextElement)ve;
			textElement.text = m_Text.GetValueFromBag(bag, cc);
			textElement.enableRichText = m_EnableRichText.GetValueFromBag(bag, cc);
			textElement.parseEscapeSequences = m_ParseEscapeSequences.GetValueFromBag(bag, cc);
			textElement.displayTooltipWhenElided = m_DisplayTooltipWhenElided.GetValueFromBag(bag, cc);
		}
	}

	public static readonly string ussClassName = "unity-text-element";

	private string m_Text = string.Empty;

	private bool m_EnableRichText = true;

	private bool m_ParseEscapeSequences = true;

	private bool m_DisplayTooltipWhenElided = true;

	internal static readonly string k_EllipsisText = "...";

	internal string elidedText;

	private bool m_WasElided;

	internal TextEditingManipulator editingManipulator;

	private bool m_Multiline;

	internal TouchScreenKeyboard m_TouchScreenKeyboard;

	internal TouchScreenKeyboardType m_KeyboardType = TouchScreenKeyboardType.Default;

	private bool m_HideMobileInput;

	private bool m_IsReadOnly = true;

	private int m_MaxLength = -1;

	private string m_RenderedText;

	private string m_OriginalText;

	private char m_MaskChar;

	private bool m_IsPassword;

	private bool m_AutoCorrection;

	private TextSelectingManipulator m_SelectingManipulator;

	private bool m_IsSelectable;

	private Color m_SelectionColor = new Color(0.239f, 0.502f, 0.875f, 0.65f);

	private Color m_CursorColor = new Color(0.706f, 0.706f, 0.706f, 1f);

	private float m_CursorWidth = 1f;

	internal UITKTextHandle uitkTextHandle { get; set; }

	public virtual string text
	{
		get
		{
			return ((INotifyValueChanged<string>)this).value;
		}
		set
		{
			((INotifyValueChanged<string>)this).value = value;
		}
	}

	public bool enableRichText
	{
		get
		{
			return m_EnableRichText;
		}
		set
		{
			if (m_EnableRichText != value)
			{
				m_EnableRichText = value;
				MarkDirtyRepaint();
			}
		}
	}

	public bool parseEscapeSequences
	{
		get
		{
			return m_ParseEscapeSequences;
		}
		set
		{
			if (m_ParseEscapeSequences != value)
			{
				m_ParseEscapeSequences = value;
				MarkDirtyRepaint();
			}
		}
	}

	public bool displayTooltipWhenElided
	{
		get
		{
			return m_DisplayTooltipWhenElided;
		}
		set
		{
			if (m_DisplayTooltipWhenElided != value)
			{
				m_DisplayTooltipWhenElided = value;
				UpdateVisibleText();
				MarkDirtyRepaint();
			}
		}
	}

	public bool isElided { get; private set; }

	internal bool hasFocus => base.elementPanel != null && base.elementPanel.focusController?.GetLeafFocusedElement() == this;

	string INotifyValueChanged<string>.value
	{
		get
		{
			return m_Text ?? string.Empty;
		}
		set
		{
			if (!(m_Text != value))
			{
				return;
			}
			if (base.panel != null)
			{
				using (ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(text, value))
				{
					changeEvent.target = this;
					((INotifyValueChanged<string>)this).SetValueWithoutNotify(value);
					SendEvent(changeEvent);
					return;
				}
			}
			((INotifyValueChanged<string>)this).SetValueWithoutNotify(value);
		}
	}

	internal ITextEdition edition => this;

	bool ITextEdition.multiline
	{
		get
		{
			return m_Multiline;
		}
		set
		{
			if (value != m_Multiline)
			{
				if (!edition.isReadOnly)
				{
					editingManipulator.editingUtilities.multiline = value;
				}
				m_Multiline = value;
			}
		}
	}

	TouchScreenKeyboard ITextEdition.touchScreenKeyboard => m_TouchScreenKeyboard;

	TouchScreenKeyboardType ITextEdition.keyboardType
	{
		get
		{
			return m_KeyboardType;
		}
		set
		{
			m_KeyboardType = value;
		}
	}

	bool ITextEdition.hideMobileInput
	{
		get
		{
			switch (Application.platform)
			{
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
			case RuntimePlatform.WebGLPlayer:
			case RuntimePlatform.tvOS:
				return m_HideMobileInput;
			default:
				return true;
			}
		}
		set
		{
			switch (Application.platform)
			{
			case RuntimePlatform.IPhonePlayer:
			case RuntimePlatform.Android:
			case RuntimePlatform.WebGLPlayer:
			case RuntimePlatform.tvOS:
				m_HideMobileInput = value;
				break;
			default:
				m_HideMobileInput = true;
				break;
			}
		}
	}

	bool ITextEdition.isReadOnly
	{
		get
		{
			return m_IsReadOnly || !base.enabledInHierarchy;
		}
		set
		{
			if (value != m_IsReadOnly)
			{
				editingManipulator = (value ? null : new TextEditingManipulator(this));
				m_IsReadOnly = value;
			}
		}
	}

	int ITextEdition.maxLength
	{
		get
		{
			return m_MaxLength;
		}
		set
		{
			m_MaxLength = value;
			text = edition.CullString(text);
		}
	}

	bool ITextEdition.isDelayed { get; set; }

	Func<char, bool> ITextEdition.AcceptCharacter { get; set; }

	Action<bool> ITextEdition.UpdateScrollOffset { get; set; }

	Action ITextEdition.UpdateValueFromText { get; set; }

	Action ITextEdition.UpdateTextFromValue { get; set; }

	Action ITextEdition.MoveFocusToCompositeRoot { get; set; }

	char ITextEdition.maskChar
	{
		get
		{
			return m_MaskChar;
		}
		set
		{
			if (m_MaskChar != value)
			{
				m_MaskChar = value;
				if (edition.isPassword)
				{
					IncrementVersion(VersionChangeType.Repaint);
				}
			}
		}
	}

	private char effectiveMaskChar => edition.isPassword ? m_MaskChar : '\0';

	bool ITextEdition.isPassword
	{
		get
		{
			return m_IsPassword;
		}
		set
		{
			if (m_IsPassword != value)
			{
				m_IsPassword = value;
				IncrementVersion(VersionChangeType.Repaint);
			}
		}
	}

	bool ITextEdition.autoCorrection
	{
		get
		{
			return m_AutoCorrection;
		}
		set
		{
			m_AutoCorrection = value;
		}
	}

	internal string renderedText
	{
		get
		{
			if (effectiveMaskChar != 0)
			{
				return "".PadLeft(text.Length, effectiveMaskChar) + "\u200b";
			}
			return string.IsNullOrEmpty(m_RenderedText) ? "\u200b" : m_RenderedText;
		}
		set
		{
			m_RenderedText = value + "\u200b";
		}
	}

	internal string originalText => m_OriginalText;

	public new ITextElementExperimentalFeatures experimental => this;

	public ITextSelection selection => this;

	bool ITextSelection.isSelectable
	{
		get
		{
			return m_IsSelectable && base.focusable;
		}
		set
		{
			if (value != m_IsSelectable)
			{
				base.focusable = value;
				m_IsSelectable = value;
			}
		}
	}

	int ITextSelection.cursorIndex
	{
		get
		{
			return selection.isSelectable ? selectingManipulator.cursorIndex : (-1);
		}
		set
		{
			if (selection.isSelectable)
			{
				selectingManipulator.cursorIndex = value;
			}
		}
	}

	int ITextSelection.selectIndex
	{
		get
		{
			return selection.isSelectable ? selectingManipulator.selectIndex : (-1);
		}
		set
		{
			if (selection.isSelectable)
			{
				selectingManipulator.selectIndex = value;
			}
		}
	}

	bool ITextSelection.doubleClickSelectsWord { get; set; } = true;

	bool ITextSelection.tripleClickSelectsLine { get; set; } = true;

	bool ITextSelection.selectAllOnFocus { get; set; } = false;

	bool ITextSelection.selectAllOnMouseUp { get; set; } = false;

	Vector2 ITextSelection.cursorPosition => uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(selection.cursorIndex) + base.contentRect.min;

	float ITextSelection.lineHeightAtCursorPosition => uitkTextHandle.GetLineHeightFromCharacterIndex(selection.cursorIndex);

	Color ITextSelection.selectionColor
	{
		get
		{
			return m_SelectionColor;
		}
		set
		{
			if (!(m_SelectionColor == value))
			{
				m_SelectionColor = value;
				MarkDirtyRepaint();
			}
		}
	}

	Color ITextSelection.cursorColor
	{
		get
		{
			return m_CursorColor;
		}
		set
		{
			if (!(m_CursorColor == value))
			{
				m_CursorColor = value;
				MarkDirtyRepaint();
			}
		}
	}

	private Color cursorColor
	{
		get
		{
			return selection.cursorColor;
		}
		set
		{
			selection.cursorColor = value;
		}
	}

	float ITextSelection.cursorWidth
	{
		get
		{
			return m_CursorWidth;
		}
		set
		{
			if (!Mathf.Approximately(m_CursorWidth, value))
			{
				m_CursorWidth = value;
				MarkDirtyRepaint();
			}
		}
	}

	internal TextSelectingManipulator selectingManipulator => m_SelectingManipulator ?? (m_SelectingManipulator = new TextSelectingManipulator(this));

	public TextElement()
	{
		base.requireMeasureFunction = true;
		base.tabIndex = -1;
		uitkTextHandle = new UITKTextHandle(this);
		AddToClassList(ussClassName);
		base.generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(base.generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
		RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
	}

	private void OnGeometryChanged(GeometryChangedEvent e)
	{
		UpdateVisibleText();
	}

	internal void OnGenerateVisualContent(MeshGenerationContext mgc)
	{
		UpdateVisibleText();
		mgc.Text(this);
		if (ShouldElide() && uitkTextHandle.TextLibraryCanElide())
		{
			isElided = uitkTextHandle.IsElided();
		}
		UpdateTooltip();
		if (selection.HasSelection() && selectingManipulator.HasFocus())
		{
			DrawHighlighting(mgc);
		}
		else if (!edition.isReadOnly && selection.isSelectable && selectingManipulator.RevealCursor())
		{
			DrawCaret(mgc);
		}
	}

	internal string ElideText(string drawText, string ellipsisText, float width, TextOverflowPosition textOverflowPosition)
	{
		float num = base.resolvedStyle.paddingRight;
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		float num2 = Mathf.Clamp(num, 1f / base.scaledPixelsPerPoint, 1f);
		if (MeasureTextSize(drawText, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined).x <= width + num2 || string.IsNullOrEmpty(ellipsisText))
		{
			return drawText;
		}
		string text = ((drawText.Length > 1) ? ellipsisText : drawText);
		if (MeasureTextSize(text, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined).x >= width)
		{
			return text;
		}
		int num3 = drawText.Length - 1;
		int num4 = -1;
		string text2 = drawText;
		int num5 = ((textOverflowPosition == TextOverflowPosition.Start) ? 1 : 0);
		int num6 = ((textOverflowPosition == TextOverflowPosition.Start || textOverflowPosition == TextOverflowPosition.Middle) ? num3 : (num3 - 1));
		for (int num7 = (num5 + num6) / 2; num5 <= num6; num7 = (num5 + num6) / 2)
		{
			switch (textOverflowPosition)
			{
			case TextOverflowPosition.Start:
				text2 = ellipsisText + drawText.Substring(num7, num3 - (num7 - 1));
				break;
			case TextOverflowPosition.End:
				text2 = drawText.Substring(0, num7) + ellipsisText;
				break;
			case TextOverflowPosition.Middle:
				text2 = ((num7 - 1 <= 0) ? "" : drawText.Substring(0, num7 - 1)) + ellipsisText + ((num3 - (num7 - 1) <= 0) ? "" : drawText.Substring(num3 - (num7 - 1)));
				break;
			}
			Vector2 vector = MeasureTextSize(text2, 0f, MeasureMode.Undefined, 0f, MeasureMode.Undefined);
			if (Math.Abs(vector.x - width) < 1E-30f)
			{
				return text2;
			}
			switch (textOverflowPosition)
			{
			case TextOverflowPosition.Start:
				if (vector.x > width)
				{
					if (num4 == num7 - 1)
					{
						return ellipsisText + drawText.Substring(num4, num3 - (num4 - 1));
					}
					num5 = num7 + 1;
				}
				else
				{
					num6 = num7 - 1;
					num4 = num7;
				}
				continue;
			default:
				if (textOverflowPosition != TextOverflowPosition.Middle)
				{
					continue;
				}
				break;
			case TextOverflowPosition.End:
				break;
			}
			if (vector.x > width)
			{
				if (num4 == num7 - 1)
				{
					if (textOverflowPosition == TextOverflowPosition.End)
					{
						return drawText.Substring(0, num4) + ellipsisText;
					}
					return drawText.Substring(0, Mathf.Max(num4 - 1, 0)) + ellipsisText + drawText.Substring(num3 - Mathf.Max(num4 - 1, 0));
				}
				num6 = num7 - 1;
			}
			else
			{
				num5 = num7 + 1;
				num4 = num7;
			}
		}
		return text2;
	}

	private void UpdateTooltip()
	{
		if (displayTooltipWhenElided && isElided)
		{
			base.tooltip = text;
			m_WasElided = true;
		}
		else if (m_WasElided)
		{
			base.tooltip = null;
			m_WasElided = false;
		}
	}

	private void UpdateVisibleText()
	{
		bool flag = ShouldElide();
		if (!flag || !uitkTextHandle.TextLibraryCanElide())
		{
			if (flag)
			{
				elidedText = ElideText(text, k_EllipsisText, base.contentRect.width, base.computedStyle.unityTextOverflowPosition);
				isElided = flag && elidedText != text;
			}
			else
			{
				isElided = false;
			}
		}
	}

	private bool ShouldElide()
	{
		return base.computedStyle.textOverflow == TextOverflow.Ellipsis && base.computedStyle.overflow == OverflowInternal.Hidden;
	}

	public Vector2 MeasureTextSize(string textToMeasure, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
	{
		return TextUtilities.MeasureVisualElementTextSize(this, textToMeasure, width, widthMode, height, heightMode);
	}

	protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
	{
		return MeasureTextSize(renderedText, desiredWidth, widthMode, desiredHeight, heightMode);
	}

	void INotifyValueChanged<string>.SetValueWithoutNotify(string newValue)
	{
		newValue = ((ITextEdition)this).CullString(newValue);
		if (m_Text != newValue)
		{
			renderedText = newValue;
			m_Text = newValue;
			IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);
			if (!string.IsNullOrEmpty(base.viewDataKey))
			{
				SaveViewData();
			}
		}
		if (editingManipulator != null)
		{
			editingManipulator.editingUtilities.text = newValue;
		}
	}

	private void ProcessMenuCommand(string command)
	{
		using ExecuteCommandEvent executeCommandEvent = CommandEventBase<ExecuteCommandEvent>.GetPooled(command);
		executeCommandEvent.target = this;
		SendEvent(executeCommandEvent);
	}

	private void Cut(DropdownMenuAction a)
	{
		ProcessMenuCommand("Cut");
	}

	private void Copy(DropdownMenuAction a)
	{
		ProcessMenuCommand("Copy");
	}

	private void Paste(DropdownMenuAction a)
	{
		ProcessMenuCommand("Paste");
	}

	private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
	{
		if (evt?.target is TextElement)
		{
			if (!edition.isReadOnly)
			{
				evt.menu.AppendAction("Cut", Cut, CutActionStatus);
				evt.menu.AppendAction("Copy", Copy, CopyActionStatus);
				evt.menu.AppendAction("Paste", Paste, PasteActionStatus);
			}
			else
			{
				evt.menu.AppendAction("Copy", Copy, CopyActionStatus);
			}
		}
	}

	private DropdownMenuAction.Status CutActionStatus(DropdownMenuAction a)
	{
		return (base.enabledInHierarchy && selection.HasSelection() && !edition.isPassword) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
	}

	private DropdownMenuAction.Status CopyActionStatus(DropdownMenuAction a)
	{
		return ((!base.enabledInHierarchy || selection.HasSelection()) && !edition.isPassword) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
	}

	private DropdownMenuAction.Status PasteActionStatus(DropdownMenuAction a)
	{
		bool flag = editingManipulator.editingUtilities.CanPaste();
		return (!base.enabledInHierarchy) ? DropdownMenuAction.Status.Hidden : (flag ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
	}

	[EventInterest(new Type[]
	{
		typeof(ContextualMenuPopulateEvent),
		typeof(FocusInEvent),
		typeof(FocusOutEvent),
		typeof(KeyDownEvent),
		typeof(KeyUpEvent),
		typeof(FocusEvent),
		typeof(BlurEvent),
		typeof(ValidateCommandEvent),
		typeof(ExecuteCommandEvent),
		typeof(PointerDownEvent),
		typeof(PointerUpEvent),
		typeof(PointerMoveEvent),
		typeof(NavigationMoveEvent),
		typeof(NavigationSubmitEvent),
		typeof(NavigationCancelEvent)
	})]
	protected override void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		if (!selection.isSelectable)
		{
			return;
		}
		bool flag = editingManipulator?.editingUtilities.TouchScreenKeyboardShouldBeUsed() ?? false;
		if (!flag || (flag && edition.hideMobileInput))
		{
			selectingManipulator?.ExecuteDefaultActionAtTarget(evt);
		}
		if (!edition.isReadOnly)
		{
			editingManipulator?.ExecuteDefaultActionAtTarget(evt);
		}
		base.elementPanel?.contextualMenuManager?.DisplayMenuIfEventMatches(evt, this);
		if (evt?.eventTypeId == EventBase<ContextualMenuPopulateEvent>.TypeId())
		{
			ContextualMenuPopulateEvent contextualMenuPopulateEvent = evt as ContextualMenuPopulateEvent;
			int count = contextualMenuPopulateEvent.menu.MenuItems().Count;
			BuildContextualMenu(contextualMenuPopulateEvent);
			if (count > 0 && contextualMenuPopulateEvent.menu.MenuItems().Count > count)
			{
				contextualMenuPopulateEvent.menu.InsertSeparator(null, count);
			}
		}
	}

	void ITextEdition.ResetValueAndText()
	{
		string text = (this.text = null);
		m_OriginalText = text;
	}

	void ITextEdition.SaveValueAndText()
	{
		m_OriginalText = text;
	}

	void ITextEdition.RestoreValueAndText()
	{
		text = m_OriginalText;
	}

	void ITextEdition.UpdateText(string value)
	{
		if (m_TouchScreenKeyboard != null && m_TouchScreenKeyboard.text != value)
		{
			m_TouchScreenKeyboard.text = value;
		}
		if (text != value)
		{
			using (InputEvent inputEvent = InputEvent.GetPooled(text, value))
			{
				inputEvent.target = base.parent;
				((INotifyValueChanged<string>)this).SetValueWithoutNotify(value);
				base.parent?.SendEvent(inputEvent);
			}
		}
	}

	string ITextEdition.CullString(string s)
	{
		int maxLength = edition.maxLength;
		if (maxLength >= 0 && s != null && s.Length > maxLength)
		{
			return s.Substring(0, maxLength);
		}
		return s;
	}

	void ITextElementExperimentalFeatures.SetRenderedText(string renderedText)
	{
		this.renderedText = renderedText;
	}

	void ITextSelection.SelectAll()
	{
		if (selection.isSelectable)
		{
			selectingManipulator.m_SelectingUtilities.SelectAll();
		}
	}

	void ITextSelection.SelectNone()
	{
		if (selection.isSelectable)
		{
			selectingManipulator.m_SelectingUtilities.SelectNone();
		}
	}

	void ITextSelection.SelectRange(int cursorIndex, int selectionIndex)
	{
		if (selection.isSelectable)
		{
			selectingManipulator.m_SelectingUtilities.cursorIndex = cursorIndex;
			selectingManipulator.m_SelectingUtilities.selectIndex = selectionIndex;
		}
	}

	bool ITextSelection.HasSelection()
	{
		return selection.isSelectable && selectingManipulator.HasSelection();
	}

	void ITextSelection.MoveTextEnd()
	{
		if (selection.isSelectable)
		{
			selectingManipulator.m_SelectingUtilities.MoveTextEnd();
		}
	}

	private void DrawHighlighting(MeshGenerationContext mgc)
	{
		Color playmodeTintColor = ((base.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white);
		int index = Math.Min(selection.cursorIndex, selection.selectIndex);
		int index2 = Math.Max(selection.cursorIndex, selection.selectIndex);
		Vector2 cursorPositionFromStringIndexUsingLineHeight = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(index);
		Vector2 cursorPositionFromStringIndexUsingLineHeight2 = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(index2);
		int lineNumber = uitkTextHandle.GetLineNumber(index);
		int lineNumber2 = uitkTextHandle.GetLineNumber(index2);
		float lineHeight = uitkTextHandle.GetLineHeight(lineNumber);
		Vector2 min = base.contentRect.min;
		if (m_TouchScreenKeyboard != null && m_HideMobileInput)
		{
			TextInfo textInfo = uitkTextHandle.textInfo;
			int num = ((selection.selectIndex < selection.cursorIndex) ? textInfo.textElementInfo[selection.selectIndex].index : textInfo.textElementInfo[selection.cursorIndex].index);
			int length = ((selection.selectIndex < selection.cursorIndex) ? (selection.cursorIndex - num) : (selection.selectIndex - num));
			m_TouchScreenKeyboard.selection = new RangeInt(num, length);
		}
		if (lineNumber == lineNumber2)
		{
			cursorPositionFromStringIndexUsingLineHeight += min;
			cursorPositionFromStringIndexUsingLineHeight2 += min;
			mgc.Rectangle(new MeshGenerationContextUtils.RectangleParams
			{
				rect = new Rect(cursorPositionFromStringIndexUsingLineHeight.x, cursorPositionFromStringIndexUsingLineHeight.y - lineHeight, cursorPositionFromStringIndexUsingLineHeight2.x - cursorPositionFromStringIndexUsingLineHeight.x, lineHeight),
				color = selection.selectionColor,
				playmodeTintColor = playmodeTintColor
			});
			return;
		}
		for (int i = lineNumber; i <= lineNumber2; i++)
		{
			if (i == lineNumber)
			{
				int lastCharacterAt = GetLastCharacterAt(i);
				cursorPositionFromStringIndexUsingLineHeight2 = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(lastCharacterAt, useXAdvance: true);
			}
			else if (i == lineNumber2)
			{
				int firstCharacterIndex = uitkTextHandle.textInfo.lineInfo[i].firstCharacterIndex;
				cursorPositionFromStringIndexUsingLineHeight = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(firstCharacterIndex);
				cursorPositionFromStringIndexUsingLineHeight2 = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(index2, useXAdvance: true);
			}
			else if (i != lineNumber && i != lineNumber2)
			{
				int firstCharacterIndex = uitkTextHandle.textInfo.lineInfo[i].firstCharacterIndex;
				cursorPositionFromStringIndexUsingLineHeight = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(firstCharacterIndex);
				int lastCharacterAt = GetLastCharacterAt(i);
				cursorPositionFromStringIndexUsingLineHeight2 = uitkTextHandle.GetCursorPositionFromStringIndexUsingLineHeight(lastCharacterAt, useXAdvance: true);
			}
			cursorPositionFromStringIndexUsingLineHeight += min;
			cursorPositionFromStringIndexUsingLineHeight2 += min;
			mgc.Rectangle(new MeshGenerationContextUtils.RectangleParams
			{
				rect = new Rect(cursorPositionFromStringIndexUsingLineHeight.x, cursorPositionFromStringIndexUsingLineHeight.y - lineHeight, cursorPositionFromStringIndexUsingLineHeight2.x - cursorPositionFromStringIndexUsingLineHeight.x, lineHeight),
				color = selection.selectionColor,
				playmodeTintColor = playmodeTintColor
			});
		}
	}

	internal void DrawCaret(MeshGenerationContext mgc)
	{
		Color playmodeTintColor = ((base.panel.contextType == ContextType.Editor) ? UIElementsUtility.editorPlayModeTintColor : Color.white);
		float characterHeightFromIndex = uitkTextHandle.GetCharacterHeightFromIndex(selection.cursorIndex);
		float width = AlignmentUtils.CeilToPixelGrid(selection.cursorWidth, base.scaledPixelsPerPoint);
		mgc.Rectangle(new MeshGenerationContextUtils.RectangleParams
		{
			rect = new Rect(selection.cursorPosition.x, selection.cursorPosition.y - characterHeightFromIndex, width, characterHeightFromIndex),
			color = selection.cursorColor,
			playmodeTintColor = playmodeTintColor
		});
	}

	private int GetLastCharacterAt(int lineIndex)
	{
		int num = uitkTextHandle.textInfo.lineInfo[lineIndex].lastCharacterIndex;
		int firstCharacterIndex = uitkTextHandle.textInfo.lineInfo[lineIndex].firstCharacterIndex;
		TextElementInfo textElementInfo = uitkTextHandle.textInfo.textElementInfo[num];
		while ((textElementInfo.character == '\n' || textElementInfo.character == '\r') && num > firstCharacterIndex)
		{
			num--;
			textElementInfo = uitkTextHandle.textInfo.textElementInfo[num];
		}
		return num;
	}
}
