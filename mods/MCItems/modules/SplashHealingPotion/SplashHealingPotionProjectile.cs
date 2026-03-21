using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SplashHealingPotion;

/// <summary>
/// 治疗药水抛体本体，负责碰撞爆开、范围治疗和飞溅粒子复用。
/// </summary>
public class EnderPearlProjectile : MonoBehaviour
{
    private const float HealPercent = 0.5f;
    private const float HealRadius = 2.8f;
    private const float HealArmorPiercing = 99f;
    private const float ArmDelaySeconds = 0.12f;
    private const int SplashFxPoolMax = 6;

    private static readonly Queue<ParticleSystem> SplashFxPool = new();
    private static Material? _splashParticleMaterial;
    private static int _splashFxCreated;

    private CharacterMainControl? _owner;
    private Collider? _col;
    private bool _resolved;
    private float _maxLifeSeconds;
    private float _spawnTime;

    public static GameObject Create(Vector3 startPos, CharacterMainControl owner, float maxLifeSeconds)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "SplashHealingPotion_Projectile";
        go.transform.position = startPos;
        go.transform.localScale = Vector3.one;

        var sphereCol = go.GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            sphereCol.radius = 0.11f;
        }

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.2f;
        rb.drag = 0.05f;
        rb.angularDrag = 0.05f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.maxAngularVelocity = 50f;
        rb.angularVelocity = Random.onUnitSphere * 20f;

        var proj = go.AddComponent<EnderPearlProjectile>();
        proj._owner = owner;
        proj._col = go.GetComponent<Collider>();
        proj._maxLifeSeconds = Mathf.Max(0.5f, maxLifeSeconds);
        proj._spawnTime = Time.time;

    // 优先挂载 bundle 模型；缺失资源时回退到简单球体以保证逻辑仍可测试。
        var attached = ModAssets.TryAttachModelToProjectile(go);
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (attached)
            {
                renderer.enabled = false;
            }
            else
            {
                renderer.material.color = new Color(0.92f, 0.35f, 0.62f, 1f);
            }
        }

        proj.StartCoroutine(proj.IgnoreOwnerCollisionForSeconds(0.35f));
        proj.StartCoroutine(proj.DestroyAfterSeconds());

        return go;
    }

    private IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(_maxLifeSeconds);
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator IgnoreOwnerCollisionForSeconds(float seconds)
    {
        if (_owner == null)
        {
            yield break;
        }

        var ownerCols = _owner.GetComponentsInChildren<Collider>(includeInactive: true);
        if (ownerCols == null || ownerCols.Length == 0)
        {
            yield break;
        }

        var projectileCols = GetComponentsInChildren<Collider>(includeInactive: true);
        if (projectileCols == null || projectileCols.Length == 0)
        {
            if (_col == null)
            {
                yield break;
            }

            projectileCols = new[] { _col };
        }

        foreach (var ownerCol in ownerCols)
        {
            if (ownerCol == null)
            {
                continue;
            }

            foreach (var projectileCol in projectileCols)
            {
                if (projectileCol == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(ownerCol, projectileCol, true);
            }
        }

        yield return new WaitForSeconds(seconds);

        foreach (var ownerCol in ownerCols)
        {
            if (ownerCol == null)
            {
                continue;
            }

            foreach (var projectileCol in projectileCols)
            {
                if (projectileCol == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(ownerCol, projectileCol, false);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_resolved)
        {
            return;
        }

        if (_owner != null)
        {
            var other = collision.collider != null ? collision.collider.transform : collision.transform;
            if (other != null && other.IsChildOf(_owner.transform))
            {
                return;
            }
        }

        // 起爆延迟用于忽略刚出手时与角色或近地面的无效碰撞。
        if (Time.time - _spawnTime < ArmDelaySeconds)
        {
            return;
        }

        _resolved = true;

        Vector3 point;
        if (collision.contactCount > 0)
        {
            point = collision.GetContact(0).point;
        }
        else
        {
            point = transform.position;
        }

        if (Physics.Raycast(point + Vector3.up, Vector3.down, out var hit, 3f, Physics.DefaultRaycastLayers))
        {
            point = hit.point;
        }

        ModSfx.PlayGlassBreak(point);
        SpawnSplashParticles(point + Vector3.up * 0.08f);
        // 游戏现有 Hurt 流程已经处理了大部分受击联动，因此这里用“负伤害”统一走回血逻辑。
        ApplyNegativeDamageHealingInRange(point);
        Destroy(gameObject);
    }

    private void ApplyNegativeDamageHealingInRange(Vector3 point)
    {
        // 直接遍历场景里的 Health，既能治疗玩家，也能治疗同范围内的其他有效目标。
        var healths = Object.FindObjectsOfType<Health>();
        if (healths == null || healths.Length == 0)
        {
            return;
        }

        foreach (var health in healths)
        {
            if (health == null || health.IsDead || health.CurrentHealth <= 0f || health.MaxHealth <= 0f)
            {
                continue;
            }

            var character = health.TryGetCharacter();
            var targetPosition = character != null ? character.transform.position : health.transform.position;
            if (Vector3.Distance(targetPosition, point) > HealRadius)
            {
                continue;
            }

            var isMainCharacter = character == CharacterMainControl.Main;

            // armorPiercing 提高到极高值，确保回血不会被护甲系统再次折损。
            var damageInfo = new DamageInfo(_owner)
            {
                damageValue = -Mathf.Max(1f, health.MaxHealth * HealPercent),
                armorPiercing = HealArmorPiercing,
                isExplosion = true,
                fromWeaponItemID = ModBehaviour.EnderPearlTypeId,
                damagePoint = targetPosition,
                damageNormal = (targetPosition - point).sqrMagnitude > 0.0001f
                    ? (targetPosition - point).normalized
                    : Vector3.up
            };

            health.Hurt(damageInfo);
            health.SetHealth(Mathf.Min(health.CurrentHealth, health.MaxHealth));

            if (isMainCharacter)
            {
                HealFlashFeedback.Trigger();
            }
        }
    }

    private static void SpawnSplashParticles(Vector3 position)
    {
        var colorStart = new Color(222f / 255f, 82f / 255f, 136f / 255f, 1f);
        var colorEnd = new Color(1f, 228f / 255f, 238f / 255f, 1f);

        var ps = GetOrCreateSplashFx();
        if (ps == null)
        {
            return;
        }

        ps.transform.position = position;

        var main = ps.main;
        main.startColor = colorStart;

        var colorOverLifetime = ps.colorOverLifetime;
        if (colorOverLifetime.enabled)
        {
            colorOverLifetime.color = CreateSplashGradient(colorStart, colorEnd);
        }

        ps.Clear(withChildren: true);
        ps.Play(withChildren: true);
    }

    private static ParticleSystem? GetOrCreateSplashFx()
    {
        if (SplashFxPool.Count > 0)
        {
            var pooled = SplashFxPool.Dequeue();
            if (pooled != null)
            {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }

        if (_splashFxCreated >= SplashFxPoolMax)
        {
            // 粒子池打满后直接跳过新特效，优先保证战斗时不卡顿。
            return null;
        }

        _splashFxCreated++;

        var go = new GameObject("SplashHealingPotion_SplashFX");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = false;
        main.duration = 1.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.15f, 1.95f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.2f);
        main.gravityModifier = 0.03f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 64;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 36)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.16f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.radial = new ParticleSystem.MinMaxCurve(0.95f, 1.65f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.45f));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = CreateSplashGradient(Color.white, Color.white);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 1f;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        _splashParticleMaterial ??= TryCreateSplashParticleMaterial();
        if (_splashParticleMaterial != null)
        {
            renderer.sharedMaterial = _splashParticleMaterial;
        }

        var recycle = go.AddComponent<SplashFxRecycle>();
        recycle.SetParticleSystem(ps);

        return ps;
    }

    private static ParticleSystem.MinMaxGradient CreateSplashGradient(Color start, Color end)
    {
        return new ParticleSystem.MinMaxGradient(
            new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(end, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            }
        );
    }

    private static Material? TryCreateSplashParticleMaterial()
    {
        var tint = new Color(238f / 255f, 112f / 255f, 156f / 255f, 1f);
        var shader = Shader.Find("Particles/Alpha Blended")
            ?? Shader.Find("Legacy Shaders/Particles/Alpha Blended")
            ?? Shader.Find("Particles/Standard Unlit")
            ?? Shader.Find("Particles/Additive");
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader)
        {
            name = "SplashHealingPotion_ParticleMat"
        };

        if (material.HasProperty("_Color")) material.SetColor("_Color", tint);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
        if (material.HasProperty("_TintColor")) material.SetColor("_TintColor", tint);

        return material;
    }

    private sealed class SplashFxRecycle : MonoBehaviour
    {
        private ParticleSystem? _ps;

        internal void SetParticleSystem(ParticleSystem ps)
        {
            _ps = ps;
        }

        private void Update()
        {
            if (_ps == null)
            {
                return;
            }

            if (!_ps.IsAlive(withChildren: true))
            {
                gameObject.SetActive(false);
                SplashFxPool.Enqueue(_ps);
            }
        }
    }
}
