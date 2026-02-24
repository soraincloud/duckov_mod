using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace Unity.Baselib.LowLevel;

[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_ErrorState.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_NetworkAddress.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_ErrorCode.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_Thread.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_DynamicLibrary.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_RegisteredNetwork.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_Memory.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_Timer.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_FileIO.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_SourceLocation.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_Socket.gen.binding.h")]
[NativeHeader("External/baselib/builds/CSharp/BindingsUnity/Baselib_ThreadLocalStorage.gen.binding.h")]
internal static class Binding
{
	public struct Baselib_DynamicLibrary_Handle
	{
		public IntPtr handle;
	}

	public enum Baselib_ErrorCode
	{
		Success = 0,
		OutOfMemory = 16777216,
		OutOfSystemResources = 16777217,
		InvalidAddressRange = 16777218,
		InvalidArgument = 16777219,
		InvalidBufferSize = 16777220,
		InvalidState = 16777221,
		NotSupported = 16777222,
		Timeout = 16777223,
		UnsupportedAlignment = 33554432,
		InvalidPageSize = 33554433,
		InvalidPageCount = 33554434,
		UnsupportedPageState = 33554435,
		ThreadCannotJoinSelf = 50331648,
		NetworkInitializationError = 67108864,
		AddressInUse = 67108865,
		AddressUnreachable = 67108866,
		AddressFamilyNotSupported = 67108867,
		Disconnected = 67108868,
		InvalidPathname = 83886080,
		RequestedAccessIsNotAllowed = 83886081,
		IOError = 83886082,
		FailedToOpenDynamicLibrary = 100663296,
		FunctionNotFound = 100663297,
		UnexpectedError = -1
	}

	public enum Baselib_ErrorState_NativeErrorCodeType : byte
	{
		None,
		PlatformDefined
	}

	public enum Baselib_ErrorState_ExtraInformationType : byte
	{
		None,
		StaticString,
		GenerationCounter
	}

	public struct Baselib_ErrorState
	{
		public Baselib_SourceLocation sourceLocation;

		public ulong nativeErrorCode;

		public ulong extraInformation;

		public Baselib_ErrorCode code;

		public Baselib_ErrorState_NativeErrorCodeType nativeErrorCodeType;

		public Baselib_ErrorState_ExtraInformationType extraInformationType;
	}

	public enum Baselib_ErrorState_ExplainVerbosity
	{
		ErrorType,
		ErrorType_SourceLocation_Explanation
	}

	public struct Baselib_FileIO_EventQueue
	{
		public IntPtr handle;
	}

	public struct Baselib_FileIO_AsyncFile
	{
		public IntPtr handle;
	}

	public struct Baselib_FileIO_SyncFile
	{
		public IntPtr handle;
	}

	public enum Baselib_FileIO_OpenFlags : uint
	{
		Read = 1u,
		Write = 2u,
		OpenAlways = 4u,
		CreateAlways = 8u
	}

	public struct Baselib_FileIO_ReadRequest
	{
		public ulong offset;

		public IntPtr buffer;

		public ulong size;
	}

	public enum Baselib_FileIO_Priority
	{
		Normal,
		High
	}

	public enum Baselib_FileIO_EventQueue_ResultType
	{
		Baselib_FileIO_EventQueue_Callback = 1,
		Baselib_FileIO_EventQueue_OpenFile,
		Baselib_FileIO_EventQueue_ReadFile,
		Baselib_FileIO_EventQueue_CloseFile
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void EventQueueCallback(ulong arg0);

	public struct Baselib_FileIO_EventQueue_Result_Callback
	{
		public IntPtr callback;
	}

	public struct Baselib_FileIO_EventQueue_Result_OpenFile
	{
		public ulong fileSize;
	}

	public struct Baselib_FileIO_EventQueue_Result_ReadFile
	{
		public ulong bytesTransferred;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Baselib_FileIO_EventQueue_Result
	{
		[FieldOffset(0)]
		public Baselib_FileIO_EventQueue_ResultType type;

		[FieldOffset(8)]
		public ulong userdata;

		[FieldOffset(16)]
		public Baselib_ErrorState errorState;

		[FieldOffset(64)]
		public Baselib_FileIO_EventQueue_Result_Callback callback;

		[FieldOffset(64)]
		[Ignore(DoesNotContributeToSize = true)]
		public Baselib_FileIO_EventQueue_Result_OpenFile openFile;

		[FieldOffset(64)]
		[Ignore(DoesNotContributeToSize = true)]
		public Baselib_FileIO_EventQueue_Result_ReadFile readFile;
	}

	public struct Baselib_Memory_PageSizeInfo
	{
		public ulong defaultPageSize;

		public ulong pageSizes0;

		public ulong pageSizes1;

		public ulong pageSizes2;

		public ulong pageSizes3;

		public ulong pageSizes4;

		public ulong pageSizes5;

		public ulong pageSizesLen;
	}

	public struct Baselib_Memory_PageAllocation
	{
		public IntPtr ptr;

		public ulong pageSize;

		public ulong pageCount;
	}

	public enum Baselib_Memory_PageState
	{
		Reserved = 0,
		NoAccess = 1,
		ReadOnly = 2,
		ReadWrite = 4,
		ReadOnly_Executable = 18,
		ReadWrite_Executable = 20
	}

	public enum Baselib_NetworkAddress_Family
	{
		Invalid,
		IPv4,
		IPv6
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Baselib_NetworkAddress
	{
		[FieldOffset(0)]
		public byte data0;

		[FieldOffset(1)]
		public byte data1;

		[FieldOffset(2)]
		public byte data2;

		[FieldOffset(3)]
		public byte data3;

		[FieldOffset(4)]
		public byte data4;

		[FieldOffset(5)]
		public byte data5;

		[FieldOffset(6)]
		public byte data6;

		[FieldOffset(7)]
		public byte data7;

		[FieldOffset(8)]
		public byte data8;

		[FieldOffset(9)]
		public byte data9;

		[FieldOffset(10)]
		public byte data10;

		[FieldOffset(11)]
		public byte data11;

		[FieldOffset(12)]
		public byte data12;

		[FieldOffset(13)]
		public byte data13;

		[FieldOffset(14)]
		public byte data14;

		[FieldOffset(15)]
		public byte data15;

		[FieldOffset(0)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_0;

		[FieldOffset(1)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_1;

		[FieldOffset(2)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_2;

		[FieldOffset(3)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_3;

		[FieldOffset(4)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_4;

		[FieldOffset(5)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_5;

		[FieldOffset(6)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_6;

		[FieldOffset(7)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_7;

		[FieldOffset(8)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_8;

		[FieldOffset(9)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_9;

		[FieldOffset(10)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_10;

		[FieldOffset(11)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_11;

		[FieldOffset(12)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_12;

		[FieldOffset(13)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_13;

		[FieldOffset(14)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_14;

		[FieldOffset(15)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv6_15;

		[FieldOffset(0)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv4_0;

		[FieldOffset(1)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv4_1;

		[FieldOffset(2)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv4_2;

		[FieldOffset(3)]
		[Ignore(DoesNotContributeToSize = true)]
		public byte ipv4_3;

		[FieldOffset(16)]
		public byte port0;

		[FieldOffset(17)]
		public byte port1;

		[FieldOffset(18)]
		public byte family;

		[FieldOffset(19)]
		public byte _padding;

		[FieldOffset(20)]
		public uint ipv6_scope_id;
	}

	public enum Baselib_NetworkAddress_AddressReuse
	{
		DoNotAllow,
		Allow
	}

	public struct Baselib_RegisteredNetwork_Buffer
	{
		public IntPtr id;

		public Baselib_Memory_PageAllocation allocation;
	}

	public struct Baselib_RegisteredNetwork_BufferSlice
	{
		public IntPtr id;

		public IntPtr data;

		public uint size;

		public uint offset;
	}

	public struct Baselib_RegisteredNetwork_Endpoint
	{
		public Baselib_RegisteredNetwork_BufferSlice slice;
	}

	public struct Baselib_RegisteredNetwork_Request
	{
		public Baselib_RegisteredNetwork_BufferSlice payload;

		public Baselib_RegisteredNetwork_Endpoint remoteEndpoint;

		public IntPtr requestUserdata;
	}

	public enum Baselib_RegisteredNetwork_CompletionStatus
	{
		Failed,
		Success
	}

	public struct Baselib_RegisteredNetwork_CompletionResult
	{
		public Baselib_RegisteredNetwork_CompletionStatus status;

		public uint bytesTransferred;

		public IntPtr requestUserdata;
	}

	public struct Baselib_RegisteredNetwork_Socket_UDP
	{
		public IntPtr handle;
	}

	public enum Baselib_RegisteredNetwork_ProcessStatus
	{
		NonePendingImmediately = 0,
		Done = 0,
		Pending = 1
	}

	public enum Baselib_RegisteredNetwork_CompletionQueueStatus
	{
		NoResultsAvailable,
		ResultsAvailable
	}

	public struct Baselib_Socket_Handle
	{
		public IntPtr handle;
	}

	public enum Baselib_Socket_Protocol
	{
		UDP = 1,
		TCP
	}

	public struct Baselib_Socket_Message
	{
		public unsafe Baselib_NetworkAddress* address;

		public IntPtr data;

		public uint dataLen;
	}

	public enum Baselib_Socket_PollEvents
	{
		Readable = 1,
		Writable = 2,
		Connected = 4
	}

	public struct Baselib_Socket_PollFd
	{
		public Baselib_Socket_Handle handle;

		public Baselib_Socket_PollEvents requestedEvents;

		public Baselib_Socket_PollEvents resultEvents;

		public unsafe Baselib_ErrorState* errorState;
	}

	public struct Baselib_SourceLocation
	{
		public unsafe byte* file;

		public unsafe byte* function;

		public uint lineNumber;
	}

	public struct Baselib_Timer_TickToNanosecondConversionRatio
	{
		public ulong ticksToNanosecondsNumerator;

		public ulong ticksToNanosecondsDenominator;
	}

	public static readonly UIntPtr Baselib_Memory_MaxAlignment = new UIntPtr(65536u);

	public static readonly UIntPtr Baselib_Memory_MinGuaranteedAlignment = new UIntPtr(8u);

	public const uint Baselib_NetworkAddress_IpMaxStringLength = 46u;

	public static readonly IntPtr Baselib_RegisteredNetwork_Buffer_Id_Invalid = IntPtr.Zero;

	public const uint Baselib_RegisteredNetwork_Endpoint_MaxSize = 28u;

	public static readonly IntPtr Baselib_Thread_InvalidId = IntPtr.Zero;

	public static readonly UIntPtr Baselib_Thread_MaxThreadNameLength = new UIntPtr(64u);

	public const uint Baselib_TLS_MinimumGuaranteedSlots = 100u;

	public const ulong Baselib_SecondsPerMinute = 60uL;

	public const ulong Baselib_MillisecondsPerSecond = 1000uL;

	public const ulong Baselib_MillisecondsPerMinute = 60000uL;

	public const ulong Baselib_MicrosecondsPerMillisecond = 1000uL;

	public const ulong Baselib_MicrosecondsPerSecond = 1000000uL;

	public const ulong Baselib_MicrosecondsPerMinute = 60000000uL;

	public const ulong Baselib_NanosecondsPerMicrosecond = 1000uL;

	public const ulong Baselib_NanosecondsPerMillisecond = 1000000uL;

	public const ulong Baselib_NanosecondsPerSecond = 1000000000uL;

	public const ulong Baselib_NanosecondsPerMinute = 60000000000uL;

	public const ulong Baselib_Timer_MaxNumberOfNanosecondsPerTick = 1000uL;

	public const double Baselib_Timer_MinNumberOfNanosecondsPerTick = 0.01;

	public const double Baselib_Timer_HighPrecisionTimerCrossThreadMontotonyTolerance_InNanoseconds = 100.0;

	public static readonly Baselib_Memory_PageAllocation Baselib_Memory_PageAllocation_Invalid = default(Baselib_Memory_PageAllocation);

	public static readonly Baselib_RegisteredNetwork_Socket_UDP Baselib_RegisteredNetwork_Socket_UDP_Invalid = default(Baselib_RegisteredNetwork_Socket_UDP);

	public static readonly Baselib_Socket_Handle Baselib_Socket_Handle_Invalid = new Baselib_Socket_Handle
	{
		handle = (IntPtr)(-1)
	};

	public static readonly Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_Handle_Invalid = new Baselib_DynamicLibrary_Handle
	{
		handle = (IntPtr)(-1)
	};

	public static readonly Baselib_FileIO_EventQueue Baselib_FileIO_EventQueue_Invalid = new Baselib_FileIO_EventQueue
	{
		handle = (IntPtr)0
	};

	public static readonly Baselib_FileIO_AsyncFile Baselib_FileIO_AsyncFile_Invalid = new Baselib_FileIO_AsyncFile
	{
		handle = (IntPtr)0
	};

	public static readonly Baselib_FileIO_SyncFile Baselib_FileIO_SyncFile_Invalid = new Baselib_FileIO_SyncFile
	{
		handle = (IntPtr)(-1)
	};

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_OpenUtf8(byte* pathnameUtf8, Baselib_ErrorState* errorState)
	{
		Baselib_DynamicLibrary_OpenUtf8_Injected(pathnameUtf8, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_OpenUtf16(char* pathnameUtf16, Baselib_ErrorState* errorState)
	{
		Baselib_DynamicLibrary_OpenUtf16_Injected(pathnameUtf16, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_OpenProgramHandle(Baselib_ErrorState* errorState)
	{
		Baselib_DynamicLibrary_OpenProgramHandle_Injected(errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_DynamicLibrary_Handle Baselib_DynamicLibrary_FromNativeHandle(ulong handle, uint type, Baselib_ErrorState* errorState)
	{
		Baselib_DynamicLibrary_FromNativeHandle_Injected(handle, type, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static IntPtr Baselib_DynamicLibrary_GetFunction(Baselib_DynamicLibrary_Handle handle, byte* functionName, Baselib_ErrorState* errorState)
	{
		return Baselib_DynamicLibrary_GetFunction_Injected(ref handle, functionName, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_DynamicLibrary_Close(Baselib_DynamicLibrary_Handle handle)
	{
		Baselib_DynamicLibrary_Close_Injected(ref handle);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern uint Baselib_ErrorState_Explain(Baselib_ErrorState* errorState, byte* buffer, uint bufferLen, Baselib_ErrorState_ExplainVerbosity verbosity);

	[FreeFunction(IsThreadSafe = true)]
	public static Baselib_FileIO_EventQueue Baselib_FileIO_EventQueue_Create()
	{
		Baselib_FileIO_EventQueue_Create_Injected(out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_FileIO_EventQueue_Free(Baselib_FileIO_EventQueue eq)
	{
		Baselib_FileIO_EventQueue_Free_Injected(ref eq);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static ulong Baselib_FileIO_EventQueue_Dequeue(Baselib_FileIO_EventQueue eq, Baselib_FileIO_EventQueue_Result* results, ulong count, uint timeoutInMilliseconds)
	{
		return Baselib_FileIO_EventQueue_Dequeue_Injected(ref eq, results, count, timeoutInMilliseconds);
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_FileIO_EventQueue_Shutdown(Baselib_FileIO_EventQueue eq, uint threadCount)
	{
		Baselib_FileIO_EventQueue_Shutdown_Injected(ref eq, threadCount);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_FileIO_AsyncFile Baselib_FileIO_AsyncOpen(Baselib_FileIO_EventQueue eq, byte* pathname, ulong userdata, Baselib_FileIO_Priority priority)
	{
		Baselib_FileIO_AsyncOpen_Injected(ref eq, pathname, userdata, priority, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_FileIO_AsyncRead(Baselib_FileIO_AsyncFile file, Baselib_FileIO_ReadRequest* requests, ulong count, ulong userdata, Baselib_FileIO_Priority priority)
	{
		Baselib_FileIO_AsyncRead_Injected(ref file, requests, count, userdata, priority);
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_FileIO_AsyncClose(Baselib_FileIO_AsyncFile file)
	{
		Baselib_FileIO_AsyncClose_Injected(ref file);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_FileIO_SyncFile Baselib_FileIO_SyncOpen(byte* pathname, Baselib_FileIO_OpenFlags openFlags, Baselib_ErrorState* errorState)
	{
		Baselib_FileIO_SyncOpen_Injected(pathname, openFlags, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public static Baselib_FileIO_SyncFile Baselib_FileIO_SyncFileFromNativeHandle(ulong handle, uint type)
	{
		Baselib_FileIO_SyncFileFromNativeHandle_Injected(handle, type, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static ulong Baselib_FileIO_SyncRead(Baselib_FileIO_SyncFile file, ulong offset, IntPtr buffer, ulong size, Baselib_ErrorState* errorState)
	{
		return Baselib_FileIO_SyncRead_Injected(ref file, offset, buffer, size, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static ulong Baselib_FileIO_SyncWrite(Baselib_FileIO_SyncFile file, ulong offset, IntPtr buffer, ulong size, Baselib_ErrorState* errorState)
	{
		return Baselib_FileIO_SyncWrite_Injected(ref file, offset, buffer, size, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_FileIO_SyncFlush(Baselib_FileIO_SyncFile file, Baselib_ErrorState* errorState)
	{
		Baselib_FileIO_SyncFlush_Injected(ref file, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_FileIO_SyncSetFileSize(Baselib_FileIO_SyncFile file, ulong size, Baselib_ErrorState* errorState)
	{
		Baselib_FileIO_SyncSetFileSize_Injected(ref file, size, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static ulong Baselib_FileIO_SyncGetFileSize(Baselib_FileIO_SyncFile file, Baselib_ErrorState* errorState)
	{
		return Baselib_FileIO_SyncGetFileSize_Injected(ref file, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_FileIO_SyncClose(Baselib_FileIO_SyncFile file, Baselib_ErrorState* errorState)
	{
		Baselib_FileIO_SyncClose_Injected(ref file, errorState);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void Baselib_Memory_GetPageSizeInfo(Baselib_Memory_PageSizeInfo* outPagesSizeInfo);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern IntPtr Baselib_Memory_Allocate(UIntPtr size);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern IntPtr Baselib_Memory_Reallocate(IntPtr ptr, UIntPtr newSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern void Baselib_Memory_Free(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern IntPtr Baselib_Memory_AlignedAllocate(UIntPtr size, UIntPtr alignment);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern IntPtr Baselib_Memory_AlignedReallocate(IntPtr ptr, UIntPtr newSize, UIntPtr alignment);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern void Baselib_Memory_AlignedFree(IntPtr ptr);

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_Memory_PageAllocation Baselib_Memory_AllocatePages(ulong pageSize, ulong pageCount, ulong alignmentInMultipleOfPageSize, Baselib_Memory_PageState pageState, Baselib_ErrorState* errorState)
	{
		Baselib_Memory_AllocatePages_Injected(pageSize, pageCount, alignmentInMultipleOfPageSize, pageState, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_Memory_ReleasePages(Baselib_Memory_PageAllocation pageAllocation, Baselib_ErrorState* errorState)
	{
		Baselib_Memory_ReleasePages_Injected(ref pageAllocation, errorState);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void Baselib_Memory_SetPageState(IntPtr addressOfFirstPage, ulong pageSize, ulong pageCount, Baselib_Memory_PageState pageState, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void Baselib_NetworkAddress_Encode(Baselib_NetworkAddress* dstAddress, Baselib_NetworkAddress_Family family, byte* ip, ushort port, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void Baselib_NetworkAddress_Decode(Baselib_NetworkAddress* srcAddress, Baselib_NetworkAddress_Family* family, byte* ipAddressBuffer, uint ipAddressBufferLen, ushort* port, Baselib_ErrorState* errorState);

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_Buffer Baselib_RegisteredNetwork_Buffer_Register(Baselib_Memory_PageAllocation pageAllocation, Baselib_ErrorState* errorState)
	{
		Baselib_RegisteredNetwork_Buffer_Register_Injected(ref pageAllocation, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_RegisteredNetwork_Buffer_Deregister(Baselib_RegisteredNetwork_Buffer buffer)
	{
		Baselib_RegisteredNetwork_Buffer_Deregister_Injected(ref buffer);
	}

	[FreeFunction(IsThreadSafe = true)]
	public static Baselib_RegisteredNetwork_BufferSlice Baselib_RegisteredNetwork_BufferSlice_Create(Baselib_RegisteredNetwork_Buffer buffer, uint offset, uint size)
	{
		Baselib_RegisteredNetwork_BufferSlice_Create_Injected(ref buffer, offset, size, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public static Baselib_RegisteredNetwork_BufferSlice Baselib_RegisteredNetwork_BufferSlice_Empty()
	{
		Baselib_RegisteredNetwork_BufferSlice_Empty_Injected(out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_Endpoint Baselib_RegisteredNetwork_Endpoint_Create(Baselib_NetworkAddress* srcAddress, Baselib_RegisteredNetwork_BufferSlice dstSlice, Baselib_ErrorState* errorState)
	{
		Baselib_RegisteredNetwork_Endpoint_Create_Injected(srcAddress, ref dstSlice, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public static Baselib_RegisteredNetwork_Endpoint Baselib_RegisteredNetwork_Endpoint_Empty()
	{
		Baselib_RegisteredNetwork_Endpoint_Empty_Injected(out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_RegisteredNetwork_Endpoint_GetNetworkAddress(Baselib_RegisteredNetwork_Endpoint endpoint, Baselib_NetworkAddress* dstAddress, Baselib_ErrorState* errorState)
	{
		Baselib_RegisteredNetwork_Endpoint_GetNetworkAddress_Injected(ref endpoint, dstAddress, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_Socket_UDP Baselib_RegisteredNetwork_Socket_UDP_Create(Baselib_NetworkAddress* bindAddress, Baselib_NetworkAddress_AddressReuse endpointReuse, uint sendQueueSize, uint recvQueueSize, Baselib_ErrorState* errorState)
	{
		Baselib_RegisteredNetwork_Socket_UDP_Create_Injected(bindAddress, endpointReuse, sendQueueSize, recvQueueSize, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_RegisteredNetwork_Socket_UDP_ScheduleRecv(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_Request* requests, uint requestsCount, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_ScheduleRecv_Injected(ref socket, requests, requestsCount, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_RegisteredNetwork_Socket_UDP_ScheduleSend(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_Request* requests, uint requestsCount, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_ScheduleSend_Injected(ref socket, requests, requestsCount, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_ProcessStatus Baselib_RegisteredNetwork_Socket_UDP_ProcessRecv(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_ProcessRecv_Injected(ref socket, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_ProcessStatus Baselib_RegisteredNetwork_Socket_UDP_ProcessSend(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_ProcessSend_Injected(ref socket, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_CompletionQueueStatus Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedRecv(Baselib_RegisteredNetwork_Socket_UDP socket, uint timeoutInMilliseconds, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedRecv_Injected(ref socket, timeoutInMilliseconds, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_RegisteredNetwork_CompletionQueueStatus Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedSend(Baselib_RegisteredNetwork_Socket_UDP socket, uint timeoutInMilliseconds, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedSend_Injected(ref socket, timeoutInMilliseconds, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_RegisteredNetwork_Socket_UDP_DequeueRecv(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_CompletionResult* results, uint resultsCount, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_DequeueRecv_Injected(ref socket, results, resultsCount, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_RegisteredNetwork_Socket_UDP_DequeueSend(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_CompletionResult* results, uint resultsCount, Baselib_ErrorState* errorState)
	{
		return Baselib_RegisteredNetwork_Socket_UDP_DequeueSend_Injected(ref socket, results, resultsCount, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_RegisteredNetwork_Socket_UDP_GetNetworkAddress(Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_NetworkAddress* dstAddress, Baselib_ErrorState* errorState)
	{
		Baselib_RegisteredNetwork_Socket_UDP_GetNetworkAddress_Injected(ref socket, dstAddress, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_RegisteredNetwork_Socket_UDP_Close(Baselib_RegisteredNetwork_Socket_UDP socket)
	{
		Baselib_RegisteredNetwork_Socket_UDP_Close_Injected(ref socket);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_Socket_Handle Baselib_Socket_Create(Baselib_NetworkAddress_Family family, Baselib_Socket_Protocol protocol, Baselib_ErrorState* errorState)
	{
		Baselib_Socket_Create_Injected(family, protocol, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_Socket_Bind(Baselib_Socket_Handle socket, Baselib_NetworkAddress* address, Baselib_NetworkAddress_AddressReuse addressReuse, Baselib_ErrorState* errorState)
	{
		Baselib_Socket_Bind_Injected(ref socket, address, addressReuse, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_Socket_TCP_Connect(Baselib_Socket_Handle socket, Baselib_NetworkAddress* address, Baselib_NetworkAddress_AddressReuse addressReuse, Baselib_ErrorState* errorState)
	{
		Baselib_Socket_TCP_Connect_Injected(ref socket, address, addressReuse, errorState);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public unsafe static extern void Baselib_Socket_Poll(Baselib_Socket_PollFd* sockets, uint socketsCount, uint timeoutInMilliseconds, Baselib_ErrorState* errorState);

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_Socket_GetAddress(Baselib_Socket_Handle socket, Baselib_NetworkAddress* address, Baselib_ErrorState* errorState)
	{
		Baselib_Socket_GetAddress_Injected(ref socket, address, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static void Baselib_Socket_TCP_Listen(Baselib_Socket_Handle socket, Baselib_ErrorState* errorState)
	{
		Baselib_Socket_TCP_Listen_Injected(ref socket, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static Baselib_Socket_Handle Baselib_Socket_TCP_Accept(Baselib_Socket_Handle socket, Baselib_ErrorState* errorState)
	{
		Baselib_Socket_TCP_Accept_Injected(ref socket, errorState, out var ret);
		return ret;
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_Socket_UDP_Send(Baselib_Socket_Handle socket, Baselib_Socket_Message* messages, uint messagesCount, Baselib_ErrorState* errorState)
	{
		return Baselib_Socket_UDP_Send_Injected(ref socket, messages, messagesCount, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_Socket_TCP_Send(Baselib_Socket_Handle socket, IntPtr data, uint dataLen, Baselib_ErrorState* errorState)
	{
		return Baselib_Socket_TCP_Send_Injected(ref socket, data, dataLen, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_Socket_UDP_Recv(Baselib_Socket_Handle socket, Baselib_Socket_Message* messages, uint messagesCount, Baselib_ErrorState* errorState)
	{
		return Baselib_Socket_UDP_Recv_Injected(ref socket, messages, messagesCount, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public unsafe static uint Baselib_Socket_TCP_Recv(Baselib_Socket_Handle socket, IntPtr data, uint dataLen, Baselib_ErrorState* errorState)
	{
		return Baselib_Socket_TCP_Recv_Injected(ref socket, data, dataLen, errorState);
	}

	[FreeFunction(IsThreadSafe = true)]
	public static void Baselib_Socket_Close(Baselib_Socket_Handle socket)
	{
		Baselib_Socket_Close_Injected(ref socket);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern void Baselib_Thread_YieldExecution();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern IntPtr Baselib_Thread_GetCurrentThreadId();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern UIntPtr Baselib_TLS_Alloc();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern void Baselib_TLS_Free(UIntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern void Baselib_TLS_Set(UIntPtr handle, UIntPtr value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern UIntPtr Baselib_TLS_Get(UIntPtr handle);

	[FreeFunction(IsThreadSafe = true)]
	public static Baselib_Timer_TickToNanosecondConversionRatio Baselib_Timer_GetTicksToNanosecondsConversionRatio()
	{
		Baselib_Timer_GetTicksToNanosecondsConversionRatio_Injected(out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern ulong Baselib_Timer_GetHighPrecisionTimerTicks();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern void Baselib_Timer_WaitForAtLeast(uint timeInMilliseconds);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern double Baselib_Timer_GetTimeSinceStartupInSeconds();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_DynamicLibrary_OpenUtf8_Injected(byte* pathnameUtf8, Baselib_ErrorState* errorState, out Baselib_DynamicLibrary_Handle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_DynamicLibrary_OpenUtf16_Injected(char* pathnameUtf16, Baselib_ErrorState* errorState, out Baselib_DynamicLibrary_Handle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_DynamicLibrary_OpenProgramHandle_Injected(Baselib_ErrorState* errorState, out Baselib_DynamicLibrary_Handle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_DynamicLibrary_FromNativeHandle_Injected(ulong handle, uint type, Baselib_ErrorState* errorState, out Baselib_DynamicLibrary_Handle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr Baselib_DynamicLibrary_GetFunction_Injected(ref Baselib_DynamicLibrary_Handle handle, byte* functionName, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_DynamicLibrary_Close_Injected(ref Baselib_DynamicLibrary_Handle handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_FileIO_EventQueue_Create_Injected(out Baselib_FileIO_EventQueue ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_FileIO_EventQueue_Free_Injected(ref Baselib_FileIO_EventQueue eq);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern ulong Baselib_FileIO_EventQueue_Dequeue_Injected(ref Baselib_FileIO_EventQueue eq, Baselib_FileIO_EventQueue_Result* results, ulong count, uint timeoutInMilliseconds);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_FileIO_EventQueue_Shutdown_Injected(ref Baselib_FileIO_EventQueue eq, uint threadCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_FileIO_AsyncOpen_Injected(ref Baselib_FileIO_EventQueue eq, byte* pathname, ulong userdata, Baselib_FileIO_Priority priority, out Baselib_FileIO_AsyncFile ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_FileIO_AsyncRead_Injected(ref Baselib_FileIO_AsyncFile file, Baselib_FileIO_ReadRequest* requests, ulong count, ulong userdata, Baselib_FileIO_Priority priority);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_FileIO_AsyncClose_Injected(ref Baselib_FileIO_AsyncFile file);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_FileIO_SyncOpen_Injected(byte* pathname, Baselib_FileIO_OpenFlags openFlags, Baselib_ErrorState* errorState, out Baselib_FileIO_SyncFile ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_FileIO_SyncFileFromNativeHandle_Injected(ulong handle, uint type, out Baselib_FileIO_SyncFile ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern ulong Baselib_FileIO_SyncRead_Injected(ref Baselib_FileIO_SyncFile file, ulong offset, IntPtr buffer, ulong size, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern ulong Baselib_FileIO_SyncWrite_Injected(ref Baselib_FileIO_SyncFile file, ulong offset, IntPtr buffer, ulong size, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_FileIO_SyncFlush_Injected(ref Baselib_FileIO_SyncFile file, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_FileIO_SyncSetFileSize_Injected(ref Baselib_FileIO_SyncFile file, ulong size, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern ulong Baselib_FileIO_SyncGetFileSize_Injected(ref Baselib_FileIO_SyncFile file, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_FileIO_SyncClose_Injected(ref Baselib_FileIO_SyncFile file, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Memory_AllocatePages_Injected(ulong pageSize, ulong pageCount, ulong alignmentInMultipleOfPageSize, Baselib_Memory_PageState pageState, Baselib_ErrorState* errorState, out Baselib_Memory_PageAllocation ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Memory_ReleasePages_Injected(ref Baselib_Memory_PageAllocation pageAllocation, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_RegisteredNetwork_Buffer_Register_Injected(ref Baselib_Memory_PageAllocation pageAllocation, Baselib_ErrorState* errorState, out Baselib_RegisteredNetwork_Buffer ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_RegisteredNetwork_Buffer_Deregister_Injected(ref Baselib_RegisteredNetwork_Buffer buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_RegisteredNetwork_BufferSlice_Create_Injected(ref Baselib_RegisteredNetwork_Buffer buffer, uint offset, uint size, out Baselib_RegisteredNetwork_BufferSlice ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_RegisteredNetwork_BufferSlice_Empty_Injected(out Baselib_RegisteredNetwork_BufferSlice ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_RegisteredNetwork_Endpoint_Create_Injected(Baselib_NetworkAddress* srcAddress, ref Baselib_RegisteredNetwork_BufferSlice dstSlice, Baselib_ErrorState* errorState, out Baselib_RegisteredNetwork_Endpoint ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_RegisteredNetwork_Endpoint_Empty_Injected(out Baselib_RegisteredNetwork_Endpoint ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_RegisteredNetwork_Endpoint_GetNetworkAddress_Injected(ref Baselib_RegisteredNetwork_Endpoint endpoint, Baselib_NetworkAddress* dstAddress, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_RegisteredNetwork_Socket_UDP_Create_Injected(Baselib_NetworkAddress* bindAddress, Baselib_NetworkAddress_AddressReuse endpointReuse, uint sendQueueSize, uint recvQueueSize, Baselib_ErrorState* errorState, out Baselib_RegisteredNetwork_Socket_UDP ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_RegisteredNetwork_Socket_UDP_ScheduleRecv_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_Request* requests, uint requestsCount, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_RegisteredNetwork_Socket_UDP_ScheduleSend_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_Request* requests, uint requestsCount, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern Baselib_RegisteredNetwork_ProcessStatus Baselib_RegisteredNetwork_Socket_UDP_ProcessRecv_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern Baselib_RegisteredNetwork_ProcessStatus Baselib_RegisteredNetwork_Socket_UDP_ProcessSend_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern Baselib_RegisteredNetwork_CompletionQueueStatus Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedRecv_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, uint timeoutInMilliseconds, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern Baselib_RegisteredNetwork_CompletionQueueStatus Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedSend_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, uint timeoutInMilliseconds, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_RegisteredNetwork_Socket_UDP_DequeueRecv_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_CompletionResult* results, uint resultsCount, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_RegisteredNetwork_Socket_UDP_DequeueSend_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_RegisteredNetwork_CompletionResult* results, uint resultsCount, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_RegisteredNetwork_Socket_UDP_GetNetworkAddress_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket, Baselib_NetworkAddress* dstAddress, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_RegisteredNetwork_Socket_UDP_Close_Injected(ref Baselib_RegisteredNetwork_Socket_UDP socket);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Socket_Create_Injected(Baselib_NetworkAddress_Family family, Baselib_Socket_Protocol protocol, Baselib_ErrorState* errorState, out Baselib_Socket_Handle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Socket_Bind_Injected(ref Baselib_Socket_Handle socket, Baselib_NetworkAddress* address, Baselib_NetworkAddress_AddressReuse addressReuse, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Socket_TCP_Connect_Injected(ref Baselib_Socket_Handle socket, Baselib_NetworkAddress* address, Baselib_NetworkAddress_AddressReuse addressReuse, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Socket_GetAddress_Injected(ref Baselib_Socket_Handle socket, Baselib_NetworkAddress* address, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Socket_TCP_Listen_Injected(ref Baselib_Socket_Handle socket, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void Baselib_Socket_TCP_Accept_Injected(ref Baselib_Socket_Handle socket, Baselib_ErrorState* errorState, out Baselib_Socket_Handle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_Socket_UDP_Send_Injected(ref Baselib_Socket_Handle socket, Baselib_Socket_Message* messages, uint messagesCount, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_Socket_TCP_Send_Injected(ref Baselib_Socket_Handle socket, IntPtr data, uint dataLen, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_Socket_UDP_Recv_Injected(ref Baselib_Socket_Handle socket, Baselib_Socket_Message* messages, uint messagesCount, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern uint Baselib_Socket_TCP_Recv_Injected(ref Baselib_Socket_Handle socket, IntPtr data, uint dataLen, Baselib_ErrorState* errorState);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_Socket_Close_Injected(ref Baselib_Socket_Handle socket);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Baselib_Timer_GetTicksToNanosecondsConversionRatio_Injected(out Baselib_Timer_TickToNanosecondConversionRatio ret);
}
