using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace UnityEngine.Rendering;

[Serializable]
public class ProbeVolumeSceneData : ISerializationCallbackReceiver
{
	[Serializable]
	private struct SerializableBoundItem
	{
		[SerializeField]
		public string sceneGUID;

		[SerializeField]
		public Bounds bounds;
	}

	[Serializable]
	private struct SerializableHasPVItem
	{
		[SerializeField]
		public string sceneGUID;

		[SerializeField]
		public bool hasProbeVolumes;
	}

	[Serializable]
	private struct SerializablePVProfile
	{
		[SerializeField]
		public string sceneGUID;

		[SerializeField]
		public ProbeReferenceVolumeProfile profile;
	}

	[Serializable]
	private struct SerializablePVBakeSettings
	{
		public string sceneGUID;

		public ProbeVolumeBakingProcessSettings settings;
	}

	[Serializable]
	internal class BakingSet
	{
		public string name;

		public List<string> sceneGUIDs = new List<string>();

		public ProbeVolumeBakingProcessSettings settings;

		public ProbeReferenceVolumeProfile profile;

		public List<string> lightingScenarios = new List<string>();

		internal string CreateScenario(string name)
		{
			if (lightingScenarios.Contains(name))
			{
				int num = 1;
				string text;
				do
				{
					text = $"{name} ({num++})";
				}
				while (lightingScenarios.Contains(text));
				name = text;
			}
			lightingScenarios.Add(name);
			return name;
		}

		internal bool RemoveScenario(string name)
		{
			return lightingScenarios.Remove(name);
		}
	}

	private static PropertyInfo s_SceneGUID = typeof(Scene).GetProperty("guid", BindingFlags.Instance | BindingFlags.NonPublic);

	[SerializeField]
	private List<SerializableBoundItem> serializedBounds;

	[SerializeField]
	private List<SerializableHasPVItem> serializedHasVolumes;

	[SerializeField]
	private List<SerializablePVProfile> serializedProfiles;

	[SerializeField]
	private List<SerializablePVBakeSettings> serializedBakeSettings;

	[SerializeField]
	private List<BakingSet> serializedBakingSets;

	internal Object parentAsset;

	internal string parentSceneDataPropertyName;

	public Dictionary<string, Bounds> sceneBounds;

	internal Dictionary<string, bool> hasProbeVolumes;

	internal Dictionary<string, ProbeReferenceVolumeProfile> sceneProfiles;

	internal Dictionary<string, ProbeVolumeBakingProcessSettings> sceneBakingSettings;

	internal List<BakingSet> bakingSets;

	[SerializeField]
	private string m_LightingScenario = ProbeReferenceVolume.defaultLightingScenario;

	private string m_OtherScenario;

	private float m_ScenarioBlendingFactor;

	internal string lightingScenario => m_LightingScenario;

	internal string otherScenario => m_OtherScenario;

	internal float scenarioBlendingFactor => m_ScenarioBlendingFactor;

	internal static string GetSceneGUID(Scene scene)
	{
		return (string)s_SceneGUID.GetValue(scene);
	}

	internal void SetActiveScenario(string scenario)
	{
		if (m_LightingScenario == scenario && m_ScenarioBlendingFactor == 0f)
		{
			return;
		}
		m_LightingScenario = scenario;
		m_OtherScenario = null;
		m_ScenarioBlendingFactor = 0f;
		foreach (ProbeVolumePerSceneData perSceneData in ProbeReferenceVolume.instance.perSceneDataList)
		{
			perSceneData.UpdateActiveScenario(m_LightingScenario, m_OtherScenario);
		}
		if (ProbeReferenceVolume.instance.enableScenarioBlending)
		{
			ProbeReferenceVolume.instance.ScenarioBlendingChanged(scenarioChanged: true);
		}
		else
		{
			ProbeReferenceVolume.instance.UnloadAllCells();
		}
	}

	internal void BlendLightingScenario(string otherScenario, float blendingFactor)
	{
		if (!ProbeReferenceVolume.instance.enableScenarioBlending)
		{
			if (!ProbeBrickBlendingPool.isSupported)
			{
				Debug.LogError("Blending between lighting scenarios is not supported by this render pipeline.");
			}
			else
			{
				Debug.LogError("Blending between lighting scenarios is disabled in the render pipeline settings.");
			}
			return;
		}
		blendingFactor = Mathf.Clamp01(blendingFactor);
		if (otherScenario == m_LightingScenario || string.IsNullOrEmpty(otherScenario))
		{
			otherScenario = null;
		}
		if (otherScenario == null)
		{
			blendingFactor = 0f;
		}
		if (otherScenario == m_OtherScenario && Mathf.Approximately(blendingFactor, m_ScenarioBlendingFactor))
		{
			return;
		}
		bool flag = otherScenario != m_OtherScenario;
		m_OtherScenario = otherScenario;
		m_ScenarioBlendingFactor = blendingFactor;
		if (flag)
		{
			foreach (ProbeVolumePerSceneData perSceneData in ProbeReferenceVolume.instance.perSceneDataList)
			{
				perSceneData.UpdateActiveScenario(m_LightingScenario, m_OtherScenario);
			}
		}
		ProbeReferenceVolume.instance.ScenarioBlendingChanged(flag);
	}

	public ProbeVolumeSceneData(Object parentAsset, string parentSceneDataPropertyName)
	{
		this.parentAsset = parentAsset;
		this.parentSceneDataPropertyName = parentSceneDataPropertyName;
		sceneBounds = new Dictionary<string, Bounds>();
		hasProbeVolumes = new Dictionary<string, bool>();
		sceneProfiles = new Dictionary<string, ProbeReferenceVolumeProfile>();
		sceneBakingSettings = new Dictionary<string, ProbeVolumeBakingProcessSettings>();
		bakingSets = new List<BakingSet>();
		serializedBounds = new List<SerializableBoundItem>();
		serializedHasVolumes = new List<SerializableHasPVItem>();
		serializedProfiles = new List<SerializablePVProfile>();
		serializedBakeSettings = new List<SerializablePVBakeSettings>();
		UpdateBakingSets();
	}

	public void SetParentObject(Object parent, string parentSceneDataPropertyName)
	{
		parentAsset = parent;
		this.parentSceneDataPropertyName = parentSceneDataPropertyName;
		UpdateBakingSets();
	}

	public void OnAfterDeserialize()
	{
		if (serializedBounds == null || serializedHasVolumes == null || serializedProfiles == null || serializedBakeSettings == null)
		{
			return;
		}
		sceneBounds = new Dictionary<string, Bounds>();
		hasProbeVolumes = new Dictionary<string, bool>();
		sceneProfiles = new Dictionary<string, ProbeReferenceVolumeProfile>();
		sceneBakingSettings = new Dictionary<string, ProbeVolumeBakingProcessSettings>();
		bakingSets = new List<BakingSet>();
		foreach (SerializableBoundItem serializedBound in serializedBounds)
		{
			sceneBounds.Add(serializedBound.sceneGUID, serializedBound.bounds);
		}
		foreach (SerializableHasPVItem serializedHasVolume in serializedHasVolumes)
		{
			hasProbeVolumes.Add(serializedHasVolume.sceneGUID, serializedHasVolume.hasProbeVolumes);
		}
		foreach (SerializablePVProfile serializedProfile in serializedProfiles)
		{
			sceneProfiles.Add(serializedProfile.sceneGUID, serializedProfile.profile);
		}
		foreach (SerializablePVBakeSettings serializedBakeSetting in serializedBakeSettings)
		{
			sceneBakingSettings.Add(serializedBakeSetting.sceneGUID, serializedBakeSetting.settings);
		}
		if (string.IsNullOrEmpty(m_LightingScenario))
		{
			m_LightingScenario = ProbeReferenceVolume.defaultLightingScenario;
		}
		foreach (BakingSet serializedBakingSet in serializedBakingSets)
		{
			serializedBakingSet.settings.Upgrade();
			bakingSets.Add(serializedBakingSet);
		}
		if (m_OtherScenario == "")
		{
			m_OtherScenario = null;
		}
	}

	private void UpdateBakingSets()
	{
		foreach (BakingSet serializedBakingSet in serializedBakingSets)
		{
			if (serializedBakingSet.profile == null)
			{
				InitializeBakingSet(serializedBakingSet, serializedBakingSet.name);
			}
			if (serializedBakingSet.lightingScenarios.Count == 0)
			{
				InitializeScenarios(serializedBakingSet);
			}
		}
		SyncBakingSetSettings();
	}

	public void OnBeforeSerialize()
	{
		if (sceneBounds == null || hasProbeVolumes == null || sceneBakingSettings == null || sceneProfiles == null || serializedBounds == null || serializedHasVolumes == null || serializedBakeSettings == null || serializedProfiles == null || serializedBakingSets == null)
		{
			return;
		}
		serializedBounds.Clear();
		serializedHasVolumes.Clear();
		serializedProfiles.Clear();
		serializedBakeSettings.Clear();
		serializedBakingSets.Clear();
		SerializableBoundItem item = default(SerializableBoundItem);
		foreach (string key5 in sceneBounds.Keys)
		{
			string key = (item.sceneGUID = key5);
			item.bounds = sceneBounds[key];
			serializedBounds.Add(item);
		}
		SerializableHasPVItem item2 = default(SerializableHasPVItem);
		foreach (string key6 in hasProbeVolumes.Keys)
		{
			string key2 = (item2.sceneGUID = key6);
			item2.hasProbeVolumes = hasProbeVolumes[key2];
			serializedHasVolumes.Add(item2);
		}
		SerializablePVBakeSettings item3 = default(SerializablePVBakeSettings);
		foreach (string key7 in sceneBakingSettings.Keys)
		{
			string key3 = (item3.sceneGUID = key7);
			item3.settings = sceneBakingSettings[key3];
			serializedBakeSettings.Add(item3);
		}
		SerializablePVProfile item4 = default(SerializablePVProfile);
		foreach (string key8 in sceneProfiles.Keys)
		{
			string key4 = (item4.sceneGUID = key8);
			item4.profile = sceneProfiles[key4];
			serializedProfiles.Add(item4);
		}
		foreach (BakingSet bakingSet in bakingSets)
		{
			serializedBakingSets.Add(bakingSet);
		}
	}

	internal BakingSet CreateNewBakingSet(string name)
	{
		BakingSet bakingSet = new BakingSet();
		InitializeBakingSet(bakingSet, name);
		bakingSets.Add(bakingSet);
		return bakingSet;
	}

	private void InitializeBakingSet(BakingSet set, string name)
	{
		ProbeReferenceVolumeProfile probeReferenceVolumeProfile = ScriptableObject.CreateInstance<ProbeReferenceVolumeProfile>();
		set.name = probeReferenceVolumeProfile.name;
		set.profile = probeReferenceVolumeProfile;
		set.settings = ProbeVolumeBakingProcessSettings.Default;
		InitializeScenarios(set);
	}

	private void InitializeScenarios(BakingSet set)
	{
		set.lightingScenarios = new List<string> { ProbeReferenceVolume.defaultLightingScenario };
	}

	internal void SyncBakingSetSettings()
	{
		foreach (BakingSet bakingSet in bakingSets)
		{
			foreach (string sceneGUID in bakingSet.sceneGUIDs)
			{
				sceneBakingSettings[sceneGUID] = bakingSet.settings;
				sceneProfiles[sceneGUID] = bakingSet.profile;
			}
		}
	}
}
