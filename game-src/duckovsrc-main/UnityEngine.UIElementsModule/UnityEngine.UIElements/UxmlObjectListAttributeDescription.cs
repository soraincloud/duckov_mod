using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class UxmlObjectListAttributeDescription<T> : UxmlObjectAttributeDescription<List<T>> where T : new()
{
	public override List<T> GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
	{
		List<T> list = cc.visualTreeAsset?.GetUxmlObjects<T>(bag, cc);
		if (list != null)
		{
			List<T> list2 = null;
			foreach (T item in list)
			{
				if (list2 == null)
				{
					list2 = new List<T>();
				}
				list2.Add(item);
			}
			return list2;
		}
		return base.defaultValue;
	}
}
