#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace UnityEngine.UIElements;

public class IMGUIContainer : VisualElement, IDisposable
{
	public new class UxmlFactory : UxmlFactory<IMGUIContainer, UxmlTraits>
	{
	}

	public new class UxmlTraits : VisualElement.UxmlTraits
	{
		public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
		{
			get
			{
				yield break;
			}
		}

		public UxmlTraits()
		{
			base.focusIndex.defaultValue = 0;
			base.focusable.defaultValue = true;
		}
	}

	private struct GUIGlobals
	{
		public Matrix4x4 matrix;

		public Color color;

		public Color contentColor;

		public Color backgroundColor;

		public bool enabled;

		public bool changed;

		public int displayIndex;
	}

	private Action m_OnGUIHandler;

	private ObjectGUIState m_ObjectGUIState;

	internal bool useOwnerObjectGUIState;

	private bool m_CullingEnabled = false;

	private bool m_IsFocusDelegated = false;

	private bool m_RefreshCachedLayout = true;

	private GUILayoutUtility.LayoutCache m_Cache = null;

	private Rect m_CachedClippingRect = Rect.zero;

	private Matrix4x4 m_CachedTransform = Matrix4x4.identity;

	private bool lostFocus = false;

	private bool receivedFocus = false;

	private FocusChangeDirection focusChangeDirection = FocusChangeDirection.unspecified;

	private bool hasFocusableControls = false;

	private int newKeyboardFocusControlID = 0;

	public static readonly string ussClassName;

	internal static readonly string ussFoldoutChildDepthClassName;

	internal static readonly List<string> ussFoldoutChildDepthClassNames;

	private GUIGlobals m_GUIGlobals;

	private static readonly ProfilerMarker k_OnGUIMarker;

	private static readonly ProfilerMarker k_ImmediateCallbackMarker;

	private static Event s_DefaultMeasureEvent;

	private static Event s_MeasureEvent;

	private static Event s_CurrentEvent;

	public Action onGUIHandler
	{
		get
		{
			return m_OnGUIHandler;
		}
		set
		{
			if (m_OnGUIHandler != value)
			{
				m_OnGUIHandler = value;
				IncrementVersion(VersionChangeType.Layout);
				IncrementVersion(VersionChangeType.Repaint);
			}
		}
	}

	internal ObjectGUIState guiState
	{
		get
		{
			Debug.Assert(!useOwnerObjectGUIState);
			if (m_ObjectGUIState == null)
			{
				m_ObjectGUIState = new ObjectGUIState();
			}
			return m_ObjectGUIState;
		}
	}

	internal Rect lastWorldClip { get; set; }

	public bool cullingEnabled
	{
		get
		{
			return m_CullingEnabled;
		}
		set
		{
			m_CullingEnabled = value;
			IncrementVersion(VersionChangeType.Repaint);
		}
	}

	private GUILayoutUtility.LayoutCache cache
	{
		get
		{
			if (m_Cache == null)
			{
				m_Cache = new GUILayoutUtility.LayoutCache();
			}
			return m_Cache;
		}
	}

	private float layoutMeasuredWidth => Mathf.Ceil(cache.topLevel.maxWidth);

	private float layoutMeasuredHeight => Mathf.Ceil(cache.topLevel.maxHeight);

	public ContextType contextType { get; set; }

	internal bool focusOnlyIfHasFocusableControls { get; set; } = true;

	public override bool canGrabFocus => (!focusOnlyIfHasFocusableControls) ? base.canGrabFocus : (hasFocusableControls && base.canGrabFocus);

	static IMGUIContainer()
	{
		ussClassName = "unity-imgui-container";
		ussFoldoutChildDepthClassName = Foldout.ussClassName + "__" + ussClassName + "--depth-";
		k_OnGUIMarker = new ProfilerMarker("OnGUI");
		k_ImmediateCallbackMarker = new ProfilerMarker("IMGUIContainer");
		s_DefaultMeasureEvent = new Event
		{
			type = EventType.Layout
		};
		s_MeasureEvent = new Event
		{
			type = EventType.Layout
		};
		s_CurrentEvent = new Event
		{
			type = EventType.Layout
		};
		ussFoldoutChildDepthClassNames = new List<string>(Foldout.ussFoldoutMaxDepth + 1);
		for (int i = 0; i <= Foldout.ussFoldoutMaxDepth; i++)
		{
			ussFoldoutChildDepthClassNames.Add(ussFoldoutChildDepthClassName + i);
		}
		ussFoldoutChildDepthClassNames.Add(ussFoldoutChildDepthClassName + "max");
	}

	public IMGUIContainer()
		: this(null)
	{
	}

	public IMGUIContainer(Action onGUIHandler)
	{
		isIMGUIContainer = true;
		base.eventCallbackCategories |= 90166;
		AddToClassList(ussClassName);
		this.onGUIHandler = onGUIHandler;
		contextType = ContextType.Editor;
		base.focusable = true;
		base.requireMeasureFunction = true;
		base.generateVisualContent = (Action<MeshGenerationContext>)Delegate.Combine(base.generateVisualContent, new Action<MeshGenerationContext>(OnGenerateVisualContent));
	}

	private void OnGenerateVisualContent(MeshGenerationContext mgc)
	{
		lastWorldClip = base.elementPanel.repaintData.currentWorldClip;
		mgc.painter.DrawImmediate(DoIMGUIRepaint, cullingEnabled);
	}

	private void SaveGlobals()
	{
		m_GUIGlobals.matrix = GUI.matrix;
		m_GUIGlobals.color = GUI.color;
		m_GUIGlobals.contentColor = GUI.contentColor;
		m_GUIGlobals.backgroundColor = GUI.backgroundColor;
		m_GUIGlobals.enabled = GUI.enabled;
		m_GUIGlobals.changed = GUI.changed;
		if (Event.current != null)
		{
			m_GUIGlobals.displayIndex = Event.current.displayIndex;
		}
	}

	private void RestoreGlobals()
	{
		GUI.matrix = m_GUIGlobals.matrix;
		GUI.color = m_GUIGlobals.color;
		GUI.contentColor = m_GUIGlobals.contentColor;
		GUI.backgroundColor = m_GUIGlobals.backgroundColor;
		GUI.enabled = m_GUIGlobals.enabled;
		GUI.changed = m_GUIGlobals.changed;
		if (Event.current != null)
		{
			Event.current.displayIndex = m_GUIGlobals.displayIndex;
		}
	}

	private void DoOnGUI(Event evt, Matrix4x4 parentTransform, Rect clippingRect, bool isComputingLayout, Rect layoutSize, Action onGUIHandler, bool canAffectFocus = true)
	{
		if (onGUIHandler == null || base.panel == null)
		{
			return;
		}
		int num = GUIClip.Internal_GetCount();
		SaveGlobals();
		float a = layoutMeasuredWidth;
		float a2 = layoutMeasuredHeight;
		UIElementsUtility.BeginContainerGUI(cache, evt, this);
		GUI.color = UIElementsUtility.editorPlayModeTintColor;
		if (Event.current.type != EventType.Layout)
		{
			if (lostFocus)
			{
				if (focusController != null && GUIUtility.OwnsId(GUIUtility.keyboardControl))
				{
					GUIUtility.keyboardControl = 0;
					focusController.imguiKeyboardControl = 0;
				}
				lostFocus = false;
			}
			if (receivedFocus)
			{
				if (hasFocusableControls)
				{
					if (focusChangeDirection != FocusChangeDirection.unspecified && focusChangeDirection != FocusChangeDirection.none)
					{
						if (Event.current.type == EventType.KeyDown)
						{
							char character = Event.current.character;
							if (character == '\t' || character == '\u0019')
							{
								Event.current.Use();
							}
						}
						if (focusChangeDirection == VisualElementFocusChangeDirection.left)
						{
							GUIUtility.SetKeyboardControlToLastControlId();
						}
						else if (focusChangeDirection == VisualElementFocusChangeDirection.right)
						{
							GUIUtility.SetKeyboardControlToFirstControlId();
						}
					}
					else if (GUIUtility.keyboardControl == 0 && m_IsFocusDelegated)
					{
						GUIUtility.SetKeyboardControlToFirstControlId();
					}
				}
				if (focusController != null)
				{
					if (focusController.imguiKeyboardControl != GUIUtility.keyboardControl && focusChangeDirection != FocusChangeDirection.unspecified)
					{
						newKeyboardFocusControlID = GUIUtility.keyboardControl;
					}
					focusController.imguiKeyboardControl = GUIUtility.keyboardControl;
				}
				receivedFocus = false;
				focusChangeDirection = FocusChangeDirection.unspecified;
			}
		}
		EventType type = Event.current.type;
		bool flag = false;
		try
		{
			using (new GUIClip.ParentClipScope(parentTransform, clippingRect))
			{
				using (k_OnGUIMarker.Auto())
				{
					onGUIHandler();
				}
			}
		}
		catch (Exception exception)
		{
			if (type != EventType.Layout)
			{
				throw;
			}
			flag = GUIUtility.IsExitGUIException(exception);
			if (!flag)
			{
				Debug.LogException(exception);
			}
		}
		finally
		{
			if (Event.current.type != EventType.Layout && canAffectFocus)
			{
				bool flag2 = Event.current.type == EventType.Used;
				int keyboardControl = GUIUtility.keyboardControl;
				int num2 = GUIUtility.CheckForTabEvent(Event.current);
				if (focusController != null)
				{
					if (num2 < 0 && !flag2)
					{
						Focusable leafFocusedElement = focusController.GetLeafFocusedElement();
						Focusable focusable = focusController.FocusNextInDirection((num2 == -1) ? VisualElementFocusChangeDirection.right : VisualElementFocusChangeDirection.left);
						if (leafFocusedElement == this)
						{
							if (focusable == this)
							{
								switch (num2)
								{
								case -2:
									GUIUtility.SetKeyboardControlToLastControlId();
									break;
								case -1:
									GUIUtility.SetKeyboardControlToFirstControlId();
									break;
								}
								newKeyboardFocusControlID = GUIUtility.keyboardControl;
								focusController.imguiKeyboardControl = GUIUtility.keyboardControl;
							}
							else
							{
								GUIUtility.keyboardControl = 0;
								focusController.imguiKeyboardControl = 0;
							}
						}
					}
					else if (num2 > 0 && !flag2)
					{
						focusController.imguiKeyboardControl = GUIUtility.keyboardControl;
						newKeyboardFocusControlID = GUIUtility.keyboardControl;
					}
					else if (num2 == 0)
					{
						if (type == EventType.MouseDown && !focusOnlyIfHasFocusableControls)
						{
							focusController.SyncIMGUIFocus(GUIUtility.keyboardControl, this, forceSwitch: true);
						}
						else if (keyboardControl != GUIUtility.keyboardControl || type == EventType.MouseDown)
						{
							focusController.SyncIMGUIFocus(GUIUtility.keyboardControl, this, forceSwitch: false);
						}
						else if (GUIUtility.keyboardControl != focusController.imguiKeyboardControl)
						{
							newKeyboardFocusControlID = GUIUtility.keyboardControl;
							if (focusController.GetLeafFocusedElement() == this)
							{
								focusController.imguiKeyboardControl = GUIUtility.keyboardControl;
							}
							else
							{
								focusController.SyncIMGUIFocus(GUIUtility.keyboardControl, this, forceSwitch: false);
							}
						}
					}
				}
				hasFocusableControls = GUIUtility.HasFocusableControls();
			}
		}
		UIElementsUtility.EndContainerGUI(evt, layoutSize);
		RestoreGlobals();
		if (evt.type == EventType.Layout && (!Mathf.Approximately(a, layoutMeasuredWidth) || !Mathf.Approximately(a2, layoutMeasuredHeight)))
		{
			if (isComputingLayout && clippingRect == Rect.zero)
			{
				base.schedule.Execute((Action)delegate
				{
					IncrementVersion(VersionChangeType.Layout);
				});
			}
			else
			{
				IncrementVersion(VersionChangeType.Layout);
			}
		}
		if (!flag && evt.type != EventType.Ignore && evt.type != EventType.Used)
		{
			int num3 = GUIClip.Internal_GetCount();
			if (num3 > num)
			{
				Debug.LogError("GUI Error: You are pushing more GUIClips than you are popping. Make sure they are balanced.");
			}
			else if (num3 < num)
			{
				Debug.LogError("GUI Error: You are popping more GUIClips than you are pushing. Make sure they are balanced.");
			}
		}
		while (GUIClip.Internal_GetCount() > num)
		{
			GUIClip.Internal_Pop();
		}
		if (evt.type == EventType.Used)
		{
			IncrementVersion(VersionChangeType.Repaint);
		}
	}

	public void MarkDirtyLayout()
	{
		m_RefreshCachedLayout = true;
		IncrementVersion(VersionChangeType.Layout);
	}

	internal void ProcessEvent(EventBase evt)
	{
		if ((evt.imguiEvent != null && SendEventToIMGUI(evt)) || evt.eventTypeId == EventBase<NavigationMoveEvent>.TypeId() || evt.eventTypeId == EventBase<NavigationSubmitEvent>.TypeId() || evt.eventTypeId == EventBase<NavigationCancelEvent>.TypeId())
		{
			evt.StopPropagation();
			evt.PreventDefault();
		}
	}

	private void DoIMGUIRepaint()
	{
		using (k_ImmediateCallbackMarker.Auto())
		{
			Matrix4x4 currentOffset = base.elementPanel.repaintData.currentOffset;
			m_CachedClippingRect = VisualElement.ComputeAAAlignedBound(base.worldClip, currentOffset);
			m_CachedTransform = currentOffset * base.worldTransform;
			HandleIMGUIEvent(base.elementPanel.repaintData.repaintEvent, m_CachedTransform, m_CachedClippingRect, onGUIHandler, canAffectFocus: true);
		}
	}

	internal bool SendEventToIMGUI(EventBase evt, bool canAffectFocus = true, bool verifyBounds = true)
	{
		if (evt is IPointerEvent)
		{
			if (evt.imguiEvent != null && evt.imguiEvent.isDirectManipulationDevice)
			{
				bool flag = false;
				EventType rawType = evt.imguiEvent.rawType;
				if (evt is PointerDownEvent)
				{
					flag = true;
					evt.imguiEvent.type = EventType.TouchDown;
				}
				else if (evt is PointerUpEvent)
				{
					flag = true;
					evt.imguiEvent.type = EventType.TouchUp;
				}
				else if (evt is PointerMoveEvent && evt.imguiEvent.rawType == EventType.MouseDrag)
				{
					flag = true;
					evt.imguiEvent.type = EventType.TouchMove;
				}
				else if (evt is PointerLeaveEvent)
				{
					flag = true;
					evt.imguiEvent.type = EventType.TouchLeave;
				}
				else if (evt is PointerEnterEvent)
				{
					flag = true;
					evt.imguiEvent.type = EventType.TouchEnter;
				}
				else if (evt is PointerStationaryEvent)
				{
					flag = true;
					evt.imguiEvent.type = EventType.TouchStationary;
				}
				if (flag)
				{
					bool result = SendEventToIMGUIRaw(evt, canAffectFocus, verifyBounds);
					evt.imguiEvent.type = rawType;
					return result;
				}
			}
			return false;
		}
		return SendEventToIMGUIRaw(evt, canAffectFocus, verifyBounds);
	}

	private bool SendEventToIMGUIRaw(EventBase evt, bool canAffectFocus, bool verifyBounds)
	{
		if (verifyBounds && !VerifyBounds(evt))
		{
			return false;
		}
		bool result;
		using (new EventDebuggerLogIMGUICall(evt))
		{
			result = HandleIMGUIEvent(evt.imguiEvent, canAffectFocus);
		}
		return result;
	}

	private bool VerifyBounds(EventBase evt)
	{
		return IsContainerCapturingTheMouse() || !IsLocalEvent(evt) || IsEventInsideLocalWindow(evt) || IsDockAreaMouseUp(evt);
	}

	private bool IsContainerCapturingTheMouse()
	{
		return this == base.panel?.dispatcher?.pointerState.GetCapturingElement(PointerId.mousePointerId);
	}

	private bool IsLocalEvent(EventBase evt)
	{
		long eventTypeId = evt.eventTypeId;
		return eventTypeId == EventBase<MouseDownEvent>.TypeId() || eventTypeId == EventBase<MouseUpEvent>.TypeId() || eventTypeId == EventBase<MouseMoveEvent>.TypeId() || eventTypeId == EventBase<PointerDownEvent>.TypeId() || eventTypeId == EventBase<PointerUpEvent>.TypeId() || eventTypeId == EventBase<PointerMoveEvent>.TypeId();
	}

	private bool IsEventInsideLocalWindow(EventBase evt)
	{
		Rect currentClipRect = GetCurrentClipRect();
		string text = (evt as IPointerEvent)?.pointerType;
		bool isDirectManipulationDevice = text == PointerType.touch || text == PointerType.pen;
		return GUIUtility.HitTest(currentClipRect, evt.originalMousePosition, isDirectManipulationDevice);
	}

	private static bool IsDockAreaMouseUp(EventBase evt)
	{
		return evt.eventTypeId == EventBase<MouseUpEvent>.TypeId() && evt.target == (evt.target as VisualElement)?.elementPanel.rootIMGUIContainer;
	}

	private bool HandleIMGUIEvent(Event e, bool canAffectFocus)
	{
		return HandleIMGUIEvent(e, onGUIHandler, canAffectFocus);
	}

	internal bool HandleIMGUIEvent(Event e, Action onGUIHandler, bool canAffectFocus)
	{
		GetCurrentTransformAndClip(this, e, out m_CachedTransform, out m_CachedClippingRect);
		return HandleIMGUIEvent(e, m_CachedTransform, m_CachedClippingRect, onGUIHandler, canAffectFocus);
	}

	private bool HandleIMGUIEvent(Event e, Matrix4x4 worldTransform, Rect clippingRect, Action onGUIHandler, bool canAffectFocus)
	{
		if (e == null || onGUIHandler == null || base.elementPanel == null || !base.elementPanel.IMGUIEventInterests.WantsEvent(e.rawType))
		{
			return false;
		}
		EventType rawType = e.rawType;
		if (rawType != EventType.Layout)
		{
			if (m_RefreshCachedLayout || base.elementPanel.IMGUIEventInterests.WantsLayoutPass(e.rawType))
			{
				e.type = EventType.Layout;
				DoOnGUI(e, worldTransform, clippingRect, isComputingLayout: false, base.layout, onGUIHandler, canAffectFocus);
				m_RefreshCachedLayout = false;
				e.type = rawType;
			}
			else
			{
				cache.ResetCursor();
			}
		}
		DoOnGUI(e, worldTransform, clippingRect, isComputingLayout: false, base.layout, onGUIHandler, canAffectFocus);
		if (newKeyboardFocusControlID > 0)
		{
			newKeyboardFocusControlID = 0;
			Event e2 = new Event
			{
				type = EventType.ExecuteCommand,
				commandName = "NewKeyboardFocus"
			};
			HandleIMGUIEvent(e2, canAffectFocus: true);
		}
		if (e.rawType == EventType.Used)
		{
			return true;
		}
		if (e.rawType == EventType.MouseUp && this.HasMouseCapture())
		{
			GUIUtility.hotControl = 0;
		}
		if (base.elementPanel == null)
		{
			GUIUtility.ExitGUI();
		}
		return false;
	}

	[EventInterest(new Type[]
	{
		typeof(BlurEvent),
		typeof(FocusEvent),
		typeof(DetachFromPanelEvent),
		typeof(AttachToPanelEvent)
	})]
	protected override void ExecuteDefaultAction(EventBase evt)
	{
		if (evt == null)
		{
			return;
		}
		if (evt.eventTypeId == EventBase<BlurEvent>.TypeId())
		{
			lostFocus = true;
			IncrementVersion(VersionChangeType.Repaint);
		}
		else if (evt.eventTypeId == EventBase<FocusEvent>.TypeId())
		{
			FocusEvent focusEvent = evt as FocusEvent;
			receivedFocus = true;
			focusChangeDirection = focusEvent.direction;
			m_IsFocusDelegated = focusEvent.IsFocusDelegated;
		}
		else if (evt.eventTypeId == EventBase<DetachFromPanelEvent>.TypeId())
		{
			if (base.elementPanel != null)
			{
				base.elementPanel.IMGUIContainersCount--;
			}
		}
		else if (evt.eventTypeId == EventBase<AttachToPanelEvent>.TypeId() && base.elementPanel != null)
		{
			base.elementPanel.IMGUIContainersCount++;
			SetFoldoutDepthClass();
		}
	}

	private void SetFoldoutDepthClass()
	{
		for (int i = 0; i < ussFoldoutChildDepthClassNames.Count; i++)
		{
			RemoveFromClassList(ussFoldoutChildDepthClassNames[i]);
		}
		int foldoutDepth = this.GetFoldoutDepth();
		if (foldoutDepth != 0)
		{
			foldoutDepth = Mathf.Min(foldoutDepth, ussFoldoutChildDepthClassNames.Count - 1);
			AddToClassList(ussFoldoutChildDepthClassNames[foldoutDepth]);
		}
	}

	protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight, MeasureMode heightMode)
	{
		float num = float.NaN;
		float num2 = float.NaN;
		bool flag = false;
		if (widthMode != MeasureMode.Exactly || heightMode != MeasureMode.Exactly)
		{
			if (Event.current != null)
			{
				s_CurrentEvent.CopyFrom(Event.current);
				flag = true;
			}
			s_MeasureEvent.CopyFrom(s_DefaultMeasureEvent);
			Rect layoutSize = base.layout;
			if (widthMode == MeasureMode.Exactly)
			{
				layoutSize.width = desiredWidth;
			}
			if (heightMode == MeasureMode.Exactly)
			{
				layoutSize.height = desiredHeight;
			}
			DoOnGUI(s_MeasureEvent, m_CachedTransform, m_CachedClippingRect, isComputingLayout: true, layoutSize, onGUIHandler);
			num = layoutMeasuredWidth;
			num2 = layoutMeasuredHeight;
			if (flag)
			{
				Event.current.CopyFrom(s_CurrentEvent);
			}
		}
		switch (widthMode)
		{
		case MeasureMode.Exactly:
			num = desiredWidth;
			break;
		case MeasureMode.AtMost:
			num = Mathf.Min(num, desiredWidth);
			break;
		}
		switch (heightMode)
		{
		case MeasureMode.Exactly:
			num2 = desiredHeight;
			break;
		case MeasureMode.AtMost:
			num2 = Mathf.Min(num2, desiredHeight);
			break;
		}
		return new Vector2(num, num2);
	}

	private Rect GetCurrentClipRect()
	{
		Rect result = lastWorldClip;
		if (result.width == 0f || result.height == 0f)
		{
			result = base.worldBound;
		}
		return result;
	}

	private static void GetCurrentTransformAndClip(IMGUIContainer container, Event evt, out Matrix4x4 transform, out Rect clipRect)
	{
		clipRect = container.GetCurrentClipRect();
		transform = container.worldTransform;
		if (evt != null && evt.rawType == EventType.Repaint && container.elementPanel != null)
		{
			transform = container.elementPanel.repaintData.currentOffset * container.worldTransform;
		}
	}

	public void Dispose()
	{
		Dispose(disposeManaged: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposeManaged)
	{
		if (disposeManaged)
		{
			m_ObjectGUIState?.Dispose();
		}
	}
}
