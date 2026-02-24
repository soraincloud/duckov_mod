namespace UnityEngine.NVIDIA;

public struct DLSSCommandExecutionData
{
	internal enum Textures
	{
		ColorInput,
		ColorOutput,
		Depth,
		MotionVectors,
		TransparencyMask,
		ExposureTexture,
		BiasColorMask
	}

	private int m_Reset;

	private float m_Sharpness;

	private float m_MVScaleX;

	private float m_MVScaleY;

	private float m_JitterOffsetX;

	private float m_JitterOffsetY;

	private float m_PreExposure;

	private uint m_SubrectOffsetX;

	private uint m_SubrectOffsetY;

	private uint m_SubrectWidth;

	private uint m_SubrectHeight;

	private uint m_InvertXAxis;

	private uint m_InvertYAxis;

	private uint m_FeatureSlot;

	public int reset
	{
		get
		{
			return m_Reset;
		}
		set
		{
			m_Reset = value;
		}
	}

	public float sharpness
	{
		get
		{
			return m_Sharpness;
		}
		set
		{
			m_Sharpness = value;
		}
	}

	public float mvScaleX
	{
		get
		{
			return m_MVScaleX;
		}
		set
		{
			m_MVScaleX = value;
		}
	}

	public float mvScaleY
	{
		get
		{
			return m_MVScaleY;
		}
		set
		{
			m_MVScaleY = value;
		}
	}

	public float jitterOffsetX
	{
		get
		{
			return m_JitterOffsetX;
		}
		set
		{
			m_JitterOffsetX = value;
		}
	}

	public float jitterOffsetY
	{
		get
		{
			return m_JitterOffsetY;
		}
		set
		{
			m_JitterOffsetY = value;
		}
	}

	public float preExposure
	{
		get
		{
			return m_PreExposure;
		}
		set
		{
			m_PreExposure = value;
		}
	}

	public uint subrectOffsetX
	{
		get
		{
			return m_SubrectOffsetX;
		}
		set
		{
			m_SubrectOffsetX = value;
		}
	}

	public uint subrectOffsetY
	{
		get
		{
			return m_SubrectOffsetY;
		}
		set
		{
			m_SubrectOffsetY = value;
		}
	}

	public uint subrectWidth
	{
		get
		{
			return m_SubrectWidth;
		}
		set
		{
			m_SubrectWidth = value;
		}
	}

	public uint subrectHeight
	{
		get
		{
			return m_SubrectHeight;
		}
		set
		{
			m_SubrectHeight = value;
		}
	}

	public uint invertXAxis
	{
		get
		{
			return m_InvertXAxis;
		}
		set
		{
			m_InvertXAxis = value;
		}
	}

	public uint invertYAxis
	{
		get
		{
			return m_InvertYAxis;
		}
		set
		{
			m_InvertYAxis = value;
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
}
