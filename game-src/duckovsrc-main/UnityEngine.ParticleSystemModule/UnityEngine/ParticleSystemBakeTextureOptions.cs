using System;

namespace UnityEngine;

[Flags]
public enum ParticleSystemBakeTextureOptions
{
	BakeRotationAndScale = 1,
	BakePosition = 2,
	PerVertex = 4,
	PerParticle = 8,
	IncludeParticleIndices = 0x10,
	Default = 4
}
