using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.IO.Archive;

[RequiredByNativeCode]
[NativeHeader("Runtime/VirtualFileSystem/ArchiveFileSystem/ArchiveFileHandle.h")]
public struct ArchiveHandle
{
	internal ulong Handle;

	public ArchiveStatus Status
	{
		get
		{
			ThrowIfInvalid();
			return ArchiveFileInterface.Archive_GetStatus(this);
		}
	}

	public JobHandle JobHandle
	{
		get
		{
			ThrowIfInvalid();
			return ArchiveFileInterface.Archive_GetJobHandle(this);
		}
	}

	public CompressionType Compression
	{
		get
		{
			ThrowIfInvalid();
			return ArchiveFileInterface.Archive_GetCompression(this);
		}
	}

	public bool IsStreamed
	{
		get
		{
			ThrowIfInvalid();
			return ArchiveFileInterface.Archive_IsStreamed(this);
		}
	}

	public JobHandle Unmount()
	{
		ThrowIfInvalid();
		return ArchiveFileInterface.Archive_UnmountAsync(this);
	}

	private void ThrowIfInvalid()
	{
		if (!ArchiveFileInterface.Archive_IsValid(this))
		{
			throw new InvalidOperationException("The archive has already been unmounted.");
		}
	}

	public string GetMountPath()
	{
		ThrowIfInvalid();
		return ArchiveFileInterface.Archive_GetMountPath(this);
	}

	public ArchiveFileInfo[] GetFileInfo()
	{
		ThrowIfInvalid();
		return ArchiveFileInterface.Archive_GetFileInfo(this);
	}
}
