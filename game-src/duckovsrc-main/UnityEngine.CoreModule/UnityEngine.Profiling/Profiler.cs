using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine.Bindings;
using UnityEngine.Scripting;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Profiling;

[MovedFrom("UnityEngine")]
[UsedByNativeCode]
[NativeHeader("Runtime/Allocator/MemoryManager.h")]
[NativeHeader("Runtime/Profiler/ScriptBindings/Profiler.bindings.h")]
[NativeHeader("Runtime/ScriptingBackend/ScriptingApi.h")]
[NativeHeader("Runtime/Profiler/MemoryProfiler.h")]
[NativeHeader("Runtime/Utilities/MemoryUtilities.h")]
[NativeHeader("Runtime/Profiler/Profiler.h")]
public sealed class Profiler
{
	internal const uint invalidProfilerArea = uint.MaxValue;

	public static extern bool supported
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "profiler_is_available", IsFreeFunction = true)]
		get;
	}

	[StaticAccessor("ProfilerBindings", StaticAccessorType.DoubleColon)]
	public static extern string logFile
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		set;
	}

	public static extern bool enableBinaryLog
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::IsBinaryLogEnabled", IsFreeFunction = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::SetBinaryLogEnabled", IsFreeFunction = true)]
		set;
	}

	public static extern int maxUsedMemory
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::GetMaxUsedMemory", IsFreeFunction = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::SetMaxUsedMemory", IsFreeFunction = true)]
		set;
	}

	public static extern bool enabled
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "profiler_is_enabled", IsFreeFunction = true, IsThreadSafe = true)]
		[NativeConditional("ENABLE_PROFILER")]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::SetProfilerEnabled", IsFreeFunction = true)]
		set;
	}

	public static extern bool enableAllocationCallstacks
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::IsAllocationCallstackCaptureEnabled", IsFreeFunction = true)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "ProfilerBindings::SetAllocationCallstackCaptureEnabled", IsFreeFunction = true)]
		set;
	}

	public static int areaCount => Enum.GetNames(typeof(ProfilerArea)).Length;

	[Obsolete("maxNumberOfSamplesPerFrame has been depricated. Use maxUsedMemory instead")]
	public static int maxNumberOfSamplesPerFrame
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	[Obsolete("usedHeapSize has been deprecated since it is limited to 4GB. Please use usedHeapSizeLong instead.")]
	public static uint usedHeapSize => (uint)usedHeapSizeLong;

	public static extern long usedHeapSizeLong
	{
		[MethodImpl(MethodImplOptions.InternalCall)]
		[NativeMethod(Name = "GetUsedHeapSize", IsFreeFunction = true)]
		get;
	}

	private Profiler()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[Conditional("ENABLE_PROFILER")]
	[FreeFunction("ProfilerBindings::profiler_set_area_enabled")]
	public static extern void SetAreaEnabled(ProfilerArea area, bool enabled);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_PROFILER")]
	[FreeFunction("ProfilerBindings::profiler_is_area_enabled")]
	public static extern bool GetAreaEnabled(ProfilerArea area);

	[Conditional("UNITY_EDITOR")]
	public static void AddFramesFromFile(string file)
	{
		if (string.IsNullOrEmpty(file))
		{
			Debug.LogError("AddFramesFromFile: Invalid or empty path");
		}
		else
		{
			AddFramesFromFile_Internal(file, keepExistingFrames: true);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "LoadFromFile")]
	[StaticAccessor("profiling::GetProfilerSessionPtr()", StaticAccessorType.Arrow)]
	[NativeConditional("ENABLE_PROFILER && UNITY_EDITOR")]
	[NativeHeader("Modules/ProfilerEditor/Public/ProfilerSession.h")]
	private static extern void AddFramesFromFile_Internal(string file, bool keepExistingFrames);

	[Conditional("ENABLE_PROFILER")]
	public static void BeginThreadProfiling(string threadGroupName, string threadName)
	{
		if (string.IsNullOrEmpty(threadGroupName))
		{
			throw new ArgumentException("Argument should be a valid string", "threadGroupName");
		}
		if (string.IsNullOrEmpty(threadName))
		{
			throw new ArgumentException("Argument should be a valid string", "threadName");
		}
		BeginThreadProfilingInternal(threadGroupName, threadName);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_PROFILER")]
	[NativeMethod(Name = "ProfilerBindings::BeginThreadProfiling", IsFreeFunction = true, IsThreadSafe = true)]
	private static extern void BeginThreadProfilingInternal(string threadGroupName, string threadName);

	[NativeConditional("ENABLE_PROFILER")]
	public static void EndThreadProfiling()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public static void BeginSample(string name)
	{
		ValidateArguments(name);
		BeginSampleImpl(name, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_PROFILER")]
	public static void BeginSample(string name, Object targetObject)
	{
		ValidateArguments(name);
		BeginSampleImpl(name, targetObject);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ValidateArguments(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentException("Argument should be a valid string.", "name");
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "ProfilerBindings::BeginSample", IsFreeFunction = true, IsThreadSafe = true)]
	private static extern void BeginSampleImpl(string name, Object targetObject);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "ProfilerBindings::EndSample", IsFreeFunction = true, IsThreadSafe = true)]
	[Conditional("ENABLE_PROFILER")]
	public static extern void EndSample();

	[Obsolete("GetRuntimeMemorySize has been deprecated since it is limited to 2GB. Please use GetRuntimeMemorySizeLong() instead.")]
	public static int GetRuntimeMemorySize(Object o)
	{
		return (int)GetRuntimeMemorySizeLong(o);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "ProfilerBindings::GetRuntimeMemorySizeLong", IsFreeFunction = true)]
	public static extern long GetRuntimeMemorySizeLong([NotNull("ArgumentNullException")] Object o);

	[Obsolete("GetMonoHeapSize has been deprecated since it is limited to 4GB. Please use GetMonoHeapSizeLong() instead.")]
	public static uint GetMonoHeapSize()
	{
		return (uint)GetMonoHeapSizeLong();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "scripting_gc_get_heap_size", IsFreeFunction = true)]
	public static extern long GetMonoHeapSizeLong();

	[Obsolete("GetMonoUsedSize has been deprecated since it is limited to 4GB. Please use GetMonoUsedSizeLong() instead.")]
	public static uint GetMonoUsedSize()
	{
		return (uint)GetMonoUsedSizeLong();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "scripting_gc_get_used_size", IsFreeFunction = true)]
	public static extern long GetMonoUsedSizeLong();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetMemoryManager()", StaticAccessorType.Dot)]
	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	public static extern bool SetTempAllocatorRequestedSize(uint size);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetMemoryManager()", StaticAccessorType.Dot)]
	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	public static extern uint GetTempAllocatorSize();

	[Obsolete("GetTotalAllocatedMemory has been deprecated since it is limited to 4GB. Please use GetTotalAllocatedMemoryLong() instead.")]
	public static uint GetTotalAllocatedMemory()
	{
		return (uint)GetTotalAllocatedMemoryLong();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	[NativeMethod(Name = "GetTotalAllocatedMemory")]
	[StaticAccessor("GetMemoryManager()", StaticAccessorType.Dot)]
	public static extern long GetTotalAllocatedMemoryLong();

	[Obsolete("GetTotalUnusedReservedMemory has been deprecated since it is limited to 4GB. Please use GetTotalUnusedReservedMemoryLong() instead.")]
	public static uint GetTotalUnusedReservedMemory()
	{
		return (uint)GetTotalUnusedReservedMemoryLong();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("GetMemoryManager()", StaticAccessorType.Dot)]
	[NativeMethod(Name = "GetTotalUnusedReservedMemory")]
	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	public static extern long GetTotalUnusedReservedMemoryLong();

	[Obsolete("GetTotalReservedMemory has been deprecated since it is limited to 4GB. Please use GetTotalReservedMemoryLong() instead.")]
	public static uint GetTotalReservedMemory()
	{
		return (uint)GetTotalReservedMemoryLong();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "GetTotalReservedMemory")]
	[StaticAccessor("GetMemoryManager()", StaticAccessorType.Dot)]
	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	public static extern long GetTotalReservedMemoryLong();

	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	public unsafe static long GetTotalFragmentationInfo(NativeArray<int> stats)
	{
		return InternalGetTotalFragmentationInfo((IntPtr)stats.GetUnsafePtr(), stats.Length);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "GetTotalFragmentationInfo")]
	[StaticAccessor("GetMemoryManager()", StaticAccessorType.Dot)]
	[NativeConditional("ENABLE_MEMORY_MANAGER")]
	private static extern long InternalGetTotalFragmentationInfo(IntPtr pStats, int count);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("MemoryProfiler", StaticAccessorType.DoubleColon)]
	[NativeMethod(Name = "GetRegisteredGFXDriverMemory", IsThreadSafe = true)]
	[NativeConditional("ENABLE_PROFILER")]
	public static extern long GetAllocatedMemoryForGraphicsDriver();

	[Conditional("ENABLE_PROFILER")]
	public unsafe static void EmitFrameMetaData(Guid id, int tag, Array data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Type elementType = data.GetType().GetElementType();
		if (!UnsafeUtility.IsBlittable(elementType))
		{
			throw new ArgumentException($"{elementType} type must be blittable");
		}
		Internal_EmitGlobalMetaData_Array(&id, 16, tag, data, data.Length, UnsafeUtility.SizeOf(elementType), frameData: true);
	}

	[Conditional("ENABLE_PROFILER")]
	public unsafe static void EmitFrameMetaData<T>(Guid id, int tag, List<T> data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Type typeFromHandle = typeof(T);
		if (!UnsafeUtility.IsBlittable(typeof(T)))
		{
			throw new ArgumentException($"{typeFromHandle} type must be blittable");
		}
		Internal_EmitGlobalMetaData_Array(&id, 16, tag, NoAllocHelpers.ExtractArrayFromList(data), data.Count, UnsafeUtility.SizeOf(typeFromHandle), frameData: true);
	}

	[Conditional("ENABLE_PROFILER")]
	public unsafe static void EmitFrameMetaData<T>(Guid id, int tag, NativeArray<T> data) where T : struct
	{
		Internal_EmitGlobalMetaData_Native(&id, 16, tag, (IntPtr)data.GetUnsafeReadOnlyPtr(), data.Length, UnsafeUtility.SizeOf<T>(), frameData: true);
	}

	[Conditional("ENABLE_PROFILER")]
	public unsafe static void EmitSessionMetaData(Guid id, int tag, Array data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Type elementType = data.GetType().GetElementType();
		if (!UnsafeUtility.IsBlittable(elementType))
		{
			throw new ArgumentException($"{elementType} type must be blittable");
		}
		Internal_EmitGlobalMetaData_Array(&id, 16, tag, data, data.Length, UnsafeUtility.SizeOf(elementType), frameData: false);
	}

	[Conditional("ENABLE_PROFILER")]
	public unsafe static void EmitSessionMetaData<T>(Guid id, int tag, List<T> data) where T : struct
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		Type typeFromHandle = typeof(T);
		if (!UnsafeUtility.IsBlittable(typeof(T)))
		{
			throw new ArgumentException($"{typeFromHandle} type must be blittable");
		}
		Internal_EmitGlobalMetaData_Array(&id, 16, tag, NoAllocHelpers.ExtractArrayFromList(data), data.Count, UnsafeUtility.SizeOf(typeFromHandle), frameData: false);
	}

	[Conditional("ENABLE_PROFILER")]
	public unsafe static void EmitSessionMetaData<T>(Guid id, int tag, NativeArray<T> data) where T : struct
	{
		Internal_EmitGlobalMetaData_Native(&id, 16, tag, (IntPtr)data.GetUnsafeReadOnlyPtr(), data.Length, UnsafeUtility.SizeOf<T>(), frameData: false);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "ProfilerBindings::Internal_EmitGlobalMetaData_Array", IsFreeFunction = true, IsThreadSafe = true)]
	[NativeConditional("ENABLE_PROFILER")]
	private unsafe static extern void Internal_EmitGlobalMetaData_Array(void* id, int idLen, int tag, Array data, int count, int elementSize, bool frameData);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "ProfilerBindings::Internal_EmitGlobalMetaData_Native", IsFreeFunction = true, IsThreadSafe = true)]
	[NativeConditional("ENABLE_PROFILER")]
	private unsafe static extern void Internal_EmitGlobalMetaData_Native(void* id, int idLen, int tag, IntPtr data, int count, int elementSize, bool frameData);

	[Conditional("ENABLE_PROFILER")]
	public static void SetCategoryEnabled(ProfilerCategory category, bool enabled)
	{
		if ((ushort)category == (ushort)ProfilerCategory.Any)
		{
			throw new ArgumentException("Argument should be a valid category", "category");
		}
		Internal_SetCategoryEnabled(category, enabled);
	}

	public static bool IsCategoryEnabled(ProfilerCategory category)
	{
		if ((ushort)category == (ushort)ProfilerCategory.Any)
		{
			throw new ArgumentException("Argument should be a valid category", "category");
		}
		return Internal_IsCategoryEnabled(category);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[StaticAccessor("profiling::GetProfilerManagerPtr()", StaticAccessorType.Arrow)]
	[NativeMethod(Name = "GetCategoriesCount")]
	[NativeHeader("Runtime/Profiler/ProfilerManager.h")]
	[NativeConditional("ENABLE_PROFILER")]
	public static extern uint GetCategoriesCount();

	[Conditional("ENABLE_PROFILER")]
	public static void GetAllCategories(ProfilerCategory[] categories)
	{
		for (int i = 0; i < Math.Min(GetCategoriesCount(), categories.Length); i++)
		{
			categories[i] = new ProfilerCategory((ushort)i);
		}
	}

	[Conditional("ENABLE_PROFILER")]
	public static void GetAllCategories(NativeArray<ProfilerCategory> categories)
	{
		for (int i = 0; i < Math.Min(GetCategoriesCount(), categories.Length); i++)
		{
			categories[i] = new ProfilerCategory((ushort)i);
		}
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeConditional("ENABLE_PROFILER")]
	[NativeMethod(Name = "profiler_set_category_enable", IsFreeFunction = true, IsThreadSafe = true)]
	private static extern void Internal_SetCategoryEnabled(ushort categoryId, bool enabled);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[NativeMethod(Name = "profiler_is_category_enabled", IsFreeFunction = true, IsThreadSafe = true)]
	[NativeConditional("ENABLE_PROFILER")]
	private static extern bool Internal_IsCategoryEnabled(ushort categoryId);
}
