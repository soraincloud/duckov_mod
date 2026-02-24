using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace Unity.Curl;

[StaticAccessor("UnityCurl", StaticAccessorType.DoubleColon)]
[NativeHeader("Modules/UnityCurl/Public/UnityCurl.h")]
internal static class UnityCurl
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern IntPtr CreateMultiHandle();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void DestroyMultiHandle(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal unsafe static extern IntPtr CreateEasyHandle(byte* method, byte* url, out uint curlMethod);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void SetupEasyHandle(IntPtr handle, uint curlMethod, IntPtr headers, ulong contentLen, uint flags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void DestroyEasyHandle(IntPtr handle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void QueueRequest(IntPtr multiHandle, IntPtr easyHandle);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal unsafe static extern IntPtr AppendHeader(IntPtr headerList, byte* header);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void FreeHeaderList(IntPtr headerList);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern int GetRequestErrorCode(IntPtr request);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern int GetRequestStatus(IntPtr request);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern uint GetRequestStatusCode(IntPtr request);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void GetDownloadSize(IntPtr request, out ulong downloaded, out ulong expected);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal unsafe static extern byte* GetResponseHeader(IntPtr request, uint index, out uint length);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal unsafe static extern byte* GetMoreBody(IntPtr handle, out int length);

	internal unsafe static void SendMoreBody(IntPtr handle, byte* chunk, uint length, BufferOwnership ownership)
	{
		SendMoreBody(handle, chunk, length, (int)ownership);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	private unsafe static extern void SendMoreBody(IntPtr handle, byte* chunk, uint length, int ownership);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	internal static extern void AbortRequest(IntPtr handle);
}
