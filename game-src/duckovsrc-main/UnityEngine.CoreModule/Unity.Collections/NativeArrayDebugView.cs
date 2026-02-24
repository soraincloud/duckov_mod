using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal sealed class NativeArrayDebugView<T> where T : struct
{
	private NativeArray<T> m_Array;

	public unsafe T[] Items
	{
		get
		{
			if (!m_Array.IsCreated)
			{
				return null;
			}
			int length = m_Array.m_Length;
			T[] array = new T[length];
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			IntPtr intPtr = gCHandle.AddrOfPinnedObject();
			UnsafeUtility.MemCpy((void*)intPtr, m_Array.m_Buffer, length * UnsafeUtility.SizeOf<T>());
			gCHandle.Free();
			return array;
		}
	}

	public NativeArrayDebugView(NativeArray<T> array)
	{
		m_Array = array;
	}
}
