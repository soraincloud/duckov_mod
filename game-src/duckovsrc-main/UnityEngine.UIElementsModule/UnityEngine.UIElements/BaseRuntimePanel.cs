using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

internal abstract class BaseRuntimePanel : Panel
{
	private GameObject m_SelectableGameObject;

	private static int s_CurrentRuntimePanelCounter = 0;

	internal readonly int m_RuntimePanelCreationIndex;

	private float m_SortingPriority = 0f;

	internal int resolvedSortingIndex = 0;

	private Shader m_StandardWorldSpaceShader;

	private bool m_DrawToCameras;

	internal RenderTexture targetTexture = null;

	internal Matrix4x4 panelToWorld = Matrix4x4.identity;

	internal static readonly Func<Vector2, Vector2> DefaultScreenToPanelSpace = (Vector2 p) => p;

	private Func<Vector2, Vector2> m_ScreenToPanelSpace = DefaultScreenToPanelSpace;

	public GameObject selectableGameObject
	{
		get
		{
			return m_SelectableGameObject;
		}
		set
		{
			if (m_SelectableGameObject != value)
			{
				AssignPanelToComponents(null);
				m_SelectableGameObject = value;
				AssignPanelToComponents(this);
			}
		}
	}

	public float sortingPriority
	{
		get
		{
			return m_SortingPriority;
		}
		set
		{
			if (!Mathf.Approximately(m_SortingPriority, value))
			{
				m_SortingPriority = value;
				if (contextType == ContextType.Player)
				{
					UIElementsRuntimeUtility.SetPanelOrderingDirty();
				}
			}
		}
	}

	internal override Shader standardWorldSpaceShader
	{
		get
		{
			return m_StandardWorldSpaceShader;
		}
		set
		{
			if (m_StandardWorldSpaceShader != value)
			{
				m_StandardWorldSpaceShader = value;
				InvokeStandardWorldSpaceShaderChanged();
			}
		}
	}

	internal bool drawToCameras
	{
		get
		{
			return m_DrawToCameras;
		}
		set
		{
			if (m_DrawToCameras != value)
			{
				m_DrawToCameras = value;
				(GetUpdater(VisualTreeUpdatePhase.Repaint) as UIRRepaintUpdater)?.DestroyRenderChain();
			}
		}
	}

	internal int targetDisplay { get; set; }

	internal int screenRenderingWidth => getScreenRenderingWidth(targetDisplay);

	internal int screenRenderingHeight => getScreenRenderingHeight(targetDisplay);

	public Func<Vector2, Vector2> screenToPanelSpace
	{
		get
		{
			return m_ScreenToPanelSpace;
		}
		set
		{
			m_ScreenToPanelSpace = value ?? DefaultScreenToPanelSpace;
		}
	}

	public event Action destroyed;

	protected BaseRuntimePanel(ScriptableObject ownerObject, EventDispatcher dispatcher = null)
		: base(ownerObject, ContextType.Player, dispatcher)
	{
		m_RuntimePanelCreationIndex = s_CurrentRuntimePanelCounter++;
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.disposed)
		{
			if (disposing)
			{
				this.destroyed?.Invoke();
			}
			base.Dispose(disposing);
		}
	}

	internal static int getScreenRenderingHeight(int display)
	{
		return (display >= 0 && display < Display.displays.Length) ? Display.displays[display].renderingHeight : Screen.height;
	}

	internal static int getScreenRenderingWidth(int display)
	{
		return (display >= 0 && display < Display.displays.Length) ? Display.displays[display].renderingWidth : Screen.width;
	}

	public override void Repaint(Event e)
	{
		if (targetTexture == null)
		{
			RenderTexture active = RenderTexture.active;
			int num = ((active != null) ? active.width : screenRenderingWidth);
			int num2 = ((active != null) ? active.height : screenRenderingHeight);
			GL.Viewport(new Rect(0f, 0f, num, num2));
			base.Repaint(e);
		}
		else
		{
			Camera current = Camera.current;
			RenderTexture active2 = RenderTexture.active;
			Camera.SetupCurrent(null);
			RenderTexture.active = targetTexture;
			GL.Viewport(new Rect(0f, 0f, targetTexture.width, targetTexture.height));
			base.Repaint(e);
			Camera.SetupCurrent(current);
			RenderTexture.active = active2;
		}
	}

	internal Vector2 ScreenToPanel(Vector2 screen)
	{
		return screenToPanelSpace(screen) / base.scale;
	}

	internal bool ScreenToPanel(Vector2 screenPosition, Vector2 screenDelta, out Vector2 panelPosition, out Vector2 panelDelta, bool allowOutside = false)
	{
		panelPosition = ScreenToPanel(screenPosition);
		Vector2 vector;
		if (!allowOutside)
		{
			Rect layout = visualTree.layout;
			if (!layout.Contains(panelPosition))
			{
				panelDelta = screenDelta;
				return false;
			}
			vector = ScreenToPanel(screenPosition - screenDelta);
			if (!layout.Contains(vector))
			{
				panelDelta = screenDelta;
				return true;
			}
		}
		else
		{
			vector = ScreenToPanel(screenPosition - screenDelta);
		}
		panelDelta = panelPosition - vector;
		return true;
	}

	private void AssignPanelToComponents(BaseRuntimePanel panel)
	{
		if (selectableGameObject == null)
		{
			return;
		}
		List<IRuntimePanelComponent> list = ObjectListPool<IRuntimePanelComponent>.Get();
		try
		{
			selectableGameObject.GetComponents(list);
			foreach (IRuntimePanelComponent item in list)
			{
				item.panel = panel;
			}
		}
		finally
		{
			ObjectListPool<IRuntimePanelComponent>.Release(list);
		}
	}

	internal void PointerLeavesPanel(int pointerId, Vector2 position)
	{
		ClearCachedElementUnderPointer(pointerId, null);
		CommitElementUnderPointers();
		PointerDeviceState.SavePointerPosition(pointerId, position, null, contextType);
	}

	internal void PointerEntersPanel(int pointerId, Vector2 position)
	{
		PointerDeviceState.SavePointerPosition(pointerId, position, this, contextType);
	}
}
