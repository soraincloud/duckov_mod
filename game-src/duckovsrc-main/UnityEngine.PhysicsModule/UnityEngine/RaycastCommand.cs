using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Runtime/Jobs/ScriptBindings/JobsBindingsTypes.h")]
[NativeHeader("Modules/Physics/BatchCommands/RaycastCommand.h")]
public struct RaycastCommand
{
	public QueryParameters queryParameters;

	public Vector3 from { get; set; }

	public Vector3 direction { get; set; }

	public PhysicsScene physicsScene { get; set; }

	public float distance { get; set; }

	[Obsolete("maxHits property was moved to be a part of RaycastCommand.ScheduleBatch.", false)]
	public int maxHits
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}

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

	public RaycastCommand(Vector3 from, Vector3 direction, QueryParameters queryParameters, float distance = float.MaxValue)
	{
		this.from = from;
		this.direction = direction;
		physicsScene = Physics.defaultPhysicsScene;
		this.distance = distance;
		this.queryParameters = queryParameters;
	}

	public RaycastCommand(PhysicsScene physicsScene, Vector3 from, Vector3 direction, QueryParameters queryParameters, float distance = float.MaxValue)
	{
		this.from = from;
		this.direction = direction;
		this.physicsScene = physicsScene;
		this.distance = distance;
		this.queryParameters = queryParameters;
	}

	public unsafe static JobHandle ScheduleBatch(NativeArray<RaycastCommand> commands, NativeArray<RaycastHit> results, int minCommandsPerJob, int maxHits, JobHandle dependsOn = default(JobHandle))
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
		BatchQueryJob<RaycastCommand, RaycastHit> output = new BatchQueryJob<RaycastCommand, RaycastHit>(commands, results);
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), BatchQueryJobStruct<BatchQueryJob<RaycastCommand, RaycastHit>>.Initialize(), dependsOn, ScheduleMode.Batched);
		return ScheduleRaycastBatch(ref parameters, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(commands), commands.Length, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results), results.Length, minCommandsPerJob, maxHits);
	}

	public static JobHandle ScheduleBatch(NativeArray<RaycastCommand> commands, NativeArray<RaycastHit> results, int minCommandsPerJob, JobHandle dependsOn = default(JobHandle))
	{
		return ScheduleBatch(commands, results, minCommandsPerJob, 1, dependsOn);
	}

	[FreeFunction("ScheduleRaycastCommandBatch", ThrowsException = true)]
	private unsafe static JobHandle ScheduleRaycastBatch(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits)
	{
		ScheduleRaycastBatch_Injected(ref parameters, commands, commandLen, result, resultLen, minCommandsPerJob, maxHits, out var ret);
		return ret;
	}

	[Obsolete("This struct signature is no longer supported. Use struct with a QueryParameters instead", false)]
	public RaycastCommand(Vector3 from, Vector3 direction, float distance = float.MaxValue, int layerMask = -5, int maxHits = 1)
	{
		this.from = from;
		this.direction = direction;
		physicsScene = Physics.defaultPhysicsScene;
		queryParameters = QueryParameters.Default;
		this.distance = distance;
		this.layerMask = layerMask;
	}

	[Obsolete("This struct signature is no longer supported. Use struct with a QueryParameters instead", false)]
	public RaycastCommand(PhysicsScene physicsScene, Vector3 from, Vector3 direction, float distance = float.MaxValue, int layerMask = -5, int maxHits = 1)
	{
		this.from = from;
		this.direction = direction;
		this.physicsScene = physicsScene;
		queryParameters = QueryParameters.Default;
		this.distance = distance;
		this.layerMask = layerMask;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ScheduleRaycastBatch_Injected(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits, out JobHandle ret);
}
