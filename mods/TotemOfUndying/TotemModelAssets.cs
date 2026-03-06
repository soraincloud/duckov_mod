using System;
using System.Collections.Generic;
using System.IO;
using ItemStatsSystem;
using UnityEngine;

namespace TotemOfUndying;

internal static class TotemModelAssets
{
    private const string ForceUnlitFlagFileName = "force_unlit.txt";
    private const string ForceLitFlagFileName = "force_lit.txt";
    private const float PickupModelScaleMultiplier = 2f;

    private const string HandheldAgentPrefabName = "TotemOfUndying_HandheldAgent";
    private const string PickupAgentPrefabName = "TotemOfUndying_PickupAgent";

    private const string HandheldModelPrefabName = "TotemOfUndying_HandheldModel";
    private const string PickupModelPrefabName = "TotemOfUndying_PickupModel";

    private static readonly string[] BundleCandidateNames =
    {
        "totemofundying_assets",
        "totemofundying_assets.bundle",
        "totemofundying_assets.unity3d",
        "totem_assets",
        "totem_assets.bundle",
        "totem_assets.unity3d"
    };

    private static string? _currentModPath;
    private static AssetBundle? _bundle;
    private static bool? _forceUnlit;

    internal static void SetModPath(string? modPath)
    {
        if (!string.IsNullOrWhiteSpace(modPath))
        {
            _currentModPath = modPath;
        }
    }

    internal static void Deinitialize()
    {
        if (_bundle != null)
        {
            _bundle.Unload(unloadAllLoadedObjects: false);
            _bundle = null;
        }

        _forceUnlit = null;
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
            ModLog.Warn($"[TotemOfUndying] Failed to inject item agents: {e.Message}");
        }
    }

    internal static void TryAttachModelToAgent(ItemAgent agent, string? modPath)
    {
        try
        {
            var path = modPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _currentModPath;
            }

            if (agent == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            SetModPath(path);

            var bundle = TryLoadBundle(path);
            if (bundle == null)
            {
                ModLog.Warn("[TotemOfUndying] AttachModelToAgent skipped: model bundle not loaded.");
                return;
            }

            var modelPrefab = ResolveModelPrefabForAgent(bundle, agent.AgentType);
            if (modelPrefab == null)
            {
                ModLog.Warn($"[TotemOfUndying] No model prefab found for agentType={agent.AgentType}. Tried '{HandheldModelPrefabName}' and '{PickupModelPrefabName}'.");
                return;
            }

            var existing = agent.transform.Find(modelPrefab.name);
            if (existing != null)
            {
                UnityEngine.Object.Destroy(existing.gameObject);
            }

            var instance = UnityEngine.Object.Instantiate(modelPrefab, agent.transform);
            instance.name = modelPrefab.name;
            instance.SetActive(true);
            instance.transform.localPosition = Vector3.zero;
            if (agent.AgentType == ItemAgent.AgentTypes.pickUp)
            {
                instance.transform.localScale *= PickupModelScaleMultiplier;
            }

            var targetLayer = ResolvePreferredRenderLayer(agent);
            SetLayerRecursively(instance, targetLayer);

            var renderers = instance.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers != null)
            {
                foreach (var r in renderers)
                {
                    if (r != null)
                    {
                        r.enabled = true;
                        TryApplyVisualMaterialFixes(r, path);
                    }
                }
            }

            DisableCompetingRenderers(agent, instance.transform);

            LogAgentModelDiagnostics(instance, agent.AgentType);

            ModLog.Info($"[TotemOfUndying] Attached model '{modelPrefab.name}' to agentType={agent.AgentType} renderers={(renderers?.Length ?? 0)} layer={targetLayer} localScale={instance.transform.localScale}.");
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Failed to attach model to agent: {e.Message}");
        }
    }

    internal static GameObject? TryLoadPickupModelPrefab(string? modPath)
    {
        try
        {
            var path = modPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = _currentModPath;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                ModLog.Warn("[TotemOfUndying] TryLoadPickupModelPrefab skipped: modPath is empty.");
                return null;
            }

            SetModPath(path);

            var bundle = TryLoadBundle(path);
            if (bundle == null)
            {
                ModLog.Warn("[TotemOfUndying] TryLoadPickupModelPrefab skipped: model bundle not loaded.");
                return null;
            }

            var prefab = bundle.LoadAsset<GameObject>(PickupModelPrefabName)
                ?? bundle.LoadAsset<GameObject>(HandheldModelPrefabName);
            if (prefab != null)
            {
                ModLog.Info($"[TotemOfUndying] TryLoadPickupModelPrefab resolved '{prefab.name}'.");
                return prefab;
            }

            var allAssets = bundle.GetAllAssetNames();
            if (allAssets == null)
            {
                return null;
            }

            foreach (var assetName in allAssets)
            {
                if (string.IsNullOrWhiteSpace(assetName))
                {
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(assetName);
                if (!string.Equals(fileName, PickupModelPrefabName, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(fileName, HandheldModelPrefabName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                prefab = bundle.LoadAsset<GameObject>(assetName);
                if (prefab != null)
                {
                    ModLog.Info($"[TotemOfUndying] TryLoadPickupModelPrefab matched by asset path '{assetName}'.");
                    return prefab;
                }
            }
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] TryLoadPickupModelPrefab failed: {e.Message}");
        }

        return null;
    }

    private static GameObject? ResolveModelPrefabForAgent(AssetBundle bundle, ItemAgent.AgentTypes agentType)
    {
        switch (agentType)
        {
            case ItemAgent.AgentTypes.handheld:
                return bundle.LoadAsset<GameObject>(HandheldModelPrefabName)
                    ?? bundle.LoadAsset<GameObject>(PickupModelPrefabName);
            case ItemAgent.AgentTypes.pickUp:
                return bundle.LoadAsset<GameObject>(PickupModelPrefabName)
                    ?? bundle.LoadAsset<GameObject>(HandheldModelPrefabName);
            default:
                return bundle.LoadAsset<GameObject>(PickupModelPrefabName)
                    ?? bundle.LoadAsset<GameObject>(HandheldModelPrefabName);
        }
    }

    private static void DisableCompetingRenderers(ItemAgent agent, Transform keepModelRoot)
    {
        if (agent == null || keepModelRoot == null)
        {
            return;
        }

        var allAgentRenderers = agent.GetComponentsInChildren<Renderer>(includeInactive: true);
        var disabled = 0;
        if (allAgentRenderers != null)
        {
            foreach (var renderer in allAgentRenderers)
            {
                if (renderer == null || renderer.transform == null)
                {
                    continue;
                }

                if (renderer.transform.IsChildOf(keepModelRoot))
                {
                    continue;
                }

                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    disabled++;
                }
            }
        }

        if (agent.AgentType == ItemAgent.AgentTypes.pickUp)
        {
            var extraDisabled = DisableCompetingPickupRenderers(agent, keepModelRoot);
            if (extraDisabled > 0)
            {
                disabled += extraDisabled;
            }

            var suppressor = agent.GetComponent<PickupVisualSuppressor>();
            if (suppressor == null)
            {
                suppressor = agent.gameObject.AddComponent<PickupVisualSuppressor>();
            }

            suppressor.Bind(agent, keepModelRoot);
        }

        if (disabled > 0)
        {
            ModLog.Info($"[TotemOfUndying] Disabled {disabled} competing renderer(s) for agentType={agent.AgentType}.");
        }
    }

    private static int ResolvePreferredRenderLayer(ItemAgent agent)
    {
        try
        {
            var agentRenderers = agent.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (agentRenderers != null)
            {
                foreach (var r in agentRenderers)
                {
                    if (r != null)
                    {
                        return r.gameObject.layer;
                    }
                }
            }

            var t = agent.transform;
            var hops = 0;
            while (t != null && hops < 16)
            {
                if (t.gameObject.layer != 0)
                {
                    return t.gameObject.layer;
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
            var child = t.GetChild(i);
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }

    private static int DisableCompetingPickupRenderers(ItemAgent agent, Transform keepModelRoot)
    {
        var container = agent.transform.parent;
        if (container == null)
        {
            return 0;
        }

        var renderers = container.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers == null || renderers.Length == 0)
        {
            return 0;
        }

        var disabled = 0;
        foreach (var renderer in renderers)
        {
            if (renderer == null || renderer.transform == null)
            {
                continue;
            }

            if (renderer.transform.IsChildOf(keepModelRoot))
            {
                continue;
            }

            if (!renderer.enabled)
            {
                continue;
            }

            renderer.enabled = false;
            disabled++;
        }

        return disabled;
    }

    private sealed class PickupVisualSuppressor : MonoBehaviour
    {
        private ItemAgent? _agent;
        private Transform? _keepModelRoot;
        private InteractablePickup? _interactablePickup;
        private SpriteRenderer? _pickupSprite;

        internal void Bind(ItemAgent agent, Transform keepModelRoot)
        {
            _agent = agent;
            _keepModelRoot = keepModelRoot;
            _interactablePickup = agent.GetComponent<InteractablePickup>();
            _pickupSprite = ResolvePickupSprite(_interactablePickup);
            SuppressNow();
        }

        private void LateUpdate()
        {
            SuppressNow();
        }

        private void SuppressNow()
        {
            if (_agent == null || _keepModelRoot == null)
            {
                return;
            }

            if (_interactablePickup == null)
            {
                _interactablePickup = _agent.GetComponent<InteractablePickup>();
                _pickupSprite = ResolvePickupSprite(_interactablePickup);
            }

            if (_pickupSprite == null)
            {
                _pickupSprite = ResolvePickupSprite(_interactablePickup);
            }

            if (_pickupSprite != null)
            {
                if (_pickupSprite.enabled)
                {
                    _pickupSprite.enabled = false;
                }

                if (_pickupSprite.gameObject != null && _pickupSprite.gameObject.activeSelf)
                {
                    _pickupSprite.gameObject.SetActive(false);
                }
            }

            var root = _agent.transform.parent != null ? _agent.transform.parent : _agent.transform;
            var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers == null)
            {
                return;
            }

            foreach (var renderer in renderers)
            {
                if (renderer == null || renderer.transform == null)
                {
                    continue;
                }

                if (renderer.transform.IsChildOf(_keepModelRoot))
                {
                    continue;
                }

                if (renderer.enabled)
                {
                    renderer.enabled = false;
                }
            }
        }

        private static SpriteRenderer? ResolvePickupSprite(InteractablePickup? pickup)
        {
            if (pickup == null)
            {
                return null;
            }

            try
            {
                var t = pickup.GetType();
                var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

                var field = t.GetField("sprite", flags) ?? t.GetField("Sprite", flags);
                if (field != null)
                {
                    return field.GetValue(pickup) as SpriteRenderer;
                }

                var prop = t.GetProperty("sprite", flags) ?? t.GetProperty("Sprite", flags);
                if (prop != null)
                {
                    return prop.GetValue(pickup) as SpriteRenderer;
                }
            }
            catch
            {
                // ignore reflection failures
            }

            return null;
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
            Path.Combine(modPath, "assets", "bundles", "models"),
            Path.Combine(modPath, "assets", "bundles"),
            modPath
        };

        foreach (var baseDir in candidateBaseDirs)
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
                        ModLog.Info($"[TotemOfUndying] Loaded model bundle: {bundlePath}");
                        try
                        {
                            var assets = _bundle.GetAllAssetNames();
                            if (assets != null && assets.Length > 0)
                            {
                                ModLog.Info($"[TotemOfUndying] Bundle assets ({assets.Length}):\n- {string.Join("\n- ", assets)}");
                            }
                        }
                        catch (Exception e)
                        {
                            ModLog.Warn($"[TotemOfUndying] Failed to list bundle assets: {e.Message}");
                        }

                        return _bundle;
                    }
                }
                catch (Exception e)
                {
                    ModLog.Warn($"[TotemOfUndying] Failed to load model bundle '{bundlePath}': {e.Message}");
                }
            }
        }

        return null;
    }

    private static bool ShouldForceUnlit(string? modPath)
    {
        if (_forceUnlit.HasValue)
        {
            return _forceUnlit.Value;
        }

        try
        {
            var basePath = modPath ?? string.Empty;

            var forceLitPath = Path.Combine(basePath, ForceLitFlagFileName);
            if (File.Exists(forceLitPath))
            {
                _forceUnlit = false;
                return false;
            }

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
            ModLog.Warn($"[TotemOfUndying] Force-unlit decision failed: {e.GetType().Name}: {e.Message}");
            _forceUnlit = true;
        }

        return _forceUnlit.Value;
    }

    internal static void TryApplyVisualMaterialFixes(Renderer renderer, string? modPath)
    {
        if (renderer == null)
        {
            return;
        }

        if (!ShouldForceUnlit(modPath))
        {
            return;
        }

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
            ModLog.Warn("[TotemOfUndying] Force-unlit enabled but no Unlit shader found.");
            return;
        }

        try
        {
            var mats = renderer.materials;
            if (mats == null || mats.Length == 0)
            {
                return;
            }

            for (var i = 0; i < mats.Length; i++)
            {
                var mat = mats[i];
                if (mat == null)
                {
                    continue;
                }

                if (mat.shader == unlitShader)
                {
                    continue;
                }

                Texture? tex = null;
                try
                {
                    if (mat.HasProperty("_BaseMap"))
                    {
                        tex = mat.GetTexture("_BaseMap");
                    }

                    if (tex == null && mat.HasProperty("_MainTex"))
                    {
                        tex = mat.GetTexture("_MainTex");
                    }

                    if (tex == null)
                    {
                        tex = mat.mainTexture;
                    }
                }
                catch
                {
                    // ignore
                }

                mat.shader = unlitShader;

                if (tex != null)
                {
                    if (mat.HasProperty("_BaseMap"))
                    {
                        mat.SetTexture("_BaseMap", tex);
                    }

                    if (mat.HasProperty("_MainTex"))
                    {
                        mat.SetTexture("_MainTex", tex);
                    }

                    mat.mainTexture = tex;
                }

                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", Color.white);
                }

                if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", Color.white);
                }
            }

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
            ModLog.Warn($"[TotemOfUndying] Force-unlit apply failed: {e.GetType().Name}: {e.Message}");
        }
    }

    private static void LogAgentModelDiagnostics(GameObject modelRoot, ItemAgent.AgentTypes agentType)
    {
        if (modelRoot == null)
        {
            return;
        }

        try
        {
            var renderers = modelRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers == null || renderers.Length == 0)
            {
                ModLog.Warn($"[TotemOfUndying] Diagnostics agentType={agentType}: no Renderer under '{modelRoot.name}'.");
                return;
            }

            ModLog.Info($"[TotemOfUndying] Diagnostics agentType={agentType}: rendererCount={renderers.Length}");

            for (var r = 0; r < renderers.Length; r++)
            {
                var renderer = renderers[r];
                if (renderer == null)
                {
                    continue;
                }

                var mats = renderer.sharedMaterials;
                var matCount = mats?.Length ?? 0;
                ModLog.Info($"[TotemOfUndying] Diagnostics renderer[{r}] type={renderer.GetType().Name} enabled={renderer.enabled} matCount={matCount}");

                if (mats == null)
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
                    var baseMap = "<none>";
                    var mainTex = "<none>";
                    var color = "<n/a>";

                    try
                    {
                        if (mat.HasProperty("_BaseMap"))
                        {
                            var tex = mat.GetTexture("_BaseMap");
                            if (tex != null)
                            {
                                baseMap = tex.name;
                            }
                        }

                        if (mat.HasProperty("_MainTex"))
                        {
                            var tex = mat.GetTexture("_MainTex");
                            if (tex != null)
                            {
                                mainTex = tex.name;
                            }
                        }

                        if (mat.HasProperty("_Color"))
                        {
                            var c = mat.GetColor("_Color");
                            color = $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                        }
                        else if (mat.HasProperty("_BaseColor"))
                        {
                            var c = mat.GetColor("_BaseColor");
                            color = $"({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                        }
                    }
                    catch (Exception e)
                    {
                        ModLog.Warn($"[TotemOfUndying] Diagnostics read material[{i}] failed: {e.Message}");
                    }

                    ModLog.Info($"[TotemOfUndying] Diagnostics renderer[{r}] material[{i}] name='{mat.name}' shader='{shaderName}' _BaseMap='{baseMap}' _MainTex='{mainTex}' color={color}");
                }
            }
        }
        catch (Exception e)
        {
            ModLog.Warn($"[TotemOfUndying] Diagnostics agentType={agentType} failed: {e.GetType().Name}: {e.Message}");
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

        agents.RemoveAll(pair => pair != null && string.Equals(pair.key, key, StringComparison.Ordinal));

        agents.Add(new ItemAgentUtilities.AgentKeyPair
        {
            key = key,
            agentPrefab = prefab
        });

        ReflectionUtil.SetPrivateField(agentUtilities, "hashedAgentsCache", null);
    }
}
