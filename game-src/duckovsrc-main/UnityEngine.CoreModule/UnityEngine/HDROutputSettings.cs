using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeHeader("Runtime/GfxDevice/HDROutputSettings.h")]
[UsedByNativeCode]
public class HDROutputSettings
{
	private int m_DisplayIndex;

	public static HDROutputSettings[] displays = new HDROutputSettings[1]
	{
		new HDROutputSettings()
	};

	private static HDROutputSettings _mainDisplay = displays[0];

	public static HDROutputSettings main => _mainDisplay;

	public bool active => GetActive(m_DisplayIndex);

	public bool available => GetAvailable(m_DisplayIndex);

	public bool automaticHDRTonemapping
	{
		get
		{
			return GetAutomaticHDRTonemapping(m_DisplayIndex);
		}
		set
		{
			SetAutomaticHDRTonemapping(m_DisplayIndex, value);
		}
	}

	public ColorGamut displayColorGamut => GetDisplayColorGamut(m_DisplayIndex);

	public RenderTextureFormat format => GraphicsFormatUtility.GetRenderTextureFormat(GetGraphicsFormat(m_DisplayIndex));

	public GraphicsFormat graphicsFormat => GetGraphicsFormat(m_DisplayIndex);

	public float paperWhiteNits
	{
		get
		{
			return GetPaperWhiteNits(m_DisplayIndex);
		}
		set
		{
			SetPaperWhiteNits(m_DisplayIndex, value);
		}
	}

	public int maxFullFrameToneMapLuminance => GetMaxFullFrameToneMapLuminance(m_DisplayIndex);

	public int maxToneMapLuminance => GetMaxToneMapLuminance(m_DisplayIndex);

	public int minToneMapLuminance => GetMinToneMapLuminance(m_DisplayIndex);

	public bool HDRModeChangeRequested => GetHDRModeChangeRequested(m_DisplayIndex);

	[VisibleToOtherModules(new string[] { "UnityEngine.XRModule" })]
	internal HDROutputSettings()
	{
		m_DisplayIndex = 0;
	}

	[VisibleToOtherModules(new string[] { "UnityEngine.XRModule" })]
	internal HDROutputSettings(int displayIndex)
	{
		m_DisplayIndex = displayIndex;
	}

	public void RequestHDRModeChange(bool enabled)
	{
		RequestHDRModeChangeInternal(m_DisplayIndex, enabled);
	}

	[Obsolete("SetPaperWhiteInNits is deprecated, please use paperWhiteNits instead.")]
	public static void SetPaperWhiteInNits(float paperWhite)
	{
		int displayIndex = 0;
		if (GetAvailable(displayIndex))
		{
			SetPaperWhiteNits(displayIndex, paperWhite);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetActive", HasExplicitThis = false, ThrowsException = true)]
	private static extern bool GetActive(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetAvailable", HasExplicitThis = false, ThrowsException = true)]
	private static extern bool GetAvailable(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetAutomaticHDRTonemapping", HasExplicitThis = false, ThrowsException = true)]
	private static extern bool GetAutomaticHDRTonemapping(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::SetAutomaticHDRTonemapping", HasExplicitThis = false, ThrowsException = true)]
	private static extern void SetAutomaticHDRTonemapping(int displayIndex, bool scripted);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetDisplayColorGamut", HasExplicitThis = false, ThrowsException = true)]
	private static extern ColorGamut GetDisplayColorGamut(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetGraphicsFormat", HasExplicitThis = false, ThrowsException = true)]
	private static extern GraphicsFormat GetGraphicsFormat(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetPaperWhiteNits", HasExplicitThis = false, ThrowsException = true)]
	private static extern float GetPaperWhiteNits(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::SetPaperWhiteNits", HasExplicitThis = false, ThrowsException = true)]
	private static extern void SetPaperWhiteNits(int displayIndex, float paperWhite);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetMaxFullFrameToneMapLuminance", HasExplicitThis = false, ThrowsException = true)]
	private static extern int GetMaxFullFrameToneMapLuminance(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetMaxToneMapLuminance", HasExplicitThis = false, ThrowsException = true)]
	private static extern int GetMaxToneMapLuminance(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetMinToneMapLuminance", HasExplicitThis = false, ThrowsException = true)]
	private static extern int GetMinToneMapLuminance(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::GetHDRModeChangeRequested", HasExplicitThis = false, ThrowsException = true)]
	private static extern bool GetHDRModeChangeRequested(int displayIndex);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::RequestHDRModeChange", HasExplicitThis = false, ThrowsException = true)]
	private static extern void RequestHDRModeChangeInternal(int displayIndex, bool enabled);
}
