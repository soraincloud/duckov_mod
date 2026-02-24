using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.VirtualTexturing;

[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
[StaticAccessor("VirtualTexturing::Debugging", StaticAccessorType.DoubleColon)]
public static class Debugging
{
	[NativeHeader("Modules/VirtualTexturing/Public/VirtualTexturingDebugHandle.h")]
	[UsedByNativeCode]
	public struct Handle
	{
		public long handle;

		public string group;

		public string name;

		public int numLayers;

		public Material material;
	}

	[NativeThrows]
	public static extern bool debugTilesEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeThrows]
	public static extern bool resolvingEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeThrows]
	public static extern bool flushEveryTickEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeThrows]
	public static extern int mipPreloadedTextureCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern int GetNumHandles();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void GrabHandleInfo(out Handle debugHandle, int index);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern string GetInfoDump();
}
