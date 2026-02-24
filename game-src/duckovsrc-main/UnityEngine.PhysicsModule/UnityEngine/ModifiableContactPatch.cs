namespace UnityEngine;

internal struct ModifiableContactPatch
{
	public enum Flags
	{
		HasFaceIndices = 1,
		HasModifiedMassRatios = 8,
		HasTargetVelocity = 0x10,
		HasMaxImpulse = 0x20,
		RegeneratePatches = 0x40
	}

	public ModifiableMassProperties massProperties;

	public Vector3 normal;

	public float restitution;

	public float dynamicFriction;

	public float staticFriction;

	public byte startContactIndex;

	public byte contactCount;

	public byte materialFlags;

	public byte internalFlags;

	public ushort materialIndex;

	public ushort otherMaterialIndex;
}
