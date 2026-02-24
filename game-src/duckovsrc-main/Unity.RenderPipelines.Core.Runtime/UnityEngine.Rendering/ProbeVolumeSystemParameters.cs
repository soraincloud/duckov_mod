namespace UnityEngine.Rendering;

public struct ProbeVolumeSystemParameters
{
	public ProbeVolumeTextureMemoryBudget memoryBudget;

	public ProbeVolumeBlendingTextureMemoryBudget blendingMemoryBudget;

	public Mesh probeDebugMesh;

	public Shader probeDebugShader;

	public Mesh offsetDebugMesh;

	public Shader offsetDebugShader;

	public ComputeShader scenarioBlendingShader;

	public ProbeVolumeSceneData sceneData;

	public ProbeVolumeSHBands shBands;

	public bool supportsRuntimeDebug;

	public bool supportStreaming;
}
