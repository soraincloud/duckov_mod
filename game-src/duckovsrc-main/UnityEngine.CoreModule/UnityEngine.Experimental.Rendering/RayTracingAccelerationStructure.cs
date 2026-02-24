using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Experimental.Rendering;

public sealed class RayTracingAccelerationStructure : IDisposable
{
	[Flags]
	public enum RayTracingModeMask
	{
		Nothing = 0,
		Static = 2,
		DynamicTransform = 4,
		DynamicGeometry = 8,
		Everything = 0xE
	}

	public enum ManagementMode
	{
		Manual,
		Automatic
	}

	public struct RASSettings
	{
		public ManagementMode managementMode;

		public RayTracingModeMask rayTracingModeMask;

		public int layerMask;

		public RASSettings(ManagementMode sceneManagementMode, RayTracingModeMask rayTracingModeMask, int layerMask)
		{
			managementMode = sceneManagementMode;
			this.rayTracingModeMask = rayTracingModeMask;
			this.layerMask = layerMask;
		}
	}

	internal IntPtr m_Ptr;

	~RayTracingAccelerationStructure()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			Destroy(this);
		}
		m_Ptr = IntPtr.Zero;
	}

	public RayTracingAccelerationStructure(RASSettings settings)
	{
		m_Ptr = Create(settings);
	}

	public RayTracingAccelerationStructure()
	{
		m_Ptr = Create(new RASSettings
		{
			rayTracingModeMask = RayTracingModeMask.Everything,
			managementMode = ManagementMode.Manual,
			layerMask = -1
		});
	}

	[FreeFunction("RayTracingAccelerationStructure_Bindings::Create")]
	private static IntPtr Create(RASSettings desc)
	{
		return Create_Injected(ref desc);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("RayTracingAccelerationStructure_Bindings::Destroy")]
	private static extern void Destroy(RayTracingAccelerationStructure accelStruct);

	public void Release()
	{
		Dispose();
	}

	public void Build()
	{
		Build(Vector3.zero);
	}

	public void AddInstance(Renderer targetRenderer, RayTracingSubMeshFlags[] subMeshFlags, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, uint id = uint.MaxValue)
	{
		AddInstanceSubMeshFlagsArray(targetRenderer, subMeshFlags, enableTriangleCulling, frontTriangleCounterClockwise, mask, id);
	}

	public int AddInstance(GraphicsBuffer aabbBuffer, uint aabbCount, bool dynamicData, Matrix4x4 matrix, Material material, bool opaqueMaterial, MaterialPropertyBlock properties, uint mask = 255u, uint id = uint.MaxValue)
	{
		return AddInstance_Procedural(aabbBuffer, aabbCount, dynamicData, matrix, material, opaqueMaterial, properties, mask, id);
	}

	public void RemoveInstance(Renderer targetRenderer)
	{
		RemoveInstance_Renderer(targetRenderer);
	}

	public void RemoveInstance(int handle)
	{
		RemoveInstance_InstanceID(handle);
	}

	public void UpdateInstanceTransform(Renderer renderer)
	{
		UpdateInstanceTransform_Renderer(renderer);
	}

	public void UpdateInstanceTransform(int handle, Matrix4x4 matrix)
	{
		UpdateInstanceTransform_InstanceID(handle, matrix);
	}

	[Obsolete("Method Update has been deprecated. Use Build instead (UnityUpgradable) -> Build()", true)]
	public void Update()
	{
		Build(Vector3.zero);
	}

	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::Update", HasExplicitThis = true)]
	[Obsolete("Method Update has been deprecated. Use Build instead (UnityUpgradable) -> Build(*)", true)]
	public void Update(Vector3 relativeOrigin)
	{
		Update_Injected(ref relativeOrigin);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::AddInstanceDeprecated", HasExplicitThis = true)]
	[Obsolete("This AddInstance method has been deprecated and will be removed in a future version. Please use the alternate method for adding Renderers to the acceleration structure.", false)]
	public extern void AddInstance([NotNull("ArgumentNullException")] Renderer targetRenderer, bool[] subMeshMask = null, bool[] subMeshTransparencyFlags = null, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, uint id = uint.MaxValue);

	[Obsolete("This AddInstance method has been deprecated and will be removed in a future version. Please use the alternate method for adding procedural geometry (AABBs) to the acceleration structure.", false)]
	public void AddInstance(GraphicsBuffer aabbBuffer, uint numElements, Material material, bool isCutOff, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, bool reuseBounds = false, uint id = uint.MaxValue)
	{
		AddInstance_Procedural_Deprecated(aabbBuffer, numElements, material, Matrix4x4.identity, isCutOff, enableTriangleCulling, frontTriangleCounterClockwise, mask, reuseBounds, id);
	}

	[Obsolete("This AddInstance method has been deprecated and will be removed in a future version. Please use the alternate method for adding procedural geometry (AABBs) to the acceleration structure.", false)]
	public void AddInstance(GraphicsBuffer aabbBuffer, uint numElements, Material material, Matrix4x4 instanceTransform, bool isCutOff, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, bool reuseBounds = false, uint id = uint.MaxValue)
	{
		AddInstance_Procedural_Deprecated(aabbBuffer, numElements, material, instanceTransform, isCutOff, enableTriangleCulling, frontTriangleCounterClockwise, mask, reuseBounds, id);
	}

	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::Build", HasExplicitThis = true)]
	public void Build(Vector3 relativeOrigin)
	{
		Build_Injected(ref relativeOrigin);
	}

	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::AddInstanceDeprecated", HasExplicitThis = true)]
	private void AddInstance_Procedural_Deprecated([NotNull("ArgumentNullException")] GraphicsBuffer aabbBuffer, uint numElements, [NotNull("ArgumentNullException")] Material material, Matrix4x4 instanceTransform, bool isCutOff, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, bool reuseBounds = false, uint id = uint.MaxValue)
	{
		AddInstance_Procedural_Deprecated_Injected(aabbBuffer, numElements, material, ref instanceTransform, isCutOff, enableTriangleCulling, frontTriangleCounterClockwise, mask, reuseBounds, id);
	}

	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::AddInstance", HasExplicitThis = true)]
	private int AddInstance_Procedural([NotNull("ArgumentNullException")] GraphicsBuffer aabbBuffer, uint aabbCount, bool dynamicData, Matrix4x4 matrix, [NotNull("ArgumentNullException")] Material material, bool opaqueMaterial, MaterialPropertyBlock properties, uint mask = 255u, uint id = uint.MaxValue)
	{
		return AddInstance_Procedural_Injected(aabbBuffer, aabbCount, dynamicData, ref matrix, material, opaqueMaterial, properties, mask, id);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::RemoveInstance", HasExplicitThis = true)]
	private extern void RemoveInstance_Renderer([NotNull("ArgumentNullException")] Renderer targetRenderer);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::RemoveInstance", HasExplicitThis = true)]
	private extern void RemoveInstance_InstanceID(int instanceID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::UpdateInstanceTransform", HasExplicitThis = true)]
	private extern void UpdateInstanceTransform_Renderer([NotNull("ArgumentNullException")] Renderer renderer);

	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::UpdateInstanceTransform", HasExplicitThis = true)]
	private void UpdateInstanceTransform_InstanceID(int instanceID, Matrix4x4 matrix)
	{
		UpdateInstanceTransform_InstanceID_Injected(instanceID, ref matrix);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::UpdateInstanceMask", HasExplicitThis = true)]
	public extern void UpdateInstanceMask([NotNull("ArgumentNullException")] Renderer renderer, uint mask);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::UpdateInstanceID", HasExplicitThis = true)]
	public extern void UpdateInstanceID([NotNull("ArgumentNullException")] Renderer renderer, uint instanceID);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::UpdateInstancePropertyBlock", HasExplicitThis = true)]
	public extern void UpdateInstancePropertyBlock(int handle, MaterialPropertyBlock properties);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::GetSize", HasExplicitThis = true)]
	public extern ulong GetSize();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::GetInstanceCount", HasExplicitThis = true)]
	public extern uint GetInstanceCount();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::ClearInstances", HasExplicitThis = true)]
	public extern void ClearInstances();

	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::CullInstances", HasExplicitThis = true)]
	public RayTracingInstanceCullingResults CullInstances(ref RayTracingInstanceCullingConfig cullingConfig)
	{
		CullInstances_Injected(ref cullingConfig, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "RayTracingAccelerationStructure_Bindings::AddInstanceSubMeshFlagsArray", HasExplicitThis = true)]
	private extern void AddInstanceSubMeshFlagsArray([NotNull("ArgumentNullException")] Renderer targetRenderer, RayTracingSubMeshFlags[] subMeshFlags, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, uint id = uint.MaxValue);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern IntPtr Create_Injected(ref RASSettings desc);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Update_Injected(ref Vector3 relativeOrigin);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void Build_Injected(ref Vector3 relativeOrigin);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddInstance_Procedural_Deprecated_Injected(GraphicsBuffer aabbBuffer, uint numElements, Material material, ref Matrix4x4 instanceTransform, bool isCutOff, bool enableTriangleCulling = true, bool frontTriangleCounterClockwise = false, uint mask = 255u, bool reuseBounds = false, uint id = uint.MaxValue);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern int AddInstance_Procedural_Injected(GraphicsBuffer aabbBuffer, uint aabbCount, bool dynamicData, ref Matrix4x4 matrix, Material material, bool opaqueMaterial, MaterialPropertyBlock properties, uint mask = 255u, uint id = uint.MaxValue);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void UpdateInstanceTransform_InstanceID_Injected(int instanceID, ref Matrix4x4 matrix);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void CullInstances_Injected(ref RayTracingInstanceCullingConfig cullingConfig, out RayTracingInstanceCullingResults ret);
}
