using Unity.Burst;
using Unity.Burst.Intrinsics;
using UnityEngine;

internal static class _0024BurstDirectCallInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		BurstCompiler.Initialize_0024BurstCompilerHelper_IsBurstEnabled_00000146_0024BurstDirectCall();
		X86.DoSetCSRTrampoline_0000012A_0024BurstDirectCall.Initialize();
		X86.DoGetCSRTrampoline_0000012B_0024BurstDirectCall.Initialize();
	}
}
