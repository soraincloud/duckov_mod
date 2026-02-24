using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

[Serializable]
internal class EventDebuggerEventRecord
{
	[field: SerializeField]
	public string eventBaseName { get; private set; }

	[field: SerializeField]
	public long eventTypeId { get; private set; }

	[field: SerializeField]
	public ulong eventId { get; private set; }

	[field: SerializeField]
	private ulong triggerEventId { get; set; }

	[field: SerializeField]
	internal long timestamp { get; private set; }

	public IEventHandler target { get; set; }

	private List<IEventHandler> skipElements { get; set; }

	[field: SerializeField]
	public bool hasUnderlyingPhysicalEvent { get; private set; }

	private bool isPropagationStopped { get; set; }

	private bool isImmediatePropagationStopped { get; set; }

	private bool isDefaultPrevented { get; set; }

	public PropagationPhase propagationPhase { get; private set; }

	private IEventHandler currentTarget { get; set; }

	private bool dispatch { get; set; }

	private Vector2 originalMousePosition { get; set; }

	public EventModifiers modifiers { get; private set; }

	[field: SerializeField]
	public Vector2 mousePosition { get; private set; }

	[field: SerializeField]
	public int clickCount { get; private set; }

	[field: SerializeField]
	public int button { get; private set; }

	[field: SerializeField]
	public int pressedButtons { get; private set; }

	[field: SerializeField]
	public Vector3 delta { get; private set; }

	[field: SerializeField]
	public char character { get; private set; }

	[field: SerializeField]
	public KeyCode keyCode { get; private set; }

	[field: SerializeField]
	public string commandName { get; private set; }

	[field: SerializeField]
	public NavigationDeviceType deviceType { get; private set; }

	[field: SerializeField]
	public NavigationMoveEvent.Direction navigationDirection { get; private set; }

	private void Init(EventBase evt)
	{
		Type type = evt.GetType();
		eventBaseName = EventDebugger.GetTypeDisplayName(type);
		eventTypeId = evt.eventTypeId;
		eventId = evt.eventId;
		triggerEventId = evt.triggerEventId;
		timestamp = evt.timestamp;
		target = evt.target;
		skipElements = evt.skipElements;
		isPropagationStopped = evt.isPropagationStopped;
		isImmediatePropagationStopped = evt.isImmediatePropagationStopped;
		isDefaultPrevented = evt.isDefaultPrevented;
		IMouseEvent mouseEvent = evt as IMouseEvent;
		IMouseEventInternal mouseEventInternal = evt as IMouseEventInternal;
		hasUnderlyingPhysicalEvent = mouseEvent != null && mouseEventInternal != null && mouseEventInternal.triggeredByOS;
		propagationPhase = evt.propagationPhase;
		originalMousePosition = evt.originalMousePosition;
		currentTarget = evt.currentTarget;
		dispatch = evt.dispatch;
		if (mouseEvent != null)
		{
			modifiers = mouseEvent.modifiers;
			mousePosition = mouseEvent.mousePosition;
			button = mouseEvent.button;
			pressedButtons = mouseEvent.pressedButtons;
			clickCount = mouseEvent.clickCount;
			if (mouseEvent is WheelEvent wheelEvent)
			{
				delta = wheelEvent.delta;
			}
		}
		if (evt is IPointerEvent pointerEvent)
		{
			IPointerEventInternal pointerEventInternal = evt as IPointerEventInternal;
			hasUnderlyingPhysicalEvent = pointerEvent != null && pointerEventInternal != null && pointerEventInternal.triggeredByOS;
			modifiers = pointerEvent.modifiers;
			mousePosition = pointerEvent.position;
			button = pointerEvent.button;
			pressedButtons = pointerEvent.pressedButtons;
			clickCount = pointerEvent.clickCount;
		}
		if (evt is IKeyboardEvent keyboardEvent)
		{
			modifiers = keyboardEvent.modifiers;
			character = keyboardEvent.character;
			keyCode = keyboardEvent.keyCode;
		}
		if (evt is ICommandEvent commandEvent)
		{
			commandName = commandEvent.commandName;
		}
		if (evt is INavigationEvent navigationEvent)
		{
			deviceType = navigationEvent.deviceType;
			if (evt is NavigationMoveEvent navigationMoveEvent)
			{
				navigationDirection = navigationMoveEvent.direction;
			}
		}
	}

	public EventDebuggerEventRecord(EventBase evt)
	{
		Init(evt);
	}

	public string TimestampString()
	{
		long ticks = (long)((float)timestamp / 1000f * 10000000f);
		return new DateTime(ticks).ToString("HH:mm:ss.ffffff");
	}
}
