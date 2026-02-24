using System;

namespace UnityEngine.UIElements;

[Flags]
internal enum VisualElementFlags
{
	WorldTransformDirty = 1,
	WorldTransformInverseDirty = 2,
	WorldClipDirty = 4,
	BoundingBoxDirty = 8,
	WorldBoundingBoxDirty = 0x10,
	EventCallbackParentCategoriesDirty = 0x20,
	LayoutManual = 0x40,
	CompositeRoot = 0x80,
	RequireMeasureFunction = 0x100,
	EnableViewDataPersistence = 0x200,
	DisableClipping = 0x400,
	NeedsAttachToPanelEvent = 0x800,
	HierarchyDisplayed = 0x1000,
	StyleInitialized = 0x2000,
	Init = 0x103F
}
