using System;

namespace UnityEngine.Rendering;

[Serializable]
public enum ProbeVolumeBlendingTextureMemoryBudget
{
	None = 0,
	MemoryBudgetLow = 0x80,
	MemoryBudgetMedium = 0x100,
	MemoryBudgetHigh = 0x200
}
