using System;

namespace UnityEngine.NVIDIA;

internal struct InitDeviceCmdData
{
	private IntPtr m_ProjectId;

	private IntPtr m_EngineVersion;

	private IntPtr m_AppDir;

	public IntPtr projectId
	{
		get
		{
			return m_ProjectId;
		}
		set
		{
			m_ProjectId = value;
		}
	}

	public IntPtr engineVersion
	{
		get
		{
			return m_EngineVersion;
		}
		set
		{
			m_EngineVersion = value;
		}
	}

	public IntPtr appDir
	{
		get
		{
			return m_AppDir;
		}
		set
		{
			m_AppDir = value;
		}
	}
}
