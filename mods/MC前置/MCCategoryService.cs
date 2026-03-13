using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duckov.Utilities;
using ItemStatsSystem;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MC前置;

public static class MCCategoryService
{
    public const string SharedCategoryTagName = "ModWorkbench_Mystic";
    public const string SharedCategoryDisplayNameKey = "CraftFilter_ModMystic";

    private static bool _initialized;
    private static string? _modPath;
    private static Tag? _sharedCategoryTag;

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
        PlayerStorage.OnLoadingFinished += OnPlayerStorageLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (item == null || item.Tags.Contains(SharedCategoryTagName))
        {
            return;
        }

        item.Tags.Add(GetOrCreateSharedCategoryTag(SharedCategoryTagName));
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        try
        {
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
            Debug.LogWarning($"[MC前置] Failed to register craft category filter: {e.Message}");
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
            Debug.LogWarning($"[MC前置] Failed to register storage category filter: {e.Message}");
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
            return TryLoadSpriteFromPngFile(iconPath, "MC前置_CraftCategory_Icon");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MC前置] Failed to load craft category icon: {e.Message}");
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