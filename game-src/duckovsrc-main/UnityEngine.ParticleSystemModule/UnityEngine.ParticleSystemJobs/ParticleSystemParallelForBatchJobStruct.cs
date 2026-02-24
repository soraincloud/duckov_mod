using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace UnityEngine.ParticleSystemJobs;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ParticleSystemParallelForBatchJobStruct<T> where T : struct, IJobParticleSystemParallelForBatch
{
	public delegate void ExecuteJobFunction(ref T data, IntPtr listDataPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

	public static readonly BurstLike.SharedStatic<IntPtr> jobReflectionData = BurstLike.SharedStatic<IntPtr>.GetOrCreate<ParticleSystemParallelForBatchJobStruct<T>>();

	[BurstDiscard]
	public static void Initialize()
	{
		if (jobReflectionData.Data == IntPtr.Zero)
		{
			jobReflectionData.Data = JobsUtility.CreateJobReflectionData(typeof(T), new ExecuteJobFunction(Execute));
		}
	}

	public unsafe static void Execute(ref T data, IntPtr listDataPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
	{
		NativeListData* ptr = (NativeListData*)(void*)listDataPtr;
		ParticleSystem.CopyManagedJobData(ptr->system, out var particleData);
		ParticleSystemJobData jobData = new ParticleSystemJobData(ref particleData);
		int beginIndex;
		int endIndex;
		while (JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out beginIndex, out endIndex))
		{
			data.Execute(jobData, beginIndex, endIndex - beginIndex);
		}
	}
}
