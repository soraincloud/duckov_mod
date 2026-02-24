using System;

namespace UnityEngine.Rendering;

public struct BatchPackedCullingViewID : IEquatable<BatchPackedCullingViewID>
{
	internal ulong handle;

	public override int GetHashCode()
	{
		return handle.GetHashCode();
	}

	public bool Equals(BatchPackedCullingViewID other)
	{
		return handle == other.handle;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is BatchPackedCullingViewID))
		{
			return false;
		}
		return Equals((BatchPackedCullingViewID)obj);
	}

	public static bool operator ==(BatchPackedCullingViewID lhs, BatchPackedCullingViewID rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(BatchPackedCullingViewID lhs, BatchPackedCullingViewID rhs)
	{
		return !lhs.Equals(rhs);
	}

	public BatchPackedCullingViewID(int instanceID, int sliceIndex)
	{
		handle = (ulong)((uint)instanceID | ((long)sliceIndex << 32));
	}

	public int GetInstanceID()
	{
		return (int)(handle & 0xFFFFFFFFu);
	}

	public int GetSliceIndex()
	{
		return (int)(handle >> 32);
	}
}
