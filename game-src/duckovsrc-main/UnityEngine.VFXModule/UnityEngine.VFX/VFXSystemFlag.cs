namespace UnityEngine.VFX;

internal enum VFXSystemFlag
{
	SystemDefault = 0,
	SystemHasKill = 1,
	SystemHasIndirectBuffer = 2,
	SystemReceivedEventGPU = 4,
	SystemHasStrips = 8,
	SystemNeedsComputeBounds = 0x10,
	SystemAutomaticBounds = 0x20,
	SystemInWorldSpace = 0x40,
	SystemHasDirectLink = 0x80,
	SystemHasAttributeBuffer = 0x100,
	SystemUsesInstancedRendering = 0x200
}
