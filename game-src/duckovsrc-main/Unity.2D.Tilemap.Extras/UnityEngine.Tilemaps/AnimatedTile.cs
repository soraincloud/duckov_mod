using System;

namespace UnityEngine.Tilemaps;

[Serializable]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/AnimatedTile.html")]
public class AnimatedTile : TileBase
{
	public Sprite[] m_AnimatedSprites;

	public float m_MinSpeed = 1f;

	public float m_MaxSpeed = 1f;

	public float m_AnimationStartTime;

	public int m_AnimationStartFrame;

	public Tile.ColliderType m_TileColliderType;

	public TileAnimationFlags m_TileAnimationFlags;

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		tileData.transform = Matrix4x4.identity;
		tileData.color = Color.white;
		if (m_AnimatedSprites != null && m_AnimatedSprites.Length != 0)
		{
			tileData.sprite = m_AnimatedSprites[m_AnimatedSprites.Length - 1];
			tileData.colliderType = m_TileColliderType;
		}
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		if (m_AnimatedSprites.Length != 0)
		{
			tileAnimationData.animatedSprites = m_AnimatedSprites;
			tileAnimationData.animationSpeed = Random.Range(m_MinSpeed, m_MaxSpeed);
			tileAnimationData.animationStartTime = m_AnimationStartTime;
			tileAnimationData.flags = m_TileAnimationFlags;
			if (0 < m_AnimationStartFrame && m_AnimationStartFrame <= m_AnimatedSprites.Length)
			{
				Tilemap component = tilemap.GetComponent<Tilemap>();
				if (component != null && component.animationFrameRate > 0f)
				{
					tileAnimationData.animationStartTime = (float)(m_AnimationStartFrame - 1) / component.animationFrameRate;
				}
			}
			return true;
		}
		return false;
	}
}
