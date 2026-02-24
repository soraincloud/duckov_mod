using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[NativeConditional("ENABLE_VR")]
public static class XRDevice
{
	[Obsolete("This is obsolete, and should no longer be used. Instead, find the active XRDisplaySubsystem and check that the running property is true (for details, see XRDevice.isPresent documentation).", true)]
	public static bool isPresent
	{
		get
		{
			throw new NotSupportedException("XRDevice is Obsolete. Instead, find the active XRDisplaySubsystem and check to see if it is running.");
		}
	}

	[StaticAccessor("GetIVRDeviceSwapChain()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	[NativeName("DeviceRefreshRate")]
	public static extern float refreshRate
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static extern float fovZoomFactor
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[StaticAccessor("GetIVRDeviceScripting()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
		[NativeName("SetProjectionZoomFactor")]
		set;
	}

	public static event Action<string> deviceLoaded;

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetIVRDeviceScripting()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	public static extern IntPtr GetNativePtr();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetIVRDevice()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	[Obsolete("This is obsolete, and should no longer be used.  Please use XRInputSubsystem.GetTrackingOriginMode.")]
	public static extern TrackingSpaceType GetTrackingSpaceType();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetIVRDevice()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	[Obsolete("This is obsolete, and should no longer be used.  Please use XRInputSubsystem.TrySetTrackingOriginMode.")]
	public static extern bool SetTrackingSpaceType(TrackingSpaceType trackingSpaceType);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("DisableAutoVRCameraTracking")]
	[StaticAccessor("GetIVRDevice()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	public static extern void DisableAutoXRCameraTracking([NotNull("ArgumentNullException")] Camera camera, bool disabled);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("UpdateEyeTextureMSAASetting")]
	[StaticAccessor("GetIVRDeviceScripting()", StaticAccessorType.ArrowWithDefaultReturnIfNull)]
	public static extern void UpdateEyeTextureMSAASetting();

	[RequiredByNativeCode]
	private static void InvokeDeviceLoaded(string loadedDeviceName)
	{
		if (XRDevice.deviceLoaded != null)
		{
			XRDevice.deviceLoaded(loadedDeviceName);
		}
	}
}
