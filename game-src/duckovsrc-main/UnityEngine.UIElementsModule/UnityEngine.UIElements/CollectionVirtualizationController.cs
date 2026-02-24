using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal abstract class CollectionVirtualizationController
{
	protected readonly ScrollView m_ScrollView;

	public abstract int firstVisibleIndex { get; protected set; }

	public abstract int visibleItemCount { get; }

	public abstract IEnumerable<ReusableCollectionItem> activeItems { get; }

	protected CollectionVirtualizationController(ScrollView scrollView)
	{
		m_ScrollView = scrollView;
	}

	public abstract void Refresh(bool rebuild);

	public abstract void ScrollToItem(int id);

	public abstract void Resize(Vector2 size);

	public abstract void OnScroll(Vector2 offset);

	public abstract int GetIndexFromPosition(Vector2 position);

	public abstract float GetExpectedItemHeight(int index);

	public abstract float GetExpectedContentHeight();

	public abstract void OnFocus(VisualElement leafTarget);

	public abstract void OnBlur(VisualElement willFocus);

	public abstract void UpdateBackground();

	internal abstract void StartDragItem(ReusableCollectionItem item);

	internal abstract void EndDrag(int dropIndex);
}
