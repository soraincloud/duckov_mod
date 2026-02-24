namespace UnityEngine.UIElements;

public interface ITextSelection
{
	bool isSelectable { get; set; }

	Color cursorColor { get; set; }

	int cursorIndex { get; set; }

	bool doubleClickSelectsWord { get; set; }

	int selectIndex { get; set; }

	Color selectionColor { get; set; }

	bool tripleClickSelectsLine { get; set; }

	internal bool selectAllOnFocus { get; set; }

	internal bool selectAllOnMouseUp { get; set; }

	Vector2 cursorPosition { get; }

	internal float lineHeightAtCursorPosition { get; }

	internal float cursorWidth { get; set; }

	bool HasSelection();

	void SelectAll();

	void SelectNone();

	void SelectRange(int cursorIndex, int selectionIndex);

	internal void MoveTextEnd();
}
