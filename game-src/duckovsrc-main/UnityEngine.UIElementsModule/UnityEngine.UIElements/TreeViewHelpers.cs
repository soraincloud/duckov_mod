using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal static class TreeViewHelpers<T, TDefaultController> where TDefaultController : BaseTreeViewController, IDefaultTreeViewController<T>
{
	internal static void SetRootItems(BaseTreeView treeView, IList<TreeViewItemData<T>> rootItems, Func<TDefaultController> createController)
	{
		if (treeView.viewController is TDefaultController val)
		{
			val.SetRootItems(rootItems);
			return;
		}
		TDefaultController val2 = createController();
		treeView.SetViewController(val2);
		val2.SetRootItems(rootItems);
	}

	internal static IEnumerable<TreeViewItemData<T>> GetSelectedItems(BaseTreeView treeView)
	{
		BaseTreeViewController viewController = treeView.viewController;
		if (viewController is TDefaultController defaultController)
		{
			foreach (int index in treeView.selectedIndices)
			{
				yield return defaultController.GetTreeViewItemDataForIndex(index);
			}
			yield break;
		}
		if (treeView.viewController?.GetType().GetGenericTypeDefinition() == typeof(TDefaultController).GetGenericTypeDefinition())
		{
			BaseTreeViewController viewController2 = treeView.viewController;
			throw new ArgumentException(string.Format(arg1: (viewController2 != null) ? viewController2.GetType().GetGenericArguments()[0] : null, format: "Type parameter ({0}) differs from data source ({1}) and is not recognized by the controller.", arg0: typeof(T)));
		}
		throw new ArgumentException("GetSelectedItems<T>() only works when using the default controller. Use your controller along with the selectedIndices enumerable instead.");
	}

	internal static T GetItemDataForIndex(BaseTreeView treeView, int index)
	{
		if (treeView.viewController is TDefaultController val)
		{
			return val.GetDataForIndex(index);
		}
		object obj = treeView.viewController?.GetItemForIndex(index);
		Type type = obj?.GetType();
		if (type == typeof(T))
		{
			return (T)obj;
		}
		if (type == null && treeView.viewController?.GetType().GetGenericTypeDefinition() == typeof(TDefaultController).GetGenericTypeDefinition())
		{
			BaseTreeViewController viewController = treeView.viewController;
			type = ((viewController != null) ? viewController.GetType().GetGenericArguments()[0] : null);
		}
		throw new ArgumentException($"Type parameter ({typeof(T)}) differs from data source ({type}) and is not recognized by the controller.");
	}

	internal static T GetItemDataForId(BaseTreeView treeView, int id)
	{
		if (treeView.viewController is TDefaultController val)
		{
			return val.GetDataForId(id);
		}
		object obj = treeView.viewController?.GetItemForIndex(treeView.viewController.GetIndexForId(id));
		Type type = obj?.GetType();
		if (type == typeof(T))
		{
			return (T)obj;
		}
		if (type == null && treeView.viewController?.GetType().GetGenericTypeDefinition() == typeof(TDefaultController).GetGenericTypeDefinition())
		{
			BaseTreeViewController viewController = treeView.viewController;
			type = ((viewController != null) ? viewController.GetType().GetGenericArguments()[0] : null);
		}
		throw new ArgumentException($"Type parameter ({typeof(T)}) differs from data source ({type}) and is not recognized by the controller.");
	}

	internal static void AddItem(BaseTreeView treeView, TreeViewItemData<T> item, int parentId = -1, int childIndex = -1, bool rebuildTree = true)
	{
		if (treeView.viewController is TDefaultController val)
		{
			val.AddItem(in item, parentId, childIndex, rebuildTree);
			if (rebuildTree)
			{
				treeView.RefreshItems();
			}
			return;
		}
		Type arg = null;
		if (treeView.viewController?.GetType().GetGenericTypeDefinition() == typeof(TDefaultController).GetGenericTypeDefinition())
		{
			BaseTreeViewController viewController = treeView.viewController;
			arg = ((viewController != null) ? viewController.GetType().GetGenericArguments()[0] : null);
		}
		throw new ArgumentException($"Type parameter ({typeof(T)}) differs from data source ({arg})and is not recognized by the controller.");
	}
}
