using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Internal;

namespace UnityEngine;

[NativeHeader("Runtime/Input/InputBindings.h")]
public class Input
{
	private static LocationService locationServiceInstance;

	private static Compass compassInstance;

	private static Gyroscope s_MainGyro;

	public static extern bool simulateMouseWithTouches
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	[NativeThrows]
	public static extern bool anyKey
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeThrows]
	public static extern bool anyKeyDown
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeThrows]
	public static extern string inputString
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[NativeThrows]
	public static Vector3 mousePosition
	{
		get
		{
			get_mousePosition_Injected(out var ret);
			return ret;
		}
	}

	[NativeThrows]
	public static Vector2 mouseScrollDelta
	{
		get
		{
			get_mouseScrollDelta_Injected(out var ret);
			return ret;
		}
	}

	public static extern IMECompositionMode imeCompositionMode
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern string compositionString
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static extern bool imeIsSelected
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	public static Vector2 compositionCursorPos
	{
		get
		{
			get_compositionCursorPos_Injected(out var ret);
			return ret;
		}
		set
		{
			set_compositionCursorPos_Injected(ref value);
		}
	}

	[Obsolete("eatKeyPressOnTextFieldFocus property is deprecated, and only provided to support legacy behavior.")]
	public static extern bool eatKeyPressOnTextFieldFocus
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool mousePresent
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetMousePresent")]
		get;
	}

	public static extern int penEventCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetPenEventCount")]
		get;
	}

	public static extern int touchCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetTouchCount")]
		get;
	}

	public static extern bool touchPressureSupported
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("IsTouchPressureSupported")]
		get;
	}

	public static extern bool stylusTouchSupported
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("IsStylusTouchSupported")]
		get;
	}

	public static extern bool touchSupported
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("IsTouchSupported")]
		get;
	}

	public static extern bool multiTouchEnabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("IsMultiTouchEnabled")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("SetMultiTouchEnabled")]
		set;
	}

	[Obsolete("isGyroAvailable property is deprecated. Please use SystemInfo.supportsGyroscope instead.")]
	public static extern bool isGyroAvailable
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("IsGyroAvailable")]
		get;
	}

	public static extern DeviceOrientation deviceOrientation
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetDeviceOrientation")]
		get;
	}

	public static Vector3 acceleration
	{
		[FreeFunction("GetAcceleration")]
		get
		{
			get_acceleration_Injected(out var ret);
			return ret;
		}
	}

	public static extern bool compensateSensors
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("IsCompensatingSensors")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("SetCompensatingSensors")]
		set;
	}

	public static extern int accelerationEventCount
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetAccelerationCount")]
		get;
	}

	public static extern bool backButtonLeavesApp
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("GetBackButtonLeavesApp")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[FreeFunction("SetBackButtonLeavesApp")]
		set;
	}

	public static LocationService location
	{
		get
		{
			if (locationServiceInstance == null)
			{
				locationServiceInstance = new LocationService();
			}
			return locationServiceInstance;
		}
	}

	public static Compass compass
	{
		get
		{
			if (compassInstance == null)
			{
				compassInstance = new Compass();
			}
			return compassInstance;
		}
	}

	public static Gyroscope gyro
	{
		get
		{
			if (s_MainGyro == null)
			{
				s_MainGyro = new Gyroscope(GetGyroInternal());
			}
			return s_MainGyro;
		}
	}

	public static Touch[] touches
	{
		get
		{
			int num = touchCount;
			Touch[] array = new Touch[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = GetTouch(i);
			}
			return array;
		}
	}

	public static AccelerationEvent[] accelerationEvents
	{
		get
		{
			int num = accelerationEventCount;
			AccelerationEvent[] array = new AccelerationEvent[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = GetAccelerationEvent(i);
			}
			return array;
		}
	}

	public static float GetAxis(string axisName)
	{
		return InputUnsafeUtility.GetAxis(axisName);
	}

	public static float GetAxisRaw(string axisName)
	{
		return InputUnsafeUtility.GetAxisRaw(axisName);
	}

	public static bool GetButton(string buttonName)
	{
		return InputUnsafeUtility.GetButton(buttonName);
	}

	public static bool GetButtonDown(string buttonName)
	{
		return InputUnsafeUtility.GetButtonDown(buttonName);
	}

	public static bool GetButtonUp(string buttonName)
	{
		return InputUnsafeUtility.GetButtonUp(buttonName);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private static extern bool GetKeyInt(KeyCode key);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private static extern bool GetKeyUpInt(KeyCode key);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private static extern bool GetKeyDownInt(KeyCode key);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern bool GetMouseButton(int button);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern bool GetMouseButtonDown(int button);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern bool GetMouseButtonUp(int button);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("ResetInput")]
	public static extern void ResetInputAxes();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern string[] GetJoystickNames();

	[NativeThrows]
	public static Touch GetTouch(int index)
	{
		GetTouch_Injected(index, out var ret);
		return ret;
	}

	[NativeThrows]
	public static PenData GetPenEvent(int index)
	{
		GetPenEvent_Injected(index, out var ret);
		return ret;
	}

	[NativeThrows]
	public static PenData GetLastPenContactEvent()
	{
		GetLastPenContactEvent_Injected(out var ret);
		return ret;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void ResetPenEvents();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void ClearLastPenContactEvent();

	[NativeThrows]
	public static AccelerationEvent GetAccelerationEvent(int index)
	{
		GetAccelerationEvent_Injected(index, out var ret);
		return ret;
	}

	public static bool GetKey(KeyCode key)
	{
		return GetKeyInt(key);
	}

	public static bool GetKey(string name)
	{
		return InputUnsafeUtility.GetKeyString(name);
	}

	public static bool GetKeyUp(KeyCode key)
	{
		return GetKeyUpInt(key);
	}

	public static bool GetKeyUp(string name)
	{
		return InputUnsafeUtility.GetKeyUpString(name);
	}

	public static bool GetKeyDown(KeyCode key)
	{
		return GetKeyDownInt(key);
	}

	public static bool GetKeyDown(string name)
	{
		return InputUnsafeUtility.GetKeyDownString(name);
	}

	[Conditional("UNITY_EDITOR")]
	internal static void SimulateTouch(Touch touch)
	{
	}

	[FreeFunction("SimulateTouch")]
	[NativeConditional("UNITY_EDITOR")]
	[Conditional("UNITY_EDITOR")]
	private static void SimulateTouchInternal(Touch touch, long timestamp)
	{
		SimulateTouchInternal_Injected(ref touch, timestamp);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("GetGyro")]
	private static extern int GetGyroInternal();

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern bool CheckDisabled();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetTouch_Injected(int index, out Touch ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetPenEvent_Injected(int index, out PenData ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetLastPenContactEvent_Injected(out PenData ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void GetAccelerationEvent_Injected(int index, out AccelerationEvent ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SimulateTouchInternal_Injected(ref Touch touch, long timestamp);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_mousePosition_Injected(out Vector3 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_mouseScrollDelta_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_compositionCursorPos_Injected(out Vector2 ret);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void set_compositionCursorPos_Injected(ref Vector2 value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SpecialName]
	private static extern void get_acceleration_Injected(out Vector3 ret);
}
