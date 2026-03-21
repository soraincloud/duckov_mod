using ItemStatsSystem;
using UnityEngine;

namespace SplashHealingPotion;

/// <summary>
/// 备用的即时投掷行为，不走技能瞄准，直接按当前朝向扔出治疗药水。
/// </summary>
public class ThrowEnderPearlUsage : UsageBehavior
{
    [SerializeField]
    private float throwSpeed = 14f;

    [SerializeField]
    private float throwUpward = 3.5f;

    [SerializeField]
    private float maxLifeSeconds = 10f;

    public override bool CanBeUsed(Item item, object user)
    {
        return user is CharacterMainControl;
    }

    protected override void OnUse(Item item, object user)
    {
        if (user is not CharacterMainControl character)
        {
            return;
        }

        // 没有有效瞄准时直接取角色朝向，避免投掷速度方向异常。
        var aimSocket = character.CurrentUsingAimSocket;
        var startPos = aimSocket != null ? aimSocket.position : character.transform.position + Vector3.up * 1.2f;

        var direction = character.CurrentAimDirection;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = character.transform.forward;
        }
        direction = direction.normalized;

        var projectile = EnderPearlProjectile.Create(startPos, character, maxLifeSeconds);
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * throwSpeed + Vector3.up * throwUpward;
        }
    }
}
