using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine;

[NativeHeader("Modules/Subsystems/SubsystemManager.h")]
public static class SubsystemManager
{
	private static List<IntegratedSubsystem> s_IntegratedSubsystems;

	private static List<SubsystemWithProvider> s_StandaloneSubsystems;

	private static List<Subsystem> s_DeprecatedSubsystems;

	public static event Action beforeReloadSubsystems;

	public static event Action afterReloadSubsystems;

	public static event Action reloadSubsytemsStarted;

	public static event Action reloadSubsytemsCompleted;

	[RequiredByNativeCode]
	private static void ReloadSubsystemsStarted()
	{
		if (SubsystemManager.reloadSubsytemsStarted != null)
		{
			SubsystemManager.reloadSubsytemsStarted();
		}
		if (SubsystemManager.beforeReloadSubsystems != null)
		{
			SubsystemManager.beforeReloadSubsystems();
		}
	}

	[RequiredByNativeCode]
	private static void ReloadSubsystemsCompleted()
	{
		if (SubsystemManager.reloadSubsytemsCompleted != null)
		{
			SubsystemManager.reloadSubsytemsCompleted();
		}
		if (SubsystemManager.afterReloadSubsystems != null)
		{
			SubsystemManager.afterReloadSubsystems();
		}
	}

	[RequiredByNativeCode]
	private static void InitializeIntegratedSubsystem(IntPtr ptr, IntegratedSubsystem subsystem)
	{
		subsystem.m_Ptr = ptr;
		subsystem.SetHandle(subsystem);
		s_IntegratedSubsystems.Add(subsystem);
	}

	[RequiredByNativeCode]
	private static void ClearSubsystems()
	{
		foreach (IntegratedSubsystem s_IntegratedSubsystem in s_IntegratedSubsystems)
		{
			s_IntegratedSubsystem.m_Ptr = IntPtr.Zero;
		}
		s_IntegratedSubsystems.Clear();
		s_StandaloneSubsystems.Clear();
		s_DeprecatedSubsystems.Clear();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void StaticConstructScriptingClassMap();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void ReportSingleSubsystemAnalytics(string id);

	static SubsystemManager()
	{
		s_IntegratedSubsystems = new List<IntegratedSubsystem>();
		s_StandaloneSubsystems = new List<SubsystemWithProvider>();
		s_DeprecatedSubsystems = new List<Subsystem>();
		StaticConstructScriptingClassMap();
	}

	public static void GetAllSubsystemDescriptors(List<ISubsystemDescriptor> descriptors)
	{
		SubsystemDescriptorStore.GetAllSubsystemDescriptors(descriptors);
	}

	public static void GetSubsystemDescriptors<T>(List<T> descriptors) where T : ISubsystemDescriptor
	{
		SubsystemDescriptorStore.GetSubsystemDescriptors(descriptors);
	}

	public static void GetSubsystems<T>(List<T> subsystems) where T : ISubsystem
	{
		subsystems.Clear();
		AddSubsystemSubset(s_IntegratedSubsystems, subsystems);
		AddSubsystemSubset(s_StandaloneSubsystems, subsystems);
		AddSubsystemSubset(s_DeprecatedSubsystems, subsystems);
	}

	private static void AddSubsystemSubset<TBaseTypeInList, TQueryType>(List<TBaseTypeInList> copyFrom, List<TQueryType> copyTo) where TBaseTypeInList : ISubsystem where TQueryType : ISubsystem
	{
		foreach (TBaseTypeInList item2 in copyFrom)
		{
			if (item2 is TQueryType item)
			{
				copyTo.Add(item);
			}
		}
	}

	internal static IntegratedSubsystem GetIntegratedSubsystemByPtr(IntPtr ptr)
	{
		foreach (IntegratedSubsystem s_IntegratedSubsystem in s_IntegratedSubsystems)
		{
			if (s_IntegratedSubsystem.m_Ptr == ptr)
			{
				return s_IntegratedSubsystem;
			}
		}
		return null;
	}

	internal static void RemoveIntegratedSubsystemByPtr(IntPtr ptr)
	{
		for (int i = 0; i < s_IntegratedSubsystems.Count; i++)
		{
			if (!(s_IntegratedSubsystems[i].m_Ptr != ptr))
			{
				s_IntegratedSubsystems[i].m_Ptr = IntPtr.Zero;
				s_IntegratedSubsystems.RemoveAt(i);
				break;
			}
		}
	}

	internal static void AddStandaloneSubsystem(SubsystemWithProvider subsystem)
	{
		s_StandaloneSubsystems.Add(subsystem);
	}

	internal static bool RemoveStandaloneSubsystem(SubsystemWithProvider subsystem)
	{
		return s_StandaloneSubsystems.Remove(subsystem);
	}

	internal static SubsystemWithProvider FindStandaloneSubsystemByDescriptor(SubsystemDescriptorWithProvider descriptor)
	{
		foreach (SubsystemWithProvider s_StandaloneSubsystem in s_StandaloneSubsystems)
		{
			if (s_StandaloneSubsystem.descriptor == descriptor)
			{
				return s_StandaloneSubsystem;
			}
		}
		return null;
	}

	public static void GetInstances<T>(List<T> subsystems) where T : ISubsystem
	{
		GetSubsystems(subsystems);
	}

	internal static void AddDeprecatedSubsystem(Subsystem subsystem)
	{
		s_DeprecatedSubsystems.Add(subsystem);
	}

	internal static bool RemoveDeprecatedSubsystem(Subsystem subsystem)
	{
		return s_DeprecatedSubsystems.Remove(subsystem);
	}

	internal static Subsystem FindDeprecatedSubsystemByDescriptor(SubsystemDescriptor descriptor)
	{
		foreach (Subsystem s_DeprecatedSubsystem in s_DeprecatedSubsystems)
		{
			if (s_DeprecatedSubsystem.m_SubsystemDescriptor == descriptor)
			{
				return s_DeprecatedSubsystem;
			}
		}
		return null;
	}
}
