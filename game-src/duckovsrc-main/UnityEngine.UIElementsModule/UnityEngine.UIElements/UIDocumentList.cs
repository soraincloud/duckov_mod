using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class UIDocumentList
{
	internal List<UIDocument> m_AttachedUIDocuments = new List<UIDocument>();

	internal void RemoveFromListAndFromVisualTree(UIDocument uiDocument)
	{
		m_AttachedUIDocuments.Remove(uiDocument);
		uiDocument.rootVisualElement?.RemoveFromHierarchy();
	}

	internal void AddToListAndToVisualTree(UIDocument uiDocument, VisualElement visualTree, int firstInsertIndex = 0)
	{
		int num = 0;
		foreach (UIDocument attachedUIDocument in m_AttachedUIDocuments)
		{
			if (uiDocument.sortingOrder > attachedUIDocument.sortingOrder)
			{
				num++;
				continue;
			}
			if (!(uiDocument.sortingOrder < attachedUIDocument.sortingOrder) && uiDocument.m_UIDocumentCreationIndex > attachedUIDocument.m_UIDocumentCreationIndex)
			{
				num++;
				continue;
			}
			break;
		}
		if (num < m_AttachedUIDocuments.Count)
		{
			m_AttachedUIDocuments.Insert(num, uiDocument);
			if (visualTree == null || uiDocument.rootVisualElement == null)
			{
				return;
			}
			if (num > 0)
			{
				VisualElement visualElement = null;
				int num2 = 1;
				while (visualElement == null && num - num2 >= 0)
				{
					UIDocument uIDocument = m_AttachedUIDocuments[num - num2++];
					visualElement = uIDocument.rootVisualElement;
				}
				if (visualElement != null)
				{
					num = visualTree.IndexOf(visualElement) + 1;
				}
			}
			if (num > visualTree.childCount)
			{
				num = visualTree.childCount;
			}
		}
		else
		{
			m_AttachedUIDocuments.Add(uiDocument);
		}
		if (visualTree != null && uiDocument.rootVisualElement != null)
		{
			int num3 = firstInsertIndex + num;
			if (num3 < visualTree.childCount)
			{
				visualTree.Insert(num3, uiDocument.rootVisualElement);
			}
			else
			{
				visualTree.Add(uiDocument.rootVisualElement);
			}
		}
	}
}
