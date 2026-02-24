using UnityEngine.UIElements.Experimental;

namespace UnityEngine.UIElements;

internal class ListViewDraggerAnimated : ListViewDragger
{
	private int m_DragStartIndex;

	private int m_CurrentIndex;

	private float m_SelectionHeight;

	private float m_LocalOffsetOnStart;

	private Vector3 m_CurrentPointerPosition;

	private ReusableCollectionItem m_Item;

	private ReusableCollectionItem m_OffsetItem;

	public bool isDragging { get; private set; }

	public ReusableCollectionItem draggedItem => m_Item;

	protected override bool supportsDragEvents => false;

	public ListViewDraggerAnimated(BaseVerticalCollectionView listView)
		: base(listView)
	{
	}

	protected internal override StartDragArgs StartDrag(Vector3 pointerPosition)
	{
		base.targetView.ClearSelection();
		ReusableCollectionItem recycledItem = GetRecycledItem(pointerPosition);
		if (recycledItem == null)
		{
			return new StartDragArgs(string.Empty, DragVisualMode.Rejected);
		}
		base.targetView.SetSelection(recycledItem.index);
		isDragging = true;
		m_Item = recycledItem;
		base.targetView.virtualizationController.StartDragItem(m_Item);
		float y = m_Item.rootElement.layout.y;
		m_SelectionHeight = m_Item.rootElement.layout.height;
		m_Item.rootElement.style.position = Position.Absolute;
		m_Item.rootElement.style.height = m_Item.rootElement.layout.height;
		m_Item.rootElement.style.width = m_Item.rootElement.layout.width;
		m_Item.rootElement.style.top = y;
		m_DragStartIndex = m_Item.index;
		m_CurrentIndex = m_DragStartIndex;
		m_CurrentPointerPosition = pointerPosition;
		m_LocalOffsetOnStart = base.targetScrollView.contentContainer.WorldToLocal(pointerPosition).y - y;
		ReusableCollectionItem recycledItemFromIndex = base.targetView.GetRecycledItemFromIndex(m_CurrentIndex + 1);
		if (recycledItemFromIndex != null)
		{
			m_OffsetItem = recycledItemFromIndex;
			Animate(m_OffsetItem, m_SelectionHeight);
			m_OffsetItem.rootElement.style.paddingTop = m_SelectionHeight;
			if (base.targetView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight)
			{
				m_OffsetItem.rootElement.style.height = base.targetView.fixedItemHeight + m_SelectionHeight;
			}
		}
		return base.dragAndDropController.SetupDragAndDrop(new int[1] { m_Item.index }, skipText: true);
	}

	protected internal override void UpdateDrag(Vector3 pointerPosition)
	{
		if (m_Item == null)
		{
			return;
		}
		HandleDragAndScroll(pointerPosition);
		m_CurrentPointerPosition = pointerPosition;
		Vector2 vector = base.targetScrollView.contentContainer.WorldToLocal(m_CurrentPointerPosition);
		Rect layout = m_Item.rootElement.layout;
		float height = base.targetScrollView.contentContainer.layout.height;
		layout.y = Mathf.Clamp(vector.y - m_LocalOffsetOnStart, 0f, height - m_SelectionHeight);
		float num = base.targetScrollView.contentContainer.resolvedStyle.paddingTop;
		m_CurrentIndex = -1;
		foreach (ReusableCollectionItem activeItem in base.targetView.activeItems)
		{
			if (activeItem.index < 0 || (activeItem.rootElement.style.display == DisplayStyle.None && !activeItem.isDragGhost))
			{
				continue;
			}
			if (activeItem.index == m_Item.index && activeItem.index < base.targetView.itemsSource.Count - 1)
			{
				float expectedItemHeight = base.targetView.virtualizationController.GetExpectedItemHeight(activeItem.index + 1);
				if (layout.y <= num + expectedItemHeight * 0.5f)
				{
					m_CurrentIndex = activeItem.index;
				}
				continue;
			}
			float expectedItemHeight2 = base.targetView.virtualizationController.GetExpectedItemHeight(activeItem.index);
			if (layout.y <= num + expectedItemHeight2 * 0.5f)
			{
				if (m_CurrentIndex == -1)
				{
					m_CurrentIndex = activeItem.index;
				}
				if (m_OffsetItem != activeItem)
				{
					Animate(m_OffsetItem, 0f);
					Animate(activeItem, m_SelectionHeight);
					m_OffsetItem = activeItem;
				}
				break;
			}
			num += expectedItemHeight2;
		}
		if (m_CurrentIndex == -1)
		{
			m_CurrentIndex = base.targetView.itemsSource.Count;
			Animate(m_OffsetItem, 0f);
			m_OffsetItem = null;
		}
		m_Item.rootElement.layout = layout;
		m_Item.rootElement.BringToFront();
	}

	private void Animate(ReusableCollectionItem element, float paddingTop)
	{
		if (element != null && (element.animator == null || ((!element.animator.isRunning || element.animator.to.paddingTop != paddingTop) && (element.animator.isRunning || !(element.rootElement.style.paddingTop == paddingTop)))))
		{
			element.animator?.Stop();
			element.animator?.Recycle();
			StyleValues to = ((base.targetView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight) ? new StyleValues
			{
				paddingTop = paddingTop,
				height = base.targetView.ResolveItemHeight() + paddingTop
			} : new StyleValues
			{
				paddingTop = paddingTop
			});
			element.animator = element.rootElement.experimental.animation.Start(to, 500);
			element.animator.KeepAlive();
		}
	}

	protected internal override void OnDrop(Vector3 pointerPosition)
	{
		if (m_Item == null)
		{
			return;
		}
		isDragging = false;
		m_Item.rootElement.ClearManualLayout();
		base.targetView.virtualizationController.EndDrag(m_CurrentIndex);
		if (m_OffsetItem != null)
		{
			m_OffsetItem.animator?.Stop();
			m_OffsetItem.animator?.Recycle();
			m_OffsetItem.animator = null;
			m_OffsetItem.rootElement.style.paddingTop = 0f;
			if (base.targetView.virtualizationMethod == CollectionVirtualizationMethod.FixedHeight)
			{
				m_OffsetItem.rootElement.style.height = base.targetView.ResolveItemHeight();
			}
		}
		DragPosition dragPosition = new DragPosition
		{
			recycledItem = m_Item,
			insertAtIndex = m_CurrentIndex,
			dropPosition = DragAndDropPosition.BetweenItems
		};
		DragAndDropArgs dragAndDropArgs = MakeDragAndDropArgs(dragPosition);
		base.dragAndDropController.OnDrop(dragAndDropArgs);
		base.dragAndDrop.AcceptDrag();
		m_Item = null;
		m_OffsetItem = null;
	}

	protected override void ClearDragAndDropUI(bool dragCancelled)
	{
	}

	protected override bool TryGetDragPosition(Vector2 pointerPosition, ref DragPosition dragPosition)
	{
		dragPosition.recycledItem = m_Item;
		dragPosition.insertAtIndex = m_CurrentIndex;
		dragPosition.dropPosition = DragAndDropPosition.BetweenItems;
		return true;
	}
}
