using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace UnityEngine;

[StaticAccessor("GetQualitySettings()", StaticAccessorType.Dot)]
[NativeHeader("Runtime/Graphics/QualitySettings.h")]
public static class TextureMipmapLimitGroups
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("GetTextureMipmapLimitGroupNames")]
	public static extern string[] GetGroups();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeName("HasTextureMipmapLimitGroup")]
	public static extern bool HasGroup([NotNull("ArgumentNullException")] string groupName);
}
