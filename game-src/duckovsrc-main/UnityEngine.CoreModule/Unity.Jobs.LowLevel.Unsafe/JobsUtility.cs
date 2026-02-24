using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.Jobs.LowLevel.Unsafe;

[NativeType(Header = "Runtime/Jobs/ScriptBindings/JobsBindings.h")]
[NativeHeader("Runtime/Jobs/JobSystem.h")]
public static class JobsUtility
{
	public struct JobScheduleParameters
	{
		public JobHandle Dependency;

		public int ScheduleMode;

		public IntPtr ReflectionData;

		public IntPtr JobDataPtr;

		public unsafe JobScheduleParameters(void* i_jobData, IntPtr i_reflectionData, JobHandle i_dependency, ScheduleMode i_scheduleMode)
		{
			Dependency = i_dependency;
			JobDataPtr = (IntPtr)i_jobData;
			ReflectionData = i_reflectionData;
			ScheduleMode = (int)i_scheduleMode;
		}
	}

	internal delegate void PanicFunction_();

	public const int MaxJobThreadCount = 128;

	public const int CacheLineSize = 64;

	internal static PanicFunction_ PanicFunction;

	public static extern bool IsExecutingJob
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(IsFreeFunction = true, IsThreadSafe = true)]
		get;
	}

	public static extern bool JobDebuggerEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction]
		set;
	}

	public static extern bool JobCompilerEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction]
		set;
	}

	public static extern int JobWorkerMaximumCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("JobSystem::GetJobQueueMaximumThreadCount")]
		get;
	}

	public static int JobWorkerCount
	{
		get
		{
			return GetJobQueueWorkerThreadCount();
		}
		set
		{
			if (value < 0 || value > JobWorkerMaximumCount)
			{
				throw new ArgumentOutOfRangeException("JobWorkerCount", $"Invalid JobWorkerCount {value} must be in the range 0 -> {JobWorkerMaximumCount}");
			}
			SetJobQueueMaximumActiveThreadCount(value);
		}
	}

	public static extern int ThreadIndex
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetJobWorkerIndex", IsThreadSafe = true)]
		[BurstAuthorizedExternalMethod]
		get;
	}

	public static extern int ThreadIndexCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetJobWorkerIndexCount", IsThreadSafe = true)]
		[BurstAuthorizedExternalMethod]
		get;
	}

	internal static bool JobBatchingEnabled => GetJobBatchingEnabled();

	public unsafe static void GetJobRange(ref JobRanges ranges, int jobIndex, out int beginIndex, out int endIndex)
	{
		int* ptr = (int*)(void*)ranges.StartEndIndex;
		beginIndex = ptr[jobIndex * 2];
		endIndex = ptr[jobIndex * 2 + 1];
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsFreeFunction = true, IsThreadSafe = true)]
	public static extern bool GetWorkStealingRange(ref JobRanges ranges, int jobIndex, out int beginIndex, out int endIndex);

	[FreeFunction("ScheduleManagedJob", ThrowsException = true, IsThreadSafe = true)]
	public static JobHandle Schedule(ref JobScheduleParameters parameters)
	{
		Schedule_Injected(ref parameters, out var ret);
		return ret;
	}

	[FreeFunction("ScheduleManagedJobParallelFor", ThrowsException = true, IsThreadSafe = true)]
	public static JobHandle ScheduleParallelFor(ref JobScheduleParameters parameters, int arrayLength, int innerloopBatchCount)
	{
		ScheduleParallelFor_Injected(ref parameters, arrayLength, innerloopBatchCount, out var ret);
		return ret;
	}

	[FreeFunction("ScheduleManagedJobParallelForDeferArraySize", ThrowsException = true, IsThreadSafe = true)]
	public unsafe static JobHandle ScheduleParallelForDeferArraySize(ref JobScheduleParameters parameters, int innerloopBatchCount, void* listData, void* listDataAtomicSafetyHandle)
	{
		ScheduleParallelForDeferArraySize_Injected(ref parameters, innerloopBatchCount, listData, listDataAtomicSafetyHandle, out var ret);
		return ret;
	}

	[FreeFunction("ScheduleManagedJobParallelForTransform", ThrowsException = true)]
	public static JobHandle ScheduleParallelForTransform(ref JobScheduleParameters parameters, IntPtr transfromAccesssArray)
	{
		ScheduleParallelForTransform_Injected(ref parameters, transfromAccesssArray, out var ret);
		return ret;
	}

	[FreeFunction("ScheduleManagedJobParallelForTransformReadOnly", ThrowsException = true)]
	public static JobHandle ScheduleParallelForTransformReadOnly(ref JobScheduleParameters parameters, IntPtr transfromAccesssArray, int innerloopBatchCount)
	{
		ScheduleParallelForTransformReadOnly_Injected(ref parameters, transfromAccesssArray, innerloopBatchCount, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true, IsFreeFunction = true)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	public unsafe static extern void PatchBufferMinMaxRanges(IntPtr bufferRangePatchData, void* jobdata, int startIndex, int rangeSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(ThrowsException = true, IsThreadSafe = true)]
	private static extern IntPtr CreateJobReflectionData(Type wrapperJobType, Type userJobType, object managedJobFunction0, object managedJobFunction1, object managedJobFunction2);

	[Obsolete("JobType is obsolete. The parameter should be removed. (UnityUpgradable) -> !1")]
	public static IntPtr CreateJobReflectionData(Type type, JobType jobType, object managedJobFunction0, object managedJobFunction1 = null, object managedJobFunction2 = null)
	{
		return CreateJobReflectionData(type, type, managedJobFunction0, managedJobFunction1, managedJobFunction2);
	}

	public static IntPtr CreateJobReflectionData(Type type, object managedJobFunction0, object managedJobFunction1 = null, object managedJobFunction2 = null)
	{
		return CreateJobReflectionData(type, type, managedJobFunction0, managedJobFunction1, managedJobFunction2);
	}

	[Obsolete("JobType is obsolete. The parameter should be removed. (UnityUpgradable) -> !2")]
	public static IntPtr CreateJobReflectionData(Type wrapperJobType, Type userJobType, JobType jobType, object managedJobFunction0)
	{
		return CreateJobReflectionData(wrapperJobType, userJobType, managedJobFunction0, null, null);
	}

	public static IntPtr CreateJobReflectionData(Type wrapperJobType, Type userJobType, object managedJobFunction0)
	{
		return CreateJobReflectionData(wrapperJobType, userJobType, managedJobFunction0, null, null);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("JobSystem::GetJobQueueWorkerThreadCount")]
	private static extern int GetJobQueueWorkerThreadCount();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("JobSystem::ForceSetJobQueueWorkerThreadCount")]
	private static extern void SetJobQueueMaximumActiveThreadCount(int count);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("JobSystem::ResetJobQueueWorkerThreadCount")]
	public static extern void ResetJobWorkerCount();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("IsJobQueueBatchingEnabled")]
	private static extern bool GetJobBatchingEnabled();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("JobDebuggerGetSystemIdCellPtr")]
	internal static extern IntPtr GetSystemIdCellPtr();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("JobDebuggerClearSystemIds")]
	internal static extern void ClearSystemIds();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("JobDebuggerGetSystemIdMappings")]
	internal unsafe static extern int GetSystemIdMappings(JobHandle* handles, int* systemIds, int maxCount);

	[RequiredByNativeCode]
	private static void InvokePanicFunction()
	{
		PanicFunction?.Invoke();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Schedule_Injected(ref JobScheduleParameters parameters, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ScheduleParallelFor_Injected(ref JobScheduleParameters parameters, int arrayLength, int innerloopBatchCount, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ScheduleParallelForDeferArraySize_Injected(ref JobScheduleParameters parameters, int innerloopBatchCount, void* listData, void* listDataAtomicSafetyHandle, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ScheduleParallelForTransform_Injected(ref JobScheduleParameters parameters, IntPtr transfromAccesssArray, out JobHandle ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ScheduleParallelForTransformReadOnly_Injected(ref JobScheduleParameters parameters, IntPtr transfromAccesssArray, int innerloopBatchCount, out JobHandle ret);
}
