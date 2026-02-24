using System.Runtime.CompilerServices;
using UnityEngine.Bindings;

namespace Unity.IO.LowLevel.Unsafe;

[NativeHeader("Runtime/VirtualFileSystem/VirtualFileSystem.h")]
[StaticAccessor("GetFileSystem()", StaticAccessorType.Dot)]
public static class VirtualFileSystem
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	[FreeFunction(IsThreadSafe = true)]
	public static extern bool GetLocalFileSystemName(string vfsFileName, out string localFileName, out ulong localFileOffset, out ulong localFileSize);

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern string ToLogicalPath(string physicalPath);
}
