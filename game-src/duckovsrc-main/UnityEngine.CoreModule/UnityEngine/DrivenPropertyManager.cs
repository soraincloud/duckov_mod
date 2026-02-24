using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[NativeHeader("Editor/Src/Properties/DrivenPropertyManager.h")]
internal class DrivenPropertyManager
{
	[Conditional("UNITY_EDITOR")]
	public static void RegisterProperty(Object driver, Object target, string propertyPath)
	{
		RegisterPropertyPartial(driver, target, propertyPath);
	}

	[Conditional("UNITY_EDITOR")]
	public static void TryRegisterProperty(Object driver, Object target, string propertyPath)
	{
		TryRegisterPropertyPartial(driver, target, propertyPath);
	}

	[Conditional("UNITY_EDITOR")]
	public static void UnregisterProperty(Object driver, Object target, string propertyPath)
	{
		UnregisterPropertyPartial(driver, target, propertyPath);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[Conditional("UNITY_EDITOR")]
	[NativeConditional("UNITY_EDITOR")]
	[StaticAccessor("GetDrivenPropertyManager()", StaticAccessorType.Dot)]
	public static extern void UnregisterProperties([NotNull("ArgumentNullException")] Object driver);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetDrivenPropertyManager()", StaticAccessorType.Dot)]
	[NativeConditional("UNITY_EDITOR")]
	private static extern void RegisterPropertyPartial([NotNull("ArgumentNullException")] Object driver, [NotNull("ArgumentNullException")] Object target, [NotNull("ArgumentNullException")] string propertyPath);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("UNITY_EDITOR")]
	[StaticAccessor("GetDrivenPropertyManager()", StaticAccessorType.Dot)]
	private static extern void TryRegisterPropertyPartial([NotNull("ArgumentNullException")] Object driver, [NotNull("ArgumentNullException")] Object target, [NotNull("ArgumentNullException")] string propertyPath);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetDrivenPropertyManager()", StaticAccessorType.Dot)]
	[NativeConditional("UNITY_EDITOR")]
	private static extern void UnregisterPropertyPartial([NotNull("ArgumentNullException")] Object driver, [NotNull("ArgumentNullException")] Object target, [NotNull("ArgumentNullException")] string propertyPath);
}
