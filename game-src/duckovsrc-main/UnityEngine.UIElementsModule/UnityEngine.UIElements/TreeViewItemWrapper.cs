using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal readonly struct TreeViewItemWrapper
{
	public readonly TreeItem item;

	public readonly int depth;

	public int id => item.id;

	public int parentId => item.parentId;

	public IEnumerable<int> childrenIds => item.childrenIds;

	public bool hasChildren => item.hasChildren;

	public TreeViewItemWrapper(TreeItem item, int depth)
	{
		this.item = item;
		this.depth = depth;
	}
}
