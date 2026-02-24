using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Android;

[NativeConditional("PLATFORM_ANDROID")]
[StaticAccessor("AndroidApp", StaticAccessorType.DoubleColon)]
[NativeHeader("Modules/AndroidJNI/Public/AndroidApp.bindings.h")]
internal static class AndroidApp
{
	private static AndroidJavaObject m_Context;

	private static AndroidJavaObject m_Activity;

	private static AndroidJavaObject m_UnityPlayer;

	public static AndroidJavaObject Context
	{
		get
		{
			AcquireContextAndActivity();
			return m_Context;
		}
	}

	public static AndroidJavaObject Activity
	{
		get
		{
			AcquireContextAndActivity();
			return m_Activity;
		}
	}

	public static extern IntPtr UnityPlayerRaw
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[ThreadSafe]
		get;
	}

	public static AndroidJavaObject UnityPlayer
	{
		get
		{
			if (m_UnityPlayer != null)
			{
				return m_UnityPlayer;
			}
			m_UnityPlayer = new AndroidJavaObject(UnityPlayerRaw);
			return m_UnityPlayer;
		}
	}

	private static void AcquireContextAndActivity()
	{
		if (m_Context != null)
		{
			return;
		}
		using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		m_Context = androidJavaClass.GetStatic<AndroidJavaObject>("currentContext");
		m_Activity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
	}
}
