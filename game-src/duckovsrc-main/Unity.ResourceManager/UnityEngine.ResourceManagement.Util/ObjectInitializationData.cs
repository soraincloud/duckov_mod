using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace UnityEngine.ResourceManagement.Util;

[Serializable]
public struct ObjectInitializationData
{
	[FormerlySerializedAs("m_id")]
	[SerializeField]
	private string m_Id;

	[FormerlySerializedAs("m_objectType")]
	[SerializeField]
	private SerializedType m_ObjectType;

	[FormerlySerializedAs("m_data")]
	[SerializeField]
	private string m_Data;

	public string Id => m_Id;

	public SerializedType ObjectType => m_ObjectType;

	public string Data => m_Data;

	public override string ToString()
	{
		return $"ObjectInitializationData: id={m_Id}, type={m_ObjectType}";
	}

	public TObject CreateInstance<TObject>(string idOverride = null)
	{
		try
		{
			Type value = m_ObjectType.Value;
			if (value == null)
			{
				return default(TObject);
			}
			object obj = Activator.CreateInstance(value, nonPublic: true);
			if (obj is IInitializableObject initializableObject && !initializableObject.Initialize((idOverride == null) ? m_Id : idOverride, m_Data))
			{
				return default(TObject);
			}
			return (TObject)obj;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return default(TObject);
		}
	}

	public AsyncOperationHandle GetAsyncInitHandle(ResourceManager rm, string idOverride = null)
	{
		try
		{
			Type value = m_ObjectType.Value;
			if (value == null)
			{
				return default(AsyncOperationHandle);
			}
			if (Activator.CreateInstance(value, nonPublic: true) is IInitializableObject initializableObject)
			{
				return initializableObject.InitializeAsync(rm, (idOverride == null) ? m_Id : idOverride, m_Data);
			}
			return default(AsyncOperationHandle);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return default(AsyncOperationHandle);
		}
	}
}
