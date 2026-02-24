using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal class UnsafeQueueBlockPool
{
	private static readonly SharedStatic<IntPtr> Data = SharedStatic<IntPtr>.GetOrCreateUnsafe(0u, 8615650021869908731L, 0L);

	internal unsafe static UnsafeQueueBlockPoolData* GetQueueBlockPool()
	{
		UnsafeQueueBlockPoolData** unsafeDataPointer = (UnsafeQueueBlockPoolData**)Data.UnsafeDataPointer;
		UnsafeQueueBlockPoolData* ptr = *unsafeDataPointer;
		if (ptr == null)
		{
			ptr = (*unsafeDataPointer = (UnsafeQueueBlockPoolData*)Memory.Unmanaged.Allocate(UnsafeUtility.SizeOf<UnsafeQueueBlockPoolData>(), 8, Allocator.Persistent));
			ptr->m_NumBlocks = (ptr->m_MaxBlocks = 256);
			ptr->m_AllocLock = 0;
			UnsafeQueueBlockHeader* ptr2 = null;
			for (int i = 0; i < ptr->m_MaxBlocks; i++)
			{
				UnsafeQueueBlockHeader* ptr3 = (UnsafeQueueBlockHeader*)Memory.Unmanaged.Allocate(16384L, 16, Allocator.Persistent);
				ptr3->m_NextBlock = ptr2;
				ptr2 = ptr3;
			}
			ptr->m_FirstBlock = (IntPtr)ptr2;
			AppDomainOnDomainUnload();
		}
		return ptr;
	}

	[BurstDiscard]
	private static void AppDomainOnDomainUnload()
	{
		AppDomain.CurrentDomain.DomainUnload += OnDomainUnload;
	}

	private unsafe static void OnDomainUnload(object sender, EventArgs e)
	{
		UnsafeQueueBlockPoolData** unsafeDataPointer = (UnsafeQueueBlockPoolData**)Data.UnsafeDataPointer;
		UnsafeQueueBlockPoolData* ptr = *unsafeDataPointer;
		while (ptr->m_FirstBlock != IntPtr.Zero)
		{
			UnsafeQueueBlockHeader* ptr2 = (UnsafeQueueBlockHeader*)(void*)ptr->m_FirstBlock;
			ptr->m_FirstBlock = (IntPtr)ptr2->m_NextBlock;
			Memory.Unmanaged.Free(ptr2, Allocator.Persistent);
			ptr->m_NumBlocks--;
		}
		Memory.Unmanaged.Free(ptr, Allocator.Persistent);
		*unsafeDataPointer = null;
	}
}
