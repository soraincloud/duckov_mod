using System;
using System.Collections.Generic;
using Duckov.Economy;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SplashSwiftnessPotion;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int SplashSwiftnessPotionTypeId = 900013;

    private const string DisplayNameKey = "Item_SplashSwiftnessPotion";
    private const string TargetMerchantId = "Merchant_Equipment";
    private const int MerchantPrice = 1000;
    private const int MerchantStock = 99;

    private static bool _initialized;
    private static Item? _prefab;

    protected override void OnAfterSetup()
    {
        if (_initialized)
        {
            Debug.Log("[SplashSwiftnessPotion] Already initialized.");
            return;
        }

        _initialized = true;

        ModLog.Initialize(info.path);
        ModAssets.SetModPath(info.path);
        ModSfx.Initialize(info.path);

        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefab(info.path);
        AddToMerchantProfile();
        PatchExistingStockShops();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ModLog.Info("[SplashSwiftnessPotion] Loaded.");
    }

    protected override void OnBeforeDeactivate()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ModSfx.Deinitialize();
        SplashSwiftnessPotionBuffRegistry.Deinitialize();
        RemoveFromMerchantProfile();
        UnpatchExistingStockShops();

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
            AddToMerchantProfile();
            PatchExistingStockShops();
        }
        catch (Exception exception)
        {
            ModLog.Exception(exception);
        }
    }

    private static void ApplyLocalizationOverrides()
    {
        LocalizationManager.SetOverrideText(DisplayNameKey, "喷溅迅捷药水");
        LocalizationManager.SetOverrideText(
            DisplayNameKey + "_Desc",
            "将药液装入来自异界的玻璃瓶后，这瓶药水会在落地时迅速扩散，让范围内的目标获得短时间的迅捷增益。命中范围内的角色会同时获得 30% 行走速度与 30% 奔跑速度提升，持续 180 秒。"
        );
        LocalizationManager.SetOverrideText("Buff_SplashSwiftnessPotion_Speed", "喷溅迅捷药水：迅捷");
        LocalizationManager.SetOverrideText("Buff_SplashSwiftnessPotion_Speed_Desc", "行走速度 +30%，奔跑速度 +30%。\n持续时间：180 秒");
    }

    private static void CreateAndRegisterItemPrefab(string? modPath)
    {
        var go = new GameObject("SplashSwiftnessPotion_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();
        ReflectionUtil.SetPrivateField(item, "typeID", SplashSwiftnessPotionTypeId);

        item.DisplayNameRaw = DisplayNameKey;
        item.Icon = ModAssets.TryLoadIconSprite(modPath) ?? RuntimeIcon.CreateSplashSwiftnessPotionIcon();
        item.MaxStackCount = 8;
        item.Value = 1;
        item.Quality = 2;
        item.SetBool("IsSkill", true);

        var skill = go.AddComponent<Skill_SplashSwiftnessPotionThrow>();
        var skillSetting = go.AddComponent<ItemSetting_Skill>();
        skillSetting.Skill = skill;
        skillSetting.onRelease = ItemSetting_Skill.OnReleaseAction.none;

        var visualHook = go.AddComponent<SplashSwiftnessPotionVisualHook>();
        visualHook.SetModPath(modPath);

        SplashSwiftnessPotionBuffRegistry.Initialize(item.Icon);

        go.SetActive(true);
        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;
        ModLog.Info($"[SplashSwiftnessPotion] Registered dynamic item. TypeID={SplashSwiftnessPotionTypeId}");
    }

    private static void AddToMerchantProfile()
    {
        var db = StockShopDatabase.Instance;
        if (db == null)
        {
            return;
        }

        var profile = db.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            return;
        }

        var existing = profile.entries.Find(entry => entry != null && entry.typeID == SplashSwiftnessPotionTypeId);
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
        profile?.entries.RemoveAll(entry => entry != null && entry.typeID == SplashSwiftnessPotionTypeId);
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

            var existing = shop.entries.Find(entry => entry != null && entry.ItemTypeID == SplashSwiftnessPotionTypeId);
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

            shop.entries.RemoveAll(entry => entry != null && entry.ItemTypeID == SplashSwiftnessPotionTypeId);

            var dict = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (dict != null && dict.TryGetValue(SplashSwiftnessPotionTypeId, out var cachedItem))
            {
                dict.Remove(SplashSwiftnessPotionTypeId);
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
            var dict = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (dict == null)
            {
                return;
            }

            if (dict.ContainsKey(SplashSwiftnessPotionTypeId) && dict[SplashSwiftnessPotionTypeId] != null)
            {
                return;
            }

            var item = ItemAssetsCollection.InstantiateSync(SplashSwiftnessPotionTypeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);
            dict[SplashSwiftnessPotionTypeId] = item;
        }
        catch (Exception exception)
        {
            ModLog.Exception(exception);
        }
    }

    private static StockShopDatabase.ItemEntry CreateMerchantItemEntry()
    {
        return new StockShopDatabase.ItemEntry
        {
            typeID = SplashSwiftnessPotionTypeId,
            maxStock = MerchantStock,
            forceUnlock = true,
            priceFactor = MerchantPrice,
            possibility = 1f,
            lockInDemo = false
        };
    }
}