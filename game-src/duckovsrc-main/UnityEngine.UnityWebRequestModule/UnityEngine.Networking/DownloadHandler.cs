using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Networking;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityWebRequest/Public/DownloadHandler/DownloadHandler.h")]
public class DownloadHandler : IDisposable
{
	[NonSerialized]
	[VisibleToOtherModules]
	internal IntPtr m_Ptr;

	public bool isDone => IsDone();

	public string error => GetErrorMsg();

	public NativeArray<byte>.ReadOnly nativeData => GetNativeData().AsReadOnly();

	public byte[] data => GetData();

	public string text => GetText();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	private extern void Release();

	[VisibleToOtherModules]
	internal DownloadHandler()
	{
	}

	~DownloadHandler()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		if (m_Ptr != IntPtr.Zero)
		{
			Release();
			m_Ptr = IntPtr.Zero;
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern bool IsDone();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string GetErrorMsg();

	protected virtual NativeArray<byte> GetNativeData()
	{
		return default(NativeArray<byte>);
	}

	protected virtual byte[] GetData()
	{
		return InternalGetByteArray(this);
	}

	protected unsafe virtual string GetText()
	{
		NativeArray<byte> nativeArray = GetNativeData();
		if (nativeArray.IsCreated && nativeArray.Length > 0)
		{
			return new string((sbyte*)nativeArray.GetUnsafeReadOnlyPtr(), 0, nativeArray.Length, GetTextEncoder());
		}
		return "";
	}

	private Encoding GetTextEncoder()
	{
		string contentType = GetContentType();
		if (!string.IsNullOrEmpty(contentType))
		{
			int num = contentType.IndexOf("charset", StringComparison.OrdinalIgnoreCase);
			if (num > -1)
			{
				int num2 = contentType.IndexOf('=', num);
				if (num2 > -1)
				{
					string text = contentType.Substring(num2 + 1).Trim().Trim('\'', '"')
						.Trim();
					int num3 = text.IndexOf(';');
					if (num3 > -1)
					{
						text = text.Substring(0, num3);
					}
					try
					{
						return Encoding.GetEncoding(text);
					}
					catch (ArgumentException ex)
					{
						Debug.LogWarning($"Unsupported encoding '{text}': {ex.Message}");
					}
					catch (NotSupportedException ex2)
					{
						Debug.LogWarning($"Unsupported encoding '{text}': {ex2.Message}");
					}
				}
			}
		}
		return Encoding.UTF8;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern string GetContentType();

	[RequiredByNativeCode]
	protected virtual bool ReceiveData(byte[] data, int dataLength)
	{
		return true;
	}

	[RequiredByNativeCode]
	protected virtual void ReceiveContentLengthHeader(ulong contentLength)
	{
		ReceiveContentLength((int)contentLength);
	}

	[Obsolete("Use ReceiveContentLengthHeader")]
	protected virtual void ReceiveContentLength(int contentLength)
	{
	}

	[RequiredByNativeCode]
	protected virtual void CompleteContent()
	{
	}

	[RequiredByNativeCode]
	protected virtual float GetProgress()
	{
		return 0f;
	}

	protected static T GetCheckedDownloader<T>(UnityWebRequest www) where T : DownloadHandler
	{
		if (www == null)
		{
			throw new NullReferenceException("Cannot get content from a null UnityWebRequest object");
		}
		if (!www.isDone)
		{
			throw new InvalidOperationException("Cannot get content from an unfinished UnityWebRequest object");
		}
		if (www.result == UnityWebRequest.Result.ProtocolError)
		{
			throw new InvalidOperationException(www.error);
		}
		return (T)www.downloadHandler;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[VisibleToOtherModules]
	[NativeThrows]
	internal unsafe static extern byte* InternalGetByteArray(DownloadHandler dh, out int length);

	internal static byte[] InternalGetByteArray(DownloadHandler dh)
	{
		NativeArray<byte> nativeArray = dh.GetNativeData();
		if (nativeArray.IsCreated)
		{
			return nativeArray.ToArray();
		}
		return null;
	}

	internal unsafe static NativeArray<byte> InternalGetNativeArray(DownloadHandler dh, ref NativeArray<byte> nativeArray)
	{
		int length;
		byte* bytes = InternalGetByteArray(dh, out length);
		if (nativeArray.IsCreated)
		{
			if (nativeArray.Length == length)
			{
				return nativeArray;
			}
			DisposeNativeArray(ref nativeArray);
		}
		CreateNativeArrayForNativeData(ref nativeArray, bytes, length);
		return nativeArray;
	}

	internal static void DisposeNativeArray(ref NativeArray<byte> data)
	{
		if (data.IsCreated)
		{
			data = default(NativeArray<byte>);
		}
	}

	internal unsafe static void CreateNativeArrayForNativeData(ref NativeArray<byte> data, byte* bytes, int length)
	{
		data = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(bytes, length, Allocator.Persistent);
	}
}
