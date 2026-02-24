using UnityEngine;

namespace EnderPearl;

public class Skill_EnderPearlThrow : SkillBase
{
    [SerializeField]
    private float maxLifeSeconds = 10f;

    [SerializeField]
    private bool canControlCastDistance = true;

    private void Awake()
    {
        // 配成“手雷型技能”，这样 SkillHud3D 会显示抛物线
        skillContext = new SkillContext
        {
            castRange = 14f,
            movableWhileAim = true,
            skillReadyTime = 0.05f,
            effectRange = 1f,
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

        var go = EnderPearlProjectile.Create(startPos, fromCharacter, maxLifeSeconds);
        var rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = velocity;
        }
    }

    private static Vector3 CalculateVelocity(Vector3 start, Vector3 target, float verticleSpeed)
    {
        float g = Physics.gravity.magnitude;
        if (g <= 0f)
        {
            g = 1f;
        }

        float tUp = verticleSpeed / g;
        float tDown = Mathf.Sqrt(2f * Mathf.Abs(tUp * verticleSpeed * 0.5f + start.y - target.y) / g);
        float totalTime = tUp + tDown;
        if (totalTime <= 0f)
        {
            totalTime = 0.001f;
        }

        Vector3 s = start;
        s.y = 0f;
        Vector3 e = target;
        e.y = 0f;

        Vector3 planar = e - s;
        Vector3 planarDir = planar.sqrMagnitude > 0.0001f ? planar.normalized : Vector3.zero;
        float planarDist = planar.magnitude;

        float planarSpeed = planarDist / totalTime;
        return planarDir * planarSpeed + Vector3.up * verticleSpeed;
    }
}
