using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.Profiling.LowLevel.Unsafe;

[StructLayout(LayoutKind.Explicit, Size = 8)]
[UsedByNativeCode]
public readonly struct ProfilerRecorderHandle
{
	private const ulong k_InvalidHandle = ulong.MaxValue;

	[FieldOffset(0)]
	internal readonly ulong handle;

	public bool Valid => handle != 0L && handle != ulong.MaxValue;

	internal ProfilerRecorderHandle(ulong handle)
	{
		this.handle = handle;
	}

	internal static ProfilerRecorderHandle Get(ProfilerMarker marker)
	{
		return new ProfilerRecorderHandle((ulong)marker.Handle.ToInt64());
	}

	internal static ProfilerRecorderHandle Get(ProfilerCategory category, string statName)
	{
		if (string.IsNullOrEmpty(statName))
		{
			throw new ArgumentException("String must be not null or empty", "statName");
		}
		return GetByName(category, statName);
	}

	public static ProfilerRecorderDescription GetDescription(ProfilerRecorderHandle handle)
	{
		if (!handle.Valid)
		{
			throw new ArgumentException("ProfilerRecorderHandle is not initialized or is not available", "handle");
		}
		return GetDescriptionInternal(handle);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true, ThrowsException = true)]
	public static extern void GetAvailable(List<ProfilerRecorderHandle> outRecorderHandleList);

	[NativeMethod(IsThreadSafe = true)]
	internal static ProfilerRecorderHandle GetByName(ProfilerCategory category, string name)
	{
		GetByName_Injected(ref category, name, out var ret);
		return ret;
	}

	[RequiredMember]
	[NativeMethod(IsThreadSafe = true)]
	internal unsafe static ProfilerRecorderHandle GetByName__Unmanaged(ProfilerCategory category, byte* name, int nameLen)
	{
		GetByName__Unmanaged_Injected(ref category, name, nameLen, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static ProfilerRecorderHandle GetByName(ProfilerCategory category, char* name, int nameLen)
	{
		return GetByName_Unsafe(category, name, nameLen);
	}

	[NativeMethod(IsThreadSafe = true)]
	private unsafe static ProfilerRecorderHandle GetByName_Unsafe(ProfilerCategory category, char* name, int nameLen)
	{
		GetByName_Unsafe_Injected(ref category, name, nameLen, out var ret);
		return ret;
	}

	[NativeMethod(IsThreadSafe = true)]
	private static ProfilerRecorderDescription GetDescriptionInternal(ProfilerRecorderHandle handle)
	{
		GetDescriptionInternal_Injected(ref handle, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetByName_Injected(ref ProfilerCategory category, string name, out ProfilerRecorderHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void GetByName__Unmanaged_Injected(ref ProfilerCategory category, byte* name, int nameLen, out ProfilerRecorderHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void GetByName_Unsafe_Injected(ref ProfilerCategory category, char* name, int nameLen, out ProfilerRecorderHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetDescriptionInternal_Injected(ref ProfilerRecorderHandle handle, out ProfilerRecorderDescription ret);
}
