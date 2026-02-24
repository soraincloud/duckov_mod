using System;

namespace UnityEngine.Polybrush;

[ExecuteInEditMode]
internal class PolybrushMesh : MonoBehaviour
{
	internal enum Mode
	{
		Mesh,
		AdditionalVertexStream
	}

	internal static class Styles
	{
		internal const string k_VertexMismatchStringFormat = "Warning! The GameObject \"{0}\" cannot apply it's 'Additional Vertex Streams' mesh, because it's base mesh has changed and has a different vertex count.";
	}

	internal enum ObjectType
	{
		Mesh,
		SkinnedMesh
	}

	internal struct MeshComponentsCache
	{
		private GameObject m_Owner;

		private MeshFilter m_MeshFilter;

		private MeshRenderer m_MeshRenderer;

		private SkinnedMeshRenderer m_SkinMeshRenderer;

		internal MeshFilter MeshFilter => m_MeshFilter;

		internal MeshRenderer MeshRenderer => m_MeshRenderer;

		internal SkinnedMeshRenderer SkinnedMeshRenderer => m_SkinMeshRenderer;

		internal bool IsValid()
		{
			return m_Owner != null;
		}

		internal MeshComponentsCache(GameObject root)
		{
			m_Owner = root;
			m_MeshFilter = root.GetComponent<MeshFilter>();
			m_MeshRenderer = root.GetComponent<MeshRenderer>();
			m_SkinMeshRenderer = root.GetComponent<SkinnedMeshRenderer>();
		}
	}

	[SerializeField]
	private PolyMesh m_PolyMesh;

	[SerializeField]
	private Mesh m_SkinMeshRef;

	[SerializeField]
	private Mesh m_OriginalMeshObject;

	[SerializeField]
	private Mode m_Mode;

	private MeshComponentsCache m_ComponentsCache;

	private bool m_Initialized;

	internal MeshComponentsCache componentsCache => m_ComponentsCache;

	internal bool hasAppliedChanges
	{
		get
		{
			if ((bool)m_ComponentsCache.MeshFilter)
			{
				return m_PolyMesh.ToUnityMesh() != m_ComponentsCache.MeshFilter.sharedMesh;
			}
			if ((bool)m_ComponentsCache.SkinnedMeshRenderer)
			{
				return m_PolyMesh.ToUnityMesh() != m_ComponentsCache.SkinnedMeshRenderer.sharedMesh;
			}
			return false;
		}
	}

	internal bool hasAppliedAdditionalVertexStreams
	{
		get
		{
			if (m_ComponentsCache.MeshRenderer != null && m_ComponentsCache.MeshRenderer.additionalVertexStreams != null)
			{
				return m_ComponentsCache.MeshRenderer.additionalVertexStreams == m_PolyMesh.ToUnityMesh();
			}
			return false;
		}
	}

	internal PolyMesh polyMesh => m_PolyMesh;

	internal Mesh storedMesh
	{
		get
		{
			if (m_PolyMesh != null)
			{
				return m_PolyMesh.ToUnityMesh();
			}
			return null;
		}
	}

	internal ObjectType type
	{
		get
		{
			if ((bool)m_ComponentsCache.SkinnedMeshRenderer)
			{
				return ObjectType.SkinnedMesh;
			}
			return ObjectType.Mesh;
		}
	}

	[Obsolete]
	internal static bool s_UseADVS { private get; set; }

	internal Mode mode
	{
		get
		{
			return m_Mode;
		}
		set
		{
			UpdateMode(value);
		}
	}

	internal Mesh skinMeshRef
	{
		get
		{
			return m_SkinMeshRef;
		}
		set
		{
			m_SkinMeshRef = value;
		}
	}

	internal Mesh sourceMesh => m_OriginalMeshObject;

	internal bool isInitialized => m_Initialized;

	internal void Initialize()
	{
		if (isInitialized)
		{
			return;
		}
		if (!m_ComponentsCache.IsValid())
		{
			m_ComponentsCache = new MeshComponentsCache(base.gameObject);
		}
		if (m_PolyMesh == null)
		{
			m_PolyMesh = new PolyMesh();
		}
		Mesh mesh = null;
		if (m_ComponentsCache.MeshFilter != null)
		{
			mesh = m_ComponentsCache.MeshFilter.sharedMesh;
			if (m_OriginalMeshObject == null)
			{
				m_OriginalMeshObject = mesh;
			}
		}
		else if (m_ComponentsCache.SkinnedMeshRenderer != null)
		{
			mesh = m_ComponentsCache.SkinnedMeshRenderer.sharedMesh;
		}
		if (!polyMesh.IsValid() && (bool)mesh)
		{
			SetMesh(mesh);
		}
		else if (polyMesh.IsValid() && (mesh == null || mesh.vertexCount != polyMesh.vertexCount))
		{
			SetMesh(polyMesh.ToUnityMesh());
		}
		m_Initialized = true;
	}

	internal void SetMesh(Mesh unityMesh)
	{
		if (!(unityMesh == null))
		{
			m_PolyMesh.InitializeWithUnityMesh(unityMesh);
			SynchronizeWithMeshRenderer();
		}
	}

	internal void SetAdditionalVertexStreams(Mesh vertexStreams)
	{
		m_PolyMesh.ApplyAttributesFromUnityMesh(vertexStreams, MeshChannelUtility.ToMask(vertexStreams));
		SynchronizeWithMeshRenderer();
	}

	internal void SynchronizeWithMeshRenderer()
	{
		if (m_PolyMesh == null)
		{
			return;
		}
		m_PolyMesh.UpdateMeshFromData();
		if (m_ComponentsCache.SkinnedMeshRenderer != null && skinMeshRef != null)
		{
			UpdateSkinMesh();
		}
		if (mode == Mode.Mesh)
		{
			if (m_ComponentsCache.MeshFilter != null)
			{
				m_ComponentsCache.MeshFilter.sharedMesh = m_PolyMesh.ToUnityMesh();
			}
			SetAdditionalVertexStreamsOnRenderer(null);
		}
		else if (!CanApplyAdditionalVertexStreams())
		{
			if (hasAppliedAdditionalVertexStreams)
			{
				RemoveAdditionalVertexStreams();
			}
			Debug.LogWarning($"Warning! The GameObject \"{base.gameObject.name}\" cannot apply it's 'Additional Vertex Streams' mesh, because it's base mesh has changed and has a different vertex count.", this);
		}
		else
		{
			SetAdditionalVertexStreamsOnRenderer(m_PolyMesh.ToUnityMesh());
		}
	}

	internal bool CanApplyAdditionalVertexStreams()
	{
		if (mode == Mode.AdditionalVertexStream && m_ComponentsCache.MeshFilter != null && m_ComponentsCache.MeshFilter.sharedMesh != null && m_ComponentsCache.MeshFilter.sharedMesh.vertexCount != polyMesh.vertexCount)
		{
			return false;
		}
		return true;
	}

	private void UpdateSkinMesh()
	{
		Mesh mesh = skinMeshRef;
		Mesh mesh2 = m_PolyMesh.ToUnityMesh();
		mesh2.boneWeights = mesh.boneWeights;
		mesh2.bindposes = mesh.bindposes;
		m_ComponentsCache.SkinnedMeshRenderer.sharedMesh = mesh2;
	}

	private void SetAdditionalVertexStreamsOnRenderer(Mesh mesh)
	{
		if (m_ComponentsCache.MeshRenderer != null)
		{
			m_ComponentsCache.MeshRenderer.additionalVertexStreams = mesh;
		}
	}

	internal void RemoveAdditionalVertexStreams()
	{
		SetAdditionalVertexStreamsOnRenderer(null);
	}

	private void UpdateMode(Mode newMode)
	{
		if (type == ObjectType.SkinnedMesh && m_Mode != Mode.Mesh)
		{
			m_Mode = Mode.Mesh;
			return;
		}
		m_Mode = newMode;
		if (mode == Mode.AdditionalVertexStream)
		{
			if (m_ComponentsCache.MeshFilter != null)
			{
				m_ComponentsCache.MeshFilter.sharedMesh = m_OriginalMeshObject;
			}
			SetMesh(m_PolyMesh.ToUnityMesh());
		}
		else if (mode == Mode.Mesh)
		{
			SetMesh(m_PolyMesh.ToUnityMesh());
		}
	}

	private void OnEnable()
	{
		m_Initialized = false;
		if (!isInitialized)
		{
			Initialize();
		}
	}

	private void OnDestroy()
	{
		if (Application.isEditor && !Application.isPlaying && Time.frameCount > 0 && (type != ObjectType.Mesh || !(componentsCache.MeshFilter != null) || !(sourceMesh == componentsCache.MeshFilter.sharedMesh)))
		{
			SetMesh(sourceMesh);
		}
	}
}
