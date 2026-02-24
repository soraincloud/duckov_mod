using System;

namespace UnityEngine.NVIDIA;

internal class InitDeviceContext
{
	private NativeStr m_ProjectId = new NativeStr();

	private NativeStr m_EngineVersion = new NativeStr();

	private NativeStr m_AppDir = new NativeStr();

	private NativeData<InitDeviceCmdData> m_InitData = new NativeData<InitDeviceCmdData>();

	public InitDeviceContext(string projectId, string engineVersion, string appDir)
	{
		m_ProjectId.Str = projectId;
		m_EngineVersion.Str = engineVersion;
		m_AppDir.Str = appDir;
	}

	internal IntPtr GetInitCmdPtr()
	{
		m_InitData.Value.projectId = m_ProjectId.Ptr;
		m_InitData.Value.engineVersion = m_EngineVersion.Ptr;
		m_InitData.Value.appDir = m_AppDir.Ptr;
		return m_InitData.Ptr;
	}
}
