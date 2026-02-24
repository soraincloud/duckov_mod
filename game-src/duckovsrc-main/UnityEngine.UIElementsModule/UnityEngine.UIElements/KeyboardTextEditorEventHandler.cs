namespace UnityEngine.UIElements;

internal class KeyboardTextEditorEventHandler : TextEditorEventHandler
{
	private readonly Event m_ImguiEvent = new Event();

	internal bool m_Changed;

	private const int k_LineFeed = 10;

	private const int k_Space = 32;

	public KeyboardTextEditorEventHandler(TextElement textElement, TextEditingUtilities editingUtilities)
		: base(textElement, editingUtilities)
	{
		editingUtilities.multiline = textElement.edition.multiline;
	}

	public override void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		base.ExecuteDefaultActionAtTarget(evt);
		if (!(evt is FocusEvent _))
		{
			if (!(evt is BlurEvent _2))
			{
				if (!(evt is KeyDownEvent evt2))
				{
					if (!(evt is ValidateCommandEvent evt3))
					{
						if (!(evt is ExecuteCommandEvent evt4))
						{
							if (!(evt is NavigationMoveEvent evt5))
							{
								if (!(evt is NavigationSubmitEvent evt6))
								{
									if (evt is NavigationCancelEvent evt7)
									{
										OnNavigationEvent(evt7);
									}
								}
								else
								{
									OnNavigationEvent(evt6);
								}
							}
							else
							{
								OnNavigationEvent(evt5);
							}
						}
						else
						{
							OnExecuteCommandEvent(evt4);
						}
					}
					else
					{
						OnValidateCommandEvent(evt3);
					}
				}
				else
				{
					OnKeyDown(evt2);
				}
			}
			else
			{
				OnBlur(_2);
			}
		}
		else
		{
			OnFocus(_);
		}
	}

	private void OnFocus(FocusEvent _)
	{
		GUIUtility.imeCompositionMode = IMECompositionMode.On;
		textElement.edition.SaveValueAndText();
	}

	private void OnBlur(BlurEvent _)
	{
		GUIUtility.imeCompositionMode = IMECompositionMode.Auto;
	}

	private void OnKeyDown(KeyDownEvent evt)
	{
		if (!textElement.hasFocus)
		{
			return;
		}
		m_Changed = false;
		evt.GetEquivalentImguiEvent(m_ImguiEvent);
		if (editingUtilities.HandleKeyEvent(m_ImguiEvent))
		{
			if (textElement.text != editingUtilities.text)
			{
				m_Changed = true;
			}
			evt.StopPropagation();
			goto IL_035d;
		}
		char c = evt.character;
		if ((evt.actionKey && (!evt.altKey || c == '\0')) || (c == '\t' && evt.keyCode == KeyCode.None && evt.modifiers == EventModifiers.None))
		{
			return;
		}
		if (evt.keyCode == KeyCode.Tab || (evt.keyCode == KeyCode.Tab && evt.character == '\t' && evt.modifiers == EventModifiers.Shift))
		{
			if (!textElement.edition.multiline || evt.shiftKey)
			{
				if (evt.ShouldSendNavigationMoveEvent())
				{
					textElement.focusController.FocusNextInDirection(evt.shiftKey ? VisualElementFocusChangeDirection.left : VisualElementFocusChangeDirection.right);
					evt.StopPropagation();
				}
				return;
			}
			if (!evt.ShouldSendNavigationMoveEvent())
			{
				return;
			}
		}
		if (!textElement.edition.multiline && (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return))
		{
			textElement.edition.UpdateValueFromText?.Invoke();
		}
		evt.StopPropagation();
		bool num;
		if (!textElement.edition.multiline)
		{
			if (c == '\n' || c == '\r' || c == '\n')
			{
				num = !evt.altKey;
				goto IL_0205;
			}
		}
		else if (c == '\n')
		{
			num = evt.shiftKey;
			goto IL_0205;
		}
		goto IL_022d;
		IL_022d:
		if (evt.keyCode == KeyCode.Escape)
		{
			textElement.edition.RestoreValueAndText();
			textElement.edition.UpdateValueFromText?.Invoke();
			textElement.edition.MoveFocusToCompositeRoot?.Invoke();
		}
		if (evt.keyCode == KeyCode.Tab)
		{
			c = '\t';
		}
		if (!textElement.edition.AcceptCharacter(c))
		{
			return;
		}
		if (c >= ' ' || evt.keyCode == KeyCode.Tab || (textElement.edition.multiline && !evt.altKey && (c == '\n' || c == '\r' || c == '\n')))
		{
			editingUtilities.Insert(c);
			m_Changed = true;
		}
		else
		{
			bool isCompositionActive = editingUtilities.isCompositionActive;
			if (editingUtilities.UpdateImeState() || isCompositionActive != editingUtilities.isCompositionActive)
			{
				m_Changed = true;
			}
		}
		goto IL_035d;
		IL_0205:
		if (num)
		{
			textElement.edition.MoveFocusToCompositeRoot?.Invoke();
			return;
		}
		goto IL_022d;
		IL_035d:
		if (m_Changed)
		{
			UpdateLabel();
		}
		textElement.edition.UpdateScrollOffset?.Invoke(evt.keyCode == KeyCode.Backspace);
	}

	private void UpdateLabel()
	{
		string text = editingUtilities.text;
		bool flag = editingUtilities.UpdateImeState();
		if (flag && editingUtilities.ShouldUpdateImeWindowPosition())
		{
			editingUtilities.SetImeWindowPosition(new Vector2(textElement.worldBound.x, textElement.worldBound.y));
		}
		string value = editingUtilities.GeneratePreviewString(textElement.enableRichText);
		textElement.edition.UpdateText(value);
		if (!textElement.edition.isDelayed)
		{
			textElement.edition.UpdateValueFromText?.Invoke();
		}
		if (flag)
		{
			editingUtilities.text = text;
			editingUtilities.EnableCursorPreviewState();
		}
		textElement.uitkTextHandle.Update();
	}

	private void OnValidateCommandEvent(ValidateCommandEvent evt)
	{
		if (!textElement.hasFocus)
		{
			return;
		}
		switch (evt.commandName)
		{
		case "SelectAll":
			return;
		case "Cut":
			if (!textElement.selection.HasSelection())
			{
				return;
			}
			break;
		case "Paste":
			if (!editingUtilities.CanPaste())
			{
				return;
			}
			break;
		}
		evt.StopPropagation();
	}

	private void OnExecuteCommandEvent(ExecuteCommandEvent evt)
	{
		if (!textElement.hasFocus)
		{
			return;
		}
		m_Changed = false;
		bool flag = false;
		string text = editingUtilities.text;
		switch (evt.commandName)
		{
		case "OnLostFocus":
			evt.StopPropagation();
			return;
		case "Cut":
			editingUtilities.Cut();
			flag = true;
			evt.StopPropagation();
			break;
		case "Paste":
			editingUtilities.Paste();
			flag = true;
			evt.StopPropagation();
			break;
		case "Delete":
			editingUtilities.Cut();
			flag = true;
			evt.StopPropagation();
			break;
		}
		if (flag)
		{
			if (text != editingUtilities.text)
			{
				m_Changed = true;
			}
			evt.StopPropagation();
		}
		if (m_Changed)
		{
			UpdateLabel();
		}
		textElement.edition.UpdateScrollOffset?.Invoke(obj: false);
	}

	private void OnNavigationEvent<TEvent>(NavigationEventBase<TEvent> evt) where TEvent : NavigationEventBase<TEvent>, new()
	{
		if (evt.deviceType == NavigationDeviceType.Keyboard || evt.deviceType == NavigationDeviceType.Unknown)
		{
			evt.StopPropagation();
			evt.PreventDefault();
		}
	}
}
