using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Internal;

namespace UnityEngine;

[NativeHeader("Modules/ClusterRenderer/ClusterSerialization.h")]
[ExcludeFromDocs]
public static class ClusterSerialization
{
	public unsafe static int SaveTimeManagerState(NativeArray<byte> buffer)
	{
		return SaveTimeManagerStateInternal(buffer.GetUnsafePtr(), buffer.Length);
	}

	public unsafe static bool RestoreTimeManagerState(NativeArray<byte> buffer)
	{
		return RestoreTimeManagerStateInternal(buffer.GetUnsafePtr(), buffer.Length);
	}

	public unsafe static int SaveInputManagerState(NativeArray<byte> buffer)
	{
		return SaveInputManagerStateInternal(buffer.GetUnsafePtr(), buffer.Length);
	}

	public unsafe static bool RestoreInputManagerState(NativeArray<byte> buffer)
	{
		return RestoreInputManagerStateInternal(buffer.GetUnsafePtr(), buffer.Length);
	}

	public unsafe static int SaveClusterInputState(NativeArray<byte> buffer)
	{
		return SaveClusterInputStateInternal(buffer.GetUnsafePtr(), buffer.Length);
	}

	public unsafe static bool RestoreClusterInputState(NativeArray<byte> buffer)
	{
		return RestoreClusterInputStateInternal(buffer.GetUnsafePtr(), buffer.Length);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ClusterSerialization::SaveTimeManagerState")]
	private unsafe static extern int SaveTimeManagerStateInternal(void* intBuffer, int bufferSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ClusterSerialization::RestoreTimeManagerState")]
	private unsafe static extern bool RestoreTimeManagerStateInternal(void* buffer, int bufferSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ClusterSerialization::SaveInputManagerState")]
	private unsafe static extern int SaveInputManagerStateInternal(void* intBuffer, int bufferSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ClusterSerialization::RestoreInputManagerState")]
	private unsafe static extern bool RestoreInputManagerStateInternal(void* buffer, int bufferSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ClusterSerialization::SaveClusterInputState")]
	private unsafe static extern int SaveClusterInputStateInternal(void* intBuffer, int bufferSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ClusterSerialization::RestoreClusterInputState")]
	private unsafe static extern bool RestoreClusterInputStateInternal(void* buffer, int bufferSize);
}
