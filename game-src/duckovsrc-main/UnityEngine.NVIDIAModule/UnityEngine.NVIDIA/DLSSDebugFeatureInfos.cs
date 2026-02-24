namespace UnityEngine.NVIDIA;

public readonly struct DLSSDebugFeatureInfos
{
	private readonly bool m_ValidFeature;

	private readonly uint m_FeatureSlot;

	private readonly DLSSCommandExecutionData m_ExecData;

	private readonly DLSSCommandInitializationData m_InitData;

	public bool validFeature => m_ValidFeature;

	public uint featureSlot => m_FeatureSlot;

	public DLSSCommandExecutionData execData => m_ExecData;

	public DLSSCommandInitializationData initData => m_InitData;
}
