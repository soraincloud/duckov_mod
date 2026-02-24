namespace UnityEngine.NVIDIA;

public struct DLSSCommandInitializationData
{
	private uint m_InputRTWidth;

	private uint m_InputRTHeight;

	private uint m_OutputRTWidth;

	private uint m_OutputRTHeight;

	private DLSSQuality m_Quality;

	private DLSSFeatureFlags m_Flags;

	private uint m_FeatureSlot;

	public uint inputRTWidth
	{
		get
		{
			return m_InputRTWidth;
		}
		set
		{
			m_InputRTWidth = value;
		}
	}

	public uint inputRTHeight
	{
		get
		{
			return m_InputRTHeight;
		}
		set
		{
			m_InputRTHeight = value;
		}
	}

	public uint outputRTWidth
	{
		get
		{
			return m_OutputRTWidth;
		}
		set
		{
			m_OutputRTWidth = value;
		}
	}

	public uint outputRTHeight
	{
		get
		{
			return m_OutputRTHeight;
		}
		set
		{
			m_OutputRTHeight = value;
		}
	}

	public DLSSQuality quality
	{
		get
		{
			return m_Quality;
		}
		set
		{
			m_Quality = value;
		}
	}

	public DLSSFeatureFlags featureFlags
	{
		get
		{
			return m_Flags;
		}
		set
		{
			m_Flags = value;
		}
	}

	internal uint featureSlot
	{
		get
		{
			return m_FeatureSlot;
		}
		set
		{
			m_FeatureSlot = value;
		}
	}

	public void SetFlag(DLSSFeatureFlags flag, bool value)
	{
		if (value)
		{
			m_Flags |= flag;
		}
		else
		{
			m_Flags &= ~flag;
		}
	}

	public bool GetFlag(DLSSFeatureFlags flag)
	{
		return (m_Flags & flag) != 0;
	}
}
