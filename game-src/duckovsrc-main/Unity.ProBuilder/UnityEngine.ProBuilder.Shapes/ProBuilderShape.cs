using System;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes;

[AddComponentMenu("")]
[DisallowMultipleComponent]
internal sealed class ProBuilderShape : MonoBehaviour
{
	[SerializeReference]
	private Shape m_Shape = new Cube();

	[SerializeField]
	private Vector3 m_Size = Vector3.one;

	[SerializeField]
	private Quaternion m_Rotation = Quaternion.identity;

	private ProBuilderMesh m_Mesh;

	[SerializeField]
	private PivotLocation m_PivotLocation;

	[SerializeField]
	private Vector3 m_PivotPosition;

	[SerializeField]
	internal ushort m_UnmodifiedMeshVersion;

	private Bounds m_EditionBounds;

	[SerializeField]
	private Bounds m_ShapeBox;

	public Shape shape
	{
		get
		{
			return m_Shape;
		}
		set
		{
			m_Shape = value;
		}
	}

	public PivotLocation pivotLocation
	{
		get
		{
			return m_PivotLocation;
		}
		set
		{
			m_PivotLocation = value;
		}
	}

	public Vector3 pivotLocalPosition
	{
		get
		{
			return m_PivotPosition;
		}
		set
		{
			m_PivotPosition = value;
		}
	}

	public Vector3 pivotGlobalPosition
	{
		get
		{
			return mesh.transform.TransformPoint(m_PivotPosition);
		}
		set
		{
			pivotLocalPosition = mesh.transform.InverseTransformPoint(value);
		}
	}

	public Vector3 size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size.x = ((System.Math.Abs(value.x) == 0f) ? (Mathf.Sign(m_Size.x) * 0.001f) : value.x);
			m_Size.y = value.y;
			m_Size.z = ((System.Math.Abs(value.z) == 0f) ? (Mathf.Sign(m_Size.z) * 0.001f) : value.z);
		}
	}

	public Quaternion rotation
	{
		get
		{
			return m_Rotation;
		}
		set
		{
			m_Rotation = value;
		}
	}

	public Bounds editionBounds
	{
		get
		{
			m_EditionBounds.center = m_ShapeBox.center;
			m_EditionBounds.size = m_Size;
			if (Mathf.Abs(m_ShapeBox.size.y) < Mathf.Epsilon)
			{
				m_EditionBounds.size = new Vector3(m_Size.x, 0f, m_Size.z);
			}
			return m_EditionBounds;
		}
	}

	public Bounds shapeBox => m_ShapeBox;

	public bool isEditable => m_UnmodifiedMeshVersion == mesh.versionIndex;

	public ProBuilderMesh mesh
	{
		get
		{
			if (m_Mesh == null)
			{
				m_Mesh = GetComponent<ProBuilderMesh>();
			}
			if (m_Mesh == null)
			{
				m_Mesh = base.gameObject.AddComponent<ProBuilderMesh>();
			}
			return m_Mesh;
		}
	}

	private void OnValidate()
	{
		m_Size.x = ((System.Math.Abs(m_Size.x) == 0f) ? 0.001f : m_Size.x);
		m_Size.z = ((System.Math.Abs(m_Size.z) == 0f) ? 0.001f : m_Size.z);
	}

	internal void UpdateComponent()
	{
		ResetPivot(mesh, size, rotation);
		Rebuild();
	}

	internal void UpdateBounds(Bounds bounds)
	{
		Vector3 center = mesh.transform.InverseTransformPoint(bounds.center);
		Bounds bounds2 = m_ShapeBox;
		bounds2.center = center;
		m_ShapeBox = bounds2;
		ResetPivot(mesh, m_Size, m_Rotation);
		size = bounds.size;
		Rebuild();
	}

	internal void Rebuild(Bounds bounds, Quaternion rotation, Vector3 cornerPivot)
	{
		Transform obj = base.transform;
		obj.position = bounds.center;
		obj.rotation = rotation;
		size = bounds.size;
		pivotGlobalPosition = ((pivotLocation == PivotLocation.Center) ? bounds.center : cornerPivot);
		Rebuild();
	}

	private void Rebuild()
	{
		if (!(base.gameObject == null) && base.gameObject.hideFlags != HideFlags.HideAndDontSave)
		{
			m_ShapeBox = m_Shape.RebuildMesh(mesh, size, rotation);
			RebuildPivot(size, rotation);
			Bounds currentSize = m_ShapeBox;
			currentSize.size = m_ShapeBox.size.Abs();
			MeshUtility.FitToSize(mesh, currentSize, size);
			m_UnmodifiedMeshVersion = mesh.versionIndex;
		}
	}

	internal void SetShape(Shape shape, PivotLocation location)
	{
		m_PivotLocation = location;
		m_Shape = shape;
		if (m_Shape is Plane || m_Shape is Sprite)
		{
			Bounds bounds = m_ShapeBox;
			Vector3 center = bounds.center;
			Vector3 vector = bounds.size;
			center.y = 0f;
			vector.y = 0f;
			bounds.center = center;
			bounds.size = vector;
			m_ShapeBox = bounds;
			m_Size.y = 0f;
		}
		else if (pivotLocation == PivotLocation.FirstCorner && m_ShapeBox.size.y == 0f && size.y != 0f)
		{
			Bounds bounds2 = m_ShapeBox;
			Vector3 center2 = bounds2.center;
			Vector3 vector2 = bounds2.size;
			center2.y += size.y / 2f;
			vector2.y = size.y;
			bounds2.center = center2;
			bounds2.size = vector2;
			m_ShapeBox = bounds2;
		}
		ResetPivot(mesh, size, rotation);
		Rebuild();
	}

	internal void RotateInsideBounds(Quaternion deltaRotation)
	{
		ResetPivot(mesh, size, rotation);
		rotation = deltaRotation * rotation;
		Rebuild();
	}

	private void ResetPivot(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		if (mesh != null && mesh.mesh != null)
		{
			Vector3 worldPosition = mesh.transform.TransformPoint(m_ShapeBox.center);
			Vector3 position = mesh.transform.TransformPoint(m_PivotPosition);
			mesh.SetPivot(worldPosition);
			m_PivotPosition = mesh.transform.InverseTransformPoint(position);
			m_ShapeBox = m_Shape.UpdateBounds(mesh, size, rotation, m_ShapeBox);
		}
	}

	internal void RebuildPivot(Vector3 size, Quaternion rotation)
	{
		if (mesh != null)
		{
			Vector3 position = mesh.transform.TransformPoint(m_ShapeBox.center);
			Vector3 vector = mesh.transform.TransformPoint(m_PivotPosition);
			mesh.SetPivot(vector);
			m_ShapeBox.center = mesh.transform.InverseTransformPoint(position);
			m_PivotPosition = mesh.transform.InverseTransformPoint(vector);
			m_ShapeBox = m_Shape.UpdateBounds(mesh, size, rotation, m_ShapeBox);
		}
	}
}
