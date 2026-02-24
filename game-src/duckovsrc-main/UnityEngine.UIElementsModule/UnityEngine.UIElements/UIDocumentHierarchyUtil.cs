using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class UIDocumentHierarchyUtil
{
	internal static UIDocumentHierarchicalIndexComparer indexComparer = new UIDocumentHierarchicalIndexComparer();

	internal static int FindHierarchicalSortedIndex(SortedDictionary<UIDocumentHierarchicalIndex, UIDocument> children, UIDocument child)
	{
		int num = 0;
		foreach (UIDocument value in children.Values)
		{
			if (value == child)
			{
				return num;
			}
			if (value.rootVisualElement != null && value.rootVisualElement.parent != null)
			{
				num++;
			}
		}
		return num;
	}

	internal static void SetHierarchicalIndex(Transform childTransform, Transform directParentTransform, Transform mainParentTransform, out UIDocumentHierarchicalIndex hierarchicalIndex)
	{
		if (mainParentTransform == null || childTransform == null)
		{
			hierarchicalIndex.pathToParent = null;
			return;
		}
		if (directParentTransform == mainParentTransform)
		{
			hierarchicalIndex.pathToParent = new int[1] { childTransform.GetSiblingIndex() };
			return;
		}
		List<int> list = new List<int>();
		while (mainParentTransform != childTransform && childTransform != null)
		{
			list.Add(childTransform.GetSiblingIndex());
			childTransform = childTransform.parent;
		}
		list.Reverse();
		hierarchicalIndex.pathToParent = list.ToArray();
	}

	internal static void SetGlobalIndex(Transform objectTransform, Transform directParentTransform, out UIDocumentHierarchicalIndex globalIndex)
	{
		if (objectTransform == null)
		{
			globalIndex.pathToParent = null;
			return;
		}
		if (directParentTransform == null)
		{
			globalIndex.pathToParent = new int[1] { objectTransform.GetSiblingIndex() };
			return;
		}
		List<int> list = new List<int> { objectTransform.GetSiblingIndex() };
		while (directParentTransform != null)
		{
			list.Add(directParentTransform.GetSiblingIndex());
			directParentTransform = directParentTransform.parent;
		}
		list.Reverse();
		globalIndex.pathToParent = list.ToArray();
	}
}
