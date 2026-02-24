#define UNITY_ASSERTIONS
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.UIElements.UIR;

namespace UnityEngine.UIElements;

internal class UIRRepaintUpdater : BaseVisualTreeUpdater
{
	private BaseVisualElementPanel attachedPanel;

	internal RenderChain renderChain;

	private static readonly string s_Description;

	private static readonly ProfilerMarker s_ProfilerMarker;

	public override ProfilerMarker profilerMarker => s_ProfilerMarker;

	public bool drawStats { get; set; }

	public bool breakBatches { get; set; }

	protected bool disposed { get; private set; }

	public UIRRepaintUpdater()
	{
		base.panelChanged += OnPanelChanged;
	}

	public override void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
	{
		if (renderChain != null)
		{
			bool flag = (versionChangeType & VersionChangeType.Transform) != 0;
			bool flag2 = (versionChangeType & VersionChangeType.Size) != 0;
			bool flag3 = (versionChangeType & VersionChangeType.Overflow) != 0;
			bool flag4 = (versionChangeType & VersionChangeType.BorderRadius) != 0;
			bool flag5 = (versionChangeType & VersionChangeType.BorderWidth) != 0;
			if ((versionChangeType & VersionChangeType.RenderHints) != 0)
			{
				renderChain.UIEOnRenderHintsChanged(ve);
			}
			if (flag || flag2 || flag5)
			{
				renderChain.UIEOnTransformOrSizeChanged(ve, flag, flag2 || flag5);
			}
			if (flag3 || flag4)
			{
				renderChain.UIEOnClippingChanged(ve, hierarchical: false);
			}
			if ((versionChangeType & VersionChangeType.Opacity) != 0)
			{
				renderChain.UIEOnOpacityChanged(ve);
			}
			if ((versionChangeType & VersionChangeType.Color) != 0)
			{
				renderChain.UIEOnColorChanged(ve);
			}
			if ((versionChangeType & VersionChangeType.Repaint) != 0)
			{
				renderChain.UIEOnVisualsChanged(ve, hierarchical: false);
			}
		}
	}

	public override void Update()
	{
		if (renderChain == null)
		{
			InitRenderChain();
		}
		if (renderChain != null && renderChain.device != null)
		{
			renderChain.ProcessChanges();
			PanelClearSettings clearSettings = base.panel.clearSettings;
			if (clearSettings.clearColor || clearSettings.clearDepthStencil)
			{
				Color color = clearSettings.color;
				color = color.RGBMultiplied(color.a);
				GL.Clear(clearSettings.clearDepthStencil, clearSettings.clearColor, color, 0.99f);
			}
			renderChain.drawStats = drawStats;
			renderChain.device.breakBatches = breakBatches;
			renderChain.Render();
		}
	}

	protected virtual RenderChain CreateRenderChain()
	{
		return new RenderChain(base.panel);
	}

	static UIRRepaintUpdater()
	{
		s_Description = "Update Rendering";
		s_ProfilerMarker = new ProfilerMarker(s_Description);
		Utility.GraphicsResourcesRecreate += OnGraphicsResourcesRecreate;
	}

	private static void OnGraphicsResourcesRecreate(bool recreate)
	{
		if (!recreate)
		{
			UIRenderDevice.PrepareForGfxDeviceRecreate();
		}
		Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
		while (panelsIterator.MoveNext())
		{
			if (recreate)
			{
				panelsIterator.Current.Value.atlas?.Reset();
			}
			else
			{
				(panelsIterator.Current.Value.GetUpdater(VisualTreeUpdatePhase.Repaint) as UIRRepaintUpdater)?.DestroyRenderChain();
			}
		}
		if (!recreate)
		{
			UIRenderDevice.FlushAllPendingDeviceDisposes();
		}
		else
		{
			UIRenderDevice.WrapUpGfxDeviceRecreate();
		}
	}

	private void OnPanelChanged(BaseVisualElementPanel obj)
	{
		DetachFromPanel();
		AttachToPanel();
	}

	private void AttachToPanel()
	{
		Debug.Assert(attachedPanel == null);
		if (base.panel != null)
		{
			attachedPanel = base.panel;
			attachedPanel.atlasChanged += OnPanelAtlasChanged;
			attachedPanel.standardShaderChanged += OnPanelStandardShaderChanged;
			attachedPanel.standardWorldSpaceShaderChanged += OnPanelStandardWorldSpaceShaderChanged;
			attachedPanel.hierarchyChanged += OnPanelHierarchyChanged;
		}
	}

	private void DetachFromPanel()
	{
		if (attachedPanel != null)
		{
			DestroyRenderChain();
			attachedPanel.atlasChanged -= OnPanelAtlasChanged;
			attachedPanel.standardShaderChanged -= OnPanelStandardShaderChanged;
			attachedPanel.standardWorldSpaceShaderChanged -= OnPanelStandardWorldSpaceShaderChanged;
			attachedPanel.hierarchyChanged -= OnPanelHierarchyChanged;
			attachedPanel = null;
		}
	}

	private void InitRenderChain()
	{
		renderChain = CreateRenderChain();
		if (attachedPanel?.visualTree != null)
		{
			renderChain.UIEOnChildAdded(attachedPanel.visualTree);
		}
		OnPanelStandardShaderChanged();
		if (base.panel.contextType == ContextType.Player)
		{
			OnPanelStandardWorldSpaceShaderChanged();
		}
	}

	internal void DestroyRenderChain()
	{
		if (renderChain != null)
		{
			renderChain.Dispose();
			renderChain = null;
			ResetAllElementsDataRecursive(attachedPanel.visualTree);
		}
	}

	private void OnPanelAtlasChanged()
	{
		DestroyRenderChain();
	}

	private void OnPanelHierarchyChanged(VisualElement ve, HierarchyChangeType changeType)
	{
		if (renderChain != null)
		{
			switch (changeType)
			{
			case HierarchyChangeType.Add:
				renderChain.UIEOnChildAdded(ve);
				break;
			case HierarchyChangeType.Remove:
				renderChain.UIEOnChildRemoving(ve);
				break;
			case HierarchyChangeType.Move:
				renderChain.UIEOnChildrenReordered(ve);
				break;
			}
		}
	}

	private void OnPanelStandardShaderChanged()
	{
		if (renderChain == null)
		{
			return;
		}
		Shader shader = base.panel.standardShader;
		if (shader == null)
		{
			shader = Shader.Find(UIRUtility.k_DefaultShaderName);
			Debug.Assert(shader != null, "Failed to load UIElements default shader");
			if (shader != null)
			{
				shader.hideFlags |= HideFlags.DontSaveInEditor;
			}
		}
		renderChain.defaultShader = shader;
	}

	private void OnPanelStandardWorldSpaceShaderChanged()
	{
		if (renderChain == null)
		{
			return;
		}
		Shader shader = base.panel.standardWorldSpaceShader;
		if (shader == null)
		{
			shader = Shader.Find(UIRUtility.k_DefaultWorldSpaceShaderName);
			Debug.Assert(shader != null, "Failed to load UIElements default world-space shader");
			if (shader != null)
			{
				shader.hideFlags |= HideFlags.DontSaveInEditor;
			}
		}
		renderChain.defaultWorldSpaceShader = shader;
	}

	private void ResetAllElementsDataRecursive(VisualElement ve)
	{
		ve.renderChainData = default(RenderChainVEData);
		int num = ve.hierarchy.childCount - 1;
		while (num >= 0)
		{
			ResetAllElementsDataRecursive(ve.hierarchy[num--]);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				DetachFromPanel();
			}
			disposed = true;
		}
	}
}
