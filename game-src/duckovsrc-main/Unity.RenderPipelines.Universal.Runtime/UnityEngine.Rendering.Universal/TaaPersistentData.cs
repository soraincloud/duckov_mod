using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal;

internal sealed class TaaPersistentData
{
	private static GraphicsFormat[] formatList = new GraphicsFormat[4]
	{
		GraphicsFormat.R16G16B16A16_SFloat,
		GraphicsFormat.B10G11R11_UFloatPack32,
		GraphicsFormat.R8G8B8A8_UNorm,
		GraphicsFormat.B8G8R8A8_UNorm
	};

	private RenderTextureDescriptor m_RtDesc;

	private RTHandle m_AccumulationTexture;

	private RTHandle m_AccumulationTexture2;

	private int m_LastAccumUpdateFrameIndex;

	private int m_LastAccumUpdateFrameIndex2;

	public RenderTextureDescriptor rtd => m_RtDesc;

	public RTHandle accumulationTexture(int index)
	{
		if (index == 0)
		{
			return m_AccumulationTexture;
		}
		return m_AccumulationTexture2;
	}

	public int GetLastAccumFrameIndex(int index)
	{
		if (index == 0)
		{
			return m_LastAccumUpdateFrameIndex;
		}
		return m_LastAccumUpdateFrameIndex2;
	}

	public void SetLastAccumFrameIndex(int index, int value)
	{
		if (index != 0)
		{
			m_LastAccumUpdateFrameIndex2 = value;
		}
		else
		{
			m_LastAccumUpdateFrameIndex = value;
		}
	}

	public void Init(int sizeX, int sizeY, int volumeDepth, GraphicsFormat format, VRTextureUsage vrUsage, TextureDimension texDim)
	{
		if ((m_RtDesc.width != sizeX || m_RtDesc.height != sizeY || m_RtDesc.volumeDepth != volumeDepth || m_AccumulationTexture == null) && sizeX > 0 && sizeY > 0)
		{
			RenderTextureDescriptor rtDesc = default(RenderTextureDescriptor);
			FormatUsage usage = FormatUsage.Render;
			rtDesc.width = sizeX;
			rtDesc.height = sizeY;
			rtDesc.msaaSamples = 1;
			rtDesc.volumeDepth = volumeDepth;
			rtDesc.mipCount = 0;
			rtDesc.graphicsFormat = CheckFormat(format, usage);
			rtDesc.sRGB = false;
			rtDesc.depthBufferBits = 0;
			rtDesc.dimension = texDim;
			rtDesc.vrUsage = vrUsage;
			rtDesc.memoryless = RenderTextureMemoryless.None;
			rtDesc.useMipMap = false;
			rtDesc.autoGenerateMips = false;
			rtDesc.enableRandomWrite = false;
			rtDesc.bindMS = false;
			rtDesc.useDynamicScale = false;
			m_RtDesc = rtDesc;
			DeallocateTargets();
		}
		static GraphicsFormat CheckFormat(GraphicsFormat graphicsFormat, FormatUsage usage2)
		{
			if (!SystemInfo.IsFormatSupported(graphicsFormat, usage2))
			{
				return FindFormat(usage2);
			}
			return graphicsFormat;
		}
		static GraphicsFormat FindFormat(FormatUsage usage2)
		{
			for (int i = 0; i < formatList.Length; i++)
			{
				if (SystemInfo.IsFormatSupported(formatList[i], usage2))
				{
					return formatList[i];
				}
			}
			return GraphicsFormat.B8G8R8A8_UNorm;
		}
	}

	public bool AllocateTargets(bool xrMultipassEnabled = false)
	{
		bool result = false;
		if (m_AccumulationTexture == null)
		{
			m_AccumulationTexture = RTHandles.Alloc(in m_RtDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_TaaAccumulationTex");
			result = true;
		}
		if (xrMultipassEnabled && m_AccumulationTexture2 == null)
		{
			m_AccumulationTexture2 = RTHandles.Alloc(in m_RtDesc, FilterMode.Bilinear, TextureWrapMode.Clamp, isShadowMap: false, 1, 0f, "_TaaAccumulationTex2");
			result = true;
		}
		return result;
	}

	public void DeallocateTargets()
	{
		m_AccumulationTexture?.Release();
		m_AccumulationTexture2?.Release();
		m_AccumulationTexture = null;
		m_AccumulationTexture2 = null;
		m_LastAccumUpdateFrameIndex = -1;
		m_LastAccumUpdateFrameIndex2 = -1;
	}
}
