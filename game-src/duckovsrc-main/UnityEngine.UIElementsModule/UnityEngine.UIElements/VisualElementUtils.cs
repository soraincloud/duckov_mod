using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class VisualElementUtils
{
	private static readonly HashSet<string> s_usedNames = new HashSet<string>();

	private static readonly Type s_FoldoutType = typeof(Foldout);

	private static readonly string s_InspectorElementUssClassName = "unity-inspector-element";

	public static string GetUniqueName(string nameBase)
	{
		string text = nameBase;
		int num = 2;
		while (s_usedNames.Contains(text))
		{
			text = nameBase + num;
			num++;
		}
		s_usedNames.Add(text);
		return text;
	}

	internal static int GetFoldoutDepth(this VisualElement element)
	{
		int num = 0;
		if (element.parent != null)
		{
			for (VisualElement parent = element.parent; parent != null; parent = parent.parent)
			{
				if (s_FoldoutType.IsAssignableFrom(parent.GetType()))
				{
					num++;
				}
			}
		}
		return num;
	}

	internal static void AssignInspectorStyleIfNecessary(this VisualElement element, string classNameToEnable)
	{
		VisualElement firstAncestorWhere = element.GetFirstAncestorWhere((VisualElement i) => i.ClassListContains(s_InspectorElementUssClassName));
		element.EnableInClassList(classNameToEnable, firstAncestorWhere != null);
	}
}
