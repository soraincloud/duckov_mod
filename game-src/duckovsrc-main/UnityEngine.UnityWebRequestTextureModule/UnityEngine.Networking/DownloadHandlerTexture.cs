using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Bindings;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityWebRequestTexture/Public/DownloadHandlerTexture.h")]
public sealed class DownloadHandlerTexture : DownloadHandler
{
	private NativeArray<byte> m_NativeData;

	private bool mNonReadable;

	public Texture2D texture => InternalGetTextureNative();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Create(DownloadHandlerTexture obj, bool readable);

	private void InternalCreateTexture(bool readable)
	{
		m_Ptr = Create(this, readable);
	}

	public DownloadHandlerTexture()
	{
		InternalCreateTexture(readable: true);
	}

	public DownloadHandlerTexture(bool readable)
	{
		InternalCreateTexture(readable);
		mNonReadable = !readable;
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

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private extern Texture2D InternalGetTextureNative();

	public static Texture2D GetContent(UnityWebRequest www)
	{
		return DownloadHandler.GetCheckedDownloader<DownloadHandlerTexture>(www).texture;
	}
}
