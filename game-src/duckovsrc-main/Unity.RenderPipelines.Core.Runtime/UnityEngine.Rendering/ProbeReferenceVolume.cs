using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine.SceneManagement;

namespace UnityEngine.Rendering;

public class ProbeReferenceVolume
{
	[Serializable]
	[DebuggerDisplay("Index = {index} position = {position}")]
	internal class Cell
	{
		public struct PerScenarioData
		{
			public NativeArray<ushort> shL0L1RxData { get; internal set; }

			public NativeArray<byte> shL1GL1RyData { get; internal set; }

			public NativeArray<byte> shL1BL1RzData { get; internal set; }

			public NativeArray<byte> shL2Data_0 { get; internal set; }

			public NativeArray<byte> shL2Data_1 { get; internal set; }

			public NativeArray<byte> shL2Data_2 { get; internal set; }

			public NativeArray<byte> shL2Data_3 { get; internal set; }
		}

		public Vector3Int position;

		public int index;

		public int probeCount;

		public int minSubdiv;

		public int maxSubdiv;

		public int indexChunkCount;

		public int shChunkCount;

		public bool hasTwoScenarios;

		public ProbeVolumeSHBands shBands;

		[NonSerialized]
		public PerScenarioData scenario0;

		[NonSerialized]
		public PerScenarioData scenario1;

		public NativeArray<ProbeBrickIndex.Brick> bricks { get; internal set; }

		public NativeArray<byte> validityNeighMaskData { get; internal set; }

		public NativeArray<Vector3> probePositions { get; internal set; }

		public NativeArray<float> touchupVolumeInteraction { get; internal set; }

		public NativeArray<Vector3> offsetVectors { get; internal set; }

		public NativeArray<float> validity { get; internal set; }

		public PerScenarioData bakingScenario => scenario0;
	}

	[DebuggerDisplay("Index = {cell.index} Loaded = {loaded}")]
	internal class CellInfo : IComparable<CellInfo>
	{
		public Cell cell;

		public BlendingCellInfo blendingCell;

		public List<ProbeBrickPool.BrickChunkAlloc> chunkList = new List<ProbeBrickPool.BrickChunkAlloc>();

		public int flatIdxInCellIndices = -1;

		public bool loaded;

		public ProbeBrickIndex.CellIndexUpdateInfo updateInfo;

		public bool indexUpdated;

		public ProbeBrickIndex.CellIndexUpdateInfo tempUpdateInfo;

		public int sourceAssetInstanceID;

		public float streamingScore;

		public int referenceCount;

		public CellInstancedDebugProbes debugProbes;

		public int CompareTo(CellInfo other)
		{
			if (streamingScore < other.streamingScore)
			{
				return -1;
			}
			if (streamingScore > other.streamingScore)
			{
				return 1;
			}
			return 0;
		}

		public void Clear()
		{
			cell = null;
			blendingCell = null;
			chunkList.Clear();
			flatIdxInCellIndices = -1;
			loaded = false;
			updateInfo = default(ProbeBrickIndex.CellIndexUpdateInfo);
			sourceAssetInstanceID = -1;
			streamingScore = 0f;
			referenceCount = 0;
			debugProbes = null;
		}
	}

	[DebuggerDisplay("Index = {cellInfo.cell.index} Factor = {blendingFactor} Score = {streamingScore}")]
	internal class BlendingCellInfo : IComparable<BlendingCellInfo>
	{
		public CellInfo cellInfo;

		public List<ProbeBrickPool.BrickChunkAlloc> chunkList = new List<ProbeBrickPool.BrickChunkAlloc>();

		public float streamingScore;

		public float blendingFactor;

		public bool blending;

		public int CompareTo(BlendingCellInfo other)
		{
			if (streamingScore < other.streamingScore)
			{
				return -1;
			}
			if (streamingScore > other.streamingScore)
			{
				return 1;
			}
			return 0;
		}

		public void Clear()
		{
			cellInfo = null;
			chunkList.Clear();
			blendingFactor = 0f;
			streamingScore = 0f;
			blending = false;
		}

		public void MarkUpToDate()
		{
			streamingScore = float.MaxValue;
		}

		public bool IsUpToDate()
		{
			return streamingScore == float.MaxValue;
		}

		public void ForceReupload()
		{
			blendingFactor = -1f;
		}

		public bool ShouldReupload()
		{
			return blendingFactor == -1f;
		}

		public void Prioritize()
		{
			blendingFactor = -2f;
		}

		public bool ShouldPrioritize()
		{
			return blendingFactor == -2f;
		}
	}

	internal struct Volume : IEquatable<Volume>
	{
		internal Vector3 corner;

		internal Vector3 X;

		internal Vector3 Y;

		internal Vector3 Z;

		internal float maxSubdivisionMultiplier;

		internal float minSubdivisionMultiplier;

		public Volume(Matrix4x4 trs, float maxSubdivision, float minSubdivision)
		{
			X = trs.GetColumn(0);
			Y = trs.GetColumn(1);
			Z = trs.GetColumn(2);
			corner = (Vector3)trs.GetColumn(3) - X * 0.5f - Y * 0.5f - Z * 0.5f;
			maxSubdivisionMultiplier = maxSubdivision;
			minSubdivisionMultiplier = minSubdivision;
		}

		public Volume(Vector3 corner, Vector3 X, Vector3 Y, Vector3 Z, float maxSubdivision = 1f, float minSubdivision = 0f)
		{
			this.corner = corner;
			this.X = X;
			this.Y = Y;
			this.Z = Z;
			maxSubdivisionMultiplier = maxSubdivision;
			minSubdivisionMultiplier = minSubdivision;
		}

		public Volume(Volume copy)
		{
			X = copy.X;
			Y = copy.Y;
			Z = copy.Z;
			corner = copy.corner;
			maxSubdivisionMultiplier = copy.maxSubdivisionMultiplier;
			minSubdivisionMultiplier = copy.minSubdivisionMultiplier;
		}

		public Volume(Bounds bounds)
		{
			Vector3 size = bounds.size;
			corner = bounds.center - size * 0.5f;
			X = new Vector3(size.x, 0f, 0f);
			Y = new Vector3(0f, size.y, 0f);
			Z = new Vector3(0f, 0f, size.z);
			maxSubdivisionMultiplier = (minSubdivisionMultiplier = 0f);
		}

		public Bounds CalculateAABB()
		{
			Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						Vector3 vector3 = new Vector3(i, j, k);
						Vector3 rhs = corner + X * vector3.x + Y * vector3.y + Z * vector3.z;
						vector = Vector3.Min(vector, rhs);
						vector2 = Vector3.Max(vector2, rhs);
					}
				}
			}
			return new Bounds((vector + vector2) / 2f, vector2 - vector);
		}

		public void CalculateCenterAndSize(out Vector3 center, out Vector3 size)
		{
			size = new Vector3(X.magnitude, Y.magnitude, Z.magnitude);
			center = corner + X * 0.5f + Y * 0.5f + Z * 0.5f;
		}

		public void Transform(Matrix4x4 trs)
		{
			corner = trs.MultiplyPoint(corner);
			X = trs.MultiplyVector(X);
			Y = trs.MultiplyVector(Y);
			Z = trs.MultiplyVector(Z);
		}

		public override string ToString()
		{
			return $"Corner: {corner}, X: {X}, Y: {Y}, Z: {Z}, MaxSubdiv: {maxSubdivisionMultiplier}";
		}

		public bool Equals(Volume other)
		{
			if (corner == other.corner && X == other.X && Y == other.Y && Z == other.Z && minSubdivisionMultiplier == other.minSubdivisionMultiplier)
			{
				return maxSubdivisionMultiplier == other.maxSubdivisionMultiplier;
			}
			return false;
		}
	}

	internal struct RefVolTransform
	{
		public Vector3 posWS;

		public Quaternion rot;

		public float scale;
	}

	public struct RuntimeResources
	{
		public ComputeBuffer index;

		public ComputeBuffer cellIndices;

		public RenderTexture L0_L1rx;

		public RenderTexture L1_G_ry;

		public RenderTexture L1_B_rz;

		public RenderTexture L2_0;

		public RenderTexture L2_1;

		public RenderTexture L2_2;

		public RenderTexture L2_3;

		public Texture3D Validity;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ExtraDataActionInput
	{
	}

	private struct InitInfo
	{
		public Vector3Int pendingMinCellPosition;

		public Vector3Int pendingMaxCellPosition;
	}

	internal class CellInstancedDebugProbes
	{
		public List<Matrix4x4[]> probeBuffers;

		public List<Matrix4x4[]> offsetBuffers;

		public List<MaterialPropertyBlock> props;
	}

	private bool m_IsInitialized;

	private bool m_SupportStreaming;

	private RefVolTransform m_Transform;

	private int m_MaxSubdivision;

	private ProbeBrickPool m_Pool;

	private ProbeBrickIndex m_Index;

	private ProbeCellIndices m_CellIndices;

	private ProbeBrickBlendingPool m_BlendingPool;

	private List<ProbeBrickPool.BrickChunkAlloc> m_TmpSrcChunks = new List<ProbeBrickPool.BrickChunkAlloc>();

	private float[] m_PositionOffsets = new float[4];

	private Bounds m_CurrGlobalBounds;

	internal Dictionary<int, CellInfo> cells = new Dictionary<int, CellInfo>();

	private ObjectPool<CellInfo> m_CellInfoPool = new ObjectPool<CellInfo>(delegate(CellInfo x)
	{
		x.Clear();
	}, null, collectionCheck: false);

	private ObjectPool<BlendingCellInfo> m_BlendingCellInfoPool = new ObjectPool<BlendingCellInfo>(delegate(BlendingCellInfo x)
	{
		x.Clear();
	}, null, collectionCheck: false);

	private ProbeBrickPool.DataLocation m_TemporaryDataLocation;

	private int m_TemporaryDataLocationMemCost;

	private int m_CurrentProbeVolumeChunkSizeInBricks;

	internal ProbeVolumeSceneData sceneData;

	private Vector3Int minLoadedCellPos = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);

	private Vector3Int maxLoadedCellPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

	public Action<ExtraDataActionInput> retrieveExtraDataAction;

	public Action checksDuringBakeAction;

	private bool m_BricksLoaded;

	private Dictionary<string, ProbeVolumeAsset> m_PendingAssetsToBeLoaded = new Dictionary<string, ProbeVolumeAsset>();

	private Dictionary<string, ProbeVolumeAsset> m_PendingAssetsToBeUnloaded = new Dictionary<string, ProbeVolumeAsset>();

	private Dictionary<string, ProbeVolumeAsset> m_ActiveAssets = new Dictionary<string, ProbeVolumeAsset>();

	private bool m_NeedLoadAsset;

	private bool m_ProbeReferenceVolumeInit;

	private bool m_EnabledBySRP;

	private InitInfo m_PendingInitInfo;

	private bool m_NeedsIndexRebuild;

	private bool m_HasChangedIndex;

	private int m_CBShaderID = Shader.PropertyToID("ShaderVariablesProbeVolumes");

	private int m_NumberOfCellsLoadedPerFrame = 2;

	private int m_NumberOfCellsBlendedPerFrame = 10000;

	private float m_TurnoverRate = 0.1f;

	private ProbeVolumeTextureMemoryBudget m_MemoryBudget;

	private ProbeVolumeBlendingTextureMemoryBudget m_BlendingMemoryBudget;

	private ProbeVolumeSHBands m_SHBands;

	private float m_ProbeVolumesWeight;

	internal bool clearAssetsOnVolumeClear;

	internal static string defaultLightingScenario = "Default";

	private static ProbeReferenceVolume _instance = new ProbeReferenceVolume();

	private const int kProbesPerBatch = 511;

	public static readonly string k_DebugPanelName = "Probe Volume";

	private DebugUI.Widget[] m_DebugItems;

	private Mesh m_DebugMesh;

	private Material m_DebugMaterial;

	private Mesh m_DebugOffsetMesh;

	private Material m_DebugOffsetMaterial;

	private Plane[] m_DebugFrustumPlanes = new Plane[6];

	private GUIContent[] m_DebugScenarioNames = new GUIContent[0];

	private int[] m_DebugScenarioValues = new int[0];

	private string m_DebugActiveSceneGUID;

	private string m_DebugActiveScenario;

	private DebugUI.EnumField m_DebugScenarioField;

	internal ProbeVolumeBakingProcessSettings bakingProcessSettings;

	internal Dictionary<Bounds, ProbeBrickIndex.Brick[]> realtimeSubdivisionInfo = new Dictionary<Bounds, ProbeBrickIndex.Brick[]>();

	private bool m_MaxSubdivVisualizedIsMaxAvailable;

	private DynamicArray<CellInfo> m_LoadedCells = new DynamicArray<CellInfo>();

	private DynamicArray<CellInfo> m_ToBeLoadedCells = new DynamicArray<CellInfo>();

	private DynamicArray<CellInfo> m_TempCellToLoadList = new DynamicArray<CellInfo>();

	private DynamicArray<CellInfo> m_TempCellToUnloadList = new DynamicArray<CellInfo>();

	private DynamicArray<BlendingCellInfo> m_LoadedBlendingCells = new DynamicArray<BlendingCellInfo>();

	private DynamicArray<BlendingCellInfo> m_ToBeLoadedBlendingCells = new DynamicArray<BlendingCellInfo>();

	private DynamicArray<BlendingCellInfo> m_TempBlendingCellToLoadList = new DynamicArray<BlendingCellInfo>();

	private DynamicArray<BlendingCellInfo> m_TempBlendingCellToUnloadList = new DynamicArray<BlendingCellInfo>();

	private Vector3 m_FrozenCameraPosition;

	private bool m_HasRemainingCellsToBlend;

	internal Bounds globalBounds
	{
		get
		{
			return m_CurrGlobalBounds;
		}
		set
		{
			m_CurrGlobalBounds = value;
		}
	}

	public bool isInitialized => m_ProbeReferenceVolumeInit;

	internal bool enabledBySRP => m_EnabledBySRP;

	internal bool hasUnloadedCells => m_ToBeLoadedCells.size != 0;

	internal bool enableScenarioBlending
	{
		get
		{
			if (m_BlendingMemoryBudget != ProbeVolumeBlendingTextureMemoryBudget.None)
			{
				return ProbeBrickBlendingPool.isSupported;
			}
			return false;
		}
	}

	internal int numberOfCellsLoadedPerFrame => m_NumberOfCellsLoadedPerFrame;

	public int numberOfCellsBlendedPerFrame
	{
		get
		{
			return m_NumberOfCellsBlendedPerFrame;
		}
		set
		{
			m_NumberOfCellsBlendedPerFrame = Mathf.Max(1, value);
		}
	}

	public float turnoverRate
	{
		get
		{
			return m_TurnoverRate;
		}
		set
		{
			m_TurnoverRate = Mathf.Clamp01(value);
		}
	}

	public ProbeVolumeSHBands shBands => m_SHBands;

	public string lightingScenario
	{
		get
		{
			return sceneData.lightingScenario;
		}
		set
		{
			sceneData.SetActiveScenario(value);
		}
	}

	public float scenarioBlendingFactor
	{
		get
		{
			return sceneData.scenarioBlendingFactor;
		}
		set
		{
			sceneData.BlendLightingScenario(sceneData.otherScenario, value);
		}
	}

	public ProbeVolumeTextureMemoryBudget memoryBudget => m_MemoryBudget;

	public float probeVolumesWeight
	{
		get
		{
			return m_ProbeVolumesWeight;
		}
		set
		{
			m_ProbeVolumesWeight = Mathf.Clamp01(value);
		}
	}

	internal List<ProbeVolumePerSceneData> perSceneDataList { get; private set; } = new List<ProbeVolumePerSceneData>();

	public static ProbeReferenceVolume instance => _instance;

	internal ProbeVolumeDebug probeVolumeDebug { get; } = new ProbeVolumeDebug();

	public Color[] subdivisionDebugColors { get; } = new Color[7];

	public void BlendLightingScenario(string otherScenario, float blendingFactor)
	{
		sceneData.BlendLightingScenario(otherScenario, blendingFactor);
	}

	internal void RegisterPerSceneData(ProbeVolumePerSceneData data)
	{
		if (!perSceneDataList.Contains(data))
		{
			perSceneDataList.Add(data);
		}
	}

	internal void UnregisterPerSceneData(ProbeVolumePerSceneData data)
	{
		perSceneDataList.Remove(data);
	}

	public void Initialize(in ProbeVolumeSystemParameters parameters)
	{
		if (m_IsInitialized)
		{
			Debug.LogError("Probe Volume System has already been initialized.");
			return;
		}
		m_MemoryBudget = parameters.memoryBudget;
		m_BlendingMemoryBudget = parameters.blendingMemoryBudget;
		m_SHBands = parameters.shBands;
		m_ProbeVolumesWeight = 1f;
		InitializeDebug(in parameters);
		ProbeBrickBlendingPool.Initialize(in parameters);
		InitProbeReferenceVolume(m_MemoryBudget, m_BlendingMemoryBudget, m_SHBands);
		m_IsInitialized = true;
		m_NeedsIndexRebuild = true;
		sceneData = parameters.sceneData;
		m_SupportStreaming = parameters.supportStreaming;
		m_EnabledBySRP = true;
		if (sceneData == null)
		{
			return;
		}
		foreach (ProbeVolumePerSceneData perSceneData in instance.perSceneDataList)
		{
			perSceneData.Initialize();
		}
	}

	public void SetEnableStateFromSRP(bool srpEnablesPV)
	{
		m_EnabledBySRP = srpEnablesPV;
	}

	internal void ForceSHBand(ProbeVolumeSHBands shBands)
	{
		if (m_ProbeReferenceVolumeInit)
		{
			CleanupLoadedData();
		}
		m_SHBands = shBands;
		m_ProbeReferenceVolumeInit = false;
		InitProbeReferenceVolume(m_MemoryBudget, m_BlendingMemoryBudget, shBands);
	}

	public void Cleanup()
	{
		if (m_ProbeReferenceVolumeInit)
		{
			if (!m_IsInitialized)
			{
				Debug.LogError("Probe Volume System has not been initialized first before calling cleanup.");
				return;
			}
			CleanupLoadedData();
			CleanupDebug();
			m_IsInitialized = false;
		}
	}

	public int GetVideoMemoryCost()
	{
		if (!m_ProbeReferenceVolumeInit)
		{
			return 0;
		}
		return m_Pool.estimatedVMemCost + m_Index.estimatedVMemCost + m_CellIndices.estimatedVMemCost + m_BlendingPool.estimatedVMemCost + m_TemporaryDataLocationMemCost;
	}

	private void RemoveCell(Cell cell)
	{
		if (!cells.TryGetValue(cell.index, out var value))
		{
			return;
		}
		value.referenceCount--;
		if (value.referenceCount <= 0)
		{
			cells.Remove(cell.index);
			if (value.loaded)
			{
				m_LoadedCells.Remove(value);
				UnloadCell(value);
			}
			else
			{
				m_ToBeLoadedCells.Remove(value);
			}
			m_BlendingCellInfoPool.Release(value.blendingCell);
			m_CellInfoPool.Release(value);
		}
	}

	internal void UnloadCell(CellInfo cellInfo)
	{
		if (cellInfo.loaded)
		{
			if (cellInfo.blendingCell.blending)
			{
				m_LoadedBlendingCells.Remove(cellInfo.blendingCell);
				UnloadBlendingCell(cellInfo.blendingCell);
			}
			else
			{
				m_ToBeLoadedBlendingCells.Remove(cellInfo.blendingCell);
			}
			if (cellInfo.flatIdxInCellIndices >= 0)
			{
				m_CellIndices.MarkCellAsUnloaded(cellInfo.flatIdxInCellIndices);
			}
			ReleaseBricks(cellInfo);
			cellInfo.loaded = false;
			cellInfo.debugProbes = null;
			cellInfo.updateInfo = default(ProbeBrickIndex.CellIndexUpdateInfo);
			ClearDebugData();
		}
	}

	internal void UnloadBlendingCell(BlendingCellInfo blendingCell)
	{
		if (blendingCell.blending)
		{
			m_BlendingPool.Deallocate(blendingCell.chunkList);
			blendingCell.chunkList.Clear();
			blendingCell.blending = false;
		}
	}

	internal void UnloadAllCells()
	{
		for (int i = 0; i < m_LoadedCells.size; i++)
		{
			UnloadCell(m_LoadedCells[i]);
		}
		m_ToBeLoadedCells.AddRange(m_LoadedCells);
		m_LoadedCells.Clear();
	}

	internal void UnloadAllBlendingCells()
	{
		for (int i = 0; i < m_LoadedBlendingCells.size; i++)
		{
			UnloadBlendingCell(m_LoadedBlendingCells[i]);
		}
		m_ToBeLoadedBlendingCells.AddRange(m_LoadedBlendingCells);
		m_LoadedBlendingCells.Clear();
	}

	private void AddCell(Cell cell, int assetInstanceID)
	{
		if (!cells.TryGetValue(cell.index, out var value))
		{
			value = m_CellInfoPool.Get();
			value.cell = cell;
			value.flatIdxInCellIndices = m_CellIndices.GetFlatIdxForCell(cell.position);
			value.sourceAssetInstanceID = assetInstanceID;
			value.referenceCount = 1;
			cells[cell.index] = value;
			BlendingCellInfo blendingCellInfo = m_BlendingCellInfoPool.Get();
			blendingCellInfo.cellInfo = value;
			value.blendingCell = blendingCellInfo;
			m_ToBeLoadedCells.Add(in value);
		}
		else
		{
			value.referenceCount++;
		}
	}

	internal bool LoadCell(CellInfo cellInfo, bool ignoreErrorLog = false)
	{
		if (GetCellIndexUpdate(cellInfo.cell, out var cellUpdateInfo, ignoreErrorLog))
		{
			minLoadedCellPos = Vector3Int.Min(minLoadedCellPos, cellInfo.cell.position);
			maxLoadedCellPos = Vector3Int.Max(maxLoadedCellPos, cellInfo.cell.position);
			return AddBricks(cellInfo, cellUpdateInfo, ignoreErrorLog);
		}
		return false;
	}

	internal void LoadAllCells()
	{
		int size = m_LoadedCells.size;
		for (int i = 0; i < m_ToBeLoadedCells.size; i++)
		{
			CellInfo value = m_ToBeLoadedCells[i];
			if (LoadCell(value, ignoreErrorLog: true))
			{
				m_LoadedCells.Add(in value);
			}
		}
		for (int j = size; j < m_LoadedCells.size; j++)
		{
			m_ToBeLoadedCells.Remove(m_LoadedCells[j]);
		}
	}

	private void RecomputeMinMaxLoadedCellPos()
	{
		minLoadedCellPos = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
		maxLoadedCellPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		foreach (CellInfo value in cells.Values)
		{
			if (value.loaded)
			{
				minLoadedCellPos = Vector3Int.Min(value.cell.position, minLoadedCellPos);
				maxLoadedCellPos = Vector3Int.Max(value.cell.position, maxLoadedCellPos);
			}
		}
	}

	private bool CheckCompatibilityWithCollection(ProbeVolumeAsset asset, Dictionary<string, ProbeVolumeAsset> collection)
	{
		if (collection.Count > 0)
		{
			foreach (ProbeVolumeAsset value in collection.Values)
			{
				if (!m_PendingAssetsToBeUnloaded.ContainsKey(value.GetSerializedFullPath()))
				{
					return value.CompatibleWith(asset);
				}
			}
		}
		return true;
	}

	internal void AddPendingAssetLoading(ProbeVolumeAsset asset)
	{
		string serializedFullPath = asset.GetSerializedFullPath();
		if (m_PendingAssetsToBeLoaded.ContainsKey(serializedFullPath))
		{
			m_PendingAssetsToBeLoaded.Remove(serializedFullPath);
		}
		if (!CheckCompatibilityWithCollection(asset, m_ActiveAssets))
		{
			Debug.LogError("Trying to load Probe Volume data for a scene that has been baked with different settings than currently loaded ones. Please make sure all loaded scenes are in the same baking set.");
			return;
		}
		if (!CheckCompatibilityWithCollection(asset, m_PendingAssetsToBeLoaded))
		{
			Debug.LogError("Trying to load Probe Volume data for a scene that has been baked with different settings from other scenes that are being loaded. Please make sure all loaded scenes are in the same baking set.");
			return;
		}
		m_PendingAssetsToBeLoaded.Add(serializedFullPath, asset);
		m_NeedLoadAsset = true;
		_ = Vector3Int.zero;
		Vector3Int vector3Int = Vector3Int.one * 10000;
		Vector3Int vector3Int2 = Vector3Int.one * -10000;
		bool flag = true;
		foreach (ProbeVolumeAsset value in m_PendingAssetsToBeLoaded.Values)
		{
			vector3Int = Vector3Int.Min(vector3Int, value.minCellPosition);
			vector3Int2 = Vector3Int.Max(vector3Int2, value.maxCellPosition);
			if (flag)
			{
				m_CurrGlobalBounds = value.globalBounds;
				flag = false;
			}
			else
			{
				m_CurrGlobalBounds.Encapsulate(value.globalBounds);
			}
		}
		foreach (ProbeVolumeAsset value2 in m_ActiveAssets.Values)
		{
			vector3Int = Vector3Int.Min(vector3Int, value2.minCellPosition);
			vector3Int2 = Vector3Int.Max(vector3Int2, value2.maxCellPosition);
			if (flag)
			{
				m_CurrGlobalBounds = value2.globalBounds;
				flag = false;
			}
			else
			{
				m_CurrGlobalBounds.Encapsulate(value2.globalBounds);
			}
		}
		m_NeedsIndexRebuild |= m_Index == null || m_PendingInitInfo.pendingMinCellPosition != vector3Int || m_PendingInitInfo.pendingMaxCellPosition != vector3Int2;
		m_PendingInitInfo.pendingMinCellPosition = vector3Int;
		m_PendingInitInfo.pendingMaxCellPosition = vector3Int2;
	}

	internal void AddPendingAssetRemoval(ProbeVolumeAsset asset)
	{
		string serializedFullPath = asset.GetSerializedFullPath();
		if (m_PendingAssetsToBeLoaded.ContainsKey(serializedFullPath))
		{
			m_PendingAssetsToBeLoaded.Remove(serializedFullPath);
		}
		if (m_ActiveAssets.ContainsKey(serializedFullPath))
		{
			m_PendingAssetsToBeUnloaded[serializedFullPath] = asset;
		}
	}

	internal void RemovePendingAsset(ProbeVolumeAsset asset)
	{
		string serializedFullPath = asset.GetSerializedFullPath();
		if (m_ActiveAssets.ContainsKey(serializedFullPath))
		{
			m_ActiveAssets.Remove(serializedFullPath);
		}
		Cell[] array = asset.cells;
		foreach (Cell cell in array)
		{
			RemoveCell(cell);
		}
		int instanceID = asset.GetInstanceID();
		for (int num = m_LoadedCells.size - 1; num >= 0; num--)
		{
			if (m_LoadedCells[num].sourceAssetInstanceID == instanceID)
			{
				if (m_LoadedCells[num].blendingCell.blending)
				{
					m_LoadedBlendingCells.Remove(m_LoadedCells[num].blendingCell);
				}
				else
				{
					m_ToBeLoadedBlendingCells.Remove(m_LoadedCells[num].blendingCell);
				}
				m_LoadedCells.RemoveAt(num);
			}
		}
		for (int num2 = m_ToBeLoadedCells.size - 1; num2 >= 0; num2--)
		{
			if (m_ToBeLoadedCells[num2].sourceAssetInstanceID == instanceID)
			{
				m_ToBeLoadedCells.RemoveAt(num2);
			}
		}
		ClearDebugData();
		RecomputeMinMaxLoadedCellPos();
	}

	private void PerformPendingIndexChangeAndInit()
	{
		if (m_NeedsIndexRebuild)
		{
			CleanupLoadedData();
			InitProbeReferenceVolume(m_MemoryBudget, m_BlendingMemoryBudget, m_SHBands);
			m_HasChangedIndex = true;
			m_NeedsIndexRebuild = false;
		}
		else
		{
			m_HasChangedIndex = false;
		}
	}

	internal void SetMinBrickAndMaxSubdiv(float minBrickSize, int maxSubdiv)
	{
		SetTRS(Vector3.zero, Quaternion.identity, minBrickSize);
		SetMaxSubdivision(maxSubdiv);
	}

	private void LoadAsset(ProbeVolumeAsset asset)
	{
		if (asset.Version != 5)
		{
			Debug.LogWarning("Trying to load an asset " + asset.GetSerializedFullPath() + " that has been baked with a previous version of the system. Please re-bake the data.");
			return;
		}
		SetMinBrickAndMaxSubdiv(asset.minBrickSize, asset.maxSubdivision);
		if (asset.chunkSizeInBricks != m_CurrentProbeVolumeChunkSizeInBricks)
		{
			m_CurrentProbeVolumeChunkSizeInBricks = asset.chunkSizeInBricks;
			AllocateTemporaryDataLocation();
		}
		ClearDebugData();
		for (int i = 0; i < asset.cells.Length; i++)
		{
			AddCell(asset.cells[i], asset.GetInstanceID());
		}
	}

	private void PerformPendingLoading()
	{
		if ((m_PendingAssetsToBeLoaded.Count == 0 && m_ActiveAssets.Count == 0) || !m_NeedLoadAsset || !m_ProbeReferenceVolumeInit)
		{
			return;
		}
		m_Pool.EnsureTextureValidity();
		m_BlendingPool.EnsureTextureValidity();
		if (m_HasChangedIndex)
		{
			foreach (ProbeVolumeAsset value in m_ActiveAssets.Values)
			{
				LoadAsset(value);
			}
		}
		foreach (ProbeVolumeAsset value2 in m_PendingAssetsToBeLoaded.Values)
		{
			LoadAsset(value2);
			if (!m_ActiveAssets.ContainsKey(value2.GetSerializedFullPath()))
			{
				m_ActiveAssets.Add(value2.GetSerializedFullPath(), value2);
			}
		}
		m_PendingAssetsToBeLoaded.Clear();
		m_NeedLoadAsset = false;
	}

	private void PerformPendingDeletion()
	{
		if (!m_ProbeReferenceVolumeInit)
		{
			m_PendingAssetsToBeUnloaded.Clear();
		}
		foreach (ProbeVolumeAsset value in m_PendingAssetsToBeUnloaded.Values)
		{
			RemovePendingAsset(value);
		}
		m_PendingAssetsToBeUnloaded.Clear();
	}

	internal int GetNumberOfBricksAtSubdiv(Vector3Int position, int minSubdiv, out Vector3Int minValidLocalIdxAtMaxRes, out Vector3Int sizeOfValidIndicesAtMaxRes)
	{
		minValidLocalIdxAtMaxRes = Vector3Int.zero;
		sizeOfValidIndicesAtMaxRes = Vector3Int.one;
		Vector3 vector = new Vector3((float)position.x * MaxBrickSize(), (float)position.y * MaxBrickSize(), (float)position.z * MaxBrickSize());
		Bounds bounds = new Bounds
		{
			min = vector,
			max = vector + Vector3.one * MaxBrickSize()
		};
		Bounds bounds2 = new Bounds
		{
			min = Vector3.Max(bounds.min, m_CurrGlobalBounds.min),
			max = Vector3.Min(bounds.max, m_CurrGlobalBounds.max)
		};
		Vector3 vector2 = bounds2.min - bounds.min;
		minValidLocalIdxAtMaxRes.x = Mathf.CeilToInt(vector2.x / MinBrickSize());
		minValidLocalIdxAtMaxRes.y = Mathf.CeilToInt(vector2.y / MinBrickSize());
		minValidLocalIdxAtMaxRes.z = Mathf.CeilToInt(vector2.z / MinBrickSize());
		Vector3 vector3 = bounds2.max - bounds.min;
		sizeOfValidIndicesAtMaxRes.x = Mathf.CeilToInt(vector3.x / MinBrickSize()) - minValidLocalIdxAtMaxRes.x + 1;
		sizeOfValidIndicesAtMaxRes.y = Mathf.CeilToInt(vector3.y / MinBrickSize()) - minValidLocalIdxAtMaxRes.y + 1;
		sizeOfValidIndicesAtMaxRes.z = Mathf.CeilToInt(vector3.z / MinBrickSize()) - minValidLocalIdxAtMaxRes.z + 1;
		Vector3Int vector3Int = default(Vector3Int);
		vector3Int = sizeOfValidIndicesAtMaxRes / CellSize(minSubdiv);
		return vector3Int.x * vector3Int.y * vector3Int.z;
	}

	private bool GetCellIndexUpdate(Cell cell, out ProbeBrickIndex.CellIndexUpdateInfo cellUpdateInfo, bool ignoreErrorLog)
	{
		cellUpdateInfo = default(ProbeBrickIndex.CellIndexUpdateInfo);
		Vector3Int minValidLocalIdxAtMaxRes;
		Vector3Int sizeOfValidIndicesAtMaxRes;
		int numberOfBricksAtSubdiv = GetNumberOfBricksAtSubdiv(cell.position, cell.minSubdiv, out minValidLocalIdxAtMaxRes, out sizeOfValidIndicesAtMaxRes);
		cellUpdateInfo.cellPositionInBricksAtMaxRes = cell.position * CellSize(m_MaxSubdivision - 1);
		cellUpdateInfo.minSubdivInCell = cell.minSubdiv;
		cellUpdateInfo.minValidBrickIndexForCellAtMaxRes = minValidLocalIdxAtMaxRes;
		cellUpdateInfo.maxValidBrickIndexForCellAtMaxResPlusOne = sizeOfValidIndicesAtMaxRes + minValidLocalIdxAtMaxRes;
		return m_Index.AssignIndexChunksToCell(numberOfBricksAtSubdiv, ref cellUpdateInfo, ignoreErrorLog);
	}

	public void PerformPendingOperations()
	{
		PerformPendingDeletion();
		PerformPendingIndexChangeAndInit();
		PerformPendingLoading();
	}

	private void InitProbeReferenceVolume(ProbeVolumeTextureMemoryBudget memoryBudget, ProbeVolumeBlendingTextureMemoryBudget blendingMemoryBudget, ProbeVolumeSHBands shBands)
	{
		Vector3Int pendingMinCellPosition = m_PendingInitInfo.pendingMinCellPosition;
		Vector3Int pendingMaxCellPosition = m_PendingInitInfo.pendingMaxCellPosition;
		if (!m_ProbeReferenceVolumeInit)
		{
			m_Pool = new ProbeBrickPool(memoryBudget, shBands);
			m_BlendingPool = new ProbeBrickBlendingPool(blendingMemoryBudget, shBands);
			m_Index = new ProbeBrickIndex(memoryBudget);
			m_CellIndices = new ProbeCellIndices(pendingMinCellPosition, pendingMaxCellPosition, (int)Mathf.Pow(3f, m_MaxSubdivision - 1));
			if (m_CurrentProbeVolumeChunkSizeInBricks != 0)
			{
				AllocateTemporaryDataLocation();
			}
			m_PositionOffsets[0] = 0f;
			float num = 1f / 3f;
			for (int i = 1; i < 3; i++)
			{
				m_PositionOffsets[i] = (float)i * num;
			}
			m_PositionOffsets[m_PositionOffsets.Length - 1] = 1f;
			m_ProbeReferenceVolumeInit = true;
			ClearDebugData();
			m_NeedLoadAsset = true;
		}
	}

	private void AllocateTemporaryDataLocation()
	{
		m_TemporaryDataLocation.Cleanup();
		m_TemporaryDataLocation = ProbeBrickPool.CreateDataLocation(m_CurrentProbeVolumeChunkSizeInBricks * 64, compressed: false, m_SHBands, "APV_Intermediate", allocateRendertexture: false, allocateValidityData: true, out m_TemporaryDataLocationMemCost);
	}

	private ProbeReferenceVolume()
	{
		m_Transform.posWS = Vector3.zero;
		m_Transform.rot = Quaternion.identity;
		m_Transform.scale = 1f;
	}

	public RuntimeResources GetRuntimeResources()
	{
		if (!m_ProbeReferenceVolumeInit)
		{
			return default(RuntimeResources);
		}
		RuntimeResources rr = default(RuntimeResources);
		m_Index.GetRuntimeResources(ref rr);
		m_CellIndices.GetRuntimeResources(ref rr);
		m_Pool.GetRuntimeResources(ref rr);
		return rr;
	}

	internal void SetTRS(Vector3 position, Quaternion rotation, float minBrickSize)
	{
		m_Transform.posWS = position;
		m_Transform.rot = rotation;
		m_Transform.scale = minBrickSize;
	}

	internal void SetMaxSubdivision(int maxSubdivision)
	{
		m_MaxSubdivision = Math.Min(maxSubdivision, 7);
	}

	internal static int CellSize(int subdivisionLevel)
	{
		return (int)Mathf.Pow(3f, subdivisionLevel);
	}

	internal float BrickSize(int subdivisionLevel)
	{
		return m_Transform.scale * (float)CellSize(subdivisionLevel);
	}

	internal float MinBrickSize()
	{
		return m_Transform.scale;
	}

	internal float MaxBrickSize()
	{
		return BrickSize(m_MaxSubdivision - 1);
	}

	internal RefVolTransform GetTransform()
	{
		return m_Transform;
	}

	internal int GetMaxSubdivision()
	{
		return m_MaxSubdivision;
	}

	internal int GetMaxSubdivision(float multiplier)
	{
		return Mathf.CeilToInt((float)m_MaxSubdivision * multiplier);
	}

	internal float GetDistanceBetweenProbes(int subdivisionLevel)
	{
		return BrickSize(subdivisionLevel) / 3f;
	}

	internal float MinDistanceBetweenProbes()
	{
		return GetDistanceBetweenProbes(0);
	}

	public bool DataHasBeenLoaded()
	{
		return m_BricksLoaded;
	}

	internal void Clear()
	{
		if (m_ProbeReferenceVolumeInit)
		{
			UnloadAllCells();
			m_Pool.Clear();
			m_BlendingPool.Clear();
			m_Index.Clear();
			cells.Clear();
		}
		if (clearAssetsOnVolumeClear)
		{
			m_PendingAssetsToBeLoaded.Clear();
			m_ActiveAssets.Clear();
		}
	}

	private List<ProbeBrickPool.BrickChunkAlloc> GetSourceLocations(int count, int chunkSize, ProbeBrickPool.DataLocation dataLoc)
	{
		ProbeBrickPool.BrickChunkAlloc item = default(ProbeBrickPool.BrickChunkAlloc);
		m_TmpSrcChunks.Clear();
		m_TmpSrcChunks.Add(item);
		for (int i = 1; i < count; i++)
		{
			item.x += chunkSize * 4;
			if (item.x >= dataLoc.width)
			{
				item.x = 0;
				item.y += 4;
				if (item.y >= dataLoc.height)
				{
					item.y = 0;
					item.z += 4;
				}
			}
			m_TmpSrcChunks.Add(item);
		}
		return m_TmpSrcChunks;
	}

	private void UpdatePool(List<ProbeBrickPool.BrickChunkAlloc> chunkList, Cell.PerScenarioData data, NativeArray<byte> validityNeighMaskData, int chunkIndex, int poolIndex)
	{
		int num = m_CurrentProbeVolumeChunkSizeInBricks * 64;
		int start = chunkIndex * num;
		int num2 = num * 4;
		int start2 = chunkIndex * num2;
		(m_TemporaryDataLocation.TexL0_L1rx as Texture3D).SetPixelData(data.shL0L1RxData.GetSubArray(start2, num2), 0);
		(m_TemporaryDataLocation.TexL0_L1rx as Texture3D).Apply(updateMipmaps: false);
		(m_TemporaryDataLocation.TexL1_G_ry as Texture3D).SetPixelData(data.shL1GL1RyData.GetSubArray(start2, num2), 0);
		(m_TemporaryDataLocation.TexL1_G_ry as Texture3D).Apply(updateMipmaps: false);
		(m_TemporaryDataLocation.TexL1_B_rz as Texture3D).SetPixelData(data.shL1BL1RzData.GetSubArray(start2, num2), 0);
		(m_TemporaryDataLocation.TexL1_B_rz as Texture3D).Apply(updateMipmaps: false);
		if (poolIndex == -1)
		{
			m_TemporaryDataLocation.TexValidity.SetPixelData(validityNeighMaskData.GetSubArray(start, num), 0);
			m_TemporaryDataLocation.TexValidity.Apply(updateMipmaps: false);
		}
		if (m_SHBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
		{
			(m_TemporaryDataLocation.TexL2_0 as Texture3D).SetPixelData(data.shL2Data_0.GetSubArray(start2, num2), 0);
			(m_TemporaryDataLocation.TexL2_0 as Texture3D).Apply(updateMipmaps: false);
			(m_TemporaryDataLocation.TexL2_1 as Texture3D).SetPixelData(data.shL2Data_1.GetSubArray(start2, num2), 0);
			(m_TemporaryDataLocation.TexL2_1 as Texture3D).Apply(updateMipmaps: false);
			(m_TemporaryDataLocation.TexL2_2 as Texture3D).SetPixelData(data.shL2Data_2.GetSubArray(start2, num2), 0);
			(m_TemporaryDataLocation.TexL2_2 as Texture3D).Apply(updateMipmaps: false);
			(m_TemporaryDataLocation.TexL2_3 as Texture3D).SetPixelData(data.shL2Data_3.GetSubArray(start2, num2), 0);
			(m_TemporaryDataLocation.TexL2_3 as Texture3D).Apply(updateMipmaps: false);
		}
		List<ProbeBrickPool.BrickChunkAlloc> sourceLocations = GetSourceLocations(1, m_CurrentProbeVolumeChunkSizeInBricks, m_TemporaryDataLocation);
		if (poolIndex == -1)
		{
			m_Pool.Update(m_TemporaryDataLocation, sourceLocations, chunkList, chunkIndex, m_SHBands);
		}
		else
		{
			m_BlendingPool.Update(m_TemporaryDataLocation, sourceLocations, chunkList, chunkIndex, m_SHBands, poolIndex);
		}
	}

	private void UpdatePoolValidity(List<ProbeBrickPool.BrickChunkAlloc> chunkList, Cell.PerScenarioData data, NativeArray<byte> validityNeighMaskData, int chunkIndex)
	{
		int num = m_CurrentProbeVolumeChunkSizeInBricks * 64;
		int start = chunkIndex * num;
		m_TemporaryDataLocation.TexValidity.SetPixelData(validityNeighMaskData.GetSubArray(start, num), 0);
		m_TemporaryDataLocation.TexValidity.Apply(updateMipmaps: false);
		List<ProbeBrickPool.BrickChunkAlloc> sourceLocations = GetSourceLocations(1, m_CurrentProbeVolumeChunkSizeInBricks, m_TemporaryDataLocation);
		m_Pool.UpdateValidity(m_TemporaryDataLocation, sourceLocations, chunkList, chunkIndex);
	}

	private bool AddBlendingBricks(BlendingCellInfo blendingCell)
	{
		using (new ProfilerMarker("AddBlendingBricks").Auto())
		{
			Cell cell = blendingCell.cellInfo.cell;
			bool flag = sceneData.otherScenario == null || !cell.hasTwoScenarios;
			if (!flag && !m_BlendingPool.Allocate(cell.shChunkCount, blendingCell.chunkList))
			{
				return false;
			}
			List<ProbeBrickPool.BrickChunkAlloc> list = (flag ? blendingCell.cellInfo.chunkList : blendingCell.chunkList);
			int count = list.Count;
			if (!blendingCell.cellInfo.indexUpdated)
			{
				UpdateCellIndex(blendingCell.cellInfo);
				for (int i = 0; i < count; i++)
				{
					UpdatePoolValidity(list, cell.scenario0, cell.validityNeighMaskData, i);
				}
			}
			if (flag)
			{
				if (blendingCell.blendingFactor != scenarioBlendingFactor)
				{
					for (int j = 0; j < count; j++)
					{
						UpdatePool(list, cell.scenario0, cell.validityNeighMaskData, j, -1);
					}
				}
			}
			else
			{
				for (int k = 0; k < count; k++)
				{
					UpdatePool(list, cell.scenario0, cell.validityNeighMaskData, k, 0);
					UpdatePool(list, cell.scenario1, cell.validityNeighMaskData, k, 1);
				}
			}
			blendingCell.blending = true;
			return true;
		}
	}

	private bool AddBricks(CellInfo cellInfo, ProbeBrickIndex.CellIndexUpdateInfo cellUpdateInfo, bool ignoreErrorLog)
	{
		using (new ProfilerMarker("AddBricks").Auto())
		{
			Cell cell = cellInfo.cell;
			int chunkCount = ProbeBrickPool.GetChunkCount(cell.bricks.Length, m_CurrentProbeVolumeChunkSizeInBricks);
			cellInfo.chunkList.Clear();
			if (!m_Pool.Allocate(chunkCount, cellInfo.chunkList, ignoreErrorLog))
			{
				return false;
			}
			if (enableScenarioBlending)
			{
				m_ToBeLoadedBlendingCells.Add(in cellInfo.blendingCell);
			}
			cellInfo.tempUpdateInfo = cellUpdateInfo;
			if (!enableScenarioBlending || scenarioBlendingFactor == 0f || !cell.hasTwoScenarios)
			{
				for (int i = 0; i < cellInfo.chunkList.Count; i++)
				{
					UpdatePool(cellInfo.chunkList, cell.scenario0, cell.validityNeighMaskData, i, -1);
				}
				UpdateCellIndex(cellInfo);
				cellInfo.blendingCell.blendingFactor = 0f;
			}
			else if (enableScenarioBlending)
			{
				cellInfo.blendingCell.Prioritize();
				m_HasRemainingCellsToBlend = true;
				cellInfo.indexUpdated = false;
			}
			cellInfo.loaded = true;
			ClearDebugData();
			return true;
		}
	}

	private void UpdateCellIndex(CellInfo cellInfo)
	{
		cellInfo.indexUpdated = true;
		m_BricksLoaded = true;
		NativeArray<ProbeBrickIndex.Brick> bricks = cellInfo.cell.bricks;
		ProbeBrickIndex.CellIndexUpdateInfo tempUpdateInfo = cellInfo.tempUpdateInfo;
		m_Index.AddBricks(cellInfo.cell, bricks, cellInfo.chunkList, m_CurrentProbeVolumeChunkSizeInBricks, m_Pool.GetPoolWidth(), m_Pool.GetPoolHeight(), tempUpdateInfo);
		cellInfo.updateInfo = tempUpdateInfo;
		m_CellIndices.UpdateCell(cellInfo.flatIdxInCellIndices, tempUpdateInfo);
	}

	private void ReleaseBricks(CellInfo cellInfo)
	{
		if (cellInfo.chunkList.Count == 0)
		{
			Debug.Log("Tried to release bricks from an empty Cell.");
			return;
		}
		m_Index.RemoveBricks(cellInfo);
		m_Pool.Deallocate(cellInfo.chunkList);
		cellInfo.chunkList.Clear();
	}

	public void UpdateConstantBuffer(CommandBuffer cmd, ProbeVolumeShadingParameters parameters)
	{
		float num = parameters.normalBias;
		float num2 = parameters.viewBias;
		if (parameters.scaleBiasByMinDistanceBetweenProbes)
		{
			num *= MinDistanceBetweenProbes();
			num2 *= MinDistanceBetweenProbes();
		}
		Vector3Int cellMinPosition = m_CellIndices.GetCellMinPosition();
		Vector3Int cellIndexDimension = m_CellIndices.GetCellIndexDimension();
		Vector3Int poolDimensions = m_Pool.GetPoolDimensions();
		ShaderVariablesProbeVolumes data = default(ShaderVariablesProbeVolumes);
		data._Biases_CellInMinBrick_MinBrickSize = new Vector4(num, num2, (int)Mathf.Pow(3f, m_MaxSubdivision - 1), MinBrickSize());
		data._IndicesDim_IndexChunkSize = new Vector4(cellIndexDimension.x, cellIndexDimension.y, cellIndexDimension.z, 243f);
		data._MinCellPos_Noise = new Vector4(cellMinPosition.x, cellMinPosition.y, cellMinPosition.z, parameters.samplingNoise);
		data._PoolDim_CellInMeters = new Vector4(poolDimensions.x, poolDimensions.y, poolDimensions.z, MaxBrickSize());
		data._Weight_MinLoadedCell = new Vector4(parameters.weight, minLoadedCellPos.x, minLoadedCellPos.y, minLoadedCellPos.z);
		data._MaxLoadedCell_FrameIndex = new Vector4(maxLoadedCellPos.x, maxLoadedCellPos.y, maxLoadedCellPos.z, parameters.frameIndexForNoise);
		data._LeakReductionParams = new Vector4((float)parameters.leakReductionMode, parameters.occlusionWeightContribution, parameters.minValidNormalWeight, 0f);
		data._NormalizationClamp_Padding12 = new Vector4(parameters.reflNormalizationLowerClamp, parameters.reflNormalizationUpperClamp, 0f, 0f);
		ConstantBuffer.PushGlobal(cmd, in data, m_CBShaderID);
	}

	private void CleanupLoadedData()
	{
		m_BricksLoaded = false;
		UnloadAllCells();
		if (m_ProbeReferenceVolumeInit)
		{
			m_Index.Cleanup();
			m_CellIndices.Cleanup();
			m_Pool.Cleanup();
			m_BlendingPool.Cleanup();
			m_TemporaryDataLocation.Cleanup();
		}
		m_ProbeReferenceVolumeInit = false;
		ClearDebugData();
	}

	public void RenderDebug(Camera camera)
	{
		if (camera.cameraType != CameraType.Reflection && camera.cameraType != CameraType.Preview)
		{
			DrawProbeDebug(camera);
		}
	}

	private void InitializeDebug(in ProbeVolumeSystemParameters parameters)
	{
		if (parameters.supportsRuntimeDebug)
		{
			m_DebugMesh = parameters.probeDebugMesh;
			m_DebugMaterial = CoreUtils.CreateEngineMaterial(parameters.probeDebugShader);
			m_DebugMaterial.enableInstancing = true;
			m_DebugOffsetMesh = parameters.offsetDebugMesh;
			m_DebugOffsetMaterial = CoreUtils.CreateEngineMaterial(parameters.offsetDebugShader);
			m_DebugOffsetMaterial.enableInstancing = true;
			subdivisionDebugColors[0] = new Color(1f, 0f, 0f);
			subdivisionDebugColors[1] = new Color(0f, 1f, 0f);
			subdivisionDebugColors[2] = new Color(0f, 0f, 1f);
			subdivisionDebugColors[3] = new Color(1f, 1f, 0f);
			subdivisionDebugColors[4] = new Color(1f, 0f, 1f);
			subdivisionDebugColors[5] = new Color(0f, 1f, 1f);
			subdivisionDebugColors[6] = new Color(0.5f, 0.5f, 0.5f);
		}
		RegisterDebug(parameters);
	}

	private void CleanupDebug()
	{
		UnregisterDebug(destroyPanel: true);
		CoreUtils.Destroy(m_DebugMaterial);
		CoreUtils.Destroy(m_DebugOffsetMaterial);
	}

	private void DebugCellIndexChanged<T>(DebugUI.Field<T> field, T value)
	{
		ClearDebugData();
	}

	private void RegisterDebug(ProbeVolumeSystemParameters parameters)
	{
		List<DebugUI.Widget> list = new List<DebugUI.Widget>();
		DebugUI.Container container = new DebugUI.Container
		{
			displayName = "Subdivision Visualization"
		};
		container.children.Add(new DebugUI.BoolField
		{
			displayName = "Display Cells",
			getter = () => probeVolumeDebug.drawCells,
			setter = delegate(bool value)
			{
				probeVolumeDebug.drawCells = value;
			},
			onValueChanged = RefreshDebug<bool>
		});
		container.children.Add(new DebugUI.BoolField
		{
			displayName = "Display Bricks",
			getter = () => probeVolumeDebug.drawBricks,
			setter = delegate(bool value)
			{
				probeVolumeDebug.drawBricks = value;
			},
			onValueChanged = RefreshDebug<bool>
		});
		container.children.Add(new DebugUI.FloatField
		{
			displayName = "Culling Distance",
			getter = () => probeVolumeDebug.subdivisionViewCullingDistance,
			setter = delegate(float value)
			{
				probeVolumeDebug.subdivisionViewCullingDistance = value;
			},
			min = () => 0f
		});
		DebugUI.Container container2 = new DebugUI.Container
		{
			displayName = "Probe Visualization"
		};
		container2.children.Add(new DebugUI.BoolField
		{
			displayName = "Display Probes",
			getter = () => probeVolumeDebug.drawProbes,
			setter = delegate(bool value)
			{
				probeVolumeDebug.drawProbes = value;
			},
			onValueChanged = RefreshDebug<bool>
		});
		if (probeVolumeDebug.drawProbes)
		{
			DebugUI.Container container3 = new DebugUI.Container();
			container3.children.Add(new DebugUI.EnumField
			{
				displayName = "Probe Shading Mode",
				getter = () => (int)probeVolumeDebug.probeShading,
				setter = delegate(int value)
				{
					probeVolumeDebug.probeShading = (DebugProbeShadingMode)value;
				},
				autoEnum = typeof(DebugProbeShadingMode),
				getIndex = () => (int)probeVolumeDebug.probeShading,
				setIndex = delegate(int value)
				{
					probeVolumeDebug.probeShading = (DebugProbeShadingMode)value;
				},
				onValueChanged = RefreshDebug<int>
			});
			container3.children.Add(new DebugUI.FloatField
			{
				displayName = "Probe Size",
				getter = () => probeVolumeDebug.probeSize,
				setter = delegate(float value)
				{
					probeVolumeDebug.probeSize = value;
				},
				min = () => 0.05f,
				max = () => 10f
			});
			if (probeVolumeDebug.probeShading == DebugProbeShadingMode.SH || probeVolumeDebug.probeShading == DebugProbeShadingMode.SHL0 || probeVolumeDebug.probeShading == DebugProbeShadingMode.SHL0L1)
			{
				container3.children.Add(new DebugUI.FloatField
				{
					displayName = "Probe Exposure Compensation",
					getter = () => probeVolumeDebug.exposureCompensation,
					setter = delegate(float value)
					{
						probeVolumeDebug.exposureCompensation = value;
					}
				});
			}
			container3.children.Add(new DebugUI.IntField
			{
				displayName = "Max subdivision displayed",
				getter = () => probeVolumeDebug.maxSubdivToVisualize,
				setter = delegate(int v)
				{
					probeVolumeDebug.maxSubdivToVisualize = Mathf.Min(v, instance.GetMaxSubdivision() - 1);
				},
				min = () => 0,
				max = () => instance.GetMaxSubdivision() - 1
			});
			container3.children.Add(new DebugUI.IntField
			{
				displayName = "Min subdivision displayed",
				getter = () => probeVolumeDebug.minSubdivToVisualize,
				setter = delegate(int v)
				{
					probeVolumeDebug.minSubdivToVisualize = Mathf.Max(v, 0);
				},
				min = () => 0,
				max = () => instance.GetMaxSubdivision() - 1
			});
			container2.children.Add(container3);
		}
		container2.children.Add(new DebugUI.BoolField
		{
			displayName = "Virtual Offset",
			getter = () => probeVolumeDebug.drawVirtualOffsetPush,
			setter = delegate(bool value)
			{
				probeVolumeDebug.drawVirtualOffsetPush = value;
				if (probeVolumeDebug.drawVirtualOffsetPush && probeVolumeDebug.drawProbes)
				{
					float value2 = (float)CellSize(0) * MinBrickSize() / 3f * bakingProcessSettings.virtualOffsetSettings.searchMultiplier + bakingProcessSettings.virtualOffsetSettings.outOfGeoOffset;
					probeVolumeDebug.probeSize = Mathf.Min(probeVolumeDebug.probeSize, Mathf.Clamp(value2, 0.05f, 10f));
				}
			},
			onValueChanged = RefreshDebug<bool>
		});
		if (probeVolumeDebug.drawVirtualOffsetPush)
		{
			DebugUI.FloatField item = new DebugUI.FloatField
			{
				displayName = "Offset Size",
				getter = () => probeVolumeDebug.offsetSize,
				setter = delegate(float value)
				{
					probeVolumeDebug.offsetSize = value;
				},
				min = () => 0.001f,
				max = () => 0.1f
			};
			container2.children.Add(new DebugUI.Container
			{
				children = { (DebugUI.Widget)item }
			});
		}
		container2.children.Add(new DebugUI.FloatField
		{
			displayName = "Culling Distance",
			getter = () => probeVolumeDebug.probeCullingDistance,
			setter = delegate(float value)
			{
				probeVolumeDebug.probeCullingDistance = value;
			},
			min = () => 0f
		});
		DebugUI.Container container4 = new DebugUI.Container
		{
			displayName = "Streaming"
		};
		container4.children.Add(new DebugUI.BoolField
		{
			displayName = "Freeze Streaming",
			getter = () => probeVolumeDebug.freezeStreaming,
			setter = delegate(bool value)
			{
				probeVolumeDebug.freezeStreaming = value;
			}
		});
		container4.children.Add(new DebugUI.IntField
		{
			displayName = "Number Of Cells Loaded Per Frame",
			getter = () => instance.numberOfCellsLoadedPerFrame,
			setter = delegate(int value)
			{
				instance.SetNumberOfCellsLoadedPerFrame(value);
			},
			min = () => 0
		});
		if (parameters.supportsRuntimeDebug)
		{
			if (Application.isEditor)
			{
				list.Add(container);
			}
			list.Add(container2);
		}
		if (parameters.supportStreaming)
		{
			list.Add(container4);
		}
		if (parameters.scenarioBlendingShader != null && parameters.blendingMemoryBudget != ProbeVolumeBlendingTextureMemoryBudget.None)
		{
			DebugUI.Container container5 = new DebugUI.Container
			{
				displayName = "Scenario Blending"
			};
			container5.children.Add(new DebugUI.IntField
			{
				displayName = "Number Of Cells Blended Per Frame",
				getter = () => instance.numberOfCellsBlendedPerFrame,
				setter = delegate(int value)
				{
					instance.numberOfCellsBlendedPerFrame = value;
				},
				min = () => 0
			});
			container5.children.Add(new DebugUI.FloatField
			{
				displayName = "Turnover Rate",
				getter = () => instance.turnoverRate,
				setter = delegate(float value)
				{
					instance.turnoverRate = value;
				},
				min = () => 0f,
				max = () => 1f
			});
			m_DebugScenarioField = new DebugUI.EnumField
			{
				displayName = "Scenario To Blend With",
				enumNames = m_DebugScenarioNames,
				enumValues = m_DebugScenarioValues,
				getIndex = delegate
				{
					RefreshScenarioNames(ProbeVolumeSceneData.GetSceneGUID(SceneManager.GetActiveScene()));
					probeVolumeDebug.otherStateIndex = 0;
					if (!string.IsNullOrEmpty(sceneData.otherScenario))
					{
						for (int i = 1; i < m_DebugScenarioNames.Length; i++)
						{
							if (m_DebugScenarioNames[i].text == sceneData.otherScenario)
							{
								probeVolumeDebug.otherStateIndex = i;
								break;
							}
						}
					}
					return probeVolumeDebug.otherStateIndex;
				},
				setIndex = delegate(int value)
				{
					string otherScenario = ((value == 0) ? null : m_DebugScenarioNames[value].text);
					sceneData.BlendLightingScenario(otherScenario, sceneData.scenarioBlendingFactor);
					probeVolumeDebug.otherStateIndex = value;
				},
				getter = () => probeVolumeDebug.otherStateIndex,
				setter = delegate(int value)
				{
					probeVolumeDebug.otherStateIndex = value;
				}
			};
			container5.children.Add(m_DebugScenarioField);
			container5.children.Add(new DebugUI.FloatField
			{
				displayName = "Scenario Blending Factor",
				getter = () => instance.scenarioBlendingFactor,
				setter = delegate(float value)
				{
					instance.scenarioBlendingFactor = value;
				},
				min = () => 0f,
				max = () => 1f
			});
			list.Add(container5);
		}
		if (list.Count > 0)
		{
			m_DebugItems = list.ToArray();
			DebugManager.instance.GetPanel(k_DebugPanelName, createIfNull: true).children.Add(m_DebugItems);
		}
		DebugManager.instance.RegisterData(probeVolumeDebug);
		void RefreshDebug<T>(DebugUI.Field<T> field, T value)
		{
			UnregisterDebug(destroyPanel: false);
			RegisterDebug(parameters);
		}
		void RefreshScenarioNames(string guid)
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (ProbeVolumeSceneData.BakingSet bakingSet in parameters.sceneData.bakingSets)
			{
				if (bakingSet.sceneGUIDs.Contains(guid))
				{
					foreach (string lightingScenario in bakingSet.lightingScenarios)
					{
						hashSet.Add(lightingScenario);
					}
				}
			}
			hashSet.Remove(sceneData.lightingScenario);
			if (!(m_DebugActiveSceneGUID == guid) || hashSet.Count + 1 != m_DebugScenarioNames.Length || !(m_DebugActiveScenario == sceneData.lightingScenario))
			{
				int num = 0;
				ArrayExtensions.ResizeArray(ref m_DebugScenarioNames, hashSet.Count + 1);
				ArrayExtensions.ResizeArray(ref m_DebugScenarioValues, hashSet.Count + 1);
				m_DebugScenarioNames[0] = new GUIContent("None");
				m_DebugScenarioValues[0] = 0;
				foreach (string item2 in hashSet)
				{
					num++;
					m_DebugScenarioNames[num] = new GUIContent(item2);
					m_DebugScenarioValues[num] = num;
				}
				m_DebugActiveSceneGUID = guid;
				m_DebugActiveScenario = sceneData.lightingScenario;
				m_DebugScenarioField.enumNames = m_DebugScenarioNames;
				m_DebugScenarioField.enumValues = m_DebugScenarioValues;
				if (probeVolumeDebug.otherStateIndex >= m_DebugScenarioNames.Length)
				{
					probeVolumeDebug.otherStateIndex = 0;
				}
			}
		}
	}

	private void UnregisterDebug(bool destroyPanel)
	{
		if (destroyPanel)
		{
			DebugManager.instance.RemovePanel(k_DebugPanelName);
		}
		else
		{
			DebugManager.instance.GetPanel(k_DebugPanelName).children.Remove(m_DebugItems);
		}
	}

	private bool ShouldCullCell(Vector3 cellPosition, Transform cameraTransform, Plane[] frustumPlanes)
	{
		float num = MaxBrickSize();
		Vector3 posWS = GetTransform().posWS;
		Vector3 vector = cellPosition * num + posWS + Vector3.one * (num / 2f);
		float num2 = (float)Mathf.CeilToInt(probeVolumeDebug.probeCullingDistance / num) * num;
		if (Vector3.Distance(cameraTransform.position, vector) > num2)
		{
			return true;
		}
		Bounds bounds = new Bounds(vector, num * Vector3.one);
		return !GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
	}

	private void DrawProbeDebug(Camera camera)
	{
		if (!enabledBySRP || !isInitialized || (!probeVolumeDebug.drawProbes && !probeVolumeDebug.drawVirtualOffsetPush))
		{
			return;
		}
		GeometryUtility.CalculateFrustumPlanes(camera, m_DebugFrustumPlanes);
		m_DebugMaterial.shaderKeywords = null;
		if (m_SHBands == ProbeVolumeSHBands.SphericalHarmonicsL1)
		{
			m_DebugMaterial.EnableKeyword("PROBE_VOLUMES_L1");
		}
		else if (m_SHBands == ProbeVolumeSHBands.SphericalHarmonicsL2)
		{
			m_DebugMaterial.EnableKeyword("PROBE_VOLUMES_L2");
		}
		m_DebugMaterial.renderQueue = 3000;
		m_DebugOffsetMaterial.renderQueue = 3000;
		int num = ((instance.cells.Count > 0) ? (instance.GetMaxSubdivision() - 1) : 0);
		foreach (CellInfo value in instance.cells.Values)
		{
			num = Mathf.Min(num, value.cell.minSubdiv);
		}
		probeVolumeDebug.maxSubdivToVisualize = Mathf.Min(probeVolumeDebug.maxSubdivToVisualize, instance.GetMaxSubdivision() - 1);
		m_MaxSubdivVisualizedIsMaxAvailable = probeVolumeDebug.maxSubdivToVisualize == instance.GetMaxSubdivision() - 1;
		probeVolumeDebug.minSubdivToVisualize = Mathf.Clamp(probeVolumeDebug.minSubdivToVisualize, num, probeVolumeDebug.maxSubdivToVisualize);
		foreach (CellInfo value2 in instance.cells.Values)
		{
			if (ShouldCullCell(value2.cell.position, camera.transform, m_DebugFrustumPlanes))
			{
				continue;
			}
			CellInstancedDebugProbes cellInstancedDebugProbes = CreateInstancedProbes(value2);
			if (cellInstancedDebugProbes == null)
			{
				continue;
			}
			for (int i = 0; i < cellInstancedDebugProbes.probeBuffers.Count; i++)
			{
				MaterialPropertyBlock materialPropertyBlock = cellInstancedDebugProbes.props[i];
				materialPropertyBlock.SetInt("_ShadingMode", (int)probeVolumeDebug.probeShading);
				materialPropertyBlock.SetFloat("_ExposureCompensation", probeVolumeDebug.exposureCompensation);
				materialPropertyBlock.SetFloat("_ProbeSize", probeVolumeDebug.probeSize);
				materialPropertyBlock.SetFloat("_CullDistance", probeVolumeDebug.probeCullingDistance);
				materialPropertyBlock.SetInt("_MaxAllowedSubdiv", probeVolumeDebug.maxSubdivToVisualize);
				materialPropertyBlock.SetInt("_MinAllowedSubdiv", probeVolumeDebug.minSubdivToVisualize);
				materialPropertyBlock.SetFloat("_ValidityThreshold", bakingProcessSettings.dilationSettings.dilationValidityThreshold);
				materialPropertyBlock.SetFloat("_OffsetSize", probeVolumeDebug.offsetSize);
				if (probeVolumeDebug.drawProbes)
				{
					Matrix4x4[] array = cellInstancedDebugProbes.probeBuffers[i];
					Graphics.DrawMeshInstanced(m_DebugMesh, 0, m_DebugMaterial, array, array.Length, materialPropertyBlock, ShadowCastingMode.Off, receiveShadows: false, 0, camera, LightProbeUsage.Off, null);
				}
				if (probeVolumeDebug.drawVirtualOffsetPush)
				{
					Matrix4x4[] array2 = cellInstancedDebugProbes.offsetBuffers[i];
					Graphics.DrawMeshInstanced(m_DebugOffsetMesh, 0, m_DebugOffsetMaterial, array2, array2.Length, materialPropertyBlock, ShadowCastingMode.Off, receiveShadows: false, 0, camera, LightProbeUsage.Off, null);
				}
			}
		}
	}

	internal void ResetDebugViewToMaxSubdiv()
	{
		if (m_MaxSubdivVisualizedIsMaxAvailable)
		{
			probeVolumeDebug.maxSubdivToVisualize = instance.GetMaxSubdivision() - 1;
		}
	}

	private void ClearDebugData()
	{
		realtimeSubdivisionInfo.Clear();
	}

	private CellInstancedDebugProbes CreateInstancedProbes(CellInfo cellInfo)
	{
		if (cellInfo.debugProbes != null)
		{
			return cellInfo.debugProbes;
		}
		int num = instance.GetMaxSubdivision() - 1;
		Cell cell = cellInfo.cell;
		if (!cell.bricks.IsCreated || cell.bricks.Length == 0 || !cellInfo.loaded)
		{
			return null;
		}
		List<Matrix4x4[]> list = new List<Matrix4x4[]>();
		List<Matrix4x4[]> list2 = new List<Matrix4x4[]>();
		List<MaterialPropertyBlock> list3 = new List<MaterialPropertyBlock>();
		List<ProbeBrickPool.BrickChunkAlloc> chunkList = cellInfo.chunkList;
		Vector4[] array = new Vector4[511];
		float[] array2 = new float[511];
		float[] array3 = new float[511];
		float[] array4 = ((cell.touchupVolumeInteraction.Length > 0) ? new float[511] : null);
		Vector4[] array5 = ((cell.offsetVectors.Length > 0) ? new Vector4[511] : null);
		List<Matrix4x4> list4 = new List<Matrix4x4>();
		List<Matrix4x4> list5 = new List<Matrix4x4>();
		CellInstancedDebugProbes cellInstancedDebugProbes = new CellInstancedDebugProbes();
		cellInstancedDebugProbes.probeBuffers = list;
		cellInstancedDebugProbes.offsetBuffers = list2;
		cellInstancedDebugProbes.props = list3;
		int num2 = m_CurrentProbeVolumeChunkSizeInBricks * 64;
		Vector3Int vector3Int = ProbeBrickPool.ProbeCountToDataLocSize(num2);
		int num3 = 0;
		int num4 = 0;
		int num5 = cell.probeCount / 64;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		for (int i = 0; i < num5; i++)
		{
			int subdivisionLevel = cell.bricks[i].subdivisionLevel;
			int num9 = i / m_CurrentProbeVolumeChunkSizeInBricks;
			ProbeBrickPool.BrickChunkAlloc brickChunkAlloc = chunkList[num9];
			Vector3Int vector3Int2 = new Vector3Int(brickChunkAlloc.x + num6, brickChunkAlloc.y + num7, brickChunkAlloc.z + num8);
			for (int j = 0; j < 4; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					for (int l = 0; l < 4; l++)
					{
						Vector3Int vector3Int3 = new Vector3Int(vector3Int2.x + l, vector3Int2.y + k, vector3Int2.z + j);
						int index = num9 * num2 + (num6 + l) + vector3Int.x * (num7 + k + vector3Int.y * (num8 + j));
						list4.Add(Matrix4x4.TRS(cell.probePositions[index], Quaternion.identity, Vector3.one * (0.3f * (float)(subdivisionLevel + 1))));
						array2[num3] = cell.validity[index];
						array[num3] = new Vector4(vector3Int3.x, vector3Int3.y, vector3Int3.z, subdivisionLevel);
						array3[num3] = (float)subdivisionLevel / (float)num;
						if (array4 != null)
						{
							array4[num3] = cell.touchupVolumeInteraction[index];
						}
						if (array5 != null)
						{
							Vector3 vector = cell.offsetVectors[index];
							array5[num3] = vector;
							if (vector.sqrMagnitude < 1E-06f)
							{
								list5.Add(Matrix4x4.identity);
							}
							else
							{
								Vector3 pos = cell.probePositions[index] + vector;
								Quaternion q = Quaternion.LookRotation(-vector);
								Vector3 s = new Vector3(0.5f, 0.5f, vector.magnitude);
								list5.Add(Matrix4x4.TRS(pos, q, s));
							}
						}
						num3++;
						if (list4.Count >= 511 || num4 == cell.probeCount - 1)
						{
							num3 = 0;
							MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
							materialPropertyBlock.SetFloatArray("_Validity", array2);
							materialPropertyBlock.SetFloatArray("_TouchupedByVolume", array4);
							materialPropertyBlock.SetFloatArray("_RelativeSize", array3);
							materialPropertyBlock.SetVectorArray("_IndexInAtlas", array);
							if (array5 != null)
							{
								materialPropertyBlock.SetVectorArray("_Offset", array5);
							}
							list3.Add(materialPropertyBlock);
							list.Add(list4.ToArray());
							list4 = new List<Matrix4x4>();
							list4.Clear();
							list2.Add(list5.ToArray());
							list5.Clear();
						}
						num4++;
					}
				}
			}
			num6 += 4;
			if (num6 < vector3Int.x)
			{
				continue;
			}
			num6 = 0;
			num7 += 4;
			if (num7 >= vector3Int.y)
			{
				num7 = 0;
				num8 += 4;
				if (num8 >= vector3Int.z)
				{
					num6 = 0;
					num7 = 0;
					num8 = 0;
				}
			}
		}
		cellInfo.debugProbes = cellInstancedDebugProbes;
		return cellInstancedDebugProbes;
	}

	private void OnClearLightingdata()
	{
		ClearDebugData();
	}

	internal void ScenarioBlendingChanged(bool scenarioChanged)
	{
		m_HasRemainingCellsToBlend = true;
		if (scenarioChanged)
		{
			UnloadAllBlendingCells();
			for (int i = 0; i < m_ToBeLoadedBlendingCells.size; i++)
			{
				m_ToBeLoadedBlendingCells[i].ForceReupload();
			}
		}
	}

	public void SetNumberOfCellsLoadedPerFrame(int numberOfCells)
	{
		m_NumberOfCellsLoadedPerFrame = Mathf.Max(1, numberOfCells);
	}

	private void ComputeCellCameraDistance(Vector3 cameraPosition, DynamicArray<CellInfo> cells)
	{
		for (int i = 0; i < cells.size; i++)
		{
			CellInfo cellInfo = cells[i];
			cellInfo.streamingScore = Vector3.Distance(cameraPosition, cellInfo.cell.position);
		}
	}

	private void ComputeStreamingScoreForBlending(DynamicArray<BlendingCellInfo> cells, float worstScore)
	{
		float num = scenarioBlendingFactor;
		for (int i = 0; i < cells.size; i++)
		{
			BlendingCellInfo blendingCellInfo = cells[i];
			if (num == blendingCellInfo.blendingFactor)
			{
				blendingCellInfo.MarkUpToDate();
				continue;
			}
			blendingCellInfo.streamingScore = blendingCellInfo.cellInfo.streamingScore;
			if (blendingCellInfo.ShouldPrioritize())
			{
				blendingCellInfo.streamingScore -= worstScore;
			}
		}
	}

	private bool TryLoadCell(CellInfo cellInfo, ref int shBudget, ref int indexBudget, DynamicArray<CellInfo> loadedCells)
	{
		if (cellInfo.cell.shChunkCount <= shBudget && cellInfo.cell.indexChunkCount <= indexBudget && LoadCell(cellInfo))
		{
			loadedCells.Add(in cellInfo);
			shBudget -= cellInfo.cell.shChunkCount;
			indexBudget -= cellInfo.cell.indexChunkCount;
			return true;
		}
		return false;
	}

	private void UnloadBlendingCell(BlendingCellInfo blendingCell, DynamicArray<BlendingCellInfo> unloadedCells)
	{
		UnloadBlendingCell(blendingCell);
		unloadedCells.Add(in blendingCell);
	}

	private bool TryLoadBlendingCell(BlendingCellInfo blendingCell, DynamicArray<BlendingCellInfo> loadedCells)
	{
		if (!AddBlendingBricks(blendingCell))
		{
			return false;
		}
		loadedCells.Add(in blendingCell);
		return true;
	}

	public void UpdateCellStreaming(CommandBuffer cmd, Camera camera)
	{
		if (!isInitialized)
		{
			return;
		}
		using (new ProfilingScope(null, ProfilingSampler.Get(CoreProfileId.APVCellStreamingUpdate)))
		{
			Vector3 position = camera.transform.position;
			if (!probeVolumeDebug.freezeStreaming)
			{
				m_FrozenCameraPosition = position;
			}
			Vector3 cameraPosition = (m_FrozenCameraPosition - m_Transform.posWS) / MaxBrickSize() - Vector3.one * 0.5f;
			ComputeCellCameraDistance(cameraPosition, m_ToBeLoadedCells);
			ComputeCellCameraDistance(cameraPosition, m_LoadedCells);
			m_ToBeLoadedCells.QuickSort();
			m_LoadedCells.QuickSort();
			int indexBudget = m_Index.GetRemainingChunkCount();
			int shBudget = m_Pool.GetRemainingChunkCount();
			int num = Mathf.Min(m_NumberOfCellsLoadedPerFrame, m_ToBeLoadedCells.size);
			if (m_SupportStreaming)
			{
				while (m_TempCellToLoadList.size < num)
				{
					CellInfo cellInfo = m_ToBeLoadedCells[m_TempCellToLoadList.size];
					if (!TryLoadCell(cellInfo, ref shBudget, ref indexBudget, m_TempCellToLoadList))
					{
						break;
					}
				}
				if (m_TempCellToLoadList.size != num)
				{
					int num2 = 0;
					while (m_TempCellToLoadList.size < num && m_LoadedCells.size - num2 != 0)
					{
						CellInfo value = m_LoadedCells[m_LoadedCells.size - num2 - 1];
						CellInfo cellInfo2 = m_ToBeLoadedCells[m_TempCellToLoadList.size];
						if (!(value.streamingScore > cellInfo2.streamingScore))
						{
							break;
						}
						num2++;
						UnloadCell(value);
						shBudget += value.cell.shChunkCount;
						indexBudget += value.cell.indexChunkCount;
						m_TempCellToUnloadList.Add(in value);
						TryLoadCell(cellInfo2, ref shBudget, ref indexBudget, m_TempCellToLoadList);
					}
					if (num2 > 0)
					{
						m_LoadedCells.RemoveRange(m_LoadedCells.size - num2, num2);
						RecomputeMinMaxLoadedCellPos();
					}
				}
			}
			else
			{
				for (int i = 0; i < num; i++)
				{
					CellInfo cellInfo3 = m_ToBeLoadedCells[m_TempCellToLoadList.size];
					TryLoadCell(cellInfo3, ref shBudget, ref indexBudget, m_TempCellToLoadList);
				}
			}
			m_ToBeLoadedCells.RemoveRange(0, m_TempCellToLoadList.size);
			m_LoadedCells.AddRange(m_TempCellToLoadList);
			m_ToBeLoadedCells.AddRange(m_TempCellToUnloadList);
			m_TempCellToLoadList.Clear();
			m_TempCellToUnloadList.Clear();
		}
		if (!enableScenarioBlending)
		{
			return;
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(CoreProfileId.APVScenarioBlendingUpdate)))
		{
			UpdateBlendingCellStreaming(cmd);
		}
	}

	private int FindWorstBlendingCellToBeLoaded()
	{
		int result = -1;
		float num = -1f;
		float num2 = scenarioBlendingFactor;
		for (int i = m_TempBlendingCellToLoadList.size; i < m_ToBeLoadedBlendingCells.size; i++)
		{
			float num3 = Mathf.Abs(m_ToBeLoadedBlendingCells[i].blendingFactor - num2);
			if (num3 > num)
			{
				result = i;
				if (m_ToBeLoadedBlendingCells[i].ShouldReupload())
				{
					break;
				}
				num = num3;
			}
		}
		return result;
	}

	private void UpdateBlendingCellStreaming(CommandBuffer cmd)
	{
		if (!m_HasRemainingCellsToBlend)
		{
			return;
		}
		float a = ((m_LoadedCells.size != 0) ? m_LoadedCells[m_LoadedCells.size - 1].streamingScore : 0f);
		float b = ((m_ToBeLoadedCells.size != 0) ? m_ToBeLoadedCells[m_ToBeLoadedCells.size - 1].streamingScore : 0f);
		float worstScore = Mathf.Max(a, b);
		ComputeStreamingScoreForBlending(m_ToBeLoadedBlendingCells, worstScore);
		ComputeStreamingScoreForBlending(m_LoadedBlendingCells, worstScore);
		m_ToBeLoadedBlendingCells.QuickSort();
		m_LoadedBlendingCells.QuickSort();
		int num = Mathf.Min(m_NumberOfCellsLoadedPerFrame, m_ToBeLoadedBlendingCells.size);
		while (m_TempBlendingCellToLoadList.size < num)
		{
			BlendingCellInfo blendingCell = m_ToBeLoadedBlendingCells[m_TempBlendingCellToLoadList.size];
			if (!TryLoadBlendingCell(blendingCell, m_TempBlendingCellToLoadList))
			{
				break;
			}
		}
		if (m_TempBlendingCellToLoadList.size != num)
		{
			int num2 = -1;
			int num3 = (int)((float)m_LoadedBlendingCells.size * (1f - turnoverRate));
			BlendingCellInfo blendingCellInfo = ((num3 < m_LoadedBlendingCells.size) ? m_LoadedBlendingCells[num3] : null);
			while (m_TempBlendingCellToLoadList.size < num && m_LoadedBlendingCells.size - m_TempBlendingCellToUnloadList.size != 0)
			{
				BlendingCellInfo blendingCellInfo2 = m_LoadedBlendingCells[m_LoadedBlendingCells.size - m_TempBlendingCellToUnloadList.size - 1];
				BlendingCellInfo blendingCellInfo3 = m_ToBeLoadedBlendingCells[m_TempBlendingCellToLoadList.size];
				if (blendingCellInfo3.streamingScore >= (blendingCellInfo ?? blendingCellInfo2).streamingScore)
				{
					if (blendingCellInfo == null)
					{
						break;
					}
					if (num2 == -1)
					{
						num2 = FindWorstBlendingCellToBeLoaded();
					}
					blendingCellInfo3 = m_ToBeLoadedBlendingCells[num2];
					if (blendingCellInfo3.IsUpToDate())
					{
						break;
					}
				}
				UnloadBlendingCell(blendingCellInfo2, m_TempBlendingCellToUnloadList);
				if (TryLoadBlendingCell(blendingCellInfo3, m_TempBlendingCellToLoadList) && num2 != -1)
				{
					m_ToBeLoadedBlendingCells[num2] = m_ToBeLoadedBlendingCells[m_TempBlendingCellToLoadList.size - 1];
					m_ToBeLoadedBlendingCells[m_TempBlendingCellToLoadList.size - 1] = blendingCellInfo3;
					if (++num2 >= m_ToBeLoadedBlendingCells.size)
					{
						num2 = m_TempBlendingCellToLoadList.size;
					}
				}
			}
			m_LoadedBlendingCells.RemoveRange(m_LoadedBlendingCells.size - m_TempBlendingCellToUnloadList.size, m_TempBlendingCellToUnloadList.size);
		}
		m_ToBeLoadedBlendingCells.RemoveRange(0, m_TempBlendingCellToLoadList.size);
		m_LoadedBlendingCells.AddRange(m_TempBlendingCellToLoadList);
		m_TempBlendingCellToLoadList.Clear();
		m_ToBeLoadedBlendingCells.AddRange(m_TempBlendingCellToUnloadList);
		m_TempBlendingCellToUnloadList.Clear();
		if (m_LoadedBlendingCells.size != 0)
		{
			float num4 = scenarioBlendingFactor;
			int num5 = Mathf.Min(numberOfCellsBlendedPerFrame, m_LoadedBlendingCells.size);
			for (int i = 0; i < num5; i++)
			{
				m_LoadedBlendingCells[i].blendingFactor = num4;
				m_BlendingPool.BlendChunks(m_LoadedBlendingCells[i], m_Pool);
			}
			m_BlendingPool.PerformBlending(cmd, num4, m_Pool);
		}
		if (m_ToBeLoadedBlendingCells.size == 0)
		{
			m_HasRemainingCellsToBlend = false;
		}
	}
}
