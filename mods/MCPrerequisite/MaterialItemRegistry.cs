using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Duckov.Utilities;
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
    internal const int IronNuggetTypeId = 800004;
    internal const int GoldNuggetTypeId = 800005;
    internal const int IronBlockTypeId = 800006;
    internal const int GoldBlockTypeId = 800007;

    private const string TargetMerchantId = "Merchant_Equipment";
    private const int MerchantPrice = 1;
    private const int MerchantStock = 99;
    private const float PickupScaleMultiplier = 3f;
    private const float PickupRefreshIntervalSeconds = 0.4f;
    private const float LootBoxGlassChance = 0.16f;
    private const float LootBoxIronNuggetChance = 0.10f;
    private const float LootBoxIronIngotChance = 0.08f;
    private const float LootBoxGoldNuggetChance = 0.07f;
    private const float LootBoxGoldIngotChance = 0.05f;
    private const float LootBoxIronBlockChance = 0.02f;
    private const float LootBoxGoldBlockChance = 0.01f;

    private static readonly MaterialDefinition[] Definitions =
    {
        new(GlassTypeId, "Item_MCGlass", "玻璃", "常见的建筑材料。", "assets/item-icons/glass.png", "MCPrerequisite_Glass_Icon", "MCGlass_ItemPrefab"),
        new(IronNuggetTypeId, "Item_MCIronNugget", "铁粒", "细碎的铁材料。", "assets/item-icons/ironNugget.png", "MCPrerequisite_IronNugget_Icon", "MCIronNugget_ItemPrefab"),
        new(IronIngotTypeId, "Item_MCIronIngot", "铁锭", "常见的金属材料。", "assets/item-icons/ironIngot.png", "MCPrerequisite_IronIngot_Icon", "MCIronIngot_ItemPrefab"),
        new(IronBlockTypeId, "Item_MCIronBlock", "铁块", "压实后的铁材料。", "assets/item-icons/ironBlock.png", "MCPrerequisite_IronBlock_Icon", "MCIronBlock_ItemPrefab"),
        new(GoldNuggetTypeId, "Item_MCGoldNugget", "金粒", "细碎的贵重金属材料。", "assets/item-icons/goldNugget.png", "MCPrerequisite_GoldNugget_Icon", "MCGoldNugget_ItemPrefab"),
        new(GoldIngotTypeId, "Item_MCGoldIngot", "金锭", "较为贵重的金属材料。", "assets/item-icons/goldIngot.png", "MCPrerequisite_GoldIngot_Icon", "MCGoldIngot_ItemPrefab"),
        new(GoldBlockTypeId, "Item_MCGoldBlock", "金块", "压实后的贵重金属材料。", "assets/item-icons/goldBlock.png", "MCPrerequisite_GoldBlock_Icon", "MCGoldBlock_ItemPrefab")
    };

    private static readonly Dictionary<int, Item> Prefabs = new();
    private static readonly HashSet<int> PatchedLootBoxIds = new();
    private static readonly HashSet<int> ManagedTypeIds = new()
    {
        GlassTypeId,
        IronNuggetTypeId,
        IronIngotTypeId,
        IronBlockTypeId,
        GoldNuggetTypeId,
        GoldIngotTypeId,
        GoldBlockTypeId
    };

    private static readonly BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static float _nextPickupRefreshTime;

    public static void Initialize(string? modPath)
    {
        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefabs(modPath);
        AddToMerchantProfile();
        PatchExistingStockShops();
        LevelManager.OnLevelInitialized += OnLevelInitialized;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void Deinitialize()
    {
        LevelManager.OnLevelInitialized -= OnLevelInitialized;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveFromMerchantProfile();
        UnpatchExistingStockShops();

        foreach (var prefab in Prefabs.Values)
        {
            if (prefab == null)
            {
                continue;
            }

            prefab.AgentUtilities.onCreateAgent -= OnManagedItemCreateAgent;
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
        PatchedLootBoxIds.Clear();
    }

    public static void UpdateRuntimeState()
    {
        if (Time.unscaledTime < _nextPickupRefreshTime)
        {
            return;
        }

        _nextPickupRefreshTime = Time.unscaledTime + PickupRefreshIntervalSeconds;
        InjectManagedMaterialsIntoLootBoxes();
        RefreshManagedPickupScales();
    }

    private static void OnLevelInitialized()
    {
        InjectManagedMaterialsIntoLootBoxes();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddToMerchantProfile();
        PatchExistingStockShops();
        PatchedLootBoxIds.Clear();
        _nextPickupRefreshTime = 0f;
    }

    private static void InjectManagedMaterialsIntoLootBoxes()
    {
        var loaders = UnityEngine.Object.FindObjectsOfType<LootBoxLoader>();
        if (loaders == null || loaders.Length == 0)
        {
            return;
        }

        foreach (var loader in loaders)
        {
            if (loader == null)
            {
                continue;
            }

            var loaderId = loader.GetInstanceID();
            if (PatchedLootBoxIds.Contains(loaderId))
            {
                continue;
            }

            var lootBox = loader.GetComponent<InteractableLootbox>();
            var inventory = lootBox?.Inventory;
            if (lootBox == null || inventory == null || inventory.Loading)
            {
                continue;
            }

            if (ContainsManagedMaterial(inventory))
            {
                PatchedLootBoxIds.Add(loaderId);
                continue;
            }

            var materialTypeId = RollLootBoxMaterialTypeId();
            PatchedLootBoxIds.Add(loaderId);
            if (materialTypeId == 0)
            {
                continue;
            }

            var item = ItemAssetsCollection.InstantiateSync(materialTypeId);
            if (item == null)
            {
                continue;
            }

            if (!inventory.AddItem(item))
            {
                inventory.SetCapacity(inventory.Capacity + 1);
                if (!inventory.AddItem(item))
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }
    }

    private static bool ContainsManagedMaterial(Inventory inventory)
    {
        return ManagedTypeIds.Any(typeId => inventory.Find(typeId) != null);
    }

    private static int RollLootBoxMaterialTypeId()
    {
        var roll = UnityEngine.Random.Range(0f, 1f);
        if (roll < LootBoxGlassChance)
        {
            return GlassTypeId;
        }

        roll -= LootBoxGlassChance;
        if (roll < LootBoxIronNuggetChance)
        {
            return IronNuggetTypeId;
        }

        roll -= LootBoxIronNuggetChance;
        if (roll < LootBoxIronIngotChance)
        {
            return IronIngotTypeId;
        }

        roll -= LootBoxIronIngotChance;
        if (roll < LootBoxIronBlockChance)
        {
            return IronBlockTypeId;
        }

        roll -= LootBoxIronBlockChance;
        if (roll < LootBoxGoldNuggetChance)
        {
            return GoldNuggetTypeId;
        }

        roll -= LootBoxGoldNuggetChance;
        if (roll < LootBoxGoldIngotChance)
        {
            return GoldIngotTypeId;
        }

        roll -= LootBoxGoldIngotChance;
        if (roll < LootBoxGoldBlockChance)
        {
            return GoldBlockTypeId;
        }

        return 0;
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
            item.AgentUtilities.onCreateAgent += OnManagedItemCreateAgent;

            go.SetActive(true);
            ItemAssetsCollection.AddDynamicEntry(item);
            Prefabs[definition.TypeId] = item;
        }
    }

    private static void OnManagedItemCreateAgent(Item sourceItem, ItemAgent newAgent)
    {
        if (sourceItem == null || newAgent == null)
        {
            return;
        }

        if (!ManagedTypeIds.Contains(sourceItem.TypeID) || newAgent.AgentType != ItemAgent.AgentTypes.pickUp)
        {
            return;
        }

        EnsurePickupVisualScale(newAgent.gameObject, ResolvePickupSprite(newAgent.GetComponent<InteractablePickup>()), PickupScaleMultiplier);
    }

    private static void RefreshManagedPickupScales()
    {
        var pickups = UnityEngine.Object.FindObjectsOfType<InteractablePickup>();
        if (pickups == null || pickups.Length == 0)
        {
            return;
        }

        foreach (var pickup in pickups)
        {
            if (pickup == null)
            {
                continue;
            }

            var item = pickup.ItemAgent?.Item;
            if (item == null || !ManagedTypeIds.Contains(item.TypeID))
            {
                continue;
            }

            EnsurePickupVisualScale(pickup.gameObject, ResolvePickupSprite(pickup), PickupScaleMultiplier);
        }
    }

    private static void EnsurePickupVisualScale(GameObject root, SpriteRenderer? pickupSprite, float multiplier)
    {
        if (root == null)
        {
            return;
        }

        var fixedScale = Vector3.one * multiplier;
        if (pickupSprite != null)
        {
            pickupSprite.transform.localScale = fixedScale;
        }

        var spriteRenderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer == null)
            {
                continue;
            }

            spriteRenderer.transform.localScale = fixedScale;
        }
    }

    private static SpriteRenderer? ResolvePickupSprite(InteractablePickup? pickup)
    {
        if (pickup == null)
        {
            return null;
        }

        var field = pickup.GetType().GetField("sprite", InstanceBindingFlags);
        return field?.GetValue(pickup) as SpriteRenderer;
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
