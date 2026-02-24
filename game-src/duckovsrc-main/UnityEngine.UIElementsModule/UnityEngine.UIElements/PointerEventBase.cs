#define UNITY_ASSERTIONS
using System;

namespace UnityEngine.UIElements;

[EventCategory(EventCategory.Pointer)]
public abstract class PointerEventBase<T> : EventBase<T>, IPointerEvent, IPointerEventInternal where T : PointerEventBase<T>, new()
{
	private const float k_DefaultButtonPressure = 0.5f;

	private bool m_AltitudeNeedsConversion = true;

	private bool m_AzimuthNeedsConversion = true;

	private float m_AltitudeAngle = 0f;

	private float m_AzimuthAngle = 0f;

	private bool m_TiltNeeded = true;

	private Vector2 m_Tilt = new Vector2(0f, 0f);

	public int pointerId { get; protected set; }

	public string pointerType { get; protected set; }

	public bool isPrimary { get; protected set; }

	public int button { get; protected set; }

	public int pressedButtons { get; protected set; }

	public Vector3 position { get; protected set; }

	public Vector3 localPosition { get; protected set; }

	public Vector3 deltaPosition { get; protected set; }

	public float deltaTime { get; protected set; }

	public int clickCount { get; protected set; }

	public float pressure { get; protected set; }

	public float tangentialPressure { get; protected set; }

	public float altitudeAngle
	{
		get
		{
			if (m_AltitudeNeedsConversion)
			{
				m_AltitudeAngle = TiltToAltitude(tilt);
				m_AltitudeNeedsConversion = false;
			}
			return m_AltitudeAngle;
		}
		protected set
		{
			m_AltitudeNeedsConversion = true;
			m_AltitudeAngle = value;
		}
	}

	public float azimuthAngle
	{
		get
		{
			if (m_AzimuthNeedsConversion)
			{
				m_AzimuthAngle = TiltToAzimuth(tilt);
				m_AzimuthNeedsConversion = false;
			}
			return m_AzimuthAngle;
		}
		protected set
		{
			m_AzimuthNeedsConversion = true;
			m_AzimuthAngle = value;
		}
	}

	public float twist { get; protected set; }

	public Vector2 tilt
	{
		get
		{
			if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer && pointerType == PointerType.touch && m_TiltNeeded)
			{
				m_Tilt = AzimuthAndAlitutudeToTilt(m_AltitudeAngle, m_AzimuthAngle);
				m_TiltNeeded = false;
			}
			return m_Tilt;
		}
		protected set
		{
			m_TiltNeeded = true;
			m_Tilt = value;
		}
	}

	public PenStatus penStatus { get; protected set; }

	public Vector2 radius { get; protected set; }

	public Vector2 radiusVariance { get; protected set; }

	public EventModifiers modifiers { get; protected set; }

	public bool shiftKey => (modifiers & EventModifiers.Shift) != 0;

	public bool ctrlKey => (modifiers & EventModifiers.Control) != 0;

	public bool commandKey => (modifiers & EventModifiers.Command) != 0;

	public bool altKey => (modifiers & EventModifiers.Alt) != 0;

	public bool actionKey
	{
		get
		{
			if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
			{
				return commandKey;
			}
			return ctrlKey;
		}
	}

	bool IPointerEventInternal.triggeredByOS { get; set; }

	bool IPointerEventInternal.recomputeTopElementUnderPointer { get; set; }

	public override IEventHandler currentTarget
	{
		get
		{
			return base.currentTarget;
		}
		internal set
		{
			base.currentTarget = value;
			if (currentTarget is VisualElement ele)
			{
				localPosition = ele.WorldToLocal(position);
			}
			else
			{
				localPosition = position;
			}
		}
	}

	protected override void Init()
	{
		base.Init();
		LocalInit();
	}

	private void LocalInit()
	{
		base.propagation = EventPropagation.Bubbles | EventPropagation.TricklesDown | EventPropagation.Cancellable;
		base.propagateToIMGUI = false;
		pointerId = 0;
		pointerType = PointerType.unknown;
		isPrimary = false;
		button = -1;
		pressedButtons = 0;
		position = Vector3.zero;
		localPosition = Vector3.zero;
		deltaPosition = Vector3.zero;
		deltaTime = 0f;
		clickCount = 0;
		pressure = 0f;
		tangentialPressure = 0f;
		altitudeAngle = 0f;
		azimuthAngle = 0f;
		tilt = new Vector2(0f, 0f);
		twist = 0f;
		penStatus = PenStatus.None;
		radius = Vector2.zero;
		radiusVariance = Vector2.zero;
		modifiers = EventModifiers.None;
		((IPointerEventInternal)this).triggeredByOS = false;
		((IPointerEventInternal)this).recomputeTopElementUnderPointer = false;
	}

	private static bool IsMouse(Event systemEvent)
	{
		EventType rawType = systemEvent.rawType;
		return rawType == EventType.MouseMove || rawType == EventType.MouseDown || rawType == EventType.MouseUp || rawType == EventType.MouseDrag || rawType == EventType.ContextClick || rawType == EventType.MouseEnterWindow || rawType == EventType.MouseLeaveWindow;
	}

	private static bool IsTouch(Event systemEvent)
	{
		EventType rawType = systemEvent.rawType;
		return rawType == EventType.TouchMove || rawType == EventType.TouchDown || rawType == EventType.TouchUp || rawType == EventType.TouchStationary || rawType == EventType.TouchEnter || rawType == EventType.TouchLeave;
	}

	private static float TiltToAzimuth(Vector2 tilt)
	{
		float result = 0f;
		if (tilt.x != 0f)
		{
			result = MathF.PI / 2f - Mathf.Atan2((0f - Mathf.Cos(tilt.x)) * Mathf.Sin(tilt.y), Mathf.Cos(tilt.y) * Mathf.Sin(tilt.x));
			if (result < 0f)
			{
				result += MathF.PI * 2f;
			}
			result = ((!(result >= MathF.PI / 2f)) ? (result + 4.712389f) : (result - MathF.PI / 2f));
		}
		return result;
	}

	private static Vector2 AzimuthAndAlitutudeToTilt(float altitude, float azimuth)
	{
		Vector2 result = new Vector2(0f, 0f);
		result.x = Mathf.Atan(Mathf.Cos(azimuth) * Mathf.Cos(altitude) / Mathf.Sin(azimuth));
		result.y = Mathf.Atan(Mathf.Cos(azimuth) * Mathf.Sin(altitude) / Mathf.Sin(azimuth));
		return result;
	}

	private static float TiltToAltitude(Vector2 tilt)
	{
		return MathF.PI / 2f - Mathf.Acos(Mathf.Cos(tilt.x) * Mathf.Cos(tilt.y));
	}

	public static T GetPooled(Event systemEvent)
	{
		T val = EventBase<T>.GetPooled();
		if (!IsMouse(systemEvent) && !IsTouch(systemEvent) && systemEvent.rawType != EventType.DragUpdated)
		{
			Debug.Assert(condition: false, "Unexpected event type: " + systemEvent.rawType.ToString() + " (" + systemEvent.type.ToString() + ")");
		}
		switch (systemEvent.pointerType)
		{
		default:
			val.pointerType = PointerType.mouse;
			val.pointerId = PointerId.mousePointerId;
			break;
		case UnityEngine.PointerType.Touch:
			val.pointerType = PointerType.touch;
			val.pointerId = PointerId.touchPointerIdBase;
			break;
		case UnityEngine.PointerType.Pen:
			val.pointerType = PointerType.pen;
			val.pointerId = PointerId.penPointerIdBase;
			if (systemEvent.penStatus == PenStatus.Barrel)
			{
				PointerDeviceState.PressButton(val.pointerId, 1);
			}
			else
			{
				PointerDeviceState.ReleaseButton(val.pointerId, 1);
			}
			if (systemEvent.penStatus == PenStatus.Eraser)
			{
				PointerDeviceState.PressButton(val.pointerId, 5);
			}
			else
			{
				PointerDeviceState.ReleaseButton(val.pointerId, 5);
			}
			break;
		}
		val.isPrimary = true;
		val.altitudeAngle = 0f;
		val.azimuthAngle = 0f;
		val.radius = Vector2.zero;
		val.radiusVariance = Vector2.zero;
		val.imguiEvent = systemEvent;
		if (systemEvent.rawType == EventType.MouseDown || systemEvent.rawType == EventType.TouchDown)
		{
			PointerDeviceState.PressButton(val.pointerId, systemEvent.button);
			val.button = systemEvent.button;
		}
		else if (systemEvent.rawType == EventType.MouseUp || systemEvent.rawType == EventType.TouchUp)
		{
			PointerDeviceState.ReleaseButton(val.pointerId, systemEvent.button);
			val.button = systemEvent.button;
		}
		else if (systemEvent.rawType == EventType.MouseMove || systemEvent.rawType == EventType.TouchMove)
		{
			val.button = -1;
		}
		val.pressedButtons = PointerDeviceState.GetPressedButtons(val.pointerId);
		val.position = systemEvent.mousePosition;
		val.localPosition = systemEvent.mousePosition;
		val.deltaPosition = systemEvent.delta;
		val.clickCount = systemEvent.clickCount;
		val.modifiers = systemEvent.modifiers;
		val.tilt = systemEvent.tilt;
		val.penStatus = systemEvent.penStatus;
		val.twist = systemEvent.twist;
		switch (systemEvent.pointerType)
		{
		default:
			val.pressure = ((val.pressedButtons == 0) ? 0f : 0.5f);
			break;
		case UnityEngine.PointerType.Touch:
			val.pressure = systemEvent.pressure;
			break;
		case UnityEngine.PointerType.Pen:
			val.pressure = systemEvent.pressure;
			break;
		}
		val.tangentialPressure = 0f;
		((IPointerEventInternal)val).triggeredByOS = true;
		return val;
	}

	internal static T GetPooled(EventType eventType, Vector3 mousePosition, Vector2 delta, int button, int clickCount, EventModifiers modifiers)
	{
		T val = EventBase<T>.GetPooled();
		val.pointerId = PointerId.mousePointerId;
		val.pointerType = PointerType.mouse;
		val.isPrimary = true;
		switch (eventType)
		{
		case EventType.MouseDown:
			PointerDeviceState.PressButton(val.pointerId, button);
			val.button = button;
			break;
		case EventType.MouseUp:
			PointerDeviceState.ReleaseButton(val.pointerId, button);
			val.button = button;
			break;
		default:
			val.button = -1;
			break;
		}
		val.pressedButtons = PointerDeviceState.GetPressedButtons(val.pointerId);
		val.position = mousePosition;
		val.localPosition = mousePosition;
		val.deltaPosition = delta;
		val.clickCount = clickCount;
		val.modifiers = modifiers;
		val.pressure = ((val.pressedButtons == 0) ? 0f : 0.5f);
		((IPointerEventInternal)val).triggeredByOS = true;
		return val;
	}

	public static T GetPooled(Touch touch, EventModifiers modifiers = EventModifiers.None)
	{
		T val = EventBase<T>.GetPooled();
		val.pointerId = touch.fingerId + PointerId.touchPointerIdBase;
		val.pointerType = PointerType.touch;
		bool flag = false;
		for (int i = PointerId.touchPointerIdBase; i < PointerId.touchPointerIdBase + PointerId.touchPointerCount; i++)
		{
			if (i != val.pointerId && PointerDeviceState.GetPressedButtons(i) != 0)
			{
				flag = true;
				break;
			}
		}
		val.isPrimary = !flag;
		if (touch.phase == TouchPhase.Began)
		{
			PointerDeviceState.PressButton(val.pointerId, 0);
			val.button = 0;
		}
		else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
		{
			PointerDeviceState.ReleaseButton(val.pointerId, 0);
			val.button = 0;
		}
		else
		{
			val.button = -1;
		}
		val.pressedButtons = PointerDeviceState.GetPressedButtons(val.pointerId);
		val.position = touch.position;
		val.localPosition = touch.position;
		val.deltaPosition = touch.deltaPosition;
		val.deltaTime = touch.deltaTime;
		val.clickCount = touch.tapCount;
		val.pressure = ((Mathf.Abs(touch.maximumPossiblePressure) > 1E-30f) ? (touch.pressure / touch.maximumPossiblePressure) : 1f);
		val.tangentialPressure = 0f;
		val.altitudeAngle = touch.altitudeAngle;
		val.azimuthAngle = touch.azimuthAngle;
		val.twist = 0f;
		val.tilt = new Vector2(0f, 0f);
		val.penStatus = PenStatus.None;
		val.radius = new Vector2(touch.radius, touch.radius);
		val.radiusVariance = new Vector2(touch.radiusVariance, touch.radiusVariance);
		val.modifiers = modifiers;
		((IPointerEventInternal)val).triggeredByOS = true;
		return val;
	}

	public static T GetPooled(PenData pen, EventModifiers modifiers = EventModifiers.None)
	{
		T val = EventBase<T>.GetPooled();
		val.pointerId = PointerId.penPointerIdBase;
		val.pointerType = PointerType.pen;
		val.isPrimary = true;
		if (pen.contactType == PenEventType.PenDown)
		{
			PointerDeviceState.PressButton(val.pointerId, 0);
			val.button = 0;
		}
		else if (pen.contactType == PenEventType.PenUp)
		{
			PointerDeviceState.ReleaseButton(val.pointerId, 0);
			val.button = 0;
		}
		else
		{
			val.button = -1;
		}
		if (pen.penStatus == PenStatus.Barrel)
		{
			PointerDeviceState.PressButton(val.pointerId, 1);
		}
		else
		{
			PointerDeviceState.ReleaseButton(val.pointerId, 1);
		}
		if (pen.penStatus == PenStatus.Eraser)
		{
			PointerDeviceState.PressButton(val.pointerId, 5);
		}
		else
		{
			PointerDeviceState.ReleaseButton(val.pointerId, 5);
		}
		val.pressedButtons = PointerDeviceState.GetPressedButtons(val.pointerId);
		val.position = pen.position;
		val.localPosition = pen.position;
		val.deltaPosition = pen.deltaPos;
		val.clickCount = 0;
		val.pressure = pen.pressure;
		val.tangentialPressure = 0f;
		val.twist = pen.twist;
		val.tilt = pen.tilt;
		val.penStatus = pen.penStatus;
		val.radius = Vector2.zero;
		val.radiusVariance = Vector2.zero;
		val.modifiers = modifiers;
		((IPointerEventInternal)val).triggeredByOS = true;
		return val;
	}

	internal static T GetPooled(IPointerEvent triggerEvent, Vector2 position, int pointerId)
	{
		if (triggerEvent != null)
		{
			return GetPooled(triggerEvent);
		}
		T val = EventBase<T>.GetPooled();
		val.position = position;
		val.localPosition = position;
		val.pointerId = pointerId;
		val.pointerType = PointerType.GetPointerType(pointerId);
		return val;
	}

	public static T GetPooled(IPointerEvent triggerEvent)
	{
		T val = EventBase<T>.GetPooled();
		if (triggerEvent != null)
		{
			val.pointerId = triggerEvent.pointerId;
			val.pointerType = triggerEvent.pointerType;
			val.isPrimary = triggerEvent.isPrimary;
			val.button = triggerEvent.button;
			val.pressedButtons = triggerEvent.pressedButtons;
			val.position = triggerEvent.position;
			val.localPosition = triggerEvent.localPosition;
			val.deltaPosition = triggerEvent.deltaPosition;
			val.deltaTime = triggerEvent.deltaTime;
			val.clickCount = triggerEvent.clickCount;
			val.pressure = triggerEvent.pressure;
			val.tangentialPressure = triggerEvent.tangentialPressure;
			val.altitudeAngle = triggerEvent.altitudeAngle;
			val.azimuthAngle = triggerEvent.azimuthAngle;
			val.twist = triggerEvent.twist;
			val.tilt = triggerEvent.tilt;
			val.penStatus = triggerEvent.penStatus;
			val.radius = triggerEvent.radius;
			val.radiusVariance = triggerEvent.radiusVariance;
			val.modifiers = triggerEvent.modifiers;
			if (triggerEvent is IPointerEventInternal pointerEventInternal)
			{
				((IPointerEventInternal)val).triggeredByOS |= pointerEventInternal.triggeredByOS;
			}
		}
		return val;
	}

	protected internal override void PreDispatch(IPanel panel)
	{
		base.PreDispatch(panel);
		if (((IPointerEventInternal)this).triggeredByOS)
		{
			PointerDeviceState.SavePointerPosition(pointerId, position, panel, panel.contextType);
		}
	}

	protected internal override void PostDispatch(IPanel panel)
	{
		for (int i = 0; i < PointerId.maxPointers; i++)
		{
			panel.ProcessPointerCapture(i);
		}
		if (!panel.ShouldSendCompatibilityMouseEvents(this) && ((IPointerEventInternal)this).triggeredByOS)
		{
			(panel as BaseVisualElementPanel)?.CommitElementUnderPointers();
		}
		base.PostDispatch(panel);
	}

	protected PointerEventBase()
	{
		LocalInit();
	}
}
