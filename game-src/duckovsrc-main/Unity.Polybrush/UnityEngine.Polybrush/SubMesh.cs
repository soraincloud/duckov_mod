using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Polybrush;

[Serializable]
internal sealed class SubMesh
{
	[SerializeField]
	private int[] m_Indexes;

	[SerializeField]
	private MeshTopology m_Topology;

	internal int[] indexes
	{
		get
		{
			return m_Indexes;
		}
		set
		{
			m_Indexes = value;
		}
	}

	internal MeshTopology topology
	{
		get
		{
			return m_Topology;
		}
		set
		{
			m_Topology = value;
		}
	}

	internal SubMesh(int submeshIndex, MeshTopology topology, IEnumerable<int> indexes)
	{
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		m_Indexes = indexes.ToArray();
		m_Topology = topology;
	}

	internal SubMesh(Mesh mesh, int subMeshIndex)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		m_Indexes = mesh.GetIndices(subMeshIndex);
		m_Topology = mesh.GetTopology(subMeshIndex);
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}", m_Topology.ToString(), (m_Indexes != null) ? m_Indexes.Length.ToString() : "0");
	}
}
