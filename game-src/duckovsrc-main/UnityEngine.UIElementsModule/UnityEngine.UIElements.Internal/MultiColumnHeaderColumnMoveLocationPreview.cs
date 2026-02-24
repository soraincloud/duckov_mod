namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumnMoveLocationPreview : VisualElement
{
	public static readonly string ussClassName = MultiColumnHeaderColumn.ussClassName + "__move-location-preview";

	public static readonly string visualUssClassName = ussClassName + "__visual";

	public MultiColumnHeaderColumnMoveLocationPreview()
	{
		AddToClassList(ussClassName);
		base.pickingMode = PickingMode.Ignore;
		VisualElement visualElement = new VisualElement();
		visualElement.AddToClassList(visualUssClassName);
		visualElement.pickingMode = PickingMode.Ignore;
		Add(visualElement);
	}
}
