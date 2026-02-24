using System.Collections.Generic;

namespace UnityEngine.Rendering;

internal class ProbeBrickBlendingPool
{
	private static ComputeShader stateBlendShader;

	private static int scenarioBlendingKernel = -1;

	private static readonly int _PoolDim_LerpFactor = Shader.PropertyToID("_PoolDim_LerpFactor");

	private static readonly int _ChunkList = Shader.PropertyToID("_ChunkList");

	private static readonly int _State0_L0_L1Rx = Shader.PropertyToID("_State0_L0_L1Rx");

	private static readonly int _State0_L1G_L1Ry = Shader.PropertyToID("_State0_L1G_L1Ry");

	private static readonly int _State0_L1B_L1Rz = Shader.PropertyToID("_State0_L1B_L1Rz");

	private static readonly int _State0_L2_0 = Shader.PropertyToID("_State0_L2_0");

	private static readonly int _State0_L2_1 = Shader.PropertyToID("_State0_L2_1");

	private static readonly int _State0_L2_2 = Shader.PropertyToID("_State0_L2_2");

	private static readonly int _State0_L2_3 = Shader.PropertyToID("_State0_L2_3");

	private static readonly int _State1_L0_L1Rx = Shader.PropertyToID("_State1_L0_L1Rx");

	private static readonly int _State1_L1G_L1Ry = Shader.PropertyToID("_State1_L1G_L1Ry");

	private static readonly int _State1_L1B_L1Rz = Shader.PropertyToID("_State1_L1B_L1Rz");

	private static readonly int _State1_L2_0 = Shader.PropertyToID("_State1_L2_0");

	private static readonly int _State1_L2_1 = Shader.PropertyToID("_State1_L2_1");

	private static readonly int _State1_L2_2 = Shader.PropertyToID("_State1_L2_2");

	private static readonly int _State1_L2_3 = Shader.PropertyToID("_State1_L2_3");

	private static readonly int _Out_L0_L1Rx = Shader.PropertyToID("_Out_L0_L1Rx");

	private static readonly int _Out_L1G_L1Ry = Shader.PropertyToID("_Out_L1G_L1Ry");

	private static readonly int _Out_L1B_L1Rz = Shader.PropertyToID("_Out_L1B_L1Rz");

	private static readonly int _Out_L2_0 = Shader.PropertyToID("_Out_L2_0");

	private static readonly int _Out_L2_1 = Shader.PropertyToID("_Out_L2_1");

	private static readonly int _Out_L2_2 = Shader.PropertyToID("_Out_L2_2");

	private static readonly int _Out_L2_3 = Shader.PropertyToID("_Out_L2_3");

	private Vector4[] m_ChunkList;

	private int m_MappedChunks;

	private ProbeBrickPool m_State0;

	private ProbeBrickPool m_State1;

	private ProbeVolumeTextureMemoryBudget m_MemoryBudget;

	private ProbeVolumeSHBands m_ShBands;

	internal static bool isSupported => stateBlendShader != null;

	internal bool isAllocated => m_State0 != null;

	internal int estimatedVMemCost
	{
		get
		{
			if (!isAllocated)
			{
				return 0;
			}
			return m_State0.estimatedVMemCost + m_State1.estimatedVMemCost;
		}
	}

	internal static void Initialize(in ProbeVolumeSystemParameters parameters)
	{
		stateBlendShader = parameters.scenarioBlendingShader;
		scenarioBlendingKernel = (stateBlendShader ? stateBlendShader.FindKernel("BlendScenarios") : (-1));
	}

	internal int GetPoolWidth()
	{
		return m_State0.m_Pool.width;
	}

	internal int GetPoolHeight()
	{
		return m_State0.m_Pool.height;
	}

	internal int GetPoolDepth()
	{
		return m_State0.m_Pool.depth;
	}

	internal ProbeBrickBlendingPool(ProbeVolumeBlendingTextureMemoryBudget memoryBudget, ProbeVolumeSHBands shBands)
	{
		m_MemoryBudget = (ProbeVolumeTextureMemoryBudget)memoryBudget;
		m_ShBands = shBands;
	}

	internal void AllocateResourcesIfNeeded()
	{
		if (!isAllocated)
		{
			m_State0 = new ProbeBrickPool(m_MemoryBudget, m_ShBands, allocateValidityData: false);
			m_State1 = new ProbeBrickPool(m_MemoryBudget, m_ShBands, allocateValidityData: false);
			int num = GetPoolWidth() / 512 * (GetPoolHeight() / 4) * (GetPoolDepth() / 4);
			m_ChunkList = new Vector4[num];
			m_MappedChunks = 0;
		}
	}

	internal void Update(ProbeBrickPool.DataLocation source, List<ProbeBrickPool.BrickChunkAlloc> srcLocations, List<ProbeBrickPool.BrickChunkAlloc> dstLocations, int destStartIndex, ProbeVolumeSHBands bands, int state)
	{
		((state == 0) ? m_State0 : m_State1).Update(source, srcLocations, dstLocations, destStartIndex, bands);
	}

	private static int DivRoundUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	internal void PerformBlending(CommandBuffer cmd, float factor, ProbeBrickPool dstPool)
	{
		if (m_MappedChunks != 0)
		{
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L0_L1Rx, m_State0.m_Pool.TexL0_L1rx);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L1G_L1Ry, m_State0.m_Pool.TexL1_G_ry);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L1B_L1Rz, m_State0.m_Pool.TexL1_B_rz);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L0_L1Rx, m_State1.m_Pool.TexL0_L1rx);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L1G_L1Ry, m_State1.m_Pool.TexL1_G_ry);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L1B_L1Rz, m_State1.m_Pool.TexL1_B_rz);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L0_L1Rx, dstPool.m_Pool.TexL0_L1rx);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L1G_L1Ry, dstPool.m_Pool.TexL1_G_ry);
			cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L1B_L1Rz, dstPool.m_Pool.TexL1_B_rz);
			if (m_ShBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
			{
				stateBlendShader.EnableKeyword("PROBE_VOLUMES_L2");
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L2_0, m_State0.m_Pool.TexL2_0);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L2_1, m_State0.m_Pool.TexL2_1);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L2_2, m_State0.m_Pool.TexL2_2);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State0_L2_3, m_State0.m_Pool.TexL2_3);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L2_0, m_State1.m_Pool.TexL2_0);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L2_1, m_State1.m_Pool.TexL2_1);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L2_2, m_State1.m_Pool.TexL2_2);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _State1_L2_3, m_State1.m_Pool.TexL2_3);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L2_0, dstPool.m_Pool.TexL2_0);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L2_1, dstPool.m_Pool.TexL2_1);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L2_2, dstPool.m_Pool.TexL2_2);
				cmd.SetComputeTextureParam(stateBlendShader, scenarioBlendingKernel, _Out_L2_3, dstPool.m_Pool.TexL2_3);
			}
			else
			{
				stateBlendShader.DisableKeyword("PROBE_VOLUMES_L2");
			}
			Vector4 val = new Vector4(dstPool.GetPoolWidth(), dstPool.GetPoolHeight(), factor, 0f);
			int threadGroupsX = DivRoundUp(512, 4);
			int threadGroupsY = DivRoundUp(4, 4);
			int num = DivRoundUp(4, 4);
			cmd.SetComputeVectorArrayParam(stateBlendShader, _ChunkList, m_ChunkList);
			cmd.SetComputeVectorParam(stateBlendShader, _PoolDim_LerpFactor, val);
			cmd.DispatchCompute(stateBlendShader, scenarioBlendingKernel, threadGroupsX, threadGroupsY, num * m_MappedChunks);
			m_MappedChunks = 0;
		}
	}

	internal void BlendChunks(ProbeReferenceVolume.BlendingCellInfo blendingCell, ProbeBrickPool dstPool)
	{
		for (int i = 0; i < blendingCell.chunkList.Count; i++)
		{
			ProbeBrickPool.BrickChunkAlloc brickChunkAlloc = blendingCell.chunkList[i];
			int num = blendingCell.cellInfo.chunkList[i].flattenIndex(dstPool.GetPoolWidth(), dstPool.GetPoolHeight());
			m_ChunkList[m_MappedChunks++] = new Vector4(brickChunkAlloc.x, brickChunkAlloc.y, brickChunkAlloc.z, num);
		}
	}

	internal void Clear()
	{
		m_State0?.Clear();
	}

	internal bool Allocate(int numberOfBrickChunks, List<ProbeBrickPool.BrickChunkAlloc> outAllocations)
	{
		AllocateResourcesIfNeeded();
		if (numberOfBrickChunks > m_State0.GetRemainingChunkCount())
		{
			return false;
		}
		return m_State0.Allocate(numberOfBrickChunks, outAllocations, ignoreErrorLog: false);
	}

	internal void Deallocate(List<ProbeBrickPool.BrickChunkAlloc> allocations)
	{
		if (allocations.Count != 0)
		{
			m_State0.Deallocate(allocations);
		}
	}

	internal void EnsureTextureValidity()
	{
		if (isAllocated)
		{
			m_State0.EnsureTextureValidity();
			m_State1.EnsureTextureValidity();
		}
	}

	internal void Cleanup()
	{
		if (isAllocated)
		{
			m_State0.Cleanup();
			m_State1.Cleanup();
		}
	}
}
