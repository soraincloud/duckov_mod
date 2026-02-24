using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering;

public sealed class VolumeManager
{
	private static readonly Lazy<VolumeManager> s_Instance = new Lazy<VolumeManager>(() => new VolumeManager());

	private static readonly Dictionary<Type, List<(string, Type)>> s_SupportedVolumeComponentsForRenderPipeline = new Dictionary<Type, List<(string, Type)>>();

	private const int k_MaxLayerCount = 32;

	private readonly Dictionary<int, List<Volume>> m_SortedVolumes;

	private readonly List<Volume> m_Volumes;

	private readonly Dictionary<int, bool> m_SortNeeded;

	private readonly List<VolumeComponent> m_ComponentsDefaultState;

	private readonly List<Collider> m_TempColliders;

	private VolumeStack m_DefaultStack;

	public static VolumeManager instance => s_Instance.Value;

	public VolumeStack stack { get; set; }

	[Obsolete("Please use baseComponentTypeArray instead.")]
	public IEnumerable<Type> baseComponentTypes
	{
		get
		{
			return baseComponentTypeArray;
		}
		private set
		{
			baseComponentTypeArray = value.ToArray();
		}
	}

	public Type[] baseComponentTypeArray { get; private set; }

	internal static List<(string, Type)> GetSupportedVolumeComponents(Type currentPipelineType)
	{
		if (s_SupportedVolumeComponentsForRenderPipeline.TryGetValue(currentPipelineType, out var value))
		{
			return value;
		}
		value = FilterVolumeComponentTypes(instance.baseComponentTypeArray, currentPipelineType);
		s_SupportedVolumeComponentsForRenderPipeline[currentPipelineType] = value;
		return value;
	}

	private static List<(string, Type)> FilterVolumeComponentTypes(Type[] types, Type currentPipelineType)
	{
		List<(string, Type)> list = new List<(string, Type)>();
		foreach (Type type in types)
		{
			string text = string.Empty;
			object[] customAttributes = type.GetCustomAttributes(inherit: false);
			bool flag = false;
			object[] array = customAttributes;
			foreach (object obj in array)
			{
				if (!(obj is VolumeComponentMenu volumeComponentMenu))
				{
					if (obj is HideInInspector || obj is ObsoleteAttribute)
					{
						flag = true;
					}
					continue;
				}
				text = volumeComponentMenu.menu;
				if (volumeComponentMenu is VolumeComponentMenuForRenderPipeline volumeComponentMenuForRenderPipeline)
				{
					flag |= !volumeComponentMenuForRenderPipeline.pipelineTypes.Contains(currentPipelineType);
				}
			}
			if (!flag)
			{
				if (string.IsNullOrEmpty(text))
				{
					text = type.Name;
				}
				list.Add((text, type));
			}
		}
		return list.OrderBy(((string, Type) tuple) => tuple.Item1).ToList();
	}

	internal VolumeComponent GetDefaultVolumeComponent(Type volumeComponentType)
	{
		foreach (VolumeComponent item in m_ComponentsDefaultState)
		{
			if (item.GetType() == volumeComponentType)
			{
				return item;
			}
		}
		return null;
	}

	private VolumeManager()
	{
		m_SortedVolumes = new Dictionary<int, List<Volume>>();
		m_Volumes = new List<Volume>();
		m_SortNeeded = new Dictionary<int, bool>();
		m_TempColliders = new List<Collider>(8);
		m_ComponentsDefaultState = new List<VolumeComponent>();
		ReloadBaseTypes();
		m_DefaultStack = CreateStack();
		stack = m_DefaultStack;
	}

	public VolumeStack CreateStack()
	{
		VolumeStack volumeStack = new VolumeStack();
		volumeStack.Reload(m_ComponentsDefaultState);
		return volumeStack;
	}

	public void ResetMainStack()
	{
		stack = m_DefaultStack;
	}

	public void DestroyStack(VolumeStack stack)
	{
		stack.Dispose();
	}

	private void ReloadBaseTypes()
	{
		m_ComponentsDefaultState.Clear();
		baseComponentTypeArray = (from t in CoreUtils.GetAllTypesDerivedFrom<VolumeComponent>()
			where !t.IsAbstract
			select t).ToArray();
		BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		Type[] array = baseComponentTypeArray;
		foreach (Type obj in array)
		{
			obj.GetMethod("Init", bindingAttr)?.Invoke(null, null);
			VolumeComponent item = (VolumeComponent)ScriptableObject.CreateInstance(obj);
			m_ComponentsDefaultState.Add(item);
		}
	}

	public void Register(Volume volume, int layer)
	{
		if (m_Volumes.Contains(volume))
		{
			return;
		}
		m_Volumes.Add(volume);
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			if ((sortedVolume.Key & (1 << layer)) != 0 && !sortedVolume.Value.Contains(volume))
			{
				sortedVolume.Value.Add(volume);
			}
		}
		SetLayerDirty(layer);
	}

	public void Unregister(Volume volume, int layer)
	{
		if (!m_Volumes.Remove(volume))
		{
			return;
		}
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			if ((sortedVolume.Key & (1 << layer)) != 0)
			{
				sortedVolume.Value.Remove(volume);
			}
		}
	}

	public bool IsComponentActiveInMask<T>(LayerMask layerMask) where T : VolumeComponent
	{
		int value = layerMask.value;
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			if (sortedVolume.Key != value)
			{
				continue;
			}
			foreach (Volume item in sortedVolume.Value)
			{
				if (item.enabled && !(item.profileRef == null) && item.profileRef.TryGet<T>(out var component) && component.active)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal void SetLayerDirty(int layer)
	{
		foreach (KeyValuePair<int, List<Volume>> sortedVolume in m_SortedVolumes)
		{
			int key = sortedVolume.Key;
			if ((key & (1 << layer)) != 0)
			{
				m_SortNeeded[key] = true;
			}
		}
	}

	internal void UpdateVolumeLayer(Volume volume, int prevLayer, int newLayer)
	{
		Unregister(volume, prevLayer);
		Register(volume, newLayer);
	}

	private void OverrideData(VolumeStack stack, List<VolumeComponent> components, float interpFactor)
	{
		int count = components.Count;
		for (int i = 0; i < count; i++)
		{
			VolumeComponent volumeComponent = components[i];
			if (volumeComponent.active)
			{
				VolumeComponent component = stack.GetComponent(volumeComponent.GetType());
				volumeComponent.Override(component, interpFactor);
			}
		}
	}

	internal void ReplaceData(VolumeStack stack)
	{
		(VolumeParameter, VolumeParameter)[] defaultParameters = stack.defaultParameters;
		int num = defaultParameters.Length;
		for (int i = 0; i < num; i++)
		{
			(VolumeParameter, VolumeParameter) tuple = defaultParameters[i];
			VolumeParameter item = tuple.Item1;
			item.overrideState = false;
			item.SetValue(tuple.Item2);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void CheckBaseTypes()
	{
		if (m_ComponentsDefaultState == null || (m_ComponentsDefaultState.Count > 0 && m_ComponentsDefaultState[0] == null))
		{
			ReloadBaseTypes();
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void CheckStack(VolumeStack stack)
	{
		Dictionary<Type, VolumeComponent> components = stack.components;
		if (components == null)
		{
			stack.Reload(m_ComponentsDefaultState);
			return;
		}
		foreach (KeyValuePair<Type, VolumeComponent> item in components)
		{
			if (item.Key == null || item.Value == null)
			{
				stack.Reload(m_ComponentsDefaultState);
				break;
			}
		}
	}

	private bool CheckUpdateRequired(VolumeStack stack)
	{
		if (m_Volumes.Count == 0)
		{
			if (stack.requiresReset)
			{
				stack.requiresReset = false;
				return true;
			}
			return false;
		}
		stack.requiresReset = true;
		return true;
	}

	public void Update(Transform trigger, LayerMask layerMask)
	{
		Update(stack, trigger, layerMask);
	}

	public void Update(VolumeStack stack, Transform trigger, LayerMask layerMask)
	{
		if (!CheckUpdateRequired(stack))
		{
			return;
		}
		ReplaceData(stack);
		bool flag = trigger == null;
		Vector3 vector = (flag ? Vector3.zero : trigger.position);
		List<Volume> list = GrabVolumes(layerMask);
		Camera component = null;
		if (!flag)
		{
			trigger.TryGetComponent<Camera>(out component);
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Volume volume = list[i];
			if (volume == null || !volume.enabled || volume.profileRef == null || volume.weight <= 0f)
			{
				continue;
			}
			if (volume.isGlobal)
			{
				OverrideData(stack, volume.profileRef.components, Mathf.Clamp01(volume.weight));
			}
			else
			{
				if (flag)
				{
					continue;
				}
				List<Collider> tempColliders = m_TempColliders;
				volume.GetComponents(tempColliders);
				if (tempColliders.Count == 0)
				{
					continue;
				}
				float num = float.PositiveInfinity;
				int count2 = tempColliders.Count;
				for (int j = 0; j < count2; j++)
				{
					Collider collider = tempColliders[j];
					if (collider.enabled)
					{
						float sqrMagnitude = (collider.ClosestPoint(vector) - vector).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
						}
					}
				}
				tempColliders.Clear();
				float num2 = volume.blendDistance * volume.blendDistance;
				if (!(num > num2))
				{
					float num3 = 1f;
					if (num2 > 0f)
					{
						num3 = 1f - num / num2;
					}
					OverrideData(stack, volume.profileRef.components, num3 * Mathf.Clamp01(volume.weight));
				}
			}
		}
	}

	public Volume[] GetVolumes(LayerMask layerMask)
	{
		List<Volume> list = GrabVolumes(layerMask);
		list.RemoveAll((Volume v) => v == null);
		return list.ToArray();
	}

	private List<Volume> GrabVolumes(LayerMask mask)
	{
		if (!m_SortedVolumes.TryGetValue(mask, out var value))
		{
			value = new List<Volume>();
			int count = m_Volumes.Count;
			for (int i = 0; i < count; i++)
			{
				Volume volume = m_Volumes[i];
				if (((int)mask & (1 << volume.gameObject.layer)) != 0)
				{
					value.Add(volume);
					m_SortNeeded[mask] = true;
				}
			}
			m_SortedVolumes.Add(mask, value);
		}
		if (m_SortNeeded.TryGetValue(mask, out var value2) && value2)
		{
			m_SortNeeded[mask] = false;
			SortByPriority(value);
		}
		return value;
	}

	private static void SortByPriority(List<Volume> volumes)
	{
		for (int i = 1; i < volumes.Count; i++)
		{
			Volume volume = volumes[i];
			int num = i - 1;
			while (num >= 0 && volumes[num].priority > volume.priority)
			{
				volumes[num + 1] = volumes[num];
				num--;
			}
			volumes[num + 1] = volume;
		}
	}

	private static bool IsVolumeRenderedByCamera(Volume volume, Camera camera)
	{
		return true;
	}
}
