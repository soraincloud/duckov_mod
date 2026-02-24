using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[RequiredByNativeCode]
[NativeHeader("Modules/AssetBundle/Public/AssetBundleUnloadOperation.h")]
public class AssetBundleUnloadOperation : AsyncOperation
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("WaitForCompletion")]
	public extern void WaitForCompletion();
}
