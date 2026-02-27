using System;
using System.Collections.Generic;
using System.IO;
using ItemStatsSystem;
using UnityEngine;

namespace EnderPearl;

internal static class ModAssets
{
    internal static string? CurrentModPath { get; private set; }

    private const string IconFileName = "icon.png";
    private const string EnderPearlIconRelativePath = "assets/item-icons/Ender_Pearl.png";
    private const string ForceUnlitFlagFileName = "force_unlit.txt";
    private const string ForceLitFlagFileName = "force_lit.txt";

    // Bundle file names we will try under the mod folder.
    // Note: We search `assets/bundles/` first, then fall back to the mod root for backward-compat.
    private static readonly string[] BundleCandidateNames =
    {
        "enderpearl_assets",
        "enderpearl_assets.bundle",
        "enderpearl_assets.unity3d"
    };

    private const string HandheldAgentPrefabName = "EnderPearl_HandheldAgent";
    private const string PickupAgentPrefabName = "EnderPearl_PickupAgent";

    // Simpler workflow: plain model prefabs (no game scripts required)
    private const string HandheldModelPrefabName = "EnderPearl_HandheldModel";
    private const string PickupModelPrefabName = "EnderPearl_PickupModel";
    private const string FlyingModelPrefabName = "EnderPearl_FlyingModel";

    private static AssetBundle? _bundle;
    private static bool? _forceUnlit;


    private static bool ShouldForceUnlit(string? modPath)
    {
        // Default behavior: force Unlit for EnderPearl models because URP/Lit path
        // is known to render incorrectly (blue) in this game runtime.
        if (_forceUnlit.HasValue) return _forceUnlit.Value;

        try
        {
            var basePath = modPath ?? string.Empty;

            // Explicit opt-out: allow trying Lit without code changes.
            var forceLitPath = Path.Combine(basePath, ForceLitFlagFileName);
            if (File.Exists(forceLitPath))
            {
                _forceUnlit = false;
                return false;
            }

            // Backward compatible: if someone still uses force_unlit.txt, respect it.
            var forceUnlitPath = Path.Combine(basePath, ForceUnlitFlagFileName);
            if (File.Exists(forceUnlitPath))
            {
                _forceUnlit = true;
                return true;
            }

            _forceUnlit = true;
        }
        catch (Exception e)
        {
            // If anything goes wrong, prefer Unlit so the item stays visible.
            ModLog.Warn($"[EnderPearl] Force-unlit decision failed: {e.GetType().Name}: {e.Message}");
            _forceUnlit = true;
        }

        return true;
    }

    private static void TryApplyOptionalUnlitOverride(Renderer renderer, string? modPath)
    {
        if (renderer == null) return;
        if (!ShouldForceUnlit(modPath)) return;

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
            ModLog.Warn("[EnderPearl] Force-unlit: no Unlit shader found (URP/Unlit, Unlit/Texture, Unlit/Color)");
            return;
        }

        try
        {
            // Use instance materials so we don't mutate shared assets.
            var mats = renderer.materials;
            if (mats == null || mats.Length == 0) return;

            var changed = 0;
            for (var i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                if (m.shader == unlitShader) continue;

                Texture? baseMap = null;
                Texture? mainTex = null;

                try
                {
                    if (m.HasProperty("_BaseMap")) baseMap = m.GetTexture("_BaseMap");
                    if (m.HasProperty("_MainTex")) mainTex = m.GetTexture("_MainTex");
                    if (baseMap == null) baseMap = m.mainTexture;
                    if (mainTex == null) mainTex = m.mainTexture;
                }
                catch
                {
                    // ignore
                }

                m.shader = unlitShader;

                // Re-apply texture onto the new shader.
                try
                {
                    var texToUse = baseMap ?? mainTex;
                    if (texToUse != null)
                    {
                        if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", texToUse);
                        if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", texToUse);
                        m.mainTexture = texToUse;
                    }

                    // Reduce accidental tinting while in debug mode.
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
                    if (m.HasProperty("_Color")) m.SetColor("_Color", Color.white);
                }
                catch
                {
                    // ignore
                }

                changed++;
            }

            if (changed > 0)
            {
                // Keep logging minimal in release.
            }

            // Clear any per-renderer overrides that may tint the material.
            try
            {
                renderer.SetPropertyBlock(new MaterialPropertyBlock());
            }
            catch
            {
                // ignore
            }
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Force-unlit apply failed: {e.GetType().Name}: {e.Message}");
        }
    }

    private static void TryApplyVisualMaterialFixes(Renderer renderer, string? modPath)
    {
        if (renderer == null) return;
        TryApplyOptionalUnlitOverride(renderer, modPath);
    }

    internal static void SetModPath(string? modPath)
    {
        if (string.IsNullOrWhiteSpace(modPath))
        {
            return;
        }
        CurrentModPath = modPath;
    }

    internal static Sprite? TryLoadIconSprite(string? modPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(modPath))
            {
                return null;
            }

            // Preferred: load from assets/item-icons (project-shipped icon).
            var itemIconPath = Path.Combine(modPath, EnderPearlIconRelativePath);
            if (File.Exists(itemIconPath))
            {
                var sprite = TryLoadSpriteFromPngFile(itemIconPath, "EnderPearl_Icon");
                if (sprite != null) return sprite;
            }

            var iconPath = Path.Combine(modPath, IconFileName);
            if (!File.Exists(iconPath))
            {
                return null;
            }

            return TryLoadSpriteFromPngFile(iconPath, "EnderPearl_Icon");
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Failed to load icon.png: {e.Message}");
            return null;
        }
    }

    private static Sprite? TryLoadSpriteFromPngFile(string pngPath, string textureName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pngPath) || !File.Exists(pngPath))
            {
                return null;
            }

            var pngBytes = File.ReadAllBytes(pngPath);
            if (pngBytes.Length == 0)
            {
                return null;
            }

            // Texture size will be replaced by LoadImage
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            if (!ImageConversion.LoadImage(texture, pngBytes))
            {
                return null;
            }

            texture.name = textureName;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            var rect = new Rect(0, 0, texture.width, texture.height);
            var pivot = new Vector2(0.5f, 0.5f);

            // Using pixelsPerUnit=100 is a common Unity default; UI will scale anyway.
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit: 100f);
        }
        catch
        {
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

            var handheldPrefab = bundle.LoadAsset<ItemAgent>(HandheldAgentPrefabName);
            var pickupPrefab = bundle.LoadAsset<ItemAgent>(PickupAgentPrefabName);

            if (handheldPrefab == null && pickupPrefab == null)
            {
                return;
            }

            UpsertAgentPrefab(item.AgentUtilities, "Handheld", handheldPrefab);
            UpsertAgentPrefab(item.AgentUtilities, "Pickup", pickupPrefab);
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Failed to inject item agents: {e.Message}");
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

            string modelName;
            switch (agent.AgentType)
            {
                case ItemAgent.AgentTypes.handheld:
                    modelName = HandheldModelPrefabName;
                    break;
                case ItemAgent.AgentTypes.pickUp:
                    modelName = PickupModelPrefabName;
                    break;
                default:
                    return;
            }

            var modelPrefab = bundle.LoadAsset<GameObject>(modelName);
            if (modelPrefab == null)
            {
                ModLog.Warn($"[EnderPearl] Bundle loaded but prefab '{modelName}' not found.");
                return;
            }

            var prefabLocalScale = modelPrefab.transform.localScale;

            var existing = agent.transform.Find(modelPrefab.name);
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            var instance = UnityEngine.Object.Instantiate(modelPrefab, agent.transform);
            instance.name = modelPrefab.name;
            instance.SetActive(true);
            instance.transform.localPosition = Vector3.zero;

            var targetLayer = ResolvePreferredRenderLayer(agent);
            SetLayerRecursively(instance, targetLayer);

            var renderers = instance.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers != null)
            {
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    r.enabled = true;
                    TryApplyVisualMaterialFixes(r, modPath);
                }
            }

            ModLog.Info($"[EnderPearl] Attached model '{modelPrefab.name}' to agentType={agent.AgentType} renderers={(renderers?.Length ?? 0)} layer={targetLayer} prefabLocalScale={prefabLocalScale} instanceLocalScale={instance.transform.localScale}");
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Failed to attach model prefab: {e.Message}");
        }
    }

    private static int ResolvePreferredRenderLayer(ItemAgent agent)
    {
        try
        {
            // Some view-model setups keep root on Default but renderers on a dedicated layer.
            var agentRenderers = agent.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (agentRenderers != null)
            {
                foreach (var r in agentRenderers)
                {
                    if (r == null) continue;
                    return r.gameObject.layer;
                }
            }

            // If the agent itself has no renderers, look upward (viewmodel roots often carry the correct layer).
            var t = agent.transform;
            int hops = 0;
            while (t != null && hops < 16)
            {
                if (t.gameObject.layer != 0)
                {
                    return t.gameObject.layer;
                }

                var parentRenderers = t.GetComponentsInParent<Renderer>(includeInactive: true);
                if (parentRenderers != null)
                {
                    foreach (var pr in parentRenderers)
                    {
                        if (pr == null) continue;
                        if (pr.gameObject.layer != 0)
                        {
                            return pr.gameObject.layer;
                        }
                    }
                }

                t = t.parent;
                hops++;
            }
        }
        catch
        {
            // ignore
        }
        return agent.gameObject.layer;
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

            var modelPrefab = bundle.LoadAsset<GameObject>(FlyingModelPrefabName)
                             ?? bundle.LoadAsset<GameObject>(PickupModelPrefabName)
                             ?? bundle.LoadAsset<GameObject>(HandheldModelPrefabName);

            if (modelPrefab == null)
            {
                ModLog.Warn($"[EnderPearl] Projectile: no model prefab found (tried '{FlyingModelPrefabName}', '{PickupModelPrefabName}', '{HandheldModelPrefabName}').");
                return false;
            }

            var instance = UnityEngine.Object.Instantiate(modelPrefab, projectileRoot.transform);
            instance.name = modelPrefab.name;
            instance.SetActive(true);
            instance.transform.localPosition = Vector3.zero;
            // Keep prefab-authored rotation/scale (do not override), so creators can tune it in Unity.

            SetLayerRecursively(instance, projectileRoot.layer);

            var renderers = instance.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers != null)
            {
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    r.enabled = true;
                    TryApplyVisualMaterialFixes(r, modPath);
                }
            }

            ModLog.Info($"[EnderPearl] Projectile model '{modelPrefab.name}' attached. renderers={(renderers?.Length ?? 0)} layer={projectileRoot.layer}");
            return true;
        }
        catch (Exception e)
        {
            ModLog.Warn($"[EnderPearl] Projectile model attach failed: {e.Message}");
            return false;
        }
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        if (root == null) return;
        root.layer = layer;
        var transform = root.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }

    private static AssetBundle? TryLoadBundle(string modPath)
    {
        if (_bundle != null)
        {
            return _bundle;
        }

        var candidateBaseDirs = new[]
        {
            // Organized bundles (preferred)
            Path.Combine(modPath, "assets", "bundles", "models"),
            Path.Combine(modPath, "assets", "bundles", "sfx"),
            // Backward compatible: allow bundles directly under assets/bundles
            Path.Combine(modPath, "assets", "bundles"),
            // Backward compatible: allow bundles at mod root
            modPath
        };

        foreach (var baseDir in candidateBaseDirs)
        {
            foreach (var name in BundleCandidateNames)
            {
                var bundlePath = Path.Combine(baseDir, name);
                if (!File.Exists(bundlePath))
                {
                    continue;
                }

                try
                {
                    try
                    {
                        var fi = new FileInfo(bundlePath);
                        ModLog.Info($"[EnderPearl] Loading AssetBundle file: path='{bundlePath}' bytes={fi.Length} lastWriteUtc={fi.LastWriteTimeUtc:O}");
                    }
                    catch
                    {
                        // ignore
                    }

                    _bundle = AssetBundle.LoadFromFile(bundlePath);
                    if (_bundle != null)
                    {
                        ModLog.Info($"[EnderPearl] Loaded AssetBundle: {bundlePath}");
                        try
                        {
                            var assets = _bundle.GetAllAssetNames();
                            if (assets != null && assets.Length > 0)
                            {
                                ModLog.Info($"[EnderPearl] Bundle assets ({assets.Length}):\n- {string.Join("\n- ", assets)}");
                            }
                            else
                            {
                                ModLog.Warn("[EnderPearl] Bundle loaded but GetAllAssetNames returned empty.");
                            }
                        }
                        catch (Exception e)
                        {
                            ModLog.Warn($"[EnderPearl] Failed to list bundle assets: {e.Message}");
                        }
                        return _bundle;
                    }
                }
                catch (Exception e)
                {
                    ModLog.Warn($"[EnderPearl] Failed to load AssetBundle '{bundlePath}': {e.Message}");
                }
            }
        }

        return null;
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

        // Remove existing entry with same key (if any)
        agents.RemoveAll(p => p != null && string.Equals(p.key, key, StringComparison.Ordinal));

        agents.Add(new ItemAgentUtilities.AgentKeyPair
        {
            key = key,
            agentPrefab = prefab
        });

        // Clear hashed cache so the new entry is picked up
        ReflectionUtil.SetPrivateField(agentUtilities, "hashedAgentsCache", null);
    }
}
