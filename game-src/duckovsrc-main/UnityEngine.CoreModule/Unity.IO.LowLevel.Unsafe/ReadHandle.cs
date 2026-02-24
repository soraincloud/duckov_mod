using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Bindings;

namespace Unity.IO.LowLevel.Unsafe;

public struct ReadHandle : IDisposable
{
	[NativeDisableUnsafePtrRestriction]
	internal IntPtr ptr;

	internal int version;

	public JobHandle JobHandle
	{
		get
		{
			if (!IsReadHandleValid(this))
			{
				throw new InvalidOperationException("ReadHandle.JobHandle cannot be called after the ReadHandle has been disposed");
			}
			return GetJobHandle(this);
		}
	}

	public ReadStatus Status
	{
		get
		{
			if (!IsReadHandleValid(this))
			{
				throw new InvalidOperationException("Cannot use a ReadHandle that has been disposed");
			}
			return GetReadStatus(this);
		}
	}

	public long ReadCount
	{
		get
		{
			if (!IsReadHandleValid(this))
			{
				throw new InvalidOperationException("Cannot use a ReadHandle that has been disposed");
			}
			return GetReadCount(this);
		}
	}

	public bool IsValid()
	{
		return IsReadHandleValid(this);
	}

	public void Dispose()
	{
		if (!IsReadHandleValid(this))
		{
			throw new InvalidOperationException("ReadHandle.Dispose cannot be called twice on the same ReadHandle");
		}
		if (Status == ReadStatus.InProgress)
		{
			throw new InvalidOperationException("ReadHandle.Dispose cannot be called until the read operation completes");
		}
		ReleaseReadHandle(this);
	}

	public void Cancel()
	{
		if (!IsReadHandleValid(this))
		{
			throw new InvalidOperationException("ReadHandle.Cancel cannot be called on a disposed ReadHandle");
		}
		CancelInternal(this);
	}

	[FreeFunction("AsyncReadManagerManaged::CancelReadRequest")]
	private static void CancelInternal(ReadHandle handle)
	{
		CancelInternal_Injected(ref handle);
	}

	public long GetBytesRead()
	{
		if (!IsReadHandleValid(this))
		{
			throw new InvalidOperationException("ReadHandle.GetBytesRead cannot be called after the ReadHandle has been disposed");
		}
		return GetBytesRead(this);
	}

	public long GetBytesRead(uint readCommandIndex)
	{
		if (!IsReadHandleValid(this))
		{
			throw new InvalidOperationException("ReadHandle.GetBytesRead cannot be called after the ReadHandle has been disposed");
		}
		return GetBytesReadForCommand(this, readCommandIndex);
	}

	public unsafe ulong* GetBytesReadArray()
	{
		if (!IsReadHandleValid(this))
		{
			throw new InvalidOperationException("ReadHandle.GetBytesReadArray cannot be called after the ReadHandle has been disposed");
		}
		return GetBytesReadArray(this);
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::GetReadStatus", IsThreadSafe = true)]
	private static ReadStatus GetReadStatus(ReadHandle handle)
	{
		return GetReadStatus_Injected(ref handle);
	}

	[FreeFunction("AsyncReadManagerManaged::GetReadCount", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	private static long GetReadCount(ReadHandle handle)
	{
		return GetReadCount_Injected(ref handle);
	}

	[FreeFunction("AsyncReadManagerManaged::GetBytesRead", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	private static long GetBytesRead(ReadHandle handle)
	{
		return GetBytesRead_Injected(ref handle);
	}

	[FreeFunction("AsyncReadManagerManaged::GetBytesReadForCommand", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	private static long GetBytesReadForCommand(ReadHandle handle, uint readCommandIndex)
	{
		return GetBytesReadForCommand_Injected(ref handle, readCommandIndex);
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::GetBytesReadArray", IsThreadSafe = true)]
	private unsafe static ulong* GetBytesReadArray(ReadHandle handle)
	{
		return GetBytesReadArray_Injected(ref handle);
	}

	[ThreadAndSerializationSafe]
	[FreeFunction("AsyncReadManagerManaged::ReleaseReadHandle", IsThreadSafe = true)]
	private static void ReleaseReadHandle(ReadHandle handle)
	{
		ReleaseReadHandle_Injected(ref handle);
	}

	[FreeFunction("AsyncReadManagerManaged::IsReadHandleValid", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	private static bool IsReadHandleValid(ReadHandle handle)
	{
		return IsReadHandleValid_Injected(ref handle);
	}

	[FreeFunction("AsyncReadManagerManaged::GetJobHandle", IsThreadSafe = true)]
	[ThreadAndSerializationSafe]
	private static JobHandle GetJobHandle(ReadHandle handle)
	{
		GetJobHandle_Injected(ref handle, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void CancelInternal_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern ReadStatus GetReadStatus_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern long GetReadCount_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern long GetBytesRead_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern long GetBytesReadForCommand_Injected(ref ReadHandle handle, uint readCommandIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern ulong* GetBytesReadArray_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ReleaseReadHandle_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool IsReadHandleValid_Injected(ref ReadHandle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetJobHandle_Injected(ref ReadHandle handle, out JobHandle ret);
}
