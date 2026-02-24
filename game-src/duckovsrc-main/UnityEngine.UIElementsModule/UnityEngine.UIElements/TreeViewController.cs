using System;

namespace UnityEngine.UIElements;

public abstract class TreeViewController : BaseTreeViewController
{
	protected TreeView treeView => base.view as TreeView;

	protected override VisualElement MakeItem()
	{
		if (treeView.makeItem == null)
		{
			if (treeView.bindItem != null)
			{
				throw new NotImplementedException("You must specify makeItem if bindItem is specified.");
			}
			return new Label();
		}
		return treeView.makeItem();
	}

	protected override void BindItem(VisualElement element, int index)
	{
		if (treeView.bindItem == null)
		{
			if (treeView.makeItem != null)
			{
				throw new NotImplementedException("You must specify bindItem if makeItem is specified.");
			}
			Label label = (Label)element;
			label.text = GetItemForIndex(index)?.ToString() ?? "null";
		}
		else
		{
			treeView.bindItem(element, index);
		}
	}

	protected override void UnbindItem(VisualElement element, int index)
	{
		treeView.unbindItem?.Invoke(element, index);
	}

	protected override void DestroyItem(VisualElement element)
	{
		treeView.destroyItem?.Invoke(element);
	}

	internal override object GetItemForId(int id)
	{
		if (this is IDefaultTreeViewController defaultTreeViewController)
		{
			return defaultTreeViewController.GetItemDataForId(id);
		}
		return base.GetItemForId(id);
	}
}
