using System;
using System.IO;
using Duckov.Economy;
using Duckov.Utilities;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TotemOfUndying;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int TotemOfUndyingTypeId = 900011;
    private const string TargetMerchantId = "Merchant_Equipment";
    private const string DisplayNameKey = "Item_TotemOfUndying";

    private static bool _initialized;
    private static Item? _prefab;
    private static string? _modPath;

    protected override void OnAfterSetup()
    {
        if (_initialized)
        {
            Debug.Log("[TotemOfUndying] Already initialized.");
            return;
        }

        _initialized = true;
        _modPath = info.path;

        ModLog.Initialize(info.path);

        Debug.Log("[TotemOfUndying] Loaded.");

        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefab(info.path);
        AddToMerchantProfile();
        TotemRescueSystem.Initialize();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnBeforeDeactivate()
    {
        TotemRescueSystem.Deinitialize();
        SceneManager.sceneLoaded -= OnSceneLoaded;

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

        _modPath = null;
        _initialized = false;
    }

    private static void ApplyLocalizationOverrides()
    {
        LocalizationManager.SetOverrideText(DisplayNameKey, "不死图腾");
        LocalizationManager.SetOverrideText(DisplayNameKey + "_Desc", "放入图腾槽位后生效。\n当你受到致命伤害时：\n- 消耗 1 个图腾\n- 免除本次死亡\n- 恢复 50% 最大生命\n- 获得 3 秒无敌\n并爆发黄绿粒子效果。");
    }

    private static void CreateAndRegisterItemPrefab(string? modPath)
    {
        var go = new GameObject("TotemOfUndying_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();

        ReflectionUtil.SetPrivateField(item, "typeID", TotemOfUndyingTypeId);

        item.DisplayNameRaw = DisplayNameKey;
        item.Icon = TryLoadIconSprite(modPath) ?? RuntimeIcon.CreateTotemIcon();
        item.MaxStackCount = 1;
        item.Value = 300;
        item.Quality = 3;
        item.SetBool("IsSkill", false);

        AddTagIfExists(item, GameplayDataSettings.Tags.Special);
        AddTagIfExists(item, GameplayDataSettings.Tags.DontDropOnDeadInSlot);
        EnsureRuntimeTag(item, "Totem");
        EnsureRuntimeTag(item, "SoulCube");

        go.SetActive(true);

        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;

        Debug.Log($"[TotemOfUndying] Registered dynamic item. TypeID={TotemOfUndyingTypeId}");
    }

    private static void AddToMerchantProfile()
    {
        var db = StockShopDatabase.Instance;
        if (db == null)
        {
            Debug.LogWarning("[TotemOfUndying] StockShopDatabase.Instance is null (too early?). Will retry on scene load.");
            return;
        }

        var profile = db.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            Debug.LogWarning($"[TotemOfUndying] Merchant profile '{TargetMerchantId}' not found.");
            return;
        }

        if (profile.entries.Exists(e => e != null && e.typeID == TotemOfUndyingTypeId))
        {
            return;
        }

        profile.entries.Add(new StockShopDatabase.ItemEntry
        {
            typeID = TotemOfUndyingTypeId,
            maxStock = 99,
            forceUnlock = true,
            priceFactor = 1f,
            possibility = 1f,
            lockInDemo = false
        });

        Debug.Log($"[TotemOfUndying] Added to merchant profile {TargetMerchantId}.");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            PatchExistingStockShops();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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
            if (shop == null)
            {
                continue;
            }

            if (!string.Equals(shop.MerchantID, TargetMerchantId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (shop.entries.Exists(e => e != null && e.ItemTypeID == TotemOfUndyingTypeId))
            {
                continue;
            }

            var itemEntry = new StockShopDatabase.ItemEntry
            {
                typeID = TotemOfUndyingTypeId,
                maxStock = 99,
                forceUnlock = true,
                priceFactor = 1f,
                possibility = 1f,
                lockInDemo = false
            };

            var entry = new StockShop.Entry(itemEntry)
            {
                Show = true,
                CurrentStock = 99
            };

            shop.entries.Add(entry);
            EnsureShopHasCachedItemInstance(shop);
        }
    }

    private static void EnsureShopHasCachedItemInstance(StockShop shop)
    {
        try
        {
            var item = ItemAssetsCollection.InstantiateSync(TotemOfUndyingTypeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);

            var dict = ReflectionUtil.GetPrivateField<System.Collections.Generic.Dictionary<int, Item>>(shop, "itemInstances");
            if (dict == null)
            {
                return;
            }

            dict[TotemOfUndyingTypeId] = item;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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

        var runtimeTag = ScriptableObject.CreateInstance<Tag>();
        runtimeTag.name = tagName;
        runtimeTag.hideFlags = HideFlags.HideAndDontSave;
        item.Tags.Add(runtimeTag);
    }

    private static Sprite? TryLoadIconSprite(string? modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath))
        {
            return null;
        }

        try
        {
            var iconPath = Path.Combine(modPath, "icon.png");
            if (!File.Exists(iconPath))
            {
                return null;
            }

            var pngBytes = File.ReadAllBytes(iconPath);
            if (pngBytes.Length == 0)
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!ImageConversion.LoadImage(texture, pngBytes))
            {
                return null;
            }

            texture.name = "TotemOfUndying_Icon";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var rect = new Rect(0, 0, texture.width, texture.height);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Failed to load icon.png: {e.Message}");
            return null;
        }
    }
}
