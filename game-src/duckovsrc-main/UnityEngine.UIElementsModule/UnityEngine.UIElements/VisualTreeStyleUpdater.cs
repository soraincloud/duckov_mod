#define UNITY_ASSERTIONS
using System.Collections.Generic;
using Unity.Profiling;

namespace UnityEngine.UIElements;

internal class VisualTreeStyleUpdater : BaseVisualTreeUpdater
{
	private HashSet<VisualElement> m_ApplyStyleUpdateList = new HashSet<VisualElement>();

	private HashSet<VisualElement> m_TransitionPropertyUpdateList = new HashSet<VisualElement>();

	private bool m_IsApplyingStyles = false;

	private uint m_Version = 0u;

	private uint m_LastVersion = 0u;

	private VisualTreeStyleUpdaterTraversal m_StyleContextHierarchyTraversal = new VisualTreeStyleUpdaterTraversal();

	private static readonly string s_Description = "Update Style";

	private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker(s_Description);

	public VisualTreeStyleUpdaterTraversal traversal
	{
		get
		{
			return m_StyleContextHierarchyTraversal;
		}
		set
		{
			m_StyleContextHierarchyTraversal = value;
			base.panel?.visualTree.IncrementVersion(VersionChangeType.Layout | VersionChangeType.StyleSheet | VersionChangeType.Styles | VersionChangeType.Transform);
		}
	}

	public override ProfilerMarker profilerMarker => s_ProfilerMarker;

	protected bool disposed { get; private set; }

	public override void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
	{
		if ((versionChangeType & (VersionChangeType.StyleSheet | VersionChangeType.TransitionProperty)) == 0)
		{
			return;
		}
		m_Version++;
		if ((versionChangeType & VersionChangeType.StyleSheet) != 0)
		{
			if (m_IsApplyingStyles)
			{
				m_ApplyStyleUpdateList.Add(ve);
			}
			else
			{
				m_StyleContextHierarchyTraversal.AddChangedElement(ve, versionChangeType);
			}
		}
		if ((versionChangeType & VersionChangeType.TransitionProperty) != 0)
		{
			m_TransitionPropertyUpdateList.Add(ve);
		}
	}

	public override void Update()
	{
		if (m_Version == m_LastVersion)
		{
			return;
		}
		m_LastVersion = m_Version;
		ApplyStyles();
		m_StyleContextHierarchyTraversal.Clear();
		foreach (VisualElement applyStyleUpdate in m_ApplyStyleUpdateList)
		{
			m_StyleContextHierarchyTraversal.AddChangedElement(applyStyleUpdate, VersionChangeType.StyleSheet);
		}
		m_ApplyStyleUpdateList.Clear();
		foreach (VisualElement transitionPropertyUpdate in m_TransitionPropertyUpdateList)
		{
			if (transitionPropertyUpdate.hasRunningAnimations || transitionPropertyUpdate.hasCompletedAnimations)
			{
				ComputedTransitionUtils.UpdateComputedTransitions(ref transitionPropertyUpdate.computedStyle);
				m_StyleContextHierarchyTraversal.CancelAnimationsWithNoTransitionProperty(transitionPropertyUpdate, ref transitionPropertyUpdate.computedStyle);
			}
		}
		m_TransitionPropertyUpdateList.Clear();
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				m_StyleContextHierarchyTraversal.Clear();
			}
			disposed = true;
		}
	}

	private void ApplyStyles()
	{
		Debug.Assert(base.visualTree.panel != null);
		m_IsApplyingStyles = true;
		m_StyleContextHierarchyTraversal.PrepareTraversal(base.panel.scaledPixelsPerPoint);
		m_StyleContextHierarchyTraversal.Traverse(base.visualTree);
		m_IsApplyingStyles = false;
	}
}
