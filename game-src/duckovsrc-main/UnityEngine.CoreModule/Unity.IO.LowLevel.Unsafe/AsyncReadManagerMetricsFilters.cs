using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.IO.LowLevel.Unsafe;

[StructLayout(LayoutKind.Sequential)]
[NativeConditional("ENABLE_PROFILER")]
[NativeAsStruct]
[RequiredByNativeCode]
public class AsyncReadManagerMetricsFilters
{
	[NativeName("typeIDs")]
	internal ulong[] TypeIDs;

	[NativeName("states")]
	internal ProcessingState[] States;

	[NativeName("readTypes")]
	internal FileReadType[] ReadTypes;

	[NativeName("priorityLevels")]
	internal Priority[] PriorityLevels;

	[NativeName("subsystems")]
	internal AssetLoadingSubsystem[] Subsystems;

	public AsyncReadManagerMetricsFilters()
	{
		ClearFilters();
	}

	public AsyncReadManagerMetricsFilters(ulong typeID)
	{
		ClearFilters();
		SetTypeIDFilter(typeID);
	}

	public AsyncReadManagerMetricsFilters(ProcessingState state)
	{
		ClearFilters();
		SetStateFilter(state);
	}

	public AsyncReadManagerMetricsFilters(FileReadType readType)
	{
		ClearFilters();
		SetReadTypeFilter(readType);
	}

	public AsyncReadManagerMetricsFilters(Priority priorityLevel)
	{
		ClearFilters();
		SetPriorityFilter(priorityLevel);
	}

	public AsyncReadManagerMetricsFilters(AssetLoadingSubsystem subsystem)
	{
		ClearFilters();
		SetSubsystemFilter(subsystem);
	}

	public AsyncReadManagerMetricsFilters(ulong[] typeIDs)
	{
		ClearFilters();
		SetTypeIDFilter(typeIDs);
	}

	public AsyncReadManagerMetricsFilters(ProcessingState[] states)
	{
		ClearFilters();
		SetStateFilter(states);
	}

	public AsyncReadManagerMetricsFilters(FileReadType[] readTypes)
	{
		ClearFilters();
		SetReadTypeFilter(readTypes);
	}

	public AsyncReadManagerMetricsFilters(Priority[] priorityLevels)
	{
		ClearFilters();
		SetPriorityFilter(priorityLevels);
	}

	public AsyncReadManagerMetricsFilters(AssetLoadingSubsystem[] subsystems)
	{
		ClearFilters();
		SetSubsystemFilter(subsystems);
	}

	public AsyncReadManagerMetricsFilters(ulong[] typeIDs, ProcessingState[] states, FileReadType[] readTypes, Priority[] priorityLevels, AssetLoadingSubsystem[] subsystems)
	{
		ClearFilters();
		SetTypeIDFilter(typeIDs);
		SetStateFilter(states);
		SetReadTypeFilter(readTypes);
		SetPriorityFilter(priorityLevels);
		SetSubsystemFilter(subsystems);
	}

	public void SetTypeIDFilter(ulong[] _typeIDs)
	{
		TypeIDs = _typeIDs;
	}

	public void SetStateFilter(ProcessingState[] _states)
	{
		States = _states;
	}

	public void SetReadTypeFilter(FileReadType[] _readTypes)
	{
		ReadTypes = _readTypes;
	}

	public void SetPriorityFilter(Priority[] _priorityLevels)
	{
		PriorityLevels = _priorityLevels;
	}

	public void SetSubsystemFilter(AssetLoadingSubsystem[] _subsystems)
	{
		Subsystems = _subsystems;
	}

	public void SetTypeIDFilter(ulong _typeID)
	{
		TypeIDs = new ulong[1] { _typeID };
	}

	public void SetStateFilter(ProcessingState _state)
	{
		States = new ProcessingState[1] { _state };
	}

	public void SetReadTypeFilter(FileReadType _readType)
	{
		ReadTypes = new FileReadType[1] { _readType };
	}

	public void SetPriorityFilter(Priority _priorityLevel)
	{
		PriorityLevels = new Priority[1] { _priorityLevel };
	}

	public void SetSubsystemFilter(AssetLoadingSubsystem _subsystem)
	{
		Subsystems = new AssetLoadingSubsystem[1] { _subsystem };
	}

	public void RemoveTypeIDFilter()
	{
		TypeIDs = null;
	}

	public void RemoveStateFilter()
	{
		States = null;
	}

	public void RemoveReadTypeFilter()
	{
		ReadTypes = null;
	}

	public void RemovePriorityFilter()
	{
		PriorityLevels = null;
	}

	public void RemoveSubsystemFilter()
	{
		Subsystems = null;
	}

	public void ClearFilters()
	{
		RemoveTypeIDFilter();
		RemoveStateFilter();
		RemoveReadTypeFilter();
		RemovePriorityFilter();
		RemoveSubsystemFilter();
	}
}
