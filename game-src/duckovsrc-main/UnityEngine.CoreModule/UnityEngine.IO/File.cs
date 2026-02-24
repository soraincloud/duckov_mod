using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.IO;

[NativeHeader("Runtime/VirtualFileSystem/VirtualFileSystem.h")]
[StaticAccessor("FileAccessor", StaticAccessorType.DoubleColon)]
[NativeConditional("ENABLE_PROFILER")]
internal static class File
{
	internal static ulong totalOpenCalls => GetTotalOpenCalls();

	internal static ulong totalCloseCalls => GetTotalCloseCalls();

	internal static ulong totalReadCalls => GetTotalReadCalls();

	internal static ulong totalWriteCalls => GetTotalWriteCalls();

	internal static ulong totalSeekCalls => GetTotalSeekCalls();

	internal static ulong totalZeroSeekCalls => GetTotalZeroSeekCalls();

	internal static ulong totalFilesOpened => GetTotalFilesOpened();

	internal static ulong totalFilesClosed => GetTotalFilesClosed();

	internal static ulong totalBytesRead => GetTotalBytesRead();

	internal static ulong totalBytesWritten => GetTotalBytesWritten();

	internal static bool recordZeroSeeks
	{
		get
		{
			return GetRecordZeroSeeks();
		}
		set
		{
			SetRecordZeroSeeks(value);
		}
	}

	internal static ThreadIORestrictionMode MainThreadIORestrictionMode
	{
		get
		{
			return GetMainThreadFileIORestriction();
		}
		set
		{
			SetMainThreadFileIORestriction(value);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void SetRecordZeroSeeks(bool enable);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool GetRecordZeroSeeks();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalOpenCalls();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalCloseCalls();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalReadCalls();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalWriteCalls();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalSeekCalls();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalZeroSeekCalls();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalFilesOpened();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalFilesClosed();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalBytesRead();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern ulong GetTotalBytesWritten();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetMainThreadFileIORestriction(ThreadIORestrictionMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern ThreadIORestrictionMode GetMainThreadFileIORestriction();
}
