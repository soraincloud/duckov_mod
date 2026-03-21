using ItemStatsSystem;
using ItemStatsSystem.Stats;
using UnityEngine;

namespace GoldenApple;

/// <summary>
/// 挂在角色身上的实际效果控制器，负责刷新持续时间、应用属性修饰器和定时回血。
/// </summary>
public class GoldenAppleEffectController : MonoBehaviour
{
    private const float BonusMaxHealth = 30f;
    private const float BonusArmor = 1.5f;
    private const float RegenerationPerTick = 15f;
    private const float MaxHealthDuration = 120f;
    private const float ArmorDuration = 300f;
    private const float RegenerationDuration = 30f;
    private const float RegenerationInterval = 1f;

    public const float MaxHealthDurationSeconds = MaxHealthDuration;
    public const float ArmorDurationSeconds = ArmorDuration;
    public const float RegenerationDurationSeconds = RegenerationDuration;

    private CharacterMainControl? _character;
    private Modifier? _maxHealthModifier;
    private Modifier? _headArmorModifier;
    private Modifier? _bodyArmorModifier;
    private Stat? _maxHealthStat;
    private Stat? _headArmorStat;
    private Stat? _bodyArmorStat;
    private float _maxHealthExpireTime;
    private float _armorExpireTime;
    private float _regenerationExpireTime;
    private float _nextRegenerationTime;

    public static void ApplyTo(CharacterMainControl character)
    {
        if (character == null)
        {
            return;
        }

        var controller = character.GetComponent<GoldenAppleEffectController>();
        if (controller == null)
        {
            controller = character.gameObject.AddComponent<GoldenAppleEffectController>();
        }

        controller.ApplyEffect(character);
    }

    private void ApplyEffect(CharacterMainControl character)
    {
        _character = character;
        CacheStats();

        // 重复食用时直接刷新三个效果的到期时间，而不是叠加多个控制器。
        var now = Time.time;
        _maxHealthExpireTime = now + MaxHealthDuration;
        _armorExpireTime = now + ArmorDuration;
        _regenerationExpireTime = now + RegenerationDuration;
        _nextRegenerationTime = now + RegenerationInterval;

        EnsureMaxHealthModifierActive();
        EnsureArmorModifiersActive();
    }

    private void Update()
    {
        if (_character == null || _character.CharacterItem == null)
        {
            CleanupAll();
            Destroy(this);
            return;
        }

        CacheStats();

        var now = Time.time;

        // 每帧都重新确认修饰器是否仍挂在角色属性上，防止外部系统移除后增益提前失效。
        if (now < _maxHealthExpireTime)
        {
            EnsureMaxHealthModifierActive();
        }
        else
        {
            RemoveMaxHealthModifier();
        }

        if (now < _armorExpireTime)
        {
            EnsureArmorModifiersActive();
        }
        else
        {
            RemoveArmorModifiers();
        }

        if (now < _regenerationExpireTime)
        {
            while (now >= _nextRegenerationTime)
            {
                _character.AddHealth(RegenerationPerTick);
                _nextRegenerationTime += RegenerationInterval;
            }
        }

        if (now >= _maxHealthExpireTime && now >= _armorExpireTime && now >= _regenerationExpireTime)
        {
            CleanupAll();
            Destroy(this);
        }
    }

    private void CacheStats()
    {
        // Stat 和 Modifier 只缓存一次，后续重复食用只刷新计时，避免频繁分配对象。
        var characterItem = _character?.CharacterItem;
        if (characterItem == null)
        {
            return;
        }

        _maxHealthStat ??= characterItem.GetStat("MaxHealth");
        _headArmorStat ??= characterItem.GetStat("HeadArmor");
        _bodyArmorStat ??= characterItem.GetStat("BodyArmor");

        _maxHealthModifier ??= new Modifier(ModifierType.Add, BonusMaxHealth, this);
        _headArmorModifier ??= new Modifier(ModifierType.Add, BonusArmor, this);
        _bodyArmorModifier ??= new Modifier(ModifierType.Add, BonusArmor, this);
    }

    private void EnsureMaxHealthModifierActive()
    {
        if (_maxHealthStat == null || _maxHealthModifier == null)
        {
            return;
        }

        _maxHealthStat.RemoveModifier(_maxHealthModifier);
        _maxHealthStat.AddModifier(_maxHealthModifier);
    }

    private void EnsureArmorModifiersActive()
    {
        if (_headArmorStat != null && _headArmorModifier != null)
        {
            _headArmorStat.RemoveModifier(_headArmorModifier);
            _headArmorStat.AddModifier(_headArmorModifier);
        }

        if (_bodyArmorStat != null && _bodyArmorModifier != null)
        {
            _bodyArmorStat.RemoveModifier(_bodyArmorModifier);
            _bodyArmorStat.AddModifier(_bodyArmorModifier);
        }
    }

    private void RemoveMaxHealthModifier()
    {
        if (_maxHealthStat != null && _maxHealthModifier != null)
        {
            _maxHealthStat.RemoveModifier(_maxHealthModifier);
        }

        // 生命上限回退后要把当前血量钳回新的 MaxHealth，避免出现超出上限的残留数值。
        if (_character?.Health != null)
        {
            _character.Health.CurrentHealth = Mathf.Min(_character.Health.CurrentHealth, _character.Health.MaxHealth);
        }
    }

    private void RemoveArmorModifiers()
    {
        if (_headArmorStat != null && _headArmorModifier != null)
        {
            _headArmorStat.RemoveModifier(_headArmorModifier);
        }

        if (_bodyArmorStat != null && _bodyArmorModifier != null)
        {
            _bodyArmorStat.RemoveModifier(_bodyArmorModifier);
        }
    }

    private void CleanupAll()
    {
        RemoveMaxHealthModifier();
        RemoveArmorModifiers();
    }

    private void OnDestroy()
    {
        CleanupAll();
    }
}