using System;
using UnityEngine;

namespace Polybrush;

[ExecuteInEditMode]
[Obsolete]
public class z_AdditionalVertexStreams : MonoBehaviour
{
	public Mesh m_AdditionalVertexStreamMesh;

	private MeshRenderer _meshRenderer;

	private MeshRenderer meshRenderer
	{
		get
		{
			if (_meshRenderer == null)
			{
				_meshRenderer = base.gameObject.GetComponent<MeshRenderer>();
			}
			return _meshRenderer;
		}
	}

	private void Start()
	{
		SetAdditionalVertexStreamsMesh(m_AdditionalVertexStreamMesh);
	}

	public void SetAdditionalVertexStreamsMesh(Mesh mesh)
	{
		if (meshRenderer != null)
		{
			m_AdditionalVertexStreamMesh = mesh;
			meshRenderer.additionalVertexStreams = mesh;
		}
	}
}
