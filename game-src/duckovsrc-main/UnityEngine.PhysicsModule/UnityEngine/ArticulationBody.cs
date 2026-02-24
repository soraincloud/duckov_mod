using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;

namespace UnityEngine;

[NativeClass("Unity::ArticulationBody")]
[NativeHeader("Modules/Physics/ArticulationBody.h")]
public class ArticulationBody : Behaviour
{
	public extern ArticulationJointType jointType
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector3 anchorPosition
	{
		get
		{
			get_anchorPosition_Injected(out var ret);
			return ret;
		}
		set
		{
			set_anchorPosition_Injected(ref value);
		}
	}

	public Vector3 parentAnchorPosition
	{
		get
		{
			get_parentAnchorPosition_Injected(out var ret);
			return ret;
		}
		set
		{
			set_parentAnchorPosition_Injected(ref value);
		}
	}

	public Quaternion anchorRotation
	{
		get
		{
			get_anchorRotation_Injected(out var ret);
			return ret;
		}
		set
		{
			set_anchorRotation_Injected(ref value);
		}
	}

	public Quaternion parentAnchorRotation
	{
		get
		{
			get_parentAnchorRotation_Injected(out var ret);
			return ret;
		}
		set
		{
			set_parentAnchorRotation_Injected(ref value);
		}
	}

	public extern bool isRoot
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern bool matchAnchors
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ArticulationDofLock linearLockX
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ArticulationDofLock linearLockY
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ArticulationDofLock linearLockZ
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ArticulationDofLock swingYLock
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ArticulationDofLock swingZLock
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern ArticulationDofLock twistLock
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public ArticulationDrive xDrive
	{
		get
		{
			get_xDrive_Injected(out var ret);
			return ret;
		}
		set
		{
			set_xDrive_Injected(ref value);
		}
	}

	public ArticulationDrive yDrive
	{
		get
		{
			get_yDrive_Injected(out var ret);
			return ret;
		}
		set
		{
			set_yDrive_Injected(ref value);
		}
	}

	public ArticulationDrive zDrive
	{
		get
		{
			get_zDrive_Injected(out var ret);
			return ret;
		}
		set
		{
			set_zDrive_Injected(ref value);
		}
	}

	public extern bool immovable
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool useGravity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float linearDamping
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float angularDamping
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float jointFriction
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public LayerMask excludeLayers
	{
		get
		{
			get_excludeLayers_Injected(out var ret);
			return ret;
		}
		set
		{
			set_excludeLayers_Injected(ref value);
		}
	}

	public LayerMask includeLayers
	{
		get
		{
			get_includeLayers_Injected(out var ret);
			return ret;
		}
		set
		{
			set_includeLayers_Injected(ref value);
		}
	}

	public Vector3 velocity
	{
		get
		{
			get_velocity_Injected(out var ret);
			return ret;
		}
		set
		{
			set_velocity_Injected(ref value);
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			get_angularVelocity_Injected(out var ret);
			return ret;
		}
		set
		{
			set_angularVelocity_Injected(ref value);
		}
	}

	public extern float mass
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern bool automaticCenterOfMass
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector3 centerOfMass
	{
		get
		{
			get_centerOfMass_Injected(out var ret);
			return ret;
		}
		set
		{
			set_centerOfMass_Injected(ref value);
		}
	}

	public Vector3 worldCenterOfMass
	{
		get
		{
			get_worldCenterOfMass_Injected(out var ret);
			return ret;
		}
	}

	public extern bool automaticInertiaTensor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public Vector3 inertiaTensor
	{
		get
		{
			get_inertiaTensor_Injected(out var ret);
			return ret;
		}
		set
		{
			set_inertiaTensor_Injected(ref value);
		}
	}

	public Quaternion inertiaTensorRotation
	{
		get
		{
			get_inertiaTensorRotation_Injected(out var ret);
			return ret;
		}
		set
		{
			set_inertiaTensorRotation_Injected(ref value);
		}
	}

	public extern float sleepThreshold
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int solverIterations
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern int solverVelocityIterations
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float maxAngularVelocity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float maxLinearVelocity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float maxJointVelocity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public extern float maxDepenetrationVelocity
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public ArticulationReducedSpace jointPosition
	{
		get
		{
			get_jointPosition_Injected(out var ret);
			return ret;
		}
		set
		{
			set_jointPosition_Injected(ref value);
		}
	}

	public ArticulationReducedSpace jointVelocity
	{
		get
		{
			get_jointVelocity_Injected(out var ret);
			return ret;
		}
		set
		{
			set_jointVelocity_Injected(ref value);
		}
	}

	public ArticulationReducedSpace jointAcceleration
	{
		get
		{
			get_jointAcceleration_Injected(out var ret);
			return ret;
		}
		[Obsolete("Setting joint accelerations is not supported in forward kinematics. To have inverse dynamics take acceleration into account, use GetJointForcesForAcceleration instead", true)]
		set
		{
			set_jointAcceleration_Injected(ref value);
		}
	}

	public ArticulationReducedSpace jointForce
	{
		get
		{
			get_jointForce_Injected(out var ret);
			return ret;
		}
		set
		{
			set_jointForce_Injected(ref value);
		}
	}

	public ArticulationReducedSpace driveForce
	{
		get
		{
			get_driveForce_Injected(out var ret);
			return ret;
		}
	}

	public extern int dofCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public extern int index
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("GetBodyIndex")]
		get;
	}

	public extern CollisionDetectionMode collisionDetectionMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[Obsolete("computeParentAnchor has been renamed to matchAnchors (UnityUpgradable) -> matchAnchors")]
	public bool computeParentAnchor
	{
		get
		{
			return matchAnchors;
		}
		set
		{
			matchAnchors = value;
		}
	}

	public Vector3 GetAccumulatedForce([DefaultValue("Time.fixedDeltaTime")] float step)
	{
		GetAccumulatedForce_Injected(step, out var ret);
		return ret;
	}

	[ExcludeFromDocs]
	public Vector3 GetAccumulatedForce()
	{
		return GetAccumulatedForce(Time.fixedDeltaTime);
	}

	public Vector3 GetAccumulatedTorque([DefaultValue("Time.fixedDeltaTime")] float step)
	{
		GetAccumulatedTorque_Injected(step, out var ret);
		return ret;
	}

	[ExcludeFromDocs]
	public Vector3 GetAccumulatedTorque()
	{
		return GetAccumulatedTorque(Time.fixedDeltaTime);
	}

	public void AddForce(Vector3 force, [DefaultValue("ForceMode.Force")] ForceMode mode)
	{
		AddForce_Injected(ref force, mode);
	}

	[ExcludeFromDocs]
	public void AddForce(Vector3 force)
	{
		AddForce(force, ForceMode.Force);
	}

	public void AddRelativeForce(Vector3 force, [DefaultValue("ForceMode.Force")] ForceMode mode)
	{
		AddRelativeForce_Injected(ref force, mode);
	}

	[ExcludeFromDocs]
	public void AddRelativeForce(Vector3 force)
	{
		AddRelativeForce(force, ForceMode.Force);
	}

	public void AddTorque(Vector3 torque, [DefaultValue("ForceMode.Force")] ForceMode mode)
	{
		AddTorque_Injected(ref torque, mode);
	}

	[ExcludeFromDocs]
	public void AddTorque(Vector3 torque)
	{
		AddTorque(torque, ForceMode.Force);
	}

	public void AddRelativeTorque(Vector3 torque, [DefaultValue("ForceMode.Force")] ForceMode mode)
	{
		AddRelativeTorque_Injected(ref torque, mode);
	}

	[ExcludeFromDocs]
	public void AddRelativeTorque(Vector3 torque)
	{
		AddRelativeTorque(torque, ForceMode.Force);
	}

	public void AddForceAtPosition(Vector3 force, Vector3 position, [DefaultValue("ForceMode.Force")] ForceMode mode)
	{
		AddForceAtPosition_Injected(ref force, ref position, mode);
	}

	[ExcludeFromDocs]
	public void AddForceAtPosition(Vector3 force, Vector3 position)
	{
		AddForceAtPosition(force, position, ForceMode.Force);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void ResetCenterOfMass();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void ResetInertiaTensor();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void Sleep();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern bool IsSleeping();

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void WakeUp();

	public void TeleportRoot(Vector3 position, Quaternion rotation)
	{
		TeleportRoot_Injected(ref position, ref rotation);
	}

	public Vector3 GetClosestPoint(Vector3 point)
	{
		GetClosestPoint_Injected(ref point, out var ret);
		return ret;
	}

	public Vector3 GetRelativePointVelocity(Vector3 relativePoint)
	{
		GetRelativePointVelocity_Injected(ref relativePoint, out var ret);
		return ret;
	}

	public Vector3 GetPointVelocity(Vector3 worldPoint)
	{
		GetPointVelocity_Injected(ref worldPoint, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetDenseJacobian")]
	private extern int GetDenseJacobian_Internal(ref ArticulationJacobian jacobian);

	public int GetDenseJacobian(ref ArticulationJacobian jacobian)
	{
		if (jacobian.elements == null)
		{
			jacobian.elements = new List<float>();
		}
		return GetDenseJacobian_Internal(ref jacobian);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointPositions(List<float> positions);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetJointPositions(List<float> positions);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointVelocities(List<float> velocities);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetJointVelocities(List<float> velocities);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointAccelerations(List<float> accelerations);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointForces(List<float> forces);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetJointForces(List<float> forces);

	public ArticulationReducedSpace GetJointForcesForAcceleration(ArticulationReducedSpace acceleration)
	{
		GetJointForcesForAcceleration_Injected(ref acceleration, out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetDriveForces(List<float> forces);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointGravityForces(List<float> forces);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointCoriolisCentrifugalForces(List<float> forces);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetJointExternalForces(List<float> forces, float step);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetDriveTargets(List<float> targets);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveTargets(List<float> targets);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetDriveTargetVelocities(List<float> targetVelocities);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveTargetVelocities(List<float> targetVelocities);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern int GetDofStartIndices(List<int> dofStartIndices);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveTarget(ArticulationDriveAxis axis, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveTargetVelocity(ArticulationDriveAxis axis, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveLimits(ArticulationDriveAxis axis, float lower, float upper);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveStiffness(ArticulationDriveAxis axis, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveDamping(ArticulationDriveAxis axis, float value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	public extern void SetDriveForceLimit(ArticulationDriveAxis axis, float value);

	public void SnapAnchorToClosestContact()
	{
		if ((bool)base.transform.parent)
		{
			ArticulationBody componentInParent = base.transform.parent.GetComponentInParent<ArticulationBody>();
			while ((bool)componentInParent && !componentInParent.enabled)
			{
				componentInParent = componentInParent.transform.parent.GetComponentInParent<ArticulationBody>();
			}
			if ((bool)componentInParent)
			{
				Vector3 vector = componentInParent.worldCenterOfMass;
				Vector3 closestPoint = GetClosestPoint(vector);
				anchorPosition = base.transform.InverseTransformPoint(closestPoint);
				anchorRotation = Quaternion.FromToRotation(Vector3.right, base.transform.InverseTransformDirection(vector - closestPoint).normalized);
			}
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[Obsolete("Setting joint accelerations is not supported in forward kinematics. To have inverse dynamics take acceleration into account, use GetJointForcesForAcceleration instead", true)]
	public extern void SetJointAccelerations(List<float> accelerations);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_anchorPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_anchorPosition_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_parentAnchorPosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_parentAnchorPosition_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_anchorRotation_Injected(out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_anchorRotation_Injected(ref Quaternion value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_parentAnchorRotation_Injected(out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_parentAnchorRotation_Injected(ref Quaternion value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_xDrive_Injected(out ArticulationDrive ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_xDrive_Injected(ref ArticulationDrive value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_yDrive_Injected(out ArticulationDrive ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_yDrive_Injected(ref ArticulationDrive value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_zDrive_Injected(out ArticulationDrive ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_zDrive_Injected(ref ArticulationDrive value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_excludeLayers_Injected(out LayerMask ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_excludeLayers_Injected(ref LayerMask value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_includeLayers_Injected(out LayerMask ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_includeLayers_Injected(ref LayerMask value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetAccumulatedForce_Injected([DefaultValue("Time.fixedDeltaTime")] float step, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetAccumulatedTorque_Injected([DefaultValue("Time.fixedDeltaTime")] float step, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddForce_Injected(ref Vector3 force, [DefaultValue("ForceMode.Force")] ForceMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddRelativeForce_Injected(ref Vector3 force, [DefaultValue("ForceMode.Force")] ForceMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddTorque_Injected(ref Vector3 torque, [DefaultValue("ForceMode.Force")] ForceMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddRelativeTorque_Injected(ref Vector3 torque, [DefaultValue("ForceMode.Force")] ForceMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void AddForceAtPosition_Injected(ref Vector3 force, ref Vector3 position, [DefaultValue("ForceMode.Force")] ForceMode mode);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_velocity_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_velocity_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_angularVelocity_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_angularVelocity_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_centerOfMass_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_centerOfMass_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_worldCenterOfMass_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_inertiaTensor_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_inertiaTensor_Injected(ref Vector3 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_inertiaTensorRotation_Injected(out Quaternion ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_inertiaTensorRotation_Injected(ref Quaternion value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_jointPosition_Injected(out ArticulationReducedSpace ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_jointPosition_Injected(ref ArticulationReducedSpace value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_jointVelocity_Injected(out ArticulationReducedSpace ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_jointVelocity_Injected(ref ArticulationReducedSpace value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_jointAcceleration_Injected(out ArticulationReducedSpace ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_jointAcceleration_Injected(ref ArticulationReducedSpace value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_jointForce_Injected(out ArticulationReducedSpace ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void set_jointForce_Injected(ref ArticulationReducedSpace value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private extern void get_driveForce_Injected(out ArticulationReducedSpace ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void TeleportRoot_Injected(ref Vector3 position, ref Quaternion rotation);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetClosestPoint_Injected(ref Vector3 point, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetRelativePointVelocity_Injected(ref Vector3 relativePoint, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetPointVelocity_Injected(ref Vector3 worldPoint, out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void GetJointForcesForAcceleration_Injected(ref ArticulationReducedSpace acceleration, out ArticulationReducedSpace ret);
}
