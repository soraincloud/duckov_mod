using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RTHandleStaticHelpers
{
	public static RTHandle s_RTHandleWrapper;

	public static void SetRTHandleStaticWrapper(RenderTargetIdentifier rtId)
	{
		if (s_RTHandleWrapper == null)
		{
			s_RTHandleWrapper = RTHandles.Alloc(rtId);
		}
		else
		{
			s_RTHandleWrapper.SetTexture(rtId);
		}
	}

	public static void SetRTHandleUserManagedWrapper(ref RTHandle rtWrapper, RenderTargetIdentifier rtId)
	{
		if (rtWrapper != null)
		{
			if (rtWrapper.m_RT != null)
			{
				throw new ArgumentException("Input wrapper must be a wrapper around RenderTargetIdentifier. Passed in warpper contains valid RenderTexture " + rtWrapper.m_RT.name + " and cannot be used as warpper.");
			}
			if (rtWrapper.m_ExternalTexture != null)
			{
				throw new ArgumentException("Input wrapper must be a wrapper around RenderTargetIdentifier. Passed in warpper contains valid Texture " + rtWrapper.m_ExternalTexture.name + " and cannot be used as warpper.");
			}
			rtWrapper.SetTexture(rtId);
		}
	}
}
