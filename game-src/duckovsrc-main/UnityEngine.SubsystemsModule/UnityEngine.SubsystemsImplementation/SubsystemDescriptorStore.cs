using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.SubsystemsImplementation;

[NativeHeader("Modules/Subsystems/SubsystemManager.h")]
public static class SubsystemDescriptorStore
{
	private static List<IntegratedSubsystemDescriptor> s_IntegratedDescriptors = new List<IntegratedSubsystemDescriptor>();

	private static List<SubsystemDescriptorWithProvider> s_StandaloneDescriptors = new List<SubsystemDescriptorWithProvider>();

	private static List<SubsystemDescriptor> s_DeprecatedDescriptors = new List<SubsystemDescriptor>();

	[RequiredByNativeCode]
	internal static void InitializeManagedDescriptor(IntPtr ptr, IntegratedSubsystemDescriptor desc)
	{
		desc.m_Ptr = ptr;
		s_IntegratedDescriptors.Add(desc);
	}

	[RequiredByNativeCode]
	internal static void ClearManagedDescriptors()
	{
		foreach (IntegratedSubsystemDescriptor s_IntegratedDescriptor in s_IntegratedDescriptors)
		{
			s_IntegratedDescriptor.m_Ptr = IntPtr.Zero;
		}
		s_IntegratedDescriptors.Clear();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ReportSingleSubsystemAnalytics(string id);

	public static void RegisterDescriptor(SubsystemDescriptorWithProvider descriptor)
	{
		descriptor.ThrowIfInvalid();
		RegisterDescriptor(descriptor, s_StandaloneDescriptors);
	}

	internal static void GetAllSubsystemDescriptors(List<ISubsystemDescriptor> descriptors)
	{
		descriptors.Clear();
		int num = s_IntegratedDescriptors.Count + s_StandaloneDescriptors.Count + s_DeprecatedDescriptors.Count;
		if (descriptors.Capacity < num)
		{
			descriptors.Capacity = num;
		}
		AddDescriptorSubset(s_IntegratedDescriptors, descriptors);
		AddDescriptorSubset(s_StandaloneDescriptors, descriptors);
		AddDescriptorSubset(s_DeprecatedDescriptors, descriptors);
	}

	private static void AddDescriptorSubset<TBaseTypeInList>(List<TBaseTypeInList> copyFrom, List<ISubsystemDescriptor> copyTo) where TBaseTypeInList : ISubsystemDescriptor
	{
		foreach (TBaseTypeInList item in copyFrom)
		{
			copyTo.Add(item);
		}
	}

	internal static void GetSubsystemDescriptors<T>(List<T> descriptors) where T : ISubsystemDescriptor
	{
		descriptors.Clear();
		AddDescriptorSubset(s_IntegratedDescriptors, descriptors);
		AddDescriptorSubset(s_StandaloneDescriptors, descriptors);
		AddDescriptorSubset(s_DeprecatedDescriptors, descriptors);
	}

	private static void AddDescriptorSubset<TBaseTypeInList, TQueryType>(List<TBaseTypeInList> copyFrom, List<TQueryType> copyTo) where TBaseTypeInList : ISubsystemDescriptor where TQueryType : ISubsystemDescriptor
	{
		foreach (TBaseTypeInList item2 in copyFrom)
		{
			if (item2 is TQueryType item)
			{
				copyTo.Add(item);
			}
		}
	}

	internal static void RegisterDescriptor<TDescriptor, TBaseTypeInList>(TDescriptor descriptor, List<TBaseTypeInList> storeInList) where TDescriptor : TBaseTypeInList where TBaseTypeInList : ISubsystemDescriptor
	{
		for (int i = 0; i < storeInList.Count; i++)
		{
			if (!(storeInList[i].id != descriptor.id))
			{
				Debug.LogWarning("Registering subsystem descriptor with duplicate ID '" + descriptor.id + "' - overwriting previous entry.");
				storeInList[i] = (TBaseTypeInList)(object)descriptor;
				return;
			}
		}
		ReportSingleSubsystemAnalytics(descriptor.id);
		storeInList.Add((TBaseTypeInList)(object)descriptor);
	}

	internal static void RegisterDeprecatedDescriptor(SubsystemDescriptor descriptor)
	{
		RegisterDescriptor(descriptor, s_DeprecatedDescriptors);
	}
}
