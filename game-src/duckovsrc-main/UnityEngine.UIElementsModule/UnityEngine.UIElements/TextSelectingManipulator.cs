using System;

namespace UnityEngine.UIElements;

internal class TextSelectingManipulator
{
	internal TextSelectingUtilities m_SelectingUtilities;

	private bool selectAllOnMouseUp;

	private TextElement m_TextElement;

	private Vector2 m_ClickStartPosition;

	private bool m_Dragged;

	private bool m_IsClicking;

	private const int k_DragThresholdSqr = 16;

	private int m_ConsecutiveMouseDownCount;

	private long m_LastMouseDownTimeStamp = 0L;

	private readonly Event m_ImguiEvent = new Event();

	internal bool isClicking
	{
		get
		{
			return m_IsClicking;
		}
		set
		{
			if (m_IsClicking != value)
			{
				m_IsClicking = value;
			}
		}
	}

	internal int cursorIndex
	{
		get
		{
			return m_SelectingUtilities?.cursorIndex ?? (-1);
		}
		set
		{
			m_SelectingUtilities.cursorIndex = value;
		}
	}

	internal int selectIndex
	{
		get
		{
			return m_SelectingUtilities?.selectIndex ?? (-1);
		}
		set
		{
			m_SelectingUtilities.selectIndex = value;
		}
	}

	public TextSelectingManipulator(TextElement textElement)
	{
		m_TextElement = textElement;
		m_SelectingUtilities = new TextSelectingUtilities(m_TextElement.uitkTextHandle);
		TextSelectingUtilities selectingUtilities = m_SelectingUtilities;
		selectingUtilities.OnCursorIndexChange = (Action)Delegate.Combine(selectingUtilities.OnCursorIndexChange, new Action(OnCursorIndexChange));
		TextSelectingUtilities selectingUtilities2 = m_SelectingUtilities;
		selectingUtilities2.OnSelectIndexChange = (Action)Delegate.Combine(selectingUtilities2.OnSelectIndexChange, new Action(OnSelectIndexChange));
		TextSelectingUtilities selectingUtilities3 = m_SelectingUtilities;
		selectingUtilities3.OnRevealCursorChange = (Action)Delegate.Combine(selectingUtilities3.OnRevealCursorChange, new Action(OnRevealCursor));
	}

	private void OnRevealCursor()
	{
		m_TextElement.IncrementVersion(VersionChangeType.Repaint);
	}

	private void OnSelectIndexChange()
	{
		m_TextElement.IncrementVersion(VersionChangeType.Repaint);
		if (HasSelection() && m_TextElement.focusController != null)
		{
			m_TextElement.focusController.selectedTextElement = m_TextElement;
		}
		if (m_SelectingUtilities.revealCursor)
		{
			m_TextElement.edition.UpdateScrollOffset?.Invoke(obj: false);
		}
	}

	private void OnCursorIndexChange()
	{
		m_TextElement.IncrementVersion(VersionChangeType.Repaint);
		if (HasSelection() && m_TextElement.focusController != null)
		{
			m_TextElement.focusController.selectedTextElement = m_TextElement;
		}
		if (m_SelectingUtilities.revealCursor)
		{
			m_TextElement.edition.UpdateScrollOffset?.Invoke(obj: false);
		}
	}

	internal bool RevealCursor()
	{
		return m_SelectingUtilities.revealCursor;
	}

	internal bool HasSelection()
	{
		return m_SelectingUtilities.hasSelection;
	}

	internal bool HasFocus()
	{
		return m_TextElement.hasFocus;
	}

	internal void ExecuteDefaultActionAtTarget(EventBase evt)
	{
		if (!(evt is FocusEvent evt2))
		{
			if (!(evt is BlurEvent evt3))
			{
				if (!(evt is PointerDownEvent evt4))
				{
					if (!(evt is KeyDownEvent evt5))
					{
						if (!(evt is PointerMoveEvent evt6))
						{
							if (!(evt is PointerUpEvent evt7))
							{
								if (!(evt is ValidateCommandEvent evt8))
								{
									if (evt is ExecuteCommandEvent evt9)
									{
										OnExecuteCommandEvent(evt9);
									}
								}
								else
								{
									OnValidateCommandEvent(evt8);
								}
							}
							else
							{
								OnPointerUpEvent(evt7);
							}
						}
						else
						{
							OnPointerMoveEvent(evt6);
						}
					}
					else
					{
						OnKeyDown(evt5);
					}
				}
				else
				{
					OnPointerDownEvent(evt4);
				}
			}
			else
			{
				OnBlurEvent(evt3);
			}
		}
		else
		{
			OnFocusEvent(evt2);
		}
	}

	private void OnFocusEvent(FocusEvent evt)
	{
		selectAllOnMouseUp = false;
		if (PointerDeviceState.GetPressedButtons(PointerId.mousePointerId) != 0 || (m_TextElement.panel.contextType == ContextType.Editor && Event.current == null))
		{
			selectAllOnMouseUp = m_TextElement.selection.selectAllOnMouseUp;
		}
		m_SelectingUtilities.OnFocus(m_TextElement.selection.selectAllOnFocus);
	}

	private void OnBlurEvent(BlurEvent evt)
	{
		selectAllOnMouseUp = m_TextElement.selection.selectAllOnMouseUp;
	}

	private void OnKeyDown(KeyDownEvent evt)
	{
		if (m_TextElement.hasFocus)
		{
			evt.GetEquivalentImguiEvent(m_ImguiEvent);
			if (m_SelectingUtilities.HandleKeyEvent(m_ImguiEvent))
			{
				evt.StopPropagation();
			}
		}
	}

	private void OnPointerDownEvent(PointerDownEvent evt)
	{
		Vector3 vector = evt.localPosition - (Vector3)m_TextElement.contentRect.min;
		if (evt.button != 0)
		{
			return;
		}
		if (evt.timestamp - m_LastMouseDownTimeStamp < Event.GetDoubleClickTime())
		{
			m_ConsecutiveMouseDownCount++;
		}
		else
		{
			m_ConsecutiveMouseDownCount = 1;
		}
		if (m_ConsecutiveMouseDownCount == 2 && m_TextElement.selection.doubleClickSelectsWord)
		{
			if (cursorIndex == 0 && cursorIndex != selectIndex)
			{
				m_SelectingUtilities.MoveCursorToPosition_Internal(vector, evt.shiftKey);
			}
			m_SelectingUtilities.SelectCurrentWord();
			m_SelectingUtilities.MouseDragSelectsWholeWords(on: true);
			m_SelectingUtilities.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
		}
		else if (m_ConsecutiveMouseDownCount == 3 && m_TextElement.selection.tripleClickSelectsLine)
		{
			m_SelectingUtilities.SelectCurrentParagraph();
			m_SelectingUtilities.MouseDragSelectsWholeWords(on: true);
			m_SelectingUtilities.DblClickSnap(TextEditor.DblClickSnapping.PARAGRAPHS);
		}
		else
		{
			m_SelectingUtilities.MoveCursorToPosition_Internal(vector, evt.shiftKey);
			m_TextElement.edition.UpdateScrollOffset?.Invoke(obj: false);
			m_SelectingUtilities.MouseDragSelectsWholeWords(on: false);
			m_SelectingUtilities.DblClickSnap(TextEditor.DblClickSnapping.WORDS);
		}
		m_LastMouseDownTimeStamp = evt.timestamp;
		isClicking = true;
		m_TextElement.CapturePointer(evt.pointerId);
		m_ClickStartPosition = vector;
	}

	private void OnPointerMoveEvent(PointerMoveEvent evt)
	{
		if (isClicking)
		{
			Vector3 vector = evt.localPosition - (Vector3)m_TextElement.contentRect.min;
			m_Dragged = m_Dragged || MoveDistanceQualifiesForDrag(m_ClickStartPosition, vector);
			if (m_Dragged)
			{
				m_SelectingUtilities.SelectToPosition(vector);
				m_TextElement.edition.UpdateScrollOffset?.Invoke(obj: false);
				selectAllOnMouseUp = m_TextElement.selection.selectAllOnMouseUp && !m_SelectingUtilities.hasSelection;
			}
			evt.StopPropagation();
		}
	}

	private void OnPointerUpEvent(PointerUpEvent evt)
	{
		if (evt.button == 0 && isClicking)
		{
			if (selectAllOnMouseUp)
			{
				m_SelectingUtilities.SelectAll();
			}
			selectAllOnMouseUp = false;
			m_Dragged = false;
			isClicking = false;
			m_TextElement.ReleasePointer(evt.pointerId);
			evt.StopPropagation();
		}
	}

	private void OnValidateCommandEvent(ValidateCommandEvent evt)
	{
		if (!m_TextElement.hasFocus)
		{
			return;
		}
		switch (evt.commandName)
		{
		case "Paste":
			return;
		case "Delete":
			return;
		case "UndoRedoPerformed":
			return;
		case "Copy":
			if (!m_SelectingUtilities.hasSelection)
			{
				return;
			}
			break;
		}
		evt.StopPropagation();
	}

	private void OnExecuteCommandEvent(ExecuteCommandEvent evt)
	{
		if (m_TextElement.hasFocus)
		{
			switch (evt.commandName)
			{
			case "OnLostFocus":
				evt.StopPropagation();
				break;
			case "Copy":
				m_SelectingUtilities.Copy();
				evt.StopPropagation();
				break;
			case "SelectAll":
				m_SelectingUtilities.SelectAll();
				evt.StopPropagation();
				break;
			}
		}
	}

	private bool MoveDistanceQualifiesForDrag(Vector2 start, Vector2 current)
	{
		return (start - current).sqrMagnitude >= 16f;
	}
}
