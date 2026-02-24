using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.XR;

[NativeConditional("ENABLE_VR")]
public static class XRStats
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetIVRDeviceScripting()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	public static extern bool TryGetGPUTimeLastFrame(out float gpuTimeLastFrame);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetIVRDeviceScripting()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	public static extern bool TryGetDroppedFrameCount(out int droppedFrameCount);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetIVRDeviceScripting()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	public static extern bool TryGetFramePresentCount(out int framePresentCount);
}
