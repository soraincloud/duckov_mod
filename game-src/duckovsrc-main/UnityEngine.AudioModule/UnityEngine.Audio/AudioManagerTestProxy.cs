using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Audio;

[NativeHeader("Modules/Audio/Public/ScriptBindings/Audio.bindings.h")]
internal sealed class AudioManagerTestProxy
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "AudioManagerTestProxy::ComputeAudibilityConsistency", IsFreeFunction = true)]
	internal static extern bool ComputeAudibilityConsistency();
}
