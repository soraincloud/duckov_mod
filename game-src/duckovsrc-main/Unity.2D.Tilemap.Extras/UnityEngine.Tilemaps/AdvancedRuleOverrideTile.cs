using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Tilemaps;

[Serializable]
[MovedFrom(true, "UnityEngine", null, null)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.2d.tilemap.extras@latest/index.html?subfolder=/manual/RuleOverrideTile.html")]
public class AdvancedRuleOverrideTile : RuleOverrideTile
{
	public Sprite m_DefaultSprite;

	public GameObject m_DefaultGameObject;

	public Tile.ColliderType m_DefaultColliderType = Tile.ColliderType.Sprite;

	public List<RuleTile.TilingRuleOutput> m_OverrideTilingRules = new List<RuleTile.TilingRuleOutput>();

	public RuleTile.TilingRuleOutput this[RuleTile.TilingRule originalRule]
	{
		get
		{
			foreach (RuleTile.TilingRuleOutput overrideTilingRule in m_OverrideTilingRules)
			{
				if (overrideTilingRule.m_Id == originalRule.m_Id)
				{
					return overrideTilingRule;
				}
			}
			return null;
		}
		set
		{
			for (int num = m_OverrideTilingRules.Count - 1; num >= 0; num--)
			{
				if (m_OverrideTilingRules[num].m_Id == originalRule.m_Id)
				{
					m_OverrideTilingRules.RemoveAt(num);
					break;
				}
			}
			if (value != null)
			{
				RuleTile.TilingRuleOutput item = JsonUtility.FromJson<RuleTile.TilingRuleOutput>(JsonUtility.ToJson(value));
				m_OverrideTilingRules.Add(item);
			}
		}
	}

	public void ApplyOverrides(IList<KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>> overrides)
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

	public void GetOverrides(List<KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>> overrides, ref int validCount)
	{
		if (overrides == null)
		{
			throw new ArgumentNullException("overrides");
		}
		overrides.Clear();
		if ((bool)m_Tile)
		{
			foreach (RuleTile.TilingRule tilingRule in m_Tile.m_TilingRules)
			{
				RuleTile.TilingRuleOutput value = this[tilingRule];
				overrides.Add(new KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>(tilingRule, value));
			}
		}
		validCount = overrides.Count;
		foreach (RuleTile.TilingRuleOutput overrideRule in m_OverrideTilingRules)
		{
			if (!overrides.Exists((KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput> o) => o.Key.m_Id == overrideRule.m_Id))
			{
				RuleTile.TilingRule key = new RuleTile.TilingRule
				{
					m_Id = overrideRule.m_Id
				};
				overrides.Add(new KeyValuePair<RuleTile.TilingRule, RuleTile.TilingRuleOutput>(key, overrideRule));
			}
		}
	}

	public override void Override()
	{
		if (!m_Tile || !m_InstanceTile)
		{
			return;
		}
		PrepareOverride();
		RuleTile instanceTile = m_InstanceTile;
		instanceTile.m_DefaultSprite = m_DefaultSprite;
		instanceTile.m_DefaultGameObject = m_DefaultGameObject;
		instanceTile.m_DefaultColliderType = m_DefaultColliderType;
		foreach (RuleTile.TilingRule tilingRule in instanceTile.m_TilingRules)
		{
			RuleTile.TilingRuleOutput tilingRuleOutput = this[tilingRule];
			if (tilingRuleOutput != null)
			{
				JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(tilingRuleOutput), tilingRule);
			}
		}
	}
}
