using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[UsedByNativeCode]
public struct WebCamDevice
{
	[NativeName("name")]
	internal string m_Name;

	[NativeName("depthCameraName")]
	internal string m_DepthCameraName;

	[NativeName("flags")]
	internal int m_Flags;

	[NativeName("kind")]
	internal WebCamKind m_Kind;

	[NativeName("resolutions")]
	internal Resolution[] m_Resolutions;

	public string name => m_Name;

	public bool isFrontFacing => (m_Flags & 1) != 0;

	public WebCamKind kind => m_Kind;

	public string depthCameraName => (m_DepthCameraName == "") ? null : m_DepthCameraName;

	public bool isAutoFocusPointSupported => (m_Flags & 2) != 0;

	public Resolution[] availableResolutions => m_Resolutions;
}
