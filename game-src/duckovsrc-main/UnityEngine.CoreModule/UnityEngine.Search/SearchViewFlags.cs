using System;

namespace UnityEngine.Search;

[Flags]
public enum SearchViewFlags
{
	None = 0,
	Debug = 0x10,
	NoIndexing = 0x20,
	Packages = 0x100,
	OpenLeftSidePanel = 0x800,
	OpenInspectorPreview = 0x1000,
	Centered = 0x2000,
	HideSearchBar = 0x4000,
	CompactView = 0x8000,
	ListView = 0x10000,
	GridView = 0x20000,
	TableView = 0x40000,
	EnableSearchQuery = 0x80000,
	DisableInspectorPreview = 0x100000,
	DisableSavedSearchQuery = 0x200000,
	OpenInBuilderMode = 0x400000,
	OpenInTextMode = 0x800000,
	DisableBuilderModeToggle = 0x1000000,
	Borderless = 0x2000000,
	DisableQueryHelpers = 0x4000000,
	DisableNoResultTips = 0x8000000,
	IgnoreSavedSearches = 0x10000000,
	ObjectPicker = 0x20000000,
	ObjectPickerAdvancedUI = 0x40000000,
	ContextSwitchPreservedMask = 0x2001800
}
