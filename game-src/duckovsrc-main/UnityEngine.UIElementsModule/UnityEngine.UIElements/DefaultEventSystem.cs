using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal class DefaultEventSystem
{
	public enum UpdateMode
	{
		Always,
		IgnoreIfAppNotFocused
	}

	internal struct FocusBasedEventSequenceContext : IDisposable
	{
		private DefaultEventSystem es;

		public FocusBasedEventSequenceContext(DefaultEventSystem es)
		{
			this.es = es;
			es.m_PreviousFocusedPanel = es.focusedPanel;
			es.m_PreviousFocusedElement = es.focusedPanel?.focusController.GetLeafFocusedElement();
		}

		public void Dispose()
		{
			es.m_PreviousFocusedPanel = null;
			es.m_PreviousFocusedElement = null;
		}
	}

	internal interface IInput
	{
		int penEventCount { get; }

		int touchCount { get; }

		bool mousePresent { get; }

		Vector3 mousePosition { get; }

		Vector2 mouseScrollDelta { get; }

		int mouseButtonCount { get; }

		bool anyKey { get; }

		float unscaledTime { get; }

		float doubleClickTime { get; }

		bool GetButtonDown(string button);

		float GetAxisRaw(string axis);

		void ResetPenEvents();

		void ClearLastPenContactEvent();

		PenData GetPenEvent(int index);

		PenData GetLastPenContactEvent();

		Touch GetTouch(int index);

		bool GetMouseButtonDown(int button);

		bool GetMouseButtonUp(int button);
	}

	private class Input : IInput
	{
		public int penEventCount => UnityEngine.Input.penEventCount;

		public int touchCount => UnityEngine.Input.touchCount;

		public bool mousePresent => UnityEngine.Input.mousePresent;

		public Vector3 mousePosition => UnityEngine.Input.mousePosition;

		public Vector2 mouseScrollDelta => UnityEngine.Input.mouseScrollDelta;

		public int mouseButtonCount => 3;

		public bool anyKey => UnityEngine.Input.anyKey;

		public float unscaledTime => Time.unscaledTime;

		public float doubleClickTime => (float)Event.GetDoubleClickTime() * 0.001f;

		public bool GetButtonDown(string button)
		{
			return UnityEngine.Input.GetButtonDown(button);
		}

		public float GetAxisRaw(string axis)
		{
			return UnityEngine.Input.GetAxis(axis);
		}

		public void ResetPenEvents()
		{
			UnityEngine.Input.ResetPenEvents();
		}

		public void ClearLastPenContactEvent()
		{
			UnityEngine.Input.ClearLastPenContactEvent();
		}

		public PenData GetPenEvent(int index)
		{
			return UnityEngine.Input.GetPenEvent(index);
		}

		public PenData GetLastPenContactEvent()
		{
			return UnityEngine.Input.GetLastPenContactEvent();
		}

		public Touch GetTouch(int index)
		{
			return UnityEngine.Input.GetTouch(index);
		}

		public bool GetMouseButtonDown(int button)
		{
			return UnityEngine.Input.GetMouseButtonDown(button);
		}

		public bool GetMouseButtonUp(int button)
		{
			return UnityEngine.Input.GetMouseButtonUp(button);
		}
	}

	private class NoInput : IInput
	{
		public int touchCount => 0;

		public int penEventCount => 0;

		public bool mousePresent => false;

		public Vector3 mousePosition => default(Vector3);

		public Vector2 mouseScrollDelta => default(Vector2);

		public int mouseButtonCount => 0;

		public bool anyKey => false;

		public float unscaledTime => 0f;

		public float doubleClickTime => float.PositiveInfinity;

		public bool GetButtonDown(string button)
		{
			return false;
		}

		public float GetAxisRaw(string axis)
		{
			return 0f;
		}

		public Touch GetTouch(int index)
		{
			return default(Touch);
		}

		public void ResetPenEvents()
		{
		}

		public void ClearLastPenContactEvent()
		{
		}

		public PenData GetPenEvent(int index)
		{
			return default(PenData);
		}

		public PenData GetLastPenContactEvent()
		{
			return default(PenData);
		}

		public bool GetMouseButtonDown(int button)
		{
			return false;
		}

		public bool GetMouseButtonUp(int button)
		{
			return false;
		}
	}

	internal static Func<bool> IsEditorRemoteConnected = () => false;

	private IInput m_Input;

	private readonly string m_HorizontalAxis = "Horizontal";

	private readonly string m_VerticalAxis = "Vertical";

	private readonly string m_SubmitButton = "Submit";

	private readonly string m_CancelButton = "Cancel";

	private readonly float m_InputActionsPerSecond = 10f;

	private readonly float m_RepeatDelay = 0.5f;

	private bool m_SendingTouchEvents;

	private bool m_SendingPenEvent;

	private Event m_Event = new Event();

	private BaseRuntimePanel m_FocusedPanel;

	private BaseRuntimePanel m_PreviousFocusedPanel;

	private Focusable m_PreviousFocusedElement;

	private EventModifiers m_CurrentModifiers;

	private int m_LastMousePressButton = -1;

	private float m_NextMousePressTime = 0f;

	private int m_LastMouseClickCount = 0;

	private Vector2 m_LastMousePosition = Vector2.zero;

	private bool m_MouseProcessedAtLeastOnce;

	private int m_ConsecutiveMoveCount;

	private Vector2 m_LastMoveVector;

	private float m_PrevActionTime;

	private bool m_IsMoveFromKeyboard;

	private bool isAppFocused => Application.isFocused;

	internal IInput input
	{
		get
		{
			return m_Input ?? (m_Input = GetDefaultInput());
		}
		set
		{
			m_Input = value;
		}
	}

	public BaseRuntimePanel focusedPanel
	{
		get
		{
			return m_FocusedPanel;
		}
		set
		{
			if (m_FocusedPanel != value)
			{
				m_FocusedPanel?.Blur();
				m_FocusedPanel = value;
				m_FocusedPanel?.Focus();
			}
		}
	}

	private IInput GetDefaultInput()
	{
		IInput input = new Input();
		try
		{
			input.GetAxisRaw(m_HorizontalAxis);
		}
		catch (InvalidOperationException)
		{
			input = new NoInput();
			Debug.LogWarning("UI Toolkit is currently relying on the legacy Input Manager for its active input source, but the legacy Input Manager is not available using your current Project Settings. Some UI Toolkit functionality might be missing or not working properly as a result. To fix this problem, you can enable \"Input Manager (old)\" or \"Both\" in the Active Input Source setting of the Player section. UI Toolkit is using its internal default event system to process input. Alternatively, you may activate new Input System support with UI Toolkit by adding an EventSystem component to your active scene.");
		}
		return input;
	}

	private bool ShouldIgnoreEventsOnAppNotFocused()
	{
		OperatingSystemFamily operatingSystemFamily = SystemInfo.operatingSystemFamily;
		OperatingSystemFamily operatingSystemFamily2 = operatingSystemFamily;
		if ((uint)(operatingSystemFamily2 - 1) <= 2u)
		{
			return true;
		}
		return false;
	}

	public void Reset()
	{
		m_LastMousePressButton = -1;
		m_NextMousePressTime = 0f;
		m_LastMouseClickCount = 0;
		m_LastMousePosition = Vector2.zero;
		m_MouseProcessedAtLeastOnce = false;
		m_ConsecutiveMoveCount = 0;
		m_IsMoveFromKeyboard = false;
		m_FocusedPanel = null;
	}

	public void Update(UpdateMode updateMode = UpdateMode.Always)
	{
		if (!isAppFocused && ShouldIgnoreEventsOnAppNotFocused() && updateMode == UpdateMode.IgnoreIfAppNotFocused)
		{
			return;
		}
		m_SendingPenEvent = ProcessPenEvents();
		if (!m_SendingPenEvent)
		{
			m_SendingTouchEvents = ProcessTouchEvents();
		}
		if (!m_SendingPenEvent && !m_SendingTouchEvents)
		{
			ProcessMouseEvents();
		}
		else
		{
			m_MouseProcessedAtLeastOnce = false;
		}
		using (FocusBasedEventSequence())
		{
			SendIMGUIEvents();
			SendInputEvents();
		}
	}

	internal FocusBasedEventSequenceContext FocusBasedEventSequence()
	{
		return new FocusBasedEventSequenceContext(this);
	}

	private void SendIMGUIEvents()
	{
		bool flag = true;
		while (Event.PopEvent(m_Event))
		{
			if (m_Event.type == EventType.Ignore || m_Event.type == EventType.Repaint || m_Event.type == EventType.Layout)
			{
				continue;
			}
			m_CurrentModifiers = (flag ? m_Event.modifiers : (m_CurrentModifiers | m_Event.modifiers));
			flag = false;
			if (m_Event.type == EventType.KeyUp || m_Event.type == EventType.KeyDown)
			{
				SendFocusBasedEvent((DefaultEventSystem self) => UIElementsRuntimeUtility.CreateEvent(self.m_Event), this);
				ProcessTabEvent(m_Event, m_CurrentModifiers);
			}
			else if (m_Event.type == EventType.ScrollWheel)
			{
				int? targetDisplay;
				Vector2 vector = UIElementsRuntimeUtility.MultiDisplayBottomLeftToPanelPosition(input.mousePosition, out targetDisplay);
				Vector2 vector2 = vector - m_LastMousePosition;
				Vector2 delta = m_Event.delta;
				SendPositionBasedEvent(vector, vector2, PointerId.mousePointerId, targetDisplay, (Vector3 panelPosition, Vector3 _, (EventModifiers modifiers, Vector2 scrollDelta) t) => WheelEvent.GetPooled(t.scrollDelta, panelPosition, t.modifiers), (m_CurrentModifiers, delta));
			}
			else if ((!m_SendingTouchEvents && !m_SendingPenEvent && m_Event.pointerType != UnityEngine.PointerType.Mouse) || m_Event.type == EventType.MouseEnterWindow || m_Event.type == EventType.MouseLeaveWindow)
			{
				int pointerId = ((m_Event.pointerType == UnityEngine.PointerType.Mouse) ? PointerId.mousePointerId : ((m_Event.pointerType == UnityEngine.PointerType.Touch) ? PointerId.touchPointerIdBase : PointerId.penPointerIdBase));
				int? targetDisplay2;
				Vector3 mousePosition = UIElementsRuntimeUtility.MultiDisplayToLocalScreenPosition(m_Event.mousePosition, out targetDisplay2);
				Vector2 delta2 = m_Event.delta;
				SendPositionBasedEvent(mousePosition, delta2, pointerId, targetDisplay2, delegate(Vector3 panelPosition, Vector3 panelDelta, Event evt)
				{
					evt.mousePosition = panelPosition;
					evt.delta = panelDelta;
					return UIElementsRuntimeUtility.CreateEvent(evt);
				}, m_Event, m_Event.type == EventType.MouseDown || m_Event.type == EventType.TouchDown);
			}
		}
	}

	private void ProcessMouseEvents()
	{
		if (!input.mousePresent)
		{
			return;
		}
		int? targetDisplay;
		Vector2 vector = UIElementsRuntimeUtility.MultiDisplayBottomLeftToPanelPosition(input.mousePosition, out targetDisplay);
		Vector2 vector2 = vector - m_LastMousePosition;
		if (!m_MouseProcessedAtLeastOnce)
		{
			vector2 = Vector2.zero;
			m_LastMousePosition = vector;
			m_MouseProcessedAtLeastOnce = true;
		}
		else if (!Mathf.Approximately(vector2.x, 0f) || !Mathf.Approximately(vector2.y, 0f))
		{
			m_LastMousePosition = vector;
			SendPositionBasedEvent(vector, vector2, PointerId.mousePointerId, targetDisplay, (Vector3 panelPosition, Vector3 panelDelta, DefaultEventSystem self) => PointerEventBase<PointerMoveEvent>.GetPooled(EventType.MouseMove, panelPosition, panelDelta, -1, 0, self.m_CurrentModifiers), this);
		}
		int mouseButtonCount = input.mouseButtonCount;
		for (int num = 0; num < mouseButtonCount; num++)
		{
			if (input.GetMouseButtonDown(num))
			{
				if (m_LastMousePressButton != num || input.unscaledTime >= m_NextMousePressTime)
				{
					m_LastMousePressButton = num;
					m_LastMouseClickCount = 0;
				}
				int item = ++m_LastMouseClickCount;
				m_NextMousePressTime = input.unscaledTime + input.doubleClickTime;
				SendPositionBasedEvent(vector, vector2, PointerId.mousePointerId, targetDisplay, (Vector3 panelPosition, Vector3 panelDelta, (int button, int clickCount, EventModifiers modifiers) t) => PointerEventHelper.GetPooled(EventType.MouseDown, panelPosition, panelDelta, t.button, t.clickCount, t.modifiers), (num, item, m_CurrentModifiers), deselectIfNoTarget: true);
			}
			if (input.GetMouseButtonUp(num))
			{
				int lastMouseClickCount = m_LastMouseClickCount;
				SendPositionBasedEvent(vector, vector2, PointerId.mousePointerId, targetDisplay, (Vector3 panelPosition, Vector3 panelDelta, (int button, int clickCount, EventModifiers modifiers) t) => PointerEventHelper.GetPooled(EventType.MouseUp, panelPosition, panelDelta, t.button, t.clickCount, t.modifiers), (num, lastMouseClickCount, m_CurrentModifiers));
			}
		}
	}

	private void SendInputEvents()
	{
		if (ShouldSendMoveFromInput())
		{
			SendFocusBasedEvent((DefaultEventSystem self) => NavigationMoveEvent.GetPooled(self.GetRawMoveVector(), self.m_IsMoveFromKeyboard ? NavigationDeviceType.Keyboard : NavigationDeviceType.NonKeyboard, self.m_CurrentModifiers), this);
		}
		if (input.GetButtonDown(m_SubmitButton))
		{
			SendFocusBasedEvent((DefaultEventSystem self) => NavigationEventBase<NavigationSubmitEvent>.GetPooled(self.input.anyKey ? NavigationDeviceType.Keyboard : NavigationDeviceType.NonKeyboard, self.m_CurrentModifiers), this);
		}
		if (input.GetButtonDown(m_CancelButton))
		{
			SendFocusBasedEvent((DefaultEventSystem self) => NavigationEventBase<NavigationCancelEvent>.GetPooled(self.input.anyKey ? NavigationDeviceType.Keyboard : NavigationDeviceType.NonKeyboard, self.m_CurrentModifiers), this);
		}
	}

	internal void OnFocusEvent(RuntimePanel panel, FocusEvent evt)
	{
		focusedPanel = panel;
	}

	internal void SendFocusBasedEvent<TArg>(Func<TArg, EventBase> evtFactory, TArg arg)
	{
		if (m_PreviousFocusedPanel != null)
		{
			using (EventBase eventBase = evtFactory(arg))
			{
				eventBase.target = m_PreviousFocusedElement ?? m_PreviousFocusedPanel.visualTree;
				m_PreviousFocusedPanel.visualTree.SendEvent(eventBase);
				UpdateFocusedPanel(m_PreviousFocusedPanel);
				return;
			}
		}
		List<Panel> sortedPlayerPanels = UIElementsRuntimeUtility.GetSortedPlayerPanels();
		for (int num = sortedPlayerPanels.Count - 1; num >= 0; num--)
		{
			Panel panel = sortedPlayerPanels[num];
			if (panel is BaseRuntimePanel baseRuntimePanel)
			{
				using EventBase eventBase2 = evtFactory(arg);
				eventBase2.target = baseRuntimePanel.visualTree;
				baseRuntimePanel.visualTree.SendEvent(eventBase2);
				if (baseRuntimePanel.focusController.focusedElement != null)
				{
					focusedPanel = baseRuntimePanel;
					break;
				}
				if (eventBase2.isPropagationStopped)
				{
					break;
				}
			}
		}
	}

	internal void SendPositionBasedEvent<TArg>(Vector3 mousePosition, Vector3 delta, int pointerId, Func<Vector3, Vector3, TArg, EventBase> evtFactory, TArg arg, bool deselectIfNoTarget = false)
	{
		SendPositionBasedEvent(mousePosition, delta, pointerId, null, evtFactory, arg, deselectIfNoTarget);
	}

	private void SendPositionBasedEvent<TArg>(Vector3 mousePosition, Vector3 delta, int pointerId, int? targetDisplay, Func<Vector3, Vector3, TArg, EventBase> evtFactory, TArg arg, bool deselectIfNoTarget = false)
	{
		if (focusedPanel != null)
		{
			UpdateFocusedPanel(focusedPanel);
		}
		IPanel panel = PointerDeviceState.GetPlayerPanelWithSoftPointerCapture(pointerId);
		IEventHandler capturingElement = RuntimePanel.s_EventDispatcher.pointerState.GetCapturingElement(pointerId);
		if (capturingElement is VisualElement visualElement)
		{
			panel = visualElement.panel;
		}
		BaseRuntimePanel baseRuntimePanel = null;
		Vector2 panelPosition = Vector2.zero;
		Vector2 panelDelta = Vector2.zero;
		if (panel is BaseRuntimePanel baseRuntimePanel2)
		{
			baseRuntimePanel = baseRuntimePanel2;
			baseRuntimePanel.ScreenToPanel(mousePosition, delta, out panelPosition, out panelDelta);
		}
		else
		{
			List<Panel> sortedPlayerPanels = UIElementsRuntimeUtility.GetSortedPlayerPanels();
			for (int num = sortedPlayerPanels.Count - 1; num >= 0; num--)
			{
				if (sortedPlayerPanels[num] is BaseRuntimePanel baseRuntimePanel3 && (!targetDisplay.HasValue || baseRuntimePanel3.targetDisplay == targetDisplay) && baseRuntimePanel3.ScreenToPanel(mousePosition, delta, out panelPosition, out panelDelta) && baseRuntimePanel3.Pick(panelPosition) != null)
				{
					baseRuntimePanel = baseRuntimePanel3;
					break;
				}
			}
		}
		BaseRuntimePanel baseRuntimePanel4 = PointerDeviceState.GetPanel(pointerId, ContextType.Player) as BaseRuntimePanel;
		if (baseRuntimePanel4 != baseRuntimePanel)
		{
			baseRuntimePanel4?.PointerLeavesPanel(pointerId, baseRuntimePanel4.ScreenToPanel(mousePosition));
			baseRuntimePanel?.PointerEntersPanel(pointerId, panelPosition);
		}
		if (baseRuntimePanel != null)
		{
			using (EventBase eventBase = evtFactory(panelPosition, panelDelta, arg))
			{
				baseRuntimePanel.visualTree.SendEvent(eventBase);
				if (eventBase.processedByFocusController)
				{
					UpdateFocusedPanel(baseRuntimePanel);
				}
				if (eventBase.eventTypeId == EventBase<PointerDownEvent>.TypeId())
				{
					PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(pointerId, baseRuntimePanel);
				}
				else if (eventBase.eventTypeId == EventBase<PointerUpEvent>.TypeId() && ((PointerUpEvent)eventBase).pressedButtons == 0)
				{
					PointerDeviceState.SetPlayerPanelWithSoftPointerCapture(pointerId, null);
				}
				return;
			}
		}
		if (deselectIfNoTarget)
		{
			focusedPanel = null;
		}
	}

	private void UpdateFocusedPanel(BaseRuntimePanel runtimePanel)
	{
		if (runtimePanel.focusController.focusedElement != null)
		{
			focusedPanel = runtimePanel;
		}
		else if (focusedPanel == runtimePanel)
		{
			focusedPanel = null;
		}
	}

	private static EventBase MakeTouchEvent(Touch touch, EventModifiers modifiers)
	{
		return touch.phase switch
		{
			TouchPhase.Began => PointerEventBase<PointerDownEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Moved => PointerEventBase<PointerMoveEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Stationary => PointerEventBase<PointerStationaryEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Ended => PointerEventBase<PointerUpEvent>.GetPooled(touch, modifiers), 
			TouchPhase.Canceled => PointerEventBase<PointerCancelEvent>.GetPooled(touch, modifiers), 
			_ => null, 
		};
	}

	private static EventBase MakePenEvent(PenData pen, EventModifiers modifiers)
	{
		return pen.contactType switch
		{
			PenEventType.PenDown => PointerEventBase<PointerDownEvent>.GetPooled(pen, modifiers), 
			PenEventType.PenUp => PointerEventBase<PointerUpEvent>.GetPooled(pen, modifiers), 
			_ => null, 
		};
	}

	private bool ProcessTouchEvents()
	{
		for (int i = 0; i < input.touchCount; i++)
		{
			Touch touch = input.GetTouch(i);
			if (touch.type != TouchType.Indirect)
			{
				touch.position = UIElementsRuntimeUtility.MultiDisplayBottomLeftToPanelPosition(touch.position, out var targetDisplay);
				touch.rawPosition = UIElementsRuntimeUtility.MultiDisplayBottomLeftToPanelPosition(touch.rawPosition, out var _);
				touch.deltaPosition = UIElementsRuntimeUtility.ScreenBottomLeftToPanelDelta(touch.deltaPosition);
				SendPositionBasedEvent(touch.position, touch.deltaPosition, PointerId.touchPointerIdBase + touch.fingerId, targetDisplay, delegate(Vector3 panelPosition, Vector3 panelDelta, Touch _touch)
				{
					_touch.position = panelPosition;
					_touch.deltaPosition = panelDelta;
					return MakeTouchEvent(_touch, EventModifiers.None);
				}, touch);
			}
		}
		return input.touchCount > 0;
	}

	private bool ProcessPenEvents()
	{
		PenData lastPenContactEvent = input.GetLastPenContactEvent();
		if (lastPenContactEvent.contactType == PenEventType.NoContact)
		{
			return false;
		}
		SendPositionBasedEvent(lastPenContactEvent.position, lastPenContactEvent.deltaPos, PointerId.penPointerIdBase, null, delegate(Vector3 panelPosition, Vector3 panelDelta, PenData _pen)
		{
			_pen.position = panelPosition;
			_pen.deltaPos = panelDelta;
			return MakePenEvent(_pen, EventModifiers.None);
		}, lastPenContactEvent);
		input.ClearLastPenContactEvent();
		return true;
	}

	private Vector2 GetRawMoveVector()
	{
		Vector2 zero = Vector2.zero;
		zero.x = input.GetAxisRaw(m_HorizontalAxis);
		zero.y = input.GetAxisRaw(m_VerticalAxis);
		if (input.GetButtonDown(m_HorizontalAxis))
		{
			if (zero.x < 0f)
			{
				zero.x = -1f;
			}
			if (zero.x > 0f)
			{
				zero.x = 1f;
			}
		}
		if (input.GetButtonDown(m_VerticalAxis))
		{
			if (zero.y < 0f)
			{
				zero.y = -1f;
			}
			if (zero.y > 0f)
			{
				zero.y = 1f;
			}
		}
		return zero;
	}

	private bool ShouldSendMoveFromInput()
	{
		float unscaledTime = input.unscaledTime;
		Vector2 rawMoveVector = GetRawMoveVector();
		if (Mathf.Approximately(rawMoveVector.x, 0f) && Mathf.Approximately(rawMoveVector.y, 0f))
		{
			m_ConsecutiveMoveCount = 0;
			m_IsMoveFromKeyboard = false;
			return false;
		}
		bool flag = input.GetButtonDown(m_HorizontalAxis) || input.GetButtonDown(m_VerticalAxis);
		bool flag2 = Vector2.Dot(rawMoveVector, m_LastMoveVector) > 0f;
		if (!flag)
		{
			flag = ((!flag2 || m_ConsecutiveMoveCount != 1) ? (unscaledTime > m_PrevActionTime + 1f / m_InputActionsPerSecond) : (unscaledTime > m_PrevActionTime + m_RepeatDelay));
		}
		if (!flag)
		{
			return false;
		}
		NavigationMoveEvent.Direction direction = NavigationMoveEvent.DetermineMoveDirection(rawMoveVector.x, rawMoveVector.y);
		if (direction != NavigationMoveEvent.Direction.None)
		{
			if (!flag2)
			{
				m_ConsecutiveMoveCount = 0;
			}
			m_ConsecutiveMoveCount++;
			m_PrevActionTime = unscaledTime;
			m_LastMoveVector = rawMoveVector;
			m_IsMoveFromKeyboard |= input.anyKey;
		}
		else
		{
			m_ConsecutiveMoveCount = 0;
			m_IsMoveFromKeyboard = false;
		}
		return direction != NavigationMoveEvent.Direction.None;
	}

	private void ProcessTabEvent(Event e, EventModifiers modifiers)
	{
		if (e.ShouldSendNavigationMoveEventRuntime())
		{
			NavigationMoveEvent.Direction item = (e.shift ? NavigationMoveEvent.Direction.Previous : NavigationMoveEvent.Direction.Next);
			SendFocusBasedEvent(((NavigationMoveEvent.Direction direction, EventModifiers modifiers, IInput input) t) => NavigationMoveEvent.GetPooled(t.direction, t.input.anyKey ? NavigationDeviceType.Keyboard : NavigationDeviceType.NonKeyboard, t.modifiers), (item, modifiers, input));
		}
	}
}
