using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DefaultTreeViewController<T> : TreeViewController, IDefaultTreeViewController, IDefaultTreeViewController<T>
{
	private TreeDataController<T> m_TreeDataController;

	private TreeDataController<T> treeDataController => m_TreeDataController ?? (m_TreeDataController = new TreeDataController<T>());

	public override IList itemsSource
	{
		get
		{
			return base.itemsSource;
		}
		set
		{
			if (value == null)
			{
				SetRootItems(null);
			}
			else if (value is IList<TreeViewItemData<T>> rootItems)
			{
				SetRootItems(rootItems);
			}
			else
			{
				Debug.LogError($"Type does not match this tree view controller's data type ({typeof(T)}).");
			}
		}
	}

	public void SetRootItems(IList<TreeViewItemData<T>> items)
	{
		if (items != base.itemsSource)
		{
			treeDataController.SetRootItems(items);
			RebuildTree();
			RaiseItemsSourceChanged();
		}
	}

	public virtual void AddItem(in TreeViewItemData<T> item, int parentId, int childIndex, bool rebuildTree = true)
	{
		treeDataController.AddItem(in item, parentId, childIndex);
		if (rebuildTree)
		{
			RebuildTree();
		}
	}

	public override bool TryRemoveItem(int id, bool rebuildTree = true)
	{
		if (treeDataController.TryRemoveItem(id))
		{
			if (rebuildTree)
			{
				RebuildTree();
			}
			return true;
		}
		return false;
	}

	public virtual object GetItemDataForId(int id)
	{
		return treeDataController.GetTreeItemDataForId(id).data;
	}

	public virtual TreeViewItemData<T> GetTreeViewItemDataForId(int id)
	{
		return treeDataController.GetTreeItemDataForId(id);
	}

	public virtual TreeViewItemData<T> GetTreeViewItemDataForIndex(int index)
	{
		int idForIndex = GetIdForIndex(index);
		return treeDataController.GetTreeItemDataForId(idForIndex);
	}

	public virtual T GetDataForId(int id)
	{
		return treeDataController.GetDataForId(id);
	}

	public virtual T GetDataForIndex(int index)
	{
		return treeDataController.GetDataForId(GetIdForIndex(index));
	}

	public override object GetItemForIndex(int index)
	{
		return treeDataController.GetDataForId(GetIdForIndex(index));
	}

	public override int GetParentId(int id)
	{
		return treeDataController.GetParentId(id);
	}

	public override bool HasChildren(int id)
	{
		return treeDataController.HasChildren(id);
	}

	public override IEnumerable<int> GetChildrenIds(int id)
	{
		return treeDataController.GetChildrenIds(id);
	}

	public override void Move(int id, int newParentId, int childIndex = -1, bool rebuildTree = true)
	{
		if (id != newParentId && !IsChildOf(newParentId, id))
		{
			treeDataController.Move(id, newParentId, childIndex);
			if (rebuildTree)
			{
				RebuildTree();
				RaiseItemParentChanged(id, newParentId);
			}
		}
	}

	private bool IsChildOf(int childId, int id)
	{
		return treeDataController.IsChildOf(childId, id);
	}

	public override IEnumerable<int> GetAllItemIds(IEnumerable<int> rootIds = null)
	{
		return treeDataController.GetAllItemIds(rootIds);
	}
}
