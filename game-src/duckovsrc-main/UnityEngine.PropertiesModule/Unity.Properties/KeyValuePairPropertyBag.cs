using System.Collections.Generic;

namespace Unity.Properties;

public class KeyValuePairPropertyBag<TKey, TValue> : PropertyBag<KeyValuePair<TKey, TValue>>, INamedProperties<KeyValuePair<TKey, TValue>>
{
	private static readonly DelegateProperty<KeyValuePair<TKey, TValue>, TKey> s_KeyProperty = new DelegateProperty<KeyValuePair<TKey, TValue>, TKey>("Key", delegate(ref KeyValuePair<TKey, TValue> container)
	{
		return container.Key;
	});

	private static readonly DelegateProperty<KeyValuePair<TKey, TValue>, TValue> s_ValueProperty = new DelegateProperty<KeyValuePair<TKey, TValue>, TValue>("Value", delegate(ref KeyValuePair<TKey, TValue> container)
	{
		return container.Value;
	});

	public override PropertyCollection<KeyValuePair<TKey, TValue>> GetProperties()
	{
		return new PropertyCollection<KeyValuePair<TKey, TValue>>(GetPropertiesEnumerable());
	}

	public override PropertyCollection<KeyValuePair<TKey, TValue>> GetProperties(ref KeyValuePair<TKey, TValue> container)
	{
		return new PropertyCollection<KeyValuePair<TKey, TValue>>(GetPropertiesEnumerable());
	}

	private static IEnumerable<IProperty<KeyValuePair<TKey, TValue>>> GetPropertiesEnumerable()
	{
		yield return s_KeyProperty;
		yield return s_ValueProperty;
	}

	public bool TryGetProperty(ref KeyValuePair<TKey, TValue> container, string name, out IProperty<KeyValuePair<TKey, TValue>> property)
	{
		if (name == "Key")
		{
			property = s_KeyProperty;
			return true;
		}
		if (name == "Value")
		{
			property = s_ValueProperty;
			return true;
		}
		property = null;
		return false;
	}
}
