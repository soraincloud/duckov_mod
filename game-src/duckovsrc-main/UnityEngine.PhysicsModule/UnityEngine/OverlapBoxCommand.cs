using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics/BatchCommands/OverlapBoxCommand.h")]
public struct OverlapBoxCommand
{
	public QueryParameters queryParameters;

	public Vector3 center { get; set; }

	public Vector3 halfExtents { get; set; }

	public Quaternion orientation { get; set; }

	public PhysicsScene physicsScene { get; set; }

	public OverlapBoxCommand(Vector3 center, Vector3 halfExtents, Quaternion orientation, QueryParameters queryParameters)
	{
		this.center = center;
		this.halfExtents = halfExtents;
		this.orientation = orientation;
		this.queryParameters = queryParameters;
		physicsScene = Physics.defaultPhysicsScene;
	}

	public OverlapBoxCommand(PhysicsScene physicsScene, Vector3 center, Vector3 halfExtents, Quaternion orientation, QueryParameters queryParameters)
	{
		this.physicsScene = physicsScene;
		this.center = center;
		this.halfExtents = halfExtents;
		this.orientation = orientation;
		this.queryParameters = queryParameters;
	}

	public unsafe static JobHandle ScheduleBatch(NativeArray<OverlapBoxCommand> commands, NativeArray<ColliderHit> results, int minCommandsPerJob, int maxHits, JobHandle dependsOn = default(JobHandle))
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
		BatchQueryJob<OverlapBoxCommand, ColliderHit> output = new BatchQueryJob<OverlapBoxCommand, ColliderHit>(commands, results);
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), BatchQueryJobStruct<BatchQueryJob<OverlapBoxCommand, ColliderHit>>.Initialize(), dependsOn, ScheduleMode.Batched);
		return ScheduleOverlapBoxBatch(ref parameters, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(commands), commands.Length, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results), results.Length, minCommandsPerJob, maxHits);
	}

	[FreeFunction("ScheduleOverlapBoxCommandBatch", ThrowsException = true)]
	private unsafe static JobHandle ScheduleOverlapBoxBatch(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits)
	{
		ScheduleOverlapBoxBatch_Injected(ref parameters, commands, commandLen, result, resultLen, minCommandsPerJob, maxHits, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ScheduleOverlapBoxBatch_Injected(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits, out JobHandle ret);
}
