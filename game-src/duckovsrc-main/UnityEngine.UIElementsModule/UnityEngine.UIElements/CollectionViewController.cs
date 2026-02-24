#define UNITY_ASSERTIONS
using System;
using System.Collections;
using System.Linq;
using UnityEngine.Assertions;

namespace UnityEngine.UIElements;

public abstract class CollectionViewController : IDisposable
{
	private BaseVerticalCollectionView m_View;

	private IList m_ItemsSource;

	public virtual IList itemsSource
	{
		get
		{
			return m_ItemsSource;
		}
		set
		{
			if (m_ItemsSource != value)
			{
				m_ItemsSource = value;
				if (m_View.GetProperty("__unity-collection-view-internal-binding") == null)
				{
					m_View.RefreshItems();
				}
				RaiseItemsSourceChanged();
			}
		}
	}

	protected BaseVerticalCollectionView view => m_View;

	public event Action itemsSourceChanged;

	public event Action<int, int> itemIndexChanged;

	protected void SetItemsSourceWithoutNotify(IList source)
	{
		m_ItemsSource = source;
	}

	public void SetView(BaseVerticalCollectionView collectionView)
	{
		m_View = collectionView;
		PrepareView();
		Assert.IsNotNull(m_View, "View must not be null.");
	}

	protected virtual void PrepareView()
	{
	}

	public virtual void Dispose()
	{
		this.itemsSourceChanged = null;
		this.itemIndexChanged = null;
		m_View = null;
	}

	public virtual int GetItemsCount()
	{
		return m_ItemsSource?.Count ?? 0;
	}

	internal virtual int GetItemsMinCount()
	{
		return GetItemsCount();
	}

	public virtual int GetIndexForId(int id)
	{
		return id;
	}

	public virtual int GetIdForIndex(int index)
	{
		return index;
	}

	public virtual object GetItemForIndex(int index)
	{
		if (m_ItemsSource == null)
		{
			return null;
		}
		if (index < 0 || index >= m_ItemsSource.Count)
		{
			return null;
		}
		return m_ItemsSource[index];
	}

	internal virtual object GetItemForId(int id)
	{
		if (m_ItemsSource == null)
		{
			return null;
		}
		int indexForId = GetIndexForId(id);
		if (indexForId < 0 || indexForId >= m_ItemsSource.Count)
		{
			return null;
		}
		return m_ItemsSource[indexForId];
	}

	internal virtual void InvokeMakeItem(ReusableCollectionItem reusableItem)
	{
		reusableItem.Init(MakeItem());
	}

	internal virtual void InvokeBindItem(ReusableCollectionItem reusableItem, int index)
	{
		BindItem(reusableItem.bindableElement, index);
		reusableItem.SetSelected(m_View.selectedIndices.Contains(index));
		reusableItem.rootElement.pseudoStates &= ~PseudoStates.Hover;
		reusableItem.index = index;
	}

	internal virtual void InvokeUnbindItem(ReusableCollectionItem reusableItem, int index)
	{
		UnbindItem(reusableItem.bindableElement, index);
		reusableItem.index = -1;
	}

	internal virtual void InvokeDestroyItem(ReusableCollectionItem reusableItem)
	{
		DestroyItem(reusableItem.bindableElement);
	}

	protected abstract VisualElement MakeItem();

	protected abstract void BindItem(VisualElement element, int index);

	protected abstract void UnbindItem(VisualElement element, int index);

	protected abstract void DestroyItem(VisualElement element);

	protected void RaiseItemsSourceChanged()
	{
		this.itemsSourceChanged?.Invoke();
	}

	protected void RaiseItemIndexChanged(int srcIndex, int dstIndex)
	{
		this.itemIndexChanged?.Invoke(srcIndex, dstIndex);
	}
}
