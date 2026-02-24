using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential)]
[NativeHeader("Modules/AssetBundle/Public/AssetBundleLoadAssetOperation.h")]
[RequiredByNativeCode]
public class AssetBundleRequest : ResourceRequest
{
	public new Object asset => GetResult();

	public extern Object[] allAssets
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod("GetAllLoadedAssets")]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod("GetLoadedAsset")]
	protected override extern Object GetResult();
}
