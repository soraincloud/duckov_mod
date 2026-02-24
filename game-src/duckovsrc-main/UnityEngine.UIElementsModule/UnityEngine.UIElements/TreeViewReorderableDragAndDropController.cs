using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal class TreeViewReorderableDragAndDropController : BaseReorderableDragAndDropController
{
	protected class DropData
	{
		public int[] expandedIdsBeforeDrag;

		public int[] draggedIds;

		public int lastItemId = -1;

		public float expandItemBeginTimerMs;

		public Vector2 expandItemBeginPosition;
	}

	private const long k_ExpandUpdateIntervalMs = 10L;

	private const float k_DropExpandTimeoutMs = 700f;

	private const float k_DropDeltaPosition = 100f;

	private const float k_HalfDropBetweenHeight = 4f;

	protected DropData m_DropData = new DropData();

	protected readonly BaseTreeView m_TreeView;

	private IVisualElementScheduledItem m_ExpandDropItemScheduledItem;

	private Action m_ExpandDropItemCallback;

	public TreeViewReorderableDragAndDropController(BaseTreeView view)
		: base(view)
	{
		m_TreeView = view;
		m_ExpandDropItemCallback = ExpandDropItem;
	}

	protected override int CompareId(int id1, int id2)
	{
		if (id1 == id2)
		{
			return id1.CompareTo(id2);
		}
		int num = id1;
		int num2 = id2;
		List<int> value;
		using (CollectionPool<List<int>, int>.Get(out value))
		{
			while (num != -1)
			{
				value.Add(num);
				num = m_TreeView.viewController.GetParentId(num);
			}
			List<int> value2;
			using (CollectionPool<List<int>, int>.Get(out value2))
			{
				while (num2 != -1)
				{
					value2.Add(num2);
					num2 = m_TreeView.viewController.GetParentId(num2);
				}
				value.Add(-1);
				value2.Add(-1);
				for (int i = 0; i < value.Count; i++)
				{
					int item = value[i];
					int num3 = value2.IndexOf(item);
					if (num3 >= 0)
					{
						if (i == 0)
						{
							return -1;
						}
						int id3 = ((i > 0) ? value[i - 1] : id1);
						int id4 = ((num3 > 0) ? value2[num3 - 1] : id2);
						int childIndexForId = m_TreeView.viewController.GetChildIndexForId(id3);
						int childIndexForId2 = m_TreeView.viewController.GetChildIndexForId(id4);
						return childIndexForId.CompareTo(childIndexForId2);
					}
				}
				throw new ArgumentOutOfRangeException("[UI Toolkit] Trying to reorder ids that are not in the same tree.");
			}
		}
	}

	public override StartDragArgs SetupDragAndDrop(IEnumerable<int> itemIds, bool skipText = false)
	{
		StartDragArgs result = base.SetupDragAndDrop(itemIds, skipText);
		m_DropData.draggedIds = GetSortedSelectedIds().ToArray();
		return result;
	}

	public override DragVisualMode HandleDragAndDrop(IListDragAndDropArgs args)
	{
		if (!enableReordering)
		{
			return DragVisualMode.Rejected;
		}
		return (args.dragAndDropData.source == m_TreeView) ? DragVisualMode.Move : DragVisualMode.Rejected;
	}

	public override void OnDrop(IListDragAndDropArgs args)
	{
		int parentId = args.parentId;
		int childIndex = args.childIndex;
		int num = 0;
		bool flag = args.dragAndDropPosition == DragAndDropPosition.OverItem || (parentId == -1 && childIndex == -1);
		List<(int, int)> value;
		using (CollectionPool<List<(int, int)>, (int, int)>.Get(out value))
		{
			int[] draggedIds = m_DropData.draggedIds;
			foreach (int id in draggedIds)
			{
				int parentId2 = m_TreeView.viewController.GetParentId(id);
				int childIndexForId = m_TreeView.viewController.GetChildIndexForId(id);
				value.Add((parentId2, childIndexForId));
				if (flag)
				{
					m_TreeView.viewController.Move(id, parentId, -1, rebuildTree: false);
					continue;
				}
				int childIndex2 = childIndex + num;
				if (parentId2 != parentId || childIndexForId >= childIndex)
				{
					num++;
				}
				m_TreeView.viewController.Move(id, parentId, childIndex2, rebuildTree: false);
			}
			if (args.dragAndDropPosition == DragAndDropPosition.OverItem)
			{
				m_TreeView.viewController.ExpandItem(parentId, expandAllChildren: false, refresh: false);
			}
			m_ExpandDropItemScheduledItem?.Pause();
			m_TreeView.viewController.RebuildTree();
			m_TreeView.RefreshItems();
			for (int j = 0; j < m_DropData.draggedIds.Length; j++)
			{
				int id2 = m_DropData.draggedIds[j];
				(int, int) tuple = value[j];
				int parentId3 = m_TreeView.viewController.GetParentId(id2);
				int childIndexForId2 = m_TreeView.viewController.GetChildIndexForId(id2);
				if (tuple.Item1 != parentId3 || tuple.Item2 != childIndexForId2)
				{
					m_TreeView.viewController.RaiseItemParentChanged(id2, parentId);
				}
			}
		}
	}

	public override void DragCleanup()
	{
		if (m_DropData != null)
		{
			if (m_DropData.expandedIdsBeforeDrag != null)
			{
				RestoreExpanded(new List<int>(m_DropData.expandedIdsBeforeDrag));
			}
			m_DropData = new DropData();
		}
		m_ExpandDropItemScheduledItem?.Pause();
	}

	private void RestoreExpanded(List<int> ids)
	{
		foreach (int allItemId in m_TreeView.viewController.GetAllItemIds())
		{
			if (!ids.Contains(allItemId))
			{
				m_TreeView.CollapseItem(allItemId);
			}
		}
	}

	public override void HandleAutoExpand(ReusableCollectionItem item, Vector2 pointerPosition)
	{
		int id = item.id;
		Rect worldBound = item.bindableElement.worldBound;
		bool flag = new Rect(worldBound.x, worldBound.y + 4f, worldBound.width, worldBound.height - 8f).Contains(pointerPosition);
		Vector2 vector = m_DropData.expandItemBeginPosition - pointerPosition;
		if (id != m_DropData.lastItemId || !flag || vector.sqrMagnitude >= 100f)
		{
			m_DropData.lastItemId = id;
			m_DropData.expandItemBeginTimerMs = Panel.TimeSinceStartupMs();
			m_DropData.expandItemBeginPosition = pointerPosition;
			DelayExpandDropItem();
		}
	}

	private void DelayExpandDropItem()
	{
		if (m_ExpandDropItemScheduledItem == null)
		{
			m_ExpandDropItemScheduledItem = m_TreeView.schedule.Execute(m_ExpandDropItemCallback).Every(10L);
			return;
		}
		m_ExpandDropItemScheduledItem.Pause();
		m_ExpandDropItemScheduledItem.Resume();
	}

	private void ExpandDropItem()
	{
		bool flag = (float)Panel.TimeSinceStartupMs() - m_DropData.expandItemBeginTimerMs > 700f;
		bool flag2 = flag;
		int lastItemId = m_DropData.lastItemId;
		if (!(m_TreeView.viewController.Exists(lastItemId) && flag2))
		{
			return;
		}
		bool flag3 = m_TreeView.viewController.HasChildren(lastItemId);
		bool flag4 = m_TreeView.IsExpanded(lastItemId);
		if (!(!flag3 || flag4))
		{
			DropData dropData = m_DropData;
			if (dropData.expandedIdsBeforeDrag == null)
			{
				dropData.expandedIdsBeforeDrag = m_TreeView.expandedItemIds.ToArray();
			}
			m_DropData.expandItemBeginTimerMs = Panel.TimeSinceStartupMs();
			m_DropData.lastItemId = 0;
			m_TreeView.ExpandItem(lastItemId);
		}
	}
}
