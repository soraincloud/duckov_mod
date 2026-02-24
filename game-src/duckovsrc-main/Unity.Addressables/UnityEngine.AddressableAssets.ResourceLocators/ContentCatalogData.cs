using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.AddressableAssets.Utility;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;

namespace UnityEngine.AddressableAssets.ResourceLocators;

[Serializable]
public class ContentCatalogData
{
	private struct Bucket
	{
		public int dataOffset;

		public int[] entries;
	}

	private class CompactLocation : IResourceLocation
	{
		private ResourceLocationMap m_Locator;

		private string m_InternalId;

		private string m_ProviderId;

		private object m_Dependency;

		private object m_Data;

		private int m_HashCode;

		private int m_DependencyHashCode;

		private string m_PrimaryKey;

		private Type m_Type;

		public string InternalId => m_InternalId;

		public string ProviderId => m_ProviderId;

		public IList<IResourceLocation> Dependencies
		{
			get
			{
				if (m_Dependency == null)
				{
					return null;
				}
				m_Locator.Locate(m_Dependency, typeof(object), out var locations);
				return locations;
			}
		}

		public bool HasDependencies => m_Dependency != null;

		public int DependencyHashCode => m_DependencyHashCode;

		public object Data => m_Data;

		public string PrimaryKey
		{
			get
			{
				return m_PrimaryKey;
			}
			set
			{
				m_PrimaryKey = value;
			}
		}

		public Type ResourceType => m_Type;

		public override string ToString()
		{
			return m_InternalId;
		}

		public int Hash(Type t)
		{
			return (m_HashCode * 31 + t.GetHashCode()) * 31 + DependencyHashCode;
		}

		public CompactLocation(ResourceLocationMap locator, string internalId, string providerId, object dependencyKey, object data, int depHash, string primaryKey, Type type)
		{
			m_Locator = locator;
			m_InternalId = internalId;
			m_ProviderId = providerId;
			m_Dependency = dependencyKey;
			m_Data = data;
			m_HashCode = internalId.GetHashCode() * 31 + providerId.GetHashCode();
			m_DependencyHashCode = depHash;
			m_PrimaryKey = primaryKey;
			m_Type = ((type == null) ? typeof(object) : type);
		}
	}

	private static int kMagic = "ContentCatalogData".GetHashCode();

	private const int kVersion = 1;

	[NonSerialized]
	public string LocalHash;

	[NonSerialized]
	internal IResourceLocation location;

	[SerializeField]
	internal string m_LocatorId;

	[SerializeField]
	internal string m_BuildResultHash;

	[SerializeField]
	private ObjectInitializationData m_InstanceProviderData;

	[SerializeField]
	private ObjectInitializationData m_SceneProviderData;

	[SerializeField]
	internal List<ObjectInitializationData> m_ResourceProviderData = new List<ObjectInitializationData>();

	private IList<ContentCatalogDataEntry> m_Entries;

	[FormerlySerializedAs("m_providerIds")]
	[SerializeField]
	internal string[] m_ProviderIds;

	[FormerlySerializedAs("m_internalIds")]
	[SerializeField]
	internal string[] m_InternalIds;

	[FormerlySerializedAs("m_keyDataString")]
	[SerializeField]
	internal string m_KeyDataString;

	[FormerlySerializedAs("m_bucketDataString")]
	[SerializeField]
	internal string m_BucketDataString;

	[FormerlySerializedAs("m_entryDataString")]
	[SerializeField]
	internal string m_EntryDataString;

	private const int kBytesPerInt32 = 4;

	private const int k_EntryDataItemPerEntry = 7;

	[FormerlySerializedAs("m_extraDataString")]
	[SerializeField]
	internal string m_ExtraDataString;

	[SerializeField]
	internal SerializedType[] m_resourceTypes;

	[SerializeField]
	private string[] m_InternalIdPrefixes;

	public string BuildResultHash
	{
		get
		{
			return m_BuildResultHash;
		}
		set
		{
			m_BuildResultHash = value;
		}
	}

	public string ProviderId
	{
		get
		{
			return m_LocatorId;
		}
		internal set
		{
			m_LocatorId = value;
		}
	}

	public ObjectInitializationData InstanceProviderData
	{
		get
		{
			return m_InstanceProviderData;
		}
		set
		{
			m_InstanceProviderData = value;
		}
	}

	public ObjectInitializationData SceneProviderData
	{
		get
		{
			return m_SceneProviderData;
		}
		set
		{
			m_SceneProviderData = value;
		}
	}

	public List<ObjectInitializationData> ResourceProviderData
	{
		get
		{
			return m_ResourceProviderData;
		}
		set
		{
			m_ResourceProviderData = value;
		}
	}

	public string[] ProviderIds => m_ProviderIds;

	public string[] InternalIds => m_InternalIds;

	public ContentCatalogData(string id)
	{
		m_LocatorId = id;
	}

	public ContentCatalogData()
	{
	}

	internal static ContentCatalogData LoadFromFile(string path, int cacheSize = 1024)
	{
		return JsonUtility.FromJson<ContentCatalogData>(File.ReadAllText(path));
	}

	internal void SaveToFile(string path)
	{
		File.WriteAllText(path, JsonUtility.ToJson(this));
	}

	internal void CleanData()
	{
		m_KeyDataString = "";
		m_BucketDataString = "";
		m_EntryDataString = "";
		m_ExtraDataString = "";
		m_InternalIds = null;
		m_LocatorId = "";
		m_ProviderIds = null;
		m_ResourceProviderData = null;
		m_resourceTypes = null;
	}

	internal ResourceLocationMap CreateCustomLocator(string overrideId = "", string providerSuffix = null)
	{
		m_LocatorId = overrideId;
		return CreateLocator(providerSuffix);
	}

	public ResourceLocationMap CreateLocator(string providerSuffix = null)
	{
		byte[] array = Convert.FromBase64String(m_BucketDataString);
		int num = BitConverter.ToInt32(array, 0);
		Bucket[] array2 = new Bucket[num];
		int num2 = 4;
		for (int i = 0; i < num; i++)
		{
			int dataOffset = SerializationUtilities.ReadInt32FromByteArray(array, num2);
			num2 += 4;
			int num3 = SerializationUtilities.ReadInt32FromByteArray(array, num2);
			num2 += 4;
			int[] array3 = new int[num3];
			for (int j = 0; j < num3; j++)
			{
				array3[j] = SerializationUtilities.ReadInt32FromByteArray(array, num2);
				num2 += 4;
			}
			array2[i] = new Bucket
			{
				entries = array3,
				dataOffset = dataOffset
			};
		}
		if (!string.IsNullOrEmpty(providerSuffix))
		{
			for (int k = 0; k < m_ProviderIds.Length; k++)
			{
				if (!m_ProviderIds[k].EndsWith(providerSuffix, StringComparison.Ordinal))
				{
					m_ProviderIds[k] += providerSuffix;
				}
			}
		}
		byte[] keyData = Convert.FromBase64String(m_ExtraDataString);
		byte[] array4 = Convert.FromBase64String(m_KeyDataString);
		object[] array5 = new object[BitConverter.ToInt32(array4, 0)];
		for (int l = 0; l < array2.Length; l++)
		{
			array5[l] = SerializationUtilities.ReadObjectFromByteArray(array4, array2[l].dataOffset);
		}
		ResourceLocationMap resourceLocationMap = new ResourceLocationMap(m_LocatorId, array2.Length);
		byte[] data = Convert.FromBase64String(m_EntryDataString);
		int num4 = SerializationUtilities.ReadInt32FromByteArray(data, 0);
		IResourceLocation[] array6 = new IResourceLocation[num4];
		for (int m = 0; m < num4; m++)
		{
			int num5 = 4 + m * 28;
			int num6 = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			num5 += 4;
			int num7 = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			num5 += 4;
			int num8 = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			num5 += 4;
			int depHash = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			num5 += 4;
			int num9 = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			num5 += 4;
			int num10 = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			num5 += 4;
			int num11 = SerializationUtilities.ReadInt32FromByteArray(data, num5);
			object data2 = ((num9 < 0) ? null : SerializationUtilities.ReadObjectFromByteArray(keyData, num9));
			array6[m] = new CompactLocation(resourceLocationMap, Addressables.ResolveInternalId(ExpandInternalId(m_InternalIdPrefixes, m_InternalIds[num6])), m_ProviderIds[num7], (num8 < 0) ? null : array5[num8], data2, depHash, array5[num10].ToString(), m_resourceTypes[num11].Value);
		}
		for (int n = 0; n < array2.Length; n++)
		{
			Bucket bucket = array2[n];
			object key = array5[n];
			IResourceLocation[] array7 = new IResourceLocation[bucket.entries.Length];
			for (int num12 = 0; num12 < bucket.entries.Length; num12++)
			{
				array7[num12] = array6[bucket.entries[num12]];
			}
			resourceLocationMap.Add(key, array7);
		}
		return resourceLocationMap;
	}

	internal IList<ContentCatalogDataEntry> GetData()
	{
		ResourceLocationMap resourceLocationMap = CreateLocator();
		List<ContentCatalogDataEntry> list = new List<ContentCatalogDataEntry>();
		Dictionary<IResourceLocation, List<object>> dictionary = new Dictionary<IResourceLocation, List<object>>();
		foreach (object key in resourceLocationMap.Keys)
		{
			resourceLocationMap.Locate(key, null, out var locations);
			foreach (IResourceLocation item in locations)
			{
				if (!dictionary.TryGetValue(item, out var value))
				{
					dictionary.Add(item, value = new List<object>());
				}
				value.Add(key.ToString());
			}
		}
		foreach (KeyValuePair<IResourceLocation, List<object>> item2 in dictionary)
		{
			list.Add(new ContentCatalogDataEntry(item2.Key.ResourceType, item2.Key.InternalId, item2.Key.ProviderId, item2.Value, (item2.Key.Dependencies == null) ? null : item2.Key.Dependencies.Select((IResourceLocation d) => d.PrimaryKey).ToList(), item2.Key.Data));
		}
		return list;
	}

	internal static string ExpandInternalId(string[] internalIdPrefixes, string v)
	{
		if (internalIdPrefixes == null || internalIdPrefixes.Length == 0)
		{
			return v;
		}
		int num = v.LastIndexOf('#');
		if (num < 0)
		{
			return v;
		}
		int result = 0;
		if (!int.TryParse(v.Substring(0, num), out result))
		{
			return v;
		}
		return internalIdPrefixes[result] + v.Substring(num + 1);
	}
}
