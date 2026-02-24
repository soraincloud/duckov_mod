using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.UIElements.UIR;

internal class DetachedAllocator : IDisposable
{
	private TempAllocator<Vertex> m_VertsPool;

	private TempAllocator<ushort> m_IndexPool;

	private List<MeshWriteData> m_MeshWriteDataPool;

	private int m_MeshWriteDataCount;

	private bool m_Disposed;

	public List<MeshWriteData> meshes => m_MeshWriteDataPool.GetRange(0, m_MeshWriteDataCount);

	public DetachedAllocator()
	{
		m_MeshWriteDataPool = new List<MeshWriteData>(16);
		m_MeshWriteDataCount = 0;
		m_VertsPool = new TempAllocator<Vertex>(8192, 2048, 65536);
		m_IndexPool = new TempAllocator<ushort>(16384, 4096, 131072);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!m_Disposed)
		{
			if (disposing)
			{
				m_VertsPool.Dispose();
				m_IndexPool.Dispose();
			}
			m_Disposed = true;
		}
	}

	public MeshWriteData Alloc(int vertexCount, int indexCount)
	{
		MeshWriteData meshWriteData = null;
		if (m_MeshWriteDataCount < m_MeshWriteDataPool.Count)
		{
			meshWriteData = m_MeshWriteDataPool[m_MeshWriteDataCount];
		}
		else
		{
			meshWriteData = new MeshWriteData();
			m_MeshWriteDataPool.Add(meshWriteData);
		}
		m_MeshWriteDataCount++;
		if (vertexCount == 0 || indexCount == 0)
		{
			meshWriteData.Reset(default(NativeSlice<Vertex>), default(NativeSlice<ushort>));
			return meshWriteData;
		}
		meshWriteData.Reset(m_VertsPool.Alloc(vertexCount), m_IndexPool.Alloc(indexCount));
		return meshWriteData;
	}

	public void Clear()
	{
		m_VertsPool.Reset();
		m_IndexPool.Reset();
		m_MeshWriteDataCount = 0;
	}
}
