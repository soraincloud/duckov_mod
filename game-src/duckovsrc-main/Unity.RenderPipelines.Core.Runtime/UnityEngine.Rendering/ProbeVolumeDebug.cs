using System;

namespace UnityEngine.Rendering;

internal class ProbeVolumeDebug : IDebugData
{
	public bool drawProbes;

	public bool drawBricks;

	public bool drawCells;

	public bool realtimeSubdivision;

	public int subdivisionCellUpdatePerFrame = 4;

	public float subdivisionDelayInSeconds = 1f;

	public DebugProbeShadingMode probeShading;

	public float probeSize = 0.3f;

	public float subdivisionViewCullingDistance = 500f;

	public float probeCullingDistance = 200f;

	public int maxSubdivToVisualize = 7;

	public int minSubdivToVisualize;

	public float exposureCompensation;

	public bool drawVirtualOffsetPush;

	public float offsetSize = 0.025f;

	public bool freezeStreaming;

	public int otherStateIndex;

	public ProbeVolumeDebug()
	{
		Init();
	}

	private void Init()
	{
		drawProbes = false;
		drawBricks = false;
		drawCells = false;
		realtimeSubdivision = false;
		subdivisionCellUpdatePerFrame = 4;
		subdivisionDelayInSeconds = 1f;
		probeShading = DebugProbeShadingMode.SH;
		probeSize = 0.3f;
		subdivisionViewCullingDistance = 500f;
		probeCullingDistance = 200f;
		maxSubdivToVisualize = 7;
		minSubdivToVisualize = 0;
		exposureCompensation = 0f;
		drawVirtualOffsetPush = false;
		offsetSize = 0.025f;
		freezeStreaming = false;
		otherStateIndex = 0;
	}

	public Action GetReset()
	{
		return delegate
		{
			Init();
		};
	}
}
