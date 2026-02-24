using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering.VirtualTexturing;

[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
[StaticAccessor("VirtualTexturing::Streaming", StaticAccessorType.DoubleColon)]
public static class Streaming
{
	[NativeThrows]
	public static void RequestRegion([NotNull("ArgumentNullException")] Material mat, int stackNameId, Rect r, int mipMap, int numMips)
	{
		RequestRegion_Injected(mat, stackNameId, ref r, mipMap, numMips);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void GetTextureStackSize([NotNull("ArgumentNullException")] Material mat, int stackNameId, out int width, out int height);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void SetCPUCacheSize(int sizeInMegabytes);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern int GetCPUCacheSize();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void SetGPUCacheSettings(GPUCacheSetting[] cacheSettings);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern GPUCacheSetting[] GetGPUCacheSettings();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void EnableMipPreloading(int texturesPerFrame, int mipCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void RequestRegion_Injected(Material mat, int stackNameId, ref Rect r, int mipMap, int numMips);
}
