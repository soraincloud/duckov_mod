using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Economy;
using Duckov.Modding;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SplashHealingPotion;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int EnderPearlTypeId = 900012;
    private const string SharedCraftCategoryTagName = "ModWorkbench_Mystic";
    private const string SharedCraftCategoryDisplayNameKey = "CraftFilter_ModMystic";
    private const string PrimaryFormulaId = "SplashHealingPotion_Workbench";
    private const string TargetMerchantId = "Merchant_Equipment";
    private const int MerchantPrice = 2000;
    private const int MerchantStock = 99;

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
    private static Tag? _sharedCraftCategoryTag;

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

        Debug.Log("[SplashHealingPotion] Loaded.");

        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefab(info.path);
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
        LocalizationManager.SetOverrideText("Item_SplashHealingPotion_Desc", "手持后：按住显示投掷线，松手投掷。\n落地时在范围内为角色恢复 50% 最大生命值，并爆开粉色粒子。");
    }

    private static void CreateAndRegisterItemPrefab(string? modPath)
    {
        ModAssets.SetModPath(modPath);

        var go = new GameObject("SplashHealingPotion_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();

        // TypeID 的 setter 是 internal，这里用反射直接写入私有字段
        ReflectionUtil.SetPrivateField(item, "typeID", EnderPearlTypeId);

        item.DisplayNameRaw = "Item_SplashHealingPotion";
        item.Icon = ModAssets.TryLoadIconSprite(modPath) ?? RuntimeIcon.CreatePearlIcon();
        item.MaxStackCount = 8;
        item.Value = 1;
        item.Quality = 0;

        item.SetBool("IsSkill", true);

        var skill = go.AddComponent<Skill_EnderPearlThrow>();
        var skillSetting = go.AddComponent<ItemSetting_Skill>();
        skillSetting.Skill = skill;
        skillSetting.onRelease = ItemSetting_Skill.OnReleaseAction.none;

        var visualHook = go.AddComponent<EnderPearlVisualHook>();
        visualHook.SetModPath(modPath);

        go.SetActive(true);
        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;

        Debug.Log($"[SplashHealingPotion] Registered dynamic item. TypeID={EnderPearlTypeId}");
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

        var existing = profile.entries.Find(e => e != null && e.typeID == EnderPearlTypeId);
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
        profile?.entries.RemoveAll(entry => entry != null && entry.typeID == EnderPearlTypeId);
    }

    private static void RegisterOrUpdateCraftingFormulas()
    {
        var formulas = CraftingFormulaCollection.Instance;
        if (formulas == null)
        {
            ModLog.Warn("[EnderPearl] CraftingFormulaCollection.Instance is null. Will retry on scene load.");
            return;
        }

        if (!TryBuildCraftingFormulas(formulas, out var builtFormulas))
        {
            return;
        }

        var formulaList = ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list");
        if (formulaList == null)
        {
            ModLog.Warn("[EnderPearl] Failed to access crafting formula list.");
            return;
        }

        formulaList.RemoveAll(existing =>
            string.Equals(existing.id, PrimaryFormulaId, StringComparison.Ordinal));
        formulaList.AddRange(builtFormulas);

        foreach (var formula in builtFormulas)
        {
            EnsureFormulaUnlocked(formula.id);
            ModLog.Info($"[EnderPearl] Registered crafting formula '{formula.id}' with tags: {string.Join(", ", formula.tags ?? Array.Empty<string>())}");
        }
    }

    private static bool TryBuildCraftingFormulas(CraftingFormulaCollection formulas, out List<CraftingFormula> builtFormulas)
    {
        builtFormulas = new List<CraftingFormula>();

        var bandageId = ResolveIngredientTypeId("止血绷带", "止血绷带", "Bandage", "Hemostatic Bandage", "HemostaticBandage");
        var firstAidKitId = ResolveIngredientTypeId("小急救箱", "小急救箱", "Small First Aid Kit", "First Aid Kit", "SmallFirstAidKit", "Small Medkit", "SmallMedkit");
        var recoveryShotId = ResolveIngredientTypeId("恢复针", "恢复针", "Recovery Shot", "Recovery Syringe", "Recovery Injection", "RecoveryShot", "RecoverySyringe", "RecoveryInjection");

        if (bandageId < 0 || firstAidKitId < 0 || recoveryShotId < 0)
        {
            return false;
        }

        var compatibleTags = BuildCompatibleFormulaTags(formulas);

        builtFormulas.Add(new CraftingFormula
        {
            id = PrimaryFormulaId,
            result = new CraftingFormula.ItemEntry
            {
                id = EnderPearlTypeId,
                amount = 1
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

        return true;
    }

    private static string[] BuildCompatibleFormulaTags(CraftingFormulaCollection formulas)
    {
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
            ModLog.Warn($"[EnderPearl] ItemAssetsCollection not ready while resolving ingredient '{label}'.");
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
                ModLog.Info($"[EnderPearl] Ingredient '{label}' matched fuzzily to '{entry.metaData.DisplayName}' ({entry.typeID}).");
                return entry.typeID;
            }
        }

        ModLog.Warn($"[EnderPearl] Failed to resolve ingredient '{label}'. Candidates: {string.Join(", ", candidates)}");
        return -1;
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
            ModLog.Warn("[EnderPearl] Failed to access unlocked formula list.");
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
            string.Equals(existing.id, PrimaryFormulaId, StringComparison.Ordinal));

        if (CraftingManager.Instance != null)
        {
            var unlockedFormulaIds = ReflectionUtil.GetPrivateField<List<string>>(CraftingManager.Instance, "unlockedFormulaIDs");
            unlockedFormulaIds?.RemoveAll(existing =>
                string.Equals(existing, PrimaryFormulaId, StringComparison.Ordinal));
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

    private static void OnPlayerStorageLoaded()
    {
        try
        {
            RegisterStorageCategoryFilter();
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

            // 仅注入到“橘子”（装备商人）对应的 merchantID
            if (!string.Equals(shop.MerchantID, TargetMerchantId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var existing = shop.entries.Find(e => e != null && e.ItemTypeID == EnderPearlTypeId);
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

            shop.entries.RemoveAll(entry => entry != null && entry.ItemTypeID == EnderPearlTypeId);

            var dict = ReflectionUtil.GetPrivateField<System.Collections.Generic.Dictionary<int, Item>>(shop, "itemInstances");
            if (dict != null && dict.TryGetValue(EnderPearlTypeId, out var cachedItem))
            {
                dict.Remove(EnderPearlTypeId);
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

            if (dict.ContainsKey(EnderPearlTypeId) && dict[EnderPearlTypeId] != null)
            {
                return;
            }

            var item = ItemAssetsCollection.InstantiateSync(EnderPearlTypeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);

            dict[EnderPearlTypeId] = item;
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
            typeID = EnderPearlTypeId,
            maxStock = MerchantStock,
            forceUnlock = true,
            priceFactor = MerchantPrice,
            possibility = 1f,
            lockInDemo = false
        };
    }

    private static void AddTagIfExists(Item item, Tag? tag)
    {
        if (item == null || tag == null)
        {
            return;
        }

        if (!item.Tags.Contains(tag))
        {
            item.Tags.Add(tag);
        }
    }

    private static void EnsureRuntimeTag(Item item, string tagName)
    {
        if (item == null || string.IsNullOrWhiteSpace(tagName) || item.Tags.Contains(tagName))
        {
            return;
        }

        item.Tags.Add(GetOrCreateSharedCraftCategoryTag(tagName));
    }

    private static void RegisterCraftCategoryFilter()
    {
        try
        {
            var filterTag = GetOrCreateSharedCraftCategoryTag(SharedCraftCategoryTagName);
            var filterIcon = ModAssets.TryLoadCraftCategoryIconSprite(ModAssets.CurrentModPath);
            var craftViews = Resources.FindObjectsOfTypeAll<CraftView>();
            if (craftViews == null || craftViews.Length == 0)
            {
                return;
            }

            foreach (var craftView in craftViews)
            {
                if (craftView == null)
                {
                    continue;
                }

                EnsureCraftViewHasSharedCategoryFilter(craftView, filterTag, filterIcon);
            }
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Failed to register craft category filter: {e.Message}");
        }
    }

    private static void RegisterStorageCategoryFilter()
    {
        try
        {
            var inventory = PlayerStorage.Inventory;
            if (inventory == null)
            {
                return;
            }

            var provider = inventory.GetComponent<InventoryFilterProvider>();
            if (provider == null)
            {
                return;
            }

            var filterTag = GetOrCreateSharedCraftCategoryTag(SharedCraftCategoryTagName);
            var filterIcon = ModAssets.TryLoadCraftCategoryIconSprite(ModAssets.CurrentModPath);
            EnsureInventoryHasSharedCategoryFilter(provider, filterTag, filterIcon);
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Failed to register storage category filter: {e.Message}");
        }
    }

    private static void EnsureCraftViewHasSharedCategoryFilter(CraftView craftView, Tag filterTag, Sprite? filterIcon)
    {
        var filters = ReflectionUtil.GetPrivateField<CraftView.FilterInfo[]>(craftView, "filters") ?? Array.Empty<CraftView.FilterInfo>();
        var updatedFilters = filters.ToList();
        var index = updatedFilters.FindIndex(HasSharedCategoryFilter);
        var filterInfo = new CraftView.FilterInfo
        {
            displayNameKey = SharedCraftCategoryDisplayNameKey,
            icon = filterIcon,
            requireTags = new[] { filterTag }
        };

        if (index >= 0)
        {
            var existing = updatedFilters[index];
            if (string.IsNullOrWhiteSpace(existing.displayNameKey))
            {
                existing.displayNameKey = filterInfo.displayNameKey;
            }

            if (existing.icon == null && filterInfo.icon != null)
            {
                existing.icon = filterInfo.icon;
            }

            existing.requireTags = MergeFilterTags(existing.requireTags, filterTag);

            updatedFilters[index] = existing;
        }
        else
        {
            updatedFilters.Add(filterInfo);
        }

        ReflectionUtil.SetPrivateField(craftView, "filters", updatedFilters.ToArray());
    }

    private static bool HasSharedCategoryFilter(CraftView.FilterInfo filter)
    {
        if (filter.requireTags == null)
        {
            return false;
        }

        return filter.requireTags.Any(tag => tag != null && Tag.Match(tag, SharedCraftCategoryTagName));
    }

    private static void EnsureInventoryHasSharedCategoryFilter(InventoryFilterProvider provider, Tag filterTag, Sprite? filterIcon)
    {
        var filters = provider.entries ?? Array.Empty<InventoryFilterProvider.FilterEntry>();
        var updatedFilters = filters.ToList();
        var index = updatedFilters.FindIndex(HasSharedStorageCategoryFilter);
        var filterEntry = new InventoryFilterProvider.FilterEntry
        {
            name = SharedCraftCategoryDisplayNameKey,
            icon = filterIcon,
            requireTags = new[] { filterTag }
        };

        if (index >= 0)
        {
            var existing = updatedFilters[index];
            if (string.IsNullOrWhiteSpace(existing.name))
            {
                existing.name = filterEntry.name;
            }

            if (existing.icon == null && filterEntry.icon != null)
            {
                existing.icon = filterEntry.icon;
            }

            existing.requireTags = MergeFilterTags(existing.requireTags, filterTag);
            updatedFilters[index] = existing;
        }
        else
        {
            updatedFilters.Add(filterEntry);
        }

        provider.entries = updatedFilters.ToArray();
    }

    private static bool HasSharedStorageCategoryFilter(InventoryFilterProvider.FilterEntry filter)
    {
        if (filter.requireTags == null)
        {
            return false;
        }

        return filter.requireTags.Any(tag => tag != null && Tag.Match(tag, SharedCraftCategoryTagName));
    }

    private static Tag[] MergeFilterTags(Tag[]? existingTags, Tag filterTag)
    {
        if (filterTag == null)
        {
            return existingTags ?? Array.Empty<Tag>();
        }

        var merged = new List<Tag>();
        if (existingTags != null)
        {
            foreach (var tag in existingTags)
            {
                if (tag != null && !merged.Any(existing => ReferenceEquals(existing, tag)))
                {
                    merged.Add(tag);
                }
            }
        }

        if (!merged.Any(existing => ReferenceEquals(existing, filterTag)))
        {
            merged.Add(filterTag);
        }

        return merged.ToArray();
    }

    private static Tag GetOrCreateSharedCraftCategoryTag(string tagName)
    {
        if (_sharedCraftCategoryTag != null)
        {
            return _sharedCraftCategoryTag;
        }

        _sharedCraftCategoryTag = GameplayDataSettings.Tags?.AllTags?.FirstOrDefault(tag => tag != null && Tag.Match(tag, tagName));
        if (_sharedCraftCategoryTag != null)
        {
            return _sharedCraftCategoryTag;
        }

        _sharedCraftCategoryTag = ScriptableObject.CreateInstance<Tag>();
        _sharedCraftCategoryTag.name = tagName;
        _sharedCraftCategoryTag.hideFlags = HideFlags.HideAndDontSave;
        return _sharedCraftCategoryTag;
    }
}
