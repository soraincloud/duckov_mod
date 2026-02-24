using System;

namespace UnityEngine.UIElements;

public class ListViewController : BaseListViewController
{
	protected ListView listView => base.view as ListView;

	protected override VisualElement MakeItem()
	{
		if (listView.makeItem == null)
		{
			if (listView.bindItem != null)
			{
				throw new NotImplementedException("You must specify makeItem if bindItem is specified.");
			}
			return new Label();
		}
		return listView.makeItem();
	}

	protected override void BindItem(VisualElement element, int index)
	{
		if (listView.bindItem == null)
		{
			if (listView.makeItem != null)
			{
				throw new NotImplementedException("You must specify bindItem if makeItem is specified.");
			}
			Label label = (Label)element;
			label.text = listView.itemsSource[index]?.ToString() ?? "null";
		}
		else
		{
			listView.bindItem(element, index);
		}
	}

	protected override void UnbindItem(VisualElement element, int index)
	{
		listView.unbindItem?.Invoke(element, index);
	}

	protected override void DestroyItem(VisualElement element)
	{
		listView.destroyItem?.Invoke(element);
	}
}
