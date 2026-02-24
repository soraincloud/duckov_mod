using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

internal class ProbeBrickPool
{
	[DebuggerDisplay("Chunk ({x}, {y}, {z})")]
	public struct BrickChunkAlloc
	{
		public int x;

		public int y;

		public int z;

		internal int flattenIndex(int sx, int sy)
		{
			return z * (sx * sy) + y * sx + x;
		}
	}

	public struct DataLocation
	{
		internal Texture TexL0_L1rx;

		internal Texture TexL1_G_ry;

		internal Texture TexL1_B_rz;

		internal Texture TexL2_0;

		internal Texture TexL2_1;

		internal Texture TexL2_2;

		internal Texture TexL2_3;

		internal Texture3D TexValidity;

		internal int width;

		internal int height;

		internal int depth;

		internal void Cleanup()
		{
			CoreUtils.Destroy(TexL0_L1rx);
			CoreUtils.Destroy(TexL1_G_ry);
			CoreUtils.Destroy(TexL1_B_rz);
			CoreUtils.Destroy(TexL2_0);
			CoreUtils.Destroy(TexL2_1);
			CoreUtils.Destroy(TexL2_2);
			CoreUtils.Destroy(TexL2_3);
			CoreUtils.Destroy(TexValidity);
			TexL0_L1rx = null;
			TexL1_G_ry = null;
			TexL1_B_rz = null;
			TexL2_0 = null;
			TexL2_1 = null;
			TexL2_2 = null;
			TexL2_3 = null;
			TexValidity = null;
		}
	}

	private const int kProbePoolChunkSizeInBricks = 128;

	internal const int kBrickCellCount = 3;

	internal const int kBrickProbeCountPerDim = 4;

	internal const int kBrickProbeCountTotal = 64;

	internal const int kChunkProbeCountPerDim = 512;

	private const int kMaxPoolWidth = 2048;

	internal DataLocation m_Pool;

	private BrickChunkAlloc m_NextFreeChunk;

	private Stack<BrickChunkAlloc> m_FreeList;

	private int m_AvailableChunkCount;

	private ProbeVolumeSHBands m_SHBands;

	private bool m_ContainsValidity;

	internal int estimatedVMemCost { get; private set; }

	internal ProbeBrickPool(ProbeVolumeTextureMemoryBudget memoryBudget, ProbeVolumeSHBands shBands, bool allocateValidityData = true)
	{
		m_NextFreeChunk.x = (m_NextFreeChunk.y = (m_NextFreeChunk.z = 0));
		m_SHBands = shBands;
		m_ContainsValidity = allocateValidityData;
		m_FreeList = new Stack<BrickChunkAlloc>(256);
		DerivePoolSizeFromBudget(memoryBudget, out var width, out var height, out var depth);
		m_Pool = CreateDataLocation(width * height * depth, compressed: false, shBands, "APV", allocateRendertexture: true, allocateValidityData, out var allocatedBytes);
		estimatedVMemCost = allocatedBytes;
		m_AvailableChunkCount = m_Pool.width / 512 * (m_Pool.height / 4) * (m_Pool.depth / 4);
	}

	public int GetRemainingChunkCount()
	{
		return m_AvailableChunkCount;
	}

	internal void EnsureTextureValidity()
	{
		if (m_Pool.TexL0_L1rx == null)
		{
			m_Pool.Cleanup();
			m_Pool = CreateDataLocation(m_Pool.width * m_Pool.height * m_Pool.depth, compressed: false, m_SHBands, "APV", allocateRendertexture: true, m_ContainsValidity, out var allocatedBytes);
			estimatedVMemCost = allocatedBytes;
		}
	}

	internal static int GetChunkSizeInBrickCount()
	{
		return 128;
	}

	internal static int GetChunkSizeInProbeCount()
	{
		return 8192;
	}

	internal int GetPoolWidth()
	{
		return m_Pool.width;
	}

	internal int GetPoolHeight()
	{
		return m_Pool.height;
	}

	internal Vector3Int GetPoolDimensions()
	{
		return new Vector3Int(m_Pool.width, m_Pool.height, m_Pool.depth);
	}

	internal void GetRuntimeResources(ref ProbeReferenceVolume.RuntimeResources rr)
	{
		rr.L0_L1rx = m_Pool.TexL0_L1rx as RenderTexture;
		rr.L1_G_ry = m_Pool.TexL1_G_ry as RenderTexture;
		rr.L1_B_rz = m_Pool.TexL1_B_rz as RenderTexture;
		rr.L2_0 = m_Pool.TexL2_0 as RenderTexture;
		rr.L2_1 = m_Pool.TexL2_1 as RenderTexture;
		rr.L2_2 = m_Pool.TexL2_2 as RenderTexture;
		rr.L2_3 = m_Pool.TexL2_3 as RenderTexture;
		rr.Validity = m_Pool.TexValidity;
	}

	internal void Clear()
	{
		m_FreeList.Clear();
		m_NextFreeChunk.x = (m_NextFreeChunk.y = (m_NextFreeChunk.z = 0));
	}

	internal static int GetChunkCount(int brickCount, int chunkSizeInBricks)
	{
		return (brickCount + chunkSizeInBricks - 1) / chunkSizeInBricks;
	}

	internal bool Allocate(int numberOfBrickChunks, List<BrickChunkAlloc> outAllocations, bool ignoreErrorLog)
	{
		while (m_FreeList.Count > 0 && numberOfBrickChunks > 0)
		{
			outAllocations.Add(m_FreeList.Pop());
			numberOfBrickChunks--;
			m_AvailableChunkCount--;
		}
		for (uint num = 0u; num < numberOfBrickChunks; num++)
		{
			if (m_NextFreeChunk.z >= m_Pool.depth)
			{
				if (!ignoreErrorLog)
				{
					Debug.LogError("Cannot allocate more brick chunks, probe volume brick pool is full.");
				}
				return false;
			}
			outAllocations.Add(m_NextFreeChunk);
			m_AvailableChunkCount--;
			m_NextFreeChunk.x += 512;
			if (m_NextFreeChunk.x >= m_Pool.width)
			{
				m_NextFreeChunk.x = 0;
				m_NextFreeChunk.y += 4;
				if (m_NextFreeChunk.y >= m_Pool.height)
				{
					m_NextFreeChunk.y = 0;
					m_NextFreeChunk.z += 4;
				}
			}
		}
		return true;
	}

	internal void Deallocate(List<BrickChunkAlloc> allocations)
	{
		m_AvailableChunkCount += allocations.Count;
		foreach (BrickChunkAlloc allocation in allocations)
		{
			m_FreeList.Push(allocation);
		}
	}

	internal void Update(DataLocation source, List<BrickChunkAlloc> srcLocations, List<BrickChunkAlloc> dstLocations, int destStartIndex, ProbeVolumeSHBands bands)
	{
		for (int i = 0; i < srcLocations.Count; i++)
		{
			BrickChunkAlloc brickChunkAlloc = srcLocations[i];
			BrickChunkAlloc brickChunkAlloc2 = dstLocations[destStartIndex + i];
			for (int j = 0; j < 4; j++)
			{
				int srcWidth = Mathf.Min(512, source.width - brickChunkAlloc.x);
				Graphics.CopyTexture(source.TexL0_L1rx, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL0_L1rx, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				Graphics.CopyTexture(source.TexL1_G_ry, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL1_G_ry, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				Graphics.CopyTexture(source.TexL1_B_rz, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL1_B_rz, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				if (m_ContainsValidity)
				{
					Graphics.CopyTexture(source.TexValidity, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexValidity, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				}
				if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
				{
					Graphics.CopyTexture(source.TexL2_0, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_0, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					Graphics.CopyTexture(source.TexL2_1, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_1, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					Graphics.CopyTexture(source.TexL2_2, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_2, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
					Graphics.CopyTexture(source.TexL2_3, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexL2_3, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
				}
			}
		}
	}

	internal void UpdateValidity(DataLocation source, List<BrickChunkAlloc> srcLocations, List<BrickChunkAlloc> dstLocations, int destStartIndex)
	{
		for (int i = 0; i < srcLocations.Count; i++)
		{
			BrickChunkAlloc brickChunkAlloc = srcLocations[i];
			BrickChunkAlloc brickChunkAlloc2 = dstLocations[destStartIndex + i];
			for (int j = 0; j < 4; j++)
			{
				int srcWidth = Mathf.Min(512, source.width - brickChunkAlloc.x);
				Graphics.CopyTexture(source.TexValidity, brickChunkAlloc.z + j, 0, brickChunkAlloc.x, brickChunkAlloc.y, srcWidth, 4, m_Pool.TexValidity, brickChunkAlloc2.z + j, 0, brickChunkAlloc2.x, brickChunkAlloc2.y);
			}
		}
	}

	internal static Vector3Int ProbeCountToDataLocSize(int numProbes)
	{
		int num = numProbes / 64;
		int num2 = 512;
		int num3 = (num + num2 * num2 - 1) / (num2 * num2);
		int num4;
		int num5;
		if (num3 > 1)
		{
			num4 = (num5 = num2);
		}
		else
		{
			num5 = (num + num2 - 1) / num2;
			num4 = ((num5 <= 1) ? num : num2);
		}
		num4 *= 4;
		num5 *= 4;
		num3 *= 4;
		return new Vector3Int(num4, num5, num3);
	}

	public static Texture CreateDataTexture(int width, int height, int depth, GraphicsFormat format, string name, bool allocateRendertexture, ref int allocatedBytes)
	{
		allocatedBytes += width * height * depth * format switch
		{
			GraphicsFormat.R8G8B8A8_UNorm => 4, 
			GraphicsFormat.R16G16B16A16_SFloat => 8, 
			_ => 1, 
		};
		Texture texture = ((!allocateRendertexture) ? ((Texture)new Texture3D(width, height, depth, format, TextureCreationFlags.None, 1)) : ((Texture)new RenderTexture(new RenderTextureDescriptor
		{
			width = width,
			height = height,
			volumeDepth = depth,
			graphicsFormat = format,
			mipCount = 1,
			enableRandomWrite = true,
			dimension = TextureDimension.Tex3D,
			msaaSamples = 1
		})));
		texture.hideFlags = HideFlags.HideAndDontSave;
		texture.name = name;
		if (allocateRendertexture)
		{
			(texture as RenderTexture).Create();
		}
		return texture;
	}

	public static DataLocation CreateDataLocation(int numProbes, bool compressed, ProbeVolumeSHBands bands, string name, bool allocateRendertexture, bool allocateValidityData, out int allocatedBytes)
	{
		Vector3Int vector3Int = ProbeCountToDataLocSize(numProbes);
		int x = vector3Int.x;
		int y = vector3Int.y;
		int z = vector3Int.z;
		GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat;
		GraphicsFormat format2 = (compressed ? GraphicsFormat.RGBA_BC7_UNorm : GraphicsFormat.R8G8B8A8_UNorm);
		allocatedBytes = 0;
		DataLocation result = default(DataLocation);
		result.TexL0_L1rx = CreateDataTexture(x, y, z, format, name + "_TexL0_L1rx", allocateRendertexture, ref allocatedBytes);
		result.TexL1_G_ry = CreateDataTexture(x, y, z, format2, name + "_TexL1_G_ry", allocateRendertexture, ref allocatedBytes);
		result.TexL1_B_rz = CreateDataTexture(x, y, z, format2, name + "_TexL1_B_rz", allocateRendertexture, ref allocatedBytes);
		if (allocateValidityData)
		{
			result.TexValidity = CreateDataTexture(x, y, z, GraphicsFormat.R8_UNorm, name + "_Validity", allocateRendertexture: false, ref allocatedBytes) as Texture3D;
		}
		else
		{
			result.TexValidity = null;
		}
		if (bands == ProbeVolumeSHBands.SphericalHarmonicsL2)
		{
			result.TexL2_0 = CreateDataTexture(x, y, z, format2, name + "_TexL2_0", allocateRendertexture, ref allocatedBytes);
			result.TexL2_1 = CreateDataTexture(x, y, z, format2, name + "_TexL2_1", allocateRendertexture, ref allocatedBytes);
			result.TexL2_2 = CreateDataTexture(x, y, z, format2, name + "_TexL2_2", allocateRendertexture, ref allocatedBytes);
			result.TexL2_3 = CreateDataTexture(x, y, z, format2, name + "_TexL2_3", allocateRendertexture, ref allocatedBytes);
		}
		else
		{
			result.TexL2_0 = null;
			result.TexL2_1 = null;
			result.TexL2_2 = null;
			result.TexL2_3 = null;
		}
		result.width = x;
		result.height = y;
		result.depth = z;
		return result;
	}

	private void DerivePoolSizeFromBudget(ProbeVolumeTextureMemoryBudget memoryBudget, out int width, out int height, out int depth)
	{
		width = (int)memoryBudget;
		height = (int)memoryBudget;
		depth = 4;
	}

	internal void Cleanup()
	{
		m_Pool.Cleanup();
	}
}
