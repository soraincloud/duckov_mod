using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

[PreferBinarySerialization]
internal class ProbeVolumeAsset : ScriptableObject
{
	[Serializable]
	internal enum AssetVersion
	{
		First = 0,
		AddProbeVolumesAtlasEncodingModes = 1,
		PV2 = 2,
		ChunkBasedIndex = 3,
		BinaryRuntimeDebugSplit = 4,
		BinaryTextureData = 5,
		Max = 6,
		Current = 5
	}

	[Serializable]
	internal struct CellCounts
	{
		public int bricksCount;

		public int probesCount;

		public int offsetsCount;

		public int chunksCount;

		public void Add(CellCounts o)
		{
			bricksCount += o.bricksCount;
			probesCount += o.probesCount;
			offsetsCount += o.offsetsCount;
			chunksCount += o.chunksCount;
		}
	}

	[SerializeField]
	protected internal int m_Version = 5;

	[SerializeField]
	internal ProbeReferenceVolume.Cell[] cells;

	[SerializeField]
	internal CellCounts[] cellCounts;

	[SerializeField]
	internal CellCounts totalCellCounts;

	[SerializeField]
	internal Vector3Int maxCellPosition;

	[SerializeField]
	internal Vector3Int minCellPosition;

	[SerializeField]
	internal Bounds globalBounds;

	[SerializeField]
	internal ProbeVolumeSHBands bands;

	[SerializeField]
	internal int chunkSizeInBricks;

	[SerializeField]
	private string m_AssetFullPath = "UNINITIALIZED!";

	[SerializeField]
	internal int cellSizeInBricks;

	[SerializeField]
	internal int simplificationLevels;

	[SerializeField]
	internal float minDistanceBetweenProbes;

	public int Version => m_Version;

	internal int maxSubdivision => simplificationLevels + 1;

	internal float minBrickSize => Mathf.Max(0.01f, minDistanceBetweenProbes * 3f);

	internal bool CompatibleWith(ProbeVolumeAsset otherAsset)
	{
		if (maxSubdivision == otherAsset.maxSubdivision && minBrickSize == otherAsset.minBrickSize && cellSizeInBricks == otherAsset.cellSizeInBricks)
		{
			return chunkSizeInBricks == otherAsset.chunkSizeInBricks;
		}
		return false;
	}

	internal bool IsInvalid()
	{
		if (maxCellPosition.x >= minCellPosition.x && maxCellPosition.y >= minCellPosition.y)
		{
			return maxCellPosition.z < minCellPosition.z;
		}
		return true;
	}

	public string GetSerializedFullPath()
	{
		return m_AssetFullPath;
	}

	private static int AlignUp16(int count)
	{
		int num = 16;
		int num2 = count % num;
		return count + ((num2 != 0) ? (num - num2) : 0);
	}

	private NativeArray<T> GetSubArray<T>(NativeArray<byte> input, int count, ref int offset) where T : struct
	{
		int num = count * UnsafeUtility.SizeOf<T>();
		if (offset + num > input.Length)
		{
			return default(NativeArray<T>);
		}
		NativeArray<T> result = input.GetSubArray(offset, num).Reinterpret<T>(1);
		offset = AlignUp16(offset + num);
		return result;
	}

	internal bool ResolveSharedCellData(TextAsset cellSharedDataAsset, TextAsset cellSupportDataAsset)
	{
		if (cellSharedDataAsset == null)
		{
			return false;
		}
		int num = chunkSizeInBricks * 64;
		int count = totalCellCounts.chunksCount * num;
		NativeArray<byte> data = cellSharedDataAsset.GetData<byte>();
		int offset = 0;
		NativeArray<ProbeBrickIndex.Brick> subArray = GetSubArray<ProbeBrickIndex.Brick>(data, totalCellCounts.bricksCount, ref offset);
		NativeArray<byte> subArray2 = GetSubArray<byte>(data, count, ref offset);
		if (offset != AlignUp16(data.Length))
		{
			return false;
		}
		NativeArray<byte> input = (cellSupportDataAsset ? cellSupportDataAsset.GetData<byte>() : default(NativeArray<byte>));
		bool isCreated = input.IsCreated;
		offset = 0;
		NativeArray<Vector3> nativeArray = (isCreated ? GetSubArray<Vector3>(input, count, ref offset) : default(NativeArray<Vector3>));
		NativeArray<float> nativeArray2 = (isCreated ? GetSubArray<float>(input, count, ref offset) : default(NativeArray<float>));
		NativeArray<float> nativeArray3 = (isCreated ? GetSubArray<float>(input, count, ref offset) : default(NativeArray<float>));
		NativeArray<Vector3> nativeArray4 = (isCreated ? GetSubArray<Vector3>(input, count, ref offset) : default(NativeArray<Vector3>));
		if (isCreated && offset != AlignUp16(input.Length))
		{
			return false;
		}
		CellCounts cellCounts = default(CellCounts);
		for (int i = 0; i < cells.Length; i++)
		{
			ProbeReferenceVolume.Cell cell = cells[i];
			CellCounts o = this.cellCounts[i];
			int start = cellCounts.chunksCount * num;
			int length = o.chunksCount * num;
			cell.bricks = subArray.GetSubArray(cellCounts.bricksCount, o.bricksCount);
			cell.validityNeighMaskData = subArray2.GetSubArray(start, length);
			if (isCreated)
			{
				cell.probePositions = nativeArray.GetSubArray(start, length);
				cell.touchupVolumeInteraction = nativeArray2.GetSubArray(start, length);
				cell.offsetVectors = nativeArray4.GetSubArray(start, length);
				cell.validity = nativeArray3.GetSubArray(start, length);
			}
			cellCounts.Add(o);
		}
		return true;
	}

	internal bool ResolvePerScenarioCellData(TextAsset cellDataAsset, TextAsset cellOptionalDataAsset, int stateIndex)
	{
		if (cellDataAsset == null)
		{
			return false;
		}
		int num = chunkSizeInBricks * 64;
		int num2 = totalCellCounts.chunksCount * num;
		NativeArray<byte> data = cellDataAsset.GetData<byte>();
		int offset = 0;
		NativeArray<ushort> subArray = GetSubArray<ushort>(data, num2 * 4, ref offset);
		NativeArray<byte> subArray2 = GetSubArray<byte>(data, num2 * 4, ref offset);
		NativeArray<byte> subArray3 = GetSubArray<byte>(data, num2 * 4, ref offset);
		if (offset != AlignUp16(data.Length))
		{
			return false;
		}
		NativeArray<byte> input = (cellOptionalDataAsset ? cellOptionalDataAsset.GetData<byte>() : default(NativeArray<byte>));
		bool isCreated = input.IsCreated;
		offset = 0;
		NativeArray<byte> subArray4 = GetSubArray<byte>(input, num2 * 4, ref offset);
		NativeArray<byte> subArray5 = GetSubArray<byte>(input, num2 * 4, ref offset);
		NativeArray<byte> subArray6 = GetSubArray<byte>(input, num2 * 4, ref offset);
		NativeArray<byte> subArray7 = GetSubArray<byte>(input, num2 * 4, ref offset);
		if (isCreated && offset != AlignUp16(input.Length))
		{
			return false;
		}
		CellCounts cellCounts = default(CellCounts);
		for (int i = 0; i < cells.Length; i++)
		{
			CellCounts o = this.cellCounts[i];
			ProbeReferenceVolume.Cell.PerScenarioData perScenarioData = default(ProbeReferenceVolume.Cell.PerScenarioData);
			int start = cellCounts.chunksCount * num * 4;
			int length = o.chunksCount * num * 4;
			perScenarioData.shL0L1RxData = subArray.GetSubArray(start, length);
			perScenarioData.shL1GL1RyData = subArray2.GetSubArray(start, length);
			perScenarioData.shL1BL1RzData = subArray3.GetSubArray(start, length);
			if (isCreated)
			{
				perScenarioData.shL2Data_0 = subArray4.GetSubArray(start, length);
				perScenarioData.shL2Data_1 = subArray5.GetSubArray(start, length);
				perScenarioData.shL2Data_2 = subArray6.GetSubArray(start, length);
				perScenarioData.shL2Data_3 = subArray7.GetSubArray(start, length);
			}
			if (stateIndex == 0)
			{
				cells[i].scenario0 = perScenarioData;
			}
			else
			{
				cells[i].scenario1 = perScenarioData;
			}
			cellCounts.Add(o);
		}
		return true;
	}
}
