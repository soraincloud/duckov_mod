using System.Runtime.CompilerServices;
using UnityEngine.Bindings;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.VirtualTexturing;

[StaticAccessor("VirtualTexturing::Editor", StaticAccessorType.DoubleColon)]
[NativeConditional("UNITY_EDITOR")]
[NativeHeader("Modules/VirtualTexturing/ScriptBindings/VirtualTexturing.bindings.h")]
public static class EditorHelpers
{
	[NativeHeader("Runtime/Shaders/SharedMaterialData.h")]
	internal struct StackValidationResult
	{
		public string stackName;

		public string errorMessage;
	}

	[NativeThrows]
	internal static extern int tileSize
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	public static extern bool ValidateTextureStack([Unmarshalled][NotNull("ArgumentNullException")] Texture[] textures, out string errorMessage);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeThrows]
	internal static extern StackValidationResult[] ValidateMaterialTextureStacks([NotNull("ArgumentNullException")] Material mat);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("UNITY_EDITOR", "{}")]
	[NativeThrows]
	public static extern GraphicsFormat[] QuerySupportedFormats();
}
