using System;
using Duckov.Economy;
using Duckov.Modding;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnderPearl;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int EnderPearlTypeId = 900001;
    private const string TargetMerchantId = "Merchant_Equipment";

    private static bool _initialized;
    private static Item? _prefab;

    protected override void OnAfterSetup()
    {
        if (_initialized)
        {
            Debug.Log("[EnderPearl] Already initialized.");
            return;
        }

        _initialized = true;

        Debug.Log("[EnderPearl] Loaded.");

        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefab();
        AddToMerchantProfile();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnBeforeDeactivate()
    {
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

        _initialized = false;
    }

    private static void ApplyLocalizationOverrides()
    {
        // Item.DisplayNameRaw 是本地化 key（Items 表），Description key 是 DisplayNameRaw + "_Desc"
        LocalizationManager.SetOverrideText("Item_EnderPearl", "末影珍珠");
        LocalizationManager.SetOverrideText("Item_EnderPearl_Desc", "手持后：按住显示投掷线，松手投掷。\n落地瞬间将你传送到落点。\n（测试版：NPC 售价 $1）");
    }

    private static void CreateAndRegisterItemPrefab()
    {
        var go = new GameObject("EnderPearl_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();

        // TypeID 的 setter 是 internal，这里用反射直接写入私有字段
        ReflectionUtil.SetPrivateField(item, "typeID", EnderPearlTypeId);

        item.DisplayNameRaw = "Item_EnderPearl";
        item.Icon = RuntimeIcon.CreatePearlIcon();
        item.MaxStackCount = 16;
        item.Value = 1;
        item.Quality = 0;

        // 标记为“技能物品”：快捷栏会走 ChangeHoldItem（拿在手上），从而支持手雷式按住/松手
        item.SetBool("IsSkill", true);

        // 绑定技能：用 ItemSetting_Skill 提供 SkillBase，触发抛物线 HUD（SkillContext.isGrenade=true）
        var skill = go.AddComponent<Skill_EnderPearlThrow>();
        var skillSetting = go.AddComponent<ItemSetting_Skill>();
        skillSetting.Skill = skill;
        skillSetting.onRelease = ItemSetting_Skill.OnReleaseAction.reduceCount;

        go.SetActive(true);

        // 注册为动态物品（InstantiateAsync/Sync 都会认识）
        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;

        Debug.Log($"[EnderPearl] Registered dynamic item. TypeID={EnderPearlTypeId}");
    }

    private static void AddToMerchantProfile()
    {
        var db = StockShopDatabase.Instance;
        if (db == null)
        {
            Debug.LogWarning("[EnderPearl] StockShopDatabase.Instance is null (too early?). Will retry on scene load.");
            return;
        }

        var profile = db.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            Debug.LogWarning($"[EnderPearl] Merchant profile '{TargetMerchantId}' not found.");
            return;
        }

        if (profile.entries.Exists(e => e != null && e.typeID == EnderPearlTypeId))
        {
            return;
        }

        profile.entries.Add(new StockShopDatabase.ItemEntry
        {
            typeID = EnderPearlTypeId,
            maxStock = 99,
            forceUnlock = true,
            priceFactor = 1f,
            possibility = 1f,
            lockInDemo = false
        });

        Debug.Log($"[EnderPearl] Added to merchant profile {TargetMerchantId}.");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 兜底：如果商店对象已经 Awake/Start 过了，确保 entries + itemInstances 都补齐
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

            if (shop.entries.Exists(e => e != null && e.ItemTypeID == EnderPearlTypeId))
            {
                continue;
            }

            var itemEntry = new StockShopDatabase.ItemEntry
            {
                typeID = EnderPearlTypeId,
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

            // StockShop.BuyTask 依赖 itemInstances 已缓存，否则会直接 return false
            EnsureShopHasCachedItemInstance(shop);
        }
    }

    private static void EnsureShopHasCachedItemInstance(StockShop shop)
    {
        try
        {
            var item = ItemAssetsCollection.InstantiateSync(EnderPearlTypeId);
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

            dict[EnderPearlTypeId] = item;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
