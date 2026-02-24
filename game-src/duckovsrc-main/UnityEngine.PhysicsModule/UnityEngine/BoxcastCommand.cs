using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics/BatchCommands/BoxcastCommand.h")]
[NativeHeader("Runtime/Jobs/ScriptBindings/JobsBindingsTypes.h")]
public struct BoxcastCommand
{
	public QueryParameters queryParameters;

	public Vector3 center { get; set; }

	public Vector3 halfExtents { get; set; }

	public Quaternion orientation { get; set; }

	public Vector3 direction { get; set; }

	public float distance { get; set; }

	public PhysicsScene physicsScene { get; set; }

	[Obsolete("Layer Mask is now a part of QueryParameters struct", false)]
	public int layerMask
	{
		get
		{
			return queryParameters.layerMask;
		}
		set
		{
			queryParameters.layerMask = value;
		}
	}

	public BoxcastCommand(Vector3 center, Vector3 halfExtents, Quaternion orientation, Vector3 direction, QueryParameters queryParameters, float distance = float.MaxValue)
	{
		this.center = center;
		this.halfExtents = halfExtents;
		this.orientation = orientation;
		this.direction = direction;
		this.distance = distance;
		physicsScene = Physics.defaultPhysicsScene;
		this.queryParameters = queryParameters;
	}

	public BoxcastCommand(PhysicsScene physicsScene, Vector3 center, Vector3 halfExtents, Quaternion orientation, Vector3 direction, QueryParameters queryParameters, float distance = float.MaxValue)
	{
		this.center = center;
		this.halfExtents = halfExtents;
		this.orientation = orientation;
		this.direction = direction;
		this.distance = distance;
		this.physicsScene = physicsScene;
		this.queryParameters = queryParameters;
	}

	public unsafe static JobHandle ScheduleBatch(NativeArray<BoxcastCommand> commands, NativeArray<RaycastHit> results, int minCommandsPerJob, int maxHits, JobHandle dependsOn = default(JobHandle))
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
		BatchQueryJob<BoxcastCommand, RaycastHit> output = new BatchQueryJob<BoxcastCommand, RaycastHit>(commands, results);
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), BatchQueryJobStruct<BatchQueryJob<BoxcastCommand, RaycastHit>>.Initialize(), dependsOn, ScheduleMode.Batched);
		return ScheduleBoxcastBatch(ref parameters, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(commands), commands.Length, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results), results.Length, minCommandsPerJob, maxHits);
	}

	public static JobHandle ScheduleBatch(NativeArray<BoxcastCommand> commands, NativeArray<RaycastHit> results, int minCommandsPerJob, JobHandle dependsOn = default(JobHandle))
	{
		return ScheduleBatch(commands, results, minCommandsPerJob, 1, dependsOn);
	}

	[FreeFunction("ScheduleBoxcastCommandBatch", ThrowsException = true)]
	private unsafe static JobHandle ScheduleBoxcastBatch(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits)
	{
		ScheduleBoxcastBatch_Injected(ref parameters, commands, commandLen, result, resultLen, minCommandsPerJob, maxHits, out var ret);
		return ret;
	}

	[Obsolete("This struct signature is no longer supported. Use struct with a QueryParameters instead", false)]
	public BoxcastCommand(Vector3 center, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance = float.MaxValue, int layerMask = -5)
	{
		this.center = center;
		this.halfExtents = halfExtents;
		this.orientation = orientation;
		this.direction = direction;
		this.distance = distance;
		physicsScene = Physics.defaultPhysicsScene;
		queryParameters = QueryParameters.Default;
		this.layerMask = layerMask;
	}

	[Obsolete("This struct signature is no longer supported. Use struct with a QueryParameters instead", false)]
	public BoxcastCommand(PhysicsScene physicsScene, Vector3 center, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance = float.MaxValue, int layerMask = -5)
	{
		this.center = center;
		this.halfExtents = halfExtents;
		this.orientation = orientation;
		this.direction = direction;
		this.distance = distance;
		this.physicsScene = physicsScene;
		queryParameters = QueryParameters.Default;
		this.layerMask = layerMask;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ScheduleBoxcastBatch_Injected(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits, out JobHandle ret);
}
