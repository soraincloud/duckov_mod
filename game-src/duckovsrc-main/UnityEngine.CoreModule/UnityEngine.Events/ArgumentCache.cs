using System;
using UnityEngine.Serialization;

namespace UnityEngine.Events;

[Serializable]
internal class ArgumentCache : ISerializationCallbackReceiver
{
	[FormerlySerializedAs("objectArgument")]
	[SerializeField]
	private Object m_ObjectArgument;

	[FormerlySerializedAs("objectArgumentAssemblyTypeName")]
	[SerializeField]
	private string m_ObjectArgumentAssemblyTypeName;

	[FormerlySerializedAs("intArgument")]
	[SerializeField]
	private int m_IntArgument;

	[SerializeField]
	[FormerlySerializedAs("floatArgument")]
	private float m_FloatArgument;

	[SerializeField]
	[FormerlySerializedAs("stringArgument")]
	private string m_StringArgument;

	[SerializeField]
	private bool m_BoolArgument;

	public Object unityObjectArgument
	{
		get
		{
			return m_ObjectArgument;
		}
		set
		{
			m_ObjectArgument = value;
			m_ObjectArgumentAssemblyTypeName = ((value != null) ? value.GetType().AssemblyQualifiedName : string.Empty);
		}
	}

	public string unityObjectArgumentAssemblyTypeName => m_ObjectArgumentAssemblyTypeName;

	public int intArgument
	{
		get
		{
			return m_IntArgument;
		}
		set
		{
			m_IntArgument = value;
		}
	}

	public float floatArgument
	{
		get
		{
			return m_FloatArgument;
		}
		set
		{
			m_FloatArgument = value;
		}
	}

	public string stringArgument
	{
		get
		{
			return m_StringArgument;
		}
		set
		{
			m_StringArgument = value;
		}
	}

	public bool boolArgument
	{
		get
		{
			return m_BoolArgument;
		}
		set
		{
			m_BoolArgument = value;
		}
	}

	public void OnBeforeSerialize()
	{
		m_ObjectArgumentAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(m_ObjectArgumentAssemblyTypeName);
	}

	public void OnAfterDeserialize()
	{
		m_ObjectArgumentAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(m_ObjectArgumentAssemblyTypeName);
	}
}
