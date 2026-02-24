using Unity.Profiling;

namespace UnityEngine.UIElements;

internal class VisualTreeHierarchyFlagsUpdater : BaseVisualTreeUpdater
{
	private uint m_Version = 0u;

	private uint m_LastVersion = 0u;

	private static readonly string s_Description = "Update Hierarchy Flags";

	private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker(s_Description);

	public override ProfilerMarker profilerMarker => s_ProfilerMarker;

	public override void OnVersionChanged(VisualElement ve, VersionChangeType versionChangeType)
	{
		if ((versionChangeType & (VersionChangeType.Hierarchy | VersionChangeType.Overflow | VersionChangeType.BorderWidth | VersionChangeType.Transform | VersionChangeType.Size | VersionChangeType.EventCallbackCategories | VersionChangeType.Picking)) != 0)
		{
			bool flag = (versionChangeType & VersionChangeType.Transform) != 0;
			bool flag2 = (versionChangeType & (VersionChangeType.Overflow | VersionChangeType.BorderWidth | VersionChangeType.Transform | VersionChangeType.Size)) != 0;
			bool flag3 = (versionChangeType & (VersionChangeType.Hierarchy | VersionChangeType.EventCallbackCategories)) != 0;
			VisualElementFlags visualElementFlags = (VisualElementFlags)((flag ? 17 : 0) | (flag2 ? 4 : 0) | (flag3 ? 32 : 0));
			VisualElementFlags visualElementFlags2 = visualElementFlags & ~ve.m_Flags;
			if (visualElementFlags2 != 0)
			{
				DirtyHierarchy(ve, visualElementFlags2);
			}
			DirtyBoundingBoxHierarchy(ve);
			m_Version++;
		}
	}

	private static void DirtyHierarchy(VisualElement ve, VisualElementFlags mustDirtyFlags)
	{
		ve.m_Flags |= mustDirtyFlags;
		int childCount = ve.hierarchy.childCount;
		for (int i = 0; i < childCount; i++)
		{
			VisualElement visualElement = ve.hierarchy[i];
			VisualElementFlags visualElementFlags = mustDirtyFlags & ~visualElement.m_Flags;
			if (visualElementFlags != 0)
			{
				DirtyHierarchy(visualElement, visualElementFlags);
			}
		}
	}

	private static void DirtyBoundingBoxHierarchy(VisualElement ve)
	{
		ve.isBoundingBoxDirty = true;
		ve.isWorldBoundingBoxDirty = true;
		VisualElement parent = ve.hierarchy.parent;
		while (parent != null && !parent.isBoundingBoxDirty)
		{
			parent.isBoundingBoxDirty = true;
			parent.isWorldBoundingBoxDirty = true;
			parent = parent.hierarchy.parent;
		}
	}

	public override void Update()
	{
		if (m_Version != m_LastVersion)
		{
			m_LastVersion = m_Version;
			base.panel.UpdateElementUnderPointers();
			base.panel.visualTree.UpdateBoundingBox();
		}
	}
}
