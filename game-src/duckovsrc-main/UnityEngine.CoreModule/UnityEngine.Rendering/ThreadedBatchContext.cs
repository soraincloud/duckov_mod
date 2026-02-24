using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering;

[NativeHeader("Runtime/Camera/BatchRendererGroup.h")]
public struct ThreadedBatchContext
{
	public IntPtr batchRendererGroup;

	[FreeFunction("BatchRendererGroup::AddDrawCommandBatch_Threaded", IsThreadSafe = true)]
	private static BatchID AddDrawCommandBatch(IntPtr brg, IntPtr values, int count, GraphicsBufferHandle buffer, uint bufferOffset, uint windowSize)
	{
		AddDrawCommandBatch_Injected(brg, values, count, ref buffer, bufferOffset, windowSize, out var ret);
		return ret;
	}

	[FreeFunction("BatchRendererGroup::SetDrawCommandBatchBuffer_Threaded", IsThreadSafe = true)]
	private static void SetDrawCommandBatchBuffer(IntPtr brg, BatchID batchID, GraphicsBufferHandle buffer)
	{
		SetDrawCommandBatchBuffer_Injected(brg, ref batchID, ref buffer);
	}

	[FreeFunction("BatchRendererGroup::RemoveDrawCommandBatch_Threaded", IsThreadSafe = true)]
	private static void RemoveDrawCommandBatch(IntPtr brg, BatchID batchID)
	{
		RemoveDrawCommandBatch_Injected(brg, ref batchID);
	}

	public unsafe BatchID AddBatch(NativeArray<MetadataValue> batchMetadata, GraphicsBufferHandle buffer)
	{
		return AddDrawCommandBatch(batchRendererGroup, (IntPtr)batchMetadata.GetUnsafeReadOnlyPtr(), batchMetadata.Length, buffer, 0u, 0u);
	}

	public unsafe BatchID AddBatch(NativeArray<MetadataValue> batchMetadata, GraphicsBufferHandle buffer, uint bufferOffset, uint windowSize)
	{
		return AddDrawCommandBatch(batchRendererGroup, (IntPtr)batchMetadata.GetUnsafeReadOnlyPtr(), batchMetadata.Length, buffer, bufferOffset, windowSize);
	}

	public void SetBatchBuffer(BatchID batchID, GraphicsBufferHandle buffer)
	{
		SetDrawCommandBatchBuffer(batchRendererGroup, batchID, buffer);
	}

	public void RemoveBatch(BatchID batchID)
	{
		RemoveDrawCommandBatch(batchRendererGroup, batchID);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void AddDrawCommandBatch_Injected(IntPtr brg, IntPtr values, int count, ref GraphicsBufferHandle buffer, uint bufferOffset, uint windowSize, out BatchID ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetDrawCommandBatchBuffer_Injected(IntPtr brg, ref BatchID batchID, ref GraphicsBufferHandle buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RemoveDrawCommandBatch_Injected(IntPtr brg, ref BatchID batchID);
}
