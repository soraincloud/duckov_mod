using UnityEngine;
using UnityEngine.Rendering;

namespace EnderPearl;

public class EnderPearlProjectile : MonoBehaviour
{
    private CharacterMainControl? _owner;
    private Rigidbody? _rb;
    private Collider? _col;
    private bool _teleported;
    private float _maxLifeSeconds;

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

        // Prefer bundle model for flight; fallback to colored sphere if not available.
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
        if (_owner == null || _col == null)
        {
            yield break;
        }

        var ownerCol = _owner.GetComponent<Collider>();
        if (ownerCol == null)
        {
            yield break;
        }

        Physics.IgnoreCollision(ownerCol, _col, true);
        yield return new WaitForSeconds(seconds);
        if (ownerCol != null && _col != null)
        {
            Physics.IgnoreCollision(ownerCol, _col, false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_teleported)
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

        // 尽量把落点贴到地面上（不依赖游戏层配置，使用默认层掩码）
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
        main.maxParticles = 64;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 22)
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

            // Recycle when finished.
            if (!_ps.IsAlive(withChildren: true))
            {
                gameObject.SetActive(false);
                _teleportFxPool.Enqueue(_ps);
            }
        }
    }

    private static Material? TryCreateTeleportParticleMaterial()
    {
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
