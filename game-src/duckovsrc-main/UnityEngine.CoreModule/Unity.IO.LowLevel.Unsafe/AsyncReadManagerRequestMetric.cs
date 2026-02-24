using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace Unity.IO.LowLevel.Unsafe;

[NativeConditional("ENABLE_PROFILER")]
[RequiredByNativeCode]
public struct AsyncReadManagerRequestMetric
{
	[NativeName("assetName")]
	public string AssetName { get; }

	[NativeName("fileName")]
	public string FileName { get; }

	[NativeName("offsetBytes")]
	public ulong OffsetBytes { get; }

	[NativeName("sizeBytes")]
	public ulong SizeBytes { get; }

	[NativeName("assetTypeId")]
	public ulong AssetTypeId { get; }

	[NativeName("currentBytesRead")]
	public ulong CurrentBytesRead { get; }

	[NativeName("batchReadCount")]
	public uint BatchReadCount { get; }

	[NativeName("isBatchRead")]
	public bool IsBatchRead { get; }

	[NativeName("state")]
	public ProcessingState State { get; }

	[NativeName("readType")]
	public FileReadType ReadType { get; }

	[NativeName("priorityLevel")]
	public Priority PriorityLevel { get; }

	[NativeName("subsystem")]
	public AssetLoadingSubsystem Subsystem { get; }

	[NativeName("requestTimeMicroseconds")]
	public double RequestTimeMicroseconds { get; }

	[NativeName("timeInQueueMicroseconds")]
	public double TimeInQueueMicroseconds { get; }

	[NativeName("totalTimeMicroseconds")]
	public double TotalTimeMicroseconds { get; }
}
