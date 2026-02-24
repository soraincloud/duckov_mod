using System;

namespace UnityEngine.NVIDIA;

public class DLSSContext
{
	private NativeData<DLSSCommandInitializationData> m_InitData = new NativeData<DLSSCommandInitializationData>();

	private NativeData<DLSSCommandExecutionData> m_ExecData = new NativeData<DLSSCommandExecutionData>();

	public ref readonly DLSSCommandInitializationData initData => ref m_InitData.Value;

	public ref DLSSCommandExecutionData executeData => ref m_ExecData.Value;

	internal uint featureSlot => initData.featureSlot;

	internal DLSSContext()
	{
	}

	internal void Init(DLSSCommandInitializationData initSettings, uint featureSlot)
	{
		m_InitData.Value = initSettings;
		m_InitData.Value.featureSlot = featureSlot;
	}

	internal void Reset()
	{
		m_InitData.Value = default(DLSSCommandInitializationData);
		m_ExecData.Value = default(DLSSCommandExecutionData);
	}

	internal IntPtr GetInitCmdPtr()
	{
		return m_InitData.Ptr;
	}

	internal IntPtr GetExecuteCmdPtr()
	{
		m_ExecData.Value.featureSlot = featureSlot;
		return m_ExecData.Ptr;
	}
}
