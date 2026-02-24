using System.Runtime.CompilerServices;
using Unity.Content;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.IO.Archive;

[NativeHeader("Runtime/VirtualFileSystem/ArchiveFileSystem/ArchiveFileHandle.h")]
[RequiredByNativeCode]
[StaticAccessor("GetManagedArchiveSystem()", StaticAccessorType.Dot)]
public static class ArchiveFileInterface
{
	public static ArchiveHandle MountAsync(ContentNamespace namespaceId, string filePath, string prefix)
	{
		MountAsync_Injected(ref namespaceId, filePath, prefix, out var ret);
		return ret;
	}

	public static ArchiveHandle[] GetMountedArchives(ContentNamespace namespaceId)
	{
		return GetMountedArchives_Injected(ref namespaceId);
	}

	internal static ArchiveStatus Archive_GetStatus(ArchiveHandle handle)
	{
		return Archive_GetStatus_Injected(ref handle);
	}

	internal static JobHandle Archive_GetJobHandle(ArchiveHandle handle)
	{
		Archive_GetJobHandle_Injected(ref handle, out var ret);
		return ret;
	}

	internal static bool Archive_IsValid(ArchiveHandle handle)
	{
		return Archive_IsValid_Injected(ref handle);
	}

	internal static JobHandle Archive_UnmountAsync(ArchiveHandle handle)
	{
		Archive_UnmountAsync_Injected(ref handle, out var ret);
		return ret;
	}

	internal static string Archive_GetMountPath(ArchiveHandle handle)
	{
		return Archive_GetMountPath_Injected(ref handle);
	}

	internal static CompressionType Archive_GetCompression(ArchiveHandle handle)
	{
		return Archive_GetCompression_Injected(ref handle);
	}

	internal static bool Archive_IsStreamed(ArchiveHandle handle)
	{
		return Archive_IsStreamed_Injected(ref handle);
	}

	internal static ArchiveFileInfo[] Archive_GetFileInfo(ArchiveHandle handle)
	{
		return Archive_GetFileInfo_Injected(ref handle);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void MountAsync_Injected(ref ContentNamespace namespaceId, string filePath, string prefix, out ArchiveHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern ArchiveHandle[] GetMountedArchives_Injected(ref ContentNamespace namespaceId);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern ArchiveStatus Archive_GetStatus_Injected(ref ArchiveHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Archive_GetJobHandle_Injected(ref ArchiveHandle handle, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Archive_IsValid_Injected(ref ArchiveHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Archive_UnmountAsync_Injected(ref ArchiveHandle handle, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string Archive_GetMountPath_Injected(ref ArchiveHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern CompressionType Archive_GetCompression_Injected(ref ArchiveHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Archive_IsStreamed_Injected(ref ArchiveHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern ArchiveFileInfo[] Archive_GetFileInfo_Injected(ref ArchiveHandle handle);
}
