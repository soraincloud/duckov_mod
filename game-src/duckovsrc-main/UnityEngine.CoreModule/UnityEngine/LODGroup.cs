using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Runtime/Graphics/LOD/LODGroup.h")]
[StaticAccessor("GetLODGroupManager()", StaticAccessorType.Dot)]
[NativeHeader("Runtime/Graphics/LOD/LODUtility.h")]
[NativeHeader("Runtime/Graphics/LOD/LODGroupManager.h")]
public class LODGroup : Component
{
	public Vector3 localReferencePoint
	{
		get
		{
			get_localReferencePoint_Injected(out var ret);
			return ret;
		}
		set
		{
			set_localReferencePoint_Injected(ref value);
		}
	}

	public extern float size
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int lodCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("GetLODCount")]
		get;
	}

	public extern bool lastLODBillboard
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("GetLastLODIsBillboard")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("SetLastLODIsBillboard")]
		set;
	}

	public extern LODFadeMode fadeMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool animateCrossFading
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool enabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[StaticAccessor("GetLODGroupManager()")]
	public static extern float crossFadeAnimationDuration
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	internal Vector3 worldReferencePoint
	{
		get
		{
			get_worldReferencePoint_Injected(out var ret);
			return ret;
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("UpdateLODGroupBoundingBox", HasExplicitThis = true)]
	public extern void RecalculateBounds();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("GetLODs_Binding", HasExplicitThis = true)]
	public extern LOD[] GetLODs();

	[Obsolete("Use SetLODs instead.")]
	public void SetLODS(LOD[] lods)
	{
		SetLODs(lods);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("SetLODs_Binding", HasExplicitThis = true)]
	public extern void SetLODs([Unmarshalled] LOD[] lods);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ForceLODLevel", HasExplicitThis = true)]
	public extern void ForceLOD(int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_localReferencePoint_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_localReferencePoint_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_worldReferencePoint_Injected(out Vector3 ret);
}
