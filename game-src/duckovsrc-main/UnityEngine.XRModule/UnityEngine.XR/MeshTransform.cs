using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.XR;

[UsedByNativeCode]
[NativeHeader("Modules/XR/Subsystems/Meshing/XRMeshBindings.h")]
public readonly struct MeshTransform : IEquatable<MeshTransform>
{
	public MeshId MeshId { get; }

	public ulong Timestamp { get; }

	public Vector3 Position { get; }

	public Quaternion Rotation { get; }

	public Vector3 Scale { get; }

	public MeshTransform(in MeshId meshId, ulong timestamp, in Vector3 position, in Quaternion rotation, in Vector3 scale)
	{
		MeshId = meshId;
		Timestamp = timestamp;
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}

	public override bool Equals(object obj)
	{
		return obj is MeshTransform other && Equals(other);
	}

	public bool Equals(MeshTransform other)
	{
		return MeshId.Equals(other.MeshId) && Timestamp == other.Timestamp && Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
	}

	public static bool operator ==(MeshTransform lhs, MeshTransform rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(MeshTransform lhs, MeshTransform rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override int GetHashCode()
	{
		return HashCodeHelper.Combine(MeshId.GetHashCode(), Timestamp.GetHashCode(), Position.GetHashCode(), Rotation.GetHashCode(), Scale.GetHashCode());
	}
}
