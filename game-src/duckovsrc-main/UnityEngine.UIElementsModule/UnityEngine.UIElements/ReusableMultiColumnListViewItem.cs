namespace UnityEngine.UIElements;

internal class ReusableMultiColumnListViewItem : ReusableListViewItem
{
	public override VisualElement rootElement => base.bindableElement;

	public override void Init(VisualElement item)
	{
	}

	public void Init(VisualElement container, Columns columns, bool usesAnimatedDrag)
	{
		int num = 0;
		base.bindableElement = container;
		foreach (Column visible in columns.visibleList)
		{
			if (columns.IsPrimary(visible))
			{
				VisualElement visualElement = container[num];
				VisualElement item = visualElement.GetProperty(MultiColumnController.bindableElementPropertyName) as VisualElement;
				UpdateHierarchy(visualElement, item, usesAnimatedDrag);
				break;
			}
			num++;
		}
	}
}
