using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityWebRequest/Public/UploadHandler/UploadHandlerRaw.h")]
public sealed class UploadHandlerRaw : UploadHandler
{
	private NativeArray<byte> m_Payload;

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr Create(UploadHandlerRaw self, byte* data, int dataLength);

	public UploadHandlerRaw(byte[] data)
		: this((data == null || data.Length == 0) ? default(NativeArray<byte>) : new NativeArray<byte>(data, Allocator.Persistent), transferOwnership: true)
	{
	}

	public unsafe UploadHandlerRaw(NativeArray<byte> data, bool transferOwnership)
	{
		if (!data.IsCreated || data.Length == 0)
		{
			m_Ptr = Create(this, null, 0);
			return;
		}
		if (transferOwnership)
		{
			m_Payload = data;
		}
		m_Ptr = Create(this, (byte*)data.GetUnsafeReadOnlyPtr(), data.Length);
	}

	public unsafe UploadHandlerRaw(NativeArray<byte>.ReadOnly data)
	{
		if (!data.IsCreated || data.Length == 0)
		{
			m_Ptr = Create(this, null, 0);
		}
		else if (data.Length == 0)
		{
			m_Ptr = Create(this, null, 0);
		}
		else
		{
			m_Ptr = Create(this, (byte*)data.GetUnsafeReadOnlyPtr(), data.Length);
		}
	}

	internal override byte[] GetData()
	{
		if (m_Payload.IsCreated)
		{
			return m_Payload.ToArray();
		}
		return null;
	}

	public override void Dispose()
	{
		if (m_Payload.IsCreated)
		{
			m_Payload.Dispose();
		}
		base.Dispose();
	}
}
