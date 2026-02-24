using System.Diagnostics;
using UnityEngine.Scripting;

namespace Unity.Profiling;

[DebuggerDisplay("Value = {Value}; Count = {Count}")]
[UsedByNativeCode]
public struct ProfilerRecorderSample
{
	private long value;

	private long count;

	private long refValue;

	public long Value => value;

	public long Count => count;
}
