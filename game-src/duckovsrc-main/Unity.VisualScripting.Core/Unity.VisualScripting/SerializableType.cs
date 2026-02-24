using System;

namespace Unity.VisualScripting;

[Serializable]
[SerializationVersion("A", new Type[] { })]
public struct SerializableType : IEquatable<SerializableType>, IComparable<SerializableType>
{
	[Serialize]
	public string Identification;

	public SerializableType(string identification)
	{
		Identification = identification;
	}

	public bool Equals(SerializableType other)
	{
		return string.Equals(Identification, other.Identification);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is SerializableType other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Identification?.GetHashCode() ?? 0;
	}

	public static bool operator ==(SerializableType left, SerializableType right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(SerializableType left, SerializableType right)
	{
		return !left.Equals(right);
	}

	public int CompareTo(SerializableType other)
	{
		return string.Compare(Identification, other.Identification, StringComparison.Ordinal);
	}
}
