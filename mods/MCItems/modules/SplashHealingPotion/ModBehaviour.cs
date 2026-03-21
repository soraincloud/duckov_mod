using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.Economy;
using Duckov.Modding;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SplashHealingPotion;

/// <summary>
/// 喷溅治疗药水模块入口，负责注册投掷道具、商店条目和兼容 MC 玻璃的配方。
/// </summary>
public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int SplashHealingPotionTypeId = 900012;
    private const int McGlassTypeId = 800001;
    private const string PrimaryFormulaId = "SplashHealingPotion_Workbench";
    private const string SecondaryFormulaId = "SplashHealingPotion_Workbench_MCGlass";
    private const string TargetMerchantId = "Merchant_Equipment";
    private const int MerchantPrice = 1000;
    private const int MerchantStock = 99;
    private const string SharedCategoryTagName = "ModWorkbench_Mystic";
    private const BindingFlags AllBindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly string[] WorkbenchFormulaTags =
    {
        "Workbench",
        "workbench",
        "Craft",
        "craft",
        "Crafter",
        "crafter",
        "Crafting",
        "crafting"
    };

    private static bool _initialized;
    private static Item? _prefab;

    protected override void OnAfterSetup()
    {
        if (_initialized)
        {
            Debug.Log("[SplashHealingPotion] Already initialized.");
            return;
        }

        _initialized = true;

        ModLog.Initialize(info.path);
        ModAssets.SetModPath(info.path);
        ModSfx.Initialize(info.path);
        HealFlashFeedback.EnsureExists();

        Debug.Log("[SplashHealingPotion] Loaded.");

        ApplyLocalizationOverrides();
        // 先把动态物品接入 ItemAssetsCollection，再去补商店和配方引用。
        CreateAndRegisterItemPrefab(info.path);
        EnsureSharedCategoryDependsOnPrerequisite();
        AddToMerchantProfile();
        RegisterOrUpdateCraftingFormulas();
        PatchExistingStockShops();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnBeforeDeactivate()
    {
        ModSfx.Deinitialize();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveFromMerchantProfile();
        UnpatchExistingStockShops();
        UnregisterCraftingFormulas();

        if (_prefab != null)
        {
            ItemAssetsCollection.RemoveDynamicEntry(_prefab);
            try
            {
                UnityEngine.Object.Destroy(_prefab.gameObject);
            }
            catch
            {
                // ignore
            }
            _prefab = null;
        }

        _initialized = false;
    }

    private static void ApplyLocalizationOverrides()
    {
        LocalizationManager.SetOverrideText("Item_SplashHealingPotion", "喷溅治疗药水");
        LocalizationManager.SetOverrideText("Item_SplashHealingPotion_Desc", "把恢复针内的药液用来自异界的玻璃装满后居然可以更便捷的使用，虽然总治疗量有所降低，但是在应急方面比恢复针的效果好多了，使用后可以把药效扩散到一定范围内，让范围内的所有目标恢复50%最大生命值的血量。");
    }

    private static void CreateAndRegisterItemPrefab(string? modPath)
    {
        ModAssets.SetModPath(modPath);

        var go = new GameObject("SplashHealingPotion_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();

        // TypeID 的 setter 是 internal，这里用反射直接写入私有字段
        ReflectionUtil.SetPrivateField(item, "typeID", SplashHealingPotionTypeId);

        item.DisplayNameRaw = "Item_SplashHealingPotion";
        item.Icon = ModAssets.TryLoadIconSprite(modPath) ?? RuntimeIcon.CreateSplashHealingPotionIcon();
        item.MaxStackCount = 8;
        item.Value = 1;
        item.Quality = 2;

        item.SetBool("IsSkill", true);

        var skill = go.AddComponent<Skill_SplashHealingPotionThrow>();
        var skillSetting = go.AddComponent<ItemSetting_Skill>();
        skillSetting.Skill = skill;
        skillSetting.onRelease = ItemSetting_Skill.OnReleaseAction.none;

        var visualHook = go.AddComponent<SplashHealingPotionVisualHook>();
        visualHook.SetModPath(modPath);

        go.SetActive(true);
        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;

        Debug.Log($"[SplashHealingPotion] Registered dynamic item. TypeID={SplashHealingPotionTypeId}");
    }

    private static void AddToMerchantProfile()
    {
        var db = StockShopDatabase.Instance;
        if (db == null)
        {
            Debug.LogWarning("[SplashHealingPotion] StockShopDatabase.Instance is null (too early?). Will retry on scene load.");
            return;
        }

        var profile = db.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            Debug.LogWarning($"[SplashHealingPotion] Merchant profile '{TargetMerchantId}' not found.");
            return;
        }

        var existing = profile.entries.Find(e => e != null && e.typeID == SplashHealingPotionTypeId);
        if (existing != null)
        {
            existing.maxStock = MerchantStock;
            existing.forceUnlock = true;
            existing.priceFactor = MerchantPrice;
            existing.possibility = 1f;
            existing.lockInDemo = false;
            return;
        }

        profile.entries.Add(CreateMerchantItemEntry());

        Debug.Log($"[SplashHealingPotion] Added to merchant profile {TargetMerchantId}.");
    }

    private static void RemoveFromMerchantProfile()
    {
        var profile = StockShopDatabase.Instance?.GetMerchantProfile(TargetMerchantId);
        profile?.entries.RemoveAll(entry => entry != null && entry.typeID == SplashHealingPotionTypeId);
    }

    private static void RegisterOrUpdateCraftingFormulas()
    {
        var formulas = CraftingFormulaCollection.Instance;
        if (formulas == null)
        {
            ModLog.Warn("[SplashHealingPotion] CraftingFormulaCollection.Instance is null. Will retry on scene load.");
            return;
        }

        if (!TryBuildCraftingFormulas(formulas, out var builtFormulas))
        {
            return;
        }

        var formulaList = ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list");
        if (formulaList == null)
        {
            ModLog.Warn("[SplashHealingPotion] Failed to access crafting formula list.");
            return;
        }

        // 场景切换时会反复走这里，所以先删除旧公式，保证列表里始终只有最新版本。
        formulaList.RemoveAll(existing =>
            string.Equals(existing.id, PrimaryFormulaId, StringComparison.Ordinal) ||
            string.Equals(existing.id, SecondaryFormulaId, StringComparison.Ordinal));
        formulaList.AddRange(builtFormulas);

        foreach (var formula in builtFormulas)
        {
            EnsureFormulaUnlocked(formula.id);
            ModLog.Info($"[SplashHealingPotion] Registered crafting formula '{formula.id}' with tags: {string.Join(", ", formula.tags ?? Array.Empty<string>())}");
        }
    }

    private static bool TryBuildCraftingFormulas(CraftingFormulaCollection formulas, out List<CraftingFormula> builtFormulas)
    {
        builtFormulas = new List<CraftingFormula>();

        // 第一套走原版医疗物资路线，第二套在 MCPrerequisite 存在时允许用 MC 玻璃替代部分成本。
        var bandageId = ResolveIngredientTypeId("止血绷带", "止血绷带", "Bandage", "Hemostatic Bandage", "HemostaticBandage");
        var firstAidKitId = ResolveIngredientTypeId("小急救箱", "小急救箱", "Small First Aid Kit", "First Aid Kit", "SmallFirstAidKit", "Small Medkit", "SmallMedkit");
        var recoveryShotId = ResolveIngredientTypeId("恢复针", "恢复针", "Recovery Shot", "Recovery Syringe", "Recovery Injection", "RecoveryShot", "RecoverySyringe", "RecoveryInjection");
        var compatibleTags = BuildCompatibleFormulaTags(formulas);

        if (bandageId >= 0 && firstAidKitId >= 0 && recoveryShotId >= 0)
        {
            builtFormulas.Add(new CraftingFormula
            {
                id = PrimaryFormulaId,
                result = new CraftingFormula.ItemEntry
                {
                    id = SplashHealingPotionTypeId,
                    amount = 2
                },
                tags = compatibleTags,
                cost = new Cost(
                    (bandageId, 6L),
                    (firstAidKitId, 2L),
                    (recoveryShotId, 1L)),
                unlockByDefault = true,
                lockInDemo = false,
                requirePerk = string.Empty,
                hideInIndex = false
            });
        }

        if (HasRegisteredItemType(McGlassTypeId) && recoveryShotId >= 0)
        {
            builtFormulas.Add(new CraftingFormula
            {
                id = SecondaryFormulaId,
                result = new CraftingFormula.ItemEntry
                {
                    id = SplashHealingPotionTypeId,
                    amount = 2
                },
                tags = compatibleTags,
                cost = new Cost(
                    (McGlassTypeId, 3L),
                    (recoveryShotId, 1L)),
                unlockByDefault = true,
                lockInDemo = false,
                requirePerk = string.Empty,
                hideInIndex = false
            });
        }

        if (builtFormulas.Count == 0)
        {
            ModLog.Warn("[SplashHealingPotion] Failed to build any crafting formula because required ingredients are unavailable.");
            return false;
        }

        return true;
    }

    private static string[] BuildCompatibleFormulaTags(CraftingFormulaCollection formulas)
    {
        // 直接吸收现有配方里的 tag，兼容不同工作台实现对标签命名的差异。
        var tags = new HashSet<string>(WorkbenchFormulaTags, StringComparer.Ordinal);
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

    private static int ResolveIngredientTypeId(string label, params string[] candidates)
    {
        var collection = ItemAssetsCollection.Instance;
        if (collection?.entries == null)
        {
            ModLog.Warn($"[SplashHealingPotion] ItemAssetsCollection not ready while resolving ingredient '{label}'.");
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

            if (MatchesIngredient(entry.metaData, normalizedCandidates, contains: false))
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

            if (MatchesIngredient(entry.metaData, normalizedCandidates, contains: true))
            {
                ModLog.Info($"[SplashHealingPotion] Ingredient '{label}' matched fuzzily to '{entry.metaData.DisplayName}' ({entry.typeID}).");
                return entry.typeID;
            }
        }

        ModLog.Warn($"[SplashHealingPotion] Failed to resolve ingredient '{label}'. Candidates: {string.Join(", ", candidates)}");
        return -1;
    }

    private static bool HasRegisteredItemType(int typeId)
    {
        var collection = ItemAssetsCollection.Instance;
        if (collection?.entries != null)
        {
            foreach (var entry in collection.entries)
            {
                if (entry != null && entry.typeID == typeId && entry.metaData.id > 0)
                {
                    return true;
                }
            }
        }

        var dynamicEntriesField = typeof(ItemAssetsCollection).GetField("dynamicDic", AllBindings);
        if (dynamicEntriesField?.GetValue(null) is not System.Collections.IDictionary dynamicEntries)
        {
            return false;
        }

        return dynamicEntries.Contains(typeId);
    }

    private static bool MatchesIngredient(ItemMetaData metaData, string[] normalizedCandidates, bool contains)
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

        var chars = value
            .Trim()
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray();

        return new string(chars);
    }

    private static void EnsureFormulaUnlocked(string formulaId)
    {
        if (CraftingManager.Instance == null)
        {
            return;
        }

        var unlockedFormulaIds = ReflectionUtil.GetPrivateField<List<string>>(CraftingManager.Instance, "unlockedFormulaIDs");
        if (unlockedFormulaIds == null)
        {
            ModLog.Warn("[SplashHealingPotion] Failed to access unlocked formula list.");
            return;
        }

        if (unlockedFormulaIds.Contains(formulaId))
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
        formulaList?.RemoveAll(existing =>
            string.Equals(existing.id, PrimaryFormulaId, StringComparison.Ordinal) ||
            string.Equals(existing.id, SecondaryFormulaId, StringComparison.Ordinal));

        if (CraftingManager.Instance != null)
        {
            var unlockedFormulaIds = ReflectionUtil.GetPrivateField<List<string>>(CraftingManager.Instance, "unlockedFormulaIDs");
            unlockedFormulaIds?.RemoveAll(existing =>
                string.Equals(existing, PrimaryFormulaId, StringComparison.Ordinal) ||
                string.Equals(existing, SecondaryFormulaId, StringComparison.Ordinal));
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            AddToMerchantProfile();
            RegisterOrUpdateCraftingFormulas();
            PatchExistingStockShops();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static void PatchExistingStockShops()
    {
        // Unity 版本兼容：FindObjectsOfType 在旧版可用
        var shops = UnityEngine.Object.FindObjectsOfType<StockShop>();
        if (shops == null || shops.Length == 0)
        {
            return;
        }

        foreach (var shop in shops)
        {
            if (shop == null)
            {
                continue;
            }

            // 仅注入到“橘子”（装备商人）对应的 merchantID。
            if (!string.Equals(shop.MerchantID, TargetMerchantId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var existing = shop.entries.Find(e => e != null && e.ItemTypeID == SplashHealingPotionTypeId);
            if (existing != null)
            {
                existing.Show = true;
                if (existing.CurrentStock < 1)
                {
                    existing.CurrentStock = MerchantStock;
                }

                EnsureShopHasCachedItemInstance(shop);
                continue;
            }

            var itemEntry = CreateMerchantItemEntry();

            var entry = new StockShop.Entry(itemEntry)
            {
                Show = true,
                CurrentStock = MerchantStock
            };

            shop.entries.Add(entry);

            // StockShop.BuyTask 依赖 itemInstances 已缓存，否则会直接 return false
            EnsureShopHasCachedItemInstance(shop);
        }
    }

    private static void EnsureSharedCategoryDependsOnPrerequisite()
    {
        if (IsPrerequisiteLoaded())
        {
            return;
        }

        RemoveSharedCategoryFromManagedItems();
    }

    private static bool IsPrerequisiteLoaded()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Any(assembly => string.Equals(assembly.GetName().Name, "MCPrerequisite", StringComparison.Ordinal));
    }

    private static void RemoveSharedCategoryFromManagedItems()
    {
        var sharedTag = GameplayDataSettings.Tags?.AllTags?.FirstOrDefault(tag => tag != null && Tag.Match(tag, SharedCategoryTagName));
        if (sharedTag == null)
        {
            return;
        }

        if (_prefab != null)
        {
            TryDetachSharedCategory(_prefab, sharedTag);
        }

        var liveItems = Resources.FindObjectsOfTypeAll<Item>();
        foreach (var item in liveItems)
        {
            if (item == null || item.TypeID != SplashHealingPotionTypeId)
            {
                continue;
            }

            TryDetachSharedCategory(item, sharedTag);
        }

        RefreshDynamicMetaData(sharedTag);
    }

    private static bool TryDetachSharedCategory(Item item, Tag sharedTag)
    {
        if (item == null || sharedTag == null)
        {
            return false;
        }

        var tags = item.Tags;
        if (tags == null || !tags.Contains(sharedTag))
        {
            return false;
        }

        tags.Remove(sharedTag);
        return true;
    }

    private static void RefreshDynamicMetaData(Tag sharedTag)
    {
        var dynamicEntriesField = typeof(ItemAssetsCollection).GetField("dynamicDic", AllBindings);
        if (dynamicEntriesField?.GetValue(null) is not System.Collections.IDictionary dynamicEntries)
        {
            return;
        }

        if (!dynamicEntries.Contains(SplashHealingPotionTypeId))
        {
            return;
        }

        var entry = dynamicEntries[SplashHealingPotionTypeId];
        if (entry == null)
        {
            return;
        }

        var entryType = entry.GetType();
        var prefabField = entryType.GetField("prefab", AllBindings);
        if (prefabField?.GetValue(entry) is not Item prefab)
        {
            return;
        }

        TryDetachSharedCategory(prefab, sharedTag);

        var metaDataField = entryType.GetField("_metaData", AllBindings);
        metaDataField?.SetValue(entry, new ItemMetaData(prefab));
    }

    private static void UnpatchExistingStockShops()
    {
        var shops = UnityEngine.Object.FindObjectsOfType<StockShop>();
        if (shops == null || shops.Length == 0)
        {
            return;
        }

        foreach (var shop in shops)
        {
            if (shop == null || !string.Equals(shop.MerchantID, TargetMerchantId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            shop.entries.RemoveAll(entry => entry != null && entry.ItemTypeID == SplashHealingPotionTypeId);

            var dict = ReflectionUtil.GetPrivateField<System.Collections.Generic.Dictionary<int, Item>>(shop, "itemInstances");
            if (dict != null && dict.TryGetValue(SplashHealingPotionTypeId, out var cachedItem))
            {
                dict.Remove(SplashHealingPotionTypeId);
                if (cachedItem != null)
                {
                    try
                    {
                        UnityEngine.Object.Destroy(cachedItem.gameObject);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }
    }

    private static void EnsureShopHasCachedItemInstance(StockShop shop)
    {
        try
        {
            var dict = ReflectionUtil.GetPrivateField<System.Collections.Generic.Dictionary<int, Item>>(shop, "itemInstances");
            if (dict == null)
            {
                return;
            }

            if (dict.ContainsKey(SplashHealingPotionTypeId) && dict[SplashHealingPotionTypeId] != null)
            {
                return;
            }

            var item = ItemAssetsCollection.InstantiateSync(SplashHealingPotionTypeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);

            dict[SplashHealingPotionTypeId] = item;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static StockShopDatabase.ItemEntry CreateMerchantItemEntry()
    {
        return new StockShopDatabase.ItemEntry
        {
            typeID = SplashHealingPotionTypeId,
            maxStock = MerchantStock,
            forceUnlock = true,
            priceFactor = MerchantPrice,
            possibility = 1f,
            lockInDemo = false
        };
    }

}
