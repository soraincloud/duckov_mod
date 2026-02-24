using Unity.Profiling;

namespace UnityEngine;

public sealed class StaticBatchingUtility
{
	internal static ProfilerMarker s_CombineMarker = new ProfilerMarker("StaticBatching.Combine");

	public static void Combine(GameObject staticBatchRoot)
	{
		using (s_CombineMarker.Auto())
		{
			CombineRoot(staticBatchRoot);
		}
	}

	public static void Combine(GameObject[] gos, GameObject staticBatchRoot)
	{
		using (s_CombineMarker.Auto())
		{
			StaticBatchingHelper.CombineMeshes(gos, staticBatchRoot);
		}
	}

	private static void CombineRoot(GameObject staticBatchRoot)
	{
		MeshFilter[] array = ((!(staticBatchRoot == null)) ? staticBatchRoot.GetComponentsInChildren<MeshFilter>() : ((MeshFilter[])Object.FindObjectsOfType(typeof(MeshFilter))));
		GameObject[] array2 = new GameObject[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = array[i].gameObject;
		}
		StaticBatchingHelper.CombineMeshes(array2, staticBatchRoot);
	}
}
