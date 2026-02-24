using System;

namespace UnityEngine.UIElements;

internal interface IEditableElement
{
	internal Action editingStarted { get; set; }

	internal Action editingEnded { get; set; }
}
