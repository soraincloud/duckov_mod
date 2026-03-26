using Duckov.Buffs;
using UnityEngine;

namespace SplashSwiftnessPotion;

internal static class SplashSwiftnessPotionBuffRegistry
{
    private const int SpeedBuffId = 990024;
    private const string SpeedBuffNameKey = "Buff_SplashSwiftnessPotion_Speed";
    private const string SpeedBuffDescKey = "Buff_SplashSwiftnessPotion_Speed_Desc";

    private static Buff? _speedBuffPrefab;

    public static void Initialize(Sprite? icon)
    {
        var resolvedIcon = icon ?? RuntimeIcon.CreateSplashSwiftnessPotionIcon();
        _speedBuffPrefab ??= CreateBuffPrefab(
            "SplashSwiftnessPotion_SpeedBuffPrefab",
            SpeedBuffId,
            SpeedBuffNameKey,
            SpeedBuffDescKey,
            SplashSwiftnessPotionEffectController.DurationSeconds,
            resolvedIcon);
    }

    public static void Deinitialize()
    {
        if (_speedBuffPrefab == null)
        {
            return;
        }

        try
        {
            UnityEngine.Object.Destroy(_speedBuffPrefab.gameObject);
        }
        catch
        {
            // ignore
        }

        _speedBuffPrefab = null;
    }

    public static void ApplyTo(CharacterMainControl character)
    {
        if (character == null)
        {
            return;
        }

        if (_speedBuffPrefab == null)
        {
            Initialize(null);
        }

        SplashSwiftnessPotionEffectController.ApplyTo(character);
        if (_speedBuffPrefab != null)
        {
            character.AddBuff(_speedBuffPrefab, character);
        }
    }

    private static Buff CreateBuffPrefab(string objectName, int buffId, string displayNameKey, string descriptionKey, float totalLifeTime, Sprite icon)
    {
        var go = new GameObject(objectName);
        UnityEngine.Object.DontDestroyOnLoad(go);

        var buff = go.AddComponent<Buff>();
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
}