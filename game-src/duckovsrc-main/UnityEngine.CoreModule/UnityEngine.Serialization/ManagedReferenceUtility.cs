using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Serialization;

[NativeHeader("Runtime/Serialize/ManagedReferenceUtility.h")]
public sealed class ManagedReferenceUtility
{
	public const long RefIdUnknown = -1L;

	public const long RefIdNull = -2L;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("SetManagedReferenceIdForObject")]
	private static extern bool SetManagedReferenceIdForObjectInternal(Object obj, object scriptObj, long refId);

	public static bool SetManagedReferenceIdForObject(Object obj, object scriptObj, long refId)
	{
		if (scriptObj == null)
		{
			return refId == -2;
		}
		Type type = scriptObj.GetType();
		if (type == typeof(Object) || type.IsSubclassOf(typeof(Object)))
		{
			throw new InvalidOperationException("Cannot assign an object deriving from UnityEngine.Object to a managed reference. This is not supported.");
		}
		return SetManagedReferenceIdForObjectInternal(obj, scriptObj, refId);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetManagedReferenceIdForObject")]
	private static extern long GetManagedReferenceIdForObjectInternal(Object obj, object scriptObj);

	public static long GetManagedReferenceIdForObject(Object obj, object scriptObj)
	{
		return GetManagedReferenceIdForObjectInternal(obj, scriptObj);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetManagedReference")]
	private static extern object GetManagedReferenceInternal(Object obj, long id);

	public static object GetManagedReference(Object obj, long id)
	{
		return GetManagedReferenceInternal(obj, id);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetManagedReferenceIds")]
	private static extern long[] GetManagedReferenceIdsForObjectInternal(Object obj);

	public static long[] GetManagedReferenceIds(Object obj)
	{
		return GetManagedReferenceIdsForObjectInternal(obj);
	}
}
