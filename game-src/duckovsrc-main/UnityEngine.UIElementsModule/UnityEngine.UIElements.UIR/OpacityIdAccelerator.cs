using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.UIElements.UIR;

internal class OpacityIdAccelerator : IDisposable
{
	private struct OpacityIdUpdateJob : IJobParallelFor
	{
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<Vertex> oldVerts;

		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<Vertex> newVerts;

		public Color32 opacityData;

		public void Execute(int i)
		{
			Vertex value = oldVerts[i];
			value.opacityColorPages.r = opacityData.r;
			value.opacityColorPages.g = opacityData.g;
			value.ids.b = opacityData.b;
			newVerts[i] = value;
		}
	}

	private const int k_VerticesPerBatch = 128;

	private const int k_JobLimit = 256;

	private NativeArray<JobHandle> m_Jobs = new NativeArray<JobHandle>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

	private int m_NextJobIndex;

	protected bool disposed { get; private set; }

	public void CreateJob(NativeSlice<Vertex> oldVerts, NativeSlice<Vertex> newVerts, Color32 opacityData, int vertexCount)
	{
		JobHandle value = new OpacityIdUpdateJob
		{
			oldVerts = oldVerts,
			newVerts = newVerts,
			opacityData = opacityData
		}.Schedule(vertexCount, 128);
		if (m_NextJobIndex == m_Jobs.Length)
		{
			m_Jobs[0] = JobHandle.CombineDependencies(m_Jobs);
			m_NextJobIndex = 1;
			JobHandle.ScheduleBatchedJobs();
		}
		m_Jobs[m_NextJobIndex++] = value;
	}

	public void CompleteJobs()
	{
		if (m_NextJobIndex > 0)
		{
			if (m_NextJobIndex > 1)
			{
				JobHandle.CombineDependencies(m_Jobs.Slice(0, m_NextJobIndex)).Complete();
			}
			else
			{
				m_Jobs[0].Complete();
			}
		}
		m_NextJobIndex = 0;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
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
