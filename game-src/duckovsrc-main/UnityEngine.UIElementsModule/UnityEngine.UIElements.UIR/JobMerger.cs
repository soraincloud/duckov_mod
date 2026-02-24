#define UNITY_ASSERTIONS
using System;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.UIElements.UIR;

internal class JobMerger : IDisposable
{
	private NativeArray<JobHandle> m_Jobs;

	private int m_JobCount;

	protected bool disposed { get; private set; }

	public JobMerger(int capacity)
	{
		Debug.Assert(capacity > 1);
		m_Jobs = new NativeArray<JobHandle>(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public void Add(JobHandle job)
	{
		if (m_JobCount < m_Jobs.Length)
		{
			m_Jobs[m_JobCount++] = job;
			return;
		}
		m_Jobs[0] = JobHandle.CombineDependencies(m_Jobs);
		m_Jobs[1] = job;
		m_JobCount = 2;
	}

	public JobHandle MergeAndReset()
	{
		JobHandle result = default(JobHandle);
		if (m_JobCount > 1)
		{
			result = JobHandle.CombineDependencies(m_Jobs.Slice(0, m_JobCount));
		}
		else if (m_JobCount == 1)
		{
			result = m_Jobs[0];
		}
		m_JobCount = 0;
		return result;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				m_Jobs.Dispose();
			}
			disposed = true;
		}
	}
}
