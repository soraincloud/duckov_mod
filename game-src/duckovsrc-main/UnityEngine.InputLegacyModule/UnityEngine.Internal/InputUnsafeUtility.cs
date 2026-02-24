using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Internal;

[NativeHeader("Runtime/Input/InputBindings.h")]
internal static class InputUnsafeUtility
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool GetKeyString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[RequiredMember]
	internal unsafe static extern bool GetKeyString__Unmanaged(byte* name, int nameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool GetKeyUpString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[RequiredMember]
	internal unsafe static extern bool GetKeyUpString__Unmanaged(byte* name, int nameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool GetKeyDownString(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[RequiredMember]
	internal unsafe static extern bool GetKeyDownString__Unmanaged(byte* name, int nameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern float GetAxis(string axisName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[RequiredMember]
	internal unsafe static extern float GetAxis__Unmanaged(byte* axisName, int axisNameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern float GetAxisRaw(string axisName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RequiredMember]
	[NativeThrows]
	internal unsafe static extern float GetAxisRaw__Unmanaged(byte* axisName, int axisNameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool GetButton(string buttonName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[RequiredMember]
	internal unsafe static extern bool GetButton__Unmanaged(byte* buttonName, int buttonNameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool GetButtonDown(string buttonName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	[RequiredMember]
	internal unsafe static extern byte GetButtonDown__Unmanaged(byte* buttonName, int buttonNameLen);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern bool GetButtonUp(string buttonName);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[RequiredMember]
	[NativeThrows]
	internal unsafe static extern bool GetButtonUp__Unmanaged(byte* buttonName, int buttonNameLen);
}
