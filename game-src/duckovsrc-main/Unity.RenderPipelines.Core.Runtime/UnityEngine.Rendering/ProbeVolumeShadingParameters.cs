namespace UnityEngine.Rendering;

public struct ProbeVolumeShadingParameters
{
	public float normalBias;

	public float viewBias;

	public bool scaleBiasByMinDistanceBetweenProbes;

	public float samplingNoise;

	public float weight;

	public APVLeakReductionMode leakReductionMode;

	public float occlusionWeightContribution;

	public float minValidNormalWeight;

	public int frameIndexForNoise;

	public float reflNormalizationLowerClamp;

	public float reflNormalizationUpperClamp;
}
