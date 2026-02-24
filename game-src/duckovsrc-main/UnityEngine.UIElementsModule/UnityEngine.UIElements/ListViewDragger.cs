using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

internal class ListViewDragger : DragEventsProcessor
{
	internal struct DragPosition : IEquatable<DragPosition>
	{
		public int insertAtIndex;

		public int parentId;

		public int childIndex;

		public ReusableCollectionItem recycledItem;

		public DragAndDropPosition dropPosition;

		public bool Equals(DragPosition other)
		{
			return insertAtIndex == other.insertAtIndex && parentId == other.parentId && childIndex == other.childIndex && object.Equals(recycledItem, other.recycledItem) && dropPosition == other.dropPosition;
		}

		public override bool Equals(object obj)
		{
			return obj is DragPosition other && Equals(other);
		}

		public override int GetHashCode()
		{
			int num = insertAtIndex;
			num = (num * 397) ^ parentId;
			num = (num * 397) ^ childIndex;
			num = (num * 397) ^ (recycledItem?.GetHashCode() ?? 0);
			return (num * 397) ^ (int)dropPosition;
		}
	}

	private DragPosition m_LastDragPosition;

	private VisualElement m_DragHoverBar;

	private VisualElement m_DragHoverItemMarker;

	private VisualElement m_DragHoverSiblingMarker;

	private float m_LeftIndentation = -1f;

	private float m_SiblingBottom = -1f;

	private const int k_AutoScrollAreaSize = 5;

	private const int k_BetweenElementsAreaSize = 5;

	private const int k_PanSpeed = 20;

	private const int k_DragHoverBarHeight = 2;

	protected BaseVerticalCollectionView targetView => m_Target as BaseVerticalCollectionView;

	protected ScrollView targetScrollView => targetView.scrollView;

	public ICollectionDragAndDropController dragAndDropController { get; set; }

	public ListViewDragger(BaseVerticalCollectionView listView)
		: base(listView)
	{
	}

	protected override bool CanStartDrag(Vector3 pointerPosition)
	{
		if (dragAndDropController == null)
		{
			return false;
		}
		if (!targetScrollView.contentContainer.worldBound.Contains(pointerPosition))
		{
			return false;
		}
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem != null && targetView.HasCanStartDrag())
		{
			IEnumerable<int> enumerable2;
			if (!targetView.selectedIds.Any())
			{
				IEnumerable<int> enumerable = new int[1] { recycledItem.id };
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = targetView.selectedIds;
			}
			IEnumerable<int> ids = enumerable2;
			return targetView.RaiseCanStartDrag(recycledItem, ids);
		}
		if (targetView.selectedIds.Any())
		{
			return dragAndDropController.CanStartDrag(targetView.selectedIds);
		}
		return recycledItem != null && dragAndDropController.CanStartDrag(new int[1] { recycledItem.id });
	}

	protected internal override StartDragArgs StartDrag(Vector3 pointerPosition)
	{
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		IEnumerable<int> itemIds;
		if (recycledItem != null)
		{
			if (!targetView.selectedIndices.Contains(recycledItem.index))
			{
				targetView.SetSelection(recycledItem.index);
			}
			itemIds = targetView.selectedIds;
		}
		else
		{
			itemIds = (targetView.selectedIds.Any() ? targetView.selectedIds : Enumerable.Empty<int>());
		}
		StartDragArgs args = dragAndDropController.SetupDragAndDrop(itemIds);
		args = targetView.RaiseSetupDragAndDrop(recycledItem, dragAndDropController.GetSortedSelectedIds(), args);
		args.SetGenericData("__unity-drag-and-drop__source-view", targetView);
		return args;
	}

	protected internal override void UpdateDrag(Vector3 pointerPosition)
	{
		DragPosition dragPosition = default(DragPosition);
		DragVisualMode visualMode = GetVisualMode(pointerPosition, ref dragPosition);
		if (visualMode == DragVisualMode.Rejected)
		{
			ClearDragAndDropUI(dragCancelled: false);
		}
		else
		{
			HandleDragAndScroll(pointerPosition);
			HandleAutoExpansion(pointerPosition);
			ApplyDragAndDropUI(dragPosition);
		}
		base.dragAndDrop.SetVisualMode(visualMode);
		base.dragAndDrop.UpdateDrag(pointerPosition);
	}

	private DragVisualMode GetVisualMode(Vector3 pointerPosition, ref DragPosition dragPosition)
	{
		if (dragAndDropController == null)
		{
			return DragVisualMode.Rejected;
		}
		bool flag = TryGetDragPosition(pointerPosition, ref dragPosition);
		DragAndDropArgs dragAndDropArgs = MakeDragAndDropArgs(dragPosition);
		DragVisualMode dragVisualMode = targetView.RaiseHandleDragAndDrop(pointerPosition, dragAndDropArgs);
		if (dragVisualMode != DragVisualMode.None)
		{
			return dragVisualMode;
		}
		return flag ? dragAndDropController.HandleDragAndDrop(dragAndDropArgs) : DragVisualMode.Rejected;
	}

	protected internal override void OnDrop(Vector3 pointerPosition)
	{
		DragPosition dragPosition = default(DragPosition);
		if (!TryGetDragPosition(pointerPosition, ref dragPosition))
		{
			return;
		}
		DragAndDropArgs dragAndDropArgs = MakeDragAndDropArgs(dragPosition);
		switch (targetView.RaiseDrop(pointerPosition, dragAndDropArgs))
		{
		default:
			base.dragAndDrop.AcceptDrag();
			break;
		case DragVisualMode.Rejected:
			dragAndDropController.DragCleanup();
			break;
		case DragVisualMode.None:
			if (dragAndDropController.HandleDragAndDrop(dragAndDropArgs) != DragVisualMode.Rejected)
			{
				dragAndDropController.OnDrop(dragAndDropArgs);
				base.dragAndDrop.AcceptDrag();
			}
			else
			{
				dragAndDropController.DragCleanup();
			}
			break;
		}
	}

	internal void HandleDragAndScroll(Vector2 pointerPosition)
	{
		bool flag = pointerPosition.y < targetScrollView.worldBound.yMin + 5f;
		bool flag2 = pointerPosition.y > targetScrollView.worldBound.yMax - 5f;
		if (flag || flag2)
		{
			Vector2 scrollOffset = targetScrollView.scrollOffset + (flag ? Vector2.down : Vector2.up) * 20f;
			scrollOffset.y = Mathf.Clamp(scrollOffset.y, 0f, Mathf.Max(0f, targetScrollView.contentContainer.worldBound.height - targetScrollView.contentViewport.worldBound.height));
			targetScrollView.scrollOffset = scrollOffset;
		}
	}

	private void HandleAutoExpansion(Vector2 pointerPosition)
	{
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem != null)
		{
			dragAndDropController.HandleAutoExpand(recycledItem, pointerPosition);
		}
	}

	private void ApplyDragAndDropUI(DragPosition dragPosition)
	{
		if (m_LastDragPosition.Equals(dragPosition))
		{
			return;
		}
		if (m_DragHoverBar == null)
		{
			m_DragHoverBar = new VisualElement();
			m_DragHoverBar.AddToClassList(BaseVerticalCollectionView.dragHoverBarUssClassName);
			m_DragHoverBar.style.width = targetView.localBound.width;
			m_DragHoverBar.style.visibility = Visibility.Hidden;
			m_DragHoverBar.pickingMode = PickingMode.Ignore;
			targetView.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
			targetScrollView.contentViewport.Add(m_DragHoverBar);
		}
		if (m_DragHoverItemMarker == null && targetView is BaseTreeView)
		{
			m_DragHoverItemMarker = new VisualElement();
			m_DragHoverItemMarker.AddToClassList(BaseVerticalCollectionView.dragHoverMarkerUssClassName);
			m_DragHoverItemMarker.style.visibility = Visibility.Hidden;
			m_DragHoverItemMarker.pickingMode = PickingMode.Ignore;
			m_DragHoverBar.Add(m_DragHoverItemMarker);
			m_DragHoverSiblingMarker = new VisualElement();
			m_DragHoverSiblingMarker.AddToClassList(BaseVerticalCollectionView.dragHoverMarkerUssClassName);
			m_DragHoverSiblingMarker.style.visibility = Visibility.Hidden;
			m_DragHoverSiblingMarker.pickingMode = PickingMode.Ignore;
			targetScrollView.contentViewport.Add(m_DragHoverSiblingMarker);
		}
		ClearDragAndDropUI(dragCancelled: false);
		m_LastDragPosition = dragPosition;
		switch (dragPosition.dropPosition)
		{
		case DragAndDropPosition.OverItem:
			dragPosition.recycledItem.rootElement.AddToClassList(BaseVerticalCollectionView.itemDragHoverUssClassName);
			break;
		case DragAndDropPosition.BetweenItems:
		{
			if (dragPosition.insertAtIndex == 0)
			{
				PlaceHoverBarAt(0f);
				break;
			}
			ReusableCollectionItem recycledItemFromIndex2 = targetView.GetRecycledItemFromIndex(dragPosition.insertAtIndex - 1);
			ReusableCollectionItem recycledItemFromIndex3 = targetView.GetRecycledItemFromIndex(dragPosition.insertAtIndex);
			PlaceHoverBarAtElement(recycledItemFromIndex2 ?? recycledItemFromIndex3);
			break;
		}
		case DragAndDropPosition.OutsideItems:
		{
			ReusableCollectionItem recycledItemFromIndex = targetView.GetRecycledItemFromIndex(targetView.itemsSource.Count - 1);
			if (recycledItemFromIndex != null)
			{
				PlaceHoverBarAtElement(recycledItemFromIndex);
			}
			else
			{
				PlaceHoverBarAt(0f);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("dropPosition", dragPosition.dropPosition, "Unsupported dropPosition value");
		}
		void GeometryChangedCallback(GeometryChangedEvent e)
		{
			m_DragHoverBar.style.width = targetView.localBound.width;
		}
	}

	protected virtual bool TryGetDragPosition(Vector2 pointerPosition, ref DragPosition dragPosition)
	{
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem == null)
		{
			if (!targetView.worldBound.Contains(pointerPosition))
			{
				return false;
			}
			dragPosition.dropPosition = DragAndDropPosition.OutsideItems;
			if (pointerPosition.y >= targetScrollView.contentContainer.worldBound.yMax)
			{
				dragPosition.insertAtIndex = targetView.itemsSource.Count;
			}
			else
			{
				dragPosition.insertAtIndex = 0;
			}
			HandleTreePosition(pointerPosition, ref dragPosition);
			return true;
		}
		if (recycledItem.rootElement.worldBound.yMax - pointerPosition.y < 5f)
		{
			dragPosition.insertAtIndex = recycledItem.index + 1;
			dragPosition.dropPosition = DragAndDropPosition.BetweenItems;
		}
		else if (pointerPosition.y - recycledItem.rootElement.worldBound.yMin > 5f)
		{
			Vector2 scrollOffset = targetScrollView.scrollOffset;
			targetView.ScrollToItem(recycledItem.index);
			if (!Mathf.Approximately(scrollOffset.x, targetScrollView.scrollOffset.x) || !Mathf.Approximately(scrollOffset.y, targetScrollView.scrollOffset.y))
			{
				return TryGetDragPosition(pointerPosition, ref dragPosition);
			}
			dragPosition.recycledItem = recycledItem;
			dragPosition.insertAtIndex = recycledItem.index;
			dragPosition.dropPosition = DragAndDropPosition.OverItem;
		}
		else
		{
			dragPosition.insertAtIndex = recycledItem.index;
			dragPosition.dropPosition = DragAndDropPosition.BetweenItems;
		}
		HandleTreePosition(pointerPosition, ref dragPosition);
		return true;
	}

	private void HandleTreePosition(Vector2 pointerPosition, ref DragPosition dragPosition)
	{
		dragPosition.parentId = -1;
		dragPosition.childIndex = -1;
		m_LeftIndentation = -1f;
		m_SiblingBottom = -1f;
		if (targetView is BaseTreeView baseTreeView && dragPosition.insertAtIndex >= 0)
		{
			BaseTreeViewController viewController = baseTreeView.viewController;
			if (dragPosition.dropPosition == DragAndDropPosition.OverItem)
			{
				dragPosition.parentId = viewController.GetIdForIndex(dragPosition.insertAtIndex);
				dragPosition.childIndex = -1;
			}
			else if (dragPosition.insertAtIndex <= 0)
			{
				dragPosition.childIndex = 0;
			}
			else
			{
				HandleSiblingInsertionAtAvailableDepthsAndChangeTargetIfNeeded(ref dragPosition, pointerPosition);
			}
		}
	}

	private void HandleSiblingInsertionAtAvailableDepthsAndChangeTargetIfNeeded(ref DragPosition dragPosition, Vector2 pointerPosition)
	{
		if (!(targetView is BaseTreeView { viewController: var viewController } baseTreeView))
		{
			return;
		}
		int insertAtIndex = dragPosition.insertAtIndex;
		int idForIndex = viewController.GetIdForIndex(insertAtIndex);
		GetPreviousAndNextItemsIgnoringDraggedItems(dragPosition.insertAtIndex, out var previousItemId, out var nextItemId);
		if (previousItemId == -1)
		{
			return;
		}
		bool flag = viewController.HasChildren(previousItemId) && baseTreeView.IsExpanded(previousItemId);
		int indentationDepth = viewController.GetIndentationDepth(previousItemId);
		int indentationDepth2 = viewController.GetIndentationDepth(nextItemId);
		int num = ((nextItemId != -1) ? indentationDepth2 : 0);
		int num2 = viewController.GetIndentationDepth(previousItemId) + (flag ? 1 : 0);
		int num3 = previousItemId;
		float num4 = 15f;
		float num5 = 15f;
		VisualElement rootElementForId = baseTreeView.GetRootElementForId(previousItemId);
		if (indentationDepth > 0 && rootElementForId != null)
		{
			VisualElement visualElement = rootElementForId.Q(BaseTreeView.itemIndentUssClassName);
			VisualElement visualElement2 = rootElementForId.Q(BaseTreeView.itemToggleUssClassName);
			num4 = visualElement2.layout.width;
			num5 = visualElement.layout.width / (float)indentationDepth;
		}
		else
		{
			int indentationDepth3 = baseTreeView.viewController.GetIndentationDepth(idForIndex);
			if (indentationDepth3 > 0)
			{
				VisualElement rootElementForId2 = baseTreeView.GetRootElementForId(idForIndex);
				VisualElement visualElement3 = rootElementForId2.Q(BaseTreeView.itemIndentUssClassName);
				VisualElement visualElement4 = rootElementForId2.Q(BaseTreeView.itemToggleUssClassName);
				num4 = visualElement4.layout.width;
				num5 = visualElement3.layout.width / (float)indentationDepth3;
			}
		}
		if (num2 <= num)
		{
			m_LeftIndentation = num4 + num5 * (float)num;
			if (flag)
			{
				dragPosition.parentId = previousItemId;
				dragPosition.childIndex = 0;
				return;
			}
			dragPosition.parentId = viewController.GetParentId(previousItemId);
			if (viewController.GetParentId(nextItemId) == viewController.GetIdForIndex(dragPosition.insertAtIndex))
			{
				dragPosition.childIndex = viewController.GetChildIndexForId(previousItemId) + 1;
			}
			else
			{
				dragPosition.childIndex = viewController.GetChildIndexForId(nextItemId);
			}
			return;
		}
		int num6 = Mathf.FloorToInt((baseTreeView.scrollView.contentContainer.WorldToLocal(pointerPosition).x - num4) / num5);
		if (num6 >= num2)
		{
			m_LeftIndentation = num4 + num5 * (float)num2;
			if (flag)
			{
				dragPosition.parentId = previousItemId;
				dragPosition.childIndex = 0;
			}
			else
			{
				dragPosition.parentId = viewController.GetParentId(previousItemId);
				dragPosition.childIndex = viewController.GetChildIndexForId(previousItemId) + 1;
			}
			return;
		}
		int num7 = viewController.GetIndentationDepth(num3);
		while (num7 > num && num7 != num6)
		{
			num3 = viewController.GetParentId(num3);
			num7--;
		}
		if (num3 != idForIndex)
		{
			VisualElement rootElementForId3 = baseTreeView.GetRootElementForId(num3);
			if (rootElementForId3 != null)
			{
				VisualElement contentViewport = targetScrollView.contentViewport;
				Rect rect = contentViewport.WorldToLocal(rootElementForId3.worldBound);
				if (contentViewport.localBound.yMin < rect.yMax && rect.yMax < contentViewport.localBound.yMax)
				{
					m_SiblingBottom = rect.yMax;
				}
			}
		}
		dragPosition.parentId = viewController.GetParentId(num3);
		dragPosition.childIndex = viewController.GetChildIndexForId(num3) + 1;
		m_LeftIndentation = num4 + num5 * (float)num7;
	}

	private void GetPreviousAndNextItemsIgnoringDraggedItems(int insertAtIndex, out int previousItemId, out int nextItemId)
	{
		previousItemId = (nextItemId = -1);
		int num = insertAtIndex - 1;
		int i = insertAtIndex;
		while (num >= 0)
		{
			int idForIndex = targetView.viewController.GetIdForIndex(num);
			if (!dragAndDropController.GetSortedSelectedIds().Contains(idForIndex))
			{
				previousItemId = idForIndex;
				break;
			}
			num--;
		}
		for (; i < targetView.itemsSource.Count; i++)
		{
			int idForIndex2 = targetView.viewController.GetIdForIndex(i);
			if (!dragAndDropController.GetSortedSelectedIds().Contains(idForIndex2))
			{
				nextItemId = idForIndex2;
				break;
			}
		}
	}

	protected DragAndDropArgs MakeDragAndDropArgs(DragPosition dragPosition)
	{
		object target = null;
		ReusableCollectionItem recycledItem = dragPosition.recycledItem;
		if (recycledItem != null)
		{
			target = targetView.viewController.GetItemForIndex(recycledItem.index);
		}
		return new DragAndDropArgs
		{
			target = target,
			insertAtIndex = dragPosition.insertAtIndex,
			parentId = dragPosition.parentId,
			childIndex = dragPosition.childIndex,
			dragAndDropPosition = dragPosition.dropPosition,
			dragAndDropData = DragAndDropUtility.GetDragAndDrop(m_Target.panel).data
		};
	}

	private float GetHoverBarTopPosition(ReusableCollectionItem item)
	{
		VisualElement contentViewport = targetScrollView.contentViewport;
		return Mathf.Min(contentViewport.WorldToLocal(item.rootElement.worldBound).yMax, contentViewport.localBound.yMax - 2f);
	}

	private void PlaceHoverBarAtElement(ReusableCollectionItem item)
	{
		PlaceHoverBarAt(GetHoverBarTopPosition(item), m_LeftIndentation, m_SiblingBottom);
	}

	private void PlaceHoverBarAt(float top, float indentationPadding = -1f, float siblingBottom = -1f)
	{
		m_DragHoverBar.style.top = top;
		m_DragHoverBar.style.visibility = Visibility.Visible;
		if (m_DragHoverItemMarker != null)
		{
			m_DragHoverItemMarker.style.visibility = Visibility.Visible;
		}
		if (indentationPadding >= 0f)
		{
			m_DragHoverBar.style.marginLeft = indentationPadding;
			m_DragHoverBar.style.width = targetView.localBound.width - indentationPadding;
			if (siblingBottom > 0f && m_DragHoverSiblingMarker != null)
			{
				m_DragHoverSiblingMarker.style.top = siblingBottom;
				m_DragHoverSiblingMarker.style.visibility = Visibility.Visible;
				m_DragHoverSiblingMarker.style.marginLeft = indentationPadding;
			}
		}
		else
		{
			m_DragHoverBar.style.marginLeft = 0f;
			m_DragHoverBar.style.width = targetView.localBound.width;
		}
	}

	protected override void ClearDragAndDropUI(bool dragCancelled)
	{
		if (dragCancelled)
		{
			dragAndDropController.DragCleanup();
		}
		targetView.elementPanel.cursorManager.ResetCursor();
		m_LastDragPosition = default(DragPosition);
		foreach (ReusableCollectionItem activeItem in targetView.activeItems)
		{
			activeItem.rootElement.RemoveFromClassList(BaseVerticalCollectionView.itemDragHoverUssClassName);
		}
		if (m_DragHoverBar != null)
		{
			m_DragHoverBar.style.visibility = Visibility.Hidden;
		}
		if (m_DragHoverItemMarker != null)
		{
			m_DragHoverItemMarker.style.visibility = Visibility.Hidden;
		}
		if (m_DragHoverSiblingMarker != null)
		{
			m_DragHoverSiblingMarker.style.visibility = Visibility.Hidden;
		}
	}

	protected ReusableCollectionItem GetRecycledItem(Vector3 pointerPosition)
	{
		foreach (ReusableCollectionItem activeItem in targetView.activeItems)
		{
			if (activeItem.rootElement.worldBound.Contains(pointerPosition))
			{
				return activeItem;
			}
		}
		return null;
	}
}
