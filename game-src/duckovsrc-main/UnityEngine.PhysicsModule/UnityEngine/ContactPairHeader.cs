using System;

namespace UnityEngine;

public readonly struct ContactPairHeader
{
	internal readonly int m_BodyID;

	internal readonly int m_OtherBodyID;

	internal readonly IntPtr m_StartPtr;

	internal readonly uint m_NbPairs;

	internal readonly CollisionPairHeaderFlags m_Flags;

	internal readonly Vector3 m_RelativeVelocity;

	public int BodyInstanceID => m_BodyID;

	public int OtherBodyInstanceID => m_OtherBodyID;

	public Component Body => Physics.GetBodyByInstanceID(m_BodyID);

	public Component OtherBody => Physics.GetBodyByInstanceID(m_OtherBodyID);

	public int PairCount => (int)m_NbPairs;

	internal bool HasRemovedBody => (m_Flags & CollisionPairHeaderFlags.RemovedActor) != 0 || (m_Flags & CollisionPairHeaderFlags.RemovedOtherActor) != 0;

	public unsafe ref readonly ContactPair GetContactPair(int index)
	{
		return ref *GetContactPair_Internal(index);
	}

	internal unsafe ContactPair* GetContactPair_Internal(int index)
	{
		if (index >= m_NbPairs)
		{
			throw new IndexOutOfRangeException("Invalid ContactPair index. Index should be greater than 0 and less than ContactPairHeader.PairCount");
		}
		return (ContactPair*)(m_StartPtr.ToInt64() + index * sizeof(ContactPair));
	}
}
