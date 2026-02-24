namespace UnityEngine.Polybrush;

internal static class MeshChannelUtility
{
	internal static int UVChannelToIndex(MeshChannel channel)
	{
		return channel switch
		{
			MeshChannel.UV0 => 0, 
			MeshChannel.UV2 => 1, 
			MeshChannel.UV3 => 2, 
			MeshChannel.UV4 => 3, 
			_ => -1, 
		};
	}

	internal static MeshChannel ToMask(Mesh mesh)
	{
		MeshChannel meshChannel = MeshChannel.Null;
		if (mesh.vertices.Length != 0)
		{
			meshChannel |= MeshChannel.Position;
		}
		if (mesh.normals.Length != 0)
		{
			meshChannel |= MeshChannel.Normal;
		}
		if (mesh.colors32.Length != 0)
		{
			meshChannel |= MeshChannel.Color;
		}
		if (mesh.tangents.Length != 0)
		{
			meshChannel |= MeshChannel.Tangent;
		}
		if (mesh.uv.Length != 0)
		{
			meshChannel |= MeshChannel.UV0;
		}
		if (mesh.uv2.Length != 0)
		{
			meshChannel |= MeshChannel.UV2;
		}
		if (mesh.uv3.Length != 0)
		{
			meshChannel |= MeshChannel.UV3;
		}
		if (mesh.uv4.Length != 0)
		{
			meshChannel |= MeshChannel.UV4;
		}
		return meshChannel;
	}
}
