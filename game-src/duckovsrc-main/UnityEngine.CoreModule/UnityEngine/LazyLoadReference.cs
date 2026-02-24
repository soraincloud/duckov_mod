using System;

namespace UnityEngine;

[Serializable]
public struct LazyLoadReference<T> where T : Object
{
	private const int kInstanceID_None = 0;

	[SerializeField]
	private int m_InstanceID;

	public bool isSet => m_InstanceID != 0;

	public bool isBroken => m_InstanceID != 0 && !Object.DoesObjectWithInstanceIDExist(m_InstanceID);

	public T asset
	{
		get
		{
			if (m_InstanceID == 0)
			{
				return null;
			}
			return (T)Object.ForceLoadFromInstanceID(m_InstanceID);
		}
		set
		{
			if (value == null)
			{
				m_InstanceID = 0;
				return;
			}
			if (!Object.IsPersistent(value))
			{
				throw new ArgumentException("Object that does not belong to a persisted asset cannot be set as the target of a LazyLoadReference.");
			}
			m_InstanceID = value.GetInstanceID();
		}
	}

	public int instanceID
	{
		get
		{
			return m_InstanceID;
		}
		set
		{
			m_InstanceID = value;
		}
	}

	public LazyLoadReference(T asset)
	{
		if (asset == null)
		{
			m_InstanceID = 0;
			return;
		}
		if (!Object.IsPersistent(asset))
		{
			throw new ArgumentException("Object that does not belong to a persisted asset cannot be set as the target of a LazyLoadReference.");
		}
		m_InstanceID = asset.GetInstanceID();
	}

	public LazyLoadReference(int instanceID)
	{
		m_InstanceID = instanceID;
	}

	public static implicit operator LazyLoadReference<T>(T asset)
	{
		return new LazyLoadReference<T>
		{
			asset = asset
		};
	}

	public static implicit operator LazyLoadReference<T>(int instanceID)
	{
		return new LazyLoadReference<T>
		{
			instanceID = instanceID
		};
	}
}
