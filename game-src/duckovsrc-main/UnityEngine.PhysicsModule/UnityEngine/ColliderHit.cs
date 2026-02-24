namespace UnityEngine;

public struct ColliderHit
{
	private int m_ColliderInstanceID;

	public int instanceID => m_ColliderInstanceID;

	public Collider collider => Object.FindObjectFromInstanceID(instanceID) as Collider;
}
