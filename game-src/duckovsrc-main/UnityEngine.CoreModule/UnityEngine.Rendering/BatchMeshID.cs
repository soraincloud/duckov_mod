using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering;

[NativeClass("BatchMeshID")]
[RequiredByNativeCode(Optional = true, GenerateProxy = true)]
[NativeHeader("Runtime/Camera/BatchRendererGroup.h")]
public struct BatchMeshID : IEquatable<BatchMeshID>
{
	public static readonly BatchMeshID Null = new BatchMeshID
	{
		value = 0u
	};

	public uint value;

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is BatchMeshID)
		{
			return Equals((BatchMeshID)obj);
		}
		return false;
	}

	public bool Equals(BatchMeshID other)
	{
		return value == other.value;
	}

	public int CompareTo(BatchMeshID other)
	{
		return value.CompareTo(other.value);
	}

	public static bool operator ==(BatchMeshID a, BatchMeshID b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(BatchMeshID a, BatchMeshID b)
	{
		return !a.Equals(b);
	}
}
