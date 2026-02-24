namespace UnityEngine.UIElements.Internal;

internal class MultiColumnHeaderColumnIcon : Image
{
	public new static readonly string ussClassName = MultiColumnHeaderColumn.ussClassName + "__icon";

	public bool isImageInline { get; set; }

	public MultiColumnHeaderColumnIcon()
	{
		AddToClassList(ussClassName);
		RegisterCallback<CustomStyleResolvedEvent>(delegate
		{
			UpdateClassList();
		});
	}

	public void UpdateClassList()
	{
		base.parent.RemoveFromClassList(MultiColumnHeaderColumn.hasIconUssClassName);
		if (base.image != null || base.sprite != null || base.vectorImage != null)
		{
			base.parent.AddToClassList(MultiColumnHeaderColumn.hasIconUssClassName);
		}
	}
}
