using System;

namespace UnityEngine.UIElements;

[Flags]
internal enum RenderHints
{
	None = 0,
	GroupTransform = 1,
	BoneTransform = 2,
	ClipWithScissors = 4,
	MaskContainer = 8,
	DynamicColor = 0x10,
	DirtyOffset = 5,
	DirtyGroupTransform = 0x20,
	DirtyBoneTransform = 0x40,
	DirtyClipWithScissors = 0x80,
	DirtyMaskContainer = 0x100,
	DirtyDynamicColor = 0x200,
	DirtyAll = 0x3E0
}
