using System.Collections.Generic;
using UnityEngine.Bindings;
using UnityEngine.TextCore.Text;

namespace UnityEngine;

internal class TextEditingUtilities
{
	private TextSelectingUtilities m_TextSelectingUtility;

	private TextHandle m_TextHandle;

	private int m_CursorIndexSavedState = -1;

	internal bool isCompositionActive;

	private bool m_UpdateImeWindowPosition;

	public bool multiline = false;

	private string m_Text;

	private static Dictionary<Event, TextEditOp> s_KeyEditOps;

	private bool hasSelection => m_TextSelectingUtility.hasSelection;

	private string SelectedText => m_TextSelectingUtility.selectedText;

	private int m_iAltCursorPos => m_TextSelectingUtility.iAltCursorPos;

	internal bool revealCursor
	{
		get
		{
			return m_TextSelectingUtility.revealCursor;
		}
		set
		{
			m_TextSelectingUtility.revealCursor = value;
		}
	}

	private int cursorIndex
	{
		get
		{
			return m_TextSelectingUtility.cursorIndex;
		}
		set
		{
			m_TextSelectingUtility.cursorIndex = value;
		}
	}

	private int selectIndex
	{
		get
		{
			return m_TextSelectingUtility.selectIndex;
		}
		set
		{
			m_TextSelectingUtility.selectIndex = value;
		}
	}

	public string text
	{
		get
		{
			return m_Text;
		}
		set
		{
			if (!(value == m_Text))
			{
				m_Text = value ?? string.Empty;
			}
		}
	}

	public TextEditingUtilities(TextSelectingUtilities selectingUtilities, TextHandle textHandle, string text)
	{
		m_TextSelectingUtility = selectingUtilities;
		m_TextHandle = textHandle;
		m_Text = text;
	}

	public bool UpdateImeState()
	{
		if (GUIUtility.compositionString.Length > 0)
		{
			if (!isCompositionActive)
			{
				m_UpdateImeWindowPosition = true;
				ReplaceSelection(string.Empty);
			}
			isCompositionActive = true;
		}
		else
		{
			isCompositionActive = false;
		}
		return isCompositionActive;
	}

	public bool ShouldUpdateImeWindowPosition()
	{
		return m_UpdateImeWindowPosition;
	}

	public void SetImeWindowPosition(Vector2 worldPosition)
	{
		Vector2 cursorPositionFromStringIndexUsingCharacterHeight = m_TextHandle.GetCursorPositionFromStringIndexUsingCharacterHeight(cursorIndex);
		GUIUtility.compositionCursorPos = worldPosition + cursorPositionFromStringIndexUsingCharacterHeight;
	}

	public string GeneratePreviewString(bool richText)
	{
		RestoreCursorState();
		string compositionString = GUIUtility.compositionString;
		if (isCompositionActive)
		{
			return richText ? text.Insert(cursorIndex, "<u>" + compositionString + "</u>") : text.Insert(cursorIndex, compositionString);
		}
		return text;
	}

	public void EnableCursorPreviewState()
	{
		if (m_CursorIndexSavedState == -1)
		{
			m_CursorIndexSavedState = m_TextSelectingUtility.cursorIndex;
			int num = (selectIndex = m_CursorIndexSavedState + GUIUtility.compositionString.Length);
			cursorIndex = num;
		}
	}

	public void RestoreCursorState()
	{
		if (m_CursorIndexSavedState != -1)
		{
			int num = (selectIndex = m_CursorIndexSavedState);
			cursorIndex = num;
			m_CursorIndexSavedState = -1;
		}
	}

	[VisibleToOtherModules]
	internal bool HandleKeyEvent(Event e)
	{
		RestoreCursorState();
		InitKeyActions();
		EventModifiers modifiers = e.modifiers;
		e.modifiers &= ~EventModifiers.CapsLock;
		if (s_KeyEditOps.ContainsKey(e))
		{
			TextEditOp operation = s_KeyEditOps[e];
			PerformOperation(operation);
			e.modifiers = modifiers;
			return true;
		}
		e.modifiers = modifiers;
		return false;
	}

	private void PerformOperation(TextEditOp operation)
	{
		revealCursor = true;
		switch (operation)
		{
		case TextEditOp.MoveLeft:
			m_TextSelectingUtility.MoveLeft();
			break;
		case TextEditOp.MoveRight:
			m_TextSelectingUtility.MoveRight();
			break;
		case TextEditOp.MoveUp:
			m_TextSelectingUtility.MoveUp();
			break;
		case TextEditOp.MoveDown:
			m_TextSelectingUtility.MoveDown();
			break;
		case TextEditOp.MoveLineStart:
			m_TextSelectingUtility.MoveLineStart();
			break;
		case TextEditOp.MoveLineEnd:
			m_TextSelectingUtility.MoveLineEnd();
			break;
		case TextEditOp.MoveWordRight:
			m_TextSelectingUtility.MoveWordRight();
			break;
		case TextEditOp.MoveToStartOfNextWord:
			m_TextSelectingUtility.MoveToStartOfNextWord();
			break;
		case TextEditOp.MoveToEndOfPreviousWord:
			m_TextSelectingUtility.MoveToEndOfPreviousWord();
			break;
		case TextEditOp.MoveWordLeft:
			m_TextSelectingUtility.MoveWordLeft();
			break;
		case TextEditOp.MoveTextStart:
			m_TextSelectingUtility.MoveTextStart();
			break;
		case TextEditOp.MoveTextEnd:
			m_TextSelectingUtility.MoveTextEnd();
			break;
		case TextEditOp.MoveParagraphForward:
			m_TextSelectingUtility.MoveParagraphForward();
			break;
		case TextEditOp.MoveParagraphBackward:
			m_TextSelectingUtility.MoveParagraphBackward();
			break;
		case TextEditOp.MoveGraphicalLineStart:
			m_TextSelectingUtility.MoveGraphicalLineStart();
			break;
		case TextEditOp.MoveGraphicalLineEnd:
			m_TextSelectingUtility.MoveGraphicalLineEnd();
			break;
		case TextEditOp.Delete:
			Delete();
			break;
		case TextEditOp.Backspace:
			Backspace();
			break;
		case TextEditOp.Cut:
			Cut();
			break;
		case TextEditOp.Paste:
			Paste();
			break;
		case TextEditOp.DeleteWordBack:
			DeleteWordBack();
			break;
		case TextEditOp.DeleteLineBack:
			DeleteLineBack();
			break;
		case TextEditOp.DeleteWordForward:
			DeleteWordForward();
			break;
		default:
			Debug.Log("Unimplemented: " + operation);
			break;
		}
	}

	private static void MapKey(string key, TextEditOp action)
	{
		s_KeyEditOps[Event.KeyboardEvent(key)] = action;
	}

	private void InitKeyActions()
	{
		if (s_KeyEditOps == null)
		{
			s_KeyEditOps = new Dictionary<Event, TextEditOp>();
			MapKey("left", TextEditOp.MoveLeft);
			MapKey("right", TextEditOp.MoveRight);
			MapKey("up", TextEditOp.MoveUp);
			MapKey("down", TextEditOp.MoveDown);
			MapKey("delete", TextEditOp.Delete);
			MapKey("backspace", TextEditOp.Backspace);
			MapKey("#backspace", TextEditOp.Backspace);
			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
			{
				MapKey("^left", TextEditOp.MoveGraphicalLineStart);
				MapKey("^right", TextEditOp.MoveGraphicalLineEnd);
				MapKey("&left", TextEditOp.MoveWordLeft);
				MapKey("&right", TextEditOp.MoveWordRight);
				MapKey("&up", TextEditOp.MoveParagraphBackward);
				MapKey("&down", TextEditOp.MoveParagraphForward);
				MapKey("%left", TextEditOp.MoveGraphicalLineStart);
				MapKey("%right", TextEditOp.MoveGraphicalLineEnd);
				MapKey("%up", TextEditOp.MoveTextStart);
				MapKey("%down", TextEditOp.MoveTextEnd);
				MapKey("%x", TextEditOp.Cut);
				MapKey("%v", TextEditOp.Paste);
				MapKey("^d", TextEditOp.Delete);
				MapKey("^h", TextEditOp.Backspace);
				MapKey("^b", TextEditOp.MoveLeft);
				MapKey("^f", TextEditOp.MoveRight);
				MapKey("^a", TextEditOp.MoveLineStart);
				MapKey("^e", TextEditOp.MoveLineEnd);
				MapKey("&delete", TextEditOp.DeleteWordForward);
				MapKey("&backspace", TextEditOp.DeleteWordBack);
				MapKey("%backspace", TextEditOp.DeleteLineBack);
			}
			else
			{
				MapKey("home", TextEditOp.MoveGraphicalLineStart);
				MapKey("end", TextEditOp.MoveGraphicalLineEnd);
				MapKey("%left", TextEditOp.MoveWordLeft);
				MapKey("%right", TextEditOp.MoveWordRight);
				MapKey("%up", TextEditOp.MoveParagraphBackward);
				MapKey("%down", TextEditOp.MoveParagraphForward);
				MapKey("^left", TextEditOp.MoveToEndOfPreviousWord);
				MapKey("^right", TextEditOp.MoveToStartOfNextWord);
				MapKey("^up", TextEditOp.MoveParagraphBackward);
				MapKey("^down", TextEditOp.MoveParagraphForward);
				MapKey("^delete", TextEditOp.DeleteWordForward);
				MapKey("^backspace", TextEditOp.DeleteWordBack);
				MapKey("%backspace", TextEditOp.DeleteLineBack);
				MapKey("^x", TextEditOp.Cut);
				MapKey("^v", TextEditOp.Paste);
				MapKey("#delete", TextEditOp.Cut);
				MapKey("#insert", TextEditOp.Paste);
			}
		}
	}

	public bool DeleteLineBack()
	{
		RestoreCursorState();
		if (hasSelection)
		{
			DeleteSelection();
			return true;
		}
		int num = cursorIndex;
		int num2 = num;
		while (num2-- != 0)
		{
			if (text[num2] == '\n')
			{
				num = num2 + 1;
				break;
			}
		}
		if (num2 == -1)
		{
			num = 0;
		}
		if (cursorIndex != num)
		{
			text = text.Remove(num, cursorIndex - num);
			TextSelectingUtilities textSelectingUtility = m_TextSelectingUtility;
			int num3 = (cursorIndex = num);
			textSelectingUtility.selectIndex = num3;
			return true;
		}
		return false;
	}

	public bool DeleteWordBack()
	{
		RestoreCursorState();
		if (hasSelection)
		{
			DeleteSelection();
			return true;
		}
		int num = m_TextSelectingUtility.FindEndOfPreviousWord(cursorIndex);
		if (cursorIndex != num)
		{
			text = text.Remove(num, cursorIndex - num);
			int num2 = (cursorIndex = num);
			selectIndex = num2;
			return true;
		}
		return false;
	}

	public bool DeleteWordForward()
	{
		RestoreCursorState();
		if (hasSelection)
		{
			DeleteSelection();
			return true;
		}
		int num = m_TextSelectingUtility.FindStartOfNextWord(cursorIndex);
		if (cursorIndex < text.Length)
		{
			text = text.Remove(cursorIndex, num - cursorIndex);
			return true;
		}
		return false;
	}

	public bool Delete()
	{
		RestoreCursorState();
		if (hasSelection)
		{
			DeleteSelection();
			return true;
		}
		if (cursorIndex < text.Length)
		{
			text = text.Remove(cursorIndex, m_TextSelectingUtility.NextCodePointIndex(cursorIndex) - cursorIndex);
			return true;
		}
		return false;
	}

	public bool Backspace()
	{
		RestoreCursorState();
		if (hasSelection)
		{
			DeleteSelection();
			return true;
		}
		if (cursorIndex > 0)
		{
			int num = m_TextSelectingUtility.PreviousCodePointIndex(cursorIndex);
			text = text.Remove(num, cursorIndex - num);
			m_TextSelectingUtility.SetCursorIndexWithoutNotify(num);
			m_TextSelectingUtility.SetSelectIndexWithoutNotify(num);
			m_TextSelectingUtility.ClearCursorPos();
			return true;
		}
		return false;
	}

	public bool DeleteSelection()
	{
		if (cursorIndex == selectIndex)
		{
			return false;
		}
		if (cursorIndex < selectIndex)
		{
			text = text.Substring(0, cursorIndex) + text.Substring(selectIndex, text.Length - selectIndex);
			m_TextSelectingUtility.SetSelectIndexWithoutNotify(cursorIndex);
		}
		else
		{
			text = text.Substring(0, selectIndex) + text.Substring(cursorIndex, text.Length - cursorIndex);
			m_TextSelectingUtility.SetCursorIndexWithoutNotify(selectIndex);
		}
		m_TextSelectingUtility.ClearCursorPos();
		return true;
	}

	public void ReplaceSelection(string replace)
	{
		RestoreCursorState();
		DeleteSelection();
		text = text.Insert(cursorIndex, replace);
		int num = cursorIndex + replace.Length;
		m_TextSelectingUtility.SetCursorIndexWithoutNotify(num);
		m_TextSelectingUtility.SetSelectIndexWithoutNotify(num);
		m_TextSelectingUtility.ClearCursorPos();
	}

	public void Insert(char c)
	{
		ReplaceSelection(c.ToString());
	}

	public void MoveSelectionToAltCursor()
	{
		RestoreCursorState();
		if (m_iAltCursorPos != -1)
		{
			int iAltCursorPos = m_iAltCursorPos;
			string selectedText = SelectedText;
			text = text.Insert(iAltCursorPos, selectedText);
			if (iAltCursorPos < cursorIndex)
			{
				cursorIndex += selectedText.Length;
				selectIndex += selectedText.Length;
			}
			DeleteSelection();
			int num = (cursorIndex = iAltCursorPos);
			selectIndex = num;
			m_TextSelectingUtility.ClearCursorPos();
		}
	}

	public bool CanPaste()
	{
		return GUIUtility.systemCopyBuffer.Length != 0;
	}

	public bool Cut()
	{
		m_TextSelectingUtility.Copy();
		return DeleteSelection();
	}

	public bool Paste()
	{
		RestoreCursorState();
		string text = GUIUtility.systemCopyBuffer;
		if (text != "")
		{
			if (!multiline)
			{
				text = ReplaceNewlinesWithSpaces(text);
			}
			ReplaceSelection(text);
			return true;
		}
		return false;
	}

	private static string ReplaceNewlinesWithSpaces(string value)
	{
		value = value.Replace("\r\n", " ");
		value = value.Replace('\n', ' ');
		value = value.Replace('\r', ' ');
		return value;
	}

	internal void OnBlur()
	{
		revealCursor = false;
		m_TextSelectingUtility.SelectNone();
	}

	internal bool TouchScreenKeyboardShouldBeUsed()
	{
		RuntimePlatform platform = Application.platform;
		RuntimePlatform runtimePlatform = platform;
		RuntimePlatform runtimePlatform2 = runtimePlatform;
		if (runtimePlatform2 == RuntimePlatform.Android || (uint)(runtimePlatform2 - 17) <= 3u)
		{
			return !TouchScreenKeyboard.isInPlaceEditingAllowed;
		}
		return TouchScreenKeyboard.isSupported;
	}
}
