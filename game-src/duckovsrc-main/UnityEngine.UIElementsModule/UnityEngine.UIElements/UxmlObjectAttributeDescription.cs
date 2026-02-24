using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class UxmlObjectAttributeDescription<T> where T : new()
{
	public T defaultValue { get; set; }

	public virtual T GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		List<T> list = cc.visualTreeAsset?.GetUxmlObjects<T>(bag, cc);
		if (list != null)
		{
			using List<T>.Enumerator enumerator = list.GetEnumerator();
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return defaultValue;
	}
}
