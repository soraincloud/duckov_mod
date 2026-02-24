using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

namespace UnityEngine.PlayerLoop;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[RequiredByNativeCode]
public struct TimeUpdate
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[RequiredByNativeCode]
	public struct WaitForLastPresentationAndUpdateTime
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[Obsolete("ProfilerStartFrame player loop component has been moved to the Initialization category. (UnityUpgradable) -> UnityEngine.PlayerLoop.Initialization/ProfilerStartFrame", true)]
	public struct ProfilerStartFrame
	{
	}
}
