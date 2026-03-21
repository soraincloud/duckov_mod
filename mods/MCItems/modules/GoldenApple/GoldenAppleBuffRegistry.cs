using Duckov.Buffs;
using UnityEngine;

namespace GoldenApple;

/// <summary>
/// 维护附魔金苹果三个 Buff 的预制体，并在食用时统一发放给角色。
/// </summary>
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
        // Buff prefab 会跨场景复用，所以只在首次初始化时创建一次。
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

        // 先确保控制器在角色身上刷新计时，再让 UI Buff 栏出现对应条目。
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
        // 这里构造的是纯展示 Buff，实际数值增益由 GoldenAppleEffectController 负责施加。
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