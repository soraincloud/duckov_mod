using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[RequiredByNativeCode]
[NativeHeader("Modules/XR/Subsystems/Meshing/XRMeshBindings.h")]
public struct MeshGenerationResult : IEquatable<MeshGenerationResult>
{
	public MeshId MeshId { get; }

	public Mesh Mesh { get; }

	public MeshCollider MeshCollider { get; }

	public MeshGenerationStatus Status { get; }

	public MeshVertexAttributes Attributes { get; }

	public ulong Timestamp { get; }

	public Vector3 Position { get; }

	public Quaternion Rotation { get; }

	public Vector3 Scale { get; }

	public override bool Equals(object obj)
	{
		if (!(obj is MeshGenerationResult))
		{
			return false;
		}
		return Equals((MeshGenerationResult)obj);
	}

	public bool Equals(MeshGenerationResult other)
	{
		return MeshId.Equals(other.MeshId) && Mesh.Equals(other.Mesh) && MeshCollider.Equals(other.MeshCollider) && Status == other.Status && Attributes == other.Attributes && Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
	}

	public static bool operator ==(MeshGenerationResult lhs, MeshGenerationResult rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(MeshGenerationResult lhs, MeshGenerationResult rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.Combine(MeshId.GetHashCode(), Mesh.GetHashCode(), MeshCollider.GetHashCode(), ((int)Status).GetHashCode(), ((int)Attributes).GetHashCode(), Position.GetHashCode(), Rotation.GetHashCode(), Scale.GetHashCode());
	}
}
