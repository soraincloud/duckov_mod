using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Diagnostics;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets.Utility;

internal class ResourceManagerDiagnostics : IDisposable
{
	private ResourceManager m_ResourceManager;

	private const int k_NumberOfCompletedOpResultEntriesToShow = 4;

	private const int k_MaximumCompletedOpResultEntryLength = 30;

	private Dictionary<int, DiagnosticInfo> m_cachedDiagnosticInfo = new Dictionary<int, DiagnosticInfo>();

	public ResourceManagerDiagnostics(ResourceManager resourceManager)
	{
		resourceManager.RegisterDiagnosticCallback(OnResourceManagerDiagnosticEvent);
		m_ResourceManager = resourceManager;
	}

	internal int SumDependencyNameHashCodes(AsyncOperationHandle handle)
	{
		List<AsyncOperationHandle> list = new List<AsyncOperationHandle>();
		handle.GetDependencies(list);
		int num = 0;
		foreach (AsyncOperationHandle item in list)
		{
			if (item.IsValid())
			{
				num += item.DebugName.GetHashCode() + SumDependencyNameHashCodes(item);
			}
		}
		return num;
	}

	internal int CalculateHashCode(AsyncOperationHandle handle)
	{
		if (handle.DebugName.Contains("CompletedOperation"))
		{
			return CalculateCompletedOperationHashcode(handle);
		}
		int num = SumDependencyNameHashCodes(handle);
		if (handle.DebugName.Contains("result=") && handle.DebugName.Contains("status="))
		{
			return handle.GetHashCode();
		}
		return handle.DebugName.GetHashCode() + num;
	}

	internal int CalculateCompletedOperationHashcode(AsyncOperationHandle handle)
	{
		if (handle.Result == null)
		{
			return handle.GetHashCode();
		}
		return handle.Result.GetHashCode() + handle.Result.GetType().GetHashCode();
	}

	internal string GenerateCompletedOperationDisplayName(AsyncOperationHandle handle)
	{
		if (handle.Result == null)
		{
			return handle.DebugName;
		}
		if (handle.Result.GetType().IsGenericType && handle.Result is IList list)
		{
			string text = handle.DebugName;
			if (list.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder("[");
				for (int i = 0; i < list.Count && i < 4; i++)
				{
					object obj = list[i];
					if (30 <= obj.ToString().Length)
					{
						stringBuilder.Append(obj.ToString().Substring(0, 30));
						stringBuilder.Append("..., ");
					}
					else
					{
						stringBuilder.Append(obj.ToString().Substring(0, obj.ToString().Length));
						stringBuilder.Append(", ");
					}
				}
				stringBuilder.Remove(stringBuilder.Length - 2, 2);
				stringBuilder.Append("]");
				text = stringBuilder.ToString();
			}
			return handle.DebugName + " Result type: List, result: " + text;
		}
		return handle.DebugName + " Result type: " + handle.Result.GetType();
	}

	private void OnResourceManagerDiagnosticEvent(ResourceManager.DiagnosticEventContext eventContext)
	{
		int num = CalculateHashCode(eventContext.OperationHandle);
		DiagnosticInfo value = null;
		if (eventContext.Type == ResourceManager.DiagnosticEventType.AsyncOperationDestroy)
		{
			if (m_cachedDiagnosticInfo.TryGetValue(num, out value))
			{
				m_cachedDiagnosticInfo.Remove(num);
			}
		}
		else if (!m_cachedDiagnosticInfo.TryGetValue(num, out value))
		{
			List<AsyncOperationHandle> list = new List<AsyncOperationHandle>();
			eventContext.OperationHandle.GetDependencies(list);
			int[] array = new int[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = CalculateHashCode(list[i]);
			}
			if (eventContext.OperationHandle.DebugName.Contains("CompletedOperation"))
			{
				string displayName = GenerateCompletedOperationDisplayName(eventContext.OperationHandle);
				Dictionary<int, DiagnosticInfo> cachedDiagnosticInfo = m_cachedDiagnosticInfo;
				DiagnosticInfo obj = new DiagnosticInfo
				{
					ObjectId = num,
					DisplayName = displayName,
					Dependencies = array
				};
				value = obj;
				cachedDiagnosticInfo.Add(num, obj);
			}
			else
			{
				Dictionary<int, DiagnosticInfo> cachedDiagnosticInfo2 = m_cachedDiagnosticInfo;
				DiagnosticInfo obj2 = new DiagnosticInfo
				{
					ObjectId = num,
					DisplayName = eventContext.OperationHandle.DebugName,
					Dependencies = array
				};
				value = obj2;
				cachedDiagnosticInfo2.Add(num, obj2);
			}
		}
		if (value != null)
		{
			ComponentSingleton<DiagnosticEventCollectorSingleton>.Instance.PostEvent(value.CreateEvent("ResourceManager", eventContext.Type, Time.frameCount, eventContext.EventValue));
		}
	}

	public void Dispose()
	{
		m_ResourceManager?.UnregisterDiagnosticCallback(OnResourceManagerDiagnosticEvent);
		if (ComponentSingleton<DiagnosticEventCollectorSingleton>.Exists)
		{
			ComponentSingleton<DiagnosticEventCollectorSingleton>.DestroySingleton();
		}
	}
}
