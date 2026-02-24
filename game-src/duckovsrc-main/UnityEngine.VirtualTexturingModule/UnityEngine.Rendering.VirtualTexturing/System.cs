using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Rendering.VirtualTexturing;

[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
[StaticAccessor("VirtualTexturing::System", StaticAccessorType.DoubleColon)]
public static class System
{
	public const int AllMips = int.MaxValue;

	internal static extern bool enabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern void Update();

	[NativeThrows]
	internal static void SetDebugFlag(Guid guid, bool enabled)
	{
		SetDebugFlagInteger(guid.ToByteArray(), enabled ? 1 : 0);
	}

	[NativeThrows]
	internal static void SetDebugFlagInteger(Guid guid, long value)
	{
		SetDebugFlagInteger(guid.ToByteArray(), value);
	}

	[NativeThrows]
	internal static void SetDebugFlagDouble(Guid guid, double value)
	{
		SetDebugFlagDouble(guid.ToByteArray(), value);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private static extern void SetDebugFlagInteger(byte[] guid, long value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	private static extern void SetDebugFlagDouble(byte[] guid, double value);
}
