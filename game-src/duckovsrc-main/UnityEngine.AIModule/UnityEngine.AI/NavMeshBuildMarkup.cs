using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.AI;

[NativeHeader("Modules/AI/Public/NavMeshBindingTypes.h")]
public struct NavMeshBuildMarkup
{
	private int m_OverrideArea;

	private int m_Area;

	private int m_InheritIgnoreFromBuild;

	private int m_IgnoreFromBuild;

	private int m_OverrideGenerateLinks;

	private int m_GenerateLinks;

	private int m_InstanceID;

	private int m_IgnoreChildren;

	public bool overrideArea
	{
		get
		{
			return m_OverrideArea != 0;
		}
		set
		{
			m_OverrideArea = (value ? 1 : 0);
		}
	}

	public int area
	{
		get
		{
			return m_Area;
		}
		set
		{
			m_Area = value;
		}
	}

	public bool overrideIgnore
	{
		get
		{
			return m_InheritIgnoreFromBuild == 0;
		}
		set
		{
			m_InheritIgnoreFromBuild = ((!value) ? 1 : 0);
		}
	}

	public bool ignoreFromBuild
	{
		get
		{
			return m_IgnoreFromBuild != 0;
		}
		set
		{
			m_IgnoreFromBuild = (value ? 1 : 0);
		}
	}

	public bool overrideGenerateLinks
	{
		get
		{
			return m_OverrideGenerateLinks != 0;
		}
		set
		{
			m_OverrideGenerateLinks = (value ? 1 : 0);
		}
	}

	public bool generateLinks
	{
		get
		{
			return m_GenerateLinks != 0;
		}
		set
		{
			m_GenerateLinks = (value ? 1 : 0);
		}
	}

	public bool applyToChildren
	{
		get
		{
			return m_IgnoreChildren == 0;
		}
		set
		{
			m_IgnoreChildren = ((!value) ? 1 : 0);
		}
	}

	public Transform root
	{
		get
		{
			return InternalGetRootGO(m_InstanceID);
		}
		set
		{
			m_InstanceID = ((value != null) ? value.GetInstanceID() : 0);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("NavMeshBuildMarkup", StaticAccessorType.DoubleColon)]
	private static extern Transform InternalGetRootGO(int instanceID);
}
