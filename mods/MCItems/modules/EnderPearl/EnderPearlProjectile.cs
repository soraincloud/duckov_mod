using UnityEngine;
using UnityEngine.Rendering;

namespace EnderPearl;

/// <summary>
/// 末影珍珠抛体本体，负责飞行、碰撞判定、传送落点修正和特效池复用。
/// </summary>
public class EnderPearlProjectile : MonoBehaviour
{
    private CharacterMainControl? _owner;
    private Rigidbody? _rb;
    private Collider? _col;
    private bool _teleported;
    private float _maxLifeSeconds;
    private float _spawnTime;

    private const float ArmDelaySeconds = 0.12f;

    private static Material? _teleportParticleMaterial;
    private static readonly System.Collections.Generic.Queue<ParticleSystem> _teleportFxPool = new();
    private static int _teleportFxCreated;
    private const int TeleportFxPoolMax = 6;

    public static GameObject Create(Vector3 startPos, CharacterMainControl owner, float maxLifeSeconds)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "EnderPearl_Projectile";
        go.transform.position = startPos;
        // Keep root scale at 1 so attached model scaling is predictable.
        go.transform.localScale = Vector3.one;

        // Ensure collider size stays reasonable.
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
        // Give it a visible spin so the flying model doesn't look static.
        rb.angularVelocity = Random.onUnitSphere * 20f;

        var proj = go.AddComponent<EnderPearlProjectile>();
        proj._owner = owner;
        proj._rb = rb;
        proj._col = go.GetComponent<Collider>();
        proj._maxLifeSeconds = Mathf.Max(0.5f, maxLifeSeconds);
        proj._spawnTime = Time.time;

        // 飞行阶段优先使用 bundle 模型；没有模型时才回退到简单球体，方便调试和容错。
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
                // 简单上色（避免纯白球太突兀）
                renderer.material.color = new Color(0.55f, 0.2f, 0.9f, 1f);
            }
        }

        proj.StartCoroutine(proj.IgnoreOwnerCollisionForSeconds(0.35f));
        proj.StartCoroutine(proj.DestroyAfterSeconds());

        return go;
    }

    private System.Collections.IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(_maxLifeSeconds);
        if (this != null && gameObject != null) Destroy(gameObject);
    }

    private System.Collections.IEnumerator IgnoreOwnerCollisionForSeconds(float seconds)
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

        // 刚生成时先临时忽略持有者碰撞，避免抛体出生在角色碰撞盒内直接炸开。
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
        if (_teleported)
        {
            return;
        }

        // Ignore owner/self contacts (owner often has multiple child colliders).
        if (_owner != null)
        {
            var other = collision.collider != null ? collision.collider.transform : collision.transform;
            if (other != null && other.IsChildOf(_owner.transform))
            {
                return;
            }
        }

        // 小段起爆延迟用于规避出生点与地面或角色身体重叠造成的瞬移误触发。
        if (Time.time - _spawnTime < ArmDelaySeconds)
        {
            return;
        }

        _teleported = true;

        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }

        var startPos = _owner.transform.position;

        Vector3 point;
        if (collision.contactCount > 0)
        {
            point = collision.GetContact(0).point;
        }
        else
        {
            point = transform.position;
        }

        // 再向地面做一次短射线，把瞬移点压回可站立高度，减少卡进斜坡或道具的概率。
        if (Physics.Raycast(point + Vector3.up * 1.0f, Vector3.down, out var hit, 3.0f, Physics.DefaultRaycastLayers))
        {
            point = hit.point;
        }

        SpawnTeleportParticles(startPos + Vector3.up * 0.1f);
        SpawnTeleportParticles(point + Vector3.up * 0.1f);

        ModSfx.PlayTransmit(point);

        _owner.SetPosition(point + Vector3.up * 0.1f);
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.GameCamera?.ForceSyncPos();
        }

        Destroy(gameObject);
    }

    private static void SpawnTeleportParticles(Vector3 position)
    {
        // NOTE (Windows perf): creating/destroying ParticleSystems on demand can cause big stalls
        // (GC + resource churn + bloom/overdraw). Use a tiny pool and keep the effect simple.
        // Target tone: #ca6de1
        var colorStart = new Color(202f / 255f, 109f / 255f, 225f / 255f, 1f);
        var colorEnd = new Color(180f / 255f, 85f / 255f, 210f / 255f, 1f);

        var ps = GetOrCreateTeleportFx();
        if (ps == null)
        {
            return;
        }

        var t = ps.transform;
        t.position = position;

        // Ensure color matches latest tuning.
        var main = ps.main;
        main.startColor = colorStart;

        var col = ps.colorOverLifetime;
        if (col.enabled)
        {
            col.color = CreateTeleportGradient(colorStart, colorEnd);
        }

        ps.Clear(withChildren: true);
        ps.Play(withChildren: true);
    }

    private static ParticleSystem? GetOrCreateTeleportFx()
    {
        if (_teleportFxPool.Count > 0)
        {
            var pooled = _teleportFxPool.Dequeue();
            if (pooled != null)
            {
                pooled.gameObject.SetActive(true);
                return pooled;
            }
        }

        if (_teleportFxCreated >= TeleportFxPoolMax)
        {
            // 粒子对象上限固定，超出时宁可不播，也不在战斗中继续分配新对象制造卡顿。
            // If pool is exhausted, do nothing rather than creating more and risking spikes.
            return null;
        }

        _teleportFxCreated++;

        var go = new GameObject("EnderPearl_TeleportFX");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop = false;
        main.duration = 1.25f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.55f, 1.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.1f);
        main.gravityModifier = 0.0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 42;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 15)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.28f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.radial = new ParticleSystem.MinMaxCurve(0.12f, 0.35f);

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = CreateTeleportGradient(new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 1f));

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 1f;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        _teleportParticleMaterial ??= TryCreateTeleportParticleMaterial();
        if (_teleportParticleMaterial != null)
        {
            renderer.sharedMaterial = _teleportParticleMaterial;
        }

        var recycle = go.AddComponent<TeleportFxRecycle>();
        recycle.SetParticleSystem(ps);

        return ps;
    }

    private static ParticleSystem.MinMaxGradient CreateTeleportGradient(Color start, Color end)
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
                    new GradientAlphaKey(0.0f, 1f)
                }
            }
        );
    }

    private sealed class TeleportFxRecycle : MonoBehaviour
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

            // 播放结束后回收到池里，下次瞬移直接复用同一粒子系统。
            if (!_ps.IsAlive(withChildren: true))
            {
                gameObject.SetActive(false);
                _teleportFxPool.Enqueue(_ps);
            }
        }
    }

    private static Material? TryCreateTeleportParticleMaterial()
    {
        // 优先找不受场景光照影响的粒子 shader，保证紫色瞬移效果在不同地图下都足够稳定。
        // Prefer unlit particle shaders so the purple stays vivid under different lighting.
        // Prefer additive for a "glow" look, but keep the effect cheap (low particle count + pooling).
        // Avoid HDR intensity > 1 elsewhere to reduce bloom-related spikes on some Windows setups.
        var shader = Shader.Find("Particles/Additive");
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Particles/Additive");
        }
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
        }
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Surface");
        }
        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);

        // Use a mild tint boost for glow-like look while avoiding aggressive HDR bloom cost.
        var glowColor = new Color(202f / 255f, 109f / 255f, 225f / 255f, 1f) * 1.2f;
        if (material.HasProperty("_TintColor"))
        {
            material.SetColor("_TintColor", glowColor);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", glowColor);
        }

        return material;
    }
}
