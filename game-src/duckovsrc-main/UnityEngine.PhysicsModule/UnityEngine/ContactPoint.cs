using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
[NativeHeader("Modules/Physics/MessageParameters.h")]
public struct ContactPoint
{
	internal Vector3 m_Point;

	internal Vector3 m_Normal;

	internal Vector3 m_Impulse;

	internal int m_ThisColliderInstanceID;

	internal int m_OtherColliderInstanceID;

	internal float m_Separation;

	public Vector3 point => m_Point;

	public Vector3 normal => m_Normal;

	public Vector3 impulse => m_Impulse;

	public Collider thisCollider => Physics.GetColliderByInstanceID(m_ThisColliderInstanceID);

	public Collider otherCollider => Physics.GetColliderByInstanceID(m_OtherColliderInstanceID);

	public float separation => m_Separation;

	internal ContactPoint(Vector3 point, Vector3 normal, Vector3 impulse, float separation, int thisInstanceID, int otherInstenceID)
	{
		m_Point = point;
		m_Normal = normal;
		m_Impulse = impulse;
		m_Separation = separation;
		m_ThisColliderInstanceID = thisInstanceID;
		m_OtherColliderInstanceID = otherInstenceID;
	}
}
