using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.UIElements;

internal class DynamicHeightVirtualizationController<T> : VerticalVirtualizationController<T> where T : ReusableCollectionItem, new()
{
	private readonly struct ContentHeightCacheInfo
	{
		public readonly float sum;

		public readonly int count;

		public ContentHeightCacheInfo(float sum, int count)
		{
			this.sum = sum;
			this.count = count;
		}
	}

	private enum VirtualizationChange
	{
		None,
		Resize,
		Scroll,
		ForcedScroll
	}

	private enum ScrollDirection
	{
		Idle,
		Up,
		Down
	}

	private int m_HighestCachedIndex = -1;

	private readonly Dictionary<int, float> m_ItemHeightCache = new Dictionary<int, float>(32);

	private readonly Dictionary<int, ContentHeightCacheInfo> m_ContentHeightCache = new Dictionary<int, ContentHeightCacheInfo>(32);

	private readonly HashSet<int> m_WaitingCache = new HashSet<int>(32);

	private int m_ForcedFirstVisibleItem = -1;

	private int m_ForcedLastVisibleItem = -1;

	private bool m_StickToBottom;

	private VirtualizationChange m_LastChange;

	private ScrollDirection m_ScrollDirection;

	private Vector2 m_DelayedScrollOffset = Vector2.negativeInfinity;

	private float m_AccumulatedHeight;

	private float m_MinimumItemHeight = -1f;

	private Action m_FillCallback;

	private Action m_ScrollCallback;

	private Action m_ScrollResetCallback;

	private Action<ReusableCollectionItem> m_GeometryChangedCallback;

	private IVisualElementScheduledItem m_ScheduledItem;

	private IVisualElementScheduledItem m_ScrollScheduledItem;

	private IVisualElementScheduledItem m_ScrollResetScheduledItem;

	private Predicate<int> m_IndexOutOfBoundsPredicate;

	internal IReadOnlyDictionary<int, float> itemHeightCache => m_ItemHeightCache;

	private float defaultExpectedHeight
	{
		get
		{
			if (m_MinimumItemHeight > 0f)
			{
				return m_MinimumItemHeight;
			}
			if (m_CollectionView.m_ItemHeightIsInline && m_CollectionView.fixedItemHeight > 0f)
			{
				return m_CollectionView.fixedItemHeight;
			}
			return BaseVerticalCollectionView.s_DefaultItemHeight;
		}
	}

	private float contentPadding
	{
		get
		{
			return base.serializedData.contentPadding;
		}
		set
		{
			m_CollectionView.scrollView.contentContainer.style.paddingTop = value;
			base.serializedData.contentPadding = value;
			m_CollectionView.SaveViewData();
		}
	}

	private float contentHeight
	{
		get
		{
			return base.serializedData.contentHeight;
		}
		set
		{
			m_CollectionView.scrollView.contentContainer.style.height = value;
			base.serializedData.contentHeight = value;
			m_CollectionView.SaveViewData();
		}
	}

	private int anchoredIndex
	{
		get
		{
			return base.serializedData.anchoredItemIndex;
		}
		set
		{
			base.serializedData.anchoredItemIndex = value;
			m_CollectionView.SaveViewData();
		}
	}

	private float anchorOffset
	{
		get
		{
			return base.serializedData.anchorOffset;
		}
		set
		{
			base.serializedData.anchorOffset = value;
			m_CollectionView.SaveViewData();
		}
	}

	private float viewportMaxOffset => base.serializedData.scrollOffset.y + m_ScrollView.contentViewport.layout.height;

	protected override bool alwaysRebindOnRefresh => false;

	public DynamicHeightVirtualizationController(BaseVerticalCollectionView collectionView)
		: base(collectionView)
	{
		m_FillCallback = Fill;
		m_ScrollCallback = OnScrollUpdate;
		m_GeometryChangedCallback = OnRecycledItemGeometryChanged;
		m_IndexOutOfBoundsPredicate = IsIndexOutOfBounds;
		m_ScrollResetCallback = ResetScroll;
	}

	public override void Refresh(bool rebuild)
	{
		CleanItemHeightCache();
		int count = m_ActiveItems.Count;
		bool flag = false;
		if (rebuild)
		{
			m_WaitingCache.Clear();
		}
		else
		{
			flag |= m_WaitingCache.RemoveWhere(m_IndexOutOfBoundsPredicate) > 0;
		}
		base.Refresh(rebuild);
		m_ScrollDirection = ScrollDirection.Idle;
		m_LastChange = VirtualizationChange.None;
		if (m_CollectionView.HasValidDataAndBindings())
		{
			if (flag || count != m_ActiveItems.Count)
			{
				contentHeight = GetExpectedContentHeight();
				float highValueWithoutNotify = Mathf.Max(0f, contentHeight - m_ScrollView.contentViewport.layout.height);
				m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(highValueWithoutNotify);
				m_ScrollView.verticalScroller.value = base.serializedData.scrollOffset.y;
				base.serializedData.scrollOffset.y = m_ScrollView.verticalScroller.value;
			}
			ScheduleFill();
		}
	}

	public override void ScrollToItem(int index)
	{
		if (index < -1)
		{
			return;
		}
		float height = m_ScrollView.contentContainer.layout.height;
		float height2 = m_ScrollView.contentViewport.layout.height;
		if (index == -1)
		{
			m_ForcedLastVisibleItem = base.itemsCount - 1;
			m_ForcedFirstVisibleItem = -1;
			m_StickToBottom = true;
			m_ScrollView.scrollOffset = new Vector2(0f, (height2 >= height) ? 0f : height);
			return;
		}
		if (firstVisibleIndex >= index)
		{
			Vector2 vector = new Vector2(0f, GetContentHeightForIndex(index - 1));
			if (!(vector == m_ScrollView.scrollOffset))
			{
				m_ForcedFirstVisibleItem = index;
				m_ForcedLastVisibleItem = -1;
				m_ScrollView.scrollOffset = vector;
			}
			return;
		}
		float contentHeightForIndex = GetContentHeightForIndex(index);
		if (!(contentHeightForIndex < contentPadding + height2))
		{
			float y = contentHeightForIndex - height2 + (float)BaseVerticalCollectionView.s_DefaultItemHeight;
			m_ForcedLastVisibleItem = index;
			m_ForcedFirstVisibleItem = -1;
			m_ScrollView.scrollOffset = new Vector2(0f, y);
		}
	}

	public override void Resize(Vector2 size)
	{
		float expectedContentHeight = GetExpectedContentHeight();
		contentHeight = Mathf.Max(expectedContentHeight, contentHeight);
		float height = m_ScrollView.contentViewport.layout.height;
		float num = Mathf.Max(0f, contentHeight - height);
		float valueWithoutNotify = Mathf.Min(base.serializedData.scrollOffset.y, num);
		m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num);
		m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(valueWithoutNotify);
		base.serializedData.scrollOffset.y = m_ScrollView.verticalScroller.value;
		float num2 = m_CollectionView.ResolveItemHeight(size.y);
		int num3 = Mathf.CeilToInt(num2 / defaultExpectedHeight);
		int num4 = num3;
		if (num4 <= 0)
		{
			return;
		}
		num4 += 2;
		int num5 = Mathf.Min(num4, base.itemsCount);
		if (m_ActiveItems.Count != num5)
		{
			int count = m_ActiveItems.Count;
			if (count > num5)
			{
				int num6 = count - num5;
				for (int i = 0; i < num6; i++)
				{
					int activeItemsIndex = m_ActiveItems.Count - 1;
					ReleaseItem(activeItemsIndex);
				}
			}
			else
			{
				int num7 = num5 - m_ActiveItems.Count;
				int num8 = ((firstVisibleIndex >= 0) ? firstVisibleIndex : 0);
				for (int j = 0; j < num7; j++)
				{
					int num9 = j + num8 + count;
					T orMakeItemAtIndex = GetOrMakeItemAtIndex();
					if (IsIndexOutOfBounds(num9))
					{
						HideItem(m_ActiveItems.Count - 1);
						continue;
					}
					Setup(orMakeItemAtIndex, num9);
					MarkWaitingForLayout(orMakeItemAtIndex);
				}
			}
		}
		ScheduleFill();
		ScheduleScrollDirectionReset();
		m_LastChange = VirtualizationChange.Resize;
	}

	public override void OnScroll(Vector2 scrollOffset)
	{
		if (m_DelayedScrollOffset == scrollOffset)
		{
			return;
		}
		m_DelayedScrollOffset = scrollOffset;
		if (m_ForcedFirstVisibleItem != -1 || m_ForcedLastVisibleItem != -1)
		{
			OnScrollUpdate();
			m_LastChange = VirtualizationChange.ForcedScroll;
			return;
		}
		if (m_ScheduledItem == null)
		{
			VirtualizationChange lastChange = m_LastChange;
			if (lastChange == VirtualizationChange.Resize || lastChange == VirtualizationChange.ForcedScroll)
			{
				m_ScheduledItem = m_CollectionView.schedule.Execute(m_FillCallback);
				float height = m_ScrollView.contentViewport.layout.height;
				float num = Mathf.Max(0f, contentHeight - height);
				float valueWithoutNotify = Mathf.Min(base.serializedData.scrollOffset.y, num);
				m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num);
				m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(valueWithoutNotify);
				base.serializedData.scrollOffset.y = m_ScrollView.verticalScroller.value;
				return;
			}
		}
		ScheduleScroll();
	}

	private void OnScrollUpdate()
	{
		Vector2 scrollOffset = (float.IsNegativeInfinity(m_DelayedScrollOffset.y) ? base.serializedData.scrollOffset : m_DelayedScrollOffset);
		if (!float.IsNaN(m_ScrollView.contentViewport.layout.height) && !float.IsNaN(scrollOffset.y))
		{
			m_LastChange = VirtualizationChange.Scroll;
			float expectedContentHeight = GetExpectedContentHeight();
			contentHeight = Mathf.Max(expectedContentHeight, contentHeight);
			m_ScrollDirection = ((scrollOffset.y < base.serializedData.scrollOffset.y) ? ScrollDirection.Up : ScrollDirection.Down);
			float num = Mathf.Max(0f, contentHeight - m_ScrollView.contentViewport.layout.height);
			if (scrollOffset.y <= 0f)
			{
				m_ForcedFirstVisibleItem = 0;
			}
			m_StickToBottom = num > 0f && Math.Abs(scrollOffset.y - m_ScrollView.verticalScroller.highValue) < float.Epsilon;
			base.serializedData.scrollOffset = scrollOffset;
			m_CollectionView.SaveViewData();
			int num2 = ((m_ForcedFirstVisibleItem != -1) ? m_ForcedFirstVisibleItem : GetFirstVisibleItem(base.serializedData.scrollOffset.y));
			float contentHeightForIndex = GetContentHeightForIndex(num2 - 1);
			contentPadding = contentHeightForIndex;
			m_ForcedFirstVisibleItem = -1;
			if (num2 != firstVisibleIndex)
			{
				CycleItems(num2);
			}
			else
			{
				Fill();
			}
			ScheduleScrollDirectionReset();
			m_DelayedScrollOffset = Vector2.negativeInfinity;
		}
	}

	private void CycleItems(int firstIndex)
	{
		if (firstIndex == firstVisibleIndex)
		{
			return;
		}
		T val = base.firstVisibleItem;
		contentPadding = GetContentHeightForIndex(firstIndex - 1);
		firstVisibleIndex = firstIndex;
		if (m_ActiveItems.Count > 0)
		{
			if (val != null && m_ActiveItems.Count > Mathf.Abs(firstVisibleIndex - val.index))
			{
				if (firstVisibleIndex < val.index)
				{
					int num = val.index - firstVisibleIndex;
					List<T> scrollInsertionList = m_ScrollInsertionList;
					for (int i = 0; i < num; i++)
					{
						List<T> list = m_ActiveItems;
						T val2 = list[list.Count - 1];
						scrollInsertionList.Insert(0, val2);
						m_ActiveItems.RemoveAt(m_ActiveItems.Count - 1);
						val2.rootElement.SendToBack();
					}
					m_ActiveItems.InsertRange(0, scrollInsertionList);
					m_ScrollInsertionList.Clear();
				}
				else
				{
					List<T> scrollInsertionList2 = m_ScrollInsertionList;
					int num2 = 0;
					while (firstVisibleIndex > m_ActiveItems[num2].index)
					{
						T val3 = m_ActiveItems[num2];
						scrollInsertionList2.Add(val3);
						num2++;
						val3.rootElement.BringToFront();
					}
					m_ActiveItems.RemoveRange(0, num2);
					m_ActiveItems.AddRange(scrollInsertionList2);
					m_ScrollInsertionList.Clear();
				}
			}
			float num3 = contentPadding;
			for (int j = 0; j < m_ActiveItems.Count; j++)
			{
				T val4 = m_ActiveItems[j];
				int num4 = firstVisibleIndex + j;
				int index = val4.index;
				bool flag = val4.rootElement.style.display == DisplayStyle.Flex;
				m_WaitingCache.Remove(index);
				if (IsIndexOutOfBounds(num4))
				{
					HideItem(j);
					continue;
				}
				Setup(val4, num4);
				if (num3 > viewportMaxOffset)
				{
					HideItem(j);
				}
				else if (num4 != index || !flag)
				{
					MarkWaitingForLayout(val4);
				}
				num3 += GetExpectedItemHeight(num4);
			}
		}
		if (m_LastChange != VirtualizationChange.Resize)
		{
			UpdateAnchor();
		}
		ScheduleFill();
	}

	private bool NeedsFill()
	{
		if (m_LastChange != VirtualizationChange.None || anchoredIndex < 0)
		{
			return false;
		}
		int num = base.lastVisibleItem?.index ?? (-1);
		float num2 = contentPadding;
		if (num2 > base.serializedData.scrollOffset.y)
		{
			return true;
		}
		for (int i = firstVisibleIndex; i < base.itemsCount; i++)
		{
			if (num2 > viewportMaxOffset)
			{
				break;
			}
			if (num2 == viewportMaxOffset && !m_StickToBottom)
			{
				break;
			}
			num2 += GetExpectedItemHeight(i);
			if (i > num)
			{
				return true;
			}
		}
		return false;
	}

	private void Fill()
	{
		if (!m_CollectionView.HasValidDataAndBindings())
		{
			return;
		}
		if (m_ActiveItems.Count == 0)
		{
			contentHeight = 0f;
			contentPadding = 0f;
		}
		else
		{
			if (anchoredIndex < 0)
			{
				return;
			}
			if (contentPadding > contentHeight)
			{
				OnScrollUpdate();
				return;
			}
			float num = contentPadding;
			float num2 = contentPadding;
			int num3 = 0;
			for (int i = firstVisibleIndex; i < base.itemsCount; i++)
			{
				if (num2 > viewportMaxOffset)
				{
					break;
				}
				if (num2 == viewportMaxOffset && !m_StickToBottom)
				{
					break;
				}
				num2 += GetExpectedItemHeight(i);
				T val = m_ActiveItems[num3++];
				if (val.index != i || val.rootElement.style.display == DisplayStyle.None)
				{
					Setup(val, i);
					MarkWaitingForLayout(val);
				}
				if (num3 >= m_ActiveItems.Count)
				{
					break;
				}
			}
			if (firstVisibleIndex > 0 && contentPadding > base.serializedData.scrollOffset.y)
			{
				List<T> scrollInsertionList = m_ScrollInsertionList;
				int num4 = m_ActiveItems.Count - 1;
				while (num4 >= num3 && firstVisibleIndex != 0)
				{
					T val2 = m_ActiveItems[num4];
					scrollInsertionList.Insert(0, val2);
					m_ActiveItems.RemoveAt(m_ActiveItems.Count - 1);
					val2.rootElement.SendToBack();
					int num5 = --firstVisibleIndex;
					Setup(val2, num5);
					MarkWaitingForLayout(val2);
					num -= GetExpectedItemHeight(num5);
					if (num < base.serializedData.scrollOffset.y)
					{
						break;
					}
					num4--;
				}
				m_ActiveItems.InsertRange(0, scrollInsertionList);
				m_ScrollInsertionList.Clear();
			}
			contentPadding = num;
			contentHeight = GetExpectedContentHeight();
			if (m_LastChange != VirtualizationChange.Resize)
			{
				UpdateAnchor();
			}
			if (m_WaitingCache.Count == 0)
			{
				ResetScroll();
				ApplyScrollViewUpdate(dimensionsOnly: true);
			}
		}
	}

	private void UpdateScrollViewContainer(float previousHeight, float newHeight)
	{
		if (!m_StickToBottom)
		{
			if (m_ForcedLastVisibleItem >= 0)
			{
				float contentHeightForIndex = GetContentHeightForIndex(m_ForcedLastVisibleItem);
				base.serializedData.scrollOffset.y = contentHeightForIndex + (float)BaseVerticalCollectionView.s_DefaultItemHeight - m_ScrollView.contentViewport.layout.height;
			}
			else if (m_ScrollDirection == ScrollDirection.Up)
			{
				base.serializedData.scrollOffset.y += newHeight - previousHeight;
			}
		}
	}

	private void ApplyScrollViewUpdate(bool dimensionsOnly = false)
	{
		float num = contentPadding;
		float y = base.serializedData.scrollOffset.y;
		float num2 = y - num;
		if (anchoredIndex >= 0)
		{
			if (firstVisibleIndex != anchoredIndex)
			{
				CycleItems(anchoredIndex);
				ScheduleFill();
			}
			firstVisibleIndex = anchoredIndex;
			num2 = anchorOffset;
		}
		float num3 = (contentHeight = GetExpectedContentHeight());
		contentPadding = GetContentHeightForIndex(firstVisibleIndex - 1);
		float num4 = Mathf.Max(0f, m_ScrollView.RoundToPanelPixelSize(num3 - m_ScrollView.contentViewport.layout.height));
		float valueWithoutNotify = Mathf.Min(contentPadding + num2, num4);
		if (m_StickToBottom && num4 > 0f)
		{
			valueWithoutNotify = num4;
		}
		else if (m_ForcedLastVisibleItem != -1)
		{
			float contentHeightForIndex = GetContentHeightForIndex(m_ForcedLastVisibleItem);
			float value = contentHeightForIndex + (float)BaseVerticalCollectionView.s_DefaultItemHeight - m_ScrollView.contentViewport.layout.height;
			valueWithoutNotify = Mathf.Clamp(value, 0f, num4);
		}
		m_ScrollView.verticalScroller.slider.SetHighValueWithoutNotify(num4);
		m_ScrollView.verticalScroller.slider.SetValueWithoutNotify(valueWithoutNotify);
		base.serializedData.scrollOffset.y = m_ScrollView.verticalScroller.slider.value;
		m_ForcedLastVisibleItem = -1;
		if (dimensionsOnly || m_LastChange == VirtualizationChange.Resize)
		{
			ScheduleScrollDirectionReset();
			return;
		}
		if (NeedsFill())
		{
			Fill();
			return;
		}
		float num5 = contentPadding;
		int num6 = firstVisibleIndex;
		List<T> scrollInsertionList = m_ScrollInsertionList;
		int num7 = 0;
		for (int i = 0; i < m_ActiveItems.Count; i++)
		{
			T val = m_ActiveItems[i];
			int index = val.index;
			if (index < 0)
			{
				break;
			}
			float expectedItemHeight = GetExpectedItemHeight(index);
			if (m_ActiveItems[i].rootElement.style.display == DisplayStyle.Flex)
			{
				if (num5 + expectedItemHeight < base.serializedData.scrollOffset.y)
				{
					val.rootElement.BringToFront();
					HideItem(i);
					scrollInsertionList.Add(val);
					num7++;
					firstVisibleIndex++;
				}
				else if (num5 > viewportMaxOffset)
				{
					HideItem(i);
				}
			}
			num5 += GetExpectedItemHeight(index);
		}
		m_ActiveItems.RemoveRange(0, num7);
		m_ActiveItems.AddRange(scrollInsertionList);
		m_ScrollInsertionList.Clear();
		if (firstVisibleIndex != num6)
		{
			contentPadding = GetContentHeightForIndex(firstVisibleIndex - 1);
			UpdateAnchor();
		}
		ScheduleScrollDirectionReset();
		m_CollectionView.SaveViewData();
	}

	private void UpdateAnchor()
	{
		anchoredIndex = firstVisibleIndex;
		anchorOffset = base.serializedData.scrollOffset.y - contentPadding;
	}

	private void ScheduleFill()
	{
		if (m_ScheduledItem == null)
		{
			m_ScheduledItem = m_CollectionView.schedule.Execute(m_FillCallback);
			return;
		}
		m_ScheduledItem.Pause();
		m_ScheduledItem.Resume();
	}

	private void ScheduleScroll()
	{
		if (m_ScrollScheduledItem == null)
		{
			m_ScrollScheduledItem = m_CollectionView.schedule.Execute(m_ScrollCallback);
			return;
		}
		m_ScrollScheduledItem.Pause();
		m_ScrollScheduledItem.Resume();
	}

	private void ScheduleScrollDirectionReset()
	{
		if (m_ScrollResetScheduledItem == null)
		{
			m_ScrollResetScheduledItem = m_CollectionView.schedule.Execute(m_ScrollResetCallback);
			return;
		}
		m_ScrollResetScheduledItem.Pause();
		m_ScrollResetScheduledItem.Resume();
	}

	private void ResetScroll()
	{
		m_ScrollDirection = ScrollDirection.Idle;
		m_LastChange = VirtualizationChange.None;
		m_ScrollView.UpdateContentViewTransform();
		UpdateAnchor();
		m_CollectionView.SaveViewData();
	}

	public override int GetIndexFromPosition(Vector2 position)
	{
		int num = 0;
		for (float num2 = 0f; num2 < position.y; num2 += GetExpectedItemHeight(num++))
		{
		}
		return num - 1;
	}

	public override float GetExpectedItemHeight(int index)
	{
		int draggedIndex = GetDraggedIndex();
		if (draggedIndex >= 0 && index == draggedIndex)
		{
			return 0f;
		}
		float value;
		return m_ItemHeightCache.TryGetValue(index, out value) ? value : defaultExpectedHeight;
	}

	private int GetFirstVisibleItem(float offset)
	{
		if (offset <= 0f)
		{
			return 0;
		}
		int num = -1;
		while (offset > 0f)
		{
			num++;
			float expectedItemHeight = GetExpectedItemHeight(num);
			offset -= expectedItemHeight;
		}
		return num;
	}

	public override float GetExpectedContentHeight()
	{
		return m_AccumulatedHeight + (float)(base.itemsCount - m_ItemHeightCache.Count) * defaultExpectedHeight;
	}

	private float GetContentHeightForIndex(int lastIndex)
	{
		if (lastIndex < 0)
		{
			return 0f;
		}
		if (m_ContentHeightCache.TryGetValue(lastIndex, out var value))
		{
			int draggedIndex = GetDraggedIndex();
			if (draggedIndex >= 0 && lastIndex >= draggedIndex)
			{
				return value.sum + (float)(lastIndex - value.count + 1) * defaultExpectedHeight - m_DraggedItem.rootElement.layout.height;
			}
			return value.sum + (float)(lastIndex - value.count + 1) * defaultExpectedHeight;
		}
		return GetContentHeightForIndex(lastIndex - 1) + GetExpectedItemHeight(lastIndex);
	}

	private ContentHeightCacheInfo GetCachedContentHeight(int index)
	{
		while (index >= 0)
		{
			if (m_ContentHeightCache.TryGetValue(index, out var value))
			{
				return value;
			}
			index--;
		}
		return default(ContentHeightCacheInfo);
	}

	private void RegisterItemHeight(int index, float height)
	{
		if (height <= 0f)
		{
			return;
		}
		float num = m_CollectionView.ResolveItemHeight(height);
		if (m_ItemHeightCache.TryGetValue(index, out var value))
		{
			m_AccumulatedHeight -= value;
		}
		m_AccumulatedHeight += num;
		m_ItemHeightCache[index] = num;
		if (index > m_HighestCachedIndex)
		{
			m_HighestCachedIndex = index;
		}
		bool flag = value == 0f;
		ContentHeightCacheInfo cachedContentHeight = GetCachedContentHeight(index - 1);
		m_ContentHeightCache[index] = new ContentHeightCacheInfo(cachedContentHeight.sum + num, cachedContentHeight.count + 1);
		foreach (KeyValuePair<int, float> item in m_ItemHeightCache)
		{
			if (item.Key > index)
			{
				ContentHeightCacheInfo contentHeightCacheInfo = m_ContentHeightCache[item.Key];
				m_ContentHeightCache[item.Key] = new ContentHeightCacheInfo(contentHeightCacheInfo.sum - value + num, flag ? (contentHeightCacheInfo.count + 1) : contentHeightCacheInfo.count);
			}
		}
	}

	private void UnregisterItemHeight(int index)
	{
		if (!m_ItemHeightCache.TryGetValue(index, out var value))
		{
			return;
		}
		m_AccumulatedHeight -= value;
		m_ItemHeightCache.Remove(index);
		m_ContentHeightCache.Remove(index);
		int num = -1;
		foreach (KeyValuePair<int, float> item in m_ItemHeightCache)
		{
			if (item.Key > index)
			{
				ContentHeightCacheInfo contentHeightCacheInfo = m_ContentHeightCache[item.Key];
				m_ContentHeightCache[item.Key] = new ContentHeightCacheInfo(contentHeightCacheInfo.sum - value, contentHeightCacheInfo.count - 1);
			}
			if (item.Key > num)
			{
				num = item.Key;
			}
		}
		m_HighestCachedIndex = num;
	}

	private void CleanItemHeightCache()
	{
		if (!IsIndexOutOfBounds(m_HighestCachedIndex))
		{
			return;
		}
		List<int> list = CollectionPool<List<int>, int>.Get();
		try
		{
			foreach (int key in m_ItemHeightCache.Keys)
			{
				if (IsIndexOutOfBounds(key))
				{
					list.Add(key);
				}
			}
			foreach (int item in list)
			{
				UnregisterItemHeight(item);
			}
		}
		finally
		{
			CollectionPool<List<int>, int>.Release(list);
		}
		m_MinimumItemHeight = -1f;
	}

	private void OnRecycledItemGeometryChanged(ReusableCollectionItem item)
	{
		if (item.index != -1 && !item.isDragGhost && !float.IsNaN(item.rootElement.layout.height) && item.rootElement.layout.height != 0f && UpdateRegisteredHeight(item))
		{
			ApplyScrollViewUpdate();
		}
	}

	private bool UpdateRegisteredHeight(ReusableCollectionItem item)
	{
		if (item.index == -1 || item.isDragGhost || float.IsNaN(item.rootElement.layout.height) || item.rootElement.layout.height == 0f)
		{
			return false;
		}
		if (item.rootElement.layout.height < defaultExpectedHeight)
		{
			m_MinimumItemHeight = item.rootElement.layout.height;
			Resize(m_ScrollView.layout.size);
		}
		float num = item.rootElement.layout.height - item.rootElement.resolvedStyle.paddingTop;
		float value;
		bool flag = m_ItemHeightCache.TryGetValue(item.index, out value);
		float num2 = (flag ? GetExpectedItemHeight(item.index) : defaultExpectedHeight);
		if (m_WaitingCache.Count == 0)
		{
			if (num > num2)
			{
				m_StickToBottom = false;
			}
			else
			{
				float num3 = num - num2;
				float num4 = Mathf.Max(0f, contentHeight - m_ScrollView.contentViewport.layout.height);
				m_StickToBottom = num4 > 0f && base.serializedData.scrollOffset.y >= m_ScrollView.verticalScroller.highValue + num3;
			}
		}
		if (!flag || !Mathf.Approximately(num, value))
		{
			RegisterItemHeight(item.index, num);
			UpdateScrollViewContainer(num2, num);
			if (m_WaitingCache.Count == 0)
			{
				return true;
			}
		}
		return m_WaitingCache.Remove(item.index) && m_WaitingCache.Count == 0;
	}

	internal override T GetOrMakeItemAtIndex(int activeItemIndex = -1, int scrollViewIndex = -1)
	{
		T orMakeItemAtIndex = base.GetOrMakeItemAtIndex(activeItemIndex, scrollViewIndex);
		orMakeItemAtIndex.onGeometryChanged += m_GeometryChangedCallback;
		return orMakeItemAtIndex;
	}

	internal override void ReleaseItem(int activeItemsIndex)
	{
		T val = m_ActiveItems[activeItemsIndex];
		val.onGeometryChanged -= m_GeometryChangedCallback;
		int index = val.index;
		UnregisterItemHeight(index);
		base.ReleaseItem(activeItemsIndex);
		m_WaitingCache.Remove(index);
	}

	internal override void StartDragItem(ReusableCollectionItem item)
	{
		m_WaitingCache.Remove(item.index);
		base.StartDragItem(item);
		m_DraggedItem.onGeometryChanged -= m_GeometryChangedCallback;
	}

	internal override void EndDrag(int dropIndex)
	{
		bool flag = m_DraggedItem.index < dropIndex;
		int index = m_DraggedItem.index;
		int num = (flag ? 1 : (-1));
		float expectedItemHeight = GetExpectedItemHeight(index);
		for (int i = index; i != dropIndex; i += num)
		{
			float expectedItemHeight2 = GetExpectedItemHeight(i);
			float expectedItemHeight3 = GetExpectedItemHeight(i + num);
			if (!Mathf.Approximately(expectedItemHeight2, expectedItemHeight3))
			{
				RegisterItemHeight(i, expectedItemHeight3);
			}
		}
		RegisterItemHeight(flag ? (dropIndex - 1) : dropIndex, expectedItemHeight);
		if (firstVisibleIndex > m_DraggedItem.index)
		{
			firstVisibleIndex = GetFirstVisibleItem(base.serializedData.scrollOffset.y);
			UpdateAnchor();
		}
		m_DraggedItem.onGeometryChanged += m_GeometryChangedCallback;
		base.EndDrag(dropIndex);
	}

	private void HideItem(int activeItemsIndex)
	{
		T val = m_ActiveItems[activeItemsIndex];
		val.rootElement.style.display = DisplayStyle.None;
		m_WaitingCache.Remove(val.index);
	}

	private void MarkWaitingForLayout(T item)
	{
		if (!item.isDragGhost)
		{
			m_WaitingCache.Add(item.index);
			item.rootElement.lastLayout.size = Vector2.zero;
			item.rootElement.MarkDirtyRepaint();
		}
	}

	private bool IsIndexOutOfBounds(int i)
	{
		return m_CollectionView.itemsSource == null || i >= base.itemsCount;
	}
}
