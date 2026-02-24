using System;

namespace UnityEngine;

public struct ModifiableContactPair
{
	private IntPtr actor;

	private IntPtr otherActor;

	private IntPtr shape;

	private IntPtr otherShape;

	public Quaternion rotation;

	public Vector3 position;

	public Quaternion otherRotation;

	public Vector3 otherPosition;

	private int numContacts;

	private IntPtr contacts;

	public int colliderInstanceID => Physics.ResolveShapeToInstanceID(shape);

	public int otherColliderInstanceID => Physics.ResolveShapeToInstanceID(otherShape);

	public int bodyInstanceID => Physics.ResolveActorToInstanceID(actor);

	public int otherBodyInstanceID => Physics.ResolveActorToInstanceID(otherActor);

	public Vector3 bodyVelocity => Physics.GetActorLinearVelocity(actor);

	public Vector3 bodyAngularVelocity => Physics.GetActorAngularVelocity(actor);

	public Vector3 otherBodyVelocity => Physics.GetActorLinearVelocity(otherActor);

	public Vector3 otherBodyAngularVelocity => Physics.GetActorAngularVelocity(otherActor);

	public int contactCount => numContacts;

	public unsafe ModifiableMassProperties massProperties
	{
		get
		{
			return GetContactPatch()->massProperties;
		}
		set
		{
			ModifiableContactPatch* contactPatch = GetContactPatch();
			contactPatch->massProperties = value;
			byte* internalFlags = &contactPatch->internalFlags;
			*internalFlags |= 8;
		}
	}

	public unsafe Vector3 GetPoint(int i)
	{
		return GetContact(i)->contact;
	}

	public unsafe void SetPoint(int i, Vector3 v)
	{
		GetContact(i)->contact = v;
	}

	public unsafe Vector3 GetNormal(int i)
	{
		return GetContact(i)->normal;
	}

	public unsafe void SetNormal(int i, Vector3 normal)
	{
		GetContact(i)->normal = normal;
		byte* internalFlags = &GetContactPatch()->internalFlags;
		*internalFlags |= 0x40;
	}

	public unsafe float GetSeparation(int i)
	{
		return GetContact(i)->separation;
	}

	public unsafe void SetSeparation(int i, float separation)
	{
		GetContact(i)->separation = separation;
	}

	public unsafe Vector3 GetTargetVelocity(int i)
	{
		return GetContact(i)->targetVelocity;
	}

	public unsafe void SetTargetVelocity(int i, Vector3 velocity)
	{
		GetContact(i)->targetVelocity = velocity;
		byte* internalFlags = &GetContactPatch()->internalFlags;
		*internalFlags |= 0x10;
	}

	public unsafe float GetBounciness(int i)
	{
		return GetContact(i)->restitution;
	}

	public unsafe void SetBounciness(int i, float bounciness)
	{
		GetContact(i)->restitution = bounciness;
		byte* internalFlags = &GetContactPatch()->internalFlags;
		*internalFlags |= 0x40;
	}

	public unsafe float GetStaticFriction(int i)
	{
		return GetContact(i)->staticFriction;
	}

	public unsafe void SetStaticFriction(int i, float staticFriction)
	{
		GetContact(i)->staticFriction = staticFriction;
		byte* internalFlags = &GetContactPatch()->internalFlags;
		*internalFlags |= 0x40;
	}

	public unsafe float GetDynamicFriction(int i)
	{
		return GetContact(i)->dynamicFriction;
	}

	public unsafe void SetDynamicFriction(int i, float dynamicFriction)
	{
		GetContact(i)->dynamicFriction = dynamicFriction;
		byte* internalFlags = &GetContactPatch()->internalFlags;
		*internalFlags |= 0x40;
	}

	public unsafe float GetMaxImpulse(int i)
	{
		return GetContact(i)->maxImpulse;
	}

	public unsafe void SetMaxImpulse(int i, float value)
	{
		GetContact(i)->maxImpulse = value;
		byte* internalFlags = &GetContactPatch()->internalFlags;
		*internalFlags |= 0x20;
	}

	public void IgnoreContact(int i)
	{
		SetMaxImpulse(i, 0f);
	}

	public unsafe uint GetFaceIndex(int i)
	{
		if ((GetContactPatch()->internalFlags & 1) != 0)
		{
			IntPtr intPtr = new IntPtr(contacts.ToInt64() + numContacts * sizeof(ModifiableContact) + (numContacts + i) * 4);
			uint rawIndex = *(uint*)(void*)intPtr;
			return Physics.TranslateTriangleIndex(otherShape, rawIndex);
		}
		return uint.MaxValue;
	}

	private unsafe ModifiableContact* GetContact(int index)
	{
		IntPtr intPtr = new IntPtr(contacts.ToInt64() + index * sizeof(ModifiableContact));
		return (ModifiableContact*)(void*)intPtr;
	}

	private unsafe ModifiableContactPatch* GetContactPatch()
	{
		IntPtr intPtr = new IntPtr(contacts.ToInt64() - numContacts * sizeof(ModifiableContactPatch));
		return (ModifiableContactPatch*)(void*)intPtr;
	}
}
