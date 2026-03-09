using System.Collections;
using System.IO;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using UnityEngine;

namespace TotemOfUndying;

internal sealed class TotemRescueSystem : MonoBehaviour
{
    private const float InvincibleSeconds = 5f;
    private const float HealPercent = 0.3f;
    private const float RescueModelScaleInSeconds = 0.72f;
    private const float RescueModelSlowDownSeconds = 1.18f;
    private const float RescueModelScaleOutSeconds = 0.55f;
    private const float RescueModelScaleMultiplier = 2f;
    private const float RescueModelFloatSpeed = 0.18f;
    private const float RescueModelSpinSpeedDeg = 540f;
    private const float RescueModelSpinSpeedMinDeg = 120f;
    private const float RescueEffectHeadOffset = 0.3f;
    private const float HeadFallbackHeight = 1.8f;
    private const float ParticleLifetimeMin = 1.2f;
    private const float ParticleLifetimeMax = 2.4f;

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
    private static Material? _fallbackModelMaterial;

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

        var healTarget = Mathf.Max(1f, health.MaxHealth * HealPercent);
        var wasInvincible = health.Invincible;
        health.SetHealth(healTarget);
        health.SetInvincible(true);

        _instance.StartCoroutine(DisableInvincibleAfterDelay(health, wasInvincible));

        var character = health.TryGetCharacter();
        var headPos = ResolveHeadPosition(character, health.transform.position + Vector3.up * HeadFallbackHeight);
        var effectPos = headPos + Vector3.up * RescueEffectHeadOffset;

        SpawnRescueParticles(effectPos);
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
        SpawnColorBurst(position, new Color(1f, 0.9f, 0.15f, 1f), new Color(1f, 0.98f, 0.35f, 1f), 25);
        SpawnColorBurst(position, new Color(0.32f, 0.94f, 0.24f, 1f), new Color(0.62f, 1f, 0.44f, 1f), 25);
    }

    private static void SpawnColorBurst(Vector3 position, Color colorA, Color colorB, short count)
    {
        var go = new GameObject("TotemOfUndying_RescueFx");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 2.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(ParticleLifetimeMin, ParticleLifetimeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.9f, 4.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.17f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 45;

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
        Destroy(go, 5f);
    }

    private static void SpawnRescueModel(Vector3 position, CharacterMainControl? character)
    {
        if (_instance == null)
        {
            return;
        }

        var prefab = TotemModelAssets.TryLoadPickupModelPrefab(_modPath);
        GameObject go;
        var modelSource = "runtime-fallback";
        if (prefab == null)
        {
            ModLog.Warn($"[TotemOfUndying] Rescue model prefab '{RescueModelPrefabName}' not found in loaded bundle. Using runtime fallback model.");
            go = CreateFallbackRescueModel();
        }
        else
        {
            go = Instantiate(prefab);
            modelSource = prefab.name;
        }

        go.name = "TotemOfUndying_RescueModel";
        go.SetActive(true);
        var headAnchor = ResolveHeadAnchor(character);
        var baseScale = go.transform.localScale == Vector3.zero ? Vector3.one : go.transform.localScale;
        go.transform.position = ResolveRescueEffectPosition(headAnchor, position);
        go.transform.localScale = Vector3.zero;

        var targetLayer = ResolveRescueRenderLayer(character, go.layer);
        SetLayerRecursively(go, targetLayer);

        var renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers != null)
        {
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                    TotemModelAssets.TryApplyVisualMaterialFixes(renderer, _modPath);
                }
            }
        }

        ModLog.Info($"[TotemOfUndying] Rescue spawn state: source='{modelSource}' activeSelf={go.activeSelf} activeInHierarchy={go.activeInHierarchy} prefabScale={baseScale} targetScale={baseScale * RescueModelScaleMultiplier} targetLayer={targetLayer} headAnchor='{(headAnchor != null ? headAnchor.name : "<null>")}' position={go.transform.position}");

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

        LogRescueModelDiagnostics(go, modelSource);

        _instance.StartCoroutine(AnimateRescueModel(go, headAnchor, position, baseScale * RescueModelScaleMultiplier));
        ModLog.Info($"[TotemOfUndying] Rescue model spawned at {go.transform.position} using source '{modelSource}'.");
    }

    private static IEnumerator AnimateRescueModel(GameObject model, Transform? headAnchor, Vector3 fallbackHeadPosition, Vector3 targetScale)
    {
        if (model == null)
        {
            yield break;
        }

        var elapsed = 0f;
        var totalDuration = RescueModelScaleInSeconds + RescueModelSlowDownSeconds + RescueModelScaleOutSeconds;

        while (model != null && elapsed < RescueModelScaleInSeconds)
        {
            elapsed += Time.deltaTime;

            var tIn = Mathf.Clamp01(elapsed / RescueModelScaleInSeconds);
            var yIn = RescueModelFloatSpeed * elapsed;
            var basePos = ResolveRescueEffectPosition(headAnchor, fallbackHeadPosition);

            model.transform.position = basePos + Vector3.up * yIn;
            model.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, tIn);
            model.transform.Rotate(Vector3.up, RescueModelSpinSpeedDeg * Time.deltaTime, Space.World);

            yield return null;
        }

        while (model != null && elapsed < RescueModelScaleInSeconds + RescueModelSlowDownSeconds)
        {
            elapsed += Time.deltaTime;

            var tSlow = Mathf.Clamp01((elapsed - RescueModelScaleInSeconds) / RescueModelSlowDownSeconds);
            var spinNow = Mathf.Lerp(RescueModelSpinSpeedDeg, RescueModelSpinSpeedMinDeg, tSlow);
            var yMid = RescueModelFloatSpeed * elapsed;
            var basePos = ResolveRescueEffectPosition(headAnchor, fallbackHeadPosition);

            model.transform.position = basePos + Vector3.up * yMid;
            model.transform.localScale = targetScale;
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
            var basePos = ResolveRescueEffectPosition(headAnchor, fallbackHeadPosition);

            model.transform.position = basePos + Vector3.up * yOut;
            model.transform.localScale = Vector3.LerpUnclamped(targetScale, Vector3.zero, tOut);
            model.transform.Rotate(Vector3.up, RescueModelSpinSpeedMinDeg * Time.deltaTime, Space.World);

            yield return null;
        }

        if (model != null)
        {
            Destroy(model);
        }
    }

    private static GameObject CreateFallbackRescueModel()
    {
        var root = new GameObject("TotemOfUndying_RuntimeFallbackModel");

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 0f, 0f);
        body.transform.localScale = new Vector3(0.22f, 0.36f, 0.12f);

        var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "Cap";
        cap.transform.SetParent(root.transform, false);
        cap.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        cap.transform.localScale = new Vector3(0.28f, 0.08f, 0.18f);

        var mat = GetOrCreateFallbackModelMaterial();
        if (mat != null)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.sharedMaterial = mat;
                }
            }
        }

        var cols = root.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (var col in cols)
        {
            if (col != null)
            {
                Destroy(col);
            }
        }

        return root;
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

    private static Material? GetOrCreateFallbackModelMaterial()
    {
        if (_fallbackModelMaterial != null)
        {
            return _fallbackModelMaterial;
        }

        Shader? shader = null;
        try
        {
            shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                ?? Shader.Find("Standard")
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
            var mat = new Material(shader)
            {
                name = "TotemOfUndying_FallbackModelMat"
            };

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", new Color(0.92f, 0.84f, 0.18f, 1f));
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", new Color(0.92f, 0.84f, 0.18f, 1f));
            }

            _fallbackModelMaterial = mat;
            return _fallbackModelMaterial;
        }
        catch
        {
            return null;
        }
    }

    private static void LogRescueModelDiagnostics(GameObject root, string modelSource)
    {
        if (root == null)
        {
            return;
        }

        try
        {
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers == null || renderers.Length == 0)
            {
                ModLog.Warn($"[TotemOfUndying] Diagnostics source='{modelSource}': no Renderer found under spawned model root '{root.name}'.");
                return;
            }

            ModLog.Info($"[TotemOfUndying] Diagnostics source='{modelSource}': rendererCount={renderers.Length}, rootScale={root.transform.localScale}, rootLayer={root.layer}");

            for (var r = 0; r < renderers.Length; r++)
            {
                var renderer = renderers[r];
                if (renderer == null)
                {
                    continue;
                }

                var rendererPath = GetTransformPath(renderer.transform, root.transform);
                var mats = renderer.sharedMaterials;
                var matCount = mats?.Length ?? 0;
                ModLog.Info($"[TotemOfUndying] Diagnostics renderer[{r}] path='{rendererPath}' type={renderer.GetType().Name} enabled={renderer.enabled} matCount={matCount}");

                if (mats == null || mats.Length == 0)
                {
                    continue;
                }

                for (var i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    if (mat == null)
                    {
                        ModLog.Warn($"[TotemOfUndying] Diagnostics renderer[{r}] material[{i}] is NULL.");
                        continue;
                    }

                    var shaderName = mat.shader != null ? mat.shader.name : "<null-shader>";
                    var baseMapName = "<none>";
                    var mainTexName = "<none>";
                    var baseColor = "<n/a>";
                    var color = "<n/a>";

                    try
                    {
                        if (mat.HasProperty("_BaseMap"))
                        {
                            var tex = mat.GetTexture("_BaseMap");
                            if (tex != null)
                            {
                                baseMapName = tex.name;
                            }
                        }

                        if (mat.HasProperty("_MainTex"))
                        {
                            var tex = mat.GetTexture("_MainTex");
                            if (tex != null)
                            {
                                mainTexName = tex.name;
                            }
                        }

                        if (mat.HasProperty("_BaseColor"))
                        {
                            var c = mat.GetColor("_BaseColor");
                            baseColor = $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                        }

                        if (mat.HasProperty("_Color"))
                        {
                            var c = mat.GetColor("_Color");
                            color = $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                        }
                    }
                    catch (System.Exception e)
                    {
                        ModLog.Warn($"[TotemOfUndying] Diagnostics read material[{i}] failed: {e.Message}");
                    }

                    ModLog.Info($"[TotemOfUndying] Diagnostics renderer[{r}] material[{i}] name='{mat.name}' shader='{shaderName}' _BaseMap='{baseMapName}' _MainTex='{mainTexName}' _BaseColor={baseColor} _Color={color}");
                }
            }
        }
        catch (System.Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Diagnostics failed: {e.GetType().Name}: {e.Message}");
        }
    }

    private static string GetTransformPath(Transform t, Transform root)
    {
        if (t == null)
        {
            return "<null-transform>";
        }

        if (root == null)
        {
            return t.name;
        }

        var path = t.name;
        var current = t;
        while (current != null && current.parent != null && current != root)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
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

    private static int ResolveRescueRenderLayer(CharacterMainControl? character, int fallbackLayer)
    {
        if (character == null)
        {
            return fallbackLayer;
        }

        try
        {
            var renderers = character.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                    {
                        ModLog.Info($"[TotemOfUndying] Rescue layer probe: using renderer '{renderer.name}' on layer={renderer.gameObject.layer} enabled={renderer.enabled}.");
                        return renderer.gameObject.layer;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Rescue layer probe failed: {e.GetType().Name}: {e.Message}");
        }

        ModLog.Info($"[TotemOfUndying] Rescue layer probe: falling back to layer={fallbackLayer}.");
        return fallbackLayer;
    }

    private static Vector3 ResolveHeadPosition(CharacterMainControl? character, Vector3 fallback)
    {
        var headAnchor = ResolveHeadAnchor(character);
        if (headAnchor != null)
        {
            return headAnchor.position;
        }

        return ResolveCharacterTopPosition(character, fallback);
    }

    private static Transform? ResolveHeadAnchor(CharacterMainControl? character)
    {
        if (character == null)
        {
            return null;
        }

        try
        {
            var animator = character.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                if (headBone != null)
                {
                    return headBone;
                }
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            var transforms = character.GetComponentsInChildren<Transform>(includeInactive: true);
            if (transforms != null)
            {
                foreach (var candidate in transforms)
                {
                    if (candidate == null)
                    {
                        continue;
                    }

                    var lowerName = candidate.name.ToLowerInvariant();
                    if (lowerName.Contains("head") || lowerName.Contains("neck"))
                    {
                        ModLog.Info($"[TotemOfUndying] Rescue head anchor matched by name: '{candidate.name}'.");
                        return candidate;
                    }
                }
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static Vector3 ResolveRescueEffectPosition(Transform? headAnchor, Vector3 fallbackHeadPosition)
    {
        var anchorPosition = headAnchor != null ? headAnchor.position : fallbackHeadPosition;
        return anchorPosition + Vector3.up * RescueEffectHeadOffset;
    }

    private static Vector3 ResolveCharacterTopPosition(CharacterMainControl? character, Vector3 fallback)
    {
        if (character == null)
        {
            return fallback;
        }

        try
        {
            var renderers = character.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers != null && renderers.Length > 0)
            {
                var foundBounds = false;
                var bounds = default(Bounds);
                foreach (var renderer in renderers)
                {
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!foundBounds)
                    {
                        bounds = renderer.bounds;
                        foundBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                if (foundBounds)
                {
                    var topPosition = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                    ModLog.Info($"[TotemOfUndying] Rescue top position resolved from renderer bounds: center={bounds.center} max={bounds.max}.");
                    return topPosition;
                }
            }
        }
        catch (System.Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] ResolveCharacterTopPosition failed: {e.GetType().Name}: {e.Message}");
        }

        return fallback;
    }
}
