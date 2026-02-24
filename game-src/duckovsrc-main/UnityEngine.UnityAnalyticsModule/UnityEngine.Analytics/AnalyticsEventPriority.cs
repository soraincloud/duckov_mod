using System;

namespace UnityEngine.Analytics;

[Flags]
public enum AnalyticsEventPriority
{
	FlushQueueFlag = 1,
	CacheImmediatelyFlag = 2,
	AllowInStopModeFlag = 4,
	SendImmediateFlag = 8,
	NoCachingFlag = 0x10,
	NoRetryFlag = 0x20,
	NormalPriorityEvent = 0,
	NormalPriorityEvent_WithCaching = 2,
	NormalPriorityEvent_NoRetryNoCaching = 0x30,
	HighPriorityEvent = 1,
	HighPriorityEvent_InStopMode = 5,
	HighestPriorityEvent = 9,
	HighestPriorityEvent_NoRetryNoCaching = 0x31
}
