using System.Collections.Generic;

namespace UnityEngine.UIElements;

public readonly struct TreeViewItemData<T>
{
	private readonly T m_Data;

	private readonly IList<TreeViewItemData<T>> m_Children;

	public int id { get; }

	public T data => m_Data;

	public IEnumerable<TreeViewItemData<T>> children => m_Children;

	public bool hasChildren => m_Children != null && m_Children.Count > 0;

	public TreeViewItemData(int id, T data, List<TreeViewItemData<T>> children = null)
	{
		this.id = id;
		m_Data = data;
		m_Children = children ?? new List<TreeViewItemData<T>>();
	}

	internal void AddChild(TreeViewItemData<T> child)
	{
		m_Children.Add(child);
	}

	internal void AddChildren(IList<TreeViewItemData<T>> children)
	{
		foreach (TreeViewItemData<T> child in children)
		{
			AddChild(child);
		}
	}

	internal void InsertChild(TreeViewItemData<T> child, int index)
	{
		if (index < 0 || index >= m_Children.Count)
		{
			m_Children.Add(child);
		}
		else
		{
			m_Children.Insert(index, child);
		}
	}

	internal void RemoveChild(int childId)
	{
		if (m_Children == null)
		{
			return;
		}
		for (int i = 0; i < m_Children.Count; i++)
		{
			if (childId == m_Children[i].id)
			{
				m_Children.RemoveAt(i);
				break;
			}
		}
	}

	internal int GetChildIndex(int itemId)
	{
		int num = 0;
		foreach (TreeViewItemData<T> child in m_Children)
		{
			if (child.id == itemId)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	internal void ReplaceChild(TreeViewItemData<T> newChild)
	{
		if (!hasChildren)
		{
			return;
		}
		int num = 0;
		foreach (TreeViewItemData<T> child in m_Children)
		{
			if (child.id == newChild.id)
			{
				m_Children.RemoveAt(num);
				m_Children.Insert(num, newChild);
				break;
			}
			num++;
		}
	}
}
