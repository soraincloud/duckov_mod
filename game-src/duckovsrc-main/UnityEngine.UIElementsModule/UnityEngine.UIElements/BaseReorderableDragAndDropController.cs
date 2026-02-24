using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal abstract class BaseReorderableDragAndDropController : ICollectionDragAndDropController, IDragAndDropController<IListDragAndDropArgs>, IReorderable
{
	protected readonly BaseVerticalCollectionView m_View;

	protected List<int> m_SortedSelectedIds = new List<int>();

	public virtual bool enableReordering { get; set; } = true;

	public IEnumerable<int> GetSortedSelectedIds()
	{
		return m_SortedSelectedIds;
	}

	protected BaseReorderableDragAndDropController(BaseVerticalCollectionView view)
	{
		m_View = view;
	}

	public virtual bool CanStartDrag(IEnumerable<int> itemIds)
	{
		return enableReordering;
	}

	public virtual StartDragArgs SetupDragAndDrop(IEnumerable<int> itemIds, bool skipText = false)
	{
		m_SortedSelectedIds.Clear();
		string text = string.Empty;
		if (itemIds != null)
		{
			foreach (int itemId in itemIds)
			{
				m_SortedSelectedIds.Add(itemId);
				if (!skipText)
				{
					if (string.IsNullOrEmpty(text))
					{
						Label label = m_View.GetRecycledItemFromId(itemId)?.rootElement.Q<Label>();
						text = ((label != null) ? label.text : $"Item {itemId}");
					}
					else
					{
						text = "<Multiple>";
						skipText = true;
					}
				}
			}
		}
		m_SortedSelectedIds.Sort(CompareId);
		return new StartDragArgs(text, DragVisualMode.Move);
	}

	protected virtual int CompareId(int id1, int id2)
	{
		return id1.CompareTo(id2);
	}

	public abstract DragVisualMode HandleDragAndDrop(IListDragAndDropArgs args);

	public abstract void OnDrop(IListDragAndDropArgs args);

	public virtual void DragCleanup()
	{
	}

	public virtual void HandleAutoExpand(ReusableCollectionItem item, Vector2 pointerPosition)
	{
	}
}
