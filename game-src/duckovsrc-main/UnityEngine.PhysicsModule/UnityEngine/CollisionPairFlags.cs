namespace UnityEngine;

internal enum CollisionPairFlags : ushort
{
	RemovedShape = 1,
	RemovedOtherShape = 2,
	ActorPairHasFirstTouch = 4,
	ActorPairLostTouch = 8,
	InternalHasImpulses = 0x10,
	InternalContactsAreFlipped = 0x20
}
