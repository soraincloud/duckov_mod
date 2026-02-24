using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
internal struct UnsafeQueueDispose
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeQueueData* m_Buffer;

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeQueueBlockPoolData* m_QueuePool;

	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

	public unsafe void Dispose()
	{
		UnsafeQueueData.DeallocateQueue(m_Buffer, m_QueuePool, m_AllocatorLabel);
	}
}
