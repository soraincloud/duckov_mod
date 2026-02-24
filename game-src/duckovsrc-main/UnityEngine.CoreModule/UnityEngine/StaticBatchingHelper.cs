using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[NativeHeader("Runtime/Graphics/Mesh/StaticBatching.h")]
internal struct StaticBatchingHelper
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction("StaticBatching::CombineMeshesForStaticBatching")]
	internal static extern void CombineMeshes(GameObject[] gos, GameObject staticBatchRoot);
}
