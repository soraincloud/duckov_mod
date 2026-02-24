using System;
using System.Collections.Generic;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Profiling;

[UsedByNativeCode]
[NativeHeader("Runtime/Profiler/ScriptBindings/Sampler.bindings.h")]
public class Sampler
{
	internal IntPtr m_Ptr;

	internal static Sampler s_InvalidSampler = new Sampler();

	public bool isValid => m_Ptr != IntPtr.Zero;

	public string name => ProfilerUnsafeUtility.Internal_GetName(m_Ptr);

	internal Sampler()
	{
	}

	internal Sampler(IntPtr ptr)
	{
		m_Ptr = ptr;
	}

	public Recorder GetRecorder()
	{
		ProfilerRecorderHandle handle = new ProfilerRecorderHandle((ulong)m_Ptr.ToInt64());
		return new Recorder(handle);
	}

	public static Sampler Get(string name)
	{
		IntPtr marker = ProfilerUnsafeUtility.GetMarker(name);
		if (marker == IntPtr.Zero)
		{
			return s_InvalidSampler;
		}
		return new Sampler(marker);
	}

	public static int GetNames(List<string> names)
	{
		List<ProfilerRecorderHandle> list = new List<ProfilerRecorderHandle>();
		ProfilerRecorderHandle.GetAvailable(list);
		if (names != null)
		{
			if (names.Count < list.Count)
			{
				names.Capacity = list.Count;
				for (int i = names.Count; i < list.Count; i++)
				{
					names.Add(null);
				}
			}
			int num = 0;
			foreach (ProfilerRecorderHandle item in list)
			{
				names[num] = ProfilerRecorderHandle.GetDescription(item).Name;
				num++;
			}
		}
		return list.Count;
	}
}
