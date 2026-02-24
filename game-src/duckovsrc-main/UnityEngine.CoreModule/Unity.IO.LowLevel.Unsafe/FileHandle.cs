using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Bindings;

namespace Unity.IO.LowLevel.Unsafe;

public readonly struct FileHandle
{
	[NativeDisableUnsafePtrRestriction]
	internal readonly IntPtr fileCommandPtr;

	internal readonly int version;

	public FileStatus Status
	{
		get
		{
			if (!IsFileHandleValid(in this))
			{
				throw new InvalidOperationException("FileHandle.Status cannot be called on a closed FileHandle");
			}
			return GetFileStatus_Internal(in this);
		}
	}

	public JobHandle JobHandle
	{
		get
		{
			if (!IsFileHandleValid(in this))
			{
				throw new InvalidOperationException("FileHandle.JobHandle cannot be called on a closed FileHandle");
			}
			return GetJobHandle_Internal(in this);
		}
	}

	public bool IsValid()
	{
		return IsFileHandleValid(in this);
	}

	public JobHandle Close(JobHandle dependency = default(JobHandle))
	{
		if (!IsFileHandleValid(in this))
		{
			throw new InvalidOperationException("FileHandle.Close cannot be called twice on the same FileHandle");
		}
		return AsyncReadManager.CloseFileAsync(in this, dependency);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("AsyncReadManagerManaged::IsFileHandleValid")]
	private static extern bool IsFileHandleValid(in FileHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("AsyncReadManagerManaged::GetFileStatusFromManagedHandle")]
	private static extern FileStatus GetFileStatus_Internal(in FileHandle handle);

	[FreeFunction("AsyncReadManagerManaged::GetJobFenceFromManagedHandle")]
	private static JobHandle GetJobHandle_Internal(in FileHandle handle)
	{
		GetJobHandle_Internal_Injected(in handle, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetJobHandle_Internal_Injected(in FileHandle handle, out JobHandle ret);
}
