using System;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace UnityEngine.ParticleSystemJobs;

public static class IParticleSystemJobExtensions
{
	private static readonly string k_UserJobScheduledOutsideOfCallbackErrorMsg = "Particle System jobs can only be scheduled in MonoBehaviour.OnParticleUpdateJobScheduled()";

	public unsafe static JobHandle Schedule<T>(this T jobData, ParticleSystem ps, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParticleSystem
	{
		if (ParticleSystem.UserJobCanBeScheduled())
		{
			JobsUtility.JobScheduleParameters parameters = ParticleSystemJobUtility.CreateScheduleParams(ref jobData, ps, dependsOn, IJobParticleSystemExtensions.GetReflectionData<T>());
			JobHandle jobHandle = ParticleSystem.ScheduleManagedJob(ref parameters, ps.GetManagedJobData());
			ps.SetManagedJobHandle(jobHandle);
			return jobHandle;
		}
		throw new InvalidOperationException(k_UserJobScheduledOutsideOfCallbackErrorMsg);
	}

	public unsafe static JobHandle Schedule<T>(this T jobData, ParticleSystem ps, int minIndicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParticleSystemParallelFor
	{
		if (ParticleSystem.UserJobCanBeScheduled())
		{
			JobsUtility.JobScheduleParameters parameters = ParticleSystemJobUtility.CreateScheduleParams(ref jobData, ps, dependsOn, IJobParticleSystemParallelForExtensions.GetReflectionData<T>());
			JobHandle jobHandle = JobsUtility.ScheduleParallelForDeferArraySize(ref parameters, minIndicesPerJobCount, ps.GetManagedJobData(), null);
			ps.SetManagedJobHandle(jobHandle);
			return jobHandle;
		}
		throw new InvalidOperationException(k_UserJobScheduledOutsideOfCallbackErrorMsg);
	}

	public unsafe static JobHandle ScheduleBatch<T>(this T jobData, ParticleSystem ps, int innerLoopBatchCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParticleSystemParallelForBatch
	{
		if (ParticleSystem.UserJobCanBeScheduled())
		{
			JobsUtility.JobScheduleParameters parameters = ParticleSystemJobUtility.CreateScheduleParams(ref jobData, ps, dependsOn, IJobParticleSystemParallelForBatchExtensions.GetReflectionData<T>());
			JobHandle jobHandle = JobsUtility.ScheduleParallelForDeferArraySize(ref parameters, innerLoopBatchCount, ps.GetManagedJobData(), null);
			ps.SetManagedJobHandle(jobHandle);
			return jobHandle;
		}
		throw new InvalidOperationException(k_UserJobScheduledOutsideOfCallbackErrorMsg);
	}
}
