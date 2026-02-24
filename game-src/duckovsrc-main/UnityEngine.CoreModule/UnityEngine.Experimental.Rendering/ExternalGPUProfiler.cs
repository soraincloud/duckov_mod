using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Experimental.Rendering;

[NativeHeader("Runtime/Graphics/GraphicsScriptBindings.h")]
public static class ExternalGPUProfiler
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ExternalGPUProfilerBindings::BeginGPUCapture")]
	public static extern void BeginGPUCapture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ExternalGPUProfilerBindings::EndGPUCapture")]
	public static extern void EndGPUCapture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ExternalGPUProfilerBindings::IsAttached")]
	public static extern bool IsAttached();
}
