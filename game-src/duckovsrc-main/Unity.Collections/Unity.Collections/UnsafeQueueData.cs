using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
internal struct UnsafeQueueData
{
	public IntPtr m_FirstBlock;

	public IntPtr m_LastBlock;

	public int m_MaxItems;

	public int m_CurrentRead;

	public unsafe byte* m_CurrentWriteBlockTLS;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe UnsafeQueueBlockHeader* GetCurrentWriteBlockTLS(int threadIndex)
	{
		UnsafeQueueBlockHeader** ptr = (UnsafeQueueBlockHeader**)(m_CurrentWriteBlockTLS + threadIndex * 64);
		return *ptr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe void SetCurrentWriteBlockTLS(int threadIndex, UnsafeQueueBlockHeader* currentWriteBlock)
	{
		UnsafeQueueBlockHeader** ptr = (UnsafeQueueBlockHeader**)(m_CurrentWriteBlockTLS + threadIndex * 64);
		*ptr = currentWriteBlock;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static UnsafeQueueBlockHeader* AllocateWriteBlockMT<T>(UnsafeQueueData* data, UnsafeQueueBlockPoolData* pool, int threadIndex) where T : unmanaged
	{
		UnsafeQueueBlockHeader* currentWriteBlockTLS = data->GetCurrentWriteBlockTLS(threadIndex);
		if (currentWriteBlockTLS != null)
		{
			if (currentWriteBlockTLS->m_NumItems != data->m_MaxItems)
			{
				return currentWriteBlockTLS;
			}
			currentWriteBlockTLS = null;
		}
		currentWriteBlockTLS = pool->AllocateBlock();
		currentWriteBlockTLS->m_NextBlock = null;
		currentWriteBlockTLS->m_NumItems = 0;
		UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)Interlocked.Exchange(ref data->m_LastBlock, (IntPtr)currentWriteBlockTLS);
		if (ptr == null)
		{
			data->m_FirstBlock = (IntPtr)currentWriteBlockTLS;
		}
		else
		{
			ptr->m_NextBlock = currentWriteBlockTLS;
		}
		data->SetCurrentWriteBlockTLS(threadIndex, currentWriteBlockTLS);
		return currentWriteBlockTLS;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static void AllocateQueue<T>(AllocatorManager.AllocatorHandle label, out UnsafeQueueData* outBuf) where T : unmanaged
	{
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		int num = CollectionHelper.Align(UnsafeUtility.SizeOf<UnsafeQueueData>(), 64);
		UnsafeQueueData* ptr = (UnsafeQueueData*)Memory.Unmanaged.Allocate(num + 64 * threadIndexCount, 64, label);
		ptr->m_CurrentWriteBlockTLS = (byte*)ptr + num;
		ptr->m_FirstBlock = IntPtr.Zero;
		ptr->m_LastBlock = IntPtr.Zero;
		ptr->m_MaxItems = (16384 - UnsafeUtility.SizeOf<UnsafeQueueBlockHeader>()) / UnsafeUtility.SizeOf<T>();
		ptr->m_CurrentRead = 0;
		for (int i = 0; i < threadIndexCount; i++)
		{
			ptr->SetCurrentWriteBlockTLS(i, null);
		}
		outBuf = ptr;
	}

	public unsafe static void DeallocateQueue(UnsafeQueueData* data, UnsafeQueueBlockPoolData* pool, AllocatorManager.AllocatorHandle allocation)
	{
		UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)data->m_FirstBlock;
		while (ptr != null)
		{
			UnsafeQueueBlockHeader* nextBlock = ptr->m_NextBlock;
			pool->FreeBlock(ptr);
			ptr = nextBlock;
		}
		Memory.Unmanaged.Free(data, allocation);
	}
}
