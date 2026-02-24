using System;
using UnityEngine.Serialization;

namespace UnityEngine.TextCore.Text;

[Serializable]
[ExcludeFromObjectFactory]
public abstract class TextAsset : ScriptableObject
{
	[SerializeField]
	internal string m_Version;

	internal int m_InstanceID;

	internal int m_HashCode;

	[FormerlySerializedAs("material")]
	[SerializeField]
	internal Material m_Material;

	internal int m_MaterialHashCode;

	public string version
	{
		get
		{
			return m_Version;
		}
		internal set
		{
			m_Version = value;
		}
	}

	public int instanceID
	{
		get
		{
			if (m_InstanceID == 0)
			{
				m_InstanceID = GetInstanceID();
			}
			return m_InstanceID;
		}
	}

	public int hashCode
	{
		get
		{
			if (m_HashCode == 0)
			{
				m_HashCode = TextUtilities.GetHashCodeCaseInSensitive(base.name);
			}
			return m_HashCode;
		}
		set
		{
			m_HashCode = value;
		}
	}

	public Material material
	{
		get
		{
			return m_Material;
		}
		set
		{
			m_Material = value;
		}
	}

	public int materialHashCode
	{
		get
		{
			if (m_MaterialHashCode == 0)
			{
				if (m_Material == null)
				{
					return 0;
				}
				m_MaterialHashCode = TextUtilities.GetHashCodeCaseInSensitive(m_Material.name);
			}
			return m_MaterialHashCode;
		}
		set
		{
			m_MaterialHashCode = value;
		}
	}
}
