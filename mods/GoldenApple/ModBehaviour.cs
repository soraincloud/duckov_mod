using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duckov.Economy;
using Duckov.Modding;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoldenApple;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int GoldenAppleTypeId = 900002;

    private const string DisplayNameKey = "Item_GoldenApple";
    private const string SharedCategoryTagName = "ModWorkbench_Mystic";
    private const string TargetMerchantId = "Merchant_Equipment";
    private const int MerchantPrice = 6666;
    private const int MerchantStock = 99;
    private const float GoldenAppleWeightKg = 0.35f;
    private const BindingFlags AllBindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static bool _initialized;
    private static Item? _prefab;

    protected override void OnAfterSetup()
    {
        if (_initialized)
        {
            Debug.Log("[GoldenApple] Already initialized.");
            return;
        }

        _initialized = true;

        ModLog.Initialize(info.path);
        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefab(info.path);
        EnsureSharedCategoryDependsOnPrerequisite();
        AddToMerchantProfile();
        PatchExistingStockShops();

        SceneManager.sceneLoaded += OnSceneLoaded;

        ModLog.Info("[GoldenApple] Loaded.");
    }

    protected override void OnBeforeDeactivate()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveFromMerchantProfile();
        UnpatchExistingStockShops();
        GoldenAppleBuffRegistry.Deinitialize();
        GoldenAppleEnchantedIcon.Shutdown();

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

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            EnsureSharedCategoryDependsOnPrerequisite();
            AddToMerchantProfile();
            PatchExistingStockShops();
        }
        catch (Exception e)
        {
            ModLog.Exception(e);
        }
    }

    private static void ApplyLocalizationOverrides()
    {
        LocalizationManager.SetOverrideText(DisplayNameKey, "附魔金苹果");
        LocalizationManager.SetOverrideText(
            DisplayNameKey + "_Desc",
            "食用后获得三段增益：\n- 生命上限 +20，持续 2 分钟\n- 每秒回复 5 点生命，持续 30 秒\n- 头甲 +1.5、身甲 +1.5，持续 5 分钟\n\n左下角状态栏会显示对应增益与剩余时间。额外生命上限会直接提高血量上限，但当前版本不会单独显示为黄色血条。兼容 MC 前置分类。"
        );

        LocalizationManager.SetOverrideText("Buff_GoldenApple_MaxHealth", "附魔金苹果：生命上限");
        LocalizationManager.SetOverrideText("Buff_GoldenApple_MaxHealth_Desc", "最大生命值 +20。剩余时间结束后自动移除。\n持续时间：120 秒");
        LocalizationManager.SetOverrideText("Buff_GoldenApple_Regeneration", "附魔金苹果：生命恢复");
        LocalizationManager.SetOverrideText("Buff_GoldenApple_Regeneration_Desc", "每秒恢复 5 点生命值。\n持续时间：30 秒");
        LocalizationManager.SetOverrideText("Buff_GoldenApple_Armor", "附魔金苹果：护甲强化");
        LocalizationManager.SetOverrideText("Buff_GoldenApple_Armor_Desc", "头甲 +1.5，身甲 +1.5。\n持续时间：300 秒");
    }

    private static void CreateAndRegisterItemPrefab(string? modPath)
    {
        var go = new GameObject("GoldenApple_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();
        ReflectionUtil.SetPrivateField(item, "typeID", GoldenAppleTypeId);

        item.DisplayNameRaw = DisplayNameKey;
        var baseIcon = ModAssets.TryLoadIconSprite(modPath) ?? RuntimeIcon.CreateGoldenAppleIcon();
        item.Icon = GoldenAppleEnchantedIcon.Create(baseIcon);
        item.MaxStackCount = 8;
        item.Value = 1;
        item.Quality = 3;
        item.SetBool("IsSkill", false);
        ReflectionUtil.SetPrivateField(item, "weight", GoldenAppleWeightKg);

        ConfigureUsage(item);
        GoldenAppleBuffRegistry.Initialize(item.Icon);

        go.SetActive(true);
        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;

        ModLog.Info($"[GoldenApple] Registered dynamic item. TypeID={GoldenAppleTypeId}");
    }

    private static void ConfigureUsage(Item item)
    {
        var usageUtilities = item.gameObject.AddComponent<UsageUtilities>();
        usageUtilities.hasSound = false;
        usageUtilities.useDurability = false;
        usageUtilities.durabilityUsage = 1;
        ReflectionUtil.SetPrivateField(usageUtilities, "useTime", 1.15f);

        var usage = item.gameObject.AddComponent<GoldenAppleUsage>();
        usageUtilities.behaviors.Add(usage);

        ReflectionUtil.SetPrivateField(item, "usageUtilities", usageUtilities);
    }

    private static void AddToMerchantProfile()
    {
        var database = StockShopDatabase.Instance;
        if (database == null)
        {
            return;
        }

        var profile = database.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            return;
        }

        var existing = profile.entries.Find(entry => entry != null && entry.typeID == GoldenAppleTypeId);
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
    }

    private static void RemoveFromMerchantProfile()
    {
        var profile = StockShopDatabase.Instance?.GetMerchantProfile(TargetMerchantId);
        profile?.entries.RemoveAll(entry => entry != null && entry.typeID == GoldenAppleTypeId);
    }

    private static StockShopDatabase.ItemEntry CreateMerchantItemEntry()
    {
        return new StockShopDatabase.ItemEntry
        {
            typeID = GoldenAppleTypeId,
            maxStock = MerchantStock,
            forceUnlock = true,
            priceFactor = MerchantPrice,
            possibility = 1f,
            lockInDemo = false
        };
    }

    private static void PatchExistingStockShops()
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

            var existing = shop.entries.Find(entry => entry != null && entry.ItemTypeID == GoldenAppleTypeId);
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

            var entry = new StockShop.Entry(CreateMerchantItemEntry())
            {
                Show = true,
                CurrentStock = MerchantStock
            };

            shop.entries.Add(entry);
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

            shop.entries.RemoveAll(entry => entry != null && entry.ItemTypeID == GoldenAppleTypeId);

            var instances = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (instances != null && instances.TryGetValue(GoldenAppleTypeId, out var cachedItem))
            {
                instances.Remove(GoldenAppleTypeId);
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
            var instances = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (instances == null)
            {
                return;
            }

            if (instances.ContainsKey(GoldenAppleTypeId) && instances[GoldenAppleTypeId] != null)
            {
                return;
            }

            var item = ItemAssetsCollection.InstantiateSync(GoldenAppleTypeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);
            instances[GoldenAppleTypeId] = item;
        }
        catch (Exception e)
        {
            ModLog.Exception(e);
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

        var changed = false;
        if (_prefab != null)
        {
            changed |= TryDetachSharedCategory(_prefab, sharedTag);
        }

        var liveItems = Resources.FindObjectsOfTypeAll<Item>();
        foreach (var item in liveItems)
        {
            if (item == null || item.TypeID != GoldenAppleTypeId)
            {
                continue;
            }

            changed |= TryDetachSharedCategory(item, sharedTag);
        }

        if (changed)
        {
            RefreshDynamicMetaData();
        }
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

    private static void RefreshDynamicMetaData()
    {
        var dynamicEntriesField = typeof(ItemAssetsCollection).GetField("dynamicDic", AllBindings);
        if (dynamicEntriesField?.GetValue(null) is not IDictionary dynamicEntries)
        {
            return;
        }

        if (!dynamicEntries.Contains(GoldenAppleTypeId))
        {
            return;
        }

        var entry = dynamicEntries[GoldenAppleTypeId];
        if (entry == null || _prefab == null)
        {
            return;
        }

        var entryType = entry.GetType();
        var metaDataField = entryType.GetField("_metaData", AllBindings);
        metaDataField?.SetValue(entry, new ItemMetaData(_prefab));
    }
}