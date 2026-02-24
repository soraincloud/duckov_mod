using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[NativeHeader("Modules/XR/XRPrefix.h")]
[NativeHeader("Modules/XR/Subsystems/Meshing/XRMeshingSubsystem.h")]
[NativeConditional("ENABLE_XR")]
[UsedByNativeCode]
public class XRMeshSubsystem : IntegratedSubsystem<XRMeshSubsystemDescriptor>
{
	[NativeConditional("ENABLE_XR")]
	private readonly struct MeshTransformList : IDisposable
	{
		private readonly IntPtr m_Self;

		public int Count => GetLength(m_Self);

		public IntPtr Data => GetData(m_Self);

		public MeshTransformList(IntPtr self)
		{
			m_Self = self;
		}

		public void Dispose()
		{
			Dispose(m_Self);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("UnityXRMeshTransformList_get_Length")]
		private static extern int GetLength(IntPtr self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("UnityXRMeshTransformList_get_Data")]
		private static extern IntPtr GetData(IntPtr self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("UnityXRMeshTransformList_Dispose")]
		private static extern void Dispose(IntPtr self);
	}

	public extern float meshDensity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public bool TryGetMeshInfos(List<MeshInfo> meshInfosOut)
	{
		if (meshInfosOut == null)
		{
			throw new ArgumentNullException("meshInfosOut");
		}
		return GetMeshInfosAsList(meshInfosOut);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern bool GetMeshInfosAsList(List<MeshInfo> meshInfos);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern MeshInfo[] GetMeshInfosAsFixedArray();

	public void GenerateMeshAsync(MeshId meshId, Mesh mesh, MeshCollider meshCollider, MeshVertexAttributes attributes, Action<MeshGenerationResult> onMeshGenerationComplete)
	{
		GenerateMeshAsync(meshId, mesh, meshCollider, attributes, onMeshGenerationComplete, MeshGenerationOptions.None);
	}

	public void GenerateMeshAsync(MeshId meshId, Mesh mesh, MeshCollider meshCollider, MeshVertexAttributes attributes, Action<MeshGenerationResult> onMeshGenerationComplete, MeshGenerationOptions options)
	{
		GenerateMeshAsync_Injected(ref meshId, mesh, meshCollider, attributes, onMeshGenerationComplete, options);
	}

	[RequiredByNativeCode]
	private void InvokeMeshReadyDelegate(MeshGenerationResult result, Action<MeshGenerationResult> onMeshGenerationComplete)
	{
		onMeshGenerationComplete?.Invoke(result);
	}

	public bool SetBoundingVolume(Vector3 origin, Vector3 extents)
	{
		return SetBoundingVolume_Injected(ref origin, ref extents);
	}

	public unsafe NativeArray<MeshTransform> GetUpdatedMeshTransforms(Allocator allocator)
	{
		using MeshTransformList meshTransformList = new MeshTransformList(GetUpdatedMeshTransforms());
		NativeArray<MeshTransform> nativeArray = new NativeArray<MeshTransform>(meshTransformList.Count, allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeUtility.MemCpy(nativeArray.GetUnsafePtr(), meshTransformList.Data.ToPointer(), meshTransformList.Count * sizeof(MeshTransform));
		return nativeArray;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern IntPtr GetUpdatedMeshTransforms();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GenerateMeshAsync_Injected(ref MeshId meshId, Mesh mesh, MeshCollider meshCollider, MeshVertexAttributes attributes, Action<MeshGenerationResult> onMeshGenerationComplete, MeshGenerationOptions options);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern bool SetBoundingVolume_Injected(ref Vector3 origin, ref Vector3 extents);
}
