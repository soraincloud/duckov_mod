using System.Collections;
using System.IO;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace TotemOfUndying;

internal sealed class TotemRescueSystem : MonoBehaviour
{
    private const float InvincibleSeconds = 3f;
    private const float RescueModelScaleInSeconds = 0.72f;
    private const float RescueModelSlowDownSeconds = 1.18f;
    private const float RescueModelScaleOutSeconds = 0.55f;
    private const float RescueModelFloatSpeed = 0.18f;
    private const float RescueModelSpinSpeedDeg = 540f;
    private const float HeadFallbackHeight = 1.8f;
    private const float ParticleLifetimeMin = 0.7f;
    private const float ParticleLifetimeMax = 1.5f;

    private const string RescueModelPrefabName = "TotemOfUndying_PickupModel";
    private static readonly string[] BundleCandidateNames =
    {
        "totemofundying_assets",
        "totemofundying_assets.bundle",
        "totemofundying_assets.unity3d",
        "totem_assets",
        "totem_assets.bundle",
        "totem_assets.unity3d"
    };

    private static TotemRescueSystem? _instance;
    private static string? _modPath;
    private static AssetBundle? _bundle;
    private static Material? _rescueParticleMaterial;

    internal static void Initialize(string? modPath)
    {
        if (_instance != null)
        {
            return;
        }

        _modPath = modPath;

        var go = new GameObject("TotemOfUndying_RescueSystem");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<TotemRescueSystem>();
    }

    internal static void Deinitialize()
    {
        if (_instance == null)
        {
            return;
        }

        Destroy(_instance.gameObject);
        _instance = null;

        if (_bundle != null)
        {
            _bundle.Unload(unloadAllLoadedObjects: false);
            _bundle = null;
        }

        _modPath = null;
    }

    private void OnEnable()
    {
        Health.OnHurt += OnHealthHurt;
    }

    private void OnDisable()
    {
        Health.OnHurt -= OnHealthHurt;
    }

    private static void OnHealthHurt(Health health, DamageInfo damageInfo)
    {
        if (_instance == null || health == null || health.IsDead)
        {
            return;
        }

        if (health.CurrentHealth > 0f)
        {
            return;
        }

        if (!TryFindTotemInTotemSlot(health, out var totemItem))
        {
            return;
        }

        ConsumeTotem(totemItem);

        var healTarget = Mathf.Max(1f, health.MaxHealth * 0.5f);
        var wasInvincible = health.Invincible;
        health.SetHealth(healTarget);
        health.SetInvincible(true);

        _instance.StartCoroutine(DisableInvincibleAfterDelay(health, wasInvincible));

        var character = health.TryGetCharacter();
        var headPos = ResolveHeadPosition(character, health.transform.position + Vector3.up * HeadFallbackHeight);

        SpawnRescueParticles(headPos);
        SpawnRescueModel(headPos, character);
    }

    private static bool TryFindTotemInTotemSlot(Health health, out Item item)
    {
        item = null!;

        var character = health.TryGetCharacter();
        if (character == null || character.CharacterItem == null || character.CharacterItem.Slots == null)
        {
            return false;
        }

        foreach (Slot slot in character.CharacterItem.Slots)
        {
            if (slot == null || slot.Content == null)
            {
                continue;
            }

            if (!IsTotemSlot(slot.Key))
            {
                continue;
            }

            if (slot.Content.TypeID != ModBehaviour.TotemOfUndyingTypeId)
            {
                continue;
            }

            item = slot.Content;
            return true;
        }

        return false;
    }

    private static bool IsTotemSlot(string? slotKey)
    {
        if (string.IsNullOrWhiteSpace(slotKey))
        {
            return false;
        }

        var key = slotKey.ToLowerInvariant();
        return key == "totem" || key.Contains("totem") || key == "soulcube" || key.Contains("soulcube");
    }

    private static void ConsumeTotem(Item totemItem)
    {
        if (totemItem.Stackable)
        {
            totemItem.StackCount--;
            return;
        }

        totemItem.Detach();
        totemItem.DestroyTree();
    }

    private static IEnumerator DisableInvincibleAfterDelay(Health health, bool wasInvincible)
    {
        yield return new WaitForSeconds(InvincibleSeconds);

        if (health == null || wasInvincible)
        {
            yield break;
        }

        health.SetInvincible(false);
    }

    private static void SpawnRescueParticles(Vector3 position)
    {
        SpawnColorBurst(position, new Color(1f, 0.9f, 0.15f, 1f), new Color(1f, 0.98f, 0.35f, 1f), 50);
        SpawnColorBurst(position, new Color(0.32f, 0.94f, 0.24f, 1f), new Color(0.62f, 1f, 0.44f, 1f), 50);
    }

    private static void SpawnColorBurst(Vector3 position, Color colorA, Color colorB, short count)
    {
        var go = new GameObject("TotemOfUndying_RescueFx");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 1.6f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(ParticleLifetimeMin, ParticleLifetimeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.9f, 4.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.17f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 90;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, count)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.45f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(
            new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(colorA, 0f),
                    new GradientColorKey(colorB, 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(0.95f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            });

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        var mat = GetOrCreateRescueParticleMaterial();
        if (mat != null)
        {
            renderer.sharedMaterial = mat;
        }

        ps.Play();
        Destroy(go, 3.5f);
    }

    private static void SpawnRescueModel(Vector3 position, CharacterMainControl? character)
    {
        if (_instance == null)
        {
            return;
        }

        var prefab = TryLoadRescueModelPrefab();
        if (prefab == null)
        {
            ModLog.Warn($"[TotemOfUndying] Rescue model prefab '{RescueModelPrefabName}' not found in loaded bundle.");
            return;
        }

        var go = Instantiate(prefab);
        go.name = "TotemOfUndying_RescueModel";
        go.transform.position = position + Vector3.up * 0.12f;
        go.transform.localScale = Vector3.zero;

        if (character != null)
        {
            SetLayerRecursively(go, character.gameObject.layer);
        }

        var cols = go.GetComponentsInChildren<Collider>(includeInactive: true);
        if (cols != null)
        {
            foreach (var c in cols)
            {
                if (c != null)
                {
                    Destroy(c);
                }
            }
        }

        var rbs = go.GetComponentsInChildren<Rigidbody>(includeInactive: true);
        if (rbs != null)
        {
            foreach (var rb in rbs)
            {
                if (rb != null)
                {
                    Destroy(rb);
                }
            }
        }

        _instance.StartCoroutine(AnimateRescueModel(go));
        ModLog.Info($"[TotemOfUndying] Rescue model spawned at {go.transform.position} using prefab '{prefab.name}'.");
    }

    private static IEnumerator AnimateRescueModel(GameObject model)
    {
        if (model == null)
        {
            yield break;
        }

        var elapsed = 0f;
        var totalDuration = RescueModelScaleInSeconds + RescueModelSlowDownSeconds + RescueModelScaleOutSeconds;
        var basePos = model.transform.position;
        var maxScale = model.transform.localScale == Vector3.zero ? Vector3.one : model.transform.localScale;

        while (model != null && elapsed < RescueModelScaleInSeconds)
        {
            elapsed += Time.deltaTime;

            var tIn = Mathf.Clamp01(elapsed / RescueModelScaleInSeconds);
            var yIn = RescueModelFloatSpeed * elapsed;

            model.transform.position = basePos + Vector3.up * yIn;
            model.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, maxScale, tIn);
            model.transform.Rotate(Vector3.up, RescueModelSpinSpeedDeg * Time.deltaTime, Space.World);

            yield return null;
        }

        while (model != null && elapsed < RescueModelScaleInSeconds + RescueModelSlowDownSeconds)
        {
            elapsed += Time.deltaTime;

            var tSlow = Mathf.Clamp01((elapsed - RescueModelScaleInSeconds) / RescueModelSlowDownSeconds);
            var spinNow = Mathf.Lerp(RescueModelSpinSpeedDeg, 0f, tSlow);
            var yMid = RescueModelFloatSpeed * elapsed;

            model.transform.position = basePos + Vector3.up * yMid;
            model.transform.localScale = maxScale;
            if (spinNow > 0.01f)
            {
                model.transform.Rotate(Vector3.up, spinNow * Time.deltaTime, Space.World);
            }

            yield return null;
        }

        while (model != null && elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            var tOut = Mathf.Clamp01((elapsed - RescueModelScaleInSeconds - RescueModelSlowDownSeconds) / RescueModelScaleOutSeconds);
            var yOut = RescueModelFloatSpeed * elapsed;

            model.transform.position = basePos + Vector3.up * yOut;
            model.transform.localScale = Vector3.LerpUnclamped(maxScale, Vector3.zero, tOut);

            yield return null;
        }

        if (model != null)
        {
            Destroy(model);
        }
    }

    private static GameObject? TryLoadRescueModelPrefab()
    {
        var bundle = TryLoadBundle();
        if (bundle == null)
        {
            ModLog.Warn("[TotemOfUndying] Rescue model bundle not loaded.");
            return null;
        }

        var direct = bundle.LoadAsset<GameObject>(RescueModelPrefabName);
        if (direct != null)
        {
            ModLog.Info($"[TotemOfUndying] Rescue model loaded by direct name: {RescueModelPrefabName}");
            return direct;
        }

        var allAssets = bundle.GetAllAssetNames();
        if (allAssets == null || allAssets.Length == 0)
        {
            ModLog.Warn("[TotemOfUndying] Bundle contains no assets.");
            return null;
        }

        ModLog.Info($"[TotemOfUndying] Bundle assets ({allAssets.Length}):\n- {string.Join("\n- ", allAssets)}");

        for (var i = 0; i < allAssets.Length; i++)
        {
            var assetName = allAssets[i];
            if (string.IsNullOrWhiteSpace(assetName))
            {
                continue;
            }

            var fileName = Path.GetFileNameWithoutExtension(assetName);
            if (!string.Equals(fileName, RescueModelPrefabName, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var byPath = bundle.LoadAsset<GameObject>(assetName);
            if (byPath != null)
            {
                ModLog.Info($"[TotemOfUndying] Rescue model matched by asset path: {assetName}");
                return byPath;
            }
        }

        return null;
    }

    private static AssetBundle? TryLoadBundle()
    {
        if (_bundle != null)
        {
            return _bundle;
        }

        if (string.IsNullOrWhiteSpace(_modPath))
        {
            return null;
        }

        var candidateBaseDirs = new[]
        {
            Path.Combine(_modPath, "assets", "bundles", "models"),
            Path.Combine(_modPath, "assets", "bundles"),
            _modPath
        };

        for (var i = 0; i < candidateBaseDirs.Length; i++)
        {
            var baseDir = candidateBaseDirs[i];
            for (var j = 0; j < BundleCandidateNames.Length; j++)
            {
                var bundlePath = Path.Combine(baseDir, BundleCandidateNames[j]);
                if (!File.Exists(bundlePath))
                {
                    continue;
                }

                try
                {
                    _bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (_bundle != null)
                    {
                        ModLog.Info($"[TotemOfUndying] Loaded rescue model bundle: {bundlePath}");
                        return _bundle;
                    }
                }
                catch (System.Exception e)
                {
                    ModLog.Warn($"[TotemOfUndying] Failed to load bundle '{bundlePath}': {e.Message}");
                }
            }
        }

        return null;
    }

    private static Material? GetOrCreateRescueParticleMaterial()
    {
        if (_rescueParticleMaterial != null)
        {
            return _rescueParticleMaterial;
        }

        Shader? shader = null;
        try
        {
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Unlit/Color");
        }
        catch
        {
            // ignore
        }

        if (shader == null)
        {
            return null;
        }

        try
        {
            _rescueParticleMaterial = new Material(shader)
            {
                name = "TotemOfUndying_RescueParticleMat"
            };
            return _rescueParticleMaterial;
        }
        catch
        {
            return null;
        }
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null)
        {
            return;
        }

        root.layer = layer;
        var t = root.transform;
        for (var i = 0; i < t.childCount; i++)
        {
            var c = t.GetChild(i);
            if (c != null)
            {
                SetLayerRecursively(c.gameObject, layer);
            }
        }
    }

    private static Vector3 ResolveHeadPosition(CharacterMainControl? character, Vector3 fallback)
    {
        if (character == null)
        {
            return fallback;
        }

        try
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                if (headBone != null)
                {
                    return headBone.position;
                }
            }
        }
        catch
        {
            // ignore
        }

        var root = character.transform;
        if (root == null)
        {
            return fallback;
        }

        return root.position + Vector3.up * HeadFallbackHeight;
    }
}
