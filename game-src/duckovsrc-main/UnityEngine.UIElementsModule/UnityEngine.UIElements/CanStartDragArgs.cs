using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal readonly struct CanStartDragArgs
{
	public readonly VisualElement draggedElement;

	public readonly int id;

	public readonly IEnumerable<int> selectedIds;

	internal CanStartDragArgs(VisualElement draggedElement, int id, IEnumerable<int> selectedIds)
	{
		this.draggedElement = draggedElement;
		this.id = id;
		this.selectedIds = selectedIds;
	}
}
