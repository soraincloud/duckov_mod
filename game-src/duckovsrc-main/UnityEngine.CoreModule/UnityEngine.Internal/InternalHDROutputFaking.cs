using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Internal;

[ExcludeFromDocs]
[NativeHeader("Runtime/GfxDevice/HDROutputSettings.h")]
internal static class InternalHDROutputFaking
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("HDROutputSettingsBindings::SetFakeHDROutputEnabled")]
	[ExcludeFromDocs]
	internal static extern void SetEnabled(bool enabled);
}
