using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEngine.UIElements;

internal class EventDebugger
{
	internal struct HistogramRecord
	{
		public long count;

		public long duration;
	}

	private Dictionary<IPanel, List<EventDebuggerCallTrace>> m_EventCalledObjects;

	private Dictionary<IPanel, List<EventDebuggerDefaultActionTrace>> m_EventDefaultActionObjects;

	private Dictionary<IPanel, List<EventDebuggerPathTrace>> m_EventPathObjects;

	private Dictionary<IPanel, List<EventDebuggerTrace>> m_EventProcessedEvents;

	private Dictionary<IPanel, Stack<EventDebuggerTrace>> m_StackOfProcessedEvent;

	private Dictionary<IPanel, Dictionary<long, int>> m_EventTypeProcessedCount;

	private readonly Dictionary<IPanel, long> m_ModificationCount;

	private readonly bool m_Log;

	public IPanel panel { get; set; }

	public bool isReplaying { get; private set; }

	public float playbackSpeed { get; set; } = 1f;

	public bool isPlaybackPaused { get; set; }

	public Dictionary<long, int> eventTypeProcessedCount
	{
		get
		{
			Dictionary<long, int> value;
			return m_EventTypeProcessedCount.TryGetValue(panel, out value) ? value : null;
		}
	}

	public bool suspended { get; set; }

	public void UpdateModificationCount()
	{
		if (panel != null)
		{
			if (!m_ModificationCount.TryGetValue(panel, out var value))
			{
				value = 0L;
			}
			value++;
			m_ModificationCount[panel] = value;
		}
	}

	public void BeginProcessEvent(EventBase evt, IEventHandler mouseCapture)
	{
		AddBeginProcessEvent(evt, mouseCapture);
		UpdateModificationCount();
	}

	public void EndProcessEvent(EventBase evt, long duration, IEventHandler mouseCapture)
	{
		AddEndProcessEvent(evt, duration, mouseCapture);
		UpdateModificationCount();
	}

	public void LogCall(int cbHashCode, string cbName, EventBase evt, bool propagationHasStopped, bool immediatePropagationHasStopped, bool defaultHasBeenPrevented, long duration, IEventHandler mouseCapture)
	{
		AddCallObject(cbHashCode, cbName, evt, propagationHasStopped, immediatePropagationHasStopped, defaultHasBeenPrevented, duration, mouseCapture);
		UpdateModificationCount();
	}

	public void LogIMGUICall(EventBase evt, long duration, IEventHandler mouseCapture)
	{
		AddIMGUICall(evt, duration, mouseCapture);
		UpdateModificationCount();
	}

	public void LogExecuteDefaultAction(EventBase evt, PropagationPhase phase, long duration, IEventHandler mouseCapture)
	{
		AddExecuteDefaultAction(evt, phase, duration, mouseCapture);
		UpdateModificationCount();
	}

	public static void LogPropagationPaths(EventBase evt, PropagationPaths paths)
	{
	}

	private void LogPropagationPathsInternal(EventBase evt, PropagationPaths paths)
	{
		PropagationPaths paths2 = ((paths == null) ? new PropagationPaths() : new PropagationPaths(paths));
		AddPropagationPaths(evt, paths2);
		UpdateModificationCount();
	}

	public List<EventDebuggerCallTrace> GetCalls(IPanel panel, EventDebuggerEventRecord evt = null)
	{
		if (!m_EventCalledObjects.TryGetValue(panel, out var value))
		{
			return null;
		}
		if (evt != null && value != null)
		{
			List<EventDebuggerCallTrace> list = new List<EventDebuggerCallTrace>();
			foreach (EventDebuggerCallTrace item in value)
			{
				if (item.eventBase.eventId == evt.eventId)
				{
					list.Add(item);
				}
			}
			value = list;
		}
		return value;
	}

	public List<EventDebuggerDefaultActionTrace> GetDefaultActions(IPanel panel, EventDebuggerEventRecord evt = null)
	{
		if (!m_EventDefaultActionObjects.TryGetValue(panel, out var value))
		{
			return null;
		}
		if (evt != null && value != null)
		{
			List<EventDebuggerDefaultActionTrace> list = new List<EventDebuggerDefaultActionTrace>();
			foreach (EventDebuggerDefaultActionTrace item in value)
			{
				if (item.eventBase.eventId == evt.eventId)
				{
					list.Add(item);
				}
			}
			value = list;
		}
		return value;
	}

	public List<EventDebuggerPathTrace> GetPropagationPaths(IPanel panel, EventDebuggerEventRecord evt = null)
	{
		if (!m_EventPathObjects.TryGetValue(panel, out var value))
		{
			return null;
		}
		if (evt != null && value != null)
		{
			List<EventDebuggerPathTrace> list = new List<EventDebuggerPathTrace>();
			foreach (EventDebuggerPathTrace item in value)
			{
				if (item.eventBase.eventId == evt.eventId)
				{
					list.Add(item);
				}
			}
			value = list;
		}
		return value;
	}

	public List<EventDebuggerTrace> GetBeginEndProcessedEvents(IPanel panel, EventDebuggerEventRecord evt = null)
	{
		if (!m_EventProcessedEvents.TryGetValue(panel, out var value))
		{
			return null;
		}
		if (evt != null && value != null)
		{
			List<EventDebuggerTrace> list = new List<EventDebuggerTrace>();
			foreach (EventDebuggerTrace item in value)
			{
				if (item.eventBase.eventId == evt.eventId)
				{
					list.Add(item);
				}
			}
			value = list;
		}
		return value;
	}

	public long GetModificationCount(IPanel panel)
	{
		if (panel == null)
		{
			return -1L;
		}
		if (!m_ModificationCount.TryGetValue(panel, out var value))
		{
			value = -1L;
		}
		return value;
	}

	public void ClearLogs()
	{
		UpdateModificationCount();
		if (panel == null)
		{
			m_EventCalledObjects.Clear();
			m_EventDefaultActionObjects.Clear();
			m_EventPathObjects.Clear();
			m_EventProcessedEvents.Clear();
			m_StackOfProcessedEvent.Clear();
			m_EventTypeProcessedCount.Clear();
			return;
		}
		m_EventCalledObjects.Remove(panel);
		m_EventDefaultActionObjects.Remove(panel);
		m_EventPathObjects.Remove(panel);
		m_EventProcessedEvents.Remove(panel);
		m_StackOfProcessedEvent.Remove(panel);
		if (m_EventTypeProcessedCount.TryGetValue(panel, out var value))
		{
			value.Clear();
		}
	}

	public void SaveReplaySessionFromSelection(string path, List<EventDebuggerEventRecord> eventList)
	{
		if (!string.IsNullOrEmpty(path))
		{
			EventDebuggerRecordList obj = new EventDebuggerRecordList
			{
				eventList = eventList
			};
			string contents = JsonUtility.ToJson(obj);
			File.WriteAllText(path, contents);
			Debug.Log("Saved under: " + path);
		}
	}

	public EventDebuggerRecordList LoadReplaySession(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		string json = File.ReadAllText(path);
		return JsonUtility.FromJson<EventDebuggerRecordList>(json);
	}

	public IEnumerator ReplayEvents(IEnumerable<EventDebuggerEventRecord> eventBases, Action<int, int> refreshList)
	{
		if (eventBases != null)
		{
			isReplaying = true;
			IEnumerator doReplay = DoReplayEvents(eventBases, refreshList);
			while (doReplay.MoveNext())
			{
				yield return null;
			}
		}
	}

	public void StopPlayback()
	{
		isReplaying = false;
		isPlaybackPaused = false;
	}

	private IEnumerator DoReplayEvents(IEnumerable<EventDebuggerEventRecord> eventBases, Action<int, int> refreshList)
	{
		List<EventDebuggerEventRecord> sortedEvents = eventBases.OrderBy((EventDebuggerEventRecord e) => e.timestamp).ToList();
		int sortedEventsCount = sortedEvents.Count;
		for (int i = 0; i < sortedEventsCount && isReplaying; i++)
		{
			EventDebuggerEventRecord eventBase = sortedEvents[i];
			Event newEvent = new Event
			{
				button = eventBase.button,
				clickCount = eventBase.clickCount,
				modifiers = eventBase.modifiers,
				mousePosition = eventBase.mousePosition
			};
			if (eventBase.eventTypeId == EventBase<MouseMoveEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseMove;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseMove), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<MouseDownEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseDown;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseDown), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<MouseUpEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseUp;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseUp), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<ContextClickEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.ContextClick;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.ContextClick), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<MouseEnterWindowEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseEnterWindow;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseEnterWindow), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<MouseLeaveWindowEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseLeaveWindow;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseLeaveWindow), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<PointerMoveEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseMove;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseMove), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<PointerDownEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseDown;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseDown), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<PointerUpEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.MouseUp;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.MouseUp), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<WheelEvent>.TypeId() && eventBase.hasUnderlyingPhysicalEvent)
			{
				newEvent.type = EventType.ScrollWheel;
				newEvent.delta = eventBase.delta;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.ScrollWheel), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<KeyDownEvent>.TypeId())
			{
				newEvent.type = EventType.KeyDown;
				newEvent.character = eventBase.character;
				newEvent.keyCode = eventBase.keyCode;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.KeyDown), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<KeyUpEvent>.TypeId())
			{
				newEvent.type = EventType.KeyUp;
				newEvent.character = eventBase.character;
				newEvent.keyCode = eventBase.keyCode;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.KeyUp), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<NavigationMoveEvent>.TypeId())
			{
				panel.dispatcher.Dispatch(NavigationMoveEvent.GetPooled(eventBase.navigationDirection, eventBase.deviceType, eventBase.modifiers), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<NavigationSubmitEvent>.TypeId())
			{
				panel.dispatcher.Dispatch(NavigationEventBase<NavigationSubmitEvent>.GetPooled(eventBase.deviceType, eventBase.modifiers), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<NavigationCancelEvent>.TypeId())
			{
				panel.dispatcher.Dispatch(NavigationEventBase<NavigationCancelEvent>.GetPooled(eventBase.deviceType, eventBase.modifiers), panel, DispatchMode.Default);
			}
			else if (eventBase.eventTypeId == EventBase<ValidateCommandEvent>.TypeId())
			{
				newEvent.type = EventType.ValidateCommand;
				newEvent.commandName = eventBase.commandName;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.ValidateCommand), panel, DispatchMode.Default);
			}
			else
			{
				if (eventBase.eventTypeId != EventBase<ExecuteCommandEvent>.TypeId())
				{
					if (eventBase.eventTypeId == EventBase<IMGUIEvent>.TypeId())
					{
						Debug.Log("Skipped IMGUI event (" + eventBase.eventBaseName + "): " + eventBase);
						IEnumerator awaitSkipped = AwaitForNextEvent(i);
						while (awaitSkipped.MoveNext())
						{
							yield return null;
						}
					}
					else
					{
						Debug.Log("Skipped event (" + eventBase.eventBaseName + "): " + eventBase);
						IEnumerator awaitSkipped2 = AwaitForNextEvent(i);
						while (awaitSkipped2.MoveNext())
						{
							yield return null;
						}
					}
					continue;
				}
				newEvent.type = EventType.ExecuteCommand;
				newEvent.commandName = eventBase.commandName;
				panel.dispatcher.Dispatch(UIElementsUtility.CreateEvent(newEvent, EventType.ExecuteCommand), panel, DispatchMode.Default);
			}
			refreshList?.Invoke(i, sortedEventsCount);
			Debug.Log($"Replayed event {eventBase.eventId.ToString()} ({eventBase.eventBaseName}): {newEvent}");
			IEnumerator await = AwaitForNextEvent(i);
			while (await.MoveNext())
			{
				yield return null;
			}
		}
		isReplaying = false;
		IEnumerator AwaitForNextEvent(int currentIndex)
		{
			if (currentIndex != sortedEvents.Count - 1)
			{
				long deltaTimestampMs = sortedEvents[currentIndex + 1].timestamp - sortedEvents[currentIndex].timestamp;
				float timeMs = 0f;
				while (timeMs < (float)deltaTimestampMs)
				{
					if (isPlaybackPaused)
					{
						yield return null;
					}
					else
					{
						long time = Panel.TimeSinceStartupMs();
						yield return null;
						long delta = Panel.TimeSinceStartupMs() - time;
						timeMs += (float)delta * playbackSpeed;
					}
				}
			}
		}
	}

	public Dictionary<string, HistogramRecord> ComputeHistogram(List<EventDebuggerEventRecord> eventBases)
	{
		if (panel == null || !m_EventProcessedEvents.TryGetValue(panel, out var value))
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		Dictionary<string, HistogramRecord> dictionary = new Dictionary<string, HistogramRecord>();
		foreach (EventDebuggerTrace item in value)
		{
			if (eventBases == null || eventBases.Count == 0 || eventBases.Contains(item.eventBase))
			{
				string eventBaseName = item.eventBase.eventBaseName;
				long num = item.duration;
				long num2 = 1L;
				if (dictionary.TryGetValue(eventBaseName, out var value2))
				{
					num += value2.duration;
					num2 += value2.count;
				}
				dictionary[eventBaseName] = new HistogramRecord
				{
					count = num2,
					duration = num
				};
			}
		}
		return dictionary;
	}

	public EventDebugger()
	{
		m_EventCalledObjects = new Dictionary<IPanel, List<EventDebuggerCallTrace>>();
		m_EventDefaultActionObjects = new Dictionary<IPanel, List<EventDebuggerDefaultActionTrace>>();
		m_EventPathObjects = new Dictionary<IPanel, List<EventDebuggerPathTrace>>();
		m_StackOfProcessedEvent = new Dictionary<IPanel, Stack<EventDebuggerTrace>>();
		m_EventProcessedEvents = new Dictionary<IPanel, List<EventDebuggerTrace>>();
		m_EventTypeProcessedCount = new Dictionary<IPanel, Dictionary<long, int>>();
		m_ModificationCount = new Dictionary<IPanel, long>();
		m_Log = true;
	}

	private void AddCallObject(int cbHashCode, string cbName, EventBase evt, bool propagationHasStopped, bool immediatePropagationHasStopped, bool defaultHasBeenPrevented, long duration, IEventHandler mouseCapture)
	{
		if (!suspended && m_Log)
		{
			EventDebuggerCallTrace item = new EventDebuggerCallTrace(panel, evt, cbHashCode, cbName, propagationHasStopped, immediatePropagationHasStopped, defaultHasBeenPrevented, duration, mouseCapture);
			if (!m_EventCalledObjects.TryGetValue(panel, out var value))
			{
				value = new List<EventDebuggerCallTrace>();
				m_EventCalledObjects.Add(panel, value);
			}
			value.Add(item);
		}
	}

	private void AddExecuteDefaultAction(EventBase evt, PropagationPhase phase, long duration, IEventHandler mouseCapture)
	{
		if (!suspended && m_Log)
		{
			EventDebuggerDefaultActionTrace item = new EventDebuggerDefaultActionTrace(panel, evt, phase, duration, mouseCapture);
			if (!m_EventDefaultActionObjects.TryGetValue(panel, out var value))
			{
				value = new List<EventDebuggerDefaultActionTrace>();
				m_EventDefaultActionObjects.Add(panel, value);
			}
			value.Add(item);
		}
	}

	private void AddPropagationPaths(EventBase evt, PropagationPaths paths)
	{
		if (!suspended && m_Log)
		{
			EventDebuggerPathTrace item = new EventDebuggerPathTrace(panel, evt, paths);
			if (!m_EventPathObjects.TryGetValue(panel, out var value))
			{
				value = new List<EventDebuggerPathTrace>();
				m_EventPathObjects.Add(panel, value);
			}
			value.Add(item);
		}
	}

	private void AddIMGUICall(EventBase evt, long duration, IEventHandler mouseCapture)
	{
		if (!suspended && m_Log)
		{
			EventDebuggerCallTrace item = new EventDebuggerCallTrace(panel, evt, 0, "OnGUI", propagationHasStopped: false, immediatePropagationHasStopped: false, defaultHasBeenPrevented: false, duration, mouseCapture);
			if (!m_EventCalledObjects.TryGetValue(panel, out var value))
			{
				value = new List<EventDebuggerCallTrace>();
				m_EventCalledObjects.Add(panel, value);
			}
			value.Add(item);
		}
	}

	private void AddBeginProcessEvent(EventBase evt, IEventHandler mouseCapture)
	{
		if (suspended)
		{
			return;
		}
		EventDebuggerTrace eventDebuggerTrace = new EventDebuggerTrace(panel, evt, -1L, mouseCapture);
		if (!m_StackOfProcessedEvent.TryGetValue(panel, out var value))
		{
			value = new Stack<EventDebuggerTrace>();
			m_StackOfProcessedEvent.Add(panel, value);
		}
		if (!m_EventProcessedEvents.TryGetValue(panel, out var value2))
		{
			value2 = new List<EventDebuggerTrace>();
			m_EventProcessedEvents.Add(panel, value2);
		}
		value2.Add(eventDebuggerTrace);
		value.Push(eventDebuggerTrace);
		if (m_EventTypeProcessedCount.TryGetValue(panel, out var value3))
		{
			if (!value3.TryGetValue(eventDebuggerTrace.eventBase.eventTypeId, out var value4))
			{
				value4 = 0;
			}
			value3[eventDebuggerTrace.eventBase.eventTypeId] = value4 + 1;
		}
	}

	private void AddEndProcessEvent(EventBase evt, long duration, IEventHandler mouseCapture)
	{
		if (suspended)
		{
			return;
		}
		bool flag = false;
		if (m_StackOfProcessedEvent.TryGetValue(panel, out var value) && value.Count > 0)
		{
			EventDebuggerTrace eventDebuggerTrace = value.Peek();
			if (eventDebuggerTrace.eventBase.eventId == evt.eventId)
			{
				value.Pop();
				eventDebuggerTrace.duration = duration;
				if (eventDebuggerTrace.eventBase.target == null)
				{
					eventDebuggerTrace.eventBase.target = evt.target;
				}
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		EventDebuggerTrace eventDebuggerTrace2 = new EventDebuggerTrace(panel, evt, duration, mouseCapture);
		if (!m_EventProcessedEvents.TryGetValue(panel, out var value2))
		{
			value2 = new List<EventDebuggerTrace>();
			m_EventProcessedEvents.Add(panel, value2);
		}
		value2.Add(eventDebuggerTrace2);
		if (m_EventTypeProcessedCount.TryGetValue(panel, out var value3))
		{
			if (!value3.TryGetValue(eventDebuggerTrace2.eventBase.eventTypeId, out var value4))
			{
				value4 = 0;
			}
			value3[eventDebuggerTrace2.eventBase.eventTypeId] = value4 + 1;
		}
	}

	public static string GetObjectDisplayName(object obj, bool withHashCode = true)
	{
		if (obj == null)
		{
			return string.Empty;
		}
		Type type = obj.GetType();
		string text = GetTypeDisplayName(type);
		if (obj is VisualElement)
		{
			VisualElement visualElement = obj as VisualElement;
			if (!string.IsNullOrEmpty(visualElement.name))
			{
				text = text + "#" + visualElement.name;
			}
		}
		if (withHashCode)
		{
			text = text + " (" + obj.GetHashCode().ToString("x8") + ")";
		}
		return text;
	}

	public static string GetTypeDisplayName(Type type)
	{
		return type.IsGenericType ? (type.Name.TrimEnd('`', '1') + "<" + type.GetGenericArguments()[0].Name + ">") : type.Name;
	}
}
