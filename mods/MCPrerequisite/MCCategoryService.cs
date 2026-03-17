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

    private static readonly int[] ManagedItemTypeIds =
    {
        900001,
        900011,
        900012
    };

    private static readonly BindingFlags AllBindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private static bool _initialized;
    private static string? _modPath;
    private static Tag? _sharedCategoryTag;
    private static float _nextRuntimeRefreshTime;

    public static void Initialize(string? modPath)
    {
        _modPath = modPath;
        LocalizationManager.SetOverrideText(SharedCategoryDisplayNameKey, "MC");

        if (_initialized)
        {
            EnsureFiltersRegistered();
            return;
        }

        _initialized = true;
        _nextRuntimeRefreshTime = 0f;
        PlayerStorage.OnLoadingFinished += OnPlayerStorageLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureManagedItemsTagged();
        EnsureFiltersRegistered();
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
        _nextRuntimeRefreshTime = 0f;
    }

    public static void UpdateRuntimeState()
    {
        if (!_initialized)
        {
            return;
        }

        if (Time.unscaledTime < _nextRuntimeRefreshTime)
        {
            return;
        }

        _nextRuntimeRefreshTime = Time.unscaledTime + 0.25f;
        try
        {
            EnsureManagedItemsTagged();
            EnsureFiltersRegistered();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to update runtime state: {e.Message}");
        }
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
        TryAttachSharedCategory(item, sharedTag);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
            EnsureManagedItemsTagged();
            EnsureFiltersRegistered();
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
            var filterIcon = TryLoadCraftCategoryIconSprite(_modPath);
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
            var filterIcon = TryLoadCraftCategoryIconSprite(_modPath);
            EnsureInventoryHasSharedCategoryFilter(provider, filterTag, filterIcon);
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

    private static void EnsureInventoryHasSharedCategoryFilter(InventoryFilterProvider provider, Tag filterTag, Sprite? filterIcon)
    {
        var filters = provider.entries ?? Array.Empty<InventoryFilterProvider.FilterEntry>();
        var updatedFilters = filters.ToList();
        var index = updatedFilters.FindIndex(HasSharedStorageCategoryFilter);
        var filterEntry = new InventoryFilterProvider.FilterEntry
        {
            name = SharedCategoryDisplayNameKey,
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

        return filter.requireTags.Any(tag => tag != null && Tag.Match(tag, SharedCategoryTagName));
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

    private static void EnsureManagedItemsTagged()
    {
        var sharedTag = GetOrCreateSharedCategoryTag(SharedCategoryTagName);

        foreach (var typeId in ManagedItemTypeIds)
        {
            TryPatchDynamicItem(typeId, sharedTag);
        }

        var liveItems = Resources.FindObjectsOfTypeAll<Item>();
        if (liveItems == null || liveItems.Length == 0)
        {
            return;
        }

        foreach (var item in liveItems)
        {
            if (item == null || !ManagedItemTypeIds.Contains(item.TypeID))
            {
                continue;
            }

            TryAttachSharedCategory(item, sharedTag);
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

        var changed = TryAttachSharedCategory(prefab, sharedTag);

        var metaDataField = entryType.GetField("_metaData", AllBindings);
        if (metaDataField != null)
        {
            metaDataField.SetValue(entry, new ItemMetaData(prefab));
            changed = true;
        }

        return changed;
    }

    private static bool TryAttachSharedCategory(Item? item, Tag sharedTag)
    {
        if (item == null || item.Tags.Contains(sharedTag))
        {
            return false;
        }

        item.Tags.Add(sharedTag);
        return true;
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

    private static Sprite? TryLoadCraftCategoryIconSprite(string? modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath))
        {
            return null;
        }

        try
        {
            var iconPath = Path.Combine(modPath, "assets", "item-icons", "grass.png");
            return TryLoadSpriteFromPngFile(iconPath, "MCPrerequisite_CraftCategory_Icon");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MCPrerequisite] Failed to load craft category icon: {e.Message}");
            return null;
        }
    }

    private static Sprite? TryLoadSpriteFromPngFile(string path, string textureName)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var bytes = File.ReadAllBytes(path);
        if (bytes.Length == 0)
        {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = textureName,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        if (!texture.LoadImage(bytes, false))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        var rect = new Rect(0f, 0f, texture.width, texture.height);
        var pivot = new Vector2(0.5f, 0.5f);
        var sprite = Sprite.Create(texture, rect, pivot, 100f);
        sprite.name = textureName + "_Sprite";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}