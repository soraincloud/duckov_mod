using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics/BatchCommands/OverlapCapsuleCommand.h")]
public struct OverlapCapsuleCommand
{
	public QueryParameters queryParameters;

	public Vector3 point0 { get; set; }

	public Vector3 point1 { get; set; }

	public float radius { get; set; }

	public PhysicsScene physicsScene { get; set; }

	public OverlapCapsuleCommand(Vector3 point0, Vector3 point1, float radius, QueryParameters queryParameters)
	{
		this.point0 = point0;
		this.point1 = point1;
		this.radius = radius;
		this.queryParameters = queryParameters;
		physicsScene = Physics.defaultPhysicsScene;
	}

	public OverlapCapsuleCommand(PhysicsScene physicsScene, Vector3 point0, Vector3 point1, float radius, QueryParameters queryParameters)
	{
		this.physicsScene = physicsScene;
		this.point0 = point0;
		this.point1 = point1;
		this.radius = radius;
		this.queryParameters = queryParameters;
	}

	public unsafe static JobHandle ScheduleBatch(NativeArray<OverlapCapsuleCommand> commands, NativeArray<ColliderHit> results, int minCommandsPerJob, int maxHits, JobHandle dependsOn = default(JobHandle))
	{
		if (maxHits < 1)
		{
			Debug.LogWarning("maxHits should be greater than 0.");
			return default(JobHandle);
		}
		if (results.Length < maxHits * commands.Length)
		{
			Debug.LogWarning("The supplied results buffer is too small, there should be at least maxHits space per each command in the batch.");
			return default(JobHandle);
		}
		BatchQueryJob<OverlapCapsuleCommand, ColliderHit> output = new BatchQueryJob<OverlapCapsuleCommand, ColliderHit>(commands, results);
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), BatchQueryJobStruct<BatchQueryJob<OverlapCapsuleCommand, ColliderHit>>.Initialize(), dependsOn, ScheduleMode.Batched);
		return ScheduleOverlapCapsuleBatch(ref parameters, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(commands), commands.Length, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results), results.Length, minCommandsPerJob, maxHits);
	}

	[FreeFunction("ScheduleOverlapCapsuleCommandBatch", ThrowsException = true)]
	private unsafe static JobHandle ScheduleOverlapCapsuleBatch(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits)
	{
		ScheduleOverlapCapsuleBatch_Injected(ref parameters, commands, commandLen, result, resultLen, minCommandsPerJob, maxHits, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ScheduleOverlapCapsuleBatch_Injected(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits, out JobHandle ret);
}
