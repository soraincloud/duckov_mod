using Duckov.Buffs;
using UnityEngine;

namespace GoldenApple;

internal static class GoldenAppleBuffRegistry
{
    private const int MaxHealthBuffId = 990021;
    private const int RegenerationBuffId = 990022;
    private const int ArmorBuffId = 990023;

    private const string MaxHealthBuffNameKey = "Buff_GoldenApple_MaxHealth";
    private const string MaxHealthBuffDescKey = "Buff_GoldenApple_MaxHealth_Desc";
    private const string RegenerationBuffNameKey = "Buff_GoldenApple_Regeneration";
    private const string RegenerationBuffDescKey = "Buff_GoldenApple_Regeneration_Desc";
    private const string ArmorBuffNameKey = "Buff_GoldenApple_Armor";
    private const string ArmorBuffDescKey = "Buff_GoldenApple_Armor_Desc";

    private static Buff? _maxHealthBuffPrefab;
    private static Buff? _regenerationBuffPrefab;
    private static Buff? _armorBuffPrefab;

    public static void Initialize(Sprite? icon)
    {
        var resolvedIcon = icon ?? RuntimeIcon.CreateGoldenAppleIcon();

        _maxHealthBuffPrefab ??= CreateBuffPrefab(
            "GoldenApple_MaxHealthBuffPrefab",
            MaxHealthBuffId,
            MaxHealthBuffNameKey,
            MaxHealthBuffDescKey,
            GoldenAppleEffectController.MaxHealthDurationSeconds,
            resolvedIcon);

        _regenerationBuffPrefab ??= CreateBuffPrefab(
            "GoldenApple_RegenerationBuffPrefab",
            RegenerationBuffId,
            RegenerationBuffNameKey,
            RegenerationBuffDescKey,
            GoldenAppleEffectController.RegenerationDurationSeconds,
            resolvedIcon);

        _armorBuffPrefab ??= CreateBuffPrefab(
            "GoldenApple_ArmorBuffPrefab",
            ArmorBuffId,
            ArmorBuffNameKey,
            ArmorBuffDescKey,
            GoldenAppleEffectController.ArmorDurationSeconds,
            resolvedIcon);
    }

    public static void Deinitialize()
    {
        DestroyPrefab(ref _maxHealthBuffPrefab);
        DestroyPrefab(ref _regenerationBuffPrefab);
        DestroyPrefab(ref _armorBuffPrefab);
    }

    public static void ApplyTo(CharacterMainControl character)
    {
        if (character == null)
        {
            return;
        }

        if (_maxHealthBuffPrefab == null || _regenerationBuffPrefab == null || _armorBuffPrefab == null)
        {
            Initialize(null);
        }

        GoldenAppleEffectController.ApplyTo(character);

        if (_maxHealthBuffPrefab != null)
        {
            character.AddBuff(_maxHealthBuffPrefab, character);
        }

        if (_regenerationBuffPrefab != null)
        {
            character.AddBuff(_regenerationBuffPrefab, character);
        }

        if (_armorBuffPrefab != null)
        {
            character.AddBuff(_armorBuffPrefab, character);
        }
    }

    private static Buff CreateBuffPrefab(string objectName, int buffId, string displayNameKey, string descriptionKey, float totalLifeTime, Sprite icon)
    {
        var gameObject = new GameObject(objectName);
        UnityEngine.Object.DontDestroyOnLoad(gameObject);

        var buff = gameObject.AddComponent<Buff>();
        ReflectionUtil.SetPrivateField(buff, "id", buffId);
        ReflectionUtil.SetPrivateField(buff, "maxLayers", 1);
        ReflectionUtil.SetPrivateField(buff, "exclusiveTag", Buff.BuffExclusiveTags.NotExclusive);
        ReflectionUtil.SetPrivateField(buff, "exclusiveTagPriority", 0);
        ReflectionUtil.SetPrivateField(buff, "displayName", displayNameKey);
        ReflectionUtil.SetPrivateField(buff, "description", descriptionKey);
        ReflectionUtil.SetPrivateField(buff, "icon", icon);
        ReflectionUtil.SetPrivateField(buff, "limitedLifeTime", true);
        ReflectionUtil.SetPrivateField(buff, "totalLifeTime", totalLifeTime);
        ReflectionUtil.SetPrivateField(buff, "hide", false);
        ReflectionUtil.SetPrivateField(buff, "currentLayers", 1);

        return buff;
    }

    private static void DestroyPrefab(ref Buff? prefab)
    {
        if (prefab == null)
        {
            return;
        }

        try
        {
            UnityEngine.Object.Destroy(prefab.gameObject);
        }
        catch
        {
            // ignore
        }

        prefab = null;
    }
}