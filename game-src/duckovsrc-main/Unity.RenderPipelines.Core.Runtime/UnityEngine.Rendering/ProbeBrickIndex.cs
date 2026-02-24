using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;

namespace UnityEngine.Rendering;

internal class ProbeBrickIndex
{
	[Serializable]
	[DebuggerDisplay("Brick [{position}, {subdivisionLevel}]")]
	public struct Brick : IEquatable<Brick>
	{
		public Vector3Int position;

		public int subdivisionLevel;

		internal Brick(Vector3Int position, int subdivisionLevel)
		{
			this.position = position;
			this.subdivisionLevel = subdivisionLevel;
		}

		public bool Equals(Brick other)
		{
			if (position == other.position)
			{
				return subdivisionLevel == other.subdivisionLevel;
			}
			return false;
		}
	}

	[DebuggerDisplay("Brick [{brick.position}, {brick.subdivisionLevel}], {flattenedIdx}")]
	private struct ReservedBrick
	{
		public Brick brick;

		public int flattenedIdx;
	}

	private class VoxelMeta
	{
		public ProbeReferenceVolume.Cell cell;

		public List<ushort> brickIndices = new List<ushort>();

		public void Clear()
		{
			cell = null;
			brickIndices.Clear();
		}
	}

	private class BrickMeta
	{
		public HashSet<Vector3Int> voxels = new HashSet<Vector3Int>();

		public List<ReservedBrick> bricks = new List<ReservedBrick>();

		public void Clear()
		{
			voxels.Clear();
			bricks.Clear();
		}
	}

	public struct CellIndexUpdateInfo
	{
		public int firstChunkIndex;

		public int numberOfChunks;

		public int minSubdivInCell;

		public Vector3Int minValidBrickIndexForCellAtMaxRes;

		public Vector3Int maxValidBrickIndexForCellAtMaxResPlusOne;

		public Vector3Int cellPositionInBricksAtMaxRes;
	}

	internal const int kMaxSubdivisionLevels = 7;

	internal const int kIndexChunkSize = 243;

	private BitArray m_IndexChunks;

	private int m_IndexInChunks;

	private int m_NextFreeChunk;

	private int m_AvailableChunkCount;

	private ComputeBuffer m_PhysicalIndexBuffer;

	private int[] m_PhysicalIndexBufferData;

	private Vector3Int m_CenterRS;

	private Dictionary<Vector3Int, List<VoxelMeta>> m_VoxelToBricks;

	private Dictionary<ProbeReferenceVolume.Cell, BrickMeta> m_BricksToVoxels;

	private ObjectPool<BrickMeta> m_BrickMetaPool = new ObjectPool<BrickMeta>(delegate(BrickMeta x)
	{
		x.Clear();
	}, null, collectionCheck: false);

	private ObjectPool<List<VoxelMeta>> m_VoxelMetaListPool = new ObjectPool<List<VoxelMeta>>(delegate(List<VoxelMeta> x)
	{
		x.Clear();
	}, null, collectionCheck: false);

	private ObjectPool<VoxelMeta> m_VoxelMetaPool = new ObjectPool<VoxelMeta>(delegate(VoxelMeta x)
	{
		x.Clear();
	}, null, collectionCheck: false);

	private bool m_NeedUpdateIndexComputeBuffer;

	private int m_UpdateMinIndex = int.MaxValue;

	private int m_UpdateMaxIndex = int.MinValue;

	private static ProbeReferenceVolume.Cell g_Cell;

	internal int estimatedVMemCost { get; private set; }

	private int GetVoxelSubdivLevel()
	{
		return Mathf.Min(3, ProbeReferenceVolume.instance.GetMaxSubdivision() - 1);
	}

	private int SizeOfPhysicalIndexFromBudget(ProbeVolumeTextureMemoryBudget memoryBudget)
	{
		return memoryBudget switch
		{
			ProbeVolumeTextureMemoryBudget.MemoryBudgetLow => 16000000, 
			ProbeVolumeTextureMemoryBudget.MemoryBudgetMedium => 32000000, 
			ProbeVolumeTextureMemoryBudget.MemoryBudgetHigh => 64000000, 
			_ => 32000000, 
		};
	}

	internal ProbeBrickIndex(ProbeVolumeTextureMemoryBudget memoryBudget)
	{
		m_CenterRS = new Vector3Int(0, 0, 0);
		m_VoxelToBricks = new Dictionary<Vector3Int, List<VoxelMeta>>();
		m_BricksToVoxels = new Dictionary<ProbeReferenceVolume.Cell, BrickMeta>();
		m_NeedUpdateIndexComputeBuffer = false;
		m_IndexInChunks = Mathf.CeilToInt((float)SizeOfPhysicalIndexFromBudget(memoryBudget) / 243f);
		m_AvailableChunkCount = m_IndexInChunks;
		m_IndexChunks = new BitArray(Mathf.Max(1, m_IndexInChunks));
		int num = m_IndexInChunks * 243;
		m_PhysicalIndexBufferData = new int[num];
		m_PhysicalIndexBuffer = new ComputeBuffer(num, 4, ComputeBufferType.Structured);
		m_NextFreeChunk = 0;
		estimatedVMemCost = num * 4;
		Clear();
	}

	public int GetRemainingChunkCount()
	{
		return m_AvailableChunkCount;
	}

	internal void UploadIndexData()
	{
		int count = m_UpdateMaxIndex - m_UpdateMinIndex + 1;
		m_PhysicalIndexBuffer.SetData(m_PhysicalIndexBufferData, m_UpdateMinIndex, m_UpdateMinIndex, count);
		m_NeedUpdateIndexComputeBuffer = false;
		m_UpdateMaxIndex = int.MinValue;
		m_UpdateMinIndex = int.MaxValue;
	}

	internal void Clear()
	{
		for (int i = 0; i < m_PhysicalIndexBufferData.Length; i++)
		{
			m_PhysicalIndexBufferData[i] = -1;
		}
		m_NeedUpdateIndexComputeBuffer = true;
		m_UpdateMinIndex = 0;
		m_UpdateMaxIndex = m_PhysicalIndexBufferData.Length - 1;
		m_NextFreeChunk = 0;
		m_IndexChunks.SetAll(value: false);
		foreach (List<VoxelMeta> value in m_VoxelToBricks.Values)
		{
			foreach (VoxelMeta item in value)
			{
				m_VoxelMetaPool.Release(item);
			}
			m_VoxelMetaListPool.Release(value);
		}
		m_VoxelToBricks.Clear();
		foreach (BrickMeta value2 in m_BricksToVoxels.Values)
		{
			m_BrickMetaPool.Release(value2);
		}
		m_BricksToVoxels.Clear();
	}

	private void MapBrickToVoxels(Brick brick, HashSet<Vector3Int> voxels)
	{
		int subdivisionLevel = brick.subdivisionLevel;
		int num = (int)Mathf.Pow(3f, Mathf.Max(0, subdivisionLevel - GetVoxelSubdivLevel()));
		Vector3Int vector3Int = brick.position;
		int num2 = ProbeReferenceVolume.CellSize(brick.subdivisionLevel);
		int num3 = ProbeReferenceVolume.CellSize(GetVoxelSubdivLevel());
		if (num <= 1)
		{
			Vector3 vector = brick.position;
			vector *= 1f / (float)num3;
			vector3Int = new Vector3Int(Mathf.FloorToInt(vector.x) * num3, Mathf.FloorToInt(vector.y) * num3, Mathf.FloorToInt(vector.z) * num3);
		}
		for (int i = vector3Int.z; i < vector3Int.z + num2; i += num3)
		{
			for (int j = vector3Int.y; j < vector3Int.y + num2; j += num3)
			{
				for (int k = vector3Int.x; k < vector3Int.x + num2; k += num3)
				{
					voxels.Add(new Vector3Int(k, j, i));
				}
			}
		}
	}

	private void ClearVoxel(Vector3Int pos, CellIndexUpdateInfo cellInfo)
	{
		ClipToIndexSpace(pos, GetVoxelSubdivLevel(), out var outMinpos, out var outMaxpos, cellInfo);
		UpdatePhysicalIndex(outMinpos, outMaxpos, -1, cellInfo);
	}

	internal void GetRuntimeResources(ref ProbeReferenceVolume.RuntimeResources rr)
	{
		if (m_NeedUpdateIndexComputeBuffer)
		{
			UploadIndexData();
		}
		rr.index = m_PhysicalIndexBuffer;
	}

	internal void Cleanup()
	{
		CoreUtils.SafeRelease(m_PhysicalIndexBuffer);
		m_PhysicalIndexBuffer = null;
	}

	private int MergeIndex(int index, int size)
	{
		return (index & -1879048193) | ((size & 7) << 28);
	}

	internal bool AssignIndexChunksToCell(int bricksCount, ref CellIndexUpdateInfo cellUpdateInfo, bool ignoreErrorLog)
	{
		int num = Mathf.CeilToInt((float)bricksCount / 243f);
		int num2 = -1;
		for (int i = 0; i < m_IndexInChunks; i++)
		{
			if (!m_IndexChunks[i] && i + num < m_IndexInChunks)
			{
				int num3 = 0;
				for (int j = i; j < i + num && !m_IndexChunks[j]; j++)
				{
					num3++;
				}
				if (num3 == num)
				{
					num2 = i;
					break;
				}
			}
		}
		if (num2 < 0)
		{
			if (!ignoreErrorLog)
			{
				Debug.LogError("APV Index Allocation failed.");
			}
			return false;
		}
		cellUpdateInfo.firstChunkIndex = num2;
		cellUpdateInfo.numberOfChunks = num;
		for (int k = num2; k < num2 + num; k++)
		{
			m_IndexChunks[k] = true;
		}
		m_NextFreeChunk += Mathf.Max(0, num2 + num - m_NextFreeChunk);
		m_AvailableChunkCount -= num;
		return true;
	}

	public void AddBricks(ProbeReferenceVolume.Cell cell, NativeArray<Brick> bricks, List<ProbeBrickPool.BrickChunkAlloc> allocations, int allocationSize, int poolWidth, int poolHeight, CellIndexUpdateInfo cellInfo)
	{
		int a = ProbeReferenceVolume.CellSize(7);
		g_Cell = cell;
		BrickMeta brickMeta = m_BrickMetaPool.Get();
		m_BricksToVoxels.Add(cell, brickMeta);
		int num = 0;
		for (int i = 0; i < allocations.Count; i++)
		{
			ProbeBrickPool.BrickChunkAlloc brickChunkAlloc = allocations[i];
			int num2 = Mathf.Min(allocationSize, bricks.Length - num);
			int num3 = 0;
			while (num3 < num2)
			{
				Brick brick = bricks[num];
				int b = ProbeReferenceVolume.CellSize(brick.subdivisionLevel);
				a = Mathf.Min(a, b);
				MapBrickToVoxels(brick, brickMeta.voxels);
				ReservedBrick item = new ReservedBrick
				{
					brick = brick,
					flattenedIdx = MergeIndex(brickChunkAlloc.flattenIndex(poolWidth, poolHeight), brick.subdivisionLevel)
				};
				brickMeta.bricks.Add(item);
				foreach (Vector3Int voxel in brickMeta.voxels)
				{
					if (!m_VoxelToBricks.TryGetValue(voxel, out var value))
					{
						value = m_VoxelMetaListPool.Get();
						m_VoxelToBricks.Add(voxel, value);
					}
					VoxelMeta voxelMeta = null;
					int num4 = value.FindIndex((VoxelMeta lhs) => lhs.cell == g_Cell);
					if (num4 == -1)
					{
						voxelMeta = m_VoxelMetaPool.Get();
						voxelMeta.cell = cell;
						value.Add(voxelMeta);
					}
					else
					{
						voxelMeta = value[num4];
					}
					voxelMeta.brickIndices.Add((ushort)num);
				}
				num3++;
				num++;
				brickChunkAlloc.x += 4;
			}
		}
		foreach (Vector3Int voxel2 in brickMeta.voxels)
		{
			UpdateIndexForVoxel(voxel2, cellInfo);
		}
	}

	public void RemoveBricks(ProbeReferenceVolume.CellInfo cellInfo)
	{
		if (!m_BricksToVoxels.ContainsKey(cellInfo.cell))
		{
			return;
		}
		CellIndexUpdateInfo updateInfo = cellInfo.updateInfo;
		g_Cell = cellInfo.cell;
		BrickMeta brickMeta = m_BricksToVoxels[cellInfo.cell];
		foreach (Vector3Int voxel in brickMeta.voxels)
		{
			List<VoxelMeta> list = m_VoxelToBricks[voxel];
			int num = list.FindIndex((VoxelMeta lhs) => lhs.cell == g_Cell);
			if (num >= 0)
			{
				m_VoxelMetaPool.Release(list[num]);
				list.RemoveAt(num);
				if (list.Count > 0)
				{
					UpdateIndexForVoxel(voxel, updateInfo);
					continue;
				}
				ClearVoxel(voxel, updateInfo);
				m_VoxelMetaListPool.Release(list);
				m_VoxelToBricks.Remove(voxel);
			}
		}
		m_BrickMetaPool.Release(brickMeta);
		m_BricksToVoxels.Remove(cellInfo.cell);
		for (int num2 = updateInfo.firstChunkIndex; num2 < updateInfo.firstChunkIndex + updateInfo.numberOfChunks; num2++)
		{
			m_IndexChunks[num2] = false;
		}
		m_AvailableChunkCount += updateInfo.numberOfChunks;
	}

	private void UpdateIndexForVoxel(Vector3Int voxel, CellIndexUpdateInfo cellInfo)
	{
		ClearVoxel(voxel, cellInfo);
		foreach (VoxelMeta item in m_VoxelToBricks[voxel])
		{
			List<ReservedBrick> bricks = m_BricksToVoxels[item.cell].bricks;
			List<ushort> brickIndices = item.brickIndices;
			UpdateIndexForVoxel(voxel, bricks, brickIndices, cellInfo);
		}
	}

	private void UpdatePhysicalIndex(Vector3Int brickMin, Vector3Int brickMax, int value, CellIndexUpdateInfo cellInfo)
	{
		brickMin -= cellInfo.cellPositionInBricksAtMaxRes;
		brickMax -= cellInfo.cellPositionInBricksAtMaxRes;
		brickMin /= ProbeReferenceVolume.CellSize(cellInfo.minSubdivInCell);
		brickMax /= ProbeReferenceVolume.CellSize(cellInfo.minSubdivInCell);
		ProbeReferenceVolume.CellSize(ProbeReferenceVolume.instance.GetMaxSubdivision() - 1 - cellInfo.minSubdivInCell);
		Vector3Int vector3Int = cellInfo.minValidBrickIndexForCellAtMaxRes / ProbeReferenceVolume.CellSize(cellInfo.minSubdivInCell);
		Vector3Int vector3Int2 = cellInfo.maxValidBrickIndexForCellAtMaxResPlusOne / ProbeReferenceVolume.CellSize(cellInfo.minSubdivInCell);
		brickMin -= vector3Int;
		brickMax -= vector3Int;
		Vector3Int vector3Int3 = vector3Int2 - vector3Int;
		int num = cellInfo.firstChunkIndex * 243;
		int val = num + brickMin.z * (vector3Int3.x * vector3Int3.y) + brickMin.x * vector3Int3.y + brickMin.y;
		int val2 = num + Math.Max(0, brickMax.z - 1) * (vector3Int3.x * vector3Int3.y) + Math.Max(0, brickMax.x - 1) * vector3Int3.y + Math.Max(0, brickMax.y - 1);
		m_UpdateMinIndex = Math.Min(m_UpdateMinIndex, val);
		m_UpdateMaxIndex = Math.Max(m_UpdateMaxIndex, val2);
		for (int i = brickMin.x; i < brickMax.x; i++)
		{
			for (int j = brickMin.z; j < brickMax.z; j++)
			{
				for (int k = brickMin.y; k < brickMax.y; k++)
				{
					int num2 = j * (vector3Int3.x * vector3Int3.y) + i * vector3Int3.y + k;
					int num3 = num + num2;
					m_PhysicalIndexBufferData[num3] = value;
				}
			}
		}
		m_NeedUpdateIndexComputeBuffer = true;
	}

	private void ClipToIndexSpace(Vector3Int pos, int subdiv, out Vector3Int outMinpos, out Vector3Int outMaxpos, CellIndexUpdateInfo cellInfo)
	{
		int num = ProbeReferenceVolume.CellSize(subdiv);
		Vector3Int vector3Int = cellInfo.cellPositionInBricksAtMaxRes + cellInfo.minValidBrickIndexForCellAtMaxRes;
		Vector3Int vector3Int2 = cellInfo.cellPositionInBricksAtMaxRes + cellInfo.maxValidBrickIndexForCellAtMaxResPlusOne - Vector3Int.one;
		int num2 = pos.x - m_CenterRS.x;
		int y = pos.y;
		int num3 = pos.z - m_CenterRS.z;
		int a = num2 + num;
		int a2 = y + num;
		int a3 = num3 + num;
		num2 = Mathf.Max(num2, vector3Int.x);
		y = Mathf.Max(y, vector3Int.y);
		num3 = Mathf.Max(num3, vector3Int.z);
		a = Mathf.Min(a, vector3Int2.x);
		a2 = Mathf.Min(a2, vector3Int2.y);
		a3 = Mathf.Min(a3, vector3Int2.z);
		outMinpos = new Vector3Int(num2, y, num3);
		outMaxpos = new Vector3Int(a, a2, a3);
	}

	private void UpdateIndexForVoxel(Vector3Int voxel, List<ReservedBrick> bricks, List<ushort> indices, CellIndexUpdateInfo cellInfo)
	{
		ClipToIndexSpace(voxel, GetVoxelSubdivLevel(), out var outMinpos, out var outMaxpos, cellInfo);
		foreach (ReservedBrick brick in bricks)
		{
			int num = ProbeReferenceVolume.CellSize(brick.brick.subdivisionLevel);
			Vector3Int position = brick.brick.position;
			Vector3Int brickMax = brick.brick.position + Vector3Int.one * num;
			position.x = Mathf.Max(outMinpos.x, position.x - m_CenterRS.x);
			position.y = Mathf.Max(outMinpos.y, position.y);
			position.z = Mathf.Max(outMinpos.z, position.z - m_CenterRS.z);
			brickMax.x = Mathf.Min(outMaxpos.x, brickMax.x - m_CenterRS.x);
			brickMax.y = Mathf.Min(outMaxpos.y, brickMax.y);
			brickMax.z = Mathf.Min(outMaxpos.z, brickMax.z - m_CenterRS.z);
			UpdatePhysicalIndex(position, brickMax, brick.flattenedIdx, cellInfo);
		}
	}
}
