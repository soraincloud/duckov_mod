using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Yoga;

namespace UnityEngine.UIElements;

internal class UIRLayoutUpdater : BaseVisualTreeUpdater
{
	private const int kMaxValidateLayoutCount = 10;

	private static readonly string s_Description = "Update Layout";

	private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker(s_Description);

	private List<KeyValuePair<Rect, VisualElement>> changeEventsList = new List<KeyValuePair<Rect, VisualElement>>();

	public override ProfilerMarker profilerMarker => s_ProfilerMarker;

	public override void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
	{
		if ((versionChangeType & (VersionChangeType.Hierarchy | VersionChangeType.Layout)) != 0)
		{
			YogaNode yogaNode = ve.yogaNode;
			if (yogaNode != null && yogaNode.IsMeasureDefined)
			{
				yogaNode.MarkDirty();
			}
		}
	}

	public override void Update()
	{
		int num = 0;
		while (base.visualTree.yogaNode.IsDirty)
		{
			changeEventsList.Clear();
			if (num > 0)
			{
				base.panel.ApplyStyles();
			}
			base.panel.duringLayoutPhase = true;
			base.visualTree.yogaNode.CalculateLayout();
			base.panel.duringLayoutPhase = false;
			UpdateSubTree(base.visualTree, isDisplayed: true, changeEventsList);
			DispatchChangeEvents(changeEventsList, num);
			if (num++ >= 10)
			{
				Debug.LogError("Layout update is struggling to process current layout (consider simplifying to avoid recursive layout): " + base.visualTree);
				break;
			}
		}
		base.visualTree.focusController.ReevaluateFocus();
	}

	private void UpdateSubTree(VisualElement ve, bool isDisplayed, List<KeyValuePair<Rect, VisualElement>> changeEvents)
	{
		Rect lastLayout = new Rect(ve.yogaNode.LayoutX, ve.yogaNode.LayoutY, ve.yogaNode.LayoutWidth, ve.yogaNode.LayoutHeight);
		Rect rect = new Rect(ve.yogaNode.LayoutPaddingLeft, ve.yogaNode.LayoutPaddingLeft, ve.yogaNode.LayoutPaddingRight, ve.yogaNode.LayoutPaddingBottom);
		Rect lastPseudoPadding = new Rect(rect.x, rect.y, lastLayout.width - (rect.x + rect.width), lastLayout.height - (rect.y + rect.height));
		Rect lastLayout2 = ve.lastLayout;
		Rect lastPseudoPadding2 = ve.lastPseudoPadding;
		bool isHierarchyDisplayed = ve.isHierarchyDisplayed;
		VersionChangeType versionChangeType = (VersionChangeType)0;
		bool flag = lastLayout2.size != lastLayout.size;
		bool flag2 = lastPseudoPadding2.size != lastPseudoPadding.size;
		if (flag || flag2)
		{
			versionChangeType |= VersionChangeType.Size | VersionChangeType.Repaint;
		}
		bool flag3 = lastLayout.position != lastLayout2.position;
		bool flag4 = lastPseudoPadding.position != lastPseudoPadding2.position;
		if (flag3 || flag4)
		{
			versionChangeType |= VersionChangeType.Transform;
		}
		if ((versionChangeType & VersionChangeType.Size) != 0 && (versionChangeType & VersionChangeType.Transform) == 0 && !ve.hasDefaultRotationAndScale && (!Mathf.Approximately(ve.resolvedStyle.transformOrigin.x, 0f) || !Mathf.Approximately(ve.resolvedStyle.transformOrigin.y, 0f)))
		{
			versionChangeType |= VersionChangeType.Transform;
		}
		isDisplayed &= ve.resolvedStyle.display != DisplayStyle.None;
		ve.isHierarchyDisplayed = isDisplayed;
		if (versionChangeType != 0)
		{
			ve.IncrementVersion(versionChangeType);
		}
		ve.lastLayout = lastLayout;
		ve.lastPseudoPadding = lastPseudoPadding;
		bool hasNewLayout = ve.yogaNode.HasNewLayout;
		if (hasNewLayout)
		{
			int childCount = ve.hierarchy.childCount;
			for (int i = 0; i < childCount; i++)
			{
				VisualElement visualElement = ve.hierarchy[i];
				if (visualElement.yogaNode.HasNewLayout)
				{
					UpdateSubTree(visualElement, isDisplayed, changeEvents);
				}
			}
		}
		if ((flag || flag3) && ve.HasEventCallbacksOrDefaultActions(EventBase<GeometryChangedEvent>.EventCategory))
		{
			changeEvents.Add(new KeyValuePair<Rect, VisualElement>(lastLayout2, ve));
		}
		if (hasNewLayout)
		{
			ve.yogaNode.MarkLayoutSeen();
		}
	}

	private void DispatchChangeEvents(List<KeyValuePair<Rect, VisualElement>> changeEvents, int currentLayoutPass)
	{
		foreach (KeyValuePair<Rect, VisualElement> changeEvent in changeEvents)
		{
			VisualElement value = changeEvent.Value;
			using GeometryChangedEvent geometryChangedEvent = GeometryChangedEvent.GetPooled(changeEvent.Key, value.lastLayout);
			geometryChangedEvent.layoutPass = currentLayoutPass;
			geometryChangedEvent.target = value;
			value.HandleEventAtTargetAndDefaultPhase(geometryChangedEvent);
		}
	}
}
