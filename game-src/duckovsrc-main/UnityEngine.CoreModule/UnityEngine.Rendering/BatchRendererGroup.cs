using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Runtime/Math/Matrix4x4.h")]
[NativeHeader("Runtime/Camera/BatchRendererGroup.h")]
[RequiredByNativeCode]
public class BatchRendererGroup : IDisposable
{
	public delegate JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext, BatchCullingOutput cullingOutput, IntPtr userContext);

	private IntPtr m_GroupHandle = IntPtr.Zero;

	private OnPerformCulling m_PerformCulling;

	public static BatchBufferTarget BufferTarget => GetBufferTarget();

	public unsafe BatchRendererGroup(OnPerformCulling cullingCallback, IntPtr userContext)
	{
		m_PerformCulling = cullingCallback;
		m_GroupHandle = Create(this, (void*)userContext);
	}

	public void Dispose()
	{
		Destroy(m_GroupHandle);
		m_GroupHandle = IntPtr.Zero;
	}

	public ThreadedBatchContext GetThreadedBatchContext()
	{
		return new ThreadedBatchContext
		{
			batchRendererGroup = m_GroupHandle
		};
	}

	private BatchID AddDrawCommandBatch(IntPtr values, int count, GraphicsBufferHandle buffer, uint bufferOffset, uint windowSize)
	{
		AddDrawCommandBatch_Injected(values, count, ref buffer, bufferOffset, windowSize, out var ret);
		return ret;
	}

	public unsafe BatchID AddBatch(NativeArray<MetadataValue> batchMetadata, GraphicsBufferHandle buffer)
	{
		return AddDrawCommandBatch((IntPtr)batchMetadata.GetUnsafeReadOnlyPtr(), batchMetadata.Length, buffer, 0u, 0u);
	}

	public unsafe BatchID AddBatch(NativeArray<MetadataValue> batchMetadata, GraphicsBufferHandle buffer, uint bufferOffset, uint windowSize)
	{
		return AddDrawCommandBatch((IntPtr)batchMetadata.GetUnsafeReadOnlyPtr(), batchMetadata.Length, buffer, bufferOffset, windowSize);
	}

	private void RemoveDrawCommandBatch(BatchID batchID)
	{
		RemoveDrawCommandBatch_Injected(ref batchID);
	}

	public void RemoveBatch(BatchID batchID)
	{
		RemoveDrawCommandBatch(batchID);
	}

	private void SetDrawCommandBatchBuffer(BatchID batchID, GraphicsBufferHandle buffer)
	{
		SetDrawCommandBatchBuffer_Injected(ref batchID, ref buffer);
	}

	public void SetBatchBuffer(BatchID batchID, GraphicsBufferHandle buffer)
	{
		SetDrawCommandBatchBuffer(batchID, buffer);
	}

	public BatchMaterialID RegisterMaterial(Material material)
	{
		RegisterMaterial_Injected(material, out var ret);
		return ret;
	}

	public BatchMaterialID RegisterMaterial(int materialInstanceID)
	{
		return RegisterMaterial_InstanceID(materialInstanceID);
	}

	private BatchMaterialID RegisterMaterial_InstanceID(int materialInstanceID)
	{
		RegisterMaterial_InstanceID_Injected(materialInstanceID, out var ret);
		return ret;
	}

	public void UnregisterMaterial(BatchMaterialID material)
	{
		UnregisterMaterial_Injected(ref material);
	}

	public Material GetRegisteredMaterial(BatchMaterialID material)
	{
		return GetRegisteredMaterial_Injected(ref material);
	}

	public BatchMeshID RegisterMesh(Mesh mesh)
	{
		RegisterMesh_Injected(mesh, out var ret);
		return ret;
	}

	public BatchMeshID RegisterMesh(int meshInstanceID)
	{
		return RegisterMesh_InstanceID(meshInstanceID);
	}

	private BatchMeshID RegisterMesh_InstanceID(int meshInstanceID)
	{
		RegisterMesh_InstanceID_Injected(meshInstanceID, out var ret);
		return ret;
	}

	public void UnregisterMesh(BatchMeshID mesh)
	{
		UnregisterMesh_Injected(ref mesh);
	}

	public Mesh GetRegisteredMesh(BatchMeshID mesh)
	{
		return GetRegisteredMesh_Injected(ref mesh);
	}

	public void SetGlobalBounds(Bounds bounds)
	{
		SetGlobalBounds_Injected(ref bounds);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetPickingMaterial(Material material);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetErrorMaterial(Material material);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetLoadingMaterial(Material material);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetEnabledViewTypes(BatchCullingViewType[] viewTypes);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern BatchBufferTarget GetBufferTarget();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern int GetConstantBufferMaxWindowSize();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern int GetConstantBufferOffsetAlignment();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern IntPtr Create(BatchRendererGroup group, void* userContext);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Destroy(IntPtr groupHandle);

	[RequiredByNativeCode]
	private unsafe static void InvokeOnPerformCulling(BatchRendererGroup group, ref BatchRendererCullingOutput context, ref LODParameters lodParameters, IntPtr userContext)
	{
		NativeArray<Plane> inCullingPlanes = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Plane>(context.cullingPlanes, context.cullingPlaneCount, Allocator.Invalid);
		NativeArray<CullingSplit> inCullingSplits = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<CullingSplit>(context.cullingSplits, context.cullingSplitCount, Allocator.Invalid);
		NativeArray<BatchCullingOutputDrawCommands> drawCommands = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<BatchCullingOutputDrawCommands>(context.drawCommands, 1, Allocator.Invalid);
		try
		{
			BatchCullingOutput cullingOutput = new BatchCullingOutput
			{
				drawCommands = drawCommands
			};
			context.cullingJobsFence = group.m_PerformCulling(group, new BatchCullingContext(inCullingPlanes, inCullingSplits, lodParameters, context.localToWorldMatrix, context.viewType, context.projectionType, context.cullingFlags, context.viewID, context.cullingLayerMask, context.sceneCullingMask, context.receiverPlaneOffset, context.receiverPlaneCount), cullingOutput, userContext);
		}
		finally
		{
			JobHandle.ScheduleBatchedJobs();
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddDrawCommandBatch_Injected(IntPtr values, int count, ref GraphicsBufferHandle buffer, uint bufferOffset, uint windowSize, out BatchID ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void RemoveDrawCommandBatch_Injected(ref BatchID batchID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetDrawCommandBatchBuffer_Injected(ref BatchID batchID, ref GraphicsBufferHandle buffer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void RegisterMaterial_Injected(Material material, out BatchMaterialID ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void RegisterMaterial_InstanceID_Injected(int materialInstanceID, out BatchMaterialID ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void UnregisterMaterial_Injected(ref BatchMaterialID material);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern Material GetRegisteredMaterial_Injected(ref BatchMaterialID material);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void RegisterMesh_Injected(Mesh mesh, out BatchMeshID ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void RegisterMesh_InstanceID_Injected(int meshInstanceID, out BatchMeshID ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void UnregisterMesh_Injected(ref BatchMeshID mesh);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern Mesh GetRegisteredMesh_Injected(ref BatchMeshID mesh);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void SetGlobalBounds_Injected(ref Bounds bounds);
}
