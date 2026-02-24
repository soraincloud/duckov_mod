namespace UnityEngine.UIElements;

internal class TouchScreenTextEditorEventHandler : TextEditorEventHandler
{
	private IVisualElementScheduledItem m_TouchKeyboardPoller = null;

	private bool m_TouchKeyboardAllowsInPlaceEditing = false;

	private bool m_IsClicking = false;

	public TouchScreenTextEditorEventHandler(TextElement textElement, TextEditingUtilities editingUtilities)
		: base(textElement, editingUtilities)
	{
	}

	private void PollTouchScreenKeyboard()
	{
		m_TouchKeyboardAllowsInPlaceEditing = TouchScreenKeyboard.isInPlaceEditingAllowed;
		if (TouchScreenKeyboard.isSupported && !m_TouchKeyboardAllowsInPlaceEditing)
		{
			if (m_TouchKeyboardPoller == null)
			{
				m_TouchKeyboardPoller = textElement?.schedule.Execute(DoPollTouchScreenKeyboard).Every(100L);
			}
			else
			{
				m_TouchKeyboardPoller.Resume();
			}
		}
	}

	private void DoPollTouchScreenKeyboard()
	{
		if (editingUtilities.TouchScreenKeyboardShouldBeUsed())
		{
			if (textElement.m_TouchScreenKeyboard == null)
			{
				return;
			}
			ITextEdition edition = textElement.edition;
			TouchScreenKeyboard touchScreenKeyboard = textElement.m_TouchScreenKeyboard;
			string text = touchScreenKeyboard.text;
			if (touchScreenKeyboard.status != TouchScreenKeyboard.Status.Visible)
			{
				if (touchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled)
				{
					edition.RestoreValueAndText();
				}
				else
				{
					text = touchScreenKeyboard.text;
					if (editingUtilities.text != text)
					{
						edition.UpdateText(text);
						textElement.uitkTextHandle.Update();
					}
				}
				CloseTouchScreenKeyboard();
				if (!edition.isDelayed)
				{
					edition.UpdateValueFromText?.Invoke();
				}
				edition.UpdateTextFromValue?.Invoke();
				textElement.Blur();
			}
			else
			{
				if (editingUtilities.text == text)
				{
					return;
				}
				if (edition.hideMobileInput)
				{
					if (editingUtilities.text != text)
					{
						bool flag = false;
						editingUtilities.text = "";
						string text2 = text;
						for (int i = 0; i < text2.Length; i++)
						{
							char c = text2[i];
							if (!edition.AcceptCharacter(c))
							{
								return;
							}
							if (c != 0)
							{
								editingUtilities.text += c;
								flag = true;
							}
						}
						if (flag)
						{
							UpdateStringPositionFromKeyboard();
						}
						edition.UpdateText(editingUtilities.text);
						textElement.uitkTextHandle.Update();
					}
					else if (!m_IsClicking && touchScreenKeyboard != null && touchScreenKeyboard.canGetSelection)
					{
						UpdateStringPositionFromKeyboard();
					}
				}
				else
				{
					edition.UpdateText(text);
					textElement.uitkTextHandle.Update();
				}
				if (!edition.isDelayed)
				{
					edition.UpdateValueFromText?.Invoke();
				}
				edition.UpdateTextFromValue?.Invoke();
				textElement.edition.UpdateScrollOffset?.Invoke(obj: false);
			}
		}
		else
		{
			CloseTouchScreenKeyboard();
		}
	}

	private void UpdateStringPositionFromKeyboard()
	{
		if (textElement.m_TouchScreenKeyboard != null)
		{
			RangeInt selection = textElement.m_TouchScreenKeyboard.selection;
			int start = selection.start;
			int end = selection.end;
			if (textElement.selection.selectIndex != start)
			{
				textElement.selection.selectIndex = start;
			}
			if (textElement.selection.cursorIndex != end)
			{
				textElement.selection.cursorIndex = end;
			}
		}
	}

	private void CloseTouchScreenKeyboard()
	{
		if (textElement.m_TouchScreenKeyboard != null)
		{
			textElement.m_TouchScreenKeyboard.active = false;
			textElement.m_TouchScreenKeyboard = null;
			m_TouchKeyboardPoller?.Pause();
			TouchScreenKeyboard.hideInput = true;
		}
	}

	private void OpenTouchScreenKeyboard()
	{
		ITextEdition edition = textElement.edition;
		TouchScreenKeyboard.hideInput = edition.hideMobileInput;
		textElement.m_TouchScreenKeyboard = TouchScreenKeyboard.Open(textElement.text, edition.keyboardType, !edition.isPassword && edition.autoCorrection, edition.multiline, edition.isPassword);
		if (edition.hideMobileInput)
		{
			int selectIndex = textElement.selection.selectIndex;
			int cursorIndex = textElement.selection.cursorIndex;
			int length = ((selectIndex < cursorIndex) ? (cursorIndex - selectIndex) : (selectIndex - cursorIndex));
			int start = ((selectIndex < cursorIndex) ? selectIndex : cursorIndex);
			textElement.m_TouchScreenKeyboard.selection = new RangeInt(start, length);
		}
		else
		{
			textElement.m_TouchScreenKeyboard.selection = new RangeInt(textElement.m_TouchScreenKeyboard.text?.Length ?? 0, 0);
		}
	}

	public override void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		base.ExecuteDefaultActionAtTarget(evt);
		if (!editingUtilities.TouchScreenKeyboardShouldBeUsed() || textElement.edition.isReadOnly)
		{
			return;
		}
		if (!(evt is PointerDownEvent))
		{
			if (!(evt is PointerUpEvent evt2))
			{
				if (!(evt is FocusInEvent))
				{
					if (evt is FocusOutEvent evt3)
					{
						OnFocusOutEvent(evt3);
					}
				}
				else
				{
					OnFocusInEvent();
				}
			}
			else
			{
				OnPointerUpEvent(evt2);
			}
		}
		else
		{
			OnPointerDownEvent();
		}
	}

	private void OnPointerDownEvent()
	{
		m_IsClicking = true;
		if (textElement.m_TouchScreenKeyboard != null && textElement.edition.hideMobileInput)
		{
			int num = textElement.selection.cursorIndex;
			int num2 = textElement.m_TouchScreenKeyboard.text?.Length ?? 0;
			if (num < 0)
			{
				num = 0;
			}
			if (num > num2)
			{
				num = num2;
			}
			textElement.m_TouchScreenKeyboard.selection = new RangeInt(num, 0);
		}
	}

	private void OnPointerUpEvent(PointerUpEvent evt)
	{
		m_IsClicking = false;
		evt.StopPropagation();
	}

	private void OnFocusInEvent()
	{
		if (textElement.m_TouchScreenKeyboard == null)
		{
			OpenTouchScreenKeyboard();
			if (textElement.m_TouchScreenKeyboard != null)
			{
				PollTouchScreenKeyboard();
			}
			textElement.edition.SaveValueAndText();
			textElement.edition.UpdateScrollOffset?.Invoke(obj: false);
		}
	}

	private void OnFocusOutEvent(FocusOutEvent evt)
	{
		TextElement textElement = (TextElement)evt.target;
		TextElement textElement2 = textElement.focusController.m_LastPendingFocusedElement as TextElement;
		if (textElement2 == textElement || textElement2 == null || textElement2.edition.keyboardType != textElement.edition.keyboardType || textElement2.edition.multiline != textElement.edition.multiline || textElement2.edition.hideMobileInput != textElement.edition.hideMobileInput)
		{
			CloseTouchScreenKeyboard();
			return;
		}
		base.textElement.m_TouchScreenKeyboard = null;
		m_TouchKeyboardPoller?.Pause();
	}
}
