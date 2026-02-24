using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Bindings;

namespace Unity.IO.LowLevel.Unsafe;

[NativeHeader("Runtime/File/AsyncReadManagerManagedApi.h")]
public static class AsyncReadManager
{
	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::Read", IsThreadSafe = true)]
	private unsafe static ReadHandle ReadInternal(string filename, void* cmds, uint cmdCount, string assetName, ulong typeID, AssetLoadingSubsystem subsystem)
	{
		ReadInternal_Injected(filename, cmds, cmdCount, assetName, typeID, subsystem, out var ret);
		return ret;
	}

	public unsafe static ReadHandle Read(string filename, ReadCommand* readCmds, uint readCmdCount, string assetName = "", ulong typeID = 0uL, AssetLoadingSubsystem subsystem = AssetLoadingSubsystem.Scripts)
	{
		return ReadInternal(filename, readCmds, readCmdCount, assetName, typeID, subsystem);
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::GetFileInfo", IsThreadSafe = true)]
	private unsafe static ReadHandle GetFileInfoInternal(string filename, void* cmd)
	{
		GetFileInfoInternal_Injected(filename, cmd, out var ret);
		return ret;
	}

	public unsafe static ReadHandle GetFileInfo(string filename, FileInfoResult* result)
	{
		if (result == null)
		{
			throw new NullReferenceException("GetFileInfo must have a valid FileInfoResult to write into.");
		}
		return GetFileInfoInternal(filename, result);
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::ReadWithHandles_NativePtr", IsThreadSafe = true)]
	private unsafe static ReadHandle ReadWithHandlesInternal_NativePtr(in FileHandle fileHandle, void* readCmdArray, JobHandle dependency)
	{
		ReadWithHandlesInternal_NativePtr_Injected(in fileHandle, readCmdArray, ref dependency, out var ret);
		return ret;
	}

	[FreeFunction("AsyncReadManagerManaged::ReadWithHandles_NativeCopy", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	private unsafe static ReadHandle ReadWithHandlesInternal_NativeCopy(in FileHandle fileHandle, void* readCmdArray)
	{
		ReadWithHandlesInternal_NativeCopy_Injected(in fileHandle, readCmdArray, out var ret);
		return ret;
	}

	public unsafe static ReadHandle ReadDeferred(in FileHandle fileHandle, ReadCommandArray* readCmdArray, JobHandle dependency)
	{
		if (!fileHandle.IsValid())
		{
			throw new InvalidOperationException("FileHandle is invalid and may not be read from.");
		}
		return ReadWithHandlesInternal_NativePtr(in fileHandle, readCmdArray, dependency);
	}

	public unsafe static ReadHandle Read(in FileHandle fileHandle, ReadCommandArray readCmdArray)
	{
		if (!fileHandle.IsValid())
		{
			throw new InvalidOperationException("FileHandle is invalid and may not be read from.");
		}
		return ReadWithHandlesInternal_NativeCopy(in fileHandle, UnsafeUtility.AddressOf(ref readCmdArray));
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::ScheduleOpenRequest", IsThreadSafe = true)]
	private static FileHandle OpenFileAsync_Internal(string fileName)
	{
		OpenFileAsync_Internal_Injected(fileName, out var ret);
		return ret;
	}

	public static FileHandle OpenFileAsync(string fileName)
	{
		if (fileName.Length == 0)
		{
			throw new InvalidOperationException("FileName is empty");
		}
		return OpenFileAsync_Internal(fileName);
	}

	[FreeFunction("AsyncReadManagerManaged::ScheduleCloseRequest", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	internal static JobHandle CloseFileAsync(in FileHandle fileHandle, JobHandle dependency)
	{
		CloseFileAsync_Injected(in fileHandle, ref dependency, out var ret);
		return ret;
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::ScheduleCloseCachedFileRequest", IsThreadSafe = true)]
	public static JobHandle CloseCachedFileAsync(string fileName, JobHandle dependency = default(JobHandle))
	{
		CloseCachedFileAsync_Injected(fileName, ref dependency, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ReadInternal_Injected(string filename, void* cmds, uint cmdCount, string assetName, ulong typeID, AssetLoadingSubsystem subsystem, out ReadHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void GetFileInfoInternal_Injected(string filename, void* cmd, out ReadHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ReadWithHandlesInternal_NativePtr_Injected(in FileHandle fileHandle, void* readCmdArray, ref JobHandle dependency, out ReadHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ReadWithHandlesInternal_NativeCopy_Injected(in FileHandle fileHandle, void* readCmdArray, out ReadHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void OpenFileAsync_Internal_Injected(string fileName, out FileHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void CloseFileAsync_Injected(in FileHandle fileHandle, ref JobHandle dependency, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void CloseCachedFileAsync_Injected(string fileName, [Optional][DefaultParameterValue(null)] ref JobHandle dependency, out JobHandle ret);
}
