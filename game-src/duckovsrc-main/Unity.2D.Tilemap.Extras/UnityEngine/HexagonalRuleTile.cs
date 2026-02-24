using System;

namespace UnityEngine;

public class HexagonalRuleTile<T> : HexagonalRuleTile
{
	public sealed override Type m_NeighborType => typeof(T);
}
[Serializable]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleTile.html")]
public class HexagonalRuleTile : RuleTile
{
	private static float[] m_CosAngleArr1 = new float[6]
	{
		Mathf.Cos(0f),
		Mathf.Cos(-MathF.PI / 3f),
		Mathf.Cos(MathF.PI * -2f / 3f),
		Mathf.Cos(-MathF.PI),
		Mathf.Cos(-4.1887903f),
		Mathf.Cos(-5.2359877f)
	};

	private static float[] m_SinAngleArr1 = new float[6]
	{
		Mathf.Sin(0f),
		Mathf.Sin(-MathF.PI / 3f),
		Mathf.Sin(MathF.PI * -2f / 3f),
		Mathf.Sin(-MathF.PI),
		Mathf.Sin(-4.1887903f),
		Mathf.Sin(-5.2359877f)
	};

	private static float[] m_CosAngleArr2 = new float[6]
	{
		Mathf.Cos(0f),
		Mathf.Cos(MathF.PI / 3f),
		Mathf.Cos(MathF.PI * 2f / 3f),
		Mathf.Cos(MathF.PI),
		Mathf.Cos(4.1887903f),
		Mathf.Cos(5.2359877f)
	};

	private static float[] m_SinAngleArr2 = new float[6]
	{
		Mathf.Sin(0f),
		Mathf.Sin(MathF.PI / 3f),
		Mathf.Sin(MathF.PI * 2f / 3f),
		Mathf.Sin(MathF.PI),
		Mathf.Sin(4.1887903f),
		Mathf.Sin(5.2359877f)
	};

	[DontOverride]
	public bool m_FlatTop;

	private static float m_TilemapToWorldYScale = Mathf.Pow(1f - Mathf.Pow(0.5f, 2f), 0.5f);

	public override int m_RotationAngle => 60;

	public static Vector3 TilemapPositionToWorldPosition(Vector3Int tilemapPosition)
	{
		Vector3 result = new Vector3(tilemapPosition.x, tilemapPosition.y);
		if (tilemapPosition.y % 2 != 0)
		{
			result.x += 0.5f;
		}
		result.y *= m_TilemapToWorldYScale;
		return result;
	}

	public static Vector3Int WorldPositionToTilemapPosition(Vector3 worldPosition)
	{
		worldPosition.y /= m_TilemapToWorldYScale;
		Vector3Int result = new Vector3Int
		{
			y = Mathf.RoundToInt(worldPosition.y)
		};
		if (result.y % 2 != 0)
		{
			result.x = Mathf.RoundToInt(worldPosition.x - 0.5f);
		}
		else
		{
			result.x = Mathf.RoundToInt(worldPosition.x);
		}
		return result;
	}

	public override Vector3Int GetOffsetPosition(Vector3Int position, Vector3Int offset)
	{
		Vector3Int result = position + offset;
		if (offset.y % 2 != 0 && position.y % 2 != 0)
		{
			result.x++;
		}
		return result;
	}

	public override Vector3Int GetOffsetPositionReverse(Vector3Int position, Vector3Int offset)
	{
		return GetOffsetPosition(position, GetRotatedPosition(offset, 180));
	}

	public override Vector3Int GetRotatedPosition(Vector3Int position, int rotation)
	{
		if (rotation != 0)
		{
			Vector3 vector = TilemapPositionToWorldPosition(position);
			int num = rotation / 60;
			vector = ((!m_FlatTop) ? new Vector3(vector.x * m_CosAngleArr1[num] - vector.y * m_SinAngleArr1[num], vector.x * m_SinAngleArr1[num] + vector.y * m_CosAngleArr1[num]) : new Vector3(vector.x * m_CosAngleArr2[num] - vector.y * m_SinAngleArr2[num], vector.x * m_SinAngleArr2[num] + vector.y * m_CosAngleArr2[num]));
			position = WorldPositionToTilemapPosition(vector);
		}
		return position;
	}

	public override Vector3Int GetMirroredPosition(Vector3Int position, bool mirrorX, bool mirrorY)
	{
		if (mirrorX || mirrorY)
		{
			Vector3 worldPosition = TilemapPositionToWorldPosition(position);
			if (m_FlatTop)
			{
				if (mirrorX)
				{
					worldPosition.y *= -1f;
				}
				if (mirrorY)
				{
					worldPosition.x *= -1f;
				}
			}
			else
			{
				if (mirrorX)
				{
					worldPosition.x *= -1f;
				}
				if (mirrorY)
				{
					worldPosition.y *= -1f;
				}
			}
			position = WorldPositionToTilemapPosition(worldPosition);
		}
		return position;
	}
}
