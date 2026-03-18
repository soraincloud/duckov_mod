using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Economy;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCPrerequisite;

internal static class MaterialItemRegistry
{
    internal const int GlassTypeId = 800001;
    internal const int IronIngotTypeId = 800002;
    internal const int GoldIngotTypeId = 800003;

    private const string TargetMerchantId = "Merchant_Equipment";
    private const int MerchantPrice = 1;
    private const int MerchantStock = 99;

    private static readonly MaterialDefinition[] Definitions =
    {
        new(MaterialItemRegistry.GlassTypeId, "Item_MCGlass", "玻璃", "常见的建筑材料。", "assets/item-icons/glass.png", "MCPrerequisite_Glass_Icon", "MCGlass_ItemPrefab"),
        new(MaterialItemRegistry.IronIngotTypeId, "Item_MCIronIngot", "铁锭", "常见的金属材料。", "assets/item-icons/ironIngot.png", "MCPrerequisite_IronIngot_Icon", "MCIronIngot_ItemPrefab"),
        new(MaterialItemRegistry.GoldIngotTypeId, "Item_MCGoldIngot", "金锭", "较为贵重的金属材料。", "assets/item-icons/goldIngot.png", "MCPrerequisite_GoldIngot_Icon", "MCGoldIngot_ItemPrefab")
    };

    private static readonly Dictionary<int, Item> Prefabs = new();

    public static void Initialize(string? modPath)
    {
        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefabs(modPath);
        AddToMerchantProfile();
        PatchExistingStockShops();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void Deinitialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveFromMerchantProfile();
        UnpatchExistingStockShops();

        foreach (var prefab in Prefabs.Values)
        {
            if (prefab == null)
            {
                continue;
            }

            ItemAssetsCollection.RemoveDynamicEntry(prefab);
            try
            {
                UnityEngine.Object.Destroy(prefab.gameObject);
            }
            catch
            {
                // ignore
            }
        }

        Prefabs.Clear();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToMerchantProfile();
        PatchExistingStockShops();
    }

    private static void ApplyLocalizationOverrides()
    {
        foreach (var definition in Definitions)
        {
            LocalizationManager.SetOverrideText(definition.DisplayNameKey, definition.DisplayName);
            LocalizationManager.SetOverrideText(definition.DisplayNameKey + "_Desc", definition.Description);
        }
    }

    private static void CreateAndRegisterItemPrefabs(string? modPath)
    {
        foreach (var definition in Definitions)
        {
            if (Prefabs.ContainsKey(definition.TypeId))
            {
                continue;
            }

            var go = new GameObject(definition.PrefabName);
            go.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(go);

            var item = go.AddComponent<Item>();
            ReflectionUtil.SetPrivateField(item, "typeID", definition.TypeId);

            item.DisplayNameRaw = definition.DisplayNameKey;
            item.Icon = ModAssets.TryLoadSprite(modPath, definition.IconRelativePath, definition.TextureName);
            item.MaxStackCount = 64;
            item.Value = 1;
            item.Quality = 0;
            item.SetBool("IsSkill", false);

            go.SetActive(true);
            ItemAssetsCollection.AddDynamicEntry(item);
            Prefabs[definition.TypeId] = item;
        }
    }

    private static void AddToMerchantProfile()
    {
        var profile = StockShopDatabase.Instance?.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            return;
        }

        foreach (var definition in Definitions)
        {
            var existing = profile.entries.Find(entry => entry != null && entry.typeID == definition.TypeId);
            if (existing != null)
            {
                existing.maxStock = MerchantStock;
                existing.forceUnlock = true;
                existing.priceFactor = MerchantPrice;
                existing.possibility = 1f;
                existing.lockInDemo = false;
                continue;
            }

            profile.entries.Add(CreateMerchantItemEntry(definition.TypeId));
        }
    }

    private static void RemoveFromMerchantProfile()
    {
        var profile = StockShopDatabase.Instance?.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            return;
        }

        var typeIds = Definitions.Select(definition => definition.TypeId).ToHashSet();
        profile.entries.RemoveAll(entry => entry != null && typeIds.Contains(entry.typeID));
    }

    private static StockShopDatabase.ItemEntry CreateMerchantItemEntry(int typeId)
    {
        return new StockShopDatabase.ItemEntry
        {
            typeID = typeId,
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

            foreach (var definition in Definitions)
            {
                var existing = shop.entries.Find(entry => entry != null && entry.ItemTypeID == definition.TypeId);
                if (existing != null)
                {
                    existing.Show = true;
                    if (existing.CurrentStock < 1)
                    {
                        existing.CurrentStock = MerchantStock;
                    }

                    EnsureShopHasCachedItemInstance(shop, definition.TypeId);
                    continue;
                }

                var entry = new StockShop.Entry(CreateMerchantItemEntry(definition.TypeId))
                {
                    Show = true,
                    CurrentStock = MerchantStock
                };

                shop.entries.Add(entry);
                EnsureShopHasCachedItemInstance(shop, definition.TypeId);
            }
        }
    }

    private static void UnpatchExistingStockShops()
    {
        var shops = UnityEngine.Object.FindObjectsOfType<StockShop>();
        if (shops == null || shops.Length == 0)
        {
            return;
        }

        var typeIds = Definitions.Select(definition => definition.TypeId).ToArray();

        foreach (var shop in shops)
        {
            if (shop == null || !string.Equals(shop.MerchantID, TargetMerchantId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            shop.entries.RemoveAll(entry => entry != null && typeIds.Contains(entry.ItemTypeID));

            var instances = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (instances == null)
            {
                continue;
            }

            foreach (var typeId in typeIds)
            {
                if (!instances.TryGetValue(typeId, out var cachedItem))
                {
                    continue;
                }

                instances.Remove(typeId);
                if (cachedItem == null)
                {
                    continue;
                }

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

    private static void EnsureShopHasCachedItemInstance(StockShop shop, int typeId)
    {
        try
        {
            var instances = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (instances == null)
            {
                return;
            }

            if (instances.ContainsKey(typeId) && instances[typeId] != null)
            {
                return;
            }

            var item = ItemAssetsCollection.InstantiateSync(typeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);
            instances[typeId] = item;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to cache merchant item instance {typeId}: {e.Message}");
        }
    }

    private readonly struct MaterialDefinition
    {
        public MaterialDefinition(int typeId, string displayNameKey, string displayName, string description, string iconRelativePath, string textureName, string prefabName)
        {
            TypeId = typeId;
            DisplayNameKey = displayNameKey;
            DisplayName = displayName;
            Description = description;
            IconRelativePath = iconRelativePath;
            TextureName = textureName;
            PrefabName = prefabName;
        }

        public int TypeId { get; }
        public string DisplayNameKey { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string IconRelativePath { get; }
        public string TextureName { get; }
        public string PrefabName { get; }
    }
}