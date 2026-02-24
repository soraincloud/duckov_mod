using System;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[UsedByNativeCode("SubsystemDescriptorBase")]
public abstract class IntegratedSubsystemDescriptor : ISubsystemDescriptorImpl, ISubsystemDescriptor
{
	internal IntPtr m_Ptr;

	public string id => SubsystemDescriptorBindings.GetId(m_Ptr);

	IntPtr ISubsystemDescriptorImpl.ptr
	{
		get
		{
			return m_Ptr;
		}
		set
		{
			m_Ptr = value;
		}
	}

	ISubsystem ISubsystemDescriptor.Create()
	{
		return CreateImpl();
	}

	internal abstract ISubsystem CreateImpl();
}
[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/Subsystems/SubsystemDescriptor.h")]
[UsedByNativeCode("SubsystemDescriptor")]
public class IntegratedSubsystemDescriptor<TSubsystem> : IntegratedSubsystemDescriptor where TSubsystem : IntegratedSubsystem
{
	internal override ISubsystem CreateImpl()
	{
		return Create();
	}

	public TSubsystem Create()
	{
		IntPtr ptr = SubsystemDescriptorBindings.Create(m_Ptr);
		TSubsystem val = (TSubsystem)SubsystemManager.GetIntegratedSubsystemByPtr(ptr);
		if (val != null)
		{
			val.m_SubsystemDescriptor = this;
		}
		return val;
	}
}
