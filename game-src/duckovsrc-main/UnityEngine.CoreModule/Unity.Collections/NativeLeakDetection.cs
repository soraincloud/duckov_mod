using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

public static class NativeLeakDetection
{
	public static NativeLeakDetectionMode Mode
	{
		get
		{
			return UnsafeUtility.GetLeakDetectionMode();
		}
		set
		{
			if (value < NativeLeakDetectionMode.Disabled || value > NativeLeakDetectionMode.EnabledWithStackTrace)
			{
				throw new ArgumentException("NativeLeakDetectionMode out of range");
			}
			UnsafeUtility.SetLeakDetectionMode(value);
		}
	}
}
