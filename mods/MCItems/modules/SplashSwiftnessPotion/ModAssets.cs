using System;
using System.Collections.Generic;
using System.IO;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.Rendering;

namespace SplashSwiftnessPotion;

internal static class ModAssets
{
    private const float ModelScaleMultiplier = 0.66f;
    private const string IconRelativePath = "assets/item-icons/SplashSwiftnessPotion.png";

    private static readonly string[] BundleCandidateNames =
    {
        "splashswiftnesspotion_assets",
        "splashswiftnesspotion_assets.bundle",
        "splashswiftnesspotion_assets.unity3d"
    };

    private static readonly string[] HandheldAgentPrefabNames = { "SplashSwiftnessPotion_HandheldAgent" };
    private static readonly string[] PickupAgentPrefabNames = { "SplashSwiftnessPotion_PickupAgent" };
    private static readonly string[] HandheldModelPrefabNames = { "SplashSwiftnessPotion_HandheldModel" };
    private static readonly string[] PickupModelPrefabNames = { "SplashSwiftnessPotion_PickupModel" };
    private static readonly string[] FlyingModelPrefabNames = { "SplashSwiftnessPotion_PickupModel", "SplashSwiftnessPotion_HandheldModel" };

    private static AssetBundle? _bundle;

    internal static string? CurrentModPath { get; private set; }

    internal static void SetModPath(string? modPath)
    {
        if (!string.IsNullOrWhiteSpace(modPath))
        {
            CurrentModPath = modPath;
        }
    }

    internal static Sprite? TryLoadIconSprite(string? modPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modPath))
            {
                return null;
            }

            var iconPath = Path.Combine(modPath, IconRelativePath);
            if (!File.Exists(iconPath))
            {
                return null;
            }

            var pngBytes = File.ReadAllBytes(iconPath);
            if (pngBytes.Length == 0)
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!ImageConversion.LoadImage(texture, pngBytes))
            {
                return null;
            }

            texture.name = "SplashSwiftnessPotion_Icon";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[SplashSwiftnessPotion] Failed to load icon: {exception.Message}");
            return null;
        }
    }

    internal static void TryInjectItemAgents(Item item, string? modPath)
    {
        try
        {
            if (item == null || string.IsNullOrWhiteSpace(modPath))
            {
                return;
            }

            SetModPath(modPath);
            var bundle = TryLoadBundle(modPath);
            if (bundle == null)
            {
                return;
            }

            var handheldPrefab = LoadFirstAsset<ItemAgent>(bundle, HandheldAgentPrefabNames);
            var pickupPrefab = LoadFirstAsset<ItemAgent>(bundle, PickupAgentPrefabNames);
            UpsertAgentPrefab(item.AgentUtilities, "Handheld", handheldPrefab);
            UpsertAgentPrefab(item.AgentUtilities, "Pickup", pickupPrefab);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[SplashSwiftnessPotion] Failed to inject item agents: {exception.Message}");
        }
    }

    internal static void TryAttachModelToAgent(ItemAgent agent, string? modPath)
    {
        try
        {
            if (agent == null || string.IsNullOrWhiteSpace(modPath))
            {
                return;
            }

            SetModPath(modPath);
            var bundle = TryLoadBundle(modPath);
            if (bundle == null)
            {
                return;
            }

            string[] modelNames = agent.AgentType == ItemAgent.AgentTypes.handheld
                ? HandheldModelPrefabNames
                : agent.AgentType == ItemAgent.AgentTypes.pickUp
                    ? PickupModelPrefabNames
                    : Array.Empty<string>();

            if (modelNames.Length == 0)
            {
                return;
            }

            var modelPrefab = LoadFirstAsset<GameObject>(bundle, modelNames);
            if (modelPrefab == null)
            {
                return;
            }

            var existing = agent.transform.Find(modelPrefab.name);
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            var instance = UnityEngine.Object.Instantiate(modelPrefab, agent.transform);
            instance.name = modelPrefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale *= ModelScaleMultiplier;
            instance.SetActive(true);

            SetLayerRecursively(instance, agent.gameObject.layer);
            ApplyRendererFixes(instance, modPath);
            DisableCompetingRenderers(agent, instance.transform);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[SplashSwiftnessPotion] Failed to attach agent model: {exception.Message}");
        }
    }

    internal static bool TryAttachModelToProjectile(GameObject projectileRoot)
    {
        try
        {
            var modPath = CurrentModPath;
            if (projectileRoot == null || string.IsNullOrWhiteSpace(modPath))
            {
                return false;
            }

            var bundle = TryLoadBundle(modPath);
            if (bundle == null)
            {
                return false;
            }

            var modelPrefab = LoadFirstAsset<GameObject>(bundle, FlyingModelPrefabNames);
            if (modelPrefab == null)
            {
                return false;
            }

            var instance = UnityEngine.Object.Instantiate(modelPrefab, projectileRoot.transform);
            instance.name = modelPrefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale *= ModelScaleMultiplier;
            instance.SetActive(true);

            var colliders = instance.GetComponentsInChildren<Collider>(includeInactive: true);
            foreach (var collider in colliders)
            {
                if (collider != null)
                {
                    UnityEngine.Object.Destroy(collider);
                }
            }

            var rigidbodies = instance.GetComponentsInChildren<Rigidbody>(includeInactive: true);
            foreach (var rigidbody in rigidbodies)
            {
                if (rigidbody != null)
                {
                    UnityEngine.Object.Destroy(rigidbody);
                }
            }

            SetLayerRecursively(instance, projectileRoot.layer);
            ApplyRendererFixes(instance, modPath);
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[SplashSwiftnessPotion] Failed to attach projectile model: {exception.Message}");
            return false;
        }
    }

    private static void ApplyRendererFixes(GameObject root, string? modPath)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = true;
            TryApplyOptionalUnlitOverride(renderer);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static void TryApplyOptionalUnlitOverride(Renderer renderer)
    {
        Shader? unlitShader = null;
        try
        {
            unlitShader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Texture")
                ?? Shader.Find("Unlit/Color");
        }
        catch
        {
            // ignore
        }

        if (unlitShader == null)
        {
            return;
        }

        var materials = renderer.materials;
        for (int index = 0; index < materials.Length; index++)
        {
            var material = materials[index];
            if (material == null)
            {
                continue;
            }

            Texture? texture = null;
            if (material.HasProperty("_BaseMap")) texture = material.GetTexture("_BaseMap");
            if (texture == null && material.HasProperty("_MainTex")) texture = material.GetTexture("_MainTex");
            if (texture == null) texture = material.mainTexture;

            material.shader = unlitShader;
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
                material.mainTexture = texture;
            }

            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
        }
    }

    private static void DisableCompetingRenderers(ItemAgent agent, Transform keepModelRoot)
    {
        var renderers = agent.GetComponentsInChildren<Renderer>(includeInactive: true);
        foreach (var renderer in renderers)
        {
            if (renderer == null || renderer.transform.IsChildOf(keepModelRoot))
            {
                continue;
            }

            renderer.enabled = false;
        }
    }

    private static AssetBundle? TryLoadBundle(string modPath)
    {
        if (_bundle != null)
        {
            return _bundle;
        }

        var candidateDirs = new[]
        {
            Path.Combine(modPath, "assets", "bundles", "models"),
            Path.Combine(modPath, "assets", "bundles"),
            modPath
        };

        foreach (var baseDir in candidateDirs)
        {
            foreach (var candidateName in BundleCandidateNames)
            {
                var bundlePath = Path.Combine(baseDir, candidateName);
                if (!File.Exists(bundlePath))
                {
                    continue;
                }

                try
                {
                    _bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (_bundle != null)
                    {
                        return _bundle;
                    }
                }
                catch (Exception exception)
                {
                    ModLog.Warn($"[SplashSwiftnessPotion] Failed to load AssetBundle '{bundlePath}': {exception.Message}");
                }
            }
        }

        return null;
    }

    private static T? LoadFirstAsset<T>(AssetBundle bundle, IEnumerable<string> assetNames) where T : UnityEngine.Object
    {
        foreach (var assetName in assetNames)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                continue;
            }

            var asset = bundle.LoadAsset<T>(assetName);
            if (asset != null)
            {
                return asset;
            }
        }

        return null;
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        var transform = root.transform;
        for (int index = 0; index < transform.childCount; index++)
        {
            var child = transform.GetChild(index);
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }

    private static void UpsertAgentPrefab(ItemAgentUtilities agentUtilities, string key, ItemAgent? prefab)
    {
        if (agentUtilities == null || prefab == null)
        {
            return;
        }

        var agents = ReflectionUtil.GetPrivateField<List<ItemAgentUtilities.AgentKeyPair>>(agentUtilities, "agents");
        if (agents == null)
        {
            agents = new List<ItemAgentUtilities.AgentKeyPair>();
            ReflectionUtil.SetPrivateField(agentUtilities, "agents", agents);
        }

        agents.RemoveAll(entry => entry != null && string.Equals(entry.key, key, StringComparison.Ordinal));
        agents.Add(new ItemAgentUtilities.AgentKeyPair
        {
            key = key,
            agentPrefab = prefab
        });

        ReflectionUtil.SetPrivateField(agentUtilities, "hashedAgentsCache", null);
    }
}