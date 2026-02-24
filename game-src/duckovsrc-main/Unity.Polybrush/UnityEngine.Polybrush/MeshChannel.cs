using System;

namespace UnityEngine.Polybrush;

[Flags]
internal enum MeshChannel
{
	Null = 0,
	Position = 1,
	Normal = 2,
	Color = 4,
	Tangent = 8,
	UV0 = 0x10,
	UV2 = 0x20,
	UV3 = 0x40,
	UV4 = 0x80,
	All = 0xFF
}
