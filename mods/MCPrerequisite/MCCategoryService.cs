using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCPrerequisite;

public static class MCCategoryService
{
    public const string SharedCategoryTagName = "ModWorkbench_Mystic";
    public const string SharedCategoryDisplayNameKey = "CraftFilter_ModMystic";
    public const string MaterialCategoryTagName = "InventoryFilter_MCMaterialTag";
    public const string MaterialCategoryDisplayNameKey = "InventoryFilter_MCMaterial";
    private const float RuntimeRefreshIntervalSeconds = 0.5f;
    private const int StartupRefreshAttempts = 6;
    private const int SceneRefreshAttempts = 8;
    private const int StorageRefreshAttempts = 4;

    private static readonly int[] ManagedItemTypeIds =
    {
        900001,
        900002,
        900011,
        900012
    };

    private static readonly int[] ManagedMaterialItemTypeIds =
    {
        MaterialItemRegistry.GlassTypeId,
        MaterialItemRegistry.IronIngotTypeId,
        MaterialItemRegistry.GoldIngotTypeId
    };

    private static readonly BindingFlags AllBindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static bool _initialized;
    private static string? _modPath;
    private static Tag? _sharedCategoryTag;
    private static Tag? _materialCategoryTag;
    private static Sprite? _sharedCategoryIcon;
    private static Sprite? _materialCategoryIcon;
    private static bool _runtimeRefreshPending;
    private static int _remainingRuntimeRefreshAttempts;
    private static float _nextRuntimeRefreshTime;

    public static void Initialize(string? modPath)
    {
        _modPath = modPath;
        LocalizationManager.SetOverrideText(SharedCategoryDisplayNameKey, "MC");
        LocalizationManager.SetOverrideText(MaterialCategoryDisplayNameKey, "MC材料");

        if (_initialized)
        {
            EnsureFiltersRegistered();
            return;
        }

        _initialized = true;
        _runtimeRefreshPending = false;
        _remainingRuntimeRefreshAttempts = 0;
        _nextRuntimeRefreshTime = 0f;
        PlayerStorage.OnLoadingFinished += OnPlayerStorageLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureManagedItemsTagged();
        EnsureFiltersRegistered();
        ScheduleRuntimeRefresh(StartupRefreshAttempts, RuntimeRefreshIntervalSeconds);
    }

    public static void Deinitialize()
    {
        if (_initialized)
        {
            PlayerStorage.OnLoadingFinished -= OnPlayerStorageLoaded;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        _initialized = false;
        _modPath = null;
        _sharedCategoryTag = null;
        _materialCategoryTag = null;
        _runtimeRefreshPending = false;
        _remainingRuntimeRefreshAttempts = 0;
        _nextRuntimeRefreshTime = 0f;
        DestroySharedCategoryIcon();
        DestroyMaterialCategoryIcon();
    }

    public static void UpdateRuntimeState()
    {
        if (!_initialized)
        {
            return;
        }

        if (!_runtimeRefreshPending || Time.unscaledTime < _nextRuntimeRefreshTime)
        {
            return;
        }

        try
        {
            EnsureManagedItemsTagged();
            EnsureFiltersRegistered();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to update runtime state: {e.Message}");
        }

        _remainingRuntimeRefreshAttempts--;
        if (_remainingRuntimeRefreshAttempts <= 0)
        {
            _runtimeRefreshPending = false;
            _nextRuntimeRefreshTime = 0f;
            return;
        }

        _nextRuntimeRefreshTime = Time.unscaledTime + RuntimeRefreshIntervalSeconds;
    }

    public static void EnsureFiltersRegistered()
    {
        RegisterCraftCategoryFilter();
        RegisterStorageCategoryFilter();
    }

    public static void EnsureStorageFilterRegistered()
    {
        RegisterStorageCategoryFilter();
    }

    public static void AttachSharedCategory(Item item)
    {
        var sharedTag = GetOrCreateSharedCategoryTag(SharedCategoryTagName);
        TryAttachCategoryTag(item, sharedTag);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            EnsureManagedItemsTagged();
            EnsureFiltersRegistered();
            ScheduleRuntimeRefresh(SceneRefreshAttempts, RuntimeRefreshIntervalSeconds);
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
            EnsureManagedItemsTagged();
            EnsureStorageFilterRegistered();
            ScheduleRuntimeRefresh(StorageRefreshAttempts, RuntimeRefreshIntervalSeconds);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static void RegisterCraftCategoryFilter()
    {
        try
        {
            var filterTag = GetOrCreateSharedCategoryTag(SharedCategoryTagName);
            var filterIcon = TryLoadSharedCategoryIconSprite(_modPath);
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
            Debug.LogWarning($"[MCPrerequisite] Failed to register craft category filter: {e.Message}");
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

            var filterTag = GetOrCreateSharedCategoryTag(SharedCategoryTagName);
            var filterIcon = TryLoadSharedCategoryIconSprite(_modPath);
            EnsureInventoryHasCategoryFilter(provider, filterTag, filterIcon, SharedCategoryDisplayNameKey, SharedCategoryTagName);

            var materialTag = GetOrCreateCategoryTag(MaterialCategoryTagName);
            var materialIcon = TryLoadMaterialCategoryIconSprite(_modPath);
            EnsureInventoryHasCategoryFilter(provider, materialTag, materialIcon, MaterialCategoryDisplayNameKey, MaterialCategoryTagName);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to register storage category filter: {e.Message}");
        }
    }

    private static void EnsureCraftViewHasSharedCategoryFilter(CraftView craftView, Tag filterTag, Sprite? filterIcon)
    {
        var filters = ReflectionUtil.GetPrivateField<CraftView.FilterInfo[]>(craftView, "filters") ?? Array.Empty<CraftView.FilterInfo>();
        var updatedFilters = filters.ToList();
        var index = updatedFilters.FindIndex(HasSharedCategoryFilter);
        var changed = false;
        var filterInfo = new CraftView.FilterInfo
        {
            displayNameKey = SharedCategoryDisplayNameKey,
            icon = filterIcon,
            requireTags = new[] { filterTag }
        };

        if (index >= 0)
        {
            var existing = updatedFilters[index];
            if (string.IsNullOrWhiteSpace(existing.displayNameKey))
            {
                existing.displayNameKey = filterInfo.displayNameKey;
                changed = true;
            }

            if (existing.icon == null && filterInfo.icon != null)
            {
                existing.icon = filterInfo.icon;
                changed = true;
            }

            var mergedTags = MergeFilterTags(existing.requireTags, filterTag);
            if (!ReferenceEquals(existing.requireTags, mergedTags))
            {
                existing.requireTags = mergedTags;
                changed = true;
            }

            updatedFilters[index] = existing;
        }
        else
        {
            updatedFilters.Add(filterInfo);
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        ReflectionUtil.SetPrivateField(craftView, "filters", updatedFilters.ToArray());

        var currentFilterIndexField = typeof(CraftView).GetField("currentFilterIndex", AllBindings);
        var currentFilterIndex = 0;
        if (currentFilterIndexField?.GetValue(craftView) is int value)
        {
            currentFilterIndex = value;
        }

        var predicateField = typeof(CraftView).GetField("predicate", AllBindings);
        if (predicateField?.GetValue(craftView) is Predicate<CraftingFormula>)
        {
            craftView.SetFilter(Mathf.Clamp(currentFilterIndex, 0, Math.Max(0, updatedFilters.Count - 1)));
        }
    }

    private static bool HasSharedCategoryFilter(CraftView.FilterInfo filter)
    {
        if (filter.requireTags == null)
        {
            return false;
        }

        return filter.requireTags.Any(tag => tag != null && Tag.Match(tag, SharedCategoryTagName));
    }

    private static void EnsureInventoryHasCategoryFilter(InventoryFilterProvider provider, Tag filterTag, Sprite? filterIcon, string displayNameKey, string tagName)
    {
        var filters = provider.entries ?? Array.Empty<InventoryFilterProvider.FilterEntry>();
        var updatedFilters = filters.ToList();
        var index = updatedFilters.FindIndex(filter => HasStorageCategoryFilter(filter, tagName));
        var changed = false;
        var filterEntry = new InventoryFilterProvider.FilterEntry
        {
            name = displayNameKey,
            icon = filterIcon,
            requireTags = new[] { filterTag }
        };

        if (index >= 0)
        {
            var existing = updatedFilters[index];
            if (string.IsNullOrWhiteSpace(existing.name))
            {
                existing.name = filterEntry.name;
                changed = true;
            }

            if (existing.icon == null && filterEntry.icon != null)
            {
                existing.icon = filterEntry.icon;
                changed = true;
            }

            var mergedTags = MergeFilterTags(existing.requireTags, filterTag);
            if (!ReferenceEquals(existing.requireTags, mergedTags))
            {
                existing.requireTags = mergedTags;
                changed = true;
            }

            updatedFilters[index] = existing;
        }
        else
        {
            updatedFilters.Add(filterEntry);
            changed = true;
        }

        if (changed)
        {
            provider.entries = updatedFilters.ToArray();
        }
    }

    private static bool HasStorageCategoryFilter(InventoryFilterProvider.FilterEntry filter, string tagName)
    {
        if (filter.requireTags == null)
        {
            return false;
        }

        return filter.requireTags.Any(tag => tag != null && Tag.Match(tag, tagName));
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
                if (tag != null && !merged.Any(existing => existing != null && existing.Hash == tag.Hash))
                {
                    merged.Add(tag);
                }
            }
        }

        if (!merged.Any(existing => existing != null && existing.Hash == filterTag.Hash))
        {
            merged.Add(filterTag);
            return merged.ToArray();
        }

        return existingTags ?? merged.ToArray();
    }

    private static void EnsureManagedItemsTagged()
    {
        var sharedTag = GetOrCreateSharedCategoryTag(SharedCategoryTagName);
        var materialTag = GetOrCreateCategoryTag(MaterialCategoryTagName);

        foreach (var typeId in ManagedItemTypeIds)
        {
            TryPatchDynamicItem(typeId, sharedTag);
        }

        foreach (var typeId in ManagedMaterialItemTypeIds)
        {
            TryPatchDynamicItem(typeId, materialTag);
        }

        var liveItems = Resources.FindObjectsOfTypeAll<Item>();
        if (liveItems == null || liveItems.Length == 0)
        {
            return;
        }

        foreach (var item in liveItems)
        {
            if (item == null)
            {
                continue;
            }

            if (ManagedItemTypeIds.Contains(item.TypeID))
            {
                TryAttachCategoryTag(item, sharedTag);
            }

            if (ManagedMaterialItemTypeIds.Contains(item.TypeID))
            {
                TryAttachCategoryTag(item, materialTag);
            }
        }
    }

    private static bool TryPatchDynamicItem(int typeId, Tag sharedTag)
    {
        var dynamicEntriesField = typeof(ItemAssetsCollection).GetField("dynamicDic", AllBindings);
        if (dynamicEntriesField?.GetValue(null) is not System.Collections.IDictionary dynamicEntries)
        {
            return false;
        }

        if (!dynamicEntries.Contains(typeId))
        {
            return false;
        }

        var entry = dynamicEntries[typeId];
        if (entry == null)
        {
            return false;
        }

        var entryType = entry.GetType();
        var prefabField = entryType.GetField("prefab", AllBindings);
        if (prefabField?.GetValue(entry) is not Item prefab)
        {
            return false;
        }

        var changed = TryAttachCategoryTag(prefab, sharedTag);

        var metaDataField = entryType.GetField("_metaData", AllBindings);
        if (changed && metaDataField != null)
        {
            metaDataField.SetValue(entry, new ItemMetaData(prefab));
        }

        return changed;
    }

    private static bool TryAttachCategoryTag(Item? item, Tag sharedTag)
    {
        if (item == null || item.Tags.Contains(sharedTag))
        {
            return false;
        }

        item.Tags.Add(sharedTag);
        return true;
    }

    private static Tag GetOrCreateCategoryTag(string tagName)
    {
        if (string.Equals(tagName, SharedCategoryTagName, StringComparison.Ordinal))
        {
            return GetOrCreateSharedCategoryTag(tagName);
        }

        if (string.Equals(tagName, MaterialCategoryTagName, StringComparison.Ordinal) && _materialCategoryTag != null)
        {
            return _materialCategoryTag;
        }

        var existingTag = GameplayDataSettings.Tags?.AllTags?.FirstOrDefault(tag => tag != null && Tag.Match(tag, tagName));
        if (existingTag != null)
        {
            if (string.Equals(tagName, MaterialCategoryTagName, StringComparison.Ordinal))
            {
                _materialCategoryTag = existingTag;
            }

            return existingTag;
        }

        var runtimeTag = ScriptableObject.CreateInstance<Tag>();
        runtimeTag.name = tagName;
        runtimeTag.hideFlags = HideFlags.HideAndDontSave;

        if (string.Equals(tagName, MaterialCategoryTagName, StringComparison.Ordinal))
        {
            _materialCategoryTag = runtimeTag;
        }

        return runtimeTag;
    }

    private static Tag GetOrCreateSharedCategoryTag(string tagName)
    {
        if (string.Equals(tagName, SharedCategoryTagName, StringComparison.Ordinal) && _sharedCategoryTag != null)
        {
            return _sharedCategoryTag;
        }

        var existingTag = GameplayDataSettings.Tags?.AllTags?.FirstOrDefault(tag => tag != null && Tag.Match(tag, tagName));
        if (existingTag != null)
        {
            if (string.Equals(tagName, SharedCategoryTagName, StringComparison.Ordinal))
            {
                _sharedCategoryTag = existingTag;
            }

            return existingTag;
        }

        var runtimeTag = ScriptableObject.CreateInstance<Tag>();
        runtimeTag.name = tagName;
        runtimeTag.hideFlags = HideFlags.HideAndDontSave;

        if (string.Equals(tagName, SharedCategoryTagName, StringComparison.Ordinal))
        {
            _sharedCategoryTag = runtimeTag;
        }

        return runtimeTag;
    }

    private static Sprite? TryLoadSharedCategoryIconSprite(string? modPath)
    {
        if (_sharedCategoryIcon != null)
        {
            return _sharedCategoryIcon;
        }

        if (string.IsNullOrWhiteSpace(modPath))
        {
            return null;
        }

        try
        {
            _sharedCategoryIcon = ModAssets.TryLoadSprite(modPath, Path.Combine("assets", "item-icons", "grass.png"), "MCPrerequisite_CraftCategory_Icon");
            return _sharedCategoryIcon;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to load craft category icon: {e.Message}");
            return null;
        }
    }

    private static Sprite? TryLoadMaterialCategoryIconSprite(string? modPath)
    {
        if (_materialCategoryIcon != null)
        {
            return _materialCategoryIcon;
        }

        if (string.IsNullOrWhiteSpace(modPath))
        {
            return null;
        }

        try
        {
            _materialCategoryIcon = ModAssets.TryLoadSprite(modPath, Path.Combine("assets", "item-icons", "ironIngot.png"), "MCPrerequisite_MaterialCategory_Icon");
            return _materialCategoryIcon;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to load material category icon: {e.Message}");
            return null;
        }
    }

    private static void ScheduleRuntimeRefresh(int attempts, float delaySeconds)
    {
        if (attempts <= 0)
        {
            return;
        }

        _runtimeRefreshPending = true;
        _remainingRuntimeRefreshAttempts = Math.Max(_remainingRuntimeRefreshAttempts, attempts);

        var scheduledTime = Time.unscaledTime + Math.Max(0f, delaySeconds);
        if (_nextRuntimeRefreshTime <= 0f || scheduledTime < _nextRuntimeRefreshTime)
        {
            _nextRuntimeRefreshTime = scheduledTime;
        }
    }

    private static void DestroySharedCategoryIcon()
    {
        if (_sharedCategoryIcon == null)
        {
            return;
        }

        var texture = _sharedCategoryIcon.texture;
        UnityEngine.Object.Destroy(_sharedCategoryIcon);
        if (texture != null)
        {
            UnityEngine.Object.Destroy(texture);
        }

        _sharedCategoryIcon = null;
    }

    private static void DestroyMaterialCategoryIcon()
    {
        if (_materialCategoryIcon == null)
        {
            return;
        }

        var texture = _materialCategoryIcon.texture;
        UnityEngine.Object.Destroy(_materialCategoryIcon);
        if (texture != null)
        {
            UnityEngine.Object.Destroy(texture);
        }

        _materialCategoryIcon = null;
    }
}