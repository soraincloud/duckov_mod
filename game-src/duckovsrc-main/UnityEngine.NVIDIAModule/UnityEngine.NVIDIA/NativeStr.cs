using System;
using System.Runtime.InteropServices;

namespace UnityEngine.NVIDIA;

internal class NativeStr : IDisposable
{
	private string m_Str = null;

	private IntPtr m_MarshalledString = IntPtr.Zero;

	public string Str
	{
		set
		{
			m_Str = value;
			Dispose();
			if (value != null)
			{
				m_MarshalledString = Marshal.StringToHGlobalUni(m_Str);
			}
		}
	}

	public IntPtr Ptr => m_MarshalledString;

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_MarshalledString != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(m_MarshalledString);
			m_MarshalledString = IntPtr.Zero;
		}
	}

	~NativeStr()
	{
		Dispose(disposing: false);
	}
}
