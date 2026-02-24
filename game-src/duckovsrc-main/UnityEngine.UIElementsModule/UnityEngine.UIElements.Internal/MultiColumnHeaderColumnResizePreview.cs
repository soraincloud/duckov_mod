namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumnResizePreview : VisualElement
{
	public static readonly string ussClassName = MultiColumnHeaderColumn.ussClassName + "__resize-preview";

	public static readonly string visualUssClassName = ussClassName + "__visual";

	public MultiColumnHeaderColumnResizePreview()
	{
		AddToClassList(ussClassName);
		base.pickingMode = PickingMode.Ignore;
		VisualElement visualElement = new VisualElement
		{
			pickingMode = PickingMode.Ignore
		};
		visualElement.AddToClassList(visualUssClassName);
		Add(visualElement);
	}
}
