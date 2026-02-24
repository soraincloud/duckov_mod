using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Diagnostics;

[NativeHeader("Runtime/Misc/GarbageCollectSharedAssets.h")]
[NativeHeader("Runtime/Export/Diagnostics/DiagnosticsUtils.bindings.h")]
public static class Utils
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("DiagnosticsUtils_Bindings::ForceCrash", IsThreadSafe = true, ThrowsException = true)]
	public static extern void ForceCrash(ForcedCrashCategory crashCategory);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("DiagnosticsUtils_Bindings::NativeAssert", IsThreadSafe = true)]
	public static extern void NativeAssert(string message);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("DiagnosticsUtils_Bindings::NativeError", IsThreadSafe = true)]
	public static extern void NativeError(string message);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("DiagnosticsUtils_Bindings::NativeWarning", IsThreadSafe = true)]
	public static extern void NativeWarning(string message);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ValidateHeap")]
	public static extern void ValidateHeap();
}
