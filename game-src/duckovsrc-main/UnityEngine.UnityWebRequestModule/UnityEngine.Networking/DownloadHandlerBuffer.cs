using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Bindings;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityWebRequest/Public/DownloadHandler/DownloadHandlerBuffer.h")]
public sealed class DownloadHandlerBuffer : DownloadHandler
{
	private NativeArray<byte> m_NativeData;

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Create(DownloadHandlerBuffer obj);

	private void InternalCreateBuffer()
	{
		m_Ptr = Create(this);
	}

	public DownloadHandlerBuffer()
	{
		InternalCreateBuffer();
	}

	protected override NativeArray<byte> GetNativeData()
	{
		return DownloadHandler.InternalGetNativeArray(this, ref m_NativeData);
	}

	public override void Dispose()
	{
		DownloadHandler.DisposeNativeArray(ref m_NativeData);
		base.Dispose();
	}

	public static string GetContent(UnityWebRequest www)
	{
		return DownloadHandler.GetCheckedDownloader<DownloadHandlerBuffer>(www).text;
	}
}
