using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UIElements;

internal readonly struct TreeItem
{
	public const int invalidId = -1;

	public int id { get; }

	public int parentId { get; }

	public IEnumerable<int> childrenIds { get; }

	public bool hasChildren => childrenIds != null && childrenIds.Any();

	public TreeItem(int id, int parentId = -1, IEnumerable<int> childrenIds = null)
	{
		this.id = id;
		this.parentId = parentId;
		this.childrenIds = childrenIds;
	}
}
