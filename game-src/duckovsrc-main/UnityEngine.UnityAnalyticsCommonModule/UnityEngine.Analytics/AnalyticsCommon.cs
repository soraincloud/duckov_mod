using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine.Analytics;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/UnityAnalyticsCommon/Public/UnityAnalyticsCommon.h")]
public static class AnalyticsCommon
{
	[StaticAccessor("GetUnityAnalyticsCommon()", StaticAccessorType.Dot)]
	private static extern bool ugsAnalyticsEnabledInternal
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("UGSAnalyticsUserOptStatus")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("SetUGSAnalyticsUserOptStatus")]
		set;
	}

	public static bool ugsAnalyticsEnabled
	{
		get
		{
			return ugsAnalyticsEnabledInternal;
		}
		set
		{
			ugsAnalyticsEnabledInternal = value;
		}
	}
}
