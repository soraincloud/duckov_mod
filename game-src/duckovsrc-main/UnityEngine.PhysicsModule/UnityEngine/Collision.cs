using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityEngine;

public class Collision
{
	private ContactPairHeader m_Header;

	private ContactPair m_Pair;

	private bool m_Flipped;

	private ContactPoint[] m_LegacyContacts = null;

	public Vector3 impulse => m_Pair.ImpulseSum;

	public Vector3 relativeVelocity => m_Flipped ? m_Header.m_RelativeVelocity : (-m_Header.m_RelativeVelocity);

	public Rigidbody rigidbody => body as Rigidbody;

	public ArticulationBody articulationBody => body as ArticulationBody;

	public Component body => m_Flipped ? m_Header.Body : m_Header.OtherBody;

	public Collider collider => m_Flipped ? m_Pair.Collider : m_Pair.OtherCollider;

	public Transform transform => (rigidbody != null) ? rigidbody.transform : collider.transform;

	public GameObject gameObject => (body != null) ? body.gameObject : collider.gameObject;

	internal bool Flipped
	{
		get
		{
			return m_Flipped;
		}
		set
		{
			m_Flipped = value;
		}
	}

	public int contactCount => (int)m_Pair.m_NbPoints;

	public ContactPoint[] contacts
	{
		get
		{
			if (m_LegacyContacts == null)
			{
				m_LegacyContacts = new ContactPoint[m_Pair.m_NbPoints];
				m_Pair.ExtractContactsArray(m_LegacyContacts, m_Flipped);
			}
			return m_LegacyContacts;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use Collision.relativeVelocity instead. (UnityUpgradable) -> relativeVelocity", false)]
	public Vector3 impactForceSum => Vector3.zero;

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Will always return zero.", true)]
	public Vector3 frictionForceSum => Vector3.zero;

	[Obsolete("Please use Collision.rigidbody, Collision.transform or Collision.collider instead", false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Component other => (body != null) ? body : collider;

	public Collision()
	{
		m_Header = default(ContactPairHeader);
		m_Pair = default(ContactPair);
		m_Flipped = false;
		m_LegacyContacts = null;
	}

	internal Collision(in ContactPairHeader header, in ContactPair pair, bool flipped)
	{
		m_LegacyContacts = new ContactPoint[pair.m_NbPoints];
		pair.ExtractContactsArray(m_LegacyContacts, flipped);
		m_Header = header;
		m_Pair = pair;
		m_Flipped = flipped;
	}

	internal void Reuse(in ContactPairHeader header, in ContactPair pair)
	{
		m_Header = header;
		m_Pair = pair;
		m_LegacyContacts = null;
		m_Flipped = false;
	}

	public unsafe ContactPoint GetContact(int index)
	{
		if (index < 0 || index >= contactCount)
		{
			throw new ArgumentOutOfRangeException($"Cannot get contact at index {index}. There are {contactCount} contact(s).");
		}
		if (m_LegacyContacts != null)
		{
			return m_LegacyContacts[index];
		}
		float num = (m_Flipped ? (-1f) : 1f);
		ContactPairPoint* contactPoint_Internal = m_Pair.GetContactPoint_Internal(index);
		return new ContactPoint(contactPoint_Internal->m_Position, contactPoint_Internal->m_Normal * num, contactPoint_Internal->m_Impulse, contactPoint_Internal->m_Separation, m_Flipped ? m_Pair.OtherColliderInstanceID : m_Pair.ColliderInstanceID, m_Flipped ? m_Pair.ColliderInstanceID : m_Pair.OtherColliderInstanceID);
	}

	public int GetContacts(ContactPoint[] contacts)
	{
		if (contacts == null)
		{
			throw new NullReferenceException("Cannot get contacts as the provided array is NULL.");
		}
		if (m_LegacyContacts != null)
		{
			int num = Mathf.Min(m_LegacyContacts.Length, contacts.Length);
			Array.Copy(m_LegacyContacts, contacts, num);
			return num;
		}
		return m_Pair.ExtractContactsArray(contacts, m_Flipped);
	}

	public int GetContacts(List<ContactPoint> contacts)
	{
		if (contacts == null)
		{
			throw new NullReferenceException("Cannot get contacts as the provided list is NULL.");
		}
		contacts.Clear();
		if (m_LegacyContacts != null)
		{
			contacts.AddRange(m_LegacyContacts);
			return m_LegacyContacts.Length;
		}
		int nbPoints = (int)m_Pair.m_NbPoints;
		if (nbPoints == 0)
		{
			return 0;
		}
		if (contacts.Capacity < nbPoints)
		{
			contacts.Capacity = nbPoints;
		}
		return m_Pair.ExtractContacts(contacts, m_Flipped);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Do not use Collision.GetEnumerator(), enumerate using non-allocating array returned by Collision.GetContacts() or enumerate using Collision.GetContact(index) instead.", false)]
	public virtual IEnumerator GetEnumerator()
	{
		return contacts.GetEnumerator();
	}
}
