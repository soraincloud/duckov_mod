using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public abstract class DebugDisplaySettings<T> : IDebugDisplaySettings, IDebugDisplaySettingsQuery where T : IDebugDisplaySettings, new()
{
	protected readonly HashSet<IDebugDisplaySettingsData> m_Settings = new HashSet<IDebugDisplaySettingsData>();

	private static readonly Lazy<T> s_Instance = new Lazy<T>(delegate
	{
		T result = new T();
		result.Reset();
		return result;
	});

	public static T Instance => s_Instance.Value;

	public virtual bool AreAnySettingsActive
	{
		get
		{
			foreach (IDebugDisplaySettingsData setting in m_Settings)
			{
				if (setting.AreAnySettingsActive)
				{
					return true;
				}
			}
			return false;
		}
	}

	public virtual bool IsPostProcessingAllowed
	{
		get
		{
			bool flag = true;
			foreach (IDebugDisplaySettingsData setting in m_Settings)
			{
				flag &= setting.IsPostProcessingAllowed;
			}
			return flag;
		}
	}

	public virtual bool IsLightingActive
	{
		get
		{
			bool flag = true;
			foreach (IDebugDisplaySettingsData setting in m_Settings)
			{
				flag &= setting.IsLightingActive;
			}
			return flag;
		}
	}

	protected TData Add<TData>(TData newData) where TData : IDebugDisplaySettingsData
	{
		m_Settings.Add(newData);
		return newData;
	}

	public void ForEach(Action<IDebugDisplaySettingsData> onExecute)
	{
		foreach (IDebugDisplaySettingsData setting in m_Settings)
		{
			onExecute(setting);
		}
	}

	public virtual void Reset()
	{
		m_Settings.Clear();
	}

	public virtual bool TryGetScreenClearColor(ref Color color)
	{
		foreach (IDebugDisplaySettingsData setting in m_Settings)
		{
			if (setting.TryGetScreenClearColor(ref color))
			{
				return true;
			}
		}
		return false;
	}
}
