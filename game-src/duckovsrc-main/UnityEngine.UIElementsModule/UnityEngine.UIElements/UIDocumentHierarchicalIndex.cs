using System;
using System.Text;

namespace UnityEngine.UIElements;

internal struct UIDocumentHierarchicalIndex : IComparable<UIDocumentHierarchicalIndex>
{
	internal int[] pathToParent;

	public int CompareTo(UIDocumentHierarchicalIndex other)
	{
		if (pathToParent == null)
		{
			if (other.pathToParent == null)
			{
				return 0;
			}
			return 1;
		}
		if (other.pathToParent == null)
		{
			return -1;
		}
		int num = pathToParent.Length;
		int num2 = other.pathToParent.Length;
		for (int i = 0; i < num && i < num2; i++)
		{
			if (pathToParent[i] < other.pathToParent[i])
			{
				return -1;
			}
			if (pathToParent[i] > other.pathToParent[i])
			{
				return 1;
			}
		}
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("pathToParent = [");
		if (pathToParent != null)
		{
			int num = pathToParent.Length;
			for (int i = 0; i < num; i++)
			{
				stringBuilder.Append(pathToParent[i]);
				if (i < num - 1)
				{
					stringBuilder.Append(", ");
				}
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
