using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

[ExecuteAlways]
[AddComponentMenu("")]
public class ProbeVolumePerSceneData : MonoBehaviour, ISerializationCallbackReceiver
{
	[Serializable]
	internal struct PerScenarioData
	{
		public int sceneHash;

		public TextAsset cellDataAsset;

		public TextAsset cellOptionalDataAsset;
	}

	[Serializable]
	private struct SerializablePerScenarioDataItem
	{
		public string scenario;

		public PerScenarioData data;
	}

	[SerializeField]
	internal ProbeVolumeAsset asset;

	[SerializeField]
	internal TextAsset cellSharedDataAsset;

	[SerializeField]
	internal TextAsset cellSupportDataAsset;

	[SerializeField]
	private List<SerializablePerScenarioDataItem> serializedScenarios = new List<SerializablePerScenarioDataItem>();

	internal Dictionary<string, PerScenarioData> scenarios = new Dictionary<string, PerScenarioData>();

	private bool assetLoaded;

	private string activeScenario;

	private string otherScenario;

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		scenarios.Clear();
		foreach (SerializablePerScenarioDataItem serializedScenario in serializedScenarios)
		{
			scenarios.Add(serializedScenario.scenario, serializedScenario.data);
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		serializedScenarios.Clear();
		foreach (KeyValuePair<string, PerScenarioData> scenario in scenarios)
		{
			serializedScenarios.Add(new SerializablePerScenarioDataItem
			{
				scenario = scenario.Key,
				data = scenario.Value
			});
		}
	}

	internal void Clear()
	{
		QueueAssetRemoval();
		scenarios.Clear();
	}

	internal void RemoveScenario(string scenario)
	{
		scenarios.Remove(scenario);
	}

	internal void RenameScenario(string scenario, string newName)
	{
		if (scenarios.TryGetValue(scenario, out var value))
		{
			scenarios.Remove(scenario);
			scenarios.Add(newName, value);
		}
	}

	internal bool ResolveCells()
	{
		if (ResolveSharedCellData())
		{
			return ResolvePerScenarioCellData();
		}
		return false;
	}

	internal bool ResolveSharedCellData()
	{
		if (asset != null)
		{
			return asset.ResolveSharedCellData(cellSharedDataAsset, cellSupportDataAsset);
		}
		return false;
	}

	private bool ResolvePerScenarioCellData()
	{
		int num = 0;
		int num2 = ((otherScenario == null) ? 1 : 2);
		if (activeScenario != null && scenarios.TryGetValue(activeScenario, out var value) && asset.ResolvePerScenarioCellData(value.cellDataAsset, value.cellOptionalDataAsset, 0))
		{
			num++;
		}
		if (otherScenario != null && scenarios.TryGetValue(otherScenario, out var value2) && asset.ResolvePerScenarioCellData(value2.cellDataAsset, value2.cellOptionalDataAsset, num))
		{
			num++;
		}
		for (int i = 0; i < asset.cells.Length; i++)
		{
			asset.cells[i].hasTwoScenarios = num == 2;
		}
		return num == num2;
	}

	internal void QueueAssetLoading()
	{
		if (!(asset == null) && !asset.IsInvalid() && ResolvePerScenarioCellData())
		{
			ProbeReferenceVolume.instance.AddPendingAssetLoading(asset);
			assetLoaded = true;
		}
	}

	internal void QueueAssetRemoval()
	{
		if (asset != null)
		{
			ProbeReferenceVolume.instance.AddPendingAssetRemoval(asset);
		}
		assetLoaded = false;
	}

	private void OnEnable()
	{
		ProbeReferenceVolume.instance.RegisterPerSceneData(this);
		if (ProbeReferenceVolume.instance.sceneData != null)
		{
			Initialize();
		}
	}

	private void OnDisable()
	{
		QueueAssetRemoval();
		activeScenario = (otherScenario = null);
		ProbeReferenceVolume.instance.UnregisterPerSceneData(this);
	}

	internal void Initialize()
	{
		ResolveSharedCellData();
		QueueAssetRemoval();
		activeScenario = ProbeReferenceVolume.instance.sceneData.lightingScenario;
		otherScenario = ProbeReferenceVolume.instance.sceneData.otherScenario;
		QueueAssetLoading();
	}

	internal void UpdateActiveScenario(string activeScenario, string otherScenario)
	{
		if (!(asset == null))
		{
			this.activeScenario = activeScenario;
			this.otherScenario = otherScenario;
			if (!assetLoaded)
			{
				QueueAssetLoading();
			}
			else if (!ResolvePerScenarioCellData())
			{
				QueueAssetRemoval();
			}
		}
	}
}
