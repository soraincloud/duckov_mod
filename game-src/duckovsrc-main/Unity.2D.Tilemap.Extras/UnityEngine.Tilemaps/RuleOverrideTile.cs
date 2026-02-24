using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Tilemaps;

[Serializable]
[MovedFrom(true, "UnityEngine", null, null)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleOverrideTile.html")]
public class RuleOverrideTile : TileBase
{
	[Serializable]
	public class TileSpritePair
	{
		public Sprite m_OriginalSprite;

		public Sprite m_OverrideSprite;
	}

	[Serializable]
	public class TileGameObjectPair
	{
		public GameObject m_OriginalGameObject;

		public GameObject m_OverrideGameObject;
	}

	public RuleTile m_Tile;

	public List<TileSpritePair> m_Sprites = new List<TileSpritePair>();

	public List<TileGameObjectPair> m_GameObjects = new List<TileGameObjectPair>();

	[HideInInspector]
	public RuleTile m_InstanceTile;

	public Sprite this[Sprite originalSprite]
	{
		get
		{
			foreach (TileSpritePair sprite in m_Sprites)
			{
				if (sprite.m_OriginalSprite == originalSprite)
				{
					return sprite.m_OverrideSprite;
				}
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_Sprites = m_Sprites.Where((TileSpritePair spritePair) => spritePair.m_OriginalSprite != originalSprite).ToList();
				return;
			}
			foreach (TileSpritePair sprite in m_Sprites)
			{
				if (sprite.m_OriginalSprite == originalSprite)
				{
					sprite.m_OverrideSprite = value;
					return;
				}
			}
			m_Sprites.Add(new TileSpritePair
			{
				m_OriginalSprite = originalSprite,
				m_OverrideSprite = value
			});
		}
	}

	public GameObject this[GameObject originalGameObject]
	{
		get
		{
			foreach (TileGameObjectPair gameObject in m_GameObjects)
			{
				if (gameObject.m_OriginalGameObject == originalGameObject)
				{
					return gameObject.m_OverrideGameObject;
				}
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				m_GameObjects = m_GameObjects.Where((TileGameObjectPair gameObjectPair) => gameObjectPair.m_OriginalGameObject != originalGameObject).ToList();
				return;
			}
			foreach (TileGameObjectPair gameObject in m_GameObjects)
			{
				if (gameObject.m_OriginalGameObject == originalGameObject)
				{
					gameObject.m_OverrideGameObject = value;
					return;
				}
			}
			m_GameObjects.Add(new TileGameObjectPair
			{
				m_OriginalGameObject = originalGameObject,
				m_OverrideGameObject = value
			});
		}
	}

	private void CreateInstanceTile()
	{
		RuleTile ruleTile = ScriptableObject.CreateInstance(m_Tile.GetType()) as RuleTile;
		ruleTile.hideFlags = HideFlags.NotEditable;
		ruleTile.name = m_Tile.name + " (Override)";
		m_InstanceTile = ruleTile;
	}

	public void ApplyOverrides(IList<KeyValuePair<Sprite, Sprite>> overrides)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		for (int i = 0; i < overrides.Count; i++)
		{
			this[overrides[i].Key] = overrides[i].Value;
		}
	}

	public void ApplyOverrides(IList<KeyValuePair<GameObject, GameObject>> overrides)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		for (int i = 0; i < overrides.Count; i++)
		{
			this[overrides[i].Key] = overrides[i].Value;
		}
	}

	public void GetOverrides(List<KeyValuePair<Sprite, Sprite>> overrides, ref int validCount)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		overrides.Clear();
		List<Sprite> list = new List<Sprite>();
		if ((bool)m_Tile)
		{
			if ((bool)m_Tile.m_DefaultSprite)
			{
				list.Add(m_Tile.m_DefaultSprite);
			}
			foreach (RuleTile.TilingRule tilingRule in m_Tile.m_TilingRules)
			{
				Sprite[] sprites = tilingRule.m_Sprites;
				foreach (Sprite sprite in sprites)
				{
					if ((bool)sprite && !list.Contains(sprite))
					{
						list.Add(sprite);
					}
				}
			}
		}
		validCount = list.Count;
		foreach (TileSpritePair sprite2 in m_Sprites)
		{
			if (!list.Contains(sprite2.m_OriginalSprite))
			{
				list.Add(sprite2.m_OriginalSprite);
			}
		}
		foreach (Sprite item in list)
		{
			overrides.Add(new KeyValuePair<Sprite, Sprite>(item, this[item]));
		}
	}

	public void GetOverrides(List<KeyValuePair<GameObject, GameObject>> overrides, ref int validCount)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		overrides.Clear();
		List<GameObject> list = new List<GameObject>();
		if ((bool)m_Tile)
		{
			if ((bool)m_Tile.m_DefaultGameObject)
			{
				list.Add(m_Tile.m_DefaultGameObject);
			}
			foreach (RuleTile.TilingRule tilingRule in m_Tile.m_TilingRules)
			{
				if ((bool)tilingRule.m_GameObject && !list.Contains(tilingRule.m_GameObject))
				{
					list.Add(tilingRule.m_GameObject);
				}
			}
		}
		validCount = list.Count;
		foreach (TileGameObjectPair gameObject in m_GameObjects)
		{
			if (!list.Contains(gameObject.m_OriginalGameObject))
			{
				list.Add(gameObject.m_OriginalGameObject);
			}
		}
		foreach (GameObject item in list)
		{
			overrides.Add(new KeyValuePair<GameObject, GameObject>(item, this[item]));
		}
	}

	public virtual void Override()
	{
		if (!m_Tile)
		{
			return;
		}
		if (!m_InstanceTile)
		{
			CreateInstanceTile();
		}
		PrepareOverride();
		RuleTile instanceTile = m_InstanceTile;
		instanceTile.m_DefaultSprite = this[instanceTile.m_DefaultSprite] ?? instanceTile.m_DefaultSprite;
		instanceTile.m_DefaultGameObject = this[instanceTile.m_DefaultGameObject] ?? instanceTile.m_DefaultGameObject;
		foreach (RuleTile.TilingRule tilingRule in instanceTile.m_TilingRules)
		{
			for (int i = 0; i < tilingRule.m_Sprites.Length; i++)
			{
				Sprite sprite = tilingRule.m_Sprites[i];
				tilingRule.m_Sprites[i] = this[sprite] ?? sprite;
			}
			tilingRule.m_GameObject = this[tilingRule.m_GameObject] ?? tilingRule.m_GameObject;
		}
	}

	public void PrepareOverride()
	{
		RuleTile tempTile = Object.Instantiate(m_InstanceTile);
		Dictionary<FieldInfo, object> dictionary = m_InstanceTile.GetCustomFields(isOverrideInstance: true).ToDictionary((FieldInfo field) => field, (FieldInfo field) => field.GetValue(tempTile));
		JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(m_Tile), m_InstanceTile);
		foreach (KeyValuePair<FieldInfo, object> item in dictionary)
		{
			item.Key.SetValue(m_InstanceTile, item.Value);
		}
	}

	public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
	{
		if (!m_InstanceTile)
		{
			return false;
		}
		return m_InstanceTile.GetTileAnimationData(position, tilemap, ref tileAnimationData);
	}

	public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
	{
		if ((bool)m_InstanceTile)
		{
			m_InstanceTile.GetTileData(position, tilemap, ref tileData);
		}
	}

	public override void RefreshTile(Vector3Int position, ITilemap tilemap)
	{
		if ((bool)m_InstanceTile)
		{
			m_InstanceTile.RefreshTile(position, tilemap);
		}
	}

	public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
	{
		if (!m_InstanceTile)
		{
			return true;
		}
		return m_InstanceTile.StartUp(position, tilemap, go);
	}

	public void OnEnable()
	{
		if (!(m_Tile == null) && m_InstanceTile == null)
		{
			Override();
		}
	}
}
