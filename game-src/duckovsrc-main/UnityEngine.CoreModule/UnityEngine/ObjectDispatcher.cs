using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[RequiredByNativeCode]
[StaticAccessor("GetObjectDispatcher()", StaticAccessorType.Dot)]
[NativeHeader("Runtime/Misc/ObjectDispatcher.h")]
internal sealed class ObjectDispatcher : IDisposable
{
	public enum TransformTrackingType
	{
		GlobalTRS,
		LocalTRS,
		Hierarchy
	}

	[Flags]
	public enum TypeTrackingFlags
	{
		SceneObjects = 1,
		Assets = 2,
		EditorOnlyObjects = 4,
		Default = 3,
		All = 7
	}

	private IntPtr m_Ptr = IntPtr.Zero;

	private unsafe static Action<Object[], IntPtr, IntPtr, int, int, Action<TypeDispatchData>> s_TypeDispatch = delegate(Object[] changed, IntPtr changedID, IntPtr destroyedID, int changedCount, int destroyedCount, Action<TypeDispatchData> callback)
	{
		NativeArray<int> changedID2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(changedID.ToPointer(), changedCount, Allocator.Invalid);
		NativeArray<int> destroyedID2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(destroyedID.ToPointer(), destroyedCount, Allocator.Invalid);
		TypeDispatchData obj = new TypeDispatchData
		{
			changed = changed,
			changedID = changedID2,
			destroyedID = destroyedID2
		};
		callback(obj);
	};

	private unsafe static Action<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, Action<TransformDispatchData>> s_TransformDispatch = delegate(IntPtr transformed, IntPtr parents, IntPtr localToWorldMatrices, IntPtr positions, IntPtr rotations, IntPtr scales, int count, Action<TransformDispatchData> callback)
	{
		NativeArray<int> transformedID = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(transformed.ToPointer(), count, Allocator.Invalid);
		NativeArray<int> parentID = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(parents.ToPointer(), (parents != IntPtr.Zero) ? count : 0, Allocator.Invalid);
		NativeArray<Matrix4x4> localToWorldMatrices2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Matrix4x4>(localToWorldMatrices.ToPointer(), (localToWorldMatrices != IntPtr.Zero) ? count : 0, Allocator.Invalid);
		NativeArray<Vector3> positions2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(positions.ToPointer(), (positions != IntPtr.Zero) ? count : 0, Allocator.Invalid);
		NativeArray<Quaternion> rotations2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Quaternion>(rotations.ToPointer(), (rotations != IntPtr.Zero) ? count : 0, Allocator.Invalid);
		NativeArray<Vector3> scales2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(scales.ToPointer(), (scales != IntPtr.Zero) ? count : 0, Allocator.Invalid);
		TransformDispatchData obj = new TransformDispatchData
		{
			transformedID = transformedID,
			parentID = parentID,
			localToWorldMatrices = localToWorldMatrices2,
			positions = positions2,
			rotations = rotations2,
			scales = scales2
		};
		callback(obj);
	};

	public bool valid => m_Ptr != IntPtr.Zero;

	public int maxDispatchHistoryFramesCount
	{
		get
		{
			ValidateSystemHandleAndThrow();
			return GetMaxDispatchHistoryFramesCount(m_Ptr);
		}
		set
		{
			ValidateSystemHandleAndThrow();
			SetMaxDispatchHistoryFramesCount(m_Ptr, value);
		}
	}

	public ObjectDispatcher()
	{
		m_Ptr = CreateDispatchSystemHandle();
	}

	~ObjectDispatcher()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (m_Ptr != IntPtr.Zero)
		{
			DestroyDispatchSystemHandle(m_Ptr);
			m_Ptr = IntPtr.Zero;
		}
	}

	private void ValidateSystemHandleAndThrow()
	{
		if (!valid)
		{
			throw new Exception("The ObjectDispatcher is invalid or has been disposed.");
		}
	}

	private void ValidateTypeAndThrow(Type type)
	{
		if (!type.IsSubclassOf(typeof(Object)))
		{
			throw new Exception("Only types inherited from UnityEngine.Object are supported.");
		}
	}

	private void ValidateComponentTypeAndThrow(Type type)
	{
		if (!type.IsSubclassOf(typeof(Component)))
		{
			throw new Exception("Only types inherited from UnityEngine.Component are supported.");
		}
	}

	public void DispatchTypeChangesAndClear(Type type, Action<TypeDispatchData> callback, bool sortByInstanceID = false, bool noScriptingArray = false)
	{
		ValidateSystemHandleAndThrow();
		ValidateTypeAndThrow(type);
		DispatchTypeChangesAndClear(m_Ptr, type, s_TypeDispatch, sortByInstanceID, noScriptingArray, callback);
	}

	public void DispatchTransformChangesAndClear(Type type, TransformTrackingType trackingType, Action<Component[]> callback, bool sortByInstanceID = false)
	{
		ValidateSystemHandleAndThrow();
		ValidateComponentTypeAndThrow(type);
		DispatchTransformChangesAndClear(m_Ptr, type, trackingType, callback, sortByInstanceID);
	}

	public void DispatchTransformChangesAndClear(Type type, TransformTrackingType trackingType, Action<TransformDispatchData> callback)
	{
		ValidateSystemHandleAndThrow();
		ValidateComponentTypeAndThrow(type);
		DispatchTransformDataChangesAndClear(m_Ptr, type, trackingType, s_TransformDispatch, callback);
	}

	public void ClearTypeChanges(Type type)
	{
		ValidateSystemHandleAndThrow();
		ValidateTypeAndThrow(type);
		DispatchTypeChangesAndClear(m_Ptr, type, null, sortByInstanceID: false, noScriptingArray: false, null);
	}

	public TypeDispatchData GetTypeChangesAndClear(Type type, Allocator allocator, bool sortByInstanceID = false, bool noScriptingArray = false)
	{
		TypeDispatchData dispatchData = default(TypeDispatchData);
		DispatchTypeChangesAndClear(type, delegate(TypeDispatchData data)
		{
			dispatchData.changed = data.changed;
			dispatchData.changedID = new NativeArray<int>(data.changedID, allocator);
			dispatchData.destroyedID = new NativeArray<int>(data.destroyedID, allocator);
		}, sortByInstanceID, noScriptingArray);
		return dispatchData;
	}

	public void GetTypeChangesAndClear(Type type, List<Object> changed, out NativeArray<int> changedID, out NativeArray<int> destroyedID, Allocator allocator, bool sortByInstanceID = false)
	{
		TypeDispatchData dispatchData = default(TypeDispatchData);
		DispatchTypeChangesAndClear(type, delegate(TypeDispatchData data)
		{
			dispatchData.changedID = new NativeArray<int>(data.changedID, allocator);
			dispatchData.destroyedID = new NativeArray<int>(data.destroyedID, allocator);
		}, sortByInstanceID, noScriptingArray: true);
		changedID = dispatchData.changedID;
		destroyedID = dispatchData.destroyedID;
		Resources.InstanceIDToObjectList(dispatchData.changedID, changed);
	}

	public Component[] GetTransformChangesAndClear(Type type, TransformTrackingType trackingType, bool sortByInstanceID = false)
	{
		Component[] dispatchData = null;
		DispatchTransformChangesAndClear(type, trackingType, delegate(Component[] instances)
		{
			dispatchData = instances;
		}, sortByInstanceID);
		return dispatchData;
	}

	public TransformDispatchData GetTransformChangesAndClear(Type type, TransformTrackingType trackingType, Allocator allocator)
	{
		TransformDispatchData dispatchData = default(TransformDispatchData);
		DispatchTransformChangesAndClear(type, trackingType, delegate(TransformDispatchData data)
		{
			dispatchData.transformedID = new NativeArray<int>(data.transformedID, allocator);
			dispatchData.parentID = new NativeArray<int>(data.parentID, allocator);
			dispatchData.localToWorldMatrices = new NativeArray<Matrix4x4>(data.localToWorldMatrices, allocator);
			dispatchData.positions = new NativeArray<Vector3>(data.positions, allocator);
			dispatchData.rotations = new NativeArray<Quaternion>(data.rotations, allocator);
			dispatchData.scales = new NativeArray<Vector3>(data.scales, allocator);
		});
		return dispatchData;
	}

	public void EnableTypeTracking(TypeTrackingFlags typeTrackingMask, params Type[] types)
	{
		ValidateSystemHandleAndThrow();
		foreach (Type type in types)
		{
			ValidateTypeAndThrow(type);
			EnableTypeTracking(m_Ptr, type, typeTrackingMask);
		}
	}

	public void EnableTypeTracking(params Type[] types)
	{
		EnableTypeTracking(TypeTrackingFlags.Default, types);
	}

	[Obsolete("EnableTypeTrackingIncludingAssets is deprecated, please use EnableTypeTracking and provide the flag that specifies whether you need assets or not.", false)]
	public void EnableTypeTrackingIncludingAssets(params Type[] types)
	{
		EnableTypeTracking(TypeTrackingFlags.Default, types);
	}

	public void DisableTypeTracking(params Type[] types)
	{
		ValidateSystemHandleAndThrow();
		foreach (Type type in types)
		{
			ValidateTypeAndThrow(type);
			DisableTypeTracking(m_Ptr, type);
		}
	}

	public void EnableTransformTracking(TransformTrackingType trackingType, params Type[] types)
	{
		ValidateSystemHandleAndThrow();
		foreach (Type type in types)
		{
			ValidateComponentTypeAndThrow(type);
			EnableTransformTracking(m_Ptr, type, trackingType);
		}
	}

	public void DisableTransformTracking(TransformTrackingType trackingType, params Type[] types)
	{
		ValidateSystemHandleAndThrow();
		foreach (Type type in types)
		{
			ValidateComponentTypeAndThrow(type);
			DisableTransformTracking(m_Ptr, type, trackingType);
		}
	}

	public void DispatchTypeChangesAndClear<T>(Action<TypeDispatchData> callback, bool sortByInstanceID = false, bool noScriptingArray = false) where T : Object
	{
		DispatchTypeChangesAndClear(typeof(T), callback, sortByInstanceID, noScriptingArray);
	}

	public void DispatchTransformChangesAndClear<T>(TransformTrackingType trackingType, Action<Component[]> callback, bool sortByInstanceID = false) where T : Object
	{
		DispatchTransformChangesAndClear(typeof(T), trackingType, callback, sortByInstanceID);
	}

	public void DispatchTransformChangesAndClear<T>(TransformTrackingType trackingType, Action<TransformDispatchData> callback) where T : Object
	{
		DispatchTransformChangesAndClear(typeof(T), trackingType, callback);
	}

	public void ClearTypeChanges<T>() where T : Object
	{
		ClearTypeChanges(typeof(T));
	}

	public TypeDispatchData GetTypeChangesAndClear<T>(Allocator allocator, bool sortByInstanceID = false, bool noScriptingArray = false) where T : Object
	{
		return GetTypeChangesAndClear(typeof(T), allocator, sortByInstanceID, noScriptingArray);
	}

	public void GetTypeChangesAndClear<T>(List<Object> changed, out NativeArray<int> changedID, out NativeArray<int> destroyedID, Allocator allocator, bool sortByInstanceID = false) where T : Object
	{
		GetTypeChangesAndClear(typeof(T), changed, out changedID, out destroyedID, allocator, sortByInstanceID);
	}

	public Component[] GetTransformChangesAndClear<T>(TransformTrackingType trackingType, bool sortByInstanceID = false) where T : Object
	{
		return GetTransformChangesAndClear(typeof(T), trackingType, sortByInstanceID);
	}

	public TransformDispatchData GetTransformChangesAndClear<T>(TransformTrackingType trackingType, Allocator allocator) where T : Object
	{
		return GetTransformChangesAndClear(typeof(T), trackingType, allocator);
	}

	public void EnableTypeTracking<T>(TypeTrackingFlags typeTrackingMask = TypeTrackingFlags.Default) where T : Object
	{
		EnableTypeTracking(typeTrackingMask, typeof(T));
	}

	public void DisableTypeTracking<T>() where T : Object
	{
		DisableTypeTracking(typeof(T));
	}

	public void EnableTransformTracking<T>(TransformTrackingType trackingType) where T : Object
	{
		EnableTransformTracking(trackingType, typeof(T));
	}

	public void DisableTransformTracking<T>(TransformTrackingType trackingType) where T : Object
	{
		DisableTransformTracking(trackingType, typeof(T));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr CreateDispatchSystemHandle();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[ThreadSafe]
	private static extern void DestroyDispatchSystemHandle(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int GetMaxDispatchHistoryFramesCount(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetMaxDispatchHistoryFramesCount(IntPtr ptr, int count);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void EnableTypeTracking(IntPtr ptr, Type type, TypeTrackingFlags typeTrackingMask);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void DisableTypeTracking(IntPtr ptr, Type type);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void EnableTransformTracking(IntPtr ptr, Type type, TransformTrackingType trackingType);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void DisableTransformTracking(IntPtr ptr, Type type, TransformTrackingType trackingType);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void DispatchTypeChangesAndClear(IntPtr ptr, Type type, Action<Object[], IntPtr, IntPtr, int, int, Action<TypeDispatchData>> callback, bool sortByInstanceID, bool noScriptingArray, Action<TypeDispatchData> param);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void DispatchTransformDataChangesAndClear(IntPtr ptr, Type type, TransformTrackingType trackingType, Action<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int, Action<TransformDispatchData>> callback, Action<TransformDispatchData> param);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void DispatchTransformChangesAndClear(IntPtr ptr, Type type, TransformTrackingType trackingType, Action<Component[]> callback, bool sortByInstanceID);
}
