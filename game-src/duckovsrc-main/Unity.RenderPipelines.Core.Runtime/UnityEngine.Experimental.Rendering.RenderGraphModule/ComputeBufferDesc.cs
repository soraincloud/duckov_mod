using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.RenderGraphModule;

public struct ComputeBufferDesc
{
	public int count;

	public int stride;

	public ComputeBufferType type;

	public string name;

	public ComputeBufferDesc(int count, int stride)
	{
		this = default(ComputeBufferDesc);
		this.count = count;
		this.stride = stride;
		type = ComputeBufferType.Default;
	}

	public ComputeBufferDesc(int count, int stride, ComputeBufferType type)
	{
		this = default(ComputeBufferDesc);
		this.count = count;
		this.stride = stride;
		this.type = type;
	}

	public override int GetHashCode()
	{
		HashFNV1A32 hashFNV1A = HashFNV1A32.Create();
		hashFNV1A.Append(in count);
		hashFNV1A.Append(in stride);
		int input = (int)type;
		hashFNV1A.Append(in input);
		return hashFNV1A.value;
	}
}
