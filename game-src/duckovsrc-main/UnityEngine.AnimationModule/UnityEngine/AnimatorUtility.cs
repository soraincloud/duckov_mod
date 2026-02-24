using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Modules/Animation/OptimizeTransformHierarchy.h")]
public class AnimatorUtility
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction]
	public static extern void OptimizeTransformHierarchy([NotNull("NullExceptionObject")] GameObject go, string[] exposedTransforms);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction]
	public static extern void DeoptimizeTransformHierarchy([NotNull("NullExceptionObject")] GameObject go);
}
