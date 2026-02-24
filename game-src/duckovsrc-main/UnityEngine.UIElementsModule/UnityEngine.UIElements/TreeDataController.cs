using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal sealed class TreeDataController<T>
{
	private TreeData<T> m_TreeData;

	private Stack<IEnumerator<int>> m_IteratorStack = new Stack<IEnumerator<int>>();

	public void SetRootItems(IList<TreeViewItemData<T>> rootItems)
	{
		m_TreeData = new TreeData<T>(rootItems);
	}

	public void AddItem(in TreeViewItemData<T> item, int parentId, int childIndex)
	{
		m_TreeData.AddItem(item, parentId, childIndex);
	}

	public bool TryRemoveItem(int id)
	{
		return m_TreeData.TryRemove(id);
	}

	public TreeViewItemData<T> GetTreeItemDataForId(int id)
	{
		return m_TreeData.GetDataForId(id);
	}

	public T GetDataForId(int id)
	{
		return m_TreeData.GetDataForId(id).data;
	}

	public int GetParentId(int id)
	{
		return m_TreeData.GetParentId(id);
	}

	public bool HasChildren(int id)
	{
		return m_TreeData.GetDataForId(id).hasChildren;
	}

	private static IEnumerable<int> GetItemIds(IEnumerable<TreeViewItemData<T>> items)
	{
		if (items == null)
		{
			yield break;
		}
		foreach (TreeViewItemData<T> item2 in items)
		{
			yield return item2.id;
		}
	}

	public IEnumerable<int> GetChildrenIds(int id)
	{
		return GetItemIds(m_TreeData.GetDataForId(id).children);
	}

	public void Move(int id, int newParentId, int childIndex = -1)
	{
		if (id != newParentId && !IsChildOf(newParentId, id))
		{
			m_TreeData.Move(id, newParentId, childIndex);
		}
	}

	public bool IsChildOf(int childId, int id)
	{
		return m_TreeData.HasAncestor(childId, id);
	}

	public IEnumerable<int> GetAllItemIds(IEnumerable<int> rootIds = null)
	{
		m_IteratorStack.Clear();
		if (rootIds == null)
		{
			if (m_TreeData.rootItemIds == null)
			{
				yield break;
			}
			rootIds = m_TreeData.rootItemIds;
		}
		IEnumerator<int> currentIterator = rootIds.GetEnumerator();
		while (true)
		{
			if (!currentIterator.MoveNext())
			{
				if (m_IteratorStack.Count > 0)
				{
					currentIterator = m_IteratorStack.Pop();
					continue;
				}
				break;
			}
			int currentItemId = currentIterator.Current;
			yield return currentItemId;
			if (HasChildren(currentItemId))
			{
				m_IteratorStack.Push(currentIterator);
				currentIterator = GetChildrenIds(currentItemId).GetEnumerator();
			}
		}
	}
}
