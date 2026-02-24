using Unity.Collections;
using UnityEngine;

internal static class _0024BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		AllocatorManager.Initialize_0024StackAllocator_Try_000000AB_0024BurstDirectCall();
		AllocatorManager.Initialize_0024SlabAllocator_Try_000000B9_0024BurstDirectCall();
		AutoFreeAllocator.Try_000000E3_0024BurstDirectCall.Initialize();
		RewindableAllocator.Try_000009DE_0024BurstDirectCall.Initialize();
		xxHash3.Hash64Long_00000A73_0024BurstDirectCall.Initialize();
		xxHash3.Hash128Long_00000A7A_0024BurstDirectCall.Initialize();
	}
}
