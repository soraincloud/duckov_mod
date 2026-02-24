using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[StaticAccessor("FrameDebugger", StaticAccessorType.DoubleColon)]
[NativeHeader("Runtime/Profiler/PerformanceTools/FrameDebugger.h")]
public static class FrameDebugger
{
	public static bool enabled => IsLocalEnabled() || IsRemoteEnabled();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsLocalEnabled();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool IsRemoteEnabled();
}
