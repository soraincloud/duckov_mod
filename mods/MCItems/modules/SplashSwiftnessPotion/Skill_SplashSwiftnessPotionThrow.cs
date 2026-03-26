using ItemStatsSystem;
using UnityEngine;

namespace SplashSwiftnessPotion;

public class Skill_SplashSwiftnessPotionThrow : SkillBase
{
    [SerializeField]
    private float maxLifeSeconds = 10f;

    [SerializeField]
    private bool canControlCastDistance = true;

    private void Awake()
    {
        skillContext = new SkillContext
        {
            castRange = 14f,
            movableWhileAim = true,
            skillReadyTime = 0.05f,
            effectRange = SplashSwiftnessPotionProjectile.BuffRadius,
            isGrenade = true,
            grenageVerticleSpeed = 8.5f,
            checkObsticle = false,
            releaseOnStartAim = false
        };

        coolDownTime = 0.1f;
        staminaCost = 0f;
    }

    public override void OnRelease()
    {
        if (fromCharacter == null)
        {
            return;
        }

        var aimSocket = fromCharacter.CurrentUsingAimSocket;
        var startPos = aimSocket != null ? aimSocket.position : fromCharacter.transform.position + Vector3.up * 1.2f;

        var releasePoint = skillReleaseContext.releasePoint;
        float targetY = releasePoint.y;

        Vector3 dir = releasePoint - fromCharacter.transform.position;
        dir.y = 0f;
        float distance = dir.magnitude;

        if (!canControlCastDistance)
        {
            distance = skillContext.castRange;
        }

        if (distance > skillContext.castRange)
        {
            distance = skillContext.castRange;
        }

        if (distance < 0.01f)
        {
            dir = fromCharacter.CurrentAimDirection;
            dir.y = 0f;
        }

        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector3.forward;

        Vector3 target = startPos + dir * distance;
        target.y = targetY;

        Vector3 velocity = CalculateVelocity(startPos, target, skillContext.grenageVerticleSpeed);
        var go = SplashSwiftnessPotionProjectile.Create(startPos, fromCharacter, maxLifeSeconds);
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = velocity;
        }

        try
        {
            var isBase = LevelManager.Instance != null && LevelManager.Instance.IsBaseLevel;
            if (!isBase && fromItem != null)
            {
                if (fromItem.Stackable)
                {
                    fromItem.StackCount--;
                }
                else
                {
                    fromItem.Detach();
                    fromItem.DestroyTree();
                }
            }
        }
        catch
        {
            // allow the throw even if consumption fails
        }

        ModSfx.PlayThrow(startPos);
    }

    private static Vector3 CalculateVelocity(Vector3 start, Vector3 target, float verticleSpeed)
    {
        float gravity = Physics.gravity.magnitude;
        if (gravity <= 0f)
        {
            gravity = 1f;
        }

        float tUp = verticleSpeed / gravity;
        float tDown = Mathf.Sqrt(2f * Mathf.Abs(tUp * verticleSpeed * 0.5f + start.y - target.y) / gravity);
        float totalTime = Mathf.Max(0.001f, tUp + tDown);

        Vector3 flatStart = start;
        flatStart.y = 0f;
        Vector3 flatTarget = target;
        flatTarget.y = 0f;

        Vector3 planar = flatTarget - flatStart;
        Vector3 planarDir = planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector3.zero;
        float planarSpeed = planar.magnitude / totalTime;

        return planarDir * planarSpeed + Vector3.up * verticleSpeed;
    }
}