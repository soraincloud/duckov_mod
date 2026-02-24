using System;

namespace UnityEngine.UIElements;

internal struct SafeHandleAccess
{
	private IntPtr m_Handle;

	public SafeHandleAccess(IntPtr ptr)
	{
		m_Handle = ptr;
	}

	public bool IsNull()
	{
		return m_Handle == IntPtr.Zero;
	}

	public static implicit operator IntPtr(SafeHandleAccess a)
	{
		if (a.m_Handle == IntPtr.Zero)
		{
			throw new ArgumentNullException();
		}
		return a.m_Handle;
	}
}
