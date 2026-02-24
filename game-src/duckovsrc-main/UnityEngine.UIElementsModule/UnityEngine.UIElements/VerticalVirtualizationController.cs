using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal abstract class VerticalVirtualizationController<T> : CollectionVirtualizationController where T : ReusableCollectionItem, new()
{
	private readonly UnityEngine.Pool.ObjectPool<T> m_Pool = new UnityEngine.Pool.ObjectPool<T>(() => new T(), null, delegate(T i)
	{
		i.DetachElement();
	}, delegate(T i)
	{
		i.DestroyElement();
	});

	protected BaseVerticalCollectionView m_CollectionView;

	protected const int k_ExtraVisibleItems = 2;

	protected List<T> m_ActiveItems;

	protected T m_DraggedItem;

	private int m_LastFocusedElementIndex = -1;

	private List<int> m_LastFocusedElementTreeChildIndexes = new List<int>();

	protected readonly Func<T, bool> m_VisibleItemPredicateDelegate;

	protected List<T> m_ScrollInsertionList = new List<T>();

	private VisualElement m_EmptyRows;

	public override IEnumerable<ReusableCollectionItem> activeItems => m_ActiveItems;

	internal int itemsCount => m_CollectionView.itemsSource.Count;

	internal T firstVisibleItem => m_ActiveItems.FirstOrDefault(m_VisibleItemPredicateDelegate);

	internal T lastVisibleItem => m_ActiveItems.LastOrDefault(m_VisibleItemPredicateDelegate);

	public override int visibleItemCount => m_ActiveItems.Count(m_VisibleItemPredicateDelegate);

	protected SerializedVirtualizationData serializedData => m_CollectionView.serializedVirtualizationData;

	public override int firstVisibleIndex
	{
		get
		{
			return Mathf.Min(serializedData.firstVisibleIndex, (m_CollectionView.viewController != null) ? (m_CollectionView.viewController.GetItemsCount() - 1) : serializedData.firstVisibleIndex);
		}
		protected set
		{
			serializedData.firstVisibleIndex = value;
		}
	}

	protected float lastHeight => m_CollectionView.lastHeight;

	protected virtual bool alwaysRebindOnRefresh => true;

	protected virtual bool VisibleItemPredicate(T i)
	{
		return i.rootElement.style.display == DisplayStyle.Flex;
	}

	protected VerticalVirtualizationController(BaseVerticalCollectionView collectionView)
		: base(collectionView.scrollView)
	{
		m_CollectionView = collectionView;
		m_ActiveItems = new List<T>();
		m_VisibleItemPredicateDelegate = VisibleItemPredicate;
		m_ScrollView.contentContainer.disableClipping = false;
	}

	public override void Refresh(bool rebuild)
	{
		bool flag = m_CollectionView.HasValidDataAndBindings();
		for (int i = 0; i < m_ActiveItems.Count; i++)
		{
			int num = firstVisibleIndex + i;
			T val = m_ActiveItems[i];
			bool flag2 = val.rootElement.style.display == DisplayStyle.Flex;
			if (rebuild)
			{
				if (flag)
				{
					m_CollectionView.viewController.InvokeUnbindItem(val, val.index);
				}
				m_Pool.Release(val);
			}
			else if (m_CollectionView.itemsSource != null && num >= 0 && num < itemsCount)
			{
				if (flag && (flag2 || alwaysRebindOnRefresh))
				{
					if (val.index != -1)
					{
						m_CollectionView.viewController.InvokeUnbindItem(val, val.index);
					}
					Setup(val, num);
				}
			}
			else if (flag2)
			{
				ReleaseItem(i--);
			}
		}
		if (rebuild)
		{
			m_Pool.Clear();
			m_ActiveItems.Clear();
			m_ScrollView.Clear();
		}
	}

	protected void Setup(T recycledItem, int newIndex)
	{
		bool isDragGhost = recycledItem.isDragGhost;
		if (GetDraggedIndex() == newIndex)
		{
			if (recycledItem.index != -1)
			{
				m_CollectionView.viewController.InvokeUnbindItem(recycledItem, recycledItem.index);
			}
			recycledItem.SetDragGhost(dragGhost: true);
			recycledItem.index = m_DraggedItem.index;
			recycledItem.rootElement.style.display = DisplayStyle.Flex;
			return;
		}
		if (isDragGhost)
		{
			recycledItem.SetDragGhost(dragGhost: false);
		}
		if (newIndex >= itemsCount)
		{
			recycledItem.rootElement.style.display = DisplayStyle.None;
			if (recycledItem.index >= 0 && recycledItem.index < itemsCount)
			{
				m_CollectionView.viewController.InvokeUnbindItem(recycledItem, recycledItem.index);
				recycledItem.index = -1;
			}
			return;
		}
		recycledItem.rootElement.style.display = DisplayStyle.Flex;
		int idForIndex = m_CollectionView.viewController.GetIdForIndex(newIndex);
		if (recycledItem.index != newIndex || recycledItem.id != idForIndex)
		{
			bool enable = m_CollectionView.showAlternatingRowBackgrounds != AlternatingRowBackground.None && newIndex % 2 == 1;
			recycledItem.rootElement.EnableInClassList(BaseVerticalCollectionView.itemAlternativeBackgroundUssClassName, enable);
			int index = recycledItem.index;
			if (recycledItem.index != -1)
			{
				m_CollectionView.viewController.InvokeUnbindItem(recycledItem, recycledItem.index);
			}
			recycledItem.index = newIndex;
			recycledItem.id = idForIndex;
			int num = newIndex - firstVisibleIndex;
			if (num >= m_ScrollView.contentContainer.childCount)
			{
				recycledItem.rootElement.BringToFront();
			}
			else if (num >= 0)
			{
				recycledItem.rootElement.PlaceBehind(m_ScrollView.contentContainer[num]);
			}
			else
			{
				recycledItem.rootElement.SendToBack();
			}
			m_CollectionView.viewController.InvokeBindItem(recycledItem, newIndex);
			HandleFocus(recycledItem, index);
		}
	}

	public override void OnFocus(VisualElement leafTarget)
	{
		if (leafTarget == m_ScrollView.contentContainer)
		{
			return;
		}
		m_LastFocusedElementTreeChildIndexes.Clear();
		if (m_ScrollView.contentContainer.FindElementInTree(leafTarget, m_LastFocusedElementTreeChildIndexes))
		{
			VisualElement visualElement = m_ScrollView.contentContainer[m_LastFocusedElementTreeChildIndexes[0]];
			foreach (ReusableCollectionItem activeItem in activeItems)
			{
				if (activeItem.rootElement == visualElement)
				{
					m_LastFocusedElementIndex = activeItem.index;
					break;
				}
			}
			m_LastFocusedElementTreeChildIndexes.RemoveAt(0);
		}
		else
		{
			m_LastFocusedElementIndex = -1;
		}
	}

	public override void OnBlur(VisualElement willFocus)
	{
		if (willFocus == null || willFocus != m_ScrollView.contentContainer)
		{
			m_LastFocusedElementTreeChildIndexes.Clear();
			m_LastFocusedElementIndex = -1;
		}
	}

	private void HandleFocus(ReusableCollectionItem recycledItem, int previousIndex)
	{
		if (m_LastFocusedElementIndex != -1)
		{
			if (m_LastFocusedElementIndex == recycledItem.index)
			{
				recycledItem.rootElement.ElementAtTreePath(m_LastFocusedElementTreeChildIndexes)?.Focus();
			}
			else if (m_LastFocusedElementIndex != previousIndex)
			{
				recycledItem.rootElement.ElementAtTreePath(m_LastFocusedElementTreeChildIndexes)?.Blur();
			}
			else
			{
				m_ScrollView.contentContainer.Focus();
			}
		}
	}

	public override void UpdateBackground()
	{
		float num;
		if (m_CollectionView.showAlternatingRowBackgrounds != AlternatingRowBackground.All || (num = m_ScrollView.contentViewport.resolvedStyle.height - GetExpectedContentHeight()) <= 0f)
		{
			m_EmptyRows?.RemoveFromHierarchy();
		}
		else
		{
			if (lastVisibleItem == null)
			{
				return;
			}
			if (m_EmptyRows == null)
			{
				m_EmptyRows = new VisualElement
				{
					classList = { BaseVerticalCollectionView.backgroundFillUssClassName }
				};
			}
			if (m_EmptyRows.parent == null)
			{
				m_ScrollView.contentViewport.Add(m_EmptyRows);
			}
			float expectedItemHeight = GetExpectedItemHeight(-1);
			int num2 = Mathf.FloorToInt(num / expectedItemHeight) + 1;
			if (num2 > m_EmptyRows.childCount)
			{
				int num3 = num2 - m_EmptyRows.childCount;
				for (int i = 0; i < num3; i++)
				{
					VisualElement visualElement = new VisualElement();
					visualElement.style.flexShrink = 0f;
					m_EmptyRows.Add(visualElement);
				}
			}
			int num4 = lastVisibleItem?.index ?? (-1);
			int childCount = m_EmptyRows.hierarchy.childCount;
			for (int j = 0; j < childCount; j++)
			{
				VisualElement visualElement2 = m_EmptyRows.hierarchy[j];
				num4++;
				visualElement2.style.height = expectedItemHeight;
				visualElement2.EnableInClassList(BaseVerticalCollectionView.itemAlternativeBackgroundUssClassName, num4 % 2 == 1);
			}
		}
	}

	internal override void StartDragItem(ReusableCollectionItem item)
	{
		m_DraggedItem = item as T;
		int num = m_ActiveItems.IndexOf(m_DraggedItem);
		m_ActiveItems.RemoveAt(num);
		T orMakeItemAtIndex = GetOrMakeItemAtIndex(num, num);
		Setup(orMakeItemAtIndex, m_DraggedItem.index);
	}

	internal override void EndDrag(int dropIndex)
	{
		ReusableCollectionItem recycledItemFromIndex = m_CollectionView.GetRecycledItemFromIndex(dropIndex);
		int index = ((recycledItemFromIndex != null) ? m_ScrollView.IndexOf(recycledItemFromIndex.rootElement) : m_ActiveItems.Count);
		m_ScrollView.Insert(index, m_DraggedItem.rootElement);
		m_ActiveItems.Insert(index, m_DraggedItem);
		for (int i = 0; i < m_ActiveItems.Count; i++)
		{
			T val = m_ActiveItems[i];
			if (val.isDragGhost)
			{
				val.index = -1;
				ReleaseItem(i);
				i--;
			}
		}
		if (Math.Min(dropIndex, itemsCount - 1) != m_DraggedItem.index)
		{
			if (lastVisibleItem != null)
			{
				lastVisibleItem.rootElement.style.display = DisplayStyle.None;
			}
			if (m_DraggedItem.index < dropIndex)
			{
				m_CollectionView.viewController.InvokeUnbindItem(m_DraggedItem, m_DraggedItem.index);
				m_DraggedItem.index = -1;
			}
			else if (recycledItemFromIndex != null)
			{
				m_CollectionView.viewController.InvokeUnbindItem(recycledItemFromIndex, recycledItemFromIndex.index);
				recycledItemFromIndex.index = -1;
			}
		}
		m_DraggedItem = null;
	}

	internal virtual T GetOrMakeItemAtIndex(int activeItemIndex = -1, int scrollViewIndex = -1)
	{
		T val = m_Pool.Get();
		if (val.rootElement == null)
		{
			m_CollectionView.viewController.InvokeMakeItem(val);
			val.onDestroy += OnDestroyItem;
		}
		val.PreAttachElement();
		if (activeItemIndex == -1)
		{
			m_ActiveItems.Add(val);
		}
		else
		{
			m_ActiveItems.Insert(activeItemIndex, val);
		}
		if (scrollViewIndex == -1)
		{
			m_ScrollView.Add(val.rootElement);
		}
		else
		{
			m_ScrollView.Insert(scrollViewIndex, val.rootElement);
		}
		return val;
	}

	internal virtual void ReleaseItem(int activeItemsIndex)
	{
		T val = m_ActiveItems[activeItemsIndex];
		if (val.index != -1)
		{
			m_CollectionView.viewController.InvokeUnbindItem(val, val.index);
		}
		m_Pool.Release(val);
		m_ActiveItems.Remove(val);
	}

	private void OnDestroyItem(ReusableCollectionItem item)
	{
		m_CollectionView.viewController.InvokeDestroyItem(item);
		item.onDestroy -= OnDestroyItem;
	}

	protected int GetDraggedIndex()
	{
		if (m_CollectionView.dragger is ListViewDraggerAnimated { isDragging: not false } listViewDraggerAnimated)
		{
			return listViewDraggerAnimated.draggedItem.index;
		}
		return -1;
	}
}
