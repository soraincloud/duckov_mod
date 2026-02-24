using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.UIElements.UIR;

internal class JobManager : IDisposable
{
	private NativePagedList<NudgeJobData> m_NudgeJobs = new NativePagedList<NudgeJobData>(64);

	private NativePagedList<ConvertMeshJobData> m_ConvertMeshJobs = new NativePagedList<ConvertMeshJobData>(64);

	private NativePagedList<CopyClosingMeshJobData> m_CopyClosingMeshJobs = new NativePagedList<CopyClosingMeshJobData>(64);

	private JobMerger m_JobMerger = new JobMerger(128);

	protected bool disposed { get; private set; }

	public void Add(ref NudgeJobData job)
	{
		m_NudgeJobs.Add(ref job);
	}

	public void Add(ref ConvertMeshJobData job)
	{
		m_ConvertMeshJobs.Add(ref job);
	}

	public void Add(ref CopyClosingMeshJobData job)
	{
		m_CopyClosingMeshJobs.Add(ref job);
	}

	public unsafe void CompleteNudgeJobs()
	{
		foreach (NativeSlice<NudgeJobData> page in m_NudgeJobs.GetPages())
		{
			m_JobMerger.Add(JobProcessor.ScheduleNudgeJobs((IntPtr)page.GetUnsafePtr(), page.Length));
		}
		m_JobMerger.MergeAndReset().Complete();
		m_NudgeJobs.Reset();
	}

	public unsafe void CompleteConvertMeshJobs()
	{
		foreach (NativeSlice<ConvertMeshJobData> page in m_ConvertMeshJobs.GetPages())
		{
			m_JobMerger.Add(JobProcessor.ScheduleConvertMeshJobs((IntPtr)page.GetUnsafePtr(), page.Length));
		}
		m_JobMerger.MergeAndReset().Complete();
		m_ConvertMeshJobs.Reset();
	}

	public unsafe void CompleteClosingMeshJobs()
	{
		foreach (NativeSlice<CopyClosingMeshJobData> page in m_CopyClosingMeshJobs.GetPages())
		{
			m_JobMerger.Add(JobProcessor.ScheduleCopyClosingMeshJobs((IntPtr)page.GetUnsafePtr(), page.Length));
		}
		m_JobMerger.MergeAndReset().Complete();
		m_CopyClosingMeshJobs.Reset();
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
				m_NudgeJobs.Dispose();
				m_ConvertMeshJobs.Dispose();
				m_CopyClosingMeshJobs.Dispose();
				m_JobMerger.Dispose();
			}
			disposed = true;
		}
	}
}
