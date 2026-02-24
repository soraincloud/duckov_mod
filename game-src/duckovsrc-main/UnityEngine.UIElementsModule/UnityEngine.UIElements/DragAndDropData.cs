using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal abstract class DragAndDropData : IDragAndDropData
{
	internal const string dragSourceKey = "__unity-drag-and-drop__source-view";

	object IDragAndDropData.userData => GetGenericData("__unity-drag-and-drop__source-view");

	public abstract object source { get; }

	public abstract DragVisualMode visualMode { get; }

	public abstract IEnumerable<Object> unityObjectReferences { get; }

	public abstract object GetGenericData(string key);

	public abstract void SetGenericData(string key, object data);
}
