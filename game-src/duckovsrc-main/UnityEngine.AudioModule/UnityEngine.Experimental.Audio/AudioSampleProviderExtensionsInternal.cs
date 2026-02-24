using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Experimental.Audio;

[StaticAccessor("AudioSampleProviderExtensionsBindings", StaticAccessorType.DoubleColon)]
[NativeHeader("Modules/Audio/Public/ScriptBindings/AudioSampleProviderExtensions.bindings.h")]
internal static class AudioSampleProviderExtensionsInternal
{
	public static float GetSpeed(this AudioSampleProvider provider)
	{
		return InternalGetAudioSampleProviderSpeed(provider.id);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsThreadSafe = true, ThrowsException = true)]
	private static extern float InternalGetAudioSampleProviderSpeed(uint providerId);
}
