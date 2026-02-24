using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/Subsystems/Subsystem.h")]
[UsedByNativeCode]
public class IntegratedSubsystem : ISubsystem
{
	internal IntPtr m_Ptr;

	internal ISubsystemDescriptor m_SubsystemDescriptor;

	public bool running => valid && IsRunning();

	internal bool valid => m_Ptr != IntPtr.Zero;

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern void SetHandle(IntegratedSubsystem subsystem);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Start();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Stop();

	public void Destroy()
	{
		IntPtr ptr = m_Ptr;
		SubsystemManager.RemoveIntegratedSubsystemByPtr(m_Ptr);
		SubsystemBindings.DestroySubsystem(ptr);
		m_Ptr = IntPtr.Zero;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal extern bool IsRunning();
}
[UsedByNativeCode("Subsystem_TSubsystemDescriptor")]
public class IntegratedSubsystem<TSubsystemDescriptor> : IntegratedSubsystem where TSubsystemDescriptor : ISubsystemDescriptor
{
	public TSubsystemDescriptor subsystemDescriptor => (TSubsystemDescriptor)m_SubsystemDescriptor;

	public TSubsystemDescriptor SubsystemDescriptor => subsystemDescriptor;
}
