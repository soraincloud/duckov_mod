using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal static class UIElementsRuntimeUtility
{
	public delegate BaseRuntimePanel CreateRuntimePanelDelegate(ScriptableObject ownerObject);

	private static bool s_RegisteredPlayerloopCallback;

	private static List<Panel> s_SortedRuntimePanels;

	private static bool s_PanelOrderingDirty;

	internal static int s_ResolvedSortingIndexMax;

	internal static readonly string s_RepaintProfilerMarkerName;

	private static readonly ProfilerMarker s_RepaintProfilerMarker;

	private static int currentOverlayIndex;

	private static DefaultEventSystem s_DefaultEventSystem;

	private static List<PanelSettings> s_PotentiallyEmptyPanelSettings;

	internal static Object activeEventSystem { get; private set; }

	internal static bool useDefaultEventSystem => activeEventSystem == null;

	internal static DefaultEventSystem defaultEventSystem => s_DefaultEventSystem ?? (s_DefaultEventSystem = new DefaultEventSystem());

	private static event Action s_onRepaintOverlayPanels;

	internal static event Action onRepaintOverlayPanels
	{
		add
		{
			if (UIElementsRuntimeUtility.s_onRepaintOverlayPanels == null)
			{
				RegisterPlayerloopCallback();
			}
			s_onRepaintOverlayPanels += value;
		}
		remove
		{
			s_onRepaintOverlayPanels -= value;
			if (UIElementsRuntimeUtility.s_onRepaintOverlayPanels == null)
			{
				UnregisterPlayerloopCallback();
			}
		}
	}

	public static event Action<BaseRuntimePanel> onCreatePanel;

	static UIElementsRuntimeUtility()
	{
		s_RegisteredPlayerloopCallback = false;
		s_SortedRuntimePanels = new List<Panel>();
		s_PanelOrderingDirty = true;
		s_ResolvedSortingIndexMax = 0;
		s_RepaintProfilerMarkerName = "UIElementsRuntimeUtility.DoDispatch(Repaint Event)";
		s_RepaintProfilerMarker = new ProfilerMarker(s_RepaintProfilerMarkerName);
		currentOverlayIndex = -1;
		s_PotentiallyEmptyPanelSettings = new List<PanelSettings>();
		UIElementsRuntimeUtilityNative.RepaintOverlayPanelsCallback = delegate
		{
		};
		UIElementsRuntimeUtilityNative.RepaintOffscreenPanelsCallback = RepaintOffscreenPanels;
		Canvas.externBeginRenderOverlays = BeginRenderOverlays;
		Canvas.externRenderOverlaysBefore = delegate(int displayIndex, int sortOrder)
		{
			RenderOverlaysBeforePriority(displayIndex, sortOrder);
		};
		Canvas.externEndRenderOverlays = EndRenderOverlays;
	}

	public static EventBase CreateEvent(Event systemEvent)
	{
		return UIElementsUtility.CreateEvent(systemEvent, systemEvent.rawType);
	}

	public static BaseRuntimePanel FindOrCreateRuntimePanel(ScriptableObject ownerObject, CreateRuntimePanelDelegate createDelegate)
	{
		if (UIElementsUtility.TryGetPanel(ownerObject.GetInstanceID(), out var panel))
		{
			if (panel is BaseRuntimePanel result)
			{
				return result;
			}
			RemoveCachedPanelInternal(ownerObject.GetInstanceID());
		}
		BaseRuntimePanel baseRuntimePanel = createDelegate(ownerObject);
		baseRuntimePanel.IMGUIEventInterests = new EventInterests
		{
			wantsMouseMove = true,
			wantsMouseEnterLeaveWindow = true
		};
		RegisterCachedPanelInternal(ownerObject.GetInstanceID(), baseRuntimePanel);
		UIElementsRuntimeUtility.onCreatePanel?.Invoke(baseRuntimePanel);
		return baseRuntimePanel;
	}

	public static void DisposeRuntimePanel(ScriptableObject ownerObject)
	{
		if (UIElementsUtility.TryGetPanel(ownerObject.GetInstanceID(), out var panel))
		{
			panel.Dispose();
			RemoveCachedPanelInternal(ownerObject.GetInstanceID());
		}
	}

	private static void RegisterCachedPanelInternal(int instanceID, IPanel panel)
	{
		UIElementsUtility.RegisterCachedPanel(instanceID, panel as Panel);
		s_PanelOrderingDirty = true;
		if (!s_RegisteredPlayerloopCallback)
		{
			s_RegisteredPlayerloopCallback = true;
			RegisterPlayerloopCallback();
			Canvas.SetExternalCanvasEnabled(enabled: true);
		}
	}

	private static void RemoveCachedPanelInternal(int instanceID)
	{
		UIElementsUtility.RemoveCachedPanel(instanceID);
		s_PanelOrderingDirty = true;
		s_SortedRuntimePanels.Clear();
		UIElementsUtility.GetAllPanels(s_SortedRuntimePanels, ContextType.Player);
		if (s_SortedRuntimePanels.Count == 0)
		{
			s_RegisteredPlayerloopCallback = false;
			UnregisterPlayerloopCallback();
			Canvas.SetExternalCanvasEnabled(enabled: false);
		}
	}

	public static void RepaintOverlayPanels()
	{
		foreach (BaseRuntimePanel sortedPlayerPanel in GetSortedPlayerPanels())
		{
			if (!sortedPlayerPanel.drawToCameras)
			{
				RepaintOverlayPanel(sortedPlayerPanel);
			}
		}
		if (UIElementsRuntimeUtility.s_onRepaintOverlayPanels != null)
		{
			UIElementsRuntimeUtility.s_onRepaintOverlayPanels();
		}
	}

	public static void RepaintOffscreenPanels()
	{
		foreach (BaseRuntimePanel sortedPlayerPanel in GetSortedPlayerPanels())
		{
			if (sortedPlayerPanel.targetTexture != null)
			{
				RepaintOverlayPanel(sortedPlayerPanel);
			}
		}
	}

	public static void RepaintOverlayPanel(BaseRuntimePanel panel)
	{
		Camera current = Camera.current;
		RenderTexture active = RenderTexture.active;
		using (s_RepaintProfilerMarker.Auto())
		{
			panel.Repaint(Event.current);
		}
		Camera.SetupCurrent(current);
		RenderTexture.active = active;
	}

	internal static void BeginRenderOverlays(int displayIndex)
	{
		currentOverlayIndex = 0;
	}

	internal static void RenderOverlaysBeforePriority(int displayIndex, float maxPriority)
	{
		if (currentOverlayIndex < 0)
		{
			return;
		}
		List<Panel> sortedPlayerPanels = GetSortedPlayerPanels();
		while (currentOverlayIndex < sortedPlayerPanels.Count)
		{
			if (sortedPlayerPanels[currentOverlayIndex] is BaseRuntimePanel baseRuntimePanel)
			{
				if (baseRuntimePanel.sortingPriority >= maxPriority)
				{
					break;
				}
				if (baseRuntimePanel.targetDisplay == displayIndex)
				{
					RepaintOverlayPanel(baseRuntimePanel);
				}
			}
			currentOverlayIndex++;
		}
	}

	internal static void EndRenderOverlays(int displayIndex)
	{
		RenderOverlaysBeforePriority(displayIndex, float.MaxValue);
		currentOverlayIndex = -1;
	}

	public static void RegisterEventSystem(Object eventSystem)
	{
		if (activeEventSystem != null && activeEventSystem != eventSystem && eventSystem.GetType().Name == "EventSystem")
		{
			Debug.LogWarning("There can be only one active Event System.");
		}
		activeEventSystem = eventSystem;
	}

	public static void UnregisterEventSystem(Object eventSystem)
	{
		if (activeEventSystem == eventSystem)
		{
			activeEventSystem = null;
		}
	}

	public static void UpdateRuntimePanels()
	{
		RemoveUnusedPanels();
		UIRenderDevice.ProcessDeviceFreeQueue();
		foreach (BaseRuntimePanel sortedPlayerPanel in GetSortedPlayerPanels())
		{
			sortedPlayerPanel.Update();
		}
		if (Application.isPlaying && useDefaultEventSystem)
		{
			defaultEventSystem.Update(DefaultEventSystem.UpdateMode.IgnoreIfAppNotFocused);
		}
	}

	internal static void MarkPotentiallyEmpty(PanelSettings settings)
	{
		if (!s_PotentiallyEmptyPanelSettings.Contains(settings))
		{
			s_PotentiallyEmptyPanelSettings.Add(settings);
		}
	}

	internal static void RemoveUnusedPanels()
	{
		foreach (PanelSettings s_PotentiallyEmptyPanelSetting in s_PotentiallyEmptyPanelSettings)
		{
			UIDocumentList attachedUIDocumentsList = s_PotentiallyEmptyPanelSetting.m_AttachedUIDocumentsList;
			if (attachedUIDocumentsList == null || attachedUIDocumentsList.m_AttachedUIDocuments.Count == 0)
			{
				s_PotentiallyEmptyPanelSetting.DisposePanel();
			}
		}
		s_PotentiallyEmptyPanelSettings.Clear();
	}

	public static void RegisterPlayerloopCallback()
	{
		UIElementsRuntimeUtilityNative.RegisterPlayerloopCallback();
		UIElementsRuntimeUtilityNative.UpdateRuntimePanelsCallback = UpdateRuntimePanels;
	}

	public static void UnregisterPlayerloopCallback()
	{
		UIElementsRuntimeUtilityNative.UnregisterPlayerloopCallback();
		UIElementsRuntimeUtilityNative.UpdateRuntimePanelsCallback = null;
	}

	internal static void SetPanelOrderingDirty()
	{
		s_PanelOrderingDirty = true;
	}

	internal static List<Panel> GetSortedPlayerPanels()
	{
		if (s_PanelOrderingDirty)
		{
			SortPanels();
		}
		return s_SortedRuntimePanels;
	}

	private static void SortPanels()
	{
		s_SortedRuntimePanels.Clear();
		UIElementsUtility.GetAllPanels(s_SortedRuntimePanels, ContextType.Player);
		s_SortedRuntimePanels.Sort(delegate(Panel a, Panel b)
		{
			BaseRuntimePanel baseRuntimePanel2 = a as BaseRuntimePanel;
			BaseRuntimePanel baseRuntimePanel3 = b as BaseRuntimePanel;
			if (baseRuntimePanel2 == null || baseRuntimePanel3 == null)
			{
				return 0;
			}
			float num2 = baseRuntimePanel2.sortingPriority - baseRuntimePanel3.sortingPriority;
			return Mathf.Approximately(0f, num2) ? baseRuntimePanel2.m_RuntimePanelCreationIndex.CompareTo(baseRuntimePanel3.m_RuntimePanelCreationIndex) : ((!(num2 < 0f)) ? 1 : (-1));
		});
		for (int num = 0; num < s_SortedRuntimePanels.Count; num++)
		{
			if (s_SortedRuntimePanels[num] is BaseRuntimePanel baseRuntimePanel)
			{
				baseRuntimePanel.resolvedSortingIndex = num;
			}
		}
		s_ResolvedSortingIndexMax = s_SortedRuntimePanels.Count - 1;
		s_PanelOrderingDirty = false;
	}

	internal static Vector2 MultiDisplayBottomLeftToPanelPosition(Vector2 position, out int? targetDisplay)
	{
		Vector2 position2 = MultiDisplayToLocalScreenPosition(position, out targetDisplay);
		return ScreenBottomLeftToPanelPosition(position2, targetDisplay.GetValueOrDefault());
	}

	internal static Vector2 MultiDisplayToLocalScreenPosition(Vector2 position, out int? targetDisplay)
	{
		Vector3 vector = Display.RelativeMouseAt(position);
		if (vector != Vector3.zero)
		{
			targetDisplay = (int)vector.z;
			return vector;
		}
		targetDisplay = null;
		return position;
	}

	internal static Vector2 ScreenBottomLeftToPanelPosition(Vector2 position, int targetDisplay)
	{
		int num = Screen.height;
		if (targetDisplay > 0 && targetDisplay < Display.displays.Length)
		{
			num = Display.displays[targetDisplay].systemHeight;
		}
		position.y = (float)num - position.y;
		return position;
	}

	internal static Vector2 ScreenBottomLeftToPanelDelta(Vector2 delta)
	{
		delta.y = 0f - delta.y;
		return delta;
	}
}
