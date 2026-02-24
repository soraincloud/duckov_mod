using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[RequiredByNativeCode]
public class OnDemandRendering
{
	private static int m_RenderFrameInterval = 1;

	public static bool willCurrentFrameRender => Time.frameCount % renderFrameInterval == 0;

	public static int renderFrameInterval
	{
		get
		{
			return m_RenderFrameInterval;
		}
		set
		{
			m_RenderFrameInterval = Math.Max(1, value);
		}
	}

	public static int effectiveRenderFrameRate
	{
		get
		{
			float num = GetEffectiveRenderFrameRate();
			if ((double)num <= 0.0)
			{
				return (int)num;
			}
			return (int)(num + 0.5f);
		}
	}

	[RequiredByNativeCode]
	internal static void GetRenderFrameInterval(out int frameInterval)
	{
		frameInterval = renderFrameInterval;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction]
	internal static extern float GetEffectiveRenderFrameRate();
}
