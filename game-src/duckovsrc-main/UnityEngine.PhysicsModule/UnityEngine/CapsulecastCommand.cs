using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Physics/BatchCommands/CapsulecastCommand.h")]
[NativeHeader("Runtime/Jobs/ScriptBindings/JobsBindingsTypes.h")]
public struct CapsulecastCommand
{
	public QueryParameters queryParameters;

	public Vector3 point1 { get; set; }

	public Vector3 point2 { get; set; }

	public float radius { get; set; }

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

	public CapsulecastCommand(Vector3 p1, Vector3 p2, float radius, Vector3 direction, QueryParameters queryParameters, float distance = float.MaxValue)
	{
		point1 = p1;
		point2 = p2;
		this.direction = direction;
		this.radius = radius;
		this.distance = distance;
		physicsScene = Physics.defaultPhysicsScene;
		this.queryParameters = queryParameters;
	}

	public CapsulecastCommand(PhysicsScene physicsScene, Vector3 p1, Vector3 p2, float radius, Vector3 direction, QueryParameters queryParameters, float distance = float.MaxValue)
	{
		point1 = p1;
		point2 = p2;
		this.direction = direction;
		this.radius = radius;
		this.distance = distance;
		this.physicsScene = physicsScene;
		this.queryParameters = queryParameters;
	}

	public unsafe static JobHandle ScheduleBatch(NativeArray<CapsulecastCommand> commands, NativeArray<RaycastHit> results, int minCommandsPerJob, int maxHits, JobHandle dependsOn = default(JobHandle))
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
		BatchQueryJob<CapsulecastCommand, RaycastHit> output = new BatchQueryJob<CapsulecastCommand, RaycastHit>(commands, results);
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), BatchQueryJobStruct<BatchQueryJob<CapsulecastCommand, RaycastHit>>.Initialize(), dependsOn, ScheduleMode.Batched);
		return ScheduleCapsulecastBatch(ref parameters, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(commands), commands.Length, NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(results), results.Length, minCommandsPerJob, maxHits);
	}

	public static JobHandle ScheduleBatch(NativeArray<CapsulecastCommand> commands, NativeArray<RaycastHit> results, int minCommandsPerJob, JobHandle dependsOn = default(JobHandle))
	{
		return ScheduleBatch(commands, results, minCommandsPerJob, 1, dependsOn);
	}

	[FreeFunction("ScheduleCapsulecastCommandBatch", ThrowsException = true)]
	private unsafe static JobHandle ScheduleCapsulecastBatch(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits)
	{
		ScheduleCapsulecastBatch_Injected(ref parameters, commands, commandLen, result, resultLen, minCommandsPerJob, maxHits, out var ret);
		return ret;
	}

	[Obsolete("This struct signature is no longer supported. Use struct with a QueryParameters instead", false)]
	public CapsulecastCommand(Vector3 p1, Vector3 p2, float radius, Vector3 direction, float distance = float.MaxValue, int layerMask = -5)
	{
		point1 = p1;
		point2 = p2;
		this.direction = direction;
		this.radius = radius;
		this.distance = distance;
		physicsScene = Physics.defaultPhysicsScene;
		queryParameters = QueryParameters.Default;
		this.layerMask = layerMask;
	}

	[Obsolete("This struct signature is no longer supported. Use struct with a QueryParameters instead", false)]
	public CapsulecastCommand(PhysicsScene physicsScene, Vector3 p1, Vector3 p2, float radius, Vector3 direction, float distance = float.MaxValue, int layerMask = -5)
	{
		point1 = p1;
		point2 = p2;
		this.direction = direction;
		this.radius = radius;
		this.distance = distance;
		this.physicsScene = physicsScene;
		queryParameters = QueryParameters.Default;
		this.layerMask = layerMask;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private unsafe static extern void ScheduleCapsulecastBatch_Injected(ref JobsUtility.JobScheduleParameters parameters, void* commands, int commandLen, void* result, int resultLen, int minCommandsPerJob, int maxHits, out JobHandle ret);
}
