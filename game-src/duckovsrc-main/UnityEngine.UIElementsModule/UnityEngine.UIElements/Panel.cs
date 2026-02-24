using System;
using System.Collections.Generic;
using Unity.Profiling;

namespace UnityEngine.UIElements;

internal class Panel : BaseVisualElementPanel
{
	private VisualElement m_RootContainer;

	private VisualTreeUpdater m_VisualTreeUpdater;

	private IStylePropertyAnimationSystem m_StylePropertyAnimationSystem;

	private string m_PanelName;

	private uint m_Version = 0u;

	private uint m_RepaintVersion = 0u;

	private uint m_HierarchyVersion = 0u;

	private ProfilerMarker m_MarkerBeforeUpdate;

	private ProfilerMarker m_MarkerUpdate;

	private ProfilerMarker m_MarkerLayout;

	private ProfilerMarker m_MarkerBindings;

	private ProfilerMarker m_MarkerAnimations;

	private static ProfilerMarker s_MarkerPickAll = new ProfilerMarker("Panel.PickAll");

	private TimerEventScheduler m_Scheduler;

	private bool m_JustReceivedFocus;

	private Shader m_StandardShader;

	private AtlasBase m_Atlas;

	private bool m_ValidatingLayout = false;

	public sealed override VisualElement visualTree => m_RootContainer;

	public sealed override EventDispatcher dispatcher { get; set; }

	public TimerEventScheduler timerEventScheduler => m_Scheduler ?? (m_Scheduler = new TimerEventScheduler());

	internal override IScheduler scheduler => timerEventScheduler;

	internal VisualTreeUpdater visualTreeUpdater => m_VisualTreeUpdater;

	internal override IStylePropertyAnimationSystem styleAnimationSystem
	{
		get
		{
			return m_StylePropertyAnimationSystem;
		}
		set
		{
			if (m_StylePropertyAnimationSystem != value)
			{
				m_StylePropertyAnimationSystem?.CancelAllAnimations();
				m_StylePropertyAnimationSystem = value;
			}
		}
	}

	public override ScriptableObject ownerObject { get; protected set; }

	public override ContextType contextType { get; protected set; }

	public override SavePersistentViewData saveViewData { get; set; }

	public override GetViewDataDictionary getViewDataDictionary { get; set; }

	public sealed override FocusController focusController { get; set; }

	public override EventInterests IMGUIEventInterests { get; set; }

	internal static LoadResourceFunction loadResourceFunc { private get; set; }

	internal string name
	{
		get
		{
			return m_PanelName;
		}
		set
		{
			m_PanelName = value;
			CreateMarkers();
		}
	}

	internal static TimeMsFunction TimeSinceStartup { private get; set; }

	public override int IMGUIContainersCount { get; set; }

	public override IMGUIContainer rootIMGUIContainer { get; set; }

	internal override uint version => m_Version;

	internal override uint repaintVersion => m_RepaintVersion;

	internal override uint hierarchyVersion => m_HierarchyVersion;

	internal override Shader standardShader
	{
		get
		{
			return m_StandardShader;
		}
		set
		{
			if (m_StandardShader != value)
			{
				m_StandardShader = value;
				InvokeStandardShaderChanged();
			}
		}
	}

	public override AtlasBase atlas
	{
		get
		{
			return m_Atlas;
		}
		set
		{
			if (m_Atlas != value)
			{
				m_Atlas?.InvokeRemovedFromPanel(this);
				m_Atlas = value;
				InvokeAtlasChanged();
				m_Atlas?.InvokeAssignedToPanel(this);
			}
		}
	}

	internal static event Action<Panel> beforeAnyRepaint;

	internal static Object LoadResource(string pathName, Type type, float dpiScaling)
	{
		Object obj = null;
		if (loadResourceFunc != null)
		{
			return loadResourceFunc(pathName, type, dpiScaling);
		}
		return Resources.Load(pathName, type);
	}

	internal void Focus()
	{
		m_JustReceivedFocus = true;
	}

	internal void Blur()
	{
		focusController?.BlurLastFocusedElement();
	}

	public override void ValidateFocus()
	{
		if (m_JustReceivedFocus)
		{
			m_JustReceivedFocus = false;
			focusController?.SetFocusToLastFocusedElement();
		}
	}

	private void CreateMarkers()
	{
		if (!string.IsNullOrEmpty(m_PanelName))
		{
			m_MarkerBeforeUpdate = new ProfilerMarker("Panel.BeforeUpdate." + m_PanelName);
			m_MarkerUpdate = new ProfilerMarker("Panel.Update." + m_PanelName);
			m_MarkerLayout = new ProfilerMarker("Panel.Layout." + m_PanelName);
			m_MarkerBindings = new ProfilerMarker("Panel.Bindings." + m_PanelName);
			m_MarkerAnimations = new ProfilerMarker("Panel.Animations." + m_PanelName);
		}
		else
		{
			m_MarkerBeforeUpdate = new ProfilerMarker("Panel.BeforeUpdate");
			m_MarkerUpdate = new ProfilerMarker("Panel.Update");
			m_MarkerLayout = new ProfilerMarker("Panel.Layout");
			m_MarkerBindings = new ProfilerMarker("Panel.Bindings");
			m_MarkerAnimations = new ProfilerMarker("Panel.Animations");
		}
	}

	internal static Panel CreateEditorPanel(ScriptableObject ownerObject)
	{
		return new Panel(ownerObject, ContextType.Editor, EventDispatcher.CreateDefault());
	}

	public Panel(ScriptableObject ownerObject, ContextType contextType, EventDispatcher dispatcher)
	{
		this.ownerObject = ownerObject;
		this.contextType = contextType;
		this.dispatcher = dispatcher;
		repaintData = new RepaintData();
		cursorManager = new CursorManager();
		base.contextualMenuManager = null;
		m_VisualTreeUpdater = new VisualTreeUpdater(this);
		m_RootContainer = new VisualElement
		{
			name = VisualElementUtils.GetUniqueName("unity-panel-container"),
			viewDataKey = "PanelContainer",
			pickingMode = ((contextType != ContextType.Editor) ? PickingMode.Ignore : PickingMode.Position),
			eventCallbackCategories = int.MinValue
		};
		visualTree.SetPanel(this);
		focusController = new FocusController(new VisualElementFocusRing(visualTree));
		styleAnimationSystem = new StylePropertyAnimationSystem();
		CreateMarkers();
		InvokeHierarchyChanged(visualTree, HierarchyChangeType.Add);
		atlas = new DynamicAtlas();
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.disposed)
		{
			if (disposing)
			{
				atlas = null;
				m_VisualTreeUpdater.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	public static long TimeSinceStartupMs()
	{
		return TimeSinceStartup?.Invoke() ?? DefaultTimeSinceStartupMs();
	}

	internal static long DefaultTimeSinceStartupMs()
	{
		return (long)(Time.realtimeSinceStartup * 1000f);
	}

	internal static VisualElement PickAllWithoutValidatingLayout(VisualElement root, Vector2 point)
	{
		return PickAll(root, point);
	}

	private static VisualElement PickAll(VisualElement root, Vector2 point, List<VisualElement> picked = null, bool includeIgnoredElement = false)
	{
		return PerformPick(root, point, picked, includeIgnoredElement);
	}

	private static VisualElement PerformPick(VisualElement root, Vector2 point, List<VisualElement> picked = null, bool includeIgnoredElement = false)
	{
		if (root.resolvedStyle.display == DisplayStyle.None)
		{
			return null;
		}
		if (root.pickingMode == PickingMode.Ignore && root.hierarchy.childCount == 0 && !includeIgnoredElement)
		{
			return null;
		}
		if (!root.worldBoundingBox.Contains(point))
		{
			return null;
		}
		Vector2 localPoint = root.WorldToLocal(point);
		bool flag = root.ContainsPoint(localPoint);
		if (!flag && root.ShouldClip())
		{
			return null;
		}
		VisualElement visualElement = null;
		int childCount = root.hierarchy.childCount;
		for (int num = childCount - 1; num >= 0; num--)
		{
			VisualElement root2 = root.hierarchy[num];
			VisualElement visualElement2 = PerformPick(root2, point, picked, includeIgnoredElement);
			if (visualElement == null && visualElement2 != null)
			{
				if (picked == null)
				{
					return visualElement2;
				}
				visualElement = visualElement2;
			}
		}
		if (root.visible && (root.pickingMode == PickingMode.Position || includeIgnoredElement) && flag)
		{
			picked?.Add(root);
			if (visualElement == null)
			{
				visualElement = root;
			}
		}
		return visualElement;
	}

	public override VisualElement PickAll(Vector2 point, List<VisualElement> picked)
	{
		ValidateLayout();
		picked?.Clear();
		return PickAll(visualTree, point, picked);
	}

	public override VisualElement Pick(Vector2 point)
	{
		ValidateLayout();
		Vector2 pickPosition;
		bool isTemporary;
		VisualElement topElementUnderPointer = m_TopElementUnderPointers.GetTopElementUnderPointer(PointerId.mousePointerId, out pickPosition, out isTemporary);
		if (!isTemporary && PixelOf(pickPosition) == PixelOf(point))
		{
			return topElementUnderPointer;
		}
		return PickAll(visualTree, point);
		static Vector2Int PixelOf(Vector2 p)
		{
			return Vector2Int.FloorToInt(p);
		}
	}

	public override void ValidateLayout()
	{
		if (!m_ValidatingLayout)
		{
			m_ValidatingLayout = true;
			m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Styles);
			m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Layout);
			m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.TransformClip);
			m_ValidatingLayout = false;
		}
	}

	public override void UpdateAnimations()
	{
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Animation);
	}

	public override void UpdateBindings()
	{
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Bindings);
	}

	public override void ApplyStyles()
	{
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Styles);
	}

	private void UpdateForRepaint()
	{
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.ViewData);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Styles);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Layout);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.TransformClip);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Repaint);
	}

	internal void UpdateWithoutRepaint()
	{
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.ViewData);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Bindings);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Animation);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Styles);
		m_VisualTreeUpdater.UpdateVisualTreePhase(VisualTreeUpdatePhase.Layout);
	}

	public override void Repaint(Event e)
	{
		m_RepaintVersion = version;
		if (contextType == ContextType.Editor)
		{
			base.pixelsPerPoint = GUIUtility.pixelsPerPoint;
		}
		repaintData.repaintEvent = e;
		using (m_MarkerBeforeUpdate.Auto())
		{
			InvokeBeforeUpdate();
		}
		Panel.beforeAnyRepaint?.Invoke(this);
		using (m_MarkerUpdate.Auto())
		{
			UpdateForRepaint();
		}
	}

	internal override void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
	{
		m_Version++;
		m_VisualTreeUpdater.OnVersionChanged(ve, versionChangeType);
		if ((versionChangeType & VersionChangeType.Hierarchy) == VersionChangeType.Hierarchy)
		{
			m_HierarchyVersion++;
		}
	}

	internal override void SetUpdater(IVisualTreeUpdater updater, VisualTreeUpdatePhase phase)
	{
		m_VisualTreeUpdater.SetUpdater(updater, phase);
	}

	internal override IVisualTreeUpdater GetUpdater(VisualTreeUpdatePhase phase)
	{
		return m_VisualTreeUpdater.GetUpdater(phase);
	}
}
