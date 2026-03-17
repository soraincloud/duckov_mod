using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duckov.Economy;
using Duckov.Utilities;
using Duckov.Modding;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using ItemStatsSystem.Stats;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TotemOfUndying;

public class ModBehaviour : Duckov.Modding.ModBehaviour
{
    internal const int TotemOfUndyingTypeId = 900011;
    private const int SoulCubeTypeId = 1165;
    private const string DisplayNameKey = "Item_TotemOfUndying";
    private const string FormulaId = "TotemOfUndying_Workbench";
    private const string TargetMerchantId = "Merchant_Equipment";
    private const float TotemWeightKg = 0.3f;
    private const int MerchantPrice = 8400;
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
        ModSfx.Initialize(info.path);
        TotemModelAssets.SetModPath(info.path);

        Debug.Log("[TotemOfUndying] Loaded.");

        ApplyLocalizationOverrides();
        CreateAndRegisterItemPrefab(info.path);
        AddToMerchantProfile();
        RegisterOrUpdateCraftingFormula();
        WorkbenchCraftSystem.Initialize();
        TotemRescueSystem.Initialize(info.path);

        PatchExistingStockShops();

        PlayerStorage.OnLoadingFinished += OnPlayerStorageLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnBeforeDeactivate()
    {
        ModSfx.Deinitialize();
        WorkbenchCraftSystem.Deinitialize();
        TotemRescueSystem.Deinitialize();
        TotemModelAssets.Deinitialize();
        PlayerStorage.OnLoadingFinished -= OnPlayerStorageLoaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveFromMerchantProfile();
        UnpatchExistingStockShops();
        UnregisterCraftingFormula();

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
        LocalizationManager.SetOverrideText(DisplayNameKey + "_Desc", "放入图腾槽位后生效。\n当你受到致命伤害时：\n- 消耗 1 个图腾\n- 免除本次死亡\n- 恢复 30% 最大生命\n- 获得 5 秒无敌\n");
    }

    private static void CreateAndRegisterItemPrefab(string? modPath)
    {
        TotemModelAssets.SetModPath(modPath);

        var go = new GameObject("TotemOfUndying_ItemPrefab");
        go.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var item = go.AddComponent<Item>();

        ReflectionUtil.SetPrivateField(item, "typeID", TotemOfUndyingTypeId);

        item.DisplayNameRaw = DisplayNameKey;
        item.Icon = TryLoadIconSprite(modPath) ?? RuntimeIcon.CreateTotemIcon();
        item.MaxStackCount = 1;
        item.Value = 1;
        item.Quality = 3;
        ReflectionUtil.SetPrivateField(item, "weight", TotemWeightKg);
        item.SetBool("IsSkill", false);

        AttachCharacterModifiers(item);

        TotemModelAssets.TryInjectItemAgents(item, modPath);

        var visualHook = go.AddComponent<TotemVisualHook>();
        visualHook.SetModPath(modPath);

        AddTagIfExists(item, GameplayDataSettings.Tags.DontDropOnDeadInSlot);
        EnsureTotemSlotCompatibility();

        ModLog.Info($"[TotemOfUndying] Item tags: {DescribeItemTags(item)}");

        go.SetActive(true);

        ItemAssetsCollection.AddDynamicEntry(item);

        _prefab = item;

        Debug.Log($"[TotemOfUndying] Registered dynamic item. TypeID={TotemOfUndyingTypeId}");
    }

    private static void AttachCharacterModifiers(Item item)
    {
        item.CreateModifiersComponent();

        if (item.Modifiers == null)
        {
            return;
        }

        var modifierList = ReflectionUtil.GetPrivateField<List<ModifierDescription>>(item.Modifiers, "list");
        if (modifierList == null)
        {
            modifierList = new List<ModifierDescription>();
            ReflectionUtil.SetPrivateField(item.Modifiers, "list", modifierList);
        }

        var walkSpeedModifier = new ModifierDescription(ModifierTarget.Character, "WalkSpeed", ModifierType.PercentageAdd, 0.08f);
        ReflectionUtil.SetPrivateField(walkSpeedModifier, "display", true);
        modifierList.Add(walkSpeedModifier);

        var runSpeedModifier = new ModifierDescription(ModifierTarget.Character, "RunSpeed", ModifierType.PercentageAdd, 0.08f);
        ReflectionUtil.SetPrivateField(runSpeedModifier, "display", true);
        modifierList.Add(runSpeedModifier);
    }

    private static void AddToMerchantProfile()
    {
        var db = StockShopDatabase.Instance;
        if (db == null)
        {
            ModLog.Warn("[TotemOfUndying] StockShopDatabase.Instance is null. Will retry on scene load.");
            return;
        }

        var profile = db.GetMerchantProfile(TargetMerchantId);
        if (profile == null)
        {
            ModLog.Warn($"[TotemOfUndying] Merchant profile '{TargetMerchantId}' not found.");
            return;
        }

        var existing = profile.entries.Find(entry => entry != null && entry.typeID == TotemOfUndyingTypeId);
        if (existing != null)
        {
            existing.maxStock = MerchantStock;
            existing.forceUnlock = true;
            existing.priceFactor = MerchantPrice;
            existing.possibility = 1f;
            existing.lockInDemo = false;
            ModLog.Info($"[TotemOfUndying] Updated merchant profile '{TargetMerchantId}' price to {MerchantPrice}.");
            return;
        }

        profile.entries.Add(CreateMerchantItemEntry());
        ModLog.Info($"[TotemOfUndying] Added to merchant profile '{TargetMerchantId}' at price {MerchantPrice}.");
    }

    private static void RemoveFromMerchantProfile()
    {
        var profile = StockShopDatabase.Instance?.GetMerchantProfile(TargetMerchantId);
        profile?.entries.RemoveAll(entry => entry != null && entry.typeID == TotemOfUndyingTypeId);
    }

    private static void RegisterOrUpdateCraftingFormula()
    {
        var formulas = CraftingFormulaCollection.Instance;
        if (formulas == null)
        {
            ModLog.Warn("[TotemOfUndying] CraftingFormulaCollection.Instance is null. Will retry later.");
            return;
        }

        if (!TryBuildCraftingFormula(formulas, out var formula))
        {
            return;
        }

        var formulaList = ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list");
        if (formulaList == null)
        {
            ModLog.Warn("[TotemOfUndying] Failed to access crafting formula list.");
            return;
        }

        formulaList.RemoveAll(existing => string.Equals(existing.id, FormulaId, StringComparison.Ordinal));
        formulaList.Add(formula);

        EnsureFormulaUnlocked(FormulaId);
        ModLog.Info($"[TotemOfUndying] Registered crafting formula '{FormulaId}' with tags: {string.Join(", ", formula.tags ?? Array.Empty<string>())}");
    }

    private static bool TryBuildCraftingFormula(CraftingFormulaCollection formulas, out CraftingFormula formula)
    {
        formula = default;

        var featherId = ResolveIngredientTypeId("羽毛", "羽毛", "Feather");
        var blueBlockId = ResolveIngredientTypeId("蓝色方块", "蓝色方块", "Blue Block", "Blue Cube", "BlueBlock", "BlueCube");
        var dogTagId = ResolveIngredientTypeId("狗牌", "狗牌", "Dog Tag", "DogTag");
        var topOrganicFiberId = ResolveIngredientTypeId("顶级有机纤维", "顶级有机纤维", "Top Organic Fiber", "Premium Organic Fiber", "Organic Fiber", "TopOrganicFiber", "PremiumOrganicFiber");

        if (featherId < 0 || blueBlockId < 0 || dogTagId < 0 || topOrganicFiberId < 0)
        {
            return false;
        }

        var formulaTags = BuildCompatibleFormulaTags(formulas);

        formula = new CraftingFormula
        {
            id = FormulaId,
            result = new CraftingFormula.ItemEntry
            {
                id = TotemOfUndyingTypeId,
                amount = 1
            },
            tags = formulaTags,
            cost = new Cost(
                (featherId, 10L),
                (blueBlockId, 150L),
                (dogTagId, 3L),
                (topOrganicFiberId, 2L)),
            unlockByDefault = true,
            lockInDemo = false,
            requirePerk = string.Empty,
            hideInIndex = false
        };

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

    internal static bool TryBuildTotemCraftCost(out Cost cost)
    {
        cost = default;

        var featherId = ResolveIngredientTypeId("羽毛", "羽毛", "Feather");
        var blueBlockId = ResolveIngredientTypeId("蓝色方块", "蓝色方块", "Blue Block", "Blue Cube", "BlueBlock", "BlueCube");
        var dogTagId = ResolveIngredientTypeId("狗牌", "狗牌", "Dog Tag", "DogTag");
        var topOrganicFiberId = ResolveIngredientTypeId("顶级有机纤维", "顶级有机纤维", "Top Organic Fiber", "Premium Organic Fiber", "Organic Fiber", "TopOrganicFiber", "PremiumOrganicFiber");

        if (featherId < 0 || blueBlockId < 0 || dogTagId < 0 || topOrganicFiberId < 0)
        {
            return false;
        }

        cost = new Cost(
            (featherId, 10L),
            (blueBlockId, 150L),
            (dogTagId, 3L),
            (topOrganicFiberId, 2L));

        return true;
    }

    private static int ResolveIngredientTypeId(string label, params string[] candidates)
    {
        var collection = ItemAssetsCollection.Instance;
        if (collection?.entries == null)
        {
            ModLog.Warn($"[TotemOfUndying] ItemAssetsCollection not ready while resolving ingredient '{label}'.");
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
                ModLog.Info($"[TotemOfUndying] Ingredient '{label}' matched fuzzily to '{entry.metaData.DisplayName}' ({entry.typeID}).");
                return entry.typeID;
            }
        }

        ModLog.Warn($"[TotemOfUndying] Failed to resolve ingredient '{label}'. Candidates: {string.Join(", ", candidates)}");
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
            ModLog.Warn("[TotemOfUndying] Failed to access unlocked formula list.");
            return;
        }

        if (unlockedFormulaIds.Contains(formulaId))
        {
            return;
        }

        unlockedFormulaIds.Add(formulaId);
        unlockedFormulaIds.Sort(StringComparer.Ordinal);
    }

    private static void UnregisterCraftingFormula()
    {
        var formulas = CraftingFormulaCollection.Instance;
        var formulaList = formulas != null ? ReflectionUtil.GetPrivateField<List<CraftingFormula>>(formulas, "list") : null;
        formulaList?.RemoveAll(existing => string.Equals(existing.id, FormulaId, StringComparison.Ordinal));

        if (CraftingManager.Instance != null)
        {
            var unlockedFormulaIds = ReflectionUtil.GetPrivateField<List<string>>(CraftingManager.Instance, "unlockedFormulaIDs");
            unlockedFormulaIds?.RemoveAll(existing => string.Equals(existing, FormulaId, StringComparison.Ordinal));
        }

        ModLog.Info($"[TotemOfUndying] Unregistered crafting formula '{FormulaId}'.");
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            EnsureTotemSlotCompatibility();
            AddToMerchantProfile();
            PatchExistingStockShops();
            RegisterOrUpdateCraftingFormula();
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
            EnsureTotemSlotCompatibility();
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
            if (shop == null || !string.Equals(shop.MerchantID, TargetMerchantId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var existing = shop.entries.Find(entry => entry != null && entry.ItemTypeID == TotemOfUndyingTypeId);
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
            ModLog.Info($"[TotemOfUndying] Patched live shop '{shop.MerchantID}' with Totem stock.");
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

            shop.entries.RemoveAll(entry => entry != null && entry.ItemTypeID == TotemOfUndyingTypeId);

            var dict = ReflectionUtil.GetPrivateField<Dictionary<int, Item>>(shop, "itemInstances");
            if (dict != null && dict.TryGetValue(TotemOfUndyingTypeId, out var cachedItem))
            {
                dict.Remove(TotemOfUndyingTypeId);
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

            if (dict.ContainsKey(TotemOfUndyingTypeId) && dict[TotemOfUndyingTypeId] != null)
            {
                return;
            }

            var item = ItemAssetsCollection.InstantiateSync(TotemOfUndyingTypeId);
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(shop.transform);
            item.gameObject.SetActive(false);
            dict[TotemOfUndyingTypeId] = item;
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
            typeID = TotemOfUndyingTypeId,
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

    private static void EnsureTotemSlotCompatibility()
    {
        try
        {
            var compatibleTags = ResolveTotemSlotTags();
            if (compatibleTags.Count == 0)
            {
                ModLog.Warn("[TotemOfUndying] Could not resolve any compatible tags for the totem slot.");
                return;
            }

            var patchedCount = 0;

            if (_prefab != null && ApplyCompatibilityTags(_prefab, compatibleTags))
            {
                patchedCount++;
            }

            var liveItems = Resources.FindObjectsOfTypeAll<Item>();
            foreach (var liveItem in liveItems)
            {
                if (liveItem == null || liveItem.TypeID != TotemOfUndyingTypeId)
                {
                    continue;
                }

                if (ApplyCompatibilityTags(liveItem, compatibleTags))
                {
                    patchedCount++;
                }
            }

            if (patchedCount > 0)
            {
                ModLog.Info($"[TotemOfUndying] Applied totem-slot compatibility tags to {patchedCount} totem instance(s). Tags: {string.Join(", ", compatibleTags.Select(tag => tag.name))}");
            }
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Failed to ensure totem-slot compatibility: {e.Message}");
        }
    }

    private static List<Tag> ResolveTotemSlotTags()
    {
        var tags = new List<Tag>();

        void AddTags(IEnumerable<Tag>? source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var tag in source)
            {
                if (tag == null || tags.Any(existing => existing.Hash == tag.Hash))
                {
                    continue;
                }

                tags.Add(tag);
            }
        }

        var soulCubePrefab = ItemAssetsCollection.GetPrefab(SoulCubeTypeId);
        if (soulCubePrefab != null)
        {
            AddTags(soulCubePrefab.Tags);
        }

        var characters = Resources.FindObjectsOfTypeAll<CharacterMainControl>();
        foreach (var character in characters)
        {
            var slots = character?.CharacterItem?.Slots;
            if (slots == null)
            {
                continue;
            }

            foreach (Slot slot in slots)
            {
                if (slot == null || !IsTotemSlotKey(slot.Key))
                {
                    continue;
                }

                AddTags(slot.requireTags);
            }
        }

        return tags;
    }

    private static bool ApplyCompatibilityTags(Item item, IEnumerable<Tag> compatibleTags)
    {
        if (item == null)
        {
            return false;
        }

        var changed = false;
        foreach (var tag in compatibleTags)
        {
            if (tag == null || item.Tags.Contains(tag))
            {
                continue;
            }

            item.Tags.Add(tag);
            changed = true;
        }

        return changed;
    }

    private static bool IsTotemSlotKey(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            return false;
        }

        var key = slotKey.ToLowerInvariant();
        return key == "totem" || key.Contains("totem") || key == "soulcube" || key.Contains("soulcube");
    }

    private static string DescribeItemTags(Item item)
    {
        return string.Join(", ", item.Tags.Select(tag => tag != null ? tag.name : "<null>"));
    }

    private static Sprite? TryLoadIconSprite(string? modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath))
        {
            return null;
        }

        try
        {
            var iconPath = Path.Combine(modPath, "assets", "item-icons", "TotemOfUndying.png");
            if (!File.Exists(iconPath))
            {
                iconPath = Path.Combine(modPath, "icon.png");
            }

            if (!File.Exists(iconPath))
            {
                return null;
            }

            return TryLoadSpriteFromPngFile(iconPath, "TotemOfUndying_Icon");
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Failed to load icon: {e.Message}");
            return null;
        }
    }

    private static Sprite? TryLoadSpriteFromPngFile(string pngPath, string textureName)
    {
        if (string.IsNullOrWhiteSpace(pngPath) || !File.Exists(pngPath))
        {
            return null;
        }

        var pngBytes = File.ReadAllBytes(pngPath);
        if (pngBytes.Length == 0)
        {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
        if (!ImageConversion.LoadImage(texture, pngBytes))
        {
            return null;
        }

        texture.name = textureName;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
    }
}
