using System;

namespace UnityEngine.UIElements;

internal static class DragAndDropUtility
{
	private static Func<IDragAndDrop> s_MakeDragAndDropClientFunc;

	private static IDragAndDrop s_DragAndDropEditor;

	private static IDragAndDrop s_DragAndDropPlayMode;

	internal static IDragAndDrop GetDragAndDrop(IPanel panel)
	{
		if (panel.contextType == ContextType.Player)
		{
			return s_DragAndDropPlayMode ?? (s_DragAndDropPlayMode = new DefaultDragAndDropClient());
		}
		IDragAndDrop dragAndDrop = s_DragAndDropEditor;
		if (dragAndDrop == null)
		{
			if (s_MakeDragAndDropClientFunc == null)
			{
				IDragAndDrop dragAndDrop2 = new DefaultDragAndDropClient();
				dragAndDrop = dragAndDrop2;
			}
			else
			{
				dragAndDrop = s_MakeDragAndDropClientFunc();
			}
			s_DragAndDropEditor = dragAndDrop;
		}
		return dragAndDrop;
	}

	internal static void RegisterMakeClientFunc(Func<IDragAndDrop> makeClient)
	{
		s_MakeDragAndDropClientFunc = makeClient;
		s_DragAndDropEditor = null;
	}
}
