using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.VFX;

[StructLayout(LayoutKind.Sequential)]
[NativeType(Header = "Modules/VFX/Public/VFXSpawnerState.h")]
[RequiredByNativeCode]
public sealed class VFXSpawnerState : IDisposable
{
	private IntPtr m_Ptr;

	private bool m_Owner;

	private VFXEventAttribute m_WrapEventAttribute;

	public bool playing
	{
		get
		{
			return loopState == VFXSpawnerLoopState.Looping;
		}
		set
		{
			loopState = (value ? VFXSpawnerLoopState.Looping : VFXSpawnerLoopState.Finished);
		}
	}

	public extern bool newLoop
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern VFXSpawnerLoopState loopState
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float spawnCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float deltaTime
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float totalTime
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float delayBeforeLoop
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float loopDuration
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float delayAfterLoop
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int loopIndex
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int loopCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public VFXEventAttribute vfxEventAttribute
	{
		get
		{
			if (!m_Owner && m_WrapEventAttribute != null)
			{
				return m_WrapEventAttribute;
			}
			return Internal_GetVFXEventAttribute();
		}
	}

	public VFXSpawnerState()
		: this(Internal_Create(), owner: true)
	{
	}

	internal VFXSpawnerState(IntPtr ptr, bool owner)
	{
		m_Ptr = ptr;
		m_Owner = owner;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern IntPtr Internal_Create();

	[RequiredByNativeCode]
	internal static VFXSpawnerState CreateSpawnerStateWrapper()
	{
		VFXSpawnerState vFXSpawnerState = new VFXSpawnerState(IntPtr.Zero, owner: false);
		vFXSpawnerState.PrepareWrapper();
		return vFXSpawnerState;
	}

	private void PrepareWrapper()
	{
		if (m_Owner)
		{
			throw new Exception("VFXSpawnerState : SetWrapValue is reserved to CreateWrapper object");
		}
		if (m_WrapEventAttribute != null)
		{
			throw new Exception("VFXSpawnerState : Unexpected calling twice prepare wrapper");
		}
		m_WrapEventAttribute = VFXEventAttribute.CreateEventAttributeWrapper();
	}

	[RequiredByNativeCode]
	internal void SetWrapValue(IntPtr ptrToSpawnerState, IntPtr ptrToEventAttribute)
	{
		if (m_Owner)
		{
			throw new Exception("VFXSpawnerState : SetWrapValue is reserved to CreateWrapper object");
		}
		if (m_WrapEventAttribute == null)
		{
			throw new Exception("VFXSpawnerState : Missing PrepareWrapper");
		}
		m_Ptr = ptrToSpawnerState;
		m_WrapEventAttribute.SetWrapValue(ptrToEventAttribute);
	}

	internal IntPtr GetPtr()
	{
		return m_Ptr;
	}

	private void Release()
	{
		if (m_Ptr != IntPtr.Zero && m_Owner)
		{
			Internal_Destroy(m_Ptr);
		}
		m_Ptr = IntPtr.Zero;
		m_WrapEventAttribute = null;
	}

	~VFXSpawnerState()
	{
		Release();
	}

	public void Dispose()
	{
		Release();
		GC.SuppressFinalize(this);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true)]
	private static extern void Internal_Destroy(IntPtr ptr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern VFXEventAttribute Internal_GetVFXEventAttribute();
}
