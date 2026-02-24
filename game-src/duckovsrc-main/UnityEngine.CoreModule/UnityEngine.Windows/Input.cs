using System;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine.Windows;

[NativeHeader("PlatformDependent/Win/Bindings/InputBindings.h")]
public static class Input
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("ForwardRawInput")]
	[StaticAccessor("", StaticAccessorType.DoubleColon)]
	[ThreadSafe]
	private unsafe static extern void ForwardRawInputImpl(uint* rawInputHeaderIndices, uint* rawInputDataIndices, uint indicesCount, byte* rawInputData, uint rawInputDataSize);

	public unsafe static void ForwardRawInput(IntPtr rawInputHeaderIndices, IntPtr rawInputDataIndices, uint indicesCount, IntPtr rawInputData, uint rawInputDataSize)
	{
		ForwardRawInput((uint*)(void*)rawInputHeaderIndices, (uint*)(void*)rawInputDataIndices, indicesCount, (byte*)(void*)rawInputData, rawInputDataSize);
	}

	public unsafe static void ForwardRawInput(uint* rawInputHeaderIndices, uint* rawInputDataIndices, uint indicesCount, byte* rawInputData, uint rawInputDataSize)
	{
		if (rawInputHeaderIndices == null)
		{
			throw new ArgumentNullException("rawInputHeaderIndices");
		}
		if (rawInputDataIndices == null)
		{
			throw new ArgumentNullException("rawInputDataIndices");
		}
		if (rawInputData == null)
		{
			throw new ArgumentNullException("rawInputData");
		}
		ForwardRawInputImpl(rawInputHeaderIndices, rawInputDataIndices, indicesCount, rawInputData, rawInputDataSize);
	}
}
