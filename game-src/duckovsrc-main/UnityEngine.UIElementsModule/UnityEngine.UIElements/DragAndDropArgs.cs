namespace UnityEngine.UIElements;

internal struct DragAndDropArgs : IListDragAndDropArgs
{
	public object target { get; set; }

	public int insertAtIndex { get; set; }

	public int parentId { get; set; }

	public int childIndex { get; set; }

	public DragAndDropPosition dragAndDropPosition { get; set; }

	public DragAndDropData dragAndDropData { get; set; }
}
