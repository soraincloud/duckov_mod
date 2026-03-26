using ItemStatsSystem;
using ItemStatsSystem.Stats;
using UnityEngine;

namespace SplashSwiftnessPotion;

public class SplashSwiftnessPotionEffectController : MonoBehaviour
{
    private const float SpeedBonus = 0.30f;
    private const float Duration = 180f;

    public const float DurationSeconds = Duration;

    private CharacterMainControl? _character;
    private Stat? _walkSpeedStat;
    private Stat? _runSpeedStat;
    private Modifier? _walkSpeedModifier;
    private Modifier? _runSpeedModifier;
    private float _expireTime;

    public static void ApplyTo(CharacterMainControl character)
    {
        if (character == null)
        {
            return;
        }

        var controller = character.GetComponent<SplashSwiftnessPotionEffectController>();
        if (controller == null)
        {
            controller = character.gameObject.AddComponent<SplashSwiftnessPotionEffectController>();
        }

        controller.ApplyEffect(character);
    }

    private void ApplyEffect(CharacterMainControl character)
    {
        _character = character;
        CacheStats();
        _expireTime = Time.time + Duration;
        EnsureModifiersActive();
    }

    private void Update()
    {
        if (_character == null || _character.CharacterItem == null)
        {
            Cleanup();
            Destroy(this);
            return;
        }

        CacheStats();
        if (Time.time < _expireTime)
        {
            EnsureModifiersActive();
            return;
        }

        Cleanup();
        Destroy(this);
    }

    private void CacheStats()
    {
        var characterItem = _character?.CharacterItem;
        if (characterItem == null)
        {
            return;
        }

        _walkSpeedStat ??= characterItem.GetStat("WalkSpeed");
        _runSpeedStat ??= characterItem.GetStat("RunSpeed");

        _walkSpeedModifier ??= new Modifier(ModifierType.PercentageAdd, SpeedBonus, this);
        _runSpeedModifier ??= new Modifier(ModifierType.PercentageAdd, SpeedBonus, this);
    }

    private void EnsureModifiersActive()
    {
        if (_walkSpeedStat != null && _walkSpeedModifier != null)
        {
            _walkSpeedStat.RemoveModifier(_walkSpeedModifier);
            _walkSpeedStat.AddModifier(_walkSpeedModifier);
        }

        if (_runSpeedStat != null && _runSpeedModifier != null)
        {
            _runSpeedStat.RemoveModifier(_runSpeedModifier);
            _runSpeedStat.AddModifier(_runSpeedModifier);
        }
    }

    private void Cleanup()
    {
        if (_walkSpeedStat != null && _walkSpeedModifier != null)
        {
            _walkSpeedStat.RemoveModifier(_walkSpeedModifier);
        }

        if (_runSpeedStat != null && _runSpeedModifier != null)
        {
            _runSpeedStat.RemoveModifier(_runSpeedModifier);
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}