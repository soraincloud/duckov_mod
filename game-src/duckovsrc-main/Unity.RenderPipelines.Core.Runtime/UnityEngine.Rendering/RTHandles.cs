using System;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

public static class RTHandles
{
	private static RTHandleSystem s_DefaultInstance = new RTHandleSystem();

	public static int maxWidth => s_DefaultInstance.GetMaxWidth();

	public static int maxHeight => s_DefaultInstance.GetMaxHeight();

	public static RTHandleProperties rtHandleProperties => s_DefaultInstance.rtHandleProperties;

	public static RTHandle Alloc(int width, int height, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(width, height, slices, depthBufferBits, colorFormat, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(int width, int height, TextureWrapMode wrapModeU, TextureWrapMode wrapModeV, TextureWrapMode wrapModeW = TextureWrapMode.Repeat, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(width, height, wrapModeU, wrapModeV, wrapModeW, slices, depthBufferBits, colorFormat, filterMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		return s_DefaultInstance.Alloc(descriptor.width, descriptor.height, descriptor.volumeDepth, (DepthBits)descriptor.depthBufferBits, descriptor.graphicsFormat, filterMode, wrapMode, descriptor.dimension, descriptor.enableRandomWrite, descriptor.useMipMap, descriptor.autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, (MSAASamples)descriptor.msaaSamples, descriptor.bindMS, descriptor.useDynamicScale, descriptor.memoryless, descriptor.vrUsage, name);
	}

	public static RTHandle Alloc(Vector2 scaleFactor, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFactor, slices, depthBufferBits, colorFormat, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(Vector2 scaleFactor, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFactor, descriptor.volumeDepth, (DepthBits)descriptor.depthBufferBits, descriptor.graphicsFormat, filterMode, wrapMode, descriptor.dimension, descriptor.enableRandomWrite, descriptor.useMipMap, descriptor.autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, (MSAASamples)descriptor.msaaSamples, descriptor.bindMS, descriptor.useDynamicScale, descriptor.memoryless, descriptor.vrUsage, name);
	}

	public static RTHandle Alloc(ScaleFunc scaleFunc, int slices = 1, DepthBits depthBufferBits = DepthBits.None, GraphicsFormat colorFormat = GraphicsFormat.R8G8B8A8_SRGB, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, TextureDimension dimension = TextureDimension.Tex2D, bool enableRandomWrite = false, bool useMipMap = false, bool autoGenerateMips = true, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, MSAASamples msaaSamples = MSAASamples.None, bool bindTextureMS = false, bool useDynamicScale = false, RenderTextureMemoryless memoryless = RenderTextureMemoryless.None, VRTextureUsage vrUsage = VRTextureUsage.None, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFunc, slices, depthBufferBits, colorFormat, filterMode, wrapMode, dimension, enableRandomWrite, useMipMap, autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, msaaSamples, bindTextureMS, useDynamicScale, memoryless, vrUsage, name);
	}

	public static RTHandle Alloc(ScaleFunc scaleFunc, in RenderTextureDescriptor descriptor, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Repeat, bool isShadowMap = false, int anisoLevel = 1, float mipMapBias = 0f, string name = "")
	{
		return s_DefaultInstance.Alloc(scaleFunc, descriptor.volumeDepth, (DepthBits)descriptor.depthBufferBits, descriptor.graphicsFormat, filterMode, wrapMode, descriptor.dimension, descriptor.enableRandomWrite, descriptor.useMipMap, descriptor.autoGenerateMips, isShadowMap, anisoLevel, mipMapBias, (MSAASamples)descriptor.msaaSamples, descriptor.bindMS, descriptor.useDynamicScale, descriptor.memoryless, descriptor.vrUsage, name);
	}

	public static RTHandle Alloc(Texture tex)
	{
		return s_DefaultInstance.Alloc(tex);
	}

	public static RTHandle Alloc(RenderTexture tex)
	{
		return s_DefaultInstance.Alloc(tex);
	}

	public static RTHandle Alloc(RenderTargetIdentifier tex)
	{
		return s_DefaultInstance.Alloc(tex);
	}

	public static RTHandle Alloc(RenderTargetIdentifier tex, string name)
	{
		return s_DefaultInstance.Alloc(tex, name);
	}

	private static RTHandle Alloc(RTHandle tex)
	{
		Debug.LogError("Allocation a RTHandle from another one is forbidden.");
		return null;
	}

	public static void Initialize(int width, int height)
	{
		s_DefaultInstance.Initialize(width, height);
	}

	[Obsolete("useLegacyDynamicResControl is deprecated. Please use SetHardwareDynamicResolutionState() instead.")]
	public static void Initialize(int width, int height, bool useLegacyDynamicResControl = false)
	{
		s_DefaultInstance.Initialize(width, height, useLegacyDynamicResControl);
	}

	public static void Release(RTHandle rth)
	{
		s_DefaultInstance.Release(rth);
	}

	public static void SetHardwareDynamicResolutionState(bool hwDynamicResRequested)
	{
		s_DefaultInstance.SetHardwareDynamicResolutionState(hwDynamicResRequested);
	}

	public static void SetReferenceSize(int width, int height)
	{
		s_DefaultInstance.SetReferenceSize(width, height);
	}

	public static void ResetReferenceSize(int width, int height)
	{
		s_DefaultInstance.ResetReferenceSize(width, height);
	}

	public static Vector2 CalculateRatioAgainstMaxSize(int width, int height)
	{
		return s_DefaultInstance.CalculateRatioAgainstMaxSize(new Vector2Int(width, height));
	}
}
