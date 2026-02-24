using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling.LowLevel.Unsafe;

namespace Unity.Profiling;

public static class ProfilerMarkerExtension
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, int metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 2,
			Size = (uint)UnsafeUtility.SizeOf<int>(),
			Ptr = &metadata
		};
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, uint metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 3,
			Size = (uint)UnsafeUtility.SizeOf<uint>(),
			Ptr = &metadata
		};
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, long metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 4,
			Size = (uint)UnsafeUtility.SizeOf<long>(),
			Ptr = &metadata
		};
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, ulong metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 5,
			Size = (uint)UnsafeUtility.SizeOf<ulong>(),
			Ptr = &metadata
		};
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, float metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 6,
			Size = (uint)UnsafeUtility.SizeOf<float>(),
			Ptr = &metadata
		};
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, double metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 7,
			Size = (uint)UnsafeUtility.SizeOf<double>(),
			Ptr = &metadata
		};
		ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public unsafe static void Begin(this ProfilerMarker marker, string metadata)
	{
		ProfilerMarkerData profilerMarkerData = new ProfilerMarkerData
		{
			Type = 9
		};
		fixed (char* ptr = metadata)
		{
			profilerMarkerData.Size = (uint)((metadata.Length + 1) * 2);
			profilerMarkerData.Ptr = ptr;
			ProfilerUnsafeUtility.BeginSampleWithMetadata(marker.Handle, 1, &profilerMarkerData);
		}
	}
}
