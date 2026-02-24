using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[NativeHeader("Runtime/Graphics/CustomRenderTextureManager.h")]
public static class CustomRenderTextureManager
{
	public static event Action<CustomRenderTexture> textureLoaded;

	public static event Action<CustomRenderTexture> textureUnloaded;

	public static event Action<CustomRenderTexture, int> updateTriggered;

	public static event Action<CustomRenderTexture> initializeTriggered;

	[RequiredByNativeCode]
	private static void InvokeOnTextureLoaded_Internal(CustomRenderTexture source)
	{
		CustomRenderTextureManager.textureLoaded?.Invoke(source);
	}

	[RequiredByNativeCode]
	private static void InvokeOnTextureUnloaded_Internal(CustomRenderTexture source)
	{
		CustomRenderTextureManager.textureUnloaded?.Invoke(source);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(Name = "CustomRenderTextureManagerScripting::GetAllCustomRenderTextures", HasExplicitThis = false)]
	public static extern void GetAllCustomRenderTextures(List<CustomRenderTexture> currentCustomRenderTextures);

	internal static void InvokeTriggerUpdate(CustomRenderTexture crt, int updateCount)
	{
		CustomRenderTextureManager.updateTriggered?.Invoke(crt, updateCount);
	}

	internal static void InvokeTriggerInitialize(CustomRenderTexture crt)
	{
		CustomRenderTextureManager.initializeTriggered?.Invoke(crt);
	}
}
