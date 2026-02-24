using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets;

public class InvalidKeyException : Exception
{
	private AddressablesImpl m_Addressables;

	private const string BaseInvalidKeyMessageFormat = "{0}, Key={1}, Type={2}";

	public object Key { get; private set; }

	public Type Type { get; private set; }

	public Addressables.MergeMode? MergeMode { get; }

	public override string Message
	{
		get
		{
			string text = Key as string;
			if (!string.IsNullOrEmpty(text))
			{
				if (m_Addressables == null)
				{
					return $"{base.Message}, Key={text}, Type={Type}";
				}
				return GetMessageForSingleKey(text);
			}
			if (Key is IEnumerable enumerable)
			{
				int num = 0;
				List<string> list = new List<string>();
				HashSet<string> hashSet = new HashSet<string>();
				foreach (object item in enumerable)
				{
					num++;
					hashSet.Add(item.GetType().ToString());
					if (item is string)
					{
						list.Add(item as string);
					}
				}
				if (!MergeMode.HasValue)
				{
					string cSVString = GetCSVString(list, "Key=", "Keys=");
					return $"{base.Message} No MergeMode is set to merge the multiple keys requested. {cSVString}, Type={Type}";
				}
				if (num != list.Count)
				{
					string cSVString2 = GetCSVString(hashSet, "Type=", "Types=");
					return base.Message + " Enumerable key contains multiple Types. " + cSVString2 + ", all Keys are expected to be strings";
				}
				if (num == 1)
				{
					return GetMessageForSingleKey(list[0]);
				}
				return GetMessageforMergeKeys(list);
			}
			return $"{base.Message}, Key={Key}, Type={Type}";
		}
	}

	public InvalidKeyException(object key)
		: this(key, typeof(object))
	{
	}

	public InvalidKeyException(object key, Type type)
	{
		Key = key;
		Type = type;
	}

	internal InvalidKeyException(object key, Type type, AddressablesImpl addr)
	{
		Key = key;
		Type = type;
		m_Addressables = addr;
	}

	public InvalidKeyException(object key, Type type, Addressables.MergeMode mergeMode)
	{
		Key = key;
		Type = type;
		MergeMode = mergeMode;
	}

	internal InvalidKeyException(object key, Type type, Addressables.MergeMode mergeMode, AddressablesImpl addr)
	{
		Key = key;
		Type = type;
		MergeMode = mergeMode;
		m_Addressables = addr;
	}

	public InvalidKeyException()
	{
	}

	public InvalidKeyException(string message)
		: base(message)
	{
	}

	public InvalidKeyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected InvalidKeyException(SerializationInfo message, StreamingContext context)
		: base(message, context)
	{
	}

	private string GetMessageForSingleKey(string keyString)
	{
		HashSet<Type> typesForKey = GetTypesForKey(keyString);
		if (typesForKey.Count == 0)
		{
			return GetNotFoundMessage(keyString);
		}
		if (typesForKey.Count == 1)
		{
			return GetTypeNotAssignableMessage(keyString, typesForKey);
		}
		return GetMultipleAssignableTypesMessage(keyString, typesForKey);
	}

	private string GetNotFoundMessage(string keyString)
	{
		return base.Message + " No Location found for Key=" + keyString;
	}

	private string GetTypeNotAssignableMessage(string keyString, HashSet<Type> typesAvailableForKey)
	{
		Type type = null;
		foreach (Type item in typesAvailableForKey)
		{
			type = item;
		}
		if (type == null)
		{
			return $"{base.Message}, Key={keyString}, Type={Type}";
		}
		return $"{base.Message} No Asset found with for Key={keyString}. Key exists as Type={type}, which is not assignable from the requested Type={Type}";
	}

	private string GetMultipleAssignableTypesMessage(string keyString, HashSet<Type> typesAvailableForKey)
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		int num = 0;
		foreach (Type item in typesAvailableForKey)
		{
			num++;
			stringBuilder.Append((num > 1) ? $", {item}" : item.ToString());
		}
		return $"{base.Message} No Asset found with for Key={keyString}. Key exists as multiple Types={stringBuilder}, which is not assignable from the requested Type={Type}";
	}

	private string GetMessageforMergeKeys(List<string> keys)
	{
		string cSVString = GetCSVString(keys, "Key=", "Keys=");
		string format = "\nNo Location found for Key={0}";
		StringBuilder stringBuilder = null;
		switch (MergeMode)
		{
		case Addressables.MergeMode.Union:
		{
			stringBuilder = new StringBuilder($"{base.Message} No {MergeMode.Value} of Assets between {cSVString} with Type={Type}");
			Dictionary<Type, List<string>> dictionary2 = new Dictionary<Type, List<string>>();
			foreach (string key in keys)
			{
				if (!GetTypeToKeys(key, dictionary2))
				{
					stringBuilder.Append(string.Format(format, key));
				}
			}
			foreach (KeyValuePair<Type, List<string>> item in dictionary2)
			{
				string cSVString3 = GetCSVString(item.Value, "Key=", "Keys=");
				List<string> list = new List<string>();
				foreach (string key2 in keys)
				{
					if (!item.Value.Contains(key2))
					{
						list.Add(key2);
					}
				}
				if (list.Count == 0)
				{
					stringBuilder.Append($"\nUnion of Type={item.Key} found with {cSVString3}");
					continue;
				}
				string cSVString4 = GetCSVString(list, "Key=", "Keys=");
				stringBuilder.Append($"\nUnion of Type={item.Key} found with {cSVString3}. Without {cSVString4}");
			}
			break;
		}
		case Addressables.MergeMode.Intersection:
		{
			stringBuilder = new StringBuilder($"{base.Message} No {MergeMode.Value} of Assets between {cSVString} with Type={Type}");
			bool flag = false;
			Dictionary<Type, List<string>> dictionary3 = new Dictionary<Type, List<string>>();
			foreach (string key3 in keys)
			{
				if (!GetTypeToKeys(key3, dictionary3))
				{
					flag = true;
					stringBuilder.Append(string.Format(format, key3));
				}
			}
			if (flag)
			{
				break;
			}
			foreach (KeyValuePair<Type, List<string>> item2 in dictionary3)
			{
				if (item2.Value.Count == keys.Count)
				{
					stringBuilder.Append($"\nAn Intersection exists for Type={item2.Key}");
				}
			}
			break;
		}
		case Addressables.MergeMode.None:
		{
			stringBuilder = new StringBuilder($"{base.Message} No {MergeMode.Value} Asset within {cSVString} with Type={Type}");
			Dictionary<Type, List<string>> dictionary = new Dictionary<Type, List<string>>();
			foreach (string key4 in keys)
			{
				if (!GetTypeToKeys(key4, dictionary))
				{
					stringBuilder.Append(string.Format(format, key4));
				}
			}
			foreach (KeyValuePair<Type, List<string>> item3 in dictionary)
			{
				string cSVString2 = GetCSVString(item3.Value, "Key=", "Keys=");
				stringBuilder.Append($"\nType={item3.Key} exists for {cSVString2}");
			}
			break;
		}
		}
		return stringBuilder.ToString();
	}

	private HashSet<Type> GetTypesForKey(string keyString)
	{
		HashSet<Type> hashSet = new HashSet<Type>();
		foreach (IResourceLocator resourceLocator in m_Addressables.ResourceLocators)
		{
			if (!resourceLocator.Locate(keyString, null, out var locations))
			{
				continue;
			}
			foreach (IResourceLocation item in locations)
			{
				hashSet.Add(item.ResourceType);
			}
		}
		return hashSet;
	}

	private bool GetTypeToKeys(string key, Dictionary<Type, List<string>> typeToKeys)
	{
		HashSet<Type> typesForKey = GetTypesForKey(key);
		if (typesForKey.Count == 0)
		{
			return false;
		}
		foreach (Type item in typesForKey)
		{
			if (!typeToKeys.TryGetValue(item, out var value))
			{
				typeToKeys.Add(item, new List<string> { key });
			}
			else
			{
				value.Add(key);
			}
		}
		return true;
	}

	private string GetCSVString(IEnumerable<string> enumerator, string prefixSingle, string prefixPlural)
	{
		StringBuilder stringBuilder = new StringBuilder(prefixPlural);
		int num = 0;
		foreach (string item in enumerator)
		{
			num++;
			stringBuilder.Append((num > 1) ? (", " + item) : item);
		}
		if (num == 1 && !string.IsNullOrEmpty(prefixPlural) && !string.IsNullOrEmpty(prefixSingle))
		{
			stringBuilder.Replace(prefixPlural, prefixSingle);
		}
		return stringBuilder.ToString();
	}
}
