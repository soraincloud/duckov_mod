using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public class TreeView : BaseTreeView
{
	public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits>
	{
	}

	public new class UxmlTraits : BaseTreeView.UxmlTraits
	{
	}

	private Func<VisualElement> m_MakeItem;

	private Action<VisualElement, int> m_BindItem;

	public new Func<VisualElement> makeItem
	{
		get
		{
			return m_MakeItem;
		}
		set
		{
			if (value != m_MakeItem)
			{
				m_MakeItem = value;
				Rebuild();
			}
		}
	}

	public new Action<VisualElement, int> bindItem
	{
		get
		{
			return m_BindItem;
		}
		set
		{
			if (value != m_BindItem)
			{
				m_BindItem = value;
				RefreshItems();
			}
		}
	}

	public new Action<VisualElement, int> unbindItem { get; set; }

	public new Action<VisualElement> destroyItem { get; set; }

	public new TreeViewController viewController => base.viewController as TreeViewController;

	internal override void SetRootItemsInternal<T>(IList<TreeViewItemData<T>> rootItems)
	{
		TreeViewHelpers<T, DefaultTreeViewController<T>>.SetRootItems(this, rootItems, () => new DefaultTreeViewController<T>());
	}

	internal override bool HasValidDataAndBindings()
	{
		return base.HasValidDataAndBindings() && makeItem != null == (bindItem != null);
	}

	protected override CollectionViewController CreateViewController()
	{
		return new DefaultTreeViewController<object>();
	}

	public TreeView()
		: this(null, null)
	{
	}

	public TreeView(Func<VisualElement> makeItem, Action<VisualElement, int> bindItem)
		: base(-1)
	{
		this.makeItem = makeItem;
		this.bindItem = bindItem;
	}

	public TreeView(int itemHeight, Func<VisualElement> makeItem, Action<VisualElement, int> bindItem)
		: this(makeItem, bindItem)
	{
		base.fixedItemHeight = itemHeight;
	}

	private protected override IEnumerable<TreeViewItemData<T>> GetSelectedItemsInternal<T>()
	{
		return TreeViewHelpers<T, DefaultTreeViewController<T>>.GetSelectedItems(this);
	}

	private protected override T GetItemDataForIndexInternal<T>(int index)
	{
		return TreeViewHelpers<T, DefaultTreeViewController<T>>.GetItemDataForIndex(this, index);
	}

	private protected override T GetItemDataForIdInternal<T>(int id)
	{
		return TreeViewHelpers<T, DefaultTreeViewController<T>>.GetItemDataForId(this, id);
	}

	private protected override void AddItemInternal<T>(TreeViewItemData<T> item, int parentId, int childIndex, bool rebuildTree)
	{
		TreeViewHelpers<T, DefaultTreeViewController<T>>.AddItem(this, item, parentId, childIndex, rebuildTree);
	}
}
