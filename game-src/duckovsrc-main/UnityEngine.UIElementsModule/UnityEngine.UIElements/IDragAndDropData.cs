using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal interface IDragAndDropData
{
	object userData { get; }

	IEnumerable<Object> unityObjectReferences { get; }

	object GetGenericData(string key);
}
