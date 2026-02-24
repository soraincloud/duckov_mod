namespace UnityEngine.TerrainUtils;

public readonly struct TerrainTileCoord
{
	public readonly int tileX;

	public readonly int tileZ;

	public TerrainTileCoord(int tileX, int tileZ)
	{
		this.tileX = tileX;
		this.tileZ = tileZ;
	}
}
