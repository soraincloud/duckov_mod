using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Economy;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCPrerequisite;

internal static class MCMaterialCraftingService
{
    private const string MaterialCraftCategoryDisplayNameKey = "CraftFilter_MCMaterial";
    private const string FormulaGoldNuggetToIngot = "MCMaterial_GoldNugget_To_GoldIngot";
    private const string FormulaGoldIngotToBlock = "MCMaterial_GoldIngot_To_GoldBlock";
    private const string FormulaIronNuggetToIngot = "MCMaterial_IronNugget_To_IronIngot";
    private const string FormulaIronIngotToBlock = "MCMaterial_IronIngot_To_IronBlock";
    private const string FormulaGoldIngotToNugget = "MCMaterial_GoldIngot_To_GoldNugget";
    private const string FormulaGoldBlockToIngot = "MCMaterial_GoldBlock_To_GoldIngot";
    private const string FormulaIronIngotToNugget = "MCMaterial_IronIngot_To_IronNugget";
    private const string FormulaIronBlockToIngot = "MCMaterial_IronBlock_To_IronIngot";
    private const string FormulaGoldenDumbbell = "MCMaterial_GoldIngot_To_GoldenDumbbell";
    private const string FormulaGoldPoop = "MCMaterial_GoldIngot_To_GoldPoop";
    private const string FormulaPureGoldBadge = "MCMaterial_GoldIngot_To_PureGoldBadge";
    private const string FormulaPeaceStar = "MCMaterial_GoldIngot_To_PeaceStar";
    private const string FormulaAprilTrophy = "MCMaterial_GoldIngot_To_AprilTrophy";
    private const string FormulaPointTwoBTC = "MCMaterial_GoldIngot_To_PointTwoBTC";
    private const string FormulaGoldenPentagram = "MCMaterial_GoldIngot_To_GoldenPentagram";
    private const string FormulaGoldRing = "MCMaterial_GoldIngot_To_GoldRing";

    private static readonly string[] ManagedFormulaIds =
    {
        FormulaGoldNuggetToIngot,
        FormulaGoldIngotToBlock,
        FormulaIronNuggetToIngot,
        FormulaIronIngotToBlock,
        FormulaGoldIngotToNugget,
        FormulaGoldBlockToIngot,
        FormulaIronIngotToNugget,
        FormulaIronBlockToIngot,
        FormulaGoldenDumbbell,
        FormulaGoldPoop,
        FormulaPureGoldBadge,
        FormulaPeaceStar,
        FormulaAprilTrophy,
        FormulaPointTwoBTC,
        FormulaGoldenPentagram,
        FormulaGoldRing
    };

    private static readonly ExternalGoldRecipeDefinition[] ExternalGoldRecipes =
    {
        new(FormulaGoldenDumbbell, 16, "黄金哑铃", "黄金哑铃"),
        new(FormulaGoldPoop, 8, "金粑粑", "金粑粑"),
        new(FormulaPureGoldBadge, 6, "纯金徽章", "纯金徽章"),
        new(FormulaPeaceStar, 6, "和平星", "和平星"),
        new(FormulaAprilTrophy, 3, "奖杯四月", "奖杯四月", "奖杯 四月", "四月奖杯", "奖杯4月", "奖杯（四月）"),
        new(FormulaPointTwoBTC, 3, "0.2BTC", "0.2BTC", "02BTC"),
        new(FormulaGoldenPentagram, 2, "金色五角星", "金色五角星"),
        new(FormulaGoldRing, 2, "金戒指", "金戒指")
    };

    private static bool _initialized;

    public static void Initialize()
    {
        LocalizationManager.SetOverrideText(MaterialCraftCategoryDisplayNameKey, "MC材料");

        if (_initialized)
        {
            RegisterCraftingFormulas();
            return;
        }

        _initialized = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        RegisterCraftingFormulas();
    }

    public static void Deinitialize()
    {
        if (_initialized)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        _initialized = false;
        UnregisterCraftingFormulas();
    }

    public static string MaterialCraftCategoryDisplayName => MaterialCraftCategoryDisplayNameKey;

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterCraftingFormulas();
    }

    public static void RegisterCraftingFormulas()
    {
        var formulas = CraftingFormulaCollection.Instance;
        if (formulas == null)
        {
            return;
        }

        var formulaList = ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list");
        if (formulaList == null)
        {
            return;
        }

        formulaList.RemoveAll(existing => ManagedFormulaIds.Contains(existing.id, StringComparer.Ordinal));

        var compatibleTags = BuildCompatibleFormulaTags(formulas);
        var builtFormulas = BuildFormulas(compatibleTags).ToList();
        formulaList.AddRange(builtFormulas);

        foreach (var formulaId in builtFormulas.Select(formula => formula.id))
        {
            EnsureFormulaUnlocked(formulaId);
        }
    }

    private static IEnumerable<CraftingFormula> BuildFormulas(string[] compatibleTags)
    {
        var craftOnlyCategoryIds = new List<int>();

        yield return BuildFormula(FormulaGoldNuggetToIngot, MaterialItemRegistry.GoldIngotTypeId, 1, MaterialItemRegistry.GoldNuggetTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaGoldIngotToBlock, MaterialItemRegistry.GoldBlockTypeId, 1, MaterialItemRegistry.GoldIngotTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaIronNuggetToIngot, MaterialItemRegistry.IronIngotTypeId, 1, MaterialItemRegistry.IronNuggetTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaIronIngotToBlock, MaterialItemRegistry.IronBlockTypeId, 1, MaterialItemRegistry.IronIngotTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaGoldIngotToNugget, MaterialItemRegistry.GoldNuggetTypeId, 9, MaterialItemRegistry.GoldIngotTypeId, 1, compatibleTags);
        yield return BuildFormula(FormulaGoldBlockToIngot, MaterialItemRegistry.GoldIngotTypeId, 9, MaterialItemRegistry.GoldBlockTypeId, 1, compatibleTags);
        yield return BuildFormula(FormulaIronIngotToNugget, MaterialItemRegistry.IronNuggetTypeId, 9, MaterialItemRegistry.IronIngotTypeId, 1, compatibleTags);
        yield return BuildFormula(FormulaIronBlockToIngot, MaterialItemRegistry.IronIngotTypeId, 9, MaterialItemRegistry.IronBlockTypeId, 1, compatibleTags);

        foreach (var recipe in ExternalGoldRecipes)
        {
            var resultTypeId = ResolveTypeId(recipe.Label, recipe.Candidates);
            if (resultTypeId < 0)
            {
                continue;
            }

            craftOnlyCategoryIds.Add(resultTypeId);
            yield return BuildFormula(recipe.FormulaId, resultTypeId, 1, MaterialItemRegistry.GoldIngotTypeId, recipe.GoldIngotCost, compatibleTags);
        }

        if (craftOnlyCategoryIds.Count > 0)
        {
            MCCategoryService.EnsureCraftOnlyCategoryTagged(craftOnlyCategoryIds);
        }
    }

    private static CraftingFormula BuildFormula(string formulaId, int resultId, int resultAmount, int ingredientId, long ingredientAmount, string[] compatibleTags)
    {
        return new CraftingFormula
        {
            id = formulaId,
            result = new CraftingFormula.ItemEntry
            {
                id = resultId,
                amount = resultAmount
            },
            tags = compatibleTags,
            cost = new Cost((ingredientId, ingredientAmount)),
            unlockByDefault = true,
            lockInDemo = false,
            requirePerk = string.Empty,
            hideInIndex = false
        };
    }

    private static string[] BuildCompatibleFormulaTags(CraftingFormulaCollection formulas)
    {
        var tags = new HashSet<string>(StringComparer.Ordinal);
        var formulaList = ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list");
        if (formulaList != null)
        {
            foreach (var existing in formulaList)
            {
                if (existing.tags == null)
                {
                    continue;
                }

                foreach (var tag in existing.tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        tags.Add(tag);
                    }
                }
            }
        }

        return tags.OrderBy(value => value, StringComparer.Ordinal).ToArray();
    }

    private static void EnsureFormulaUnlocked(string formulaId)
    {
        if (CraftingManager.Instance == null)
        {
            return;
        }

        var unlockedFormulaIds = ReflectionUtil.GetPrivateField<List<string>>(CraftingManager.Instance, "unlockedFormulaIDs");
        if (unlockedFormulaIds == null || unlockedFormulaIds.Contains(formulaId))
        {
            return;
        }

        unlockedFormulaIds.Add(formulaId);
        unlockedFormulaIds.Sort(StringComparer.Ordinal);
    }

    private static void UnregisterCraftingFormulas()
    {
        var formulas = CraftingFormulaCollection.Instance;
        var formulaList = formulas != null ? ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list") : null;
        formulaList?.RemoveAll(existing => ManagedFormulaIds.Contains(existing.id, StringComparer.Ordinal));

        if (CraftingManager.Instance != null)
        {
            var unlockedFormulaIds = ReflectionUtil.GetPrivateField<List<string>>(CraftingManager.Instance, "unlockedFormulaIDs");
            unlockedFormulaIds?.RemoveAll(existing => ManagedFormulaIds.Contains(existing, StringComparer.Ordinal));
        }
    }

    private static int ResolveTypeId(string label, params string[] candidates)
    {
        var collection = ItemAssetsCollection.Instance;
        if (collection?.entries == null)
        {
            return -1;
        }

        var normalizedCandidates = candidates
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(NormalizeLookupText)
            .Distinct()
            .ToArray();

        foreach (var entry in collection.entries)
        {
            if (entry == null || entry.metaData.id <= 0)
            {
                continue;
            }

            if (MatchesEntry(entry.metaData, normalizedCandidates, contains: false))
            {
                return entry.typeID;
            }
        }

        foreach (var entry in collection.entries)
        {
            if (entry == null || entry.metaData.id <= 0)
            {
                continue;
            }

            if (MatchesEntry(entry.metaData, normalizedCandidates, contains: true))
            {
                return entry.typeID;
            }
        }

        Debug.LogWarning($"[MCPrerequisite] Failed to resolve crafting result '{label}'. Candidates: {string.Join(", ", candidates)}");
        return -1;
    }

    private static bool MatchesEntry(ItemMetaData metaData, string[] normalizedCandidates, bool contains)
    {
        var names = new[]
        {
            metaData.Name,
            metaData.DisplayName,
            metaData.DisplayNameKey
        }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizeLookupText)
            .Distinct()
            .ToArray();

        foreach (var candidate in normalizedCandidates)
        {
            foreach (var name in names)
            {
                if (!contains && string.Equals(name, candidate, StringComparison.Ordinal))
                {
                    return true;
                }

                if (contains && (name.Contains(candidate, StringComparison.Ordinal) || candidate.Contains(name, StringComparison.Ordinal)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string NormalizeLookupText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = value.Trim().Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
        return new string(chars);
    }

    private readonly struct ExternalGoldRecipeDefinition
    {
        public ExternalGoldRecipeDefinition(string formulaId, long goldIngotCost, string label, params string[] candidates)
        {
            FormulaId = formulaId;
            GoldIngotCost = goldIngotCost;
            Label = label;
            Candidates = candidates;
        }

        public string FormulaId { get; }
        public long GoldIngotCost { get; }
        public string Label { get; }
        public string[] Candidates { get; }
    }
}