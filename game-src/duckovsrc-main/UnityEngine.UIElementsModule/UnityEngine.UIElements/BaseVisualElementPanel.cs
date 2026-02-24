#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using UnityEngine.Yoga;

namespace UnityEngine.UIElements;

internal abstract class BaseVisualElementPanel : IPanel, IDisposable, IGroupBox
{
	private UIElementsBridge m_UIElementsBridge;

	private float m_Scale = 1f;

	internal YogaConfig yogaConfig;

	private float m_PixelsPerPoint = 1f;

	internal ElementUnderPointer m_TopElementUnderPointers = new ElementUnderPointer();

	public abstract EventInterests IMGUIEventInterests { get; set; }

	public abstract ScriptableObject ownerObject { get; protected set; }

	public abstract SavePersistentViewData saveViewData { get; set; }

	public abstract GetViewDataDictionary getViewDataDictionary { get; set; }

	public abstract int IMGUIContainersCount { get; set; }

	public abstract FocusController focusController { get; set; }

	public abstract IMGUIContainer rootIMGUIContainer { get; set; }

	internal UIElementsBridge uiElementsBridge
	{
		get
		{
			if (m_UIElementsBridge != null)
			{
				return m_UIElementsBridge;
			}
			throw new Exception("Panel has no UIElementsBridge.");
		}
		set
		{
			m_UIElementsBridge = value;
		}
	}

	internal float scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			if (!Mathf.Approximately(m_Scale, value))
			{
				m_Scale = value;
				visualTree.IncrementVersion(VersionChangeType.Layout);
				yogaConfig.PointScaleFactor = scaledPixelsPerPoint;
				visualTree.IncrementVersion(VersionChangeType.StyleSheet);
			}
		}
	}

	internal float pixelsPerPoint
	{
		get
		{
			return m_PixelsPerPoint;
		}
		set
		{
			if (!Mathf.Approximately(m_PixelsPerPoint, value))
			{
				m_PixelsPerPoint = value;
				visualTree.IncrementVersion(VersionChangeType.Layout);
				yogaConfig.PointScaleFactor = scaledPixelsPerPoint;
				visualTree.IncrementVersion(VersionChangeType.StyleSheet);
			}
		}
	}

	public float scaledPixelsPerPoint => m_PixelsPerPoint * m_Scale;

	public float referenceSpritePixelsPerUnit { get; set; } = 100f;

	public PanelClearFlags clearFlags
	{
		get
		{
			PanelClearFlags panelClearFlags = PanelClearFlags.None;
			if (clearSettings.clearColor)
			{
				panelClearFlags |= PanelClearFlags.Color;
			}
			if (clearSettings.clearDepthStencil)
			{
				panelClearFlags |= PanelClearFlags.Depth;
			}
			return panelClearFlags;
		}
		set
		{
			PanelClearSettings panelClearSettings = clearSettings;
			panelClearSettings.clearColor = (value & PanelClearFlags.Color) == PanelClearFlags.Color;
			panelClearSettings.clearDepthStencil = (value & PanelClearFlags.Depth) == PanelClearFlags.Depth;
			clearSettings = panelClearSettings;
		}
	}

	internal PanelClearSettings clearSettings { get; set; } = new PanelClearSettings
	{
		clearDepthStencil = true,
		clearColor = true,
		color = Color.clear
	};

	internal bool duringLayoutPhase { get; set; }

	public bool isDirty => version != repaintVersion;

	internal abstract uint version { get; }

	internal abstract uint repaintVersion { get; }

	internal abstract uint hierarchyVersion { get; }

	internal virtual RepaintData repaintData { get; set; }

	internal virtual ICursorManager cursorManager { get; set; }

	public ContextualMenuManager contextualMenuManager { get; internal set; }

	public abstract VisualElement visualTree { get; }

	public abstract EventDispatcher dispatcher { get; set; }

	internal abstract IScheduler scheduler { get; }

	internal abstract IStylePropertyAnimationSystem styleAnimationSystem { get; set; }

	public abstract ContextType contextType { get; protected set; }

	internal bool disposed { get; private set; }

	internal abstract Shader standardShader { get; set; }

	internal virtual Shader standardWorldSpaceShader
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public abstract AtlasBase atlas { get; set; }

	internal event Action<BaseVisualElementPanel> panelDisposed;

	internal event Action standardShaderChanged;

	internal event Action standardWorldSpaceShaderChanged;

	internal event Action atlasChanged;

	internal event Action<Material> updateMaterial;

	internal event HierarchyEvent hierarchyChanged;

	internal event Action<IPanel> beforeUpdate;

	protected BaseVisualElementPanel()
	{
		yogaConfig = new YogaConfig();
		yogaConfig.UseWebDefaults = YogaConfig.Default.UseWebDefaults;
		m_UIElementsBridge = new RuntimeUIElementsBridge();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			if (ownerObject != null)
			{
				UIElementsUtility.RemoveCachedPanel(ownerObject.GetInstanceID());
			}
			PointerDeviceState.RemovePanelData(this);
		}
		this.panelDisposed?.Invoke(this);
		yogaConfig = null;
		disposed = true;
	}

	public abstract void Repaint(Event e);

	public abstract void ValidateFocus();

	public abstract void ValidateLayout();

	public abstract void UpdateAnimations();

	public abstract void UpdateBindings();

	public abstract void ApplyStyles();

	internal abstract void OnVersionChanged(VisualElement ele, VersionChangeType changeTypeFlag);

	internal abstract void SetUpdater(IVisualTreeUpdater updater, VisualTreeUpdatePhase phase);

	internal void SendEvent(EventBase e, DispatchMode dispatchMode = DispatchMode.Default)
	{
		Debug.Assert(dispatcher != null);
		dispatcher?.Dispatch(e, this, dispatchMode);
	}

	public abstract VisualElement Pick(Vector2 point);

	public abstract VisualElement PickAll(Vector2 point, List<VisualElement> picked);

	internal abstract IVisualTreeUpdater GetUpdater(VisualTreeUpdatePhase phase);

	internal VisualElement GetTopElementUnderPointer(int pointerId)
	{
		return m_TopElementUnderPointers.GetTopElementUnderPointer(pointerId);
	}

	internal VisualElement RecomputeTopElementUnderPointer(int pointerId, Vector2 pointerPos, EventBase triggerEvent)
	{
		VisualElement visualElement = null;
		if (PointerDeviceState.GetPanel(pointerId, contextType) == this && !PointerDeviceState.HasLocationFlag(pointerId, contextType, PointerDeviceState.LocationFlag.OutsidePanel))
		{
			visualElement = Pick(pointerPos);
		}
		m_TopElementUnderPointers.SetElementUnderPointer(visualElement, pointerId, triggerEvent);
		return visualElement;
	}

	internal void ClearCachedElementUnderPointer(int pointerId, EventBase triggerEvent)
	{
		m_TopElementUnderPointers.SetTemporaryElementUnderPointer(null, pointerId, triggerEvent);
	}

	internal void CommitElementUnderPointers()
	{
		m_TopElementUnderPointers.CommitElementUnderPointers(dispatcher, contextType);
	}

	protected void InvokeStandardShaderChanged()
	{
		if (this.standardShaderChanged != null)
		{
			this.standardShaderChanged();
		}
	}

	protected void InvokeStandardWorldSpaceShaderChanged()
	{
		if (this.standardWorldSpaceShaderChanged != null)
		{
			this.standardWorldSpaceShaderChanged();
		}
	}

	protected void InvokeAtlasChanged()
	{
		this.atlasChanged?.Invoke();
	}

	internal void InvokeUpdateMaterial(Material mat)
	{
		this.updateMaterial?.Invoke(mat);
	}

	internal void InvokeHierarchyChanged(VisualElement ve, HierarchyChangeType changeType)
	{
		if (this.hierarchyChanged != null)
		{
			this.hierarchyChanged(ve, changeType);
		}
	}

	internal void InvokeBeforeUpdate()
	{
		this.beforeUpdate?.Invoke(this);
	}

	internal void UpdateElementUnderPointers()
	{
		int[] hoveringPointers = PointerId.hoveringPointers;
		foreach (int pointerId in hoveringPointers)
		{
			if (PointerDeviceState.GetPanel(pointerId, contextType) != this || PointerDeviceState.HasLocationFlag(pointerId, contextType, PointerDeviceState.LocationFlag.OutsidePanel))
			{
				m_TopElementUnderPointers.SetElementUnderPointer(null, pointerId, new Vector2(float.MinValue, float.MinValue));
				continue;
			}
			Vector2 pointerPosition = PointerDeviceState.GetPointerPosition(pointerId, contextType);
			VisualElement newElementUnderPointer = PickAll(pointerPosition, null);
			m_TopElementUnderPointers.SetElementUnderPointer(newElementUnderPointer, pointerId, pointerPosition);
		}
		CommitElementUnderPointers();
	}

	void IGroupBox.OnOptionAdded(IGroupBoxOption option)
	{
	}

	void IGroupBox.OnOptionRemoved(IGroupBoxOption option)
	{
	}

	internal virtual IGenericMenu CreateMenu()
	{
		return new GenericDropdownMenu();
	}

	public virtual void Update()
	{
		scheduler.UpdateScheduledEvents();
		ValidateFocus();
		ValidateLayout();
		UpdateAnimations();
		UpdateBindings();
	}
}
