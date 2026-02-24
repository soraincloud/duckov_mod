namespace UnityEngine;

public readonly struct ContactPairPoint
{
	internal readonly Vector3 m_Position;

	internal readonly float m_Separation;

	internal readonly Vector3 m_Normal;

	internal readonly uint m_InternalFaceIndex0;

	internal readonly Vector3 m_Impulse;

	internal readonly uint m_InternalFaceIndex1;

	public Vector3 Position => m_Position;

	public float Separation => m_Separation;

	public Vector3 Normal => m_Normal;

	public Vector3 Impulse => m_Impulse;
}
