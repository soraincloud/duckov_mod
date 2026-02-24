using System;

namespace UnityEngine.UIElements;

[Flags]
internal enum EventCategoryFlags
{
	None = 0,
	All = -1,
	TriggeredByOS = 0x14036,
	TargetOnly = 0xAD0
}
