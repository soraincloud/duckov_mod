using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering;

[NativeHeader("Runtime/Graphics/GraphicsScriptBindings.h")]
public class PIX
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("PIX::BeginGPUCapture")]
	public static extern void BeginGPUCapture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("PIX::EndGPUCapture")]
	public static extern void EndGPUCapture();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("PIX::IsAttached")]
	public static extern bool IsAttached();
}
