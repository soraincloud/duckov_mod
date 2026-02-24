using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
public readonly struct ContactPair
{
	private const uint c_InvalidFaceIndex = uint.MaxValue;

	internal readonly int m_ColliderID;

	internal readonly int m_OtherColliderID;

	internal readonly IntPtr m_StartPtr;

	internal readonly uint m_NbPoints;

	internal readonly CollisionPairFlags m_Flags;

	internal readonly CollisionPairEventFlags m_Events;

	internal readonly Vector3 m_ImpulseSum;

	public int ColliderInstanceID => m_ColliderID;

	public int OtherColliderInstanceID => m_OtherColliderID;

	public Collider Collider => (m_ColliderID == 0) ? null : Physics.GetColliderByInstanceID(m_ColliderID);

	public Collider OtherCollider => (m_OtherColliderID == 0) ? null : Physics.GetColliderByInstanceID(m_OtherColliderID);

	public int ContactCount => (int)m_NbPoints;

	public Vector3 ImpulseSum => m_ImpulseSum;

	public bool IsCollisionEnter => (m_Events & CollisionPairEventFlags.NotifyTouchFound) != 0;

	public bool IsCollisionExit => (m_Events & CollisionPairEventFlags.NotifyTouchLost) != 0;

	public bool IsCollisionStay => (m_Events & CollisionPairEventFlags.NotifyTouchPersists) != 0;

	internal bool HasRemovedCollider => (m_Flags & CollisionPairFlags.RemovedShape) != 0 || (m_Flags & CollisionPairFlags.RemovedOtherShape) != 0;

	internal int ExtractContacts(List<ContactPoint> managedContainer, bool flipped)
	{
		return ExtractContacts_Injected(ref this, managedContainer, flipped);
	}

	internal int ExtractContactsArray([Unmarshalled] ContactPoint[] managedContainer, bool flipped)
	{
		return ExtractContactsArray_Injected(ref this, managedContainer, flipped);
	}

	public void CopyToNativeArray(NativeArray<ContactPairPoint> buffer)
	{
		int num = Mathf.Min(buffer.Length, ContactCount);
		for (int i = 0; i < num; i++)
		{
			buffer[i] = GetContactPoint(i);
		}
	}

	public unsafe ref readonly ContactPairPoint GetContactPoint(int index)
	{
		return ref *GetContactPoint_Internal(index);
	}

	public unsafe uint GetContactPointFaceIndex(int contactIndex)
	{
		uint internalFaceIndex = GetContactPoint_Internal(contactIndex)->m_InternalFaceIndex0;
		uint internalFaceIndex2 = GetContactPoint_Internal(contactIndex)->m_InternalFaceIndex1;
		if (internalFaceIndex != uint.MaxValue)
		{
			return Physics.TranslateTriangleIndexFromID(m_ColliderID, internalFaceIndex);
		}
		if (internalFaceIndex2 != uint.MaxValue)
		{
			return Physics.TranslateTriangleIndexFromID(m_OtherColliderID, internalFaceIndex2);
		}
		return uint.MaxValue;
	}

	internal unsafe ContactPairPoint* GetContactPoint_Internal(int index)
	{
		if (index >= m_NbPoints)
		{
			throw new IndexOutOfRangeException("Invalid ContactPairPoint index. Index should be greater than 0 and less than ContactPair.ContactCount");
		}
		return (ContactPairPoint*)(m_StartPtr.ToInt64() + index * sizeof(ContactPairPoint));
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int ExtractContacts_Injected(ref ContactPair _unity_self, List<ContactPoint> managedContainer, bool flipped);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int ExtractContactsArray_Injected(ref ContactPair _unity_self, ContactPoint[] managedContainer, bool flipped);
}
