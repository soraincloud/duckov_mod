using System.Runtime.InteropServices;
using UnityEngine.Scripting;

namespace Unity.Profiling.LowLevel.Unsafe;

[StructLayout(LayoutKind.Explicit, Size = 24)]
[UsedByNativeCode]
public readonly struct ProfilerRecorderDescription
{
	[FieldOffset(0)]
	private readonly ProfilerCategory category;

	[FieldOffset(2)]
	private readonly MarkerFlags flags;

	[FieldOffset(4)]
	private readonly ProfilerMarkerDataType dataType;

	[FieldOffset(5)]
	private readonly ProfilerMarkerDataUnit unitType;

	[FieldOffset(8)]
	private readonly int reserved0;

	[FieldOffset(12)]
	private readonly int nameUtf8Len;

	[FieldOffset(16)]
	private unsafe readonly byte* nameUtf8;

	public ProfilerCategory Category => category;

	public MarkerFlags Flags => flags;

	public ProfilerMarkerDataType DataType => dataType;

	public ProfilerMarkerDataUnit UnitType => unitType;

	public int NameUtf8Len => nameUtf8Len;

	public unsafe byte* NameUtf8 => nameUtf8;

	public unsafe string Name => ProfilerUnsafeUtility.Utf8ToString(nameUtf8, nameUtf8Len);
}
