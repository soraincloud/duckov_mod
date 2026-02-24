namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Packages\\com.unity.render-pipelines.core@14.0.11\\Runtime\\Lighting\\ProbeVolume\\ShaderVariablesProbeVolumes.cs", needAccessors = false, generateCBuffer = true, constantRegister = 5)]
internal struct ShaderVariablesProbeVolumes
{
	public Vector4 _PoolDim_CellInMeters;

	public Vector4 _MinCellPos_Noise;

	public Vector4 _IndicesDim_IndexChunkSize;

	public Vector4 _Biases_CellInMinBrick_MinBrickSize;

	public Vector4 _LeakReductionParams;

	public Vector4 _Weight_MinLoadedCell;

	public Vector4 _MaxLoadedCell_FrameIndex;

	public Vector4 _NormalizationClamp_Padding12;
}
