using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace UnityEngine;

public class RuleTile<T> : RuleTile
{
	public sealed override Type m_NeighborType => typeof(T);
}
[Serializable]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleTile.html")]
public class RuleTile : TileBase
{
	[Serializable]
	public class TilingRuleOutput
	{
		public class Neighbor
		{
			public const int This = 1;

			public const int NotThis = 2;
		}

		public enum Transform
		{
			Fixed,
			Rotated,
			MirrorX,
			MirrorY,
			MirrorXY,
			RotatedMirror
		}

		public enum OutputSprite
		{
			Single,
			Random,
			Animation
		}

		public int m_Id;

		public Sprite[] m_Sprites = new Sprite[1];

		public GameObject m_GameObject;

		[FormerlySerializedAs("m_AnimationSpeed")]
		public float m_MinAnimationSpeed = 1f;

		[FormerlySerializedAs("m_AnimationSpeed")]
		public float m_MaxAnimationSpeed = 1f;

		public float m_PerlinScale = 0.5f;

		public OutputSprite m_Output;

		public Tile.ColliderType m_ColliderType = Tile.ColliderType.Sprite;

		public Transform m_RandomTransform;
	}

	[Serializable]
	public class TilingRule : TilingRuleOutput
	{
		public List<int> m_Neighbors = new List<int>();

		public List<Vector3Int> m_NeighborPositions = new List<Vector3Int>
		{
			new Vector3Int(-1, 1, 0),
			new Vector3Int(0, 1, 0),
			new Vector3Int(1, 1, 0),
			new Vector3Int(-1, 0, 0),
			new Vector3Int(1, 0, 0),
			new Vector3Int(-1, -1, 0),
			new Vector3Int(0, -1, 0),
			new Vector3Int(1, -1, 0)
		};

		public Transform m_RuleTransform;

		public TilingRule Clone()
		{
			TilingRule tilingRule = new TilingRule
			{
				m_Neighbors = new List<int>(m_Neighbors),
				m_NeighborPositions = new List<Vector3Int>(m_NeighborPositions),
				m_RuleTransform = m_RuleTransform,
				m_Sprites = new Sprite[m_Sprites.Length],
				m_GameObject = m_GameObject,
				m_MinAnimationSpeed = m_MinAnimationSpeed,
				m_MaxAnimationSpeed = m_MaxAnimationSpeed,
				m_PerlinScale = m_PerlinScale,
				m_Output = m_Output,
				m_ColliderType = m_ColliderType,
				m_RandomTransform = m_RandomTransform
			};
			Array.Copy(m_Sprites, tilingRule.m_Sprites, m_Sprites.Length);
			return tilingRule;
		}

		public Dictionary<Vector3Int, int> GetNeighbors()
		{
			Dictionary<Vector3Int, int> dictionary = new Dictionary<Vector3Int, int>();
			for (int i = 0; i < m_Neighbors.Count && i < m_NeighborPositions.Count; i++)
			{
				dictionary.Add(m_NeighborPositions[i], m_Neighbors[i]);
			}
			return dictionary;
		}

		public void ApplyNeighbors(Dictionary<Vector3Int, int> dict)
		{
			m_NeighborPositions = dict.Keys.ToList();
			m_Neighbors = dict.Values.ToList();
		}

		public BoundsInt GetBounds()
		{
			BoundsInt result = new BoundsInt(Vector3Int.zero, Vector3Int.one);
			foreach (KeyValuePair<Vector3Int, int> neighbor in GetNeighbors())
			{
				result.xMin = Mathf.Min(result.xMin, neighbor.Key.x);
				result.yMin = Mathf.Min(result.yMin, neighbor.Key.y);
				result.xMax = Mathf.Max(result.xMax, neighbor.Key.x + 1);
				result.yMax = Mathf.Max(result.yMax, neighbor.Key.y + 1);
			}
			return result;
		}
	}

	public class DontOverride : Attribute
	{
	}

	public Sprite m_DefaultSprite;

	public GameObject m_DefaultGameObject;

	public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

	[HideInInspector]
	public List<TilingRule> m_TilingRules = new List<TilingRule>();

	private HashSet<Vector3Int> m_NeighborPositions = new HashSet<Vector3Int>();

	private static Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>> m_CacheTilemapsNeighborPositions = new Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>>();

	private static TileBase[] m_AllocatedUsedTileArr = Array.Empty<TileBase>();

	public virtual Type m_NeighborType => typeof(TilingRuleOutput.Neighbor);

	public virtual int m_RotationAngle => 90;

	public int m_RotationCount => 360 / m_RotationAngle;

	public HashSet<Vector3Int> neighborPositions
	{
		get
		{
			if (m_NeighborPositions.Count == 0)
			{
				UpdateNeighborPositions();
			}
			return m_NeighborPositions;
		}
	}

	public void UpdateNeighborPositions()
	{
		m_CacheTilemapsNeighborPositions.Clear();
		HashSet<Vector3Int> hashSet = m_NeighborPositions;
		hashSet.Clear();
		foreach (TilingRule tilingRule in m_TilingRules)
		{
			foreach (KeyValuePair<Vector3Int, int> neighbor in tilingRule.GetNeighbors())
			{
				Vector3Int key = neighbor.Key;
				hashSet.Add(key);
				if (tilingRule.m_RuleTransform == TilingRuleOutput.Transform.Rotated)
				{
					for (int i = m_RotationAngle; i < 360; i += m_RotationAngle)
					{
						hashSet.Add(GetRotatedPosition(key, i));
					}
				}
				else if (tilingRule.m_RuleTransform == TilingRuleOutput.Transform.MirrorXY)
				{
					hashSet.Add(GetMirroredPosition(key, mirrorX: true, mirrorY: true));
					hashSet.Add(GetMirroredPosition(key, mirrorX: true, mirrorY: false));
					hashSet.Add(GetMirroredPosition(key, mirrorX: false, mirrorY: true));
				}
				else if (tilingRule.m_RuleTransform == TilingRuleOutput.Transform.MirrorX)
				{
					hashSet.Add(GetMirroredPosition(key, mirrorX: true, mirrorY: false));
				}
				else if (tilingRule.m_RuleTransform == TilingRuleOutput.Transform.MirrorY)
				{
					hashSet.Add(GetMirroredPosition(key, mirrorX: false, mirrorY: true));
				}
				else if (tilingRule.m_RuleTransform == TilingRuleOutput.Transform.RotatedMirror)
				{
					Vector3Int mirroredPosition = GetMirroredPosition(key, mirrorX: true, mirrorY: false);
					for (int j = m_RotationAngle; j < 360; j += m_RotationAngle)
					{
						hashSet.Add(GetRotatedPosition(key, j));
						hashSet.Add(GetRotatedPosition(mirroredPosition, j));
					}
				}
			}
		}
	}

	public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject instantiatedGameObject)
	{
		if (instantiatedGameObject != null)
		{
			Tilemap component = tilemap.GetComponent<Tilemap>();
			Matrix4x4 orientationMatrix = component.orientationMatrix;
			Matrix4x4 identity = Matrix4x4.identity;
			Vector3 vector = default(Vector3);
			Quaternion localRotation = default(Quaternion);
			Vector3 localScale = default(Vector3);
			bool flag = false;
			Matrix4x4 transform = identity;
			foreach (TilingRule tilingRule in m_TilingRules)
			{
				if (RuleMatches(tilingRule, position, tilemap, ref transform))
				{
					transform = orientationMatrix * transform;
					vector = new Vector3(transform.m03, transform.m13, transform.m23);
					localRotation = Quaternion.LookRotation(new Vector3(transform.m02, transform.m12, transform.m22), new Vector3(transform.m01, transform.m11, transform.m21));
					localScale = transform.lossyScale;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				vector = new Vector3(orientationMatrix.m03, orientationMatrix.m13, orientationMatrix.m23);
				localRotation = Quaternion.LookRotation(new Vector3(orientationMatrix.m02, orientationMatrix.m12, orientationMatrix.m22), new Vector3(orientationMatrix.m01, orientationMatrix.m11, orientationMatrix.m21));
				localScale = orientationMatrix.lossyScale;
			}
			instantiatedGameObject.transform.localPosition = vector + component.CellToLocalInterpolated(position + component.tileAnchor);
			instantiatedGameObject.transform.localRotation = localRotation;
			instantiatedGameObject.transform.localScale = localScale;
		}
		return true;
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		tileData.sprite = m_DefaultSprite;
		tileData.gameObject = m_DefaultGameObject;
		tileData.colliderType = m_DefaultColliderType;
		tileData.flags = TileFlags.LockTransform;
		tileData.transform = identity;
		Matrix4x4 transform = identity;
		foreach (TilingRule tilingRule in m_TilingRules)
		{
			if (!RuleMatches(tilingRule, position, tilemap, ref transform))
			{
				continue;
			}
			switch (tilingRule.m_Output)
			{
			case TilingRuleOutput.OutputSprite.Single:
			case TilingRuleOutput.OutputSprite.Animation:
				tileData.sprite = tilingRule.m_Sprites[0];
				break;
			case TilingRuleOutput.OutputSprite.Random:
			{
				int num = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, tilingRule.m_PerlinScale, 100000f) * (float)tilingRule.m_Sprites.Length), 0, tilingRule.m_Sprites.Length - 1);
				tileData.sprite = tilingRule.m_Sprites[num];
				if (tilingRule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
				{
					transform = ApplyRandomTransform(tilingRule.m_RandomTransform, transform, tilingRule.m_PerlinScale, position);
				}
				break;
			}
			}
			tileData.transform = transform;
			tileData.gameObject = tilingRule.m_GameObject;
			tileData.colliderType = tilingRule.m_ColliderType;
			break;
		}
	}

	public static float GetPerlinValue(Vector3Int position, float scale, float offset)
	{
		return Mathf.PerlinNoise(((float)position.x + offset) * scale, ((float)position.y + offset) * scale);
	}

	private static bool IsTilemapUsedTilesChange(Tilemap tilemap, out KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> hashSet)
	{
		if (!m_CacheTilemapsNeighborPositions.TryGetValue(tilemap, out hashSet))
		{
			return true;
		}
		HashSet<TileBase> key = hashSet.Key;
		int usedTilesCount = tilemap.GetUsedTilesCount();
		if (usedTilesCount != key.Count)
		{
			return true;
		}
		if (m_AllocatedUsedTileArr.Length < usedTilesCount)
		{
			Array.Resize(ref m_AllocatedUsedTileArr, usedTilesCount);
		}
		tilemap.GetUsedTilesNonAlloc(m_AllocatedUsedTileArr);
		for (int i = 0; i < usedTilesCount; i++)
		{
			TileBase item = m_AllocatedUsedTileArr[i];
			if (!key.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	private static KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> CachingTilemapNeighborPositions(Tilemap tilemap)
	{
		int usedTilesCount = tilemap.GetUsedTilesCount();
		HashSet<TileBase> hashSet = new HashSet<TileBase>();
		HashSet<Vector3Int> hashSet2 = new HashSet<Vector3Int>();
		if (m_AllocatedUsedTileArr.Length < usedTilesCount)
		{
			Array.Resize(ref m_AllocatedUsedTileArr, usedTilesCount);
		}
		tilemap.GetUsedTilesNonAlloc(m_AllocatedUsedTileArr);
		for (int i = 0; i < usedTilesCount; i++)
		{
			TileBase tileBase = m_AllocatedUsedTileArr[i];
			hashSet.Add(tileBase);
			RuleTile ruleTile = null;
			if (tileBase is RuleTile ruleTile2)
			{
				ruleTile = ruleTile2;
			}
			else if (tileBase is RuleOverrideTile ruleOverrideTile)
			{
				ruleTile = ruleOverrideTile.m_Tile;
			}
			if (!ruleTile)
			{
				continue;
			}
			foreach (Vector3Int neighborPosition in ruleTile.neighborPositions)
			{
				hashSet2.Add(neighborPosition);
			}
		}
		KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>> keyValuePair = new KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>(hashSet, hashSet2);
		m_CacheTilemapsNeighborPositions[tilemap] = keyValuePair;
		return keyValuePair;
	}

	private static bool NeedRelease()
	{
		foreach (KeyValuePair<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>> cacheTilemapsNeighborPosition in m_CacheTilemapsNeighborPositions)
		{
			if (cacheTilemapsNeighborPosition.Key == null)
			{
				return true;
			}
		}
		return false;
	}

	private static void ReleaseDestroyedTilemapCacheData()
	{
		if (!NeedRelease())
		{
			return;
		}
		bool flag = false;
		Tilemap[] array = m_CacheTilemapsNeighborPositions.Keys.ToArray();
		foreach (Tilemap tilemap in array)
		{
			if (tilemap == null && m_CacheTilemapsNeighborPositions.Remove(tilemap))
			{
				flag = true;
			}
		}
		if (flag)
		{
			m_CacheTilemapsNeighborPositions = new Dictionary<Tilemap, KeyValuePair<HashSet<TileBase>, HashSet<Vector3Int>>>(m_CacheTilemapsNeighborPositions);
		}
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		Matrix4x4 transform = Matrix4x4.identity;
		foreach (TilingRule tilingRule in m_TilingRules)
		{
			if (tilingRule.m_Output == TilingRuleOutput.OutputSprite.Animation && RuleMatches(tilingRule, position, tilemap, ref transform))
			{
				tileAnimationData.animatedSprites = tilingRule.m_Sprites;
				tileAnimationData.animationSpeed = Random.Range(tilingRule.m_MinAnimationSpeed, tilingRule.m_MaxAnimationSpeed);
				return true;
			}
		}
		return false;
	}

	public override void RefreshTile(Vector3Int position, ITilemap tilemap)
	{
		base.RefreshTile(position, tilemap);
		Tilemap component = tilemap.GetComponent<Tilemap>();
		ReleaseDestroyedTilemapCacheData();
		if (IsTilemapUsedTilesChange(component, out var hashSet))
		{
			hashSet = CachingTilemapNeighborPositions(component);
		}
		foreach (Vector3Int item in hashSet.Value)
		{
			Vector3Int offsetPositionReverse = GetOffsetPositionReverse(position, item);
			TileBase tile = tilemap.GetTile(offsetPositionReverse);
			RuleTile ruleTile = null;
			if (tile is RuleTile ruleTile2)
			{
				ruleTile = ruleTile2;
			}
			else if (tile is RuleOverrideTile ruleOverrideTile)
			{
				ruleTile = ruleOverrideTile.m_Tile;
			}
			if (ruleTile != null && (ruleTile == this || ruleTile.neighborPositions.Contains(item)))
			{
				base.RefreshTile(offsetPositionReverse, tilemap);
			}
		}
	}

	public virtual bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
	{
		if (RuleMatches(rule, position, tilemap, 0))
		{
			transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, 0f), Vector3.one);
			return true;
		}
		if (rule.m_RuleTransform == TilingRuleOutput.Transform.Rotated)
		{
			for (int i = m_RotationAngle; i < 360; i += m_RotationAngle)
			{
				if (RuleMatches(rule, position, tilemap, i))
				{
					transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -i), Vector3.one);
					return true;
				}
			}
		}
		else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorXY)
		{
			if (RuleMatches(rule, position, tilemap, mirrorX: true, mirrorY: true))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, -1f, 1f));
				return true;
			}
			if (RuleMatches(rule, position, tilemap, mirrorX: true, mirrorY: false))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
				return true;
			}
			if (RuleMatches(rule, position, tilemap, mirrorX: false, mirrorY: true))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
				return true;
			}
		}
		else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorX)
		{
			if (RuleMatches(rule, position, tilemap, mirrorX: true, mirrorY: false))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
				return true;
			}
		}
		else if (rule.m_RuleTransform == TilingRuleOutput.Transform.MirrorY)
		{
			if (RuleMatches(rule, position, tilemap, mirrorX: false, mirrorY: true))
			{
				transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
				return true;
			}
		}
		else if (rule.m_RuleTransform == TilingRuleOutput.Transform.RotatedMirror)
		{
			for (int j = 0; j < 360; j += m_RotationAngle)
			{
				if (j != 0 && RuleMatches(rule, position, tilemap, j))
				{
					transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -j), Vector3.one);
					return true;
				}
				if (RuleMatches(rule, position, tilemap, j, mirrorX: true))
				{
					transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -j), new Vector3(-1f, 1f, 1f));
					return true;
				}
			}
		}
		return false;
	}

	public virtual Matrix4x4 ApplyRandomTransform(TilingRuleOutput.Transform type, Matrix4x4 original, float perlinScale, Vector3Int position)
	{
		float perlinValue = GetPerlinValue(position, perlinScale, 200000f);
		switch (type)
		{
		case TilingRuleOutput.Transform.MirrorXY:
			return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((Math.Abs((double)perlinValue - 0.5) > 0.25) ? 1f : (-1f), ((double)perlinValue < 0.5) ? 1f : (-1f), 1f));
		case TilingRuleOutput.Transform.MirrorX:
			return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(((double)perlinValue < 0.5) ? 1f : (-1f), 1f, 1f));
		case TilingRuleOutput.Transform.MirrorY:
			return original * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, ((double)perlinValue < 0.5) ? 1f : (-1f), 1f));
		case TilingRuleOutput.Transform.Rotated:
		{
			int num2 = Mathf.Clamp(Mathf.FloorToInt(perlinValue * (float)m_RotationCount), 0, m_RotationCount - 1) * m_RotationAngle;
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -num2), Vector3.one);
		}
		case TilingRuleOutput.Transform.RotatedMirror:
		{
			int num = Mathf.Clamp(Mathf.FloorToInt(perlinValue * (float)m_RotationCount), 0, m_RotationCount - 1) * m_RotationAngle;
			return Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -num), new Vector3(((double)perlinValue < 0.5) ? 1f : (-1f), 1f, 1f));
		}
		default:
			return original;
		}
	}

	public FieldInfo[] GetCustomFields(bool isOverrideInstance)
	{
		return (from field in GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where typeof(RuleTile).GetField(field.Name) == null
			where field.IsPublic || field.IsDefined(typeof(SerializeField))
			where !field.IsDefined(typeof(HideInInspector))
			where !isOverrideInstance || !field.IsDefined(typeof(DontOverride))
			select field).ToArray();
	}

	public virtual bool RuleMatch(int neighbor, TileBase other)
	{
		if (other is RuleOverrideTile ruleOverrideTile)
		{
			other = ruleOverrideTile.m_InstanceTile;
		}
		return neighbor switch
		{
			1 => other == this, 
			2 => other != this, 
			_ => true, 
		};
	}

	public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle, bool mirrorX = false)
	{
		int num = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
		for (int i = 0; i < num; i++)
		{
			int neighbor = rule.m_Neighbors[i];
			Vector3Int position2 = rule.m_NeighborPositions[i];
			if (mirrorX)
			{
				position2 = GetMirroredPosition(position2, mirrorX: true, mirrorY: false);
			}
			Vector3Int rotatedPosition = GetRotatedPosition(position2, angle);
			TileBase tile = tilemap.GetTile(GetOffsetPosition(position, rotatedPosition));
			if (!RuleMatch(neighbor, tile))
			{
				return false;
			}
		}
		return true;
	}

	public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
	{
		int num = Math.Min(rule.m_Neighbors.Count, rule.m_NeighborPositions.Count);
		for (int i = 0; i < num; i++)
		{
			int neighbor = rule.m_Neighbors[i];
			Vector3Int mirroredPosition = GetMirroredPosition(rule.m_NeighborPositions[i], mirrorX, mirrorY);
			TileBase tile = tilemap.GetTile(GetOffsetPosition(position, mirroredPosition));
			if (!RuleMatch(neighbor, tile))
			{
				return false;
			}
		}
		return true;
	}

	public virtual Vector3Int GetRotatedPosition(Vector3Int position, int rotation)
	{
		return rotation switch
		{
			0 => position, 
			90 => new Vector3Int(position.y, -position.x, 0), 
			180 => new Vector3Int(-position.x, -position.y, 0), 
			270 => new Vector3Int(-position.y, position.x, 0), 
			_ => position, 
		};
	}

	public virtual Vector3Int GetMirroredPosition(Vector3Int position, bool mirrorX, bool mirrorY)
	{
		if (mirrorX)
		{
			position.x *= -1;
		}
		if (mirrorY)
		{
			position.y *= -1;
		}
		return position;
	}

	public virtual Vector3Int GetOffsetPosition(Vector3Int position, Vector3Int offset)
	{
		return position + offset;
	}

	public virtual Vector3Int GetOffsetPositionReverse(Vector3Int position, Vector3Int offset)
	{
		return position - offset;
	}
}
