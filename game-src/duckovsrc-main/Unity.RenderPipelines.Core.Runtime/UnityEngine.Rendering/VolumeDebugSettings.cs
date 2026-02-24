using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering;

public abstract class VolumeDebugSettings<T> : IVolumeDebugSettings2, IVolumeDebugSettings where T : MonoBehaviour, IAdditionalData
{
	private Camera m_SelectedCamera;

	protected int m_SelectedCameraIndex = -1;

	private Camera[] m_CamerasArray;

	private List<Camera> m_Cameras = new List<Camera>();

	private static List<(string, Type)> s_ComponentPathAndType;

	private float[] weights;

	private Volume[] volumes;

	private VolumeParameter[,] savedStates;

	private static List<Type> s_ComponentTypes;

	public int selectedComponent { get; set; }

	public Camera selectedCamera => m_SelectedCamera;

	public int selectedCameraIndex
	{
		get
		{
			return m_SelectedCameraIndex;
		}
		set
		{
			m_SelectedCameraIndex = value;
			int num = cameras.Count();
			if (num != 0)
			{
				m_SelectedCamera = ((m_SelectedCameraIndex < 0 || m_SelectedCameraIndex >= num) ? cameras.First() : cameras.ElementAt(m_SelectedCameraIndex));
			}
			else
			{
				m_SelectedCamera = null;
			}
		}
	}

	public IEnumerable<Camera> cameras
	{
		get
		{
			m_Cameras.Clear();
			if (m_CamerasArray == null || m_CamerasArray.Length != Camera.allCamerasCount)
			{
				m_CamerasArray = new Camera[Camera.allCamerasCount];
			}
			Camera.GetAllCameras(m_CamerasArray);
			Camera[] camerasArray = m_CamerasArray;
			foreach (Camera camera in camerasArray)
			{
				if (!(camera == null) && camera.cameraType != CameraType.Preview && camera.cameraType != CameraType.Reflection && camera.TryGetComponent<T>(out var _))
				{
					m_Cameras.Add(camera);
				}
			}
			return m_Cameras;
		}
	}

	public abstract VolumeStack selectedCameraVolumeStack { get; }

	public abstract LayerMask selectedCameraLayerMask { get; }

	public abstract Vector3 selectedCameraPosition { get; }

	public Type selectedComponentType
	{
		get
		{
			return volumeComponentsPathAndType[selectedComponent - 1].Item2;
		}
		set
		{
			int num = volumeComponentsPathAndType.FindIndex(((string, Type) t) => t.Item2 == value);
			if (num != -1)
			{
				selectedComponent = num + 1;
			}
		}
	}

	public List<(string, Type)> volumeComponentsPathAndType => s_ComponentPathAndType ?? (s_ComponentPathAndType = VolumeManager.GetSupportedVolumeComponents(targetRenderPipeline));

	public abstract Type targetRenderPipeline { get; }

	[Obsolete("Please use volumeComponentsPathAndType instead, and get the second element of the tuple", false)]
	public static List<Type> componentTypes
	{
		get
		{
			if (s_ComponentTypes == null)
			{
				s_ComponentTypes = (from t in VolumeManager.instance.baseComponentTypeArray
					where !t.IsDefined(typeof(HideInInspector), inherit: false)
					where !t.IsDefined(typeof(ObsoleteAttribute), inherit: false)
					orderby ComponentDisplayName(t)
					select t).ToList();
			}
			return s_ComponentTypes;
		}
	}

	[Obsolete("Cameras are auto registered/unregistered, use property cameras", false)]
	protected static List<T> additionalCameraDatas { get; private set; } = new List<T>();

	internal VolumeParameter GetParameter(VolumeComponent component, FieldInfo field)
	{
		return (VolumeParameter)field.GetValue(component);
	}

	internal VolumeParameter GetParameter(FieldInfo field)
	{
		VolumeStack volumeStack = selectedCameraVolumeStack;
		if (volumeStack != null)
		{
			return GetParameter(volumeStack.GetComponent(selectedComponentType), field);
		}
		return null;
	}

	internal VolumeParameter GetParameter(Volume volume, FieldInfo field)
	{
		if (!(volume.HasInstantiatedProfile() ? volume.profile : volume.sharedProfile).TryGet<VolumeComponent>(selectedComponentType, out var component))
		{
			return null;
		}
		VolumeParameter parameter = GetParameter(component, field);
		if (!parameter.overrideState)
		{
			return null;
		}
		return parameter;
	}

	private float ComputeWeight(Volume volume, Vector3 triggerPos)
	{
		if (volume == null)
		{
			return 0f;
		}
		VolumeProfile volumeProfile = (volume.HasInstantiatedProfile() ? volume.profile : volume.sharedProfile);
		if (!volume.gameObject.activeInHierarchy)
		{
			return 0f;
		}
		if (!volume.enabled || volumeProfile == null || volume.weight <= 0f)
		{
			return 0f;
		}
		if (!volumeProfile.TryGet<VolumeComponent>(selectedComponentType, out var component))
		{
			return 0f;
		}
		if (!component.active)
		{
			return 0f;
		}
		float num = Mathf.Clamp01(volume.weight);
		if (!volume.isGlobal)
		{
			Collider[] components = volume.GetComponents<Collider>();
			float num2 = float.PositiveInfinity;
			Collider[] array = components;
			foreach (Collider collider in array)
			{
				if (collider.enabled)
				{
					float sqrMagnitude = (collider.ClosestPoint(triggerPos) - triggerPos).sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						num2 = sqrMagnitude;
					}
				}
			}
			float num3 = volume.blendDistance * volume.blendDistance;
			if (num2 > num3)
			{
				num = 0f;
			}
			else if (num3 > 0f)
			{
				num *= 1f - num2 / num3;
			}
		}
		return num;
	}

	public Volume[] GetVolumes()
	{
		return (from v in VolumeManager.instance.GetVolumes(selectedCameraLayerMask)
			where v.sharedProfile != null
			select v).Reverse().ToArray();
	}

	private VolumeParameter[,] GetStates()
	{
		FieldInfo[] array = (from t in selectedComponentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where t.FieldType.IsSubclassOf(typeof(VolumeParameter))
			select t).ToArray();
		VolumeParameter[,] array2 = new VolumeParameter[volumes.Length, array.Length];
		for (int num = 0; num < volumes.Length; num++)
		{
			if ((volumes[num].HasInstantiatedProfile() ? volumes[num].profile : volumes[num].sharedProfile).TryGet<VolumeComponent>(selectedComponentType, out var component))
			{
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					VolumeParameter parameter = GetParameter(component, array[num2]);
					array2[num, num2] = (parameter.overrideState ? parameter : null);
				}
			}
		}
		return array2;
	}

	private bool ChangedStates(VolumeParameter[,] newStates)
	{
		if (savedStates.GetLength(1) != newStates.GetLength(1))
		{
			return true;
		}
		for (int i = 0; i < savedStates.GetLength(0); i++)
		{
			for (int j = 0; j < savedStates.GetLength(1); j++)
			{
				if (savedStates[i, j] == null != (newStates[i, j] == null))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool RefreshVolumes(Volume[] newVolumes)
	{
		bool result = false;
		if (volumes == null || !newVolumes.SequenceEqual(volumes))
		{
			volumes = (Volume[])newVolumes.Clone();
			savedStates = GetStates();
			result = true;
		}
		else
		{
			VolumeParameter[,] states = GetStates();
			if (savedStates == null || ChangedStates(states))
			{
				savedStates = states;
				result = true;
			}
		}
		Vector3 triggerPos = selectedCameraPosition;
		weights = new float[volumes.Length];
		for (int i = 0; i < volumes.Length; i++)
		{
			weights[i] = ComputeWeight(volumes[i], triggerPos);
		}
		return result;
	}

	public float GetVolumeWeight(Volume volume)
	{
		if (weights == null)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < volumes.Length; i++)
		{
			num2 = weights[i];
			num2 *= 1f - num;
			num += num2;
			if (volumes[i] == volume)
			{
				return num2;
			}
		}
		return 0f;
	}

	public bool VolumeHasInfluence(Volume volume)
	{
		if (weights == null)
		{
			return false;
		}
		int num = Array.IndexOf(volumes, volume);
		if (num == -1)
		{
			return false;
		}
		return weights[num] != 0f;
	}

	[Obsolete("Please use componentPathAndType instead, and get the first element of the tuple", false)]
	public static string ComponentDisplayName(Type component)
	{
		if (component.GetCustomAttribute(typeof(VolumeComponentMenuForRenderPipeline), inherit: false) is VolumeComponentMenuForRenderPipeline volumeComponentMenuForRenderPipeline)
		{
			return volumeComponentMenuForRenderPipeline.menu;
		}
		if (component.GetCustomAttribute(typeof(VolumeComponentMenu), inherit: false) is VolumeComponentMenuForRenderPipeline volumeComponentMenuForRenderPipeline2)
		{
			return volumeComponentMenuForRenderPipeline2.menu;
		}
		return component.Name;
	}

	[Obsolete("Cameras are auto registered/unregistered", false)]
	public static void RegisterCamera(T additionalCamera)
	{
		if (!additionalCameraDatas.Contains(additionalCamera))
		{
			additionalCameraDatas.Add(additionalCamera);
		}
	}

	[Obsolete("Cameras are auto registered/unregistered", false)]
	public static void UnRegisterCamera(T additionalCamera)
	{
		if (additionalCameraDatas.Contains(additionalCamera))
		{
			additionalCameraDatas.Remove(additionalCamera);
		}
	}
}
