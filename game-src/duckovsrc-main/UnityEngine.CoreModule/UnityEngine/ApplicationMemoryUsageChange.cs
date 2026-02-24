namespace UnityEngine;

public struct ApplicationMemoryUsageChange
{
	public ApplicationMemoryUsage memoryUsage { get; private set; }

	public ApplicationMemoryUsageChange(ApplicationMemoryUsage usage)
	{
		memoryUsage = usage;
	}
}
