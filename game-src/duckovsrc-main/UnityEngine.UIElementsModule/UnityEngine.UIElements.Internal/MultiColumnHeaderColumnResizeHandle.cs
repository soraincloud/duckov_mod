namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumnResizeHandle : VisualElement
{
	public static readonly string ussClassName = MultiColumnCollectionHeader.ussClassName + "__column-resize-handle";

	public static readonly string dragAreaUssClassName = ussClassName + "__drag-area";

	public VisualElement dragArea { get; }

	public MultiColumnHeaderColumnResizeHandle()
	{
		AddToClassList(ussClassName);
		dragArea = new VisualElement
		{
			focusable = true
		};
		dragArea.AddToClassList(dragAreaUssClassName);
		Add(dragArea);
	}
}
