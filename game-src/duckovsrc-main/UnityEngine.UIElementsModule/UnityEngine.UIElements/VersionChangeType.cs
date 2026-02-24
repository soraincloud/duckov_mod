using System;

namespace UnityEngine.UIElements;

[Flags]
internal enum VersionChangeType
{
	Bindings = 1,
	ViewData = 2,
	Hierarchy = 4,
	Layout = 8,
	StyleSheet = 0x10,
	Styles = 0x20,
	Overflow = 0x40,
	BorderRadius = 0x80,
	BorderWidth = 0x100,
	Transform = 0x200,
	Size = 0x400,
	Repaint = 0x800,
	Opacity = 0x1000,
	Color = 0x2000,
	RenderHints = 0x4000,
	TransitionProperty = 0x8000,
	EventCallbackCategories = 0x10000,
	Picking = 0x100000
}
