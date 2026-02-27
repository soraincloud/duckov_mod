using UnityEngine;

namespace EnderPearl;

public class EnderPearlProjectile : MonoBehaviour
{
    private CharacterMainControl? _owner;
    private Rigidbody? _rb;
    private Collider? _col;
    private bool _teleported;
    private float _maxLifeSeconds;

    private static Material? _teleportParticleMaterial;

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
        // Simple, self-contained purple particle burst (Enderman-ish).
        // Darker purple target (approx): #ca6de1
        var brightPurple = new Color(202f / 255f, 109f / 255f, 225f / 255f, 1f);
        var brightPurpleEnd = new Color(180f / 255f, 85f / 255f, 210f / 255f, 1f);
        // Subtle glow: HDR intensity (> 1) but keep the tone darker.
        var glowStart = Color.Lerp(brightPurple, Color.white, 0.06f) * 1.8f;
        glowStart.a = 1f;
        var glowEnd = Color.Lerp(brightPurpleEnd, Color.white, 0.04f) * 1.45f;
        glowEnd.a = 1f;
        var go = new GameObject("EnderPearl_TeleportFX");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.duration = 2.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startColor = glowStart;
        main.gravityModifier = 0.0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 80;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 55)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        // Slow drift with slight upward float.
        vel.radial = new ParticleSystem.MinMaxCurve(0.12f, 0.45f);
        vel.y = new ParticleSystem.MinMaxCurve(0.10f, 0.35f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.strength = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        noise.frequency = 0.35f;
        noise.damping = true;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(
            new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(brightPurple, 0f),
                    new GradientColorKey(brightPurpleEnd, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.85f, 0.7f),
                    new GradientAlphaKey(0.0f, 1f)
                }
            }
        );

        // Also push a HDR gradient into the particle color stream (works best with additive shaders).
        col.color = new ParticleSystem.MinMaxGradient(
            new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(glowStart, 0f),
                    new GradientColorKey(glowEnd, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0.85f, 0.7f),
                    new GradientAlphaKey(0.0f, 1f)
                }
            }
        );

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 1f;

        _teleportParticleMaterial ??= TryCreateTeleportParticleMaterial();
        if (_teleportParticleMaterial != null)
        {
            renderer.sharedMaterial = _teleportParticleMaterial;

            // Try to brighten material tint as well (shader-dependent).
            try
            {
                if (renderer.sharedMaterial.HasProperty("_TintColor"))
                {
                    renderer.sharedMaterial.SetColor("_TintColor", glowStart);
                }
                else if (renderer.sharedMaterial.HasProperty("_Color"))
                {
                    renderer.sharedMaterial.SetColor("_Color", glowStart);
                }
            }
            catch
            {
                // ignore
            }
        }

        ps.Play(true);
        Destroy(go, 6.0f);
    }

    private static Material? TryCreateTeleportParticleMaterial()
    {
        // Prefer unlit particle shaders so the purple stays vivid under different lighting.
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
        return new Material(shader);
    }
}
