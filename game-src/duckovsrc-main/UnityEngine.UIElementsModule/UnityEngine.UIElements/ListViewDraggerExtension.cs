namespace UnityEngine.UIElements;

internal static class ListViewDraggerExtension
{
	public static ReusableCollectionItem GetRecycledItemFromId(this BaseVerticalCollectionView listView, int id)
	{
		foreach (ReusableCollectionItem activeItem in listView.activeItems)
		{
			if (activeItem.id.Equals(id))
			{
				return activeItem;
			}
		}
		return null;
	}

	public static ReusableCollectionItem GetRecycledItemFromIndex(this BaseVerticalCollectionView listView, int index)
	{
		foreach (ReusableCollectionItem activeItem in listView.activeItems)
		{
			if (activeItem.index.Equals(index))
			{
				return activeItem;
			}
		}
		return null;
	}
}
