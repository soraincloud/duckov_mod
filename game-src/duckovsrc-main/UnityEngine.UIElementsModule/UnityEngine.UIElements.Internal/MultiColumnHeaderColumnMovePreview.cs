namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumnMovePreview : VisualElement
{
	public static readonly string ussClassName = MultiColumnHeaderColumn.ussClassName + "__move-preview";

	public MultiColumnHeaderColumnMovePreview()
	{
		AddToClassList(ussClassName);
		base.pickingMode = PickingMode.Ignore;
	}
}
