using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.NVIDIA;

internal class NativeData<T> : IDisposable where T : struct
{
	private IntPtr m_MarshalledValue = IntPtr.Zero;

	public T Value = new T();

	public unsafe IntPtr Ptr
	{
		get
		{
			UnsafeUtility.CopyStructureToPtr(ref Value, m_MarshalledValue.ToPointer());
			return m_MarshalledValue;
		}
	}

	public NativeData()
	{
		m_MarshalledValue = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_MarshalledValue != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(m_MarshalledValue);
			m_MarshalledValue = IntPtr.Zero;
		}
	}

	~NativeData()
	{
		Dispose(disposing: false);
	}
}
