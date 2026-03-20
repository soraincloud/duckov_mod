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
    private const string FormulaBlackWhiteDisplay = "MCMaterial_Materials_To_BlackWhiteDisplay";
    private const string FormulaBlackWhiteDisplayReverse = "MCMaterial_Materials_To_BlackWhiteDisplay_Reverse";
    private const string FormulaWhiskey = "MCMaterial_Glass_To_Whiskey";
    private const string FormulaThermite = "MCMaterial_Glass_To_Thermite";
    private const string FormulaVodka = "MCMaterial_Glass_To_Vodka";
    private const string FormulaKetchup = "MCMaterial_Glass_To_Ketchup";
    private const string FormulaInk = "MCMaterial_Glass_To_Ink";
    private const string FormulaTelescope = "MCMaterial_IronNugget_To_Telescope";
    private const string FormulaShinyGlasses = "MCMaterial_IronNugget_To_ShinyGlasses";
    private const string FormulaSunGlasses = "MCMaterial_IronNugget_To_SunGlasses";
    private const string FormulaBlackGlasses = "MCMaterial_IronNugget_To_BlackGlasses";
    private const string FormulaSkiGoggles = "MCMaterial_IronNugget_To_SkiGoggles";
    private const string FormulaSyringe = "MCMaterial_Glass_To_Syringe";
    private const string FormulaClock = "MCMaterial_Glass_To_Clock";
    private const string FormulaFlashLight = "MCMaterial_Glass_To_FlashLight";
    private const string FormulaLightBulb = "MCMaterial_Glass_To_LightBulb";
    private const string FormulaUltravioletLamp = "MCMaterial_Glass_To_UltravioletLamp";
    private const string FormulaEnergySavingLamp = "MCMaterial_Glass_To_EnergySavingLamp";
    private const string FormulaMetalOilBarrel = "MCMaterial_IronIngot_To_MetalOilBarrel";
    private const string FormulaMetalBucket = "MCMaterial_IronIngot_To_MetalBucket";
    private const string FormulaPropane = "MCMaterial_IronIngot_To_Propane";
    private const string FormulaGolfClub = "MCMaterial_IronIngot_To_GolfClub";
    private const string FormulaPot = "MCMaterial_IronIngot_To_Pot";
    private const string FormulaSledgeHammer = "MCMaterial_IronIngot_To_SledgeHammer";
    private const string FormulaCrowbar = "MCMaterial_IronIngot_To_Crowbar";
    private const string FormulaShovel = "MCMaterial_IronIngot_To_Shovel";
    private const string FormulaTrap = "MCMaterial_IronIngot_To_Trap";
    private const string FormulaWrench = "MCMaterial_IronIngot_To_Wrench";
    private const string FormulaHammer = "MCMaterial_IronIngot_To_Hammer";
    private const string FormulaFlatScrewdriver = "MCMaterial_IronNugget_To_FlatScrewdriver";
    private const string FormulaAdvancedWeaponParts = "MCMaterial_IronNugget_To_AdvancedWeaponParts";
    private const string FormulaMediumWeaponParts = "MCMaterial_IronNugget_To_MediumWeaponParts";
    private const string FormulaWeaponParts = "MCMaterial_IronNugget_To_WeaponParts";
    private const string FormulaNut = "MCMaterial_IronNugget_To_Nut";
    private const string FormulaNail = "MCMaterial_IronNugget_To_Nail";
    private const string FormulaBolt = "MCMaterial_IronNugget_To_Bolt";
    private const string FormulaKeroseneLamp = "MCMaterial_GlassAndIron_To_KeroseneLamp";
    private const string FormulaKeroseneLampReverse = "MCMaterial_GlassAndIron_To_KeroseneLamp_Reverse";

    private static readonly ExternalRecipeDefinition[] ExternalRecipes =
    {
        new(FormulaGoldenDumbbell, "黄金哑铃", new[] { (MaterialItemRegistry.GoldIngotTypeId, 16L) }, "黄金哑铃"),
        new(FormulaGoldPoop, "金粑粑", new[] { (MaterialItemRegistry.GoldIngotTypeId, 8L) }, "金粑粑"),
        new(FormulaPureGoldBadge, "纯金徽章", new[] { (MaterialItemRegistry.GoldIngotTypeId, 6L) }, "纯金徽章"),
        new(FormulaPeaceStar, "和平星", new[] { (MaterialItemRegistry.GoldIngotTypeId, 6L) }, "和平星"),
        new(FormulaAprilTrophy, "奖杯四月", new[] { (MaterialItemRegistry.GoldIngotTypeId, 3L) }, "奖杯四月", "奖杯 四月", "四月奖杯", "奖杯4月", "奖杯（四月）"),
        new(FormulaPointTwoBTC, "0.2BTC", new[] { (MaterialItemRegistry.GoldIngotTypeId, 3L) }, "0.2BTC", "02BTC"),
        new(FormulaGoldenPentagram, "金色五角星", new[] { (MaterialItemRegistry.GoldIngotTypeId, 2L) }, "金色五角星"),
        new(FormulaGoldRing, "金戒指", new[] { (MaterialItemRegistry.GoldIngotTypeId, 2L) }, "金戒指"),
        new(FormulaBlackWhiteDisplay, "黑白显示器", new[] { (MaterialItemRegistry.GlassTypeId, 5L), (MaterialItemRegistry.IronIngotTypeId, 3L) }, "黑白显示器"),
        new(FormulaWhiskey, "威士忌", new[] { (MaterialItemRegistry.GlassTypeId, 4L) }, "威士忌"),
        new(FormulaThermite, "铝热剂", new[] { (MaterialItemRegistry.GlassTypeId, 4L) }, "铝热剂"),
        new(FormulaVodka, "伏特加", new[] { (MaterialItemRegistry.GlassTypeId, 3L) }, "伏特加"),
        new(FormulaKetchup, "番茄酱", new[] { (MaterialItemRegistry.GlassTypeId, 3L) }, "番茄酱"),
        new(FormulaInk, "墨水", new[] { (MaterialItemRegistry.GlassTypeId, 2L) }, "墨水"),
        new(FormulaTelescope, "望远镜", new[] { (MaterialItemRegistry.IronNuggetTypeId, 4L) }, "望远镜"),
        new(FormulaShinyGlasses, "闪光的眼镜", new[] { (MaterialItemRegistry.IronNuggetTypeId, 4L) }, "闪光的眼镜", "闪光眼镜"),
        new(FormulaSunGlasses, "太阳镜", new[] { (MaterialItemRegistry.IronNuggetTypeId, 4L) }, "太阳镜"),
        new(FormulaBlackGlasses, "黑色眼镜", new[] { (MaterialItemRegistry.IronNuggetTypeId, 4L) }, "黑色眼镜"),
        new(FormulaSkiGoggles, "滑雪镜", new[] { (MaterialItemRegistry.IronNuggetTypeId, 4L) }, "滑雪镜"),
        new(FormulaSyringe, "注射器", new[] { (MaterialItemRegistry.GlassTypeId, 1L) }, "注射器"),
        new(FormulaClock, "闹钟", new[] { (MaterialItemRegistry.GlassTypeId, 1L) }, "闹钟"),
        new(FormulaFlashLight, "手电", new[] { (MaterialItemRegistry.GlassTypeId, 1L) }, "手电", "手电筒"),
        new(FormulaLightBulb, "灯泡", new[] { (MaterialItemRegistry.GlassTypeId, 3L) }, "灯泡"),
        new(FormulaUltravioletLamp, "紫外灯", new[] { (MaterialItemRegistry.GlassTypeId, 3L) }, "紫外灯"),
        new(FormulaEnergySavingLamp, "节能灯", new[] { (MaterialItemRegistry.GlassTypeId, 3L) }, "节能灯"),
        new(FormulaMetalOilBarrel, "金属油桶", new[] { (MaterialItemRegistry.IronIngotTypeId, 3L) }, "金属油桶"),
        new(FormulaMetalBucket, "金属桶", new[] { (MaterialItemRegistry.IronIngotTypeId, 3L) }, "金属桶"),
        new(FormulaPropane, "丙烷", new[] { (MaterialItemRegistry.IronIngotTypeId, 2L) }, "丙烷"),
        new(FormulaGolfClub, "高尔夫球棒", new[] { (MaterialItemRegistry.IronIngotTypeId, 2L) }, "高尔夫球棒"),
        new(FormulaPot, "锅", new[] { (MaterialItemRegistry.IronIngotTypeId, 2L) }, "锅"),
        new(FormulaSledgeHammer, "大锤子", new[] { (MaterialItemRegistry.IronIngotTypeId, 2L) }, "大锤子"),
        new(FormulaCrowbar, "撬棍", new[] { (MaterialItemRegistry.IronIngotTypeId, 1L) }, "撬棍"),
        new(FormulaShovel, "铲子", new[] { (MaterialItemRegistry.IronIngotTypeId, 1L) }, "铲子"),
        new(FormulaTrap, "捕兽陷阱", new[] { (MaterialItemRegistry.IronIngotTypeId, 1L) }, "捕兽陷阱"),
        new(FormulaWrench, "扳手", new[] { (MaterialItemRegistry.IronIngotTypeId, 1L) }, "扳手"),
        new(FormulaHammer, "锤子", new[] { (MaterialItemRegistry.IronIngotTypeId, 1L) }, "锤子"),
        new(FormulaFlatScrewdriver, "平头螺丝刀", new[] { (MaterialItemRegistry.IronNuggetTypeId, 7L) }, "平头螺丝刀"),
        new(FormulaAdvancedWeaponParts, "高级武器零件", new[] { (MaterialItemRegistry.IronNuggetTypeId, 7L) }, "高级武器零件"),
        new(FormulaMediumWeaponParts, "中级武器零件", new[] { (MaterialItemRegistry.IronNuggetTypeId, 7L) }, "中级武器零件"),
        new(FormulaWeaponParts, "武器零件", new[] { (MaterialItemRegistry.IronNuggetTypeId, 7L) }, "武器零件"),
        new(FormulaNut, "螺母", new[] { (MaterialItemRegistry.IronNuggetTypeId, 1L) }, "螺母"),
        new(FormulaNail, "钉子", new[] { (MaterialItemRegistry.IronNuggetTypeId, 1L) }, "钉子"),
        new(FormulaBolt, "螺栓", new[] { (MaterialItemRegistry.IronNuggetTypeId, 1L) }, "螺栓"),
        new(FormulaKeroseneLamp, "煤油灯", new[] { (MaterialItemRegistry.GlassTypeId, 3L), (MaterialItemRegistry.IronIngotTypeId, 1L) }, "煤油灯")
    };

    private static readonly string[] ManagedFormulaIds = BuildManagedFormulaIds();

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

        foreach (var recipe in ExternalRecipes)
        {
            var resultTypeId = ResolveTypeId(recipe.Label, recipe.Candidates);
            if (resultTypeId < 0)
            {
                continue;
            }

            craftOnlyCategoryIds.Add(resultTypeId);

            if (recipe.Costs.Count == 1)
            {
                var reverseCost = recipe.Costs[0];
                if (string.Equals(recipe.FormulaId, FormulaNut, StringComparison.Ordinal)
                    || string.Equals(recipe.FormulaId, FormulaNail, StringComparison.Ordinal)
                    || string.Equals(recipe.FormulaId, FormulaBolt, StringComparison.Ordinal))
                {
                    reverseCost = (reverseCost.itemTypeId, 3L);
                }

                yield return BuildFormula(GetReverseFormulaId(recipe.FormulaId), reverseCost.itemTypeId, (int)reverseCost.amount, resultTypeId, 1, compatibleTags);
            }
        }

        var blackWhiteDisplayId = ResolveTypeId("黑白显示器", "黑白显示器");
        if (blackWhiteDisplayId > 0)
        {
            craftOnlyCategoryIds.Add(blackWhiteDisplayId);
            yield return BuildFormula(FormulaBlackWhiteDisplayReverse, MaterialItemRegistry.GlassTypeId, 6, blackWhiteDisplayId, 1, compatibleTags);
        }

        var keroseneLampId = ResolveTypeId("煤油灯", "煤油灯");
        if (keroseneLampId > 0)
        {
            craftOnlyCategoryIds.Add(keroseneLampId);
            yield return BuildFormula(FormulaKeroseneLampReverse, MaterialItemRegistry.GlassTypeId, 3, keroseneLampId, 1, compatibleTags);
        }

        if (craftOnlyCategoryIds.Count > 0)
        {
            MCCategoryService.EnsureCraftOnlyCategoryTagged(craftOnlyCategoryIds);
        }
    }

    private static CraftingFormula BuildFormula(string formulaId, int resultId, int resultAmount, int ingredientId, long ingredientAmount, string[] compatibleTags)
    {
        return BuildFormula(formulaId, resultId, resultAmount, new[] { (ingredientId, ingredientAmount) }, compatibleTags);
    }

    private static CraftingFormula BuildFormula(string formulaId, int resultId, int resultAmount, IReadOnlyList<(int itemTypeId, long amount)> costs, string[] compatibleTags)
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
            cost = new Cost(costs.Select(entry => (entry.itemTypeId, entry.amount)).ToArray()),
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

    private static string[] BuildManagedFormulaIds()
    {
        var formulaIds = new List<string>
        {
            FormulaGoldNuggetToIngot,
            FormulaGoldIngotToBlock,
            FormulaIronNuggetToIngot,
            FormulaIronIngotToBlock,
            FormulaGoldIngotToNugget,
            FormulaGoldBlockToIngot,
            FormulaIronIngotToNugget,
            FormulaIronBlockToIngot
        };

        foreach (var recipe in ExternalRecipes)
        {
            formulaIds.Add(recipe.FormulaId);
            if (recipe.Costs.Count == 1)
            {
                formulaIds.Add(GetReverseFormulaId(recipe.FormulaId));
            }
        }

        formulaIds.Add(FormulaBlackWhiteDisplayReverse);
        formulaIds.Add(FormulaKeroseneLampReverse);

        return formulaIds.ToArray();
    }

    private static string GetReverseFormulaId(string formulaId)
    {
        return formulaId + "_Reverse";
    }

    private readonly struct ExternalRecipeDefinition
    {
        public ExternalRecipeDefinition(string formulaId, string label, IReadOnlyList<(int itemTypeId, long amount)> costs, params string[] candidates)
        {
            FormulaId = formulaId;
            Label = label;
            Costs = costs;
            Candidates = candidates;
        }

        public string FormulaId { get; }
        public string Label { get; }
        public IReadOnlyList<(int itemTypeId, long amount)> Costs { get; }
        public string[] Candidates { get; }
    }
}