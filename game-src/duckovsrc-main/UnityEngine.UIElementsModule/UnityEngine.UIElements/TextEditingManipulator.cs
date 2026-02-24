using System;

namespace UnityEngine.UIElements;

internal class TextEditingManipulator
{
	private TextElement m_TextElement;

	internal TextEditorEventHandler editingEventHandler;

	internal TextEditingUtilities editingUtilities;

	private bool m_TouchScreenTextFieldInitialized;

	private IVisualElementScheduledItem m_HardwareKeyboardPoller = null;

	private bool touchScreenTextFieldChanged => m_TouchScreenTextFieldInitialized != editingUtilities?.TouchScreenKeyboardShouldBeUsed();

	public TextEditingManipulator(TextElement textElement)
	{
		m_TextElement = textElement;
		editingUtilities = new TextEditingUtilities(textElement.selectingManipulator.m_SelectingUtilities, textElement.uitkTextHandle, textElement.text);
		InitTextEditorEventHandler();
	}

	private void InitTextEditorEventHandler()
	{
		m_TouchScreenTextFieldInitialized = editingUtilities?.TouchScreenKeyboardShouldBeUsed() ?? false;
		if (m_TouchScreenTextFieldInitialized)
		{
			editingEventHandler = new TouchScreenTextEditorEventHandler(m_TextElement, editingUtilities);
		}
		else
		{
			editingEventHandler = new KeyboardTextEditorEventHandler(m_TextElement, editingUtilities);
		}
	}

	internal void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		if (m_TextElement.edition.isReadOnly)
		{
			return;
		}
		if (!(evt is FocusInEvent _))
		{
			if (evt is FocusOutEvent _2)
			{
				OnFocusOutEvent(_2);
			}
		}
		else
		{
			OnFocusInEvent(_);
		}
		editingEventHandler?.ExecuteDefaultActionAtTarget(evt);
	}

	private void OnFocusInEvent(FocusInEvent _)
	{
		m_TextElement.edition.SaveValueAndText();
		m_TextElement.focusController.selectedTextElement = m_TextElement;
		if (touchScreenTextFieldChanged)
		{
			InitTextEditorEventHandler();
		}
		if (m_HardwareKeyboardPoller == null)
		{
			m_HardwareKeyboardPoller = m_TextElement.schedule.Execute((Action)delegate
			{
				if (touchScreenTextFieldChanged)
				{
					InitTextEditorEventHandler();
					m_TextElement.Blur();
				}
			}).Every(250L);
		}
		else
		{
			m_HardwareKeyboardPoller.Resume();
		}
	}

	private void OnFocusOutEvent(FocusOutEvent _)
	{
		m_HardwareKeyboardPoller?.Pause();
		editingUtilities.OnBlur();
	}
}
