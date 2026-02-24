using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class FixedHeightVirtualizationController<T> : VerticalVirtualizationController<T> where T : ReusableCollectionItem, new()
{
	private float resolvedItemHeight => m_CollectionView.ResolveItemHeight();

	protected override bool VisibleItemPredicate(T i)
	{
		return true;
	}

	public FixedHeightVirtualizationController(BaseVerticalCollectionView collectionView)
		: base(collectionView)
	{
	}

	public override int GetIndexFromPosition(Vector2 position)
	{
		return (int)(position.y / resolvedItemHeight);
	}

	public override float GetExpectedItemHeight(int index)
	{
		return resolvedItemHeight;
	}

	public override float GetExpectedContentHeight()
	{
		return (float)base.itemsCount * resolvedItemHeight;
	}

	public override void ScrollToItem(int index)
	{
		if (visibleItemCount == 0 || index < -1)
		{
			return;
		}
		float num = resolvedItemHeight;
		if (index == -1)
		{
			int num2 = (int)(base.lastHeight / num);
			if (base.itemsCount < num2)
			{
				m_ScrollView.scrollOffset = new Vector2(0f, 0f);
			}
			else
			{
				m_ScrollView.scrollOffset = new Vector2(0f, (float)(base.itemsCount + 1) * num);
			}
			return;
		}
		if (firstVisibleIndex >= index)
		{
			m_ScrollView.scrollOffset = Vector2.up * (num * (float)index);
			return;
		}
		int num3 = (int)(base.lastHeight / num);
		if (index >= firstVisibleIndex + num3)
		{
			int num4 = index - num3 + 1;
			float num5 = num - (base.lastHeight - (float)num3 * num);
			float y = num * (float)num4 + num5;
			m_ScrollView.scrollOffset = new Vector2(m_ScrollView.scrollOffset.x, y);
		}
	}

	public override void Resize(Vector2 size)
	{
		float expectedContentHeight = GetExpectedContentHeight();
		m_ScrollView.contentContainer.style.height = expectedContentHeight;
		float num = Mathf.Max(0f, expectedContentHeight - m_ScrollView.contentViewport.layout.height);
		float num2 = Mathf.Min(base.serializedData.scrollOffset.y, num);
		m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num);
		m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(num2);
		int a = 0;
		float num3 = size.y / resolvedItemHeight;
		if (num3 > 0f)
		{
			a = (int)num3 + 2;
		}
		int num4 = Mathf.Min(a, base.itemsCount);
		if (visibleItemCount != num4)
		{
			int num5 = visibleItemCount;
			if (visibleItemCount > num4)
			{
				int num6 = num5 - num4;
				for (int i = 0; i < num6; i++)
				{
					int activeItemsIndex = m_ActiveItems.Count - 1;
					ReleaseItem(activeItemsIndex);
				}
			}
			else
			{
				int num7 = num4 - visibleItemCount;
				for (int j = 0; j < num7; j++)
				{
					int newIndex = j + firstVisibleIndex + num5;
					T orMakeItemAtIndex = GetOrMakeItemAtIndex();
					Setup(orMakeItemAtIndex, newIndex);
				}
			}
		}
		OnScroll(new Vector2(0f, num2));
	}

	public override void OnScroll(Vector2 scrollOffset)
	{
		float num = Mathf.Max(0f, scrollOffset.y);
		float num2 = resolvedItemHeight;
		int num3 = (int)(num / num2);
		m_ScrollView.contentContainer.style.paddingTop = (float)num3 * num2;
		m_ScrollView.contentContainer.style.height = (float)base.itemsCount * num2;
		base.serializedData.scrollOffset.y = scrollOffset.y;
		if (num3 == firstVisibleIndex)
		{
			return;
		}
		firstVisibleIndex = num3;
		if (m_ActiveItems.Count <= 0)
		{
			return;
		}
		if (firstVisibleIndex < m_ActiveItems[0].index)
		{
			int num4 = m_ActiveItems[0].index - firstVisibleIndex;
			List<T> scrollInsertionList = m_ScrollInsertionList;
			for (int i = 0; i < num4; i++)
			{
				if (m_ActiveItems.Count <= 0)
				{
					break;
				}
				List<T> list = m_ActiveItems;
				T val = list[list.Count - 1];
				scrollInsertionList.Add(val);
				m_ActiveItems.RemoveAt(m_ActiveItems.Count - 1);
				val.rootElement.SendToBack();
			}
			m_ActiveItems.InsertRange(0, scrollInsertionList);
			m_ScrollInsertionList.Clear();
		}
		else
		{
			int num5 = firstVisibleIndex;
			List<T> list2 = m_ActiveItems;
			if (num5 < list2[list2.Count - 1].index)
			{
				List<T> scrollInsertionList2 = m_ScrollInsertionList;
				int num6 = 0;
				while (firstVisibleIndex > m_ActiveItems[num6].index)
				{
					T val2 = m_ActiveItems[num6];
					scrollInsertionList2.Add(val2);
					num6++;
					val2.rootElement.BringToFront();
				}
				m_ActiveItems.RemoveRange(0, num6);
				m_ActiveItems.AddRange(scrollInsertionList2);
				scrollInsertionList2.Clear();
			}
		}
		for (int j = 0; j < m_ActiveItems.Count; j++)
		{
			int newIndex = j + firstVisibleIndex;
			Setup(m_ActiveItems[j], newIndex);
		}
	}

	internal override T GetOrMakeItemAtIndex(int activeItemIndex = -1, int scrollViewIndex = -1)
	{
		T orMakeItemAtIndex = base.GetOrMakeItemAtIndex(activeItemIndex, scrollViewIndex);
		orMakeItemAtIndex.rootElement.style.height = resolvedItemHeight;
		return orMakeItemAtIndex;
	}

	internal override void EndDrag(int dropIndex)
	{
		m_DraggedItem.rootElement.style.height = resolvedItemHeight;
		if (firstVisibleIndex > m_DraggedItem.index)
		{
			m_ScrollView.verticalScroller.value = base.serializedData.scrollOffset.y - resolvedItemHeight;
		}
		base.EndDrag(dropIndex);
	}
}
