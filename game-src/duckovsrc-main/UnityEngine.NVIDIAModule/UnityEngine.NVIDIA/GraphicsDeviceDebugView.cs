using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.NVIDIA;

public class GraphicsDeviceDebugView
{
	internal uint m_ViewId = 0u;

	internal uint m_DeviceVersion = 0u;

	internal uint m_NgxVersion = 0u;

	internal DLSSDebugFeatureInfos[] m_DlssDebugFeatures = null;

	public uint deviceVersion => m_DeviceVersion;

	public uint ngxVersion => m_NgxVersion;

	public IEnumerable<DLSSDebugFeatureInfos> dlssFeatureInfos
	{
		get
		{
			IEnumerable<DLSSDebugFeatureInfos> result;
			if (m_DlssDebugFeatures != null)
			{
				IEnumerable<DLSSDebugFeatureInfos> dlssDebugFeatures = m_DlssDebugFeatures;
				result = dlssDebugFeatures;
			}
			else
			{
				result = Enumerable.Empty<DLSSDebugFeatureInfos>();
			}
			return result;
		}
	}

	internal GraphicsDeviceDebugView(uint viewId)
	{
		m_ViewId = viewId;
	}
}
