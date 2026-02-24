using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Rendering/2D/Shadow Caster 2D")]
[MovedFrom("UnityEngine.Experimental.Rendering.Universal")]
public class ShadowCaster2D : ShadowCasterGroup2D, ISerializationCallbackReceiver
{
	public enum ComponentVersions
	{
		Version_Unserialized,
		Version_1
	}

	private const ComponentVersions k_CurrentComponentVersion = ComponentVersions.Version_1;

	[SerializeField]
	private ComponentVersions m_ComponentVersion;

	[SerializeField]
	private bool m_HasRenderer;

	[SerializeField]
	private bool m_UseRendererSilhouette = true;

	[SerializeField]
	private bool m_CastsShadows = true;

	[SerializeField]
	private bool m_SelfShadows;

	[SerializeField]
	private int[] m_ApplyToSortingLayers;

	[SerializeField]
	private Vector3[] m_ShapePath;

	[SerializeField]
	private int m_ShapePathHash;

	[SerializeField]
	private Mesh m_Mesh;

	[SerializeField]
	private int m_InstanceId;

	internal ShadowCasterGroup2D m_ShadowCasterGroup;

	internal ShadowCasterGroup2D m_PreviousShadowCasterGroup;

	[SerializeField]
	internal Bounds m_LocalBounds;

	internal BoundingSphere m_BoundingSphere;

	private int m_PreviousShadowGroup;

	private bool m_PreviousCastsShadows = true;

	private int m_PreviousPathHash;

	internal Vector3 m_CachedPosition;

	internal Vector3 m_CachedLossyScale;

	internal Quaternion m_CachedRotation;

	internal Matrix4x4 m_CachedShadowMatrix;

	internal Matrix4x4 m_CachedInverseShadowMatrix;

	internal Matrix4x4 m_CachedLocalToWorldMatrix;

	public Mesh mesh => m_Mesh;

	public Vector3[] shapePath => m_ShapePath;

	internal int shapePathHash
	{
		get
		{
			return m_ShapePathHash;
		}
		set
		{
			m_ShapePathHash = value;
		}
	}

	public bool useRendererSilhouette
	{
		get
		{
			if (m_UseRendererSilhouette)
			{
				return m_HasRenderer;
			}
			return false;
		}
		set
		{
			m_UseRendererSilhouette = value;
		}
	}

	public bool selfShadows
	{
		get
		{
			return m_SelfShadows;
		}
		set
		{
			m_SelfShadows = value;
		}
	}

	public bool castsShadows
	{
		get
		{
			return m_CastsShadows;
		}
		set
		{
			m_CastsShadows = value;
		}
	}

	internal override void CacheValues()
	{
		m_CachedPosition = base.transform.position;
		m_CachedLossyScale = base.transform.lossyScale;
		m_CachedRotation = base.transform.rotation;
		m_CachedShadowMatrix = Matrix4x4.TRS(m_CachedPosition, m_CachedRotation, Vector3.one);
		m_CachedInverseShadowMatrix = m_CachedShadowMatrix.inverse;
		m_CachedLocalToWorldMatrix = base.transform.localToWorldMatrix;
	}

	private static int[] SetDefaultSortingLayers()
	{
		int num = SortingLayer.layers.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = SortingLayer.layers[i].id;
		}
		return array;
	}

	internal bool IsLit(Light2D light)
	{
		Vector3 vector = default(Vector3);
		vector.x = light.m_CachedPosition.x - m_BoundingSphere.position.x;
		vector.y = light.m_CachedPosition.y - m_BoundingSphere.position.y;
		vector.z = light.m_CachedPosition.z - m_BoundingSphere.position.z;
		float num = Vector3.SqrMagnitude(vector);
		float num2 = light.boundingSphere.radius + m_BoundingSphere.radius;
		return num <= num2 * num2;
	}

	internal bool IsShadowedLayer(int layer)
	{
		if (m_ApplyToSortingLayers == null)
		{
			return false;
		}
		return Array.IndexOf(m_ApplyToSortingLayers, layer) >= 0;
	}

	private void Awake()
	{
		if (m_ApplyToSortingLayers == null)
		{
			m_ApplyToSortingLayers = SetDefaultSortingLayers();
		}
		Bounds bounds = new Bounds(base.transform.position, Vector3.one);
		Renderer component = GetComponent<Renderer>();
		if (component != null)
		{
			bounds = component.bounds;
		}
		else
		{
			Collider2D component2 = GetComponent<Collider2D>();
			if (component2 != null)
			{
				bounds = component2.bounds;
			}
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = base.transform.position;
		if (base.transform.lossyScale.x != 0f && base.transform.lossyScale.y != 0f)
		{
			vector = new Vector3(1f / base.transform.lossyScale.x, 1f / base.transform.lossyScale.y);
			vector2 = new Vector3(vector.x * (0f - base.transform.position.x), vector.y * (0f - base.transform.position.y));
		}
		if (m_ShapePath == null || m_ShapePath.Length == 0)
		{
			m_ShapePath = new Vector3[4]
			{
				vector2 + new Vector3(vector.x * bounds.min.x, vector.y * bounds.min.y),
				vector2 + new Vector3(vector.x * bounds.min.x, vector.y * bounds.max.y),
				vector2 + new Vector3(vector.x * bounds.max.x, vector.y * bounds.max.y),
				vector2 + new Vector3(vector.x * bounds.max.x, vector.y * bounds.min.y)
			};
		}
	}

	protected void OnEnable()
	{
		if (m_Mesh == null || m_InstanceId != GetInstanceID())
		{
			m_Mesh = new Mesh();
			m_LocalBounds = ShadowUtility.GenerateShadowMesh(m_Mesh, m_ShapePath);
			m_InstanceId = GetInstanceID();
		}
		m_ShadowCasterGroup = null;
	}

	protected void OnDisable()
	{
		ShadowCasterGroup2DManager.RemoveFromShadowCasterGroup(this, m_ShadowCasterGroup);
	}

	public void Update()
	{
		m_HasRenderer = TryGetComponent<Renderer>(out var _);
		if (LightUtility.CheckForChange(m_ShapePathHash, ref m_PreviousPathHash))
		{
			m_LocalBounds = ShadowUtility.GenerateShadowMesh(m_Mesh, m_ShapePath);
		}
		m_PreviousShadowCasterGroup = m_ShadowCasterGroup;
		if (ShadowCasterGroup2DManager.AddToShadowCasterGroup(this, ref m_ShadowCasterGroup) && m_ShadowCasterGroup != null)
		{
			if (m_PreviousShadowCasterGroup == this)
			{
				ShadowCasterGroup2DManager.RemoveGroup(this);
			}
			ShadowCasterGroup2DManager.RemoveFromShadowCasterGroup(this, m_PreviousShadowCasterGroup);
			if (m_ShadowCasterGroup == this)
			{
				ShadowCasterGroup2DManager.AddGroup(this);
			}
		}
		if (LightUtility.CheckForChange(m_ShadowGroup, ref m_PreviousShadowGroup))
		{
			ShadowCasterGroup2DManager.RemoveGroup(this);
			ShadowCasterGroup2DManager.AddGroup(this);
		}
		if (LightUtility.CheckForChange(m_CastsShadows, ref m_PreviousCastsShadows))
		{
			if (m_CastsShadows)
			{
				ShadowCasterGroup2DManager.AddGroup(this);
			}
			else
			{
				ShadowCasterGroup2DManager.RemoveGroup(this);
			}
		}
		UpdateBoundingSphere();
	}

	public void OnBeforeSerialize()
	{
		m_ComponentVersion = ComponentVersions.Version_1;
	}

	public void OnAfterDeserialize()
	{
		if (m_ComponentVersion == ComponentVersions.Version_Unserialized)
		{
			m_LocalBounds = ShadowUtility.CalculateLocalBounds(m_ShapePath);
			m_ComponentVersion = ComponentVersions.Version_1;
		}
	}

	private void UpdateBoundingSphere()
	{
		Vector3 vector = base.transform.TransformPoint(m_LocalBounds.max);
		Vector3 vector2 = base.transform.TransformPoint(m_LocalBounds.min);
		Vector3 vector3 = 0.5f * (vector + vector2);
		float rad = Vector3.Magnitude(vector - vector3);
		m_BoundingSphere = new BoundingSphere(vector3, rad);
	}
}
