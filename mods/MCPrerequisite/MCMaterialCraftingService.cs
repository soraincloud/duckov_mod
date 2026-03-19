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

    private static readonly string[] ManagedFormulaIds =
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
        formulaList.AddRange(BuildFormulas(compatibleTags));

        foreach (var formulaId in ManagedFormulaIds)
        {
            EnsureFormulaUnlocked(formulaId);
        }
    }

    private static IEnumerable<CraftingFormula> BuildFormulas(string[] compatibleTags)
    {
        yield return BuildFormula(FormulaGoldNuggetToIngot, MaterialItemRegistry.GoldIngotTypeId, 1, MaterialItemRegistry.GoldNuggetTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaGoldIngotToBlock, MaterialItemRegistry.GoldBlockTypeId, 1, MaterialItemRegistry.GoldIngotTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaIronNuggetToIngot, MaterialItemRegistry.IronIngotTypeId, 1, MaterialItemRegistry.IronNuggetTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaIronIngotToBlock, MaterialItemRegistry.IronBlockTypeId, 1, MaterialItemRegistry.IronIngotTypeId, 9, compatibleTags);
        yield return BuildFormula(FormulaGoldIngotToNugget, MaterialItemRegistry.GoldNuggetTypeId, 9, MaterialItemRegistry.GoldIngotTypeId, 1, compatibleTags);
        yield return BuildFormula(FormulaGoldBlockToIngot, MaterialItemRegistry.GoldIngotTypeId, 9, MaterialItemRegistry.GoldBlockTypeId, 1, compatibleTags);
        yield return BuildFormula(FormulaIronIngotToNugget, MaterialItemRegistry.IronNuggetTypeId, 9, MaterialItemRegistry.IronIngotTypeId, 1, compatibleTags);
        yield return BuildFormula(FormulaIronBlockToIngot, MaterialItemRegistry.IronIngotTypeId, 9, MaterialItemRegistry.IronBlockTypeId, 1, compatibleTags);
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
}