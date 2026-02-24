using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Experimental.Audio;

[NativeHeader("AudioScriptingClasses.h")]
[NativeHeader("Modules/Audio/Public/AudioSource.h")]
[NativeHeader("Modules/Audio/Public/ScriptBindings/AudioSourceExtensions.bindings.h")]
internal static class AudioSourceExtensionsInternal
{
	public static void RegisterSampleProvider(this AudioSource source, AudioSampleProvider provider)
	{
		Internal_RegisterSampleProviderWithAudioSource(source, provider.id);
	}

	public static void UnregisterSampleProvider(this AudioSource source, AudioSampleProvider provider)
	{
		Internal_UnregisterSampleProviderFromAudioSource(source, provider.id);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsFreeFunction = true, ThrowsException = true)]
	private static extern void Internal_RegisterSampleProviderWithAudioSource([NotNull("NullExceptionObject")] AudioSource source, uint providerId);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(IsFreeFunction = true, ThrowsException = true)]
	private static extern void Internal_UnregisterSampleProviderFromAudioSource([NotNull("NullExceptionObject")] AudioSource source, uint providerId);
}
